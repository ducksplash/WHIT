using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCSittingBehaviour : NPCBehaviourBase
{
    public enum SitPhase { None, SearchingSeat, RoutingToLadder, ClimbingLadder, ApproachingFront, Aligning, Backstepping, SitDownPlaying, SittingIdle, StandUpPlaying }

    [Header("Sitting")]
    [Tooltip("Seat objects must be on this layer (e.g. 'SEAT').")]
    [SerializeField] private LayerMask seatLayerMask;
    [SerializeField] private bool  seatDebugLogs          = false;
    [SerializeField] private QueryTriggerInteraction seatQueryTriggers = QueryTriggerInteraction.Collide;
    [SerializeField] private float seatSearchTimeout      = 4.0f;
    [SerializeField] private float seatRescanInterval     = 0.35f;
    [SerializeField] private float seatSearchRadius       = 0f;
    [SerializeField] private float preSitForwardOffset    = 0.45f;
    [SerializeField] private float preSitArriveDistance   = 0.35f;
    [SerializeField] private float alignYawToleranceDeg   = 6f;
    [SerializeField] private float backstepDistance       = 0.25f;
    [SerializeField] private float backstepSpeed          = 0.8f;
    [SerializeField] private bool  snapToSeatWhenSeated   = true;

    [Header("Sitting Placement")]
    [SerializeField] private Vector3 seatedRootOffset     = Vector3.zero;
    [SerializeField] private float   autoStandAfterSeconds = 0f;

    [Header("Sitting Lerp")]
    [SerializeField] private float sitDownLerpDuration  = 0.4f;
    [SerializeField] private float standUpLerpDuration  = 0.15f;

    [Header("Sitting Animations")]
    [SerializeField] private string sitDownStateName      = "SitDown";
    [SerializeField] private string sitIdleStateName      = "SitIdle";
    [SerializeField] private int    sitAnimLayer          = 0;
    [SerializeField] private float  sitCrossfade          = 0.10f;
    [SerializeField] private bool   useSitTriggerParam    = false;
    [SerializeField] private string sitTriggerParam       = "SitDown";
    [SerializeField] private float  sitTriggerFallbackDelay = 0.08f;
    [SerializeField] private bool   useSitIdleTriggerParam  = true;
    [SerializeField] private string sitIdleTriggerParam     = "SitIdle";
    [SerializeField] private bool   useSitIdleFloorTriggerParam = true;
    [SerializeField] private string sitIdleFloorTriggerParam    = "SitIdleFloor";
    [SerializeField] private string sitIdleFloorStateName       = "SitIdleFloor";

    [SerializeField] private bool   useTypingTriggerParam = true;
    [SerializeField] private string typingTriggerParam    = "Typing";
    [SerializeField] private string typingStateName       = "Typing";
    [SerializeField] private bool   useTalkingTriggerParam = true;
    [SerializeField] private string talkingTriggerParam    = "IdleTalking";
    [SerializeField] private string talkingStateName       = "IdleTalking";

    [SerializeField] private bool   useGabbingTriggerParam = true;
    [SerializeField] private string gabbingTriggerParam    = "IdleGabbing";
    [SerializeField] private string gabbingStateName       = "IdleGabbing";

    [SerializeField] private bool   useDawdlingTriggerParam = true;
    [SerializeField] private string dawdlingTriggerParam    = "IdleDawdling";
    [SerializeField] private string dawdlingStateName       = "IdleDawdling";
    
    
    [Header("Stand Up Animations")]
    [SerializeField] private bool   useStandUpTriggerParam = true;
    [SerializeField] private string standUpTriggerParam    = "StandUp";
    [SerializeField] private string standUpStateName       = "StandUp";

    [Header("Floor Sitting")]
    [Tooltip("If seat height above the sampled navmesh/ground is <= this, use floor-sit idle.")]
    [SerializeField] private float floorSitHeightThreshold = 0.2f;

    [Header("Desk Behaviour")]
    public bool isTyping;
    public bool isTalking;
    public bool isGabbing;
    public bool isDawdling;
    
    private Coroutine _sitLerpCoroutine;
    private Coroutine _standLerpCoroutine;
    private float _standUpT;
    private bool  _standUpTriggerSent;

    public SitPhase Phase          => _sitPhase;
    public bool IsApproachingFront => _sitPhase == SitPhase.ApproachingFront;
    public bool IsRoutingToLadder  => _sitPhase == SitPhase.RoutingToLadder;

    private SitPhase  _sitPhase = SitPhase.None;
    private Seat      _seat;
    private Transform _seatTf;
    private Vector3   _seatNavPos, _preSitNavPos, _preSitPoint, _backstepTarget;
    private float     _seatSearchT, _seatRescanT, _seatedT, _sitTriggerT;
    private bool      _sitTriggerSent;

    private Ladder  _routeLadder;
    private bool    _routeGoingUp;
    private Vector3 _routeApproachPoint, _routeExitPoint;

    public override void Init(NPCController controller)
    {
        base.Init(controller);
        if (seatLayerMask.value != 0) return;
        seatLayerMask = LayerMask.GetMask("SEAT");
        if (seatLayerMask.value == 0) Debug.LogWarning($"{name}: Layer 'SEAT' not found.");
    }

    public void EnterSitting()
    {
        if (seatDebugLogs) Debug.Log($"{name} Sit: EnterSitting()");
        _sitPhase = SitPhase.SearchingSeat;
        _seatSearchT = _seatRescanT = _seatedT = 0f;
        ClearLadderRoute(); ReleaseSeatIfAny();
        if (Agent == null) return;
        if (!Agent.enabled) Agent.enabled = true;
        if (!Agent.isOnNavMesh && TryGetNavmeshPoint(Body.position, out Vector3 navPos)) Agent.Warp(navPos);
        if (AgentReady()) { Agent.isStopped = true; Agent.ResetPath(); }
    }

    public void Tick(float dt)
    {
        if (seatDebugLogs) Debug.Log($"{name} Sit: Tick phase={_sitPhase}");
        switch (_sitPhase)
        {
            case SitPhase.SearchingSeat:   TickSearchingSeat(dt); break;
            case SitPhase.RoutingToLadder: TickRoutingToLadder(); break;
            case SitPhase.ClimbingLadder:  ForceIdlePose(); break;
            case SitPhase.ApproachingFront:
                if (AgentReady()) TickApproachFront();
                else if (seatDebugLogs) Debug.LogWarning($"{name} Sit: ApproachingFront agent not ready.");
                break;
            case SitPhase.Aligning:
                if (!AgentReady()) { if (seatDebugLogs) Debug.LogWarning($"{name} Sit: Aligning agent not ready."); return; }
                if (_seatTf == null) { Fail(); return; }
                TickAlign(dt, GetSeatFacing()); break;
            case SitPhase.Backstepping:
                if (!AgentReady()) { if (seatDebugLogs) Debug.LogWarning($"{name} Sit: Backstepping agent not ready."); return; }
                if (_seatTf == null) { Fail(); return; }
                TickBackstep(dt, GetSeatFacing()); break;
            case SitPhase.SitDownPlaying:  TickSitDownPlaying(dt); break;
            case SitPhase.SittingIdle:
                if (_seatTf == null) { Fail(); return; }
                TickSittingIdle(dt, GetSeatFacing()); break;
            case SitPhase.StandUpPlaying:  TickStandUpPlaying(dt); break;
        }
    }

    public void BeginStandUp()
    {
        if (_sitPhase == SitPhase.StandUpPlaying) return;
        DetachAgentForAnimation();
        if (Anim == null) { FinishStandUpToPatrol(); return; }
        ResetAllAnimatorTriggers();
        Anim.speed = 1f;
        if (useStandUpTriggerParam && !string.IsNullOrWhiteSpace(standUpTriggerParam))
            Anim.SetTrigger(standUpTriggerParam);
        else if (!string.IsNullOrWhiteSpace(standUpStateName))
            Anim.CrossFadeInFixedTime(standUpStateName, sitCrossfade, sitAnimLayer, 0f);
        _standUpT = 0f; _standUpTriggerSent = true; _sitPhase = SitPhase.StandUpPlaying;
        if (_standLerpCoroutine != null) StopCoroutine(_standLerpCoroutine);
        Vector3 standTarget = _preSitPoint != Vector3.zero ? _preSitPoint : Body.position;
        _standLerpCoroutine = StartCoroutine(LerpBodyTo(standTarget, standUpLerpDuration));
        if (seatDebugLogs) Debug.Log($"{name} Sit: BeginStandUp()");
    }

    private Vector3 GetSeatFacing()
    {
        if (_seatTf == null) return Body.forward;
        Vector3 f = _seatTf.forward; f.y = 0f;
        return f.sqrMagnitude < 0.0001f ? Body.forward : f.normalized;
    }

    private bool IsFloorSeat()
    {
        if (_seatTf == null) return false;
        float h = _seatTf.position.y - _seatNavPos.y;
        bool isFloor = h <= floorSitHeightThreshold;
        if (seatDebugLogs) Debug.Log($"{name} Sit: seatHeight={h:F3} isFloor={isFloor}");
        return isFloor;
    }

    private void TriggerCorrectSitIdle()
    {
        bool useFloor = IsFloorSeat();
        ResetAllAnimatorTriggers();

        if (useFloor)
        {
            if (useSitIdleFloorTriggerParam && !string.IsNullOrWhiteSpace(sitIdleFloorTriggerParam))
                Anim.SetTrigger(sitIdleFloorTriggerParam);
            else if (!string.IsNullOrWhiteSpace(sitIdleFloorStateName))
                Anim.CrossFadeInFixedTime(sitIdleFloorStateName, sitCrossfade, sitAnimLayer, 0f);

            if (seatDebugLogs) Debug.Log($"{name} Sit: Triggered floor idle.");
            return;
        }

        // Priority order: Typing > Talking > Gabbing > Dawdling > normal SitIdle
        if (isTyping)
        {
            if (useTypingTriggerParam && !string.IsNullOrWhiteSpace(typingTriggerParam))
                Anim.SetTrigger(typingTriggerParam);
            else if (!string.IsNullOrWhiteSpace(typingStateName))
                Anim.CrossFadeInFixedTime(typingStateName, sitCrossfade, sitAnimLayer, 0f);

            if (seatDebugLogs) Debug.Log($"{name} Sit: Triggered typing idle.");
        }
        else if (isTalking)
        {
            if (useTalkingTriggerParam && !string.IsNullOrWhiteSpace(talkingTriggerParam))
                Anim.SetTrigger(talkingTriggerParam);
            else if (!string.IsNullOrWhiteSpace(talkingStateName))
                Anim.CrossFadeInFixedTime(talkingStateName, sitCrossfade, sitAnimLayer, 0f);

            if (seatDebugLogs) Debug.Log($"{name} Sit: Triggered talking idle.");
        }
        else if (isGabbing)
        {
            if (useGabbingTriggerParam && !string.IsNullOrWhiteSpace(gabbingTriggerParam))
                Anim.SetTrigger(gabbingTriggerParam);
            else if (!string.IsNullOrWhiteSpace(gabbingStateName))
                Anim.CrossFadeInFixedTime(gabbingStateName, sitCrossfade, sitAnimLayer, 0f);

            if (seatDebugLogs) Debug.Log($"{name} Sit: Triggered gabbing idle.");
        }
        else if (isDawdling)
        {
            if (useDawdlingTriggerParam && !string.IsNullOrWhiteSpace(dawdlingTriggerParam))
                Anim.SetTrigger(dawdlingTriggerParam);
            else if (!string.IsNullOrWhiteSpace(dawdlingStateName))
                Anim.CrossFadeInFixedTime(dawdlingStateName, sitCrossfade, sitAnimLayer, 0f);

            if (seatDebugLogs) Debug.Log($"{name} Sit: Triggered dawdling idle.");
        }
        else
        {
            if (useSitIdleTriggerParam && !string.IsNullOrWhiteSpace(sitIdleTriggerParam))
                Anim.SetTrigger(sitIdleTriggerParam);
            else if (!string.IsNullOrWhiteSpace(sitIdleStateName))
                Anim.CrossFadeInFixedTime(sitIdleStateName, sitCrossfade, sitAnimLayer, 0f);

            if (seatDebugLogs) Debug.Log($"{name} Sit: Triggered normal sit idle.");
        }
    }


    private void TickSearchingSeat(float dt)
    {
        _seatSearchT += dt; _seatRescanT -= dt;
        if (_seatRescanT <= 0f) { _seatRescanT = Mathf.Max(0.05f, seatRescanInterval); if (TryAcquireSeat()) return; }
        if (_seatSearchT >= Mathf.Max(0.1f, seatSearchTimeout))
            { if (seatDebugLogs) Debug.LogWarning($"{name} Sit: search timed out after {_seatSearchT:F2}s"); Fail(); return; }
        if (AgentReady()) { Agent.isStopped = true; Agent.ResetPath(); }
        ForceIdlePose();
    }

    private bool TryAcquireSeat()
    {
        float   radius = seatSearchRadius > 0.01f ? seatSearchRadius : Mathf.Max(0.1f, ActiveRadius);
        Vector3 center = SpawnPoint;
        Seat[]  seats  = FindObjectsByType<Seat>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (seats == null || seats.Length == 0) { if (seatDebugLogs) Debug.Log($"{name} Sit: No Seat components found."); return false; }

        Seat  best    = null;
        float bestSqr = float.PositiveInfinity;

        for (int i = 0; i < seats.Length; i++)
        {
            Seat s = seats[i];
            if (!s) continue;
            if (seatDebugLogs) Debug.Log($"{name} Sit: checking seat '{s.name}'");

            bool matchesMask = ((1 << s.gameObject.layer) & seatLayerMask.value) != 0
                || (s.seatTransform != null && ((1 << s.seatTransform.gameObject.layer) & seatLayerMask.value) != 0);

            if (!matchesMask)            { if (seatDebugLogs) Debug.Log($"{name} Sit: REJECT '{s.name}' layer");           continue; }
            if (!s.IsValid)              { if (seatDebugLogs) Debug.Log($"{name} Sit: REJECT '{s.name}' IsValid=false");   continue; }
            if (s.seatTransform == null) { if (seatDebugLogs) Debug.Log($"{name} Sit: REJECT '{s.name}' seatTf=null");     continue; }
            if (s.IsOccupied)            { if (seatDebugLogs) Debug.Log($"{name} Sit: REJECT '{s.name}' occupied");        continue; }

            Vector3 seatPos = s.seatTransform.position;
            Vector3 d = seatPos - center; d.y = 0f;
            if (d.sqrMagnitude > radius * radius)
                { if (seatDebugLogs) Debug.Log($"{name} Sit: REJECT '{s.name}' out of radius"); continue; }
            if (!NavMesh.SamplePosition(seatPos, out NavMeshHit _, ActiveRadius, NavMesh.AllAreas))
                { if (seatDebugLogs) Debug.Log($"{name} Sit: REJECT '{s.name}' navmesh failed"); continue; }

            float sqr = (Body.position - seatPos).sqrMagnitude;
            if (sqr < bestSqr) { best = s; bestSqr = sqr; }
        }

        if (best == null) { if (seatDebugLogs) Debug.LogWarning($"{name} Sit: No valid/free seat found."); return false; }
        if (!best.TryOccupy(npc)) { Debug.LogWarning($"{name} Sit: Seat '{best.name}' refused occupancy."); return false; }

        _seat = best; _seatTf = best.seatTransform;
        Vector3 seatForward = _seatTf.forward; seatForward.y = 0f;
        if (seatForward.sqrMagnitude < 0.0001f) seatForward = Body.forward;
        seatForward.Normalize();

        Vector3 seatPos2 = _seatTf.position;
        _preSitPoint = seatPos2 + seatForward * Mathf.Max(0f, preSitForwardOffset);

        NavMesh.SamplePosition(seatPos2, out NavMeshHit seatHit, ActiveRadius, NavMesh.AllAreas);
        _seatNavPos = seatHit.position;
        _preSitNavPos = NavMesh.SamplePosition(_preSitPoint, out NavMeshHit preHit, ActiveRadius, NavMesh.AllAreas) ? preHit.position : _seatNavPos;

        Vector3 bsWorld = seatPos2 + seatForward * Mathf.Max(0f, backstepDistance);
        _backstepTarget = NavMesh.SamplePosition(bsWorld, out NavMeshHit bsHit, ActiveRadius, NavMesh.AllAreas) ? bsHit.position : _preSitNavPos;

        if (!Agent.enabled) Agent.enabled = true;
        if (!Agent.isOnNavMesh && TryGetNavmeshPoint(Body.position, out Vector3 navPos)) Agent.Warp(navPos);
        if (!AgentReady()) { ReleaseSeatIfAny(); return false; }

        if (npc.CanReachPosition(_preSitNavPos, out NavMeshPath directPath))
        {
            if (seatDebugLogs) Debug.Log($"{name} Sit: direct path to seat ok");
            Agent.isStopped = false; Agent.autoBraking = true; Agent.ResetPath(); Agent.SetPath(directPath);
            _sitPhase = SitPhase.ApproachingFront; return true;
        }

        if (npc.TryFindLadderRoute(_preSitNavPos, out Ladder ladder, out bool goingUp, out Vector3 approachPoint, out Vector3 exitPoint, out NavMeshPath ladderPath))
        {
            _routeLadder = ladder; _routeGoingUp = goingUp; _routeApproachPoint = approachPoint; _routeExitPoint = exitPoint;
            if (seatDebugLogs) Debug.Log($"{name} Sit: routing via ladder '{ladder.name}' goingUp={goingUp}");
            Agent.isStopped = false; Agent.autoBraking = true; Agent.ResetPath(); Agent.SetPath(ladderPath);
            _sitPhase = SitPhase.RoutingToLadder; return true;
        }

        if (seatDebugLogs) Debug.LogWarning($"{name} Sit: REJECT '{_seat.name}' no path.");
        ReleaseSeatIfAny(); ClearLadderRoute(); return false;
    }

    private void TickRoutingToLadder()
    {
        if (_routeLadder == null) { if (seatDebugLogs) Debug.LogWarning($"{name} Sit: RoutingToLadder no ladder."); Fail(); return; }
        if (seatDebugLogs) Debug.Log($"{name} Sit: RoutingToLadder hasPath={Agent.hasPath} remaining={Agent.remainingDistance:F2}");
        Agent.isStopped = false;
        if (!Agent.pathPending && Agent.pathStatus == NavMeshPathStatus.PathInvalid)
            { if (seatDebugLogs) Debug.LogWarning($"{name} Sit: ladder path invalid."); Fail(); return; }
        if (!Agent.pathPending && !Agent.hasPath)
            { if (seatDebugLogs) Debug.LogWarning($"{name} Sit: lost ladder path."); Fail(); return; }
        float dist = Vector3.Distance(new Vector3(Body.position.x, 0f, Body.position.z),
                                      new Vector3(_routeApproachPoint.x, 0f, _routeApproachPoint.z));
        if (dist <= npc.ladderApproachArriveDistance)
        {
            if (seatDebugLogs) Debug.Log($"{name} Sit: reached ladder approach.");
            Agent.isStopped = true; Agent.ResetPath();
            _sitPhase = SitPhase.ClimbingLadder;
            npc.StartLadderTraversal(_routeLadder, _routeGoingUp, OnFinishedLadderRoute);
        }
    }

    private void OnFinishedLadderRoute()
    {
        if (seatDebugLogs) Debug.Log($"{name} Sit: ladder traversal complete");
        if (_seat == null || _seatTf == null) { Fail(); return; }
        if (!AgentReady()) { if (seatDebugLogs) Debug.LogWarning($"{name} Sit: agent not ready after ladder."); Fail(); return; }
        if (!npc.CanReachPosition(_preSitNavPos, out NavMeshPath path))
            { if (seatDebugLogs) Debug.LogWarning($"{name} Sit: cannot reach seat after ladder."); Fail(); return; }
        Agent.isStopped = false; Agent.autoBraking = true; Agent.ResetPath(); Agent.SetPath(path);
        _sitPhase = SitPhase.ApproachingFront;
    }

    private void TickApproachFront()
    {
        if (seatDebugLogs) Debug.Log($"{name} Sit: ApproachingFront hasPath={Agent.hasPath} remaining={Agent.remainingDistance:F2}");
        Agent.isStopped = false;
        if (!Agent.pathPending && Agent.pathStatus == NavMeshPathStatus.PathInvalid)
            { if (seatDebugLogs) Debug.LogWarning($"{name} Sit: path invalid approaching."); Fail(); return; }
        if (!Agent.pathPending && !Agent.hasPath)
            { if (seatDebugLogs) Debug.LogWarning($"{name} Sit: lost path approaching."); Fail(); return; }
        if (!Agent.pathPending && Vector3.Distance(Agent.destination, _preSitNavPos) > 0.15f)
            Agent.SetDestination(_preSitNavPos);

        float dist = Vector3.Distance(new Vector3(Body.position.x, 0f, Body.position.z),
                                      new Vector3(_preSitNavPos.x, 0f, _preSitNavPos.z));
        if (dist <= Mathf.Max(preSitArriveDistance, ArriveDistance))
        {
            if (seatDebugLogs) Debug.Log($"{name} Sit: Arrived at pre-sit point. Aligning.");
            Agent.isStopped = true; Agent.ResetPath(); _sitPhase = SitPhase.Aligning;
        }
    }

    private void TickAlign(float dt, Vector3 seatFacing)
    {
        Agent.isStopped = true;
        Quaternion targetRot = Quaternion.LookRotation(seatFacing, Vector3.up);
        Body.rotation = Quaternion.Slerp(Body.rotation, targetRot, dt * (TurnSmoothing * 1.5f));
        if (Mathf.Abs(Mathf.DeltaAngle(Body.eulerAngles.y, targetRot.eulerAngles.y)) <= Mathf.Max(0.5f, alignYawToleranceDeg))
            { if (seatDebugLogs) Debug.Log($"{name} Sit: Aligned. Backstepping."); _sitPhase = SitPhase.Backstepping; }
    }

    private void TickBackstep(float dt, Vector3 seatFacing)
    {
        Agent.isStopped = true;
        Body.position = Vector3.MoveTowards(Body.position, _backstepTarget, Mathf.Max(0.01f, backstepSpeed) * dt);
        Body.rotation = Quaternion.Slerp(Body.rotation, Quaternion.LookRotation(seatFacing, Vector3.up), dt * (TurnSmoothing * 2.0f));
        float planarDist = Vector3.Distance(new Vector3(Body.position.x, 0f, Body.position.z),
                                            new Vector3(_backstepTarget.x, 0f, _backstepTarget.z));
        if (planarDist <= 0.06f) { if (seatDebugLogs) Debug.Log($"{name} Sit: Backstepped. Beginning SitDown."); BeginSitDown(); }
    }

    private void BeginSitDown()
    {
        DetachAgentForAnimation();
        if (_seatTf != null) Body.rotation = Quaternion.LookRotation(GetSeatFacing(), Vector3.up);
        if (_sitLerpCoroutine != null) StopCoroutine(_sitLerpCoroutine);
        if (Anim != null)
        {
            Anim.SetFloat(ParamMovingX, 0f); Anim.SetFloat(ParamMovingY, 0f); Anim.SetFloat(ParamBlend, 0f);
            ResetAllAnimatorTriggers(); Anim.speed = 1f;
            if (useSitTriggerParam && !string.IsNullOrWhiteSpace(sitTriggerParam))
                Anim.SetTrigger(sitTriggerParam);
            else
                Anim.CrossFadeInFixedTime(sitDownStateName, sitCrossfade, sitAnimLayer, 0f);
            if (seatDebugLogs) Debug.Log($"{name} Sit: Start SitDown.");
        }
        _sitTriggerSent = true; _sitTriggerT = 0f; _sitPhase = SitPhase.SitDownPlaying;
        _sitLerpCoroutine = StartCoroutine(SitDownLerpRoutine());
    }

    private IEnumerator SitDownLerpRoutine()
    {
        float waited = 0f, maxWait = sitTriggerFallbackDelay + 0.15f;
        while (waited < maxWait)
        {
            if (!string.IsNullOrWhiteSpace(sitDownStateName) && Anim.GetCurrentAnimatorStateInfo(sitAnimLayer).IsName(sitDownStateName)) break;
            waited += Time.deltaTime; yield return null;
        }
        Vector3 lerpStart = Body.position;
        Vector3 lerpEnd   = _seatTf != null ? _seatTf.position + seatedRootOffset : Body.position;
        while (true)
        {
            if (_seatTf == null) yield break;
            AnimatorStateInfo info = Anim.GetCurrentAnimatorStateInfo(sitAnimLayer);
            if (!(!string.IsNullOrWhiteSpace(sitDownStateName) && info.IsName(sitDownStateName))) yield break;
            float t = Mathf.Clamp01(info.normalizedTime);
            Vector3 p = Body.position; p.x = Mathf.Lerp(lerpStart.x, lerpEnd.x, t); p.z = Mathf.Lerp(lerpStart.z, lerpEnd.z, t);
            Body.position = p;
            if (info.normalizedTime >= 0.95f) yield break;
            yield return null;
        }
    }

    private void TickSitDownPlaying(float dt)
    {
        if (Anim == null) return;
        _sitTriggerT += dt;
        AnimatorStateInfo info = Anim.GetCurrentAnimatorStateInfo(sitAnimLayer);
        bool inSitDown      = !string.IsNullOrWhiteSpace(sitDownStateName)      && info.IsName(sitDownStateName);
        bool inSitIdle      = !string.IsNullOrWhiteSpace(sitIdleStateName)      && info.IsName(sitIdleStateName);
        bool inSitIdleFloor = !string.IsNullOrWhiteSpace(sitIdleFloorStateName) && info.IsName(sitIdleFloorStateName);
        bool inTyping       = !string.IsNullOrWhiteSpace(typingStateName)       && info.IsName(typingStateName);
        bool inTalking      = !string.IsNullOrWhiteSpace(talkingStateName)      && info.IsName(talkingStateName);
        bool inGabbing      = !string.IsNullOrWhiteSpace(gabbingStateName)      && info.IsName(gabbingStateName);
        bool inDawdling     = !string.IsNullOrWhiteSpace(dawdlingStateName)     && info.IsName(dawdlingStateName);

        if (inSitIdle || inSitIdleFloor || inTyping || inTalking || inGabbing || inDawdling)
        {
            _sitPhase = SitPhase.SittingIdle;
            _seatedT = 0f;

            if (_seatTf != null)
                Body.rotation = Quaternion.LookRotation(GetSeatFacing(), Vector3.up);

            if (seatDebugLogs)
                Debug.Log($"{name} Sit: SittingIdle. floor={inSitIdleFloor} typing={inTyping} talking={inTalking} gabbing={inGabbing} dawdling={inDawdling}");
            return;
        }
        if (_sitTriggerSent && _sitTriggerT >= sitTriggerFallbackDelay)
        {
            if (!inSitDown && !inSitIdle && !inSitIdleFloor && !string.IsNullOrWhiteSpace(sitDownStateName))
                { if (seatDebugLogs) Debug.Log($"{name} Sit: Fallback Play('{sitDownStateName}')."); Anim.Play(sitDownStateName, sitAnimLayer, 0f); }
            _sitTriggerSent = false;
        }
        if (inSitDown && !Anim.IsInTransition(sitAnimLayer) && info.normalizedTime >= 0.95f) TriggerCorrectSitIdle();
    }

    private void TickSittingIdle(float dt, Vector3 seatForward)
    {
        ForceIdlePose();
        Body.rotation = Quaternion.LookRotation(seatForward, Vector3.up);
        if (snapToSeatWhenSeated && _seatTf != null)
        {
            Vector3 target = _seatTf.position + seatedRootOffset;
            Body.position = new Vector3(target.x, Body.position.y, target.z);
        }
        if (autoStandAfterSeconds > 0f) { _seatedT += dt; if (_seatedT >= autoStandAfterSeconds) BeginStandUp(); }
    }

    private void TickStandUpPlaying(float dt)
    {
        if (Anim == null) { FinishStandUpToPatrol(); return; }
        _standUpT += dt;
        AnimatorStateInfo info = Anim.GetCurrentAnimatorStateInfo(sitAnimLayer);
        bool inStandUp      = !string.IsNullOrWhiteSpace(standUpStateName)      && info.IsName(standUpStateName);
        bool inSitIdle      = !string.IsNullOrWhiteSpace(sitIdleStateName)      && info.IsName(sitIdleStateName);
        bool inSitIdleFloor = !string.IsNullOrWhiteSpace(sitIdleFloorStateName) && info.IsName(sitIdleFloorStateName);

        if (seatDebugLogs) Debug.Log($"{name} Sit: StandUpPlaying inStandUp={inStandUp} normalized={info.normalizedTime:F2}");

        if (_standUpTriggerSent && _standUpT >= sitTriggerFallbackDelay)
        {
            if (!inStandUp && !string.IsNullOrWhiteSpace(standUpStateName))
                { if (seatDebugLogs) Debug.Log($"{name} Sit: Fallback Play('{standUpStateName}')"); Anim.Play(standUpStateName, sitAnimLayer, 0f); }
            _standUpTriggerSent = false;
        }
        if (inStandUp && !Anim.IsInTransition(sitAnimLayer) && info.normalizedTime >= 1f)
            { if (seatDebugLogs) Debug.Log($"{name} Sit: StandUp complete."); FinishStandUpToPatrol(); return; }
        if (_standUpT > 0.20f && !Anim.IsInTransition(sitAnimLayer) && !inStandUp && !inSitIdle && !inSitIdleFloor)
            { if (seatDebugLogs) Debug.LogWarning($"{name} Sit: StandUp exited unexpectedly."); FinishStandUpToPatrol(); return; }
        if (_standUpT >= Mathf.Max(0.5f, TriggerMaxDuration))
            { if (seatDebugLogs) Debug.LogWarning($"{name} Sit: StandUp timeout."); FinishStandUpToPatrol(); }
    }

    private IEnumerator LerpBodyTo(Vector3 target, float duration)
    {
        Vector3 start = Body.position;
        float endX = target.x, endZ = target.z, elapsed = 0f;
        duration = Mathf.Max(0.01f, duration);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector3 p = Body.position; p.x = Mathf.Lerp(start.x, endX, t); p.z = Mathf.Lerp(start.z, endZ, t);
            Body.position = p; yield return null;
        }
        Vector3 final = Body.position; final.x = endX; final.z = endZ; Body.position = final;
    }

    private void FinishStandUpToPatrol()
    {
        if (Anim != null) { Anim.speed = 1f; ForceReturnToLocomotion(); Anim.Update(0f); }
        ReleaseSeatIfAny(); ClearLadderRoute(); _sitPhase = SitPhase.None;
        StartCoroutine(FinishStandUpGroundingRoutine());
    }

    private IEnumerator FinishStandUpGroundingRoutine()
    {
        ReattachAgentToNavmeshAtCurrentXZ(); yield return null;
        ReattachAgentToNavmeshAtCurrentXZ();
        if (Anim != null) { Anim.Update(0f); ForceIdlePose(); }
        npc.HasCommand = false; npc.CommandGoal = NPCState.Patrolling;
        _sitPhase = SitPhase.None;
        npc.SetStateDirectly(NPCState.Patrolling);
        npc.ExecutePendingPostStandAction();
    }

    public void ForceCancelSitting()
    {
        StopAllCoroutines();
        _sitLerpCoroutine = _standLerpCoroutine = null;
        _standUpT = _seatSearchT = _seatRescanT = _seatedT = _sitTriggerT = 0f;
        _standUpTriggerSent = _sitTriggerSent = false;
        ReleaseSeatIfAny(); ClearLadderRoute();
        _seatNavPos = _preSitNavPos = _preSitPoint = _backstepTarget = Vector3.zero;
        _sitPhase = SitPhase.None;
        if (Anim != null) { Anim.speed = 1f; ResetAllAnimatorTriggers(); ForceReturnToLocomotion(); Anim.Update(0f); ForceIdlePose(); }
        if (Agent != null)
        {
            if (!Agent.enabled) Agent.enabled = true;
            if (!Agent.isOnNavMesh && TryGetNavmeshPoint(Body.position, out Vector3 navPos)) Agent.Warp(navPos);
            if (AgentReady()) { Agent.isStopped = false; Agent.ResetPath(); Agent.velocity = Vector3.zero; Agent.autoBraking = true; Agent.stoppingDistance = Mathf.Max(0.05f, ArriveDistance); }
        }
        npc.HasCommand = false; npc.CommandGoal = NPCState.Patrolling;
    }

    private void Fail()
    {
        if (seatDebugLogs) Debug.LogWarning($"{name} Sit: FAIL");
        ReleaseSeatIfAny(); ClearLadderRoute();
        npc.HasCommand = false; npc.CommandGoal = NPCState.Patrolling;
        _sitPhase = SitPhase.None;
        if (Anim != null) ForceReturnToLocomotion();
        ReattachAgentToNavmeshAtCurrentXZ();
        EnterState(NPCState.Patrolling);
    }

    private void ReleaseSeatIfAny() { if (_seat != null) _seat.Release(npc); _seat = null; _seatTf = null; }
    private void ClearLadderRoute()  { _routeLadder = null; _routeGoingUp = false; _routeApproachPoint = _routeExitPoint = Vector3.zero; }
}