// NPCSittingBehaviour.cs
// Handles the full sit-down / stand-up sub-FSM.
// Attach alongside NPCController. NPCController calls Init() then delegates
// EnterSitting() / TickSitting() / RequestStandUp() to this component.

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCSittingBehaviour : NPCBehaviourBase
{
    // =========================================================
    // Sub-FSM
    // =========================================================
    public enum SitPhase
    {
        None,
        SearchingSeat,
        ApproachingFront,
        Aligning,
        Backstepping,
        SitDownPlaying,
        SittingIdle,
        StandUpPlaying
    }

    // =========================================================
    // Inspector
    // =========================================================
    [Header("Sitting")]
    [Tooltip("Seat objects must be on this layer (e.g. 'SEAT').")]
    [SerializeField] private LayerMask seatLayerMask;
    [SerializeField] private bool seatDebugLogs = false;
    [SerializeField] private QueryTriggerInteraction seatQueryTriggers = QueryTriggerInteraction.Collide;
    [SerializeField] private float seatSearchTimeout  = 4.0f;
    [SerializeField] private float seatRescanInterval = 0.35f;
    [SerializeField] private float seatSearchRadius   = 0f;
    [SerializeField] private float preSitForwardOffset  = 0.45f;
    [SerializeField] private float preSitArriveDistance = 0.35f;
    [SerializeField] private float alignYawToleranceDeg = 6f;
    [SerializeField] private float backstepDistance = 0.25f;
    [SerializeField] private float backstepSpeed    = 0.8f;
    [SerializeField] private bool  snapToSeatWhenSeated = true;

    [Header("Sitting Placement")]
    [SerializeField] private Vector3 seatedRootOffset   = Vector3.zero;
    [SerializeField] private float   autoStandAfterSeconds = 0f;

    [Header("Sitting Animations")]
    [SerializeField] private string sitDownStateName = "SitDown";
    [SerializeField] private string sitIdleStateName = "SitIdle";
    [SerializeField] private int    sitAnimLayer  = 0;
    [SerializeField] private float  sitCrossfade  = 0.10f;
    [SerializeField] private bool   useSitTriggerParam = false;
    [SerializeField] private string sitTriggerParam    = "SitDown";
    [SerializeField] private float  sitTriggerFallbackDelay = 0.08f;

    [Header("Stand Up Animations")]
    [SerializeField] private bool   useStandUpTriggerParam = true;
    [SerializeField] private string standUpTriggerParam    = "StandUp";
    [SerializeField] private string standUpStateName       = "StandUp";

    // =========================================================
    // Runtime
    // =========================================================
    public SitPhase Phase => _sitPhase;
    private SitPhase _sitPhase = SitPhase.None;

    private Seat      _seat;
    private Transform _seatTf;
    private Vector3   _seatNavPos;
    private Vector3   _preSitNavPos;
    private Vector3   _preSitPoint;

    private float _seatSearchT;
    private float _seatRescanT;
    private float _seatedT;
    private float _sitTriggerT;
    private bool  _sitTriggerSent;

    // =========================================================
    // Init
    // =========================================================
    public override void Init(NPCController controller)
    {
        base.Init(controller);
        EnsureSeatLayerMask();
    }

    private void EnsureSeatLayerMask()
    {
        if (seatLayerMask.value != 0) return;
        seatLayerMask = LayerMask.GetMask("SEAT");
        if (seatLayerMask.value == 0)
            Debug.LogWarning($"{name}: Layer 'SEAT' not found.");
    }

    // =========================================================
    // Public API (called by NPCController)
    // =========================================================
    public void EnterSitting()
    {
        _sitPhase   = SitPhase.SearchingSeat;
        _seatSearchT = 0f;
        _seatRescanT = 0f;
        _seatedT     = 0f;

        ReleaseSeatIfAny();

        if (Agent != null)
        {
            if (!Agent.enabled) Agent.enabled = true;
            if (!Agent.isOnNavMesh && TryGetNavmeshPoint(Body.position, out Vector3 navPos))
                Agent.Warp(navPos);

            if (AgentReady())
            {
                Agent.isStopped = true;
                Agent.ResetPath();
            }
        }
    }

    public void Tick(float dt)
    {
        switch (_sitPhase)
        {
            case SitPhase.SearchingSeat:   TickSearchingSeat(dt);               break;
            case SitPhase.ApproachingFront:
                if (AgentReady()) TickApproachFront();
                break;
            case SitPhase.Aligning:
                if (!AgentReady()) return;
                if (_seatTf == null) { Fail(); return; }
                TickAlign(dt, GetSeatFacing());
                break;
            case SitPhase.Backstepping:
                if (!AgentReady()) return;
                if (_seatTf == null) { Fail(); return; }
                TickBackstep(dt, _seatNavPos, GetSeatFacing());
                break;
            case SitPhase.SitDownPlaying:  TickSitDownPlaying(dt);             break;
            case SitPhase.SittingIdle:
                if (_seatTf == null) { Fail(); return; }
                TickSittingIdle(dt, _seatTf.position, GetSeatFacing());
                break;
            case SitPhase.StandUpPlaying:  TickStandUpPlaying(dt);             break;
        }
    }

    /// <summary>Called externally (e.g. NPCController.RequestStandUp) to begin standing.</summary>
    public void BeginStandUp()
    {
        if (_sitPhase == SitPhase.StandUpPlaying) return;

        DetachAgentForAnimation();

        if (Anim == null)
        {
            FinishStandUpToPatrol();
            return;
        }

        ResetAllAnimatorTriggers();
        Anim.speed = 1f;

        if (useStandUpTriggerParam && !string.IsNullOrWhiteSpace(standUpTriggerParam))
            Anim.SetTrigger(standUpTriggerParam);
        else if (!string.IsNullOrWhiteSpace(standUpStateName))
            Anim.CrossFadeInFixedTime(standUpStateName, sitCrossfade, sitAnimLayer, 0f);

        _sitPhase = SitPhase.StandUpPlaying;
    }

    // =========================================================
    // Walk-up locomotion check (used by NPCController.UpdateLocomotionAndFacing)
    // =========================================================
    public bool IsApproachingFront => _sitPhase == SitPhase.ApproachingFront;
    public bool IsActive           => _sitPhase != SitPhase.None;

    // =========================================================
    // Seat facing helper
    // =========================================================
    private Vector3 GetSeatFacing()
    {
        if (_seatTf == null) return Body.forward;
        Vector3 f = _seatTf.forward; f.y = 0f;
        if (f.sqrMagnitude < 0.0001f) return Body.forward;
        return f.normalized;
    }

    // =========================================================
    // Sub-phase ticks
    // =========================================================
    private void TickSearchingSeat(float dt)
    {
        _seatSearchT += dt;
        _seatRescanT -= dt;

        if (_seatRescanT <= 0f)
        {
            _seatRescanT = Mathf.Max(0.05f, seatRescanInterval);
            if (TryAcquireSeat()) return;
        }

        if (AgentReady()) { Agent.isStopped = true; Agent.ResetPath(); }
        ForceIdlePose();
    }

    private bool TryAcquireSeat()
    {
        float radius = (seatSearchRadius > 0.01f) ? seatSearchRadius : Mathf.Max(0.1f, ActiveRadius);
        Vector3 center = SpawnPoint;

        Seat[] seats = FindObjectsByType<Seat>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (seats == null || seats.Length == 0)
        {
            if (seatDebugLogs) Debug.Log($"{name} Sit: No Seat components found.");
            return false;
        }

        Seat best = null;
        float bestSqr = float.PositiveInfinity;

        for (int i = 0; i < seats.Length; i++)
        {
            Seat s = seats[i];
            if (!s) continue;

            bool matchesMask =
                (((1 << s.gameObject.layer) & seatLayerMask.value) != 0) ||
                (s.seatTransform != null && (((1 << s.seatTransform.gameObject.layer) & seatLayerMask.value) != 0));

            if (!matchesMask)                     continue;
            if (!s.IsValid)                       continue;
            if (s.seatTransform == null)          continue;
            if (s.IsOccupied)                     continue;

            Vector3 seatPos = s.seatTransform.position;
            Vector3 d = seatPos - center; d.y = 0f;
            if (d.sqrMagnitude > radius * radius) continue;

            if (!NavMesh.SamplePosition(seatPos, out NavMeshHit _, ActiveRadius, NavMesh.AllAreas))
            {
                if (seatDebugLogs) Debug.Log($"{name} Sit: REJECT '{s.name}' navmesh_sample_failed");
                continue;
            }

            float sqr = (Body.position - seatPos).sqrMagnitude;
            if (sqr < bestSqr) { best = s; bestSqr = sqr; }
        }

        if (best == null) { Debug.LogWarning($"{name} Sit: No valid/free seat found."); return false; }
        if (!best.TryOccupy(npc))  { Debug.LogWarning($"{name} Sit: Seat '{best.name}' refused occupancy."); return false; }

        _seat  = best;
        _seatTf = best.seatTransform;

        Vector3 seatForward = _seatTf.forward; seatForward.y = 0f;
        if (seatForward.sqrMagnitude < 0.0001f) seatForward = Body.forward;
        seatForward.Normalize();

        Vector3 seatPos2 = _seatTf.position;
        _preSitPoint = seatPos2 + seatForward * Mathf.Max(0f, preSitForwardOffset);

        NavMesh.SamplePosition(seatPos2, out NavMeshHit seatHit, ActiveRadius, NavMesh.AllAreas);
        _seatNavPos = seatHit.position;

        if (!NavMesh.SamplePosition(_preSitPoint, out NavMeshHit preHit, ActiveRadius, NavMesh.AllAreas))
            _preSitNavPos = _seatNavPos;
        else
            _preSitNavPos = preHit.position;

        if (seatDebugLogs) Debug.Log($"{name} Sit: ACQUIRED '{_seat.name}'");

        if (!Agent.enabled) Agent.enabled = true;
        if (!Agent.isOnNavMesh && TryGetNavmeshPoint(Body.position, out Vector3 navPos))
            Agent.Warp(navPos);

        if (!AgentReady()) return false;

        Agent.isStopped = false;
        Agent.autoBraking = true;
        Agent.ResetPath();
        Agent.SetDestination(_preSitNavPos);

        _sitPhase = SitPhase.ApproachingFront;
        return true;
    }

    private void TickApproachFront()
    {
        Agent.isStopped = false;

        if (!Agent.pathPending && (!Agent.hasPath || Vector3.Distance(Agent.destination, _preSitNavPos) > 0.15f))
            Agent.SetDestination(_preSitNavPos);

        float dist = Vector3.Distance(
            new Vector3(Body.position.x, 0f, Body.position.z),
            new Vector3(_preSitNavPos.x, 0f, _preSitNavPos.z));

        float threshold = Mathf.Max(preSitArriveDistance, ArriveDistance);

        if (dist <= threshold)
        {
            if (seatDebugLogs) Debug.Log($"{name} Sit: Arrived at pre-sit point. Aligning.");
            Agent.isStopped = true;
            Agent.ResetPath();
            _sitPhase = SitPhase.Aligning;
        }
    }

    private void TickAlign(float dt, Vector3 seatFacing)
    {
        Agent.isStopped = true;

        Quaternion targetRot = Quaternion.LookRotation(seatFacing, Vector3.up);
        Body.rotation = Quaternion.Slerp(Body.rotation, targetRot, dt * (TurnSmoothing * 1.5f));

        float yawDelta = Mathf.Abs(Mathf.DeltaAngle(Body.eulerAngles.y, targetRot.eulerAngles.y));

        if (yawDelta <= Mathf.Max(0.5f, alignYawToleranceDeg))
        {
            if (seatDebugLogs) Debug.Log($"{name} Sit: Aligned. Backstepping.");
            _sitPhase = SitPhase.Backstepping;
        }
    }

    private void TickBackstep(float dt, Vector3 seatNavPos, Vector3 seatFacing)
    {
        Agent.isStopped = true;

        float step = Mathf.Max(0.01f, backstepSpeed) * dt;
        Body.position = Vector3.MoveTowards(Body.position, seatNavPos, step);

        Quaternion targetRot = Quaternion.LookRotation(seatFacing, Vector3.up);
        Body.rotation = Quaternion.Slerp(Body.rotation, targetRot, dt * (TurnSmoothing * 2.0f));

        float dist = Vector3.Distance(
            new Vector3(Body.position.x, 0f, Body.position.z),
            new Vector3(seatNavPos.x, 0f, seatNavPos.z));

        if (dist <= 0.12f)
        {
            if (seatDebugLogs) Debug.Log($"{name} Sit: Backstepped. Beginning SitDown.");
            BeginSitDown();
        }
    }

    private void BeginSitDown()
    {
        DetachAgentForAnimation();

        // Do NOT snap XZ to seat — backstep already placed the NPC correctly.
        if (_seatTf != null)
            Body.rotation = Quaternion.LookRotation(GetSeatFacing(), Vector3.up);

        if (Anim != null)
        {
            npc.AnimVelocitySmoothed = Vector3.zero;

            Anim.SetFloat(ParamMovingX, 0f);
            Anim.SetFloat(ParamMovingY, 0f);
            Anim.SetFloat(ParamBlend,   0f);

            ResetAllAnimatorTriggers();

            if (useSitTriggerParam && !string.IsNullOrWhiteSpace(sitTriggerParam))
                Anim.SetTrigger(sitTriggerParam);
            else
                Anim.CrossFadeInFixedTime(sitDownStateName, sitCrossfade, sitAnimLayer, 0f);

            if (seatDebugLogs) Debug.Log($"{name} Sit: Start SitDown.");
        }

        _sitTriggerSent = true;
        _sitTriggerT    = 0f;
        _sitPhase       = SitPhase.SitDownPlaying;
    }

    private void TickSitDownPlaying(float dt)
    {
        if (Anim == null) return;

        _sitTriggerT += dt;
        AnimatorStateInfo info = Anim.GetCurrentAnimatorStateInfo(sitAnimLayer);

        if (!string.IsNullOrWhiteSpace(sitIdleStateName) && info.IsName(sitIdleStateName))
        {
            _sitPhase = SitPhase.SittingIdle;
            _seatedT  = 0f;
            if (_seatTf != null)
                Body.rotation = Quaternion.LookRotation(GetSeatFacing(), Vector3.up);
            if (seatDebugLogs) Debug.Log($"{name} Sit: SittingIdle.");
            return;
        }

        if (_sitTriggerSent && _sitTriggerT >= sitTriggerFallbackDelay)
        {
            bool inSitDown = !string.IsNullOrWhiteSpace(sitDownStateName) && info.IsName(sitDownStateName);
            bool inSitIdle = !string.IsNullOrWhiteSpace(sitIdleStateName) && info.IsName(sitIdleStateName);

            if (!inSitDown && !inSitIdle && !string.IsNullOrWhiteSpace(sitDownStateName))
            {
                if (seatDebugLogs) Debug.Log($"{name} Sit: Fallback Play('{sitDownStateName}').");
                Anim.Play(sitDownStateName, sitAnimLayer, 0f);
            }
            _sitTriggerSent = false;
        }
    }

    private void TickSittingIdle(float dt, Vector3 seatPos, Vector3 seatForward)
    {
        ForceIdlePose();
        Body.rotation = Quaternion.LookRotation(seatForward, Vector3.up);

        if (autoStandAfterSeconds > 0f)
        {
            _seatedT += dt;
            if (_seatedT >= autoStandAfterSeconds)
                BeginStandUp();
        }
    }

    private void TickStandUpPlaying(float dt)
    {
        if (Anim == null) { FinishStandUpToPatrol(); return; }

        AnimatorStateInfo info = Anim.GetCurrentAnimatorStateInfo(sitAnimLayer);
        bool inStandUp = !string.IsNullOrWhiteSpace(standUpStateName) && info.IsName(standUpStateName);

        if (inStandUp && !Anim.IsInTransition(sitAnimLayer) && info.normalizedTime >= 1f)
            FinishStandUpToPatrol();
    }

    // =========================================================
    // Finish / Fail
    // =========================================================
    private void FinishStandUpToPatrol()
    {
        if (Anim != null)
        {
            Anim.speed = 1f;
            ForceReturnToLocomotion();
            Anim.Update(0f);
        }

        ReleaseSeatIfAny();

        npc.HasCommand  = false;
        npc.CommandGoal = NPCController.NPCState.Patrolling;
        _sitPhase       = SitPhase.None;

        StartCoroutine(FinishStandUpGroundingRoutine());
    }

    private IEnumerator FinishStandUpGroundingRoutine()
    {
        ReattachAgentToNavmeshAtCurrentXZ();
        yield return null;
        ReattachAgentToNavmeshAtCurrentXZ();

        if (Anim != null)
        {
            Anim.Update(0f);
            ForceIdlePose();
        }

        npc.HasCommand  = false;
        npc.CommandGoal = NPCController.NPCState.Patrolling;
        _sitPhase       = SitPhase.None;
        npc.SetStateDirectly(NPCController.NPCState.Patrolling);

        npc.ExecutePendingPostStandAction();
    }

    private void Fail()
    {
        ReleaseSeatIfAny();
        npc.HasCommand  = false;
        npc.CommandGoal = NPCController.NPCState.Patrolling;
        _sitPhase       = SitPhase.None;

        if (Anim != null) ForceReturnToLocomotion();
        ReattachAgentToNavmeshAtCurrentXZ();
        EnterState(NPCController.NPCState.Patrolling);
    }

    private void ReleaseSeatIfAny()
    {
        if (_seat != null) _seat.Release(npc);
        _seat   = null;
        _seatTf = null;
    }
}
