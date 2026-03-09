using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCSittingBehaviour : NPCBehaviourBase
{
    public enum SitPhase
    {
        None,
        SearchingSeat,
        RoutingToLadder,
        ClimbingLadder,
        ApproachingFront,
        Aligning,
        Backstepping,
        SitDownPlaying,
        SittingIdle,
        StandUpPlaying
    }

    [Header("Sitting")]
    [Tooltip("Seat objects must be on this layer (e.g. 'SEAT').")]
    [SerializeField] private LayerMask seatLayerMask;
    private bool seatDebugLogs = false;
    [SerializeField] private QueryTriggerInteraction seatQueryTriggers = QueryTriggerInteraction.Collide;
    [SerializeField] private float seatSearchTimeout = 4.0f;
    [SerializeField] private float seatRescanInterval = 0.35f;
    [SerializeField] private float seatSearchRadius = 0f;
    [SerializeField] private float preSitForwardOffset = 0.45f;
    [SerializeField] private float preSitArriveDistance = 0.35f;
    [SerializeField] private float alignYawToleranceDeg = 6f;
    [SerializeField] private float backstepDistance = 0.25f;
    [SerializeField] private float backstepSpeed = 0.8f;
    [SerializeField] private bool snapToSeatWhenSeated = true;

    [Header("Sitting Placement")]
    [SerializeField] private Vector3 seatedRootOffset = Vector3.zero;
    [SerializeField] private float autoStandAfterSeconds = 0f;

    [Header("Sitting Lerp")]
    [SerializeField] private float sitDownLerpDuration  = 0.4f;
    [SerializeField] private float standUpLerpDuration  = 0.35f;

    [Header("Sitting Animations")]
    [SerializeField] private string sitDownStateName = "SitDown";
    [SerializeField] private string sitIdleStateName = "SitIdle";
    [SerializeField] private int sitAnimLayer = 0;
    [SerializeField] private float sitCrossfade = 0.10f;
    [SerializeField] private bool useSitTriggerParam = false;
    [SerializeField] private string sitTriggerParam = "SitDown";
    [SerializeField] private float sitTriggerFallbackDelay = 0.08f;

    [Header("Stand Up Animations")]
    [SerializeField] private bool useStandUpTriggerParam = true;
    [SerializeField] private string standUpTriggerParam = "StandUp";
    [SerializeField] private string standUpStateName = "StandUp";

    private Coroutine _sitLerpCoroutine;
    private Coroutine _standLerpCoroutine;

    private float _standUpT;
    private bool  _standUpTriggerSent;
    
    public SitPhase Phase => _sitPhase;
    public bool IsApproachingFront => _sitPhase == SitPhase.ApproachingFront;
    public bool IsRoutingToLadder => _sitPhase == SitPhase.RoutingToLadder;

    private SitPhase _sitPhase = SitPhase.None;

    private Seat _seat;
    private Transform _seatTf;
    private Vector3 _seatNavPos;
    private Vector3 _preSitNavPos;
    private Vector3 _preSitPoint;

    private float _seatSearchT;
    private float _seatRescanT;
    private float _seatedT;
    private float _sitTriggerT;
    private bool _sitTriggerSent;

    private Ladder _routeLadder;
    private bool _routeGoingUp;
    private Vector3 _routeApproachPoint;
    private Vector3 _routeExitPoint;

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

    public void EnterSitting()
    {
        if (seatDebugLogs) Debug.Log($"{name} Sit: EnterSitting()");

        _sitPhase = SitPhase.SearchingSeat;
        _seatSearchT = 0f;
        _seatRescanT = 0f;
        _seatedT = 0f;

        ClearLadderRoute();
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
        if (seatDebugLogs) Debug.Log($"{name} Sit: Tick phase={_sitPhase}");

        switch (_sitPhase)
        {
            case SitPhase.SearchingSeat:   TickSearchingSeat(dt); break;
            case SitPhase.RoutingToLadder: TickRoutingToLadder(); break;
            case SitPhase.ClimbingLadder:
                ForceIdlePose();
                break;
            case SitPhase.ApproachingFront:
                if (AgentReady()) TickApproachFront();
                else if (seatDebugLogs) Debug.LogWarning($"{name} Sit: ApproachingFront but agent not ready.");
                break;
            case SitPhase.Aligning:
                if (!AgentReady()) { if (seatDebugLogs) Debug.LogWarning($"{name} Sit: Aligning but agent not ready."); return; }
                if (_seatTf == null) { Fail(); return; }
                TickAlign(dt, GetSeatFacing());
                break;
            case SitPhase.Backstepping:
                if (!AgentReady()) { if (seatDebugLogs) Debug.LogWarning($"{name} Sit: Backstepping but agent not ready."); return; }
                if (_seatTf == null) { Fail(); return; }
                TickBackstep(dt, _seatNavPos, GetSeatFacing());
                break;
            case SitPhase.SitDownPlaying:
                TickSitDownPlaying(dt);
                break;
            case SitPhase.SittingIdle:
                if (_seatTf == null) { Fail(); return; }
                TickSittingIdle(dt, _seatTf.position, GetSeatFacing());
                break;
            case SitPhase.StandUpPlaying:
                TickStandUpPlaying(dt);
                break;
        }
    }

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

        _standUpT = 0f;
        _standUpTriggerSent = true;
        _sitPhase = SitPhase.StandUpPlaying;

        // Lerp body back to the pre-sit point simultaneously with the stand-up animation
        if (_standLerpCoroutine != null) StopCoroutine(_standLerpCoroutine);
        Vector3 standTarget = _preSitPoint != Vector3.zero ? _preSitPoint : Body.position;
        _standLerpCoroutine = StartCoroutine(LerpBodyTo(standTarget, standUpLerpDuration));

        if (seatDebugLogs)
            Debug.Log($"{name} Sit: BeginStandUp()");
    }

    private Vector3 GetSeatFacing()
    {
        if (_seatTf == null) return Body.forward;
        Vector3 f = _seatTf.forward; f.y = 0f;
        if (f.sqrMagnitude < 0.0001f) return Body.forward;
        return f.normalized;
    }

    private void TickSearchingSeat(float dt)
    {
        _seatSearchT += dt;
        _seatRescanT -= dt;

        if (_seatRescanT <= 0f)
        {
            _seatRescanT = Mathf.Max(0.05f, seatRescanInterval);
            if (TryAcquireSeat())
                return;
        }

        if (_seatSearchT >= Mathf.Max(0.1f, seatSearchTimeout))
        {
            if (seatDebugLogs)
                Debug.LogWarning($"{name} Sit: search timed out after {_seatSearchT:F2}s");
            Fail();
            return;
        }

        if (AgentReady())
        {
            Agent.isStopped = true;
            Agent.ResetPath();
        }

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

            if (seatDebugLogs) Debug.Log($"{name} Sit: checking seat '{s.name}'");

            bool matchesMask =
                (((1 << s.gameObject.layer) & seatLayerMask.value) != 0) ||
                (s.seatTransform != null && (((1 << s.seatTransform.gameObject.layer) & seatLayerMask.value) != 0));

            if (!matchesMask)
            {
                if (seatDebugLogs) Debug.Log($"{name} Sit: REJECT '{s.name}' layer mismatch");
                continue;
            }

            if (!s.IsValid)
            {
                if (seatDebugLogs) Debug.Log($"{name} Sit: REJECT '{s.name}' IsValid=false");
                continue;
            }

            if (s.seatTransform == null)
            {
                if (seatDebugLogs) Debug.Log($"{name} Sit: REJECT '{s.name}' seatTransform=null");
                continue;
            }

            if (s.IsOccupied)
            {
                if (seatDebugLogs) Debug.Log($"{name} Sit: REJECT '{s.name}' occupied");
                continue;
            }

            Vector3 seatPos = s.seatTransform.position;
            Vector3 d = seatPos - center; d.y = 0f;
            if (d.sqrMagnitude > radius * radius)
            {
                if (seatDebugLogs) Debug.Log($"{name} Sit: REJECT '{s.name}' out of radius ({Mathf.Sqrt(d.sqrMagnitude):F2} > {radius:F2})");
                continue;
            }

            if (!NavMesh.SamplePosition(seatPos, out NavMeshHit _, ActiveRadius, NavMesh.AllAreas))
            {
                if (seatDebugLogs) Debug.Log($"{name} Sit: REJECT '{s.name}' navmesh_sample_failed");
                continue;
            }

            float sqr = (Body.position - seatPos).sqrMagnitude;
            if (sqr < bestSqr)
            {
                best = s;
                bestSqr = sqr;
            }
        }

        if (best == null)
        {
            if (seatDebugLogs) Debug.LogWarning($"{name} Sit: No valid/free seat found.");
            return false;
        }

        if (!best.TryOccupy(npc))
        {
            Debug.LogWarning($"{name} Sit: Seat '{best.name}' refused occupancy.");
            return false;
        }

        _seat = best;
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

        if (!Agent.enabled) Agent.enabled = true;
        if (!Agent.isOnNavMesh && TryGetNavmeshPoint(Body.position, out Vector3 navPos))
            Agent.Warp(navPos);

        if (!AgentReady())
        {
            ReleaseSeatIfAny();
            return false;
        }

        if (npc.CanReachPosition(_preSitNavPos, out NavMeshPath directPath))
        {
            if (seatDebugLogs)
                Debug.Log($"{name} Sit: direct path to seat ok");

            Agent.isStopped = false;
            Agent.autoBraking = true;
            Agent.ResetPath();
            Agent.SetPath(directPath);
            _sitPhase = SitPhase.ApproachingFront;
            return true;
        }

        if (npc.TryFindLadderRoute(_preSitNavPos, out Ladder ladder, out bool goingUp, out Vector3 approachPoint, out Vector3 exitPoint, out NavMeshPath ladderPath))
        {
            _routeLadder = ladder;
            _routeGoingUp = goingUp;
            _routeApproachPoint = approachPoint;
            _routeExitPoint = exitPoint;

            if (seatDebugLogs) Debug.Log($"{name} Sit: routing via ladder '{ladder.name}' goingUp={goingUp}");

            Agent.isStopped = false;
            Agent.autoBraking = true;
            Agent.ResetPath();
            Agent.SetPath(ladderPath);

            _sitPhase = SitPhase.RoutingToLadder;
            return true;
        }

        if (seatDebugLogs)
            Debug.LogWarning($"{name} Sit: REJECT '{_seat.name}' no direct path and no ladder route");

        ReleaseSeatIfAny();
        ClearLadderRoute();
        return false;
    }

    private void TickRoutingToLadder()
    {
        if (_routeLadder == null)
        {
            if (seatDebugLogs) Debug.LogWarning($"{name} Sit: RoutingToLadder but no ladder.");
            Fail();
            return;
        }

        if (seatDebugLogs)
        {
            Debug.Log(
                $"{name} Sit: RoutingToLadder " +
                $"hasPath={Agent.hasPath} pending={Agent.pathPending} " +
                $"status={Agent.pathStatus} remaining={Agent.remainingDistance:F2}");
        }

        Agent.isStopped = false;

        if (!Agent.pathPending && Agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            if (seatDebugLogs) Debug.LogWarning($"{name} Sit: ladder approach path became invalid.");
            Fail();
            return;
        }

        if (!Agent.pathPending && !Agent.hasPath)
        {
            if (seatDebugLogs) Debug.LogWarning($"{name} Sit: lost ladder approach path.");
            Fail();
            return;
        }

        float dist = Vector3.Distance(
            new Vector3(Body.position.x, 0f, Body.position.z),
            new Vector3(_routeApproachPoint.x, 0f, _routeApproachPoint.z));

        if (dist <= npc.ladderApproachArriveDistance)
        {
            if (seatDebugLogs) Debug.Log($"{name} Sit: reached ladder approach, starting climb.");

            Agent.isStopped = true;
            Agent.ResetPath();
            _sitPhase = SitPhase.ClimbingLadder;

            npc.StartLadderTraversal(_routeLadder, _routeGoingUp, OnFinishedLadderRoute);
        }
    }

    private void OnFinishedLadderRoute()
    {
        if (seatDebugLogs) Debug.Log($"{name} Sit: ladder traversal complete");

        if (_seat == null || _seatTf == null)
        {
            Fail();
            return;
        }

        if (!AgentReady())
        {
            if (seatDebugLogs) Debug.LogWarning($"{name} Sit: agent not ready after ladder.");
            Fail();
            return;
        }

        if (!npc.CanReachPosition(_preSitNavPos, out NavMeshPath path))
        {
            if (seatDebugLogs) Debug.LogWarning($"{name} Sit: cannot reach seat after ladder.");
            Fail();
            return;
        }

        Agent.isStopped = false;
        Agent.autoBraking = true;
        Agent.ResetPath();
        Agent.SetPath(path);

        _sitPhase = SitPhase.ApproachingFront;
    }

    private void TickApproachFront()
    {
        if (seatDebugLogs)
        {
            Debug.Log(
                $"{name} Sit: ApproachingFront " +
                $"hasPath={Agent.hasPath} " +
                $"pending={Agent.pathPending} " +
                $"status={Agent.pathStatus} " +
                $"remaining={Agent.remainingDistance:F2} " +
                $"dest={Agent.destination} " +
                $"pos={Body.position}"
            );
        }

        Agent.isStopped = false;

        if (!Agent.pathPending && Agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            if (seatDebugLogs) Debug.LogWarning($"{name} Sit: path became invalid while approaching.");
            Fail();
            return;
        }

        if (!Agent.pathPending && !Agent.hasPath)
        {
            if (seatDebugLogs) Debug.LogWarning($"{name} Sit: lost path while approaching.");
            Fail();
            return;
        }

        if (!Agent.pathPending && Vector3.Distance(Agent.destination, _preSitNavPos) > 0.15f)
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

        float dist = Vector3.Distance(new Vector3(Body.position.x, 0f, Body.position.z), new Vector3(seatNavPos.x, 0f, seatNavPos.z));

        if (dist <= 0.12f)
        {
            if (seatDebugLogs) Debug.Log($"{name} Sit: Backstepped. Beginning SitDown.");
            BeginSitDown();
        }
    }

    private void BeginSitDown()
    {
        DetachAgentForAnimation();

        if (_seatTf != null)
            Body.rotation = Quaternion.LookRotation(GetSeatFacing(), Vector3.up);

        if (Anim != null)
        {
            npc.AnimVelocitySmoothed = Vector3.zero;

            Anim.SetFloat(ParamMovingX, 0f);
            Anim.SetFloat(ParamMovingY, 0f);
            Anim.SetFloat(ParamBlend, 0f);

            ResetAllAnimatorTriggers();

            if (useSitTriggerParam && !string.IsNullOrWhiteSpace(sitTriggerParam))
                Anim.SetTrigger(sitTriggerParam);
            else
                Anim.CrossFadeInFixedTime(sitDownStateName, sitCrossfade, sitAnimLayer, 0f);

            if (seatDebugLogs) Debug.Log($"{name} Sit: Start SitDown.");
        }

        _sitTriggerSent = true;
        _sitTriggerT = 0f;
        _sitPhase = SitPhase.SitDownPlaying;

        // Lerp body to seat centre simultaneously with the sit-down animation
        if (_sitLerpCoroutine != null) StopCoroutine(_sitLerpCoroutine);
        Vector3 seatedTarget = _seatTf != null
            ? _seatTf.position + seatedRootOffset
            : Body.position;
        _sitLerpCoroutine = StartCoroutine(LerpBodyTo(seatedTarget, sitDownLerpDuration));
    }

    private void TickSitDownPlaying(float dt)
    {
        if (Anim == null) return;

        _sitTriggerT += dt;
        AnimatorStateInfo info = Anim.GetCurrentAnimatorStateInfo(sitAnimLayer);

        if (!string.IsNullOrWhiteSpace(sitIdleStateName) && info.IsName(sitIdleStateName))
        {
            _sitPhase = SitPhase.SittingIdle;
            _seatedT = 0f;
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
    if (Anim == null)
    {
        FinishStandUpToPatrol();
        return;
    }

    _standUpT += dt;

    AnimatorStateInfo info = Anim.GetCurrentAnimatorStateInfo(sitAnimLayer);
    bool inStandUp = !string.IsNullOrWhiteSpace(standUpStateName) && info.IsName(standUpStateName);
    bool inSitIdle = !string.IsNullOrWhiteSpace(sitIdleStateName) && info.IsName(sitIdleStateName);

    if (seatDebugLogs)
    {
        Debug.Log(
            $"{name} Sit: StandUpPlaying " +
            $"inStandUp={inStandUp} " +
            $"inSitIdle={inSitIdle} " +
            $"transition={Anim.IsInTransition(sitAnimLayer)} " +
            $"normalized={info.normalizedTime:F2} " +
            $"stateHash={info.fullPathHash}"
        );
    }

    // Fallback: if trigger didn't get us into the state, force-play it shortly after.
    if (_standUpTriggerSent && _standUpT >= sitTriggerFallbackDelay)
    {
        if (!inStandUp && !string.IsNullOrWhiteSpace(standUpStateName))
        {
            if (seatDebugLogs)
                Debug.Log($"{name} Sit: Fallback Play('{standUpStateName}')");
            Anim.Play(standUpStateName, sitAnimLayer, 0f);
        }
        _standUpTriggerSent = false;
    }

    // Normal completion.
    if (inStandUp && !Anim.IsInTransition(sitAnimLayer) && info.normalizedTime >= 1f)
    {
        if (seatDebugLogs)
            Debug.Log($"{name} Sit: StandUp complete by normalizedTime.");
        FinishStandUpToPatrol();
        return;
    }

    // If we've already left the stand-up state after some time, assume we're done.
    if (_standUpT > 0.20f && !Anim.IsInTransition(sitAnimLayer) && !inStandUp && !inSitIdle)
    {
        if (seatDebugLogs)
            Debug.LogWarning($"{name} Sit: StandUp exited unexpectedly, forcing finish.");
        FinishStandUpToPatrol();
        return;
    }

    // Hard timeout safety.
    if (_standUpT >= Mathf.Max(0.5f, TriggerMaxDuration))
    {
        if (seatDebugLogs)
            Debug.LogWarning($"{name} Sit: StandUp timeout, forcing finish.");
        FinishStandUpToPatrol();
    }
}

    private IEnumerator LerpBodyTo(Vector3 target, float duration)
    {
        Vector3 start = Body.position;
        // Only lerp XZ — the NPC's root stays grounded at its own Y.
        target = new Vector3(target.x, start.y, target.z);
        float elapsed = 0f;
        duration = Mathf.Max(0.01f, duration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Body.position = Vector3.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        Body.position = target;
    }

    private void FinishStandUpToPatrol()
    {
        if (Anim != null)
        {
            Anim.speed = 1f;
            ForceReturnToLocomotion();
            Anim.Update(0f);
        }

        ReleaseSeatIfAny();
        ClearLadderRoute();

        npc.HasCommand = false;
        npc.CommandGoal = NPCController.NPCState.Patrolling;
        _sitPhase = SitPhase.None;

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

        npc.HasCommand = false;
        npc.CommandGoal = NPCController.NPCState.Patrolling;
        _sitPhase = SitPhase.None;
        npc.SetStateDirectly(NPCController.NPCState.Patrolling);

        npc.ExecutePendingPostStandAction();
    }
    public void ForceCancelSitting()
    {
        StopAllCoroutines();

        _sitLerpCoroutine   = null;
        _standLerpCoroutine = null;
        _standUpT = 0f;
        _standUpTriggerSent = false;

        _seatSearchT = 0f;
        _seatRescanT = 0f;
        _seatedT = 0f;
        _sitTriggerT = 0f;
        _sitTriggerSent = false;

        ReleaseSeatIfAny();
        ClearLadderRoute();

        _seatNavPos = Vector3.zero;
        _preSitNavPos = Vector3.zero;
        _preSitPoint = Vector3.zero;

        _sitPhase = SitPhase.None;

        if (Anim != null)
        {
            Anim.speed = 1f;
            ResetAllAnimatorTriggers();
            ForceReturnToLocomotion();
            Anim.Update(0f);
            ForceIdlePose();
        }

        if (Agent != null)
        {
            if (!Agent.enabled) Agent.enabled = true;

            if (!Agent.isOnNavMesh && TryGetNavmeshPoint(Body.position, out Vector3 navPos))
                Agent.Warp(navPos);

            if (AgentReady())
            {
                Agent.isStopped = false;
                Agent.ResetPath();
                Agent.velocity = Vector3.zero;
                Agent.autoBraking = true;
                Agent.stoppingDistance = Mathf.Max(0.05f, ArriveDistance);
            }
        }

        npc.HasCommand = false;
        npc.CommandGoal = NPCController.NPCState.Patrolling;
    }
    private void Fail()
    {
        if (seatDebugLogs) Debug.LogWarning($"{name} Sit: FAIL");

        ReleaseSeatIfAny();
        ClearLadderRoute();

        npc.HasCommand = false;
        npc.CommandGoal = NPCController.NPCState.Patrolling;
        _sitPhase = SitPhase.None;

        if (Anim != null)
            ForceReturnToLocomotion();

        ReattachAgentToNavmeshAtCurrentXZ();
        npc.SetStateDirectly(NPCController.NPCState.Patrolling);
        EnterState(NPCController.NPCState.Patrolling);
    }

    private void ReleaseSeatIfAny()
    {
        if (_seat != null) _seat.Release(npc);
        _seat = null;
        _seatTf = null;
    }

    private void ClearLadderRoute()
    {
        _routeLadder = null;
        _routeGoingUp = false;
        _routeApproachPoint = Vector3.zero;
        _routeExitPoint = Vector3.zero;
    }
}