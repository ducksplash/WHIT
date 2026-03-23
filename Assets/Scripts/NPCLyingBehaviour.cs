using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCLyingBehaviour : NPCBehaviourBase
{
    public enum LiePhase { None, SearchingBed, RoutingToLadder, ClimbingLadder, ApproachingFront, Aligning, LieDownPlaying, LyingIdle, WakeUpPlaying }

    [Header("Lying / Bed")]
    [SerializeField] private LayerMask bedLayerMask;
    [SerializeField] private bool  bedDebugLogs          = false;
    [SerializeField] private float bedRescanInterval     = 0.35f;
    [SerializeField] private float bedSearchRadius       = 0f;
    [SerializeField] private float bedSearchTimeout      = 4.0f;
    [SerializeField] private float preLieArriveDistance  = 0.45f;
    [SerializeField] private float bedAlignYawToleranceDeg = 6f;
    [SerializeField] private bool  snapToBedWhenLying    = true;

    [Header("Lying Placement")]
    [SerializeField] private float autoWakeAfterSeconds      = 0f;
    [SerializeField] private float bodyRecoverySampleRadius  = 4f;

    [Header("Lying Animations")]
    [SerializeField] private string lieDownStateName        = "LieDown";
    [SerializeField] private string lieIdleStateName        = "LieIdle";
    [SerializeField] private int    lieAnimLayer            = 0;
    [SerializeField] private float  lieCrossfade            = 0.10f;
    [SerializeField] private bool   useLieDownTriggerParam  = true;
    [SerializeField] private string lieDownTriggerParam     = "LieDown";
    [SerializeField] private float  lieTriggerFallbackDelay = 0.08f;
    [SerializeField] private bool   useLieIdleTriggerParam  = true;
    [SerializeField] private string lieIdleTriggerParam     = "LieIdle";

    [Header("Lying Lerp")]
    [SerializeField] private float lieDownLerpDuration = 0.5f;
    [SerializeField] private float wakeUpLerpDuration  = 0.4f;

    [Header("Wake Up Animations")]
    [SerializeField] private bool   useWakeUpTriggerParam     = true;
    [SerializeField] private string wakeUpTriggerParam        = "WakeUp";
    [SerializeField] private string wakeUpStateName           = "WakeUp";
    [SerializeField] private float  wakeTriggerFallbackDelay  = 0.08f;

    public LiePhase Phase          => _liePhase;
    public bool IsApproachingFront => _liePhase == LiePhase.ApproachingFront;
    public bool IsRoutingToLadder  => _liePhase == LiePhase.RoutingToLadder;

    private Coroutine _lieLerpCoroutine;
    private Coroutine _wakeLerpCoroutine;
    private LiePhase  _liePhase = LiePhase.None;

    private Bed       _bed;
    private Transform _bedTf, _bedFootTf, _bedLieTf;
    private Vector3   _bedNavPos, _preLieNavPos, _bedLieWorldPos;
    private bool      _snappedToBedLyingPose;

    private float _bedSearchT, _bedRescanT, _lyingT, _lieTriggerT, _wakeTriggerT;
    private bool  _lieTriggerSent, _wakeTriggerSent;

    private Ladder  _routeLadder;
    private bool    _routeGoingUp;
    private Vector3 _routeApproachPoint, _routeExitPoint;

    private Transform _animationRoot;
    private bool      _hasAnimationRootCachedXZ;
    private Vector2   _animationRootCachedLocalXZ;

    public override void Init(NPCController controller)
    {
        base.Init(controller);
        EnsureBedLayerMask();
        _animationRoot = Anim != null ? Anim.transform : null;
        _hasAnimationRootCachedXZ = false;
    }

    private void EnsureBedLayerMask()
    {
        if (bedLayerMask.value != 0) return;
        bedLayerMask = LayerMask.GetMask("BED");
        if (bedLayerMask.value == 0) Debug.LogWarning($"{name}: Layer 'BED' not found.");
    }

    public void EnterLying()
    {
        _liePhase = LiePhase.SearchingBed;
        _bedSearchT = _bedRescanT = _lyingT = _lieTriggerT = _wakeTriggerT = 0f;
        _lieTriggerSent = _wakeTriggerSent = _snappedToBedLyingPose = false;
        ClearLadderRoute();
        ReleaseBedIfAny();
        if (Agent == null) return;
        if (!Agent.enabled) Agent.enabled = true;
        if (!Agent.isOnNavMesh && TryGetNavmeshPoint(Body.position, out Vector3 navPos)) Agent.Warp(navPos);
        Agent.autoBraking = true;
        Agent.stoppingDistance = Mathf.Max(0.05f, ArriveDistance);
        if (AgentReady()) { Agent.isStopped = true; Agent.ResetPath(); }
    }

    public void Tick(float dt)
    {
        switch (_liePhase)
        {
            case LiePhase.SearchingBed:     TickSearchingBed(dt);                                                         break;
            case LiePhase.RoutingToLadder:  TickRoutingToLadder();                                                        break;
            case LiePhase.ClimbingLadder:   ForceIdlePose();                                                              break;
            case LiePhase.ApproachingFront: if (AgentReady()) TickApproachBedFront();                                     break;
            case LiePhase.Aligning:         if (_bedTf == null) { Fail(); return; } if (AgentReady()) TickAlignToBed(dt, GetBedFacing()); break;
            case LiePhase.LieDownPlaying:   TickLieDownPlaying(dt);                                                       break;
            case LiePhase.LyingIdle:        if (_bedTf == null) { Fail(); return; } TickLyingIdle(dt, GetBedFacing());    break;
            case LiePhase.WakeUpPlaying:    TickWakeUpPlaying(dt);                                                        break;
        }
    }

    public void BeginWakeUp()
    {
        if (_liePhase == LiePhase.WakeUpPlaying) return;

        // Cache anything you still need before releasing the bed
        Vector3 wakeTarget =
            _preLieNavPos != Vector3.zero
                ? _preLieNavPos
                : Body.position;

        Vector3 liePos =
            _bedLieTf != null
                ? _bedLieTf.position
                : _bedLieWorldPos;

        Vector3 bedFacing = GetBedFacing();

        Body.rotation = GetPlanarLookRotation(bedFacing);
        if (snapToBedWhenLying)
            Body.position = liePos;

        // RELEASE BED HERE, at wake start
        ReleaseBedIfAny();

        DetachAgentForAnimation();

        if (Anim == null)
        {
            FinishWakeUpToPatrol();
            return;
        }

        Anim.SetFloat(ParamMovingX, 0f);
        Anim.SetFloat(ParamMovingY, 0f);
        Anim.SetFloat(ParamBlend, 0f);

        ResetAllAnimatorTriggers();
        Anim.speed = 1f;

        if (useWakeUpTriggerParam && !string.IsNullOrWhiteSpace(wakeUpTriggerParam))
            Anim.SetTrigger(wakeUpTriggerParam);
        else if (!string.IsNullOrWhiteSpace(wakeUpStateName))
            Anim.CrossFadeInFixedTime(wakeUpStateName, lieCrossfade, lieAnimLayer, 0f);

        _wakeTriggerT = 0f;
        _wakeTriggerSent = true;
        _liePhase = LiePhase.WakeUpPlaying;

        if (_wakeLerpCoroutine != null)
            StopCoroutine(_wakeLerpCoroutine);

        _wakeLerpCoroutine = StartCoroutine(LerpBodyXZOnly(wakeTarget, wakeUpLerpDuration));
    }

    private Vector3 GetBedFacing()
    {
        if (_bedFootTf != null && _bedLieTf != null)
        {
            Vector3 away = _bedFootTf.position - _bedLieTf.position; away.y = 0f;
            if (away.sqrMagnitude > 0.0001f) return away.normalized;
        }
        if (_bedFootTf != null)
        {
            Vector3 f = _bedFootTf.forward; f.y = 0f;
            if (f.sqrMagnitude > 0.0001f) return f.normalized;
        }
        return Body.forward;
    }

    private void TickSearchingBed(float dt)
    {
        _bedRescanT -= dt;
        if (_bedRescanT <= 0f) { _bedRescanT = Mathf.Max(0.05f, bedRescanInterval); if (TryAcquireBed()) return; }
    }

    private bool TryAcquireBed()
    {
        float radius = bedSearchRadius > 0.01f ? bedSearchRadius : Mathf.Max(0.1f, ActiveRadius);
        Bed[] beds = FindObjectsByType<Bed>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (beds == null || beds.Length == 0) { if (bedDebugLogs) Debug.Log($"{name} Bed: No Bed components found."); return false; }

        Bed   best    = null;
        float bestSqr = float.PositiveInfinity;

        foreach (var b in beds)
        {
            if (!b || b.bedFootTransform == null || b.bedLyingTransform == null || b.IsOccupied) continue;
            bool matchesMask = ((1 << b.gameObject.layer) & bedLayerMask.value) != 0
                || ((1 << b.bedFootTransform.gameObject.layer) & bedLayerMask.value) != 0
                || ((1 << b.bedLyingTransform.gameObject.layer) & bedLayerMask.value) != 0;
            if (!matchesMask) continue;
            Vector3 bedPos = b.bedFootTransform.position;
            Vector3 d = bedPos - SpawnPoint; d.y = 0f;
            if (d.sqrMagnitude > radius * radius) continue;
            if (!NavMesh.SamplePosition(bedPos, out _, ActiveRadius, NavMesh.AllAreas))
                { if (bedDebugLogs) Debug.Log($"{name} Bed: REJECT '{b.name}' navmesh failed"); continue; }
            float sqr = (Body.position - bedPos).sqrMagnitude;
            if (sqr < bestSqr) { best = b; bestSqr = sqr; }
        }

        if (best == null) { if (bedDebugLogs) Debug.LogWarning($"{name} Bed: No valid/free bed found."); return false; }
        if (!best.TryOccupy(npc)) { Debug.LogWarning($"{name} Bed: '{best.name}' refused occupancy."); return false; }

        _bed = best; _bedTf = best.transform; _bedFootTf = best.bedFootTransform; _bedLieTf = best.bedLyingTransform;

        if (!NavMesh.SamplePosition(_bedLieTf.position, out NavMeshHit bedLieHit, ActiveRadius, NavMesh.AllAreas))
            { Debug.LogWarning($"{name} Bed: Could not sample lying NavMesh point."); ReleaseBedIfAny(); return false; }
        _bedNavPos = bedLieHit.position; _bedLieWorldPos = _bedLieTf.position;

        if (!NavMesh.SamplePosition(_bedFootTf.position, out NavMeshHit footHit, ActiveRadius, NavMesh.AllAreas))
            { Debug.LogWarning($"{name} Bed: Could not sample foot NavMesh point."); ReleaseBedIfAny(); return false; }
        _preLieNavPos = footHit.position;

        if (!Agent.enabled) Agent.enabled = true;
        if (!Agent.isOnNavMesh && TryGetNavmeshPoint(Body.position, out Vector3 navPos)) Agent.Warp(navPos);
        if (!AgentReady()) { if (bedDebugLogs) Debug.LogWarning($"{name} Bed: agent not ready."); ReleaseBedIfAny(); return false; }

        void SetupAgent(NavMeshPath path) { Agent.isStopped = false; Agent.autoBraking = true; Agent.stoppingDistance = Mathf.Max(0.05f, ArriveDistance); Agent.ResetPath(); Agent.SetPath(path); }

        if (npc.CanReachPosition(_preLieNavPos, out NavMeshPath directPath))
            { if (bedDebugLogs) Debug.Log($"{name} Bed: direct path ok"); SetupAgent(directPath); _liePhase = LiePhase.ApproachingFront; return true; }

        if (npc.TryFindLadderRoute(_preLieNavPos, out Ladder ladder, out bool goingUp, out Vector3 approachPoint, out Vector3 exitPoint, out NavMeshPath ladderPath))
        {
            _routeLadder = ladder; _routeGoingUp = goingUp; _routeApproachPoint = approachPoint; _routeExitPoint = exitPoint;
            if (bedDebugLogs) Debug.Log($"{name} Bed: routing via ladder '{ladder.name}' goingUp={goingUp}");
            SetupAgent(ladderPath); _liePhase = LiePhase.RoutingToLadder; return true;
        }

        if (bedDebugLogs) Debug.LogWarning($"{name} Bed: REJECT '{_bed?.name}' no path.");
        ReleaseBedIfAny(); ClearLadderRoute(); return false;
    }

    private void TickRoutingToLadder()
    {
        if (_routeLadder == null) { if (bedDebugLogs) Debug.LogWarning($"{name} Bed: RoutingToLadder but no ladder."); Fail(); return; }
        Agent.isStopped = false; Agent.autoBraking = true;
        Agent.stoppingDistance = Mathf.Max(0.05f, ArriveDistance);
        if (!Agent.pathPending && Agent.pathStatus == NavMeshPathStatus.PathInvalid)
            { if (bedDebugLogs) Debug.LogWarning($"{name} Bed: ladder path invalid."); Fail(); return; }
        if (!Agent.pathPending && !Agent.hasPath)
            { if (bedDebugLogs) Debug.LogWarning($"{name} Bed: lost ladder path."); Fail(); return; }
        float dist = Vector3.Distance(new Vector3(Body.position.x, 0f, Body.position.z),
                                      new Vector3(_routeApproachPoint.x, 0f, _routeApproachPoint.z));
        if (dist <= npc.ladderApproachArriveDistance)
        {
            if (bedDebugLogs) Debug.Log($"{name} Bed: reached ladder approach.");
            Agent.isStopped = true; Agent.ResetPath();
            _liePhase = LiePhase.ClimbingLadder;
            npc.StartLadderTraversal(_routeLadder, _routeGoingUp, OnFinishedLadderRoute);
        }
    }

    private void OnFinishedLadderRoute()
    {
        if (bedDebugLogs) Debug.Log($"{name} Bed: ladder traversal complete");
        if (_bed == null || _bedTf == null) { Fail(); return; }
        if (!AgentReady()) { if (bedDebugLogs) Debug.LogWarning($"{name} Bed: agent not ready after ladder."); Fail(); return; }
        if (!npc.CanReachPosition(_preLieNavPos, out NavMeshPath path))
            { if (bedDebugLogs) Debug.LogWarning($"{name} Bed: cannot reach bed after ladder."); Fail(); return; }
        Agent.isStopped = false; Agent.autoBraking = true;
        Agent.stoppingDistance = Mathf.Max(0.05f, ArriveDistance);
        Agent.ResetPath(); Agent.SetPath(path);
        _liePhase = LiePhase.ApproachingFront;
    }

    private void TickApproachBedFront()
    {
        Agent.isStopped = false; Agent.autoBraking = true;
        Agent.stoppingDistance = Mathf.Max(0.05f, ArriveDistance);
        if (!Agent.pathPending && Agent.pathStatus == NavMeshPathStatus.PathInvalid)
            { if (bedDebugLogs) Debug.LogWarning($"{name} Bed: path invalid approaching bed."); Fail(); return; }
        if (!Agent.pathPending && !Agent.hasPath)
            { if (bedDebugLogs) Debug.LogWarning($"{name} Bed: lost path approaching bed."); Fail(); return; }
        if (!Agent.pathPending && (!Agent.hasPath || Vector3.Distance(Agent.destination, _preLieNavPos) > 0.15f))
            Agent.SetDestination(_preLieNavPos);

        float planarDist = new Vector2(Body.position.x - _preLieNavPos.x, Body.position.z - _preLieNavPos.z).magnitude;
        float threshold  = Mathf.Max(preLieArriveDistance, Agent.stoppingDistance + 0.05f);
        bool  navArrived = !Agent.pathPending && Agent.hasPath && !float.IsInfinity(Agent.remainingDistance)
            && Agent.remainingDistance <= Mathf.Max(Agent.stoppingDistance, 0.05f) + 0.02f;

        if (planarDist <= threshold || navArrived)
        {
            if (bedDebugLogs) Debug.Log($"{name} Bed: Arrived at foot point. Aligning.");
            Agent.isStopped = true; Agent.ResetPath(); _liePhase = LiePhase.Aligning;
        }
    }

    private void TickAlignToBed(float dt, Vector3 bedFacing)
    {
        Agent.isStopped = true;
        Quaternion targetRot = Quaternion.LookRotation(bedFacing, Vector3.up);
        Body.rotation = Quaternion.Slerp(Body.rotation, targetRot, dt * (TurnSmoothing * 1.5f));
        float yawDelta = Mathf.Abs(Mathf.DeltaAngle(Body.eulerAngles.y, targetRot.eulerAngles.y));
        if (bedDebugLogs) Debug.Log($"{name} Bed: Aligning yawDelta={yawDelta:F1}");
        if (yawDelta <= Mathf.Max(0.5f, bedAlignYawToleranceDeg)) { if (bedDebugLogs) Debug.Log($"{name} Bed: Aligned."); BeginLieDown(); }
    }

    private void BeginLieDown()
    {
        CacheAnimationRootLocalXZ();
        DetachAgentForAnimation();
        if (_bedFootTf != null) Body.position = _bedFootTf.position;
        Body.rotation = Quaternion.LookRotation(GetBedFacing(), Vector3.up);
        if (Anim != null)
        {
            Anim.SetFloat(ParamMovingX, 0f); Anim.SetFloat(ParamMovingY, 0f); Anim.SetFloat(ParamBlend, 0f);
            ResetAllAnimatorTriggers();
            if (useLieDownTriggerParam && !string.IsNullOrWhiteSpace(lieDownTriggerParam))
                Anim.SetTrigger(lieDownTriggerParam);
            else
                Anim.CrossFadeInFixedTime(lieDownStateName, lieCrossfade, lieAnimLayer, 0f);
            if (bedDebugLogs) Debug.Log($"{name} Bed: LieDown trigger fired.");
        }
        _lieTriggerT = 0f; _lieTriggerSent = true; _liePhase = LiePhase.LieDownPlaying;
        if (_lieLerpCoroutine != null) StopCoroutine(_lieLerpCoroutine);
        Vector3 lieTargetWorld = _bedLieTf != null ? _bedLieTf.position : _bedLieWorldPos;
        float   authorativeY   = _bedFootTf != null ? _bedFootTf.position.y : Body.position.y;
        _lieLerpCoroutine = StartCoroutine(LerpBodyXZOnly(new Vector3(lieTargetWorld.x, authorativeY, lieTargetWorld.z), lieDownLerpDuration));
    }

    private IEnumerator LerpBodyXZOnly(Vector3 target, float duration)
    {
        float startX = Body.position.x, startZ = Body.position.z;
        float endX = target.x, endZ = target.z;
        float elapsed = 0f;
        duration = Mathf.Max(0.01f, duration);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector3 p = Body.position; p.x = Mathf.Lerp(startX, endX, t); p.z = Mathf.Lerp(startZ, endZ, t);
            Body.position = p;
            yield return null;
        }
        Vector3 final = Body.position; final.x = endX; final.z = endZ; Body.position = final;
    }

    public void ForceCancelLying()
    {
        StopAllCoroutines();
        _lieLerpCoroutine = _wakeLerpCoroutine = null;
        _bedRescanT = _lyingT = _lieTriggerT = _wakeTriggerT = 0f;
        _lieTriggerSent = _wakeTriggerSent = false;
        ReleaseBedIfAny(); ClearLadderRoute(); RestoreAnimationRootLocalXZ();
        _liePhase = LiePhase.None;
        if (Anim != null) { Anim.speed = 1f; ResetAllAnimatorTriggers(); ForceReturnToLocomotion(); Anim.Update(0f); ForceIdlePose(); }
        if (Agent != null)
        {
            if (!Agent.enabled) Agent.enabled = true;
            Vector3 preferred = _preLieNavPos != Vector3.zero ? _preLieNavPos : (_bedNavPos != Vector3.zero ? _bedNavPos : Body.position);
            RestoreStandingBodyAt(preferred, bodyRecoverySampleRadius);
            if (!Agent.isOnNavMesh && TryGetNavmeshPoint(Body.position, out Vector3 navPos)) Agent.Warp(navPos);
            if (AgentReady()) { Agent.isStopped = false; Agent.ResetPath(); Agent.velocity = Vector3.zero; Agent.autoBraking = true; Agent.stoppingDistance = Mathf.Max(0.05f, ArriveDistance); }
        }
        npc.HasCommand = false; npc.CommandGoal = NPCState.Patrolling;
    }

    private void TickLieDownPlaying(float dt)
    {
        if (Anim == null) return;
        _lieTriggerT += dt;
        AnimatorStateInfo info = Anim.GetCurrentAnimatorStateInfo(lieAnimLayer);
        bool inLieDown = !string.IsNullOrWhiteSpace(lieDownStateName) && info.IsName(lieDownStateName);
        bool inLieIdle = !string.IsNullOrWhiteSpace(lieIdleStateName) && info.IsName(lieIdleStateName);

        if (_lieTriggerSent && _lieTriggerT >= lieTriggerFallbackDelay)
        {
            if (!inLieDown && !inLieIdle && !string.IsNullOrWhiteSpace(lieDownStateName))
                { if (bedDebugLogs) Debug.Log($"{name} Bed: Fallback Play('{lieDownStateName}')."); Anim.Play(lieDownStateName, lieAnimLayer, 0f); }
            _lieTriggerSent = false;
        }

        bool lieDownFinished = inLieIdle || (inLieDown && !Anim.IsInTransition(lieAnimLayer) && info.normalizedTime >= 0.95f);
        bool timedOut        = _lieTriggerT >= Mathf.Max(TriggerMaxDuration * 0.9f, 5f);

        if (lieDownFinished || timedOut)
        {
            if (!_snappedToBedLyingPose) SnapToBedLyingPoseNow();
            if (!inLieIdle)
            {
                ResetAllAnimatorTriggers();
                if (useLieIdleTriggerParam && !string.IsNullOrWhiteSpace(lieIdleTriggerParam))
                    Anim.SetTrigger(lieIdleTriggerParam);
                else if (!string.IsNullOrWhiteSpace(lieIdleStateName))
                    Anim.CrossFadeInFixedTime(lieIdleStateName, lieCrossfade, lieAnimLayer, 0f);
                if (bedDebugLogs) Debug.Log($"{name} Bed: LieDown done. (timedOut={timedOut})");
            }
            _liePhase = LiePhase.LyingIdle; _lyingT = 0f;
            if (bedDebugLogs) Debug.Log($"{name} Bed: LyingIdle.");
        }
    }

    private void TickLyingIdle(float dt, Vector3 bedFacing)
    {
        ForceIdlePose();
        Body.rotation = GetPlanarLookRotation(bedFacing);
        if (snapToBedWhenLying) Body.position = _bedLieTf != null ? _bedLieTf.position : _bedLieWorldPos;
        if (autoWakeAfterSeconds > 0f) { _lyingT += dt; if (_lyingT >= autoWakeAfterSeconds) BeginWakeUp(); }
    }

    private void TickWakeUpPlaying(float dt)
    {
        if (Anim == null) { FinishWakeUpToPatrol(); return; }
        _wakeTriggerT += dt;
        AnimatorStateInfo info = Anim.GetCurrentAnimatorStateInfo(lieAnimLayer);
        bool inWakeUp  = !string.IsNullOrWhiteSpace(wakeUpStateName)  && info.IsName(wakeUpStateName);
        bool inLieIdle = !string.IsNullOrWhiteSpace(lieIdleStateName) && info.IsName(lieIdleStateName);

        if (inWakeUp && !Anim.IsInTransition(lieAnimLayer) && info.normalizedTime >= 1f) { FinishWakeUpToPatrol(); return; }
        if (_wakeTriggerSent && _wakeTriggerT >= wakeTriggerFallbackDelay)
        {
            if (!inWakeUp && !string.IsNullOrWhiteSpace(wakeUpStateName))
                { if (bedDebugLogs) Debug.Log($"{name} Bed: Fallback Play('{wakeUpStateName}')."); Anim.Play(wakeUpStateName, lieAnimLayer, 0f); }
            _wakeTriggerSent = false;
        }
        if (_wakeTriggerT >= Mathf.Max(0.25f, TriggerMaxDuration)) { if (bedDebugLogs) Debug.LogWarning($"{name} Bed: WakeUp timeout."); FinishWakeUpToPatrol(); return; }
        if (_wakeTriggerT > 0.20f && !Anim.IsInTransition(lieAnimLayer) && !inWakeUp && !inLieIdle)
            { if (bedDebugLogs) Debug.Log($"{name} Bed: WakeUp exited unexpectedly."); FinishWakeUpToPatrol(); }
    }

    private void FinishWakeUpToPatrol()
    {
        Vector3 preferred = _preLieNavPos != Vector3.zero ? _preLieNavPos : (_bedNavPos != Vector3.zero ? _bedNavPos : Body.position);
        ClearLadderRoute();
        _liePhase = LiePhase.None;
        StartCoroutine(FinishWakeUpGroundingRoutine(preferred));
    }

    private IEnumerator FinishWakeUpGroundingRoutine(Vector3 preferred)
    {
        RestoreStandingBodyAt(preferred, bodyRecoverySampleRadius); yield return null;
        RestoreStandingBodyAt(preferred, bodyRecoverySampleRadius); yield return null;
        RestoreStandingBodyAt(Body.position, bodyRecoverySampleRadius);
        RestoreAnimationRootLocalXZ();
        if (Anim != null) { Anim.speed = 1f; ForceReturnToLocomotion(); Anim.Update(0f); ForceIdlePose(); }
        npc.HasCommand = false; npc.CommandGoal = NPCState.Patrolling;
        _liePhase = LiePhase.None;
        npc.SetStateDirectly(NPCState.Patrolling);
        npc.ExecutePendingPostStandAction();
    }

    private void Fail()
    {
        Vector3 preferred = _preLieNavPos != Vector3.zero ? _preLieNavPos : (_bedNavPos != Vector3.zero ? _bedNavPos : Body.position);
        ReleaseBedIfAny(); ClearLadderRoute();
        npc.HasCommand = false; npc.CommandGoal = NPCState.Patrolling;
        _liePhase = LiePhase.None;
        RestoreStandingBodyAt(preferred, bodyRecoverySampleRadius);
        RestoreAnimationRootLocalXZ();
        if (Anim != null) ForceReturnToLocomotion();
        ReattachAgentToNavmeshAtCurrentXZ();
        EnterState(NPCState.Patrolling);
    }

    private void ReleaseBedIfAny()
    {
        _bed?.Release(npc);
        _bed = null; _bedTf = null; _bedFootTf = null; _bedLieTf = null;
        _bedNavPos = _preLieNavPos = _bedLieWorldPos = Vector3.zero;
        _snappedToBedLyingPose = false;
    }

    private void ClearLadderRoute()
    {
        _routeLadder = null; _routeGoingUp = false;
        _routeApproachPoint = _routeExitPoint = Vector3.zero;
    }

    private void SnapToBedLyingPoseNow()
    {
        Body.rotation = Quaternion.LookRotation(GetBedFacing(), Vector3.up);
        if (snapToBedWhenLying) Body.position = _bedLieTf != null ? _bedLieTf.position : _bedLieWorldPos;
        _snappedToBedLyingPose = true;
        if (bedDebugLogs) Debug.Log($"{name} Bed: Snapped to lying pose.");
    }

    private void CacheAnimationRootLocalXZ()
    {
        if (_animationRoot == null) return;
        Vector3 lp = _animationRoot.localPosition;
        _animationRootCachedLocalXZ = new Vector2(lp.x, lp.z);
        _hasAnimationRootCachedXZ = true;
    }

    private void RestoreAnimationRootLocalXZ()
    {
        if (_animationRoot == null || !_hasAnimationRootCachedXZ) return;
        _animationRoot.localPosition = new Vector3(_animationRootCachedLocalXZ.x, 0f, _animationRootCachedLocalXZ.y);
    }
}