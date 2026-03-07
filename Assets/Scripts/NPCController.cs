using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NPCController : MonoBehaviour
{
    public enum NPCState { Patrolling, Alerted, Approaching, Attacking, Seeking, Talk, Sitting, Lying }

    [Header("Identity")]
    public NPC thisNPC = NPC.Eimear_Scott;

    [Header("AI State Machine")]
    [Tooltip("If true, the NPC runs the FSM.")]
    public bool useStateMachine = true;
    [Tooltip("Current target the NPC can spot/follow/attack.")]
    public Transform currentTarget;
    [Tooltip("Patrol/seeking area radius from starting point.")]
    public float activeRadius = 15f;

    [Header("Testing / Overrides")]
    [Tooltip("TESTING: Allow chase/approach outside Active Radius.")]
    public bool allowApproachOutsideActiveRadius = false;
    [Tooltip("TESTING: Pick a target NPC by enum.")]
    public NPC debugTargetNPC = NPC.Eimear_Scott;

    [Header("Perception")]
    public float visionRange = 12f;
    [Range(0f, 360f)] public float visionFOV = 120f;
    public bool requireLineOfSight = false;
    public LayerMask losBlockers = ~0;

    [Header("Combat")]
    public float attackRange = 1.8f;
    public float alertedDuration = 0.35f;

    [Header("Interaction / Talk")]
    public float talkRange = 4.0f;

    [Header("Patrol")]
    public Vector2 patrolChangeDirInterval = new Vector2(3f, 7f);
    public Vector2 patrolArrivePause = new Vector2(0.2f, 1.0f);
    public int patrolPointTries = 10;
    public float patrolSampleRadius = 2.0f;

    [Header("Seeking")]
    public float seekingSpeedMultiplier = 1.2f;
    public float seekGiveUpSeconds = 6f;
    public float approachRepathInterval = 0.25f;

    [Header("Debug (Runtime)")]
    [SerializeField] private NPCState _state = NPCState.Patrolling;

    [Header("Approach Smoothing")]
    public float approachTargetMoveThreshold = 0.35f;
    public bool disableBrakingWhileApproaching = true;

    public NPCManager sceneNPCManager;

    [Header("Turn / BlendTree Driving")]
    [SerializeField] private float turnAngleForFullX = 90f;
    [SerializeField] private float turnInPlaceSpeed = 160f;
    [SerializeField] private float turnWhileMovingSpeed = 260f;
    [SerializeField] private float turnInPlaceSpeedThreshold = 0.15f;

    Vector3 _lastApproachDest;
    bool _hasLastApproachDest;

    // FSM runtime
    Vector3 _spawnPoint, _patrolDestination;
    bool _hasPatrolDestination;
    float _stateTimer, _patrolChangeTimer, _patrolArriveTimer, _approachRepathTimer, _seekTimer;

    // Conversation
    private readonly HashSet<NPCController> _conversationSpeakers = new HashSet<NPCController>();
    private NPCController _primaryConversationSpeaker;
    bool _isConversationLocked;
    bool _hasCommand;
    NPCState _commandGoal = NPCState.Patrolling;
    NPCController _talkTargetController;
    bool _registeredAsConversationSpeaker;

    [Header("Components")]
    public Animator animationController;
    public NavMeshAgent agent;

    [Header("Animation Root Safety")]
    [Tooltip("Optional override. If empty, uses animationController.transform.")]
    [SerializeField] private Transform animationRootOverride;

    private Transform _animationRoot;
    private bool _hasAnimationRootCachedXZ;
    private Vector2 _animationRootCachedLocalXZ;

    [Header("Arrival")]
    public float arriveDistance = 0.3f;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float angularSpeed = 240f;
    public float acceleration = 8f;

    [Header("Smoothing")]
    public float animDampTime = 0.15f;
    public float turnSmoothing = 10f;

    [Header("NavMesh Placement")]
    public float snapToNavMeshRadius = 2f;
    public bool autoSnapToNavMeshOnGo = true;

    [Header("Animator Params")]
    public string paramBlend = "Blend";
    public string paramMovingX = "MovingX";
    public string paramMovingY = "MovingY";

    [Header("Triggered Animations")]
    public List<string> triggerNames = new List<string>();
    [HideInInspector] public int selectedTriggerIndex = 0;
    public float triggerEnterTimeout = 1.0f;
    public float triggerMaxDuration = 8.0f;
    public float triggerExitBuffer = 0.05f;

    [Header("Locomotion / Blend Tree Return")]
    [SerializeField] private string locomotionStateName = "Locomotion";
    [SerializeField] private int locomotionLayer = 0;
    [SerializeField] private float locomotionCrossfade = 0.12f;

    [NonSerialized] public bool isPaused = false;

    Coroutine _triggerCoroutine;
    bool _isPlayingTriggeredAnimation;

    [Header("Crowd Avoidance")]
    public LayerMask npcLayerMask = ~0;
    public float personalSpaceRadius = 0.9f;
    [Range(-1f, 1f)] public float inFrontDotThreshold = 0.35f;
    public float sidestepDistance = 0.9f;
    public float sidestepDuration = 0.7f;
    public float renegotiateCooldown = 0.8f;
    public float renegotiateCheckInterval = 0.15f;

    public MeNPC npcMetaActions;

    private Action _pendingPostStandAction;
    private bool _executePendingActionAfterStand;

    float _renegotiateT, _renegotiateCooldownT;
    Coroutine _sidestepCoroutine;
    Vector3 _resumeDestination;
    bool _hasResumeDestination;

    bool _triggerPrevPaused, _triggerAgentWasValid, _triggerWasStoppedBefore;

    // --- SITTING ---
    private Vector3 _seatNavPos, _preSitNavPos;

    [SerializeField] private float sitTriggerFallbackDelay = 0.08f;
    private float _sitTriggerT;
    private bool _sitTriggerSent;

    private enum SitPhase { None, SearchingSeat, ApproachingFront, Aligning, Backstepping, SitDownPlaying, SittingIdle, StandUpPlaying }

    [Header("Sitting")]
    [SerializeField] private LayerMask seatLayerMask;
    [SerializeField] private bool seatDebugLogs = false;
    [SerializeField] private float seatRescanInterval = 0.35f;
    [SerializeField] private float seatSearchRadius = 0f;
    [SerializeField] private float preSitForwardOffset = 0.45f;
    [SerializeField] private float preSitArriveDistance = 0.35f;
    [SerializeField] private float alignYawToleranceDeg = 6f;
    [SerializeField] private float backstepSpeed = 0.8f;
    [SerializeField] private bool snapToSeatWhenSeated = true;

    [Header("Sitting Placement")]
    [SerializeField] private float autoStandAfterSeconds = 0f;

    [Header("Sitting Animations")]
    [SerializeField] private string sitDownStateName = "SitDown";
    [SerializeField] private string sitIdleStateName = "SitIdle";
    [SerializeField] private int sitAnimLayer = 0;
    [SerializeField] private float sitCrossfade = 0.10f;
    [SerializeField] private bool useSitTriggerParam = false;
    [SerializeField] private string sitTriggerParam = "SitDown";

    [Header("Stand Up Animations")]
    [SerializeField] private bool useStandUpTriggerParam = true;
    [SerializeField] private string standUpTriggerParam = "StandUp";
    [SerializeField] private string standUpStateName = "StandUp";

    private SitPhase _sitPhase = SitPhase.None;
    private Seat _seat;
    private Transform _seatTf;
    private float _seatRescanT, _seatedT;

    // --- LYING / BED ---
    private Vector3 _bedNavPos, _preLieNavPos;
    private Vector3 _bedLieWorldPos;
    private Transform _bedFootTf;
    private Transform _bedLieTf;
    private bool _snappedToBedLyingPose;
    private bool _forcedLiePoseByTimer;

    private enum LiePhase
    {
        None,
        SearchingBed,
        ApproachingFront,
        Aligning,
        LieDownPlaying,
        LyingIdle,
        WakeUpPlaying
    }

    [Header("Lying / Bed")]
    [SerializeField] private LayerMask bedLayerMask;
    [SerializeField] private bool bedDebugLogs = false;
    [SerializeField] private float bedRescanInterval = 0.35f;
    [SerializeField] private float bedSearchRadius = 0f;
    [SerializeField] private float preLieForwardOffset = 0.7f;
    [SerializeField] private float preLieArriveDistance = 0.45f;
    [SerializeField] private float bedAlignYawToleranceDeg = 6f;
    [SerializeField] private bool snapToBedWhenLying = true;

    [Header("Lying Placement")]
    [SerializeField] private float autoWakeAfterSeconds = 0f;
    [SerializeField] private float bodyRecoverySampleRadius = 4f;
    [SerializeField] private float lieDownSnapDelay = 0.55f;

    [Header("Lying Animations")]
    [SerializeField] private string lieDownStateName = "LieDown";
    [SerializeField] private string lieIdleStateName = "LieIdle";
    [SerializeField] private int lieAnimLayer = 0;
    [SerializeField] private float lieCrossfade = 0.10f;
    [SerializeField] private bool useLieDownTriggerParam = true;
    [SerializeField] private string lieDownTriggerParam = "LieDown";
    [SerializeField] private float lieTriggerFallbackDelay = 0.08f;

    [SerializeField] private bool useLieIdleTriggerParam = true;
    [SerializeField] private string lieIdleTriggerParam = "LieIdle";

    [Header("Wake Up Animations")]
    [SerializeField] private bool useWakeUpTriggerParam = true;
    [SerializeField] private string wakeUpTriggerParam = "WakeUp";
    [SerializeField] private string wakeUpStateName = "WakeUp";
    [SerializeField] private float wakeTriggerFallbackDelay = 0.08f;

    private LiePhase _liePhase = LiePhase.None;
    private Bed _bed;
    private Transform _bedTf;
    private float _bedRescanT, _lyingT;
    private float _lieTriggerT, _wakeTriggerT;
    private bool _lieTriggerSent, _wakeTriggerSent;

    // cached local XZ for child animation root restoration
    private void ResolveAnimationRoot()
    {
        _animationRoot = animationRootOverride != null
            ? animationRootOverride
            : (animationController != null ? animationController.transform : null);

        _hasAnimationRootCachedXZ = false;
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
        if (_animationRoot == null) return;
        if (!_hasAnimationRootCachedXZ) return;

        Vector3 lp = _animationRoot.localPosition;
        lp.x = _animationRootCachedLocalXZ.x;
        lp.z = _animationRootCachedLocalXZ.y;
        _animationRoot.localPosition = lp;
    }

    bool IsNavDriven() => !isPaused && !_isPlayingTriggeredAnimation && useStateMachine;

    public NPCController ResolveNPC(NPC npcEnum)
    {
        var list = sceneNPCManager?.NPCList;
        if (list == null) return null;
        foreach (var c in list)
            if (c != null && c.thisNPC == npcEnum) return c;
        return null;
    }

    private void EnsureBedLayerMask()
    {
        bedLayerMask = LayerMask.GetMask("BED");
        if (bedLayerMask.value == 0)
            Debug.LogWarning($"{name}: Layer 'BED' not found. Create it in Project Settings > Tags and Layers.");
    }

    private void EnsureSeatLayerMask()
    {
        seatLayerMask = LayerMask.GetMask("SEAT");
        if (seatLayerMask.value == 0)
            Debug.LogWarning($"{name}: Layer 'SEAT' not found. Create it in Project Settings > Tags and Layers.");
    }

    private bool AgentReady() =>
        agent != null && agent.isActiveAndEnabled && agent.enabled && agent.isOnNavMesh;

    private bool TryGetNavmeshPoint(Vector3 near, out Vector3 navPos)
    {
        navPos = near;
        if (NavMesh.SamplePosition(near, out NavMeshHit hit, snapToNavMeshRadius, NavMesh.AllAreas))
        {
            navPos = hit.position;
            return true;
        }
        return false;
    }

    private bool TryGetNavmeshPointNear(Vector3 near, float radius, out Vector3 navPos)
    {
        navPos = near;
        if (NavMesh.SamplePosition(near, out NavMeshHit hit, radius, NavMesh.AllAreas))
        {
            navPos = hit.position;
            return true;
        }
        return false;
    }

    private Quaternion GetPlanarLookRotation(Vector3 forward)
    {
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
            forward = transform.forward;

        forward.Normalize();
        return Quaternion.LookRotation(forward, Vector3.up);
    }

    private void SnapTransformXZTo(Vector3 worldPos)
    {
        transform.position = new Vector3(worldPos.x, transform.position.y, worldPos.z);
    }

    private void ForceUpright()
    {
        Vector3 e = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, e.y, 0f);
    }

    private bool GroundTransformToNavmesh(Vector3 preferred, float sampleRadius = 3f)
    {
        if (TryGetNavmeshPointNear(preferred, sampleRadius, out Vector3 navPos))
        {
            transform.position = navPos;
            ForceUpright();
            return true;
        }

        if (TryGetNavmeshPoint(transform.position, out navPos))
        {
            transform.position = navPos;
            ForceUpright();
            return true;
        }

        return false;
    }

    private bool RestoreStandingBodyAt(Vector3 preferred, float sampleRadius)
    {
        if (!GroundTransformToNavmesh(preferred, sampleRadius))
            return false;

        if (agent == null) return true;

        if (!agent.enabled)
            agent.enabled = true;

        if (TryGetNavmeshPointNear(transform.position, sampleRadius, out Vector3 navPos))
        {
            transform.position = navPos;
            ForceUpright();
            agent.Warp(navPos);
            agent.isStopped = false;
            agent.ResetPath();
            return true;
        }

        return false;
    }

    private void DetachAgentForAnimation()
    {
        if (agent == null) return;
        if (AgentReady())
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
        agent.velocity = Vector3.zero;
        agent.enabled = false;
    }

    private void ReattachAgentToNavmeshAtCurrentXZ()
    {
        if (agent == null) return;
        if (TryGetNavmeshPoint(transform.position, out Vector3 navPos))
        {
            transform.position = navPos;
            agent.enabled = true;
            agent.Warp(navPos);
            agent.isStopped = false;
            agent.ResetPath();
        }
        else
        {
            agent.enabled = true;
            Debug.LogWarning($"{name}: Could not find NavMesh near {transform.position} to reattach agent.");
        }
    }

    private NPCController GetNearestConversationSpeaker()
    {
        NPCController best = null;
        float bestSqr = float.PositiveInfinity;
        foreach (var s in _conversationSpeakers)
        {
            if (s == null) continue;
            float sqr = (s.transform.position - transform.position).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; best = s; }
        }
        return best;
    }

    private bool IsInConversation() =>
        _registeredAsConversationSpeaker || _isConversationLocked ||
        _talkTargetController != null || _conversationSpeakers.Count > 0;

    private NPCController GetConversationPartner()
    {
        if (_talkTargetController != null) return _talkTargetController;
        return currentTarget != null ? currentTarget.GetComponentInParent<NPCController>() : null;
    }

    private void EndConversationLocalOnly(NPCController other)
    {
        if (other != null) _conversationSpeakers.Remove(other);
        _conversationSpeakers.Clear();
        _primaryConversationSpeaker = null;
        _isConversationLocked = false;
        _registeredAsConversationSpeaker = false;
        _talkTargetController = null;
        if (other != null && currentTarget == other.transform) currentTarget = null;
        useStateMachine = true;
        if (AgentReady()) { agent.isStopped = false; agent.ResetPath(); }
        if (animationController != null && _state != NPCState.Sitting && _state != NPCState.Lying)
            ForceReturnToLocomotion();
        if (_state == NPCState.Talk)
        {
            _hasCommand = false;
            _commandGoal = NPCState.Patrolling;
            EnterState(NPCState.Patrolling);
        }
    }

    private void EndConversationForBoth()
    {
        if (!IsInConversation()) return;
        var other = GetConversationPartner();
        EndConversationLocalOnly(other);
        other?.EndConversationLocalOnly(this);
    }

    private void UnregisterAsConversationSpeaker()
    {
        if (!IsInConversation())
        {
            _registeredAsConversationSpeaker = false;
            _talkTargetController = null;
            return;
        }
        EndConversationForBoth();
    }

    private void InterruptAllTransientActions(bool returnToLocomotion = true)
    {
        UnregisterAsConversationSpeaker();

        if (_triggerCoroutine != null)
        {
            StopCoroutine(_triggerCoroutine);
            _triggerCoroutine = null;
        }

        _isPlayingTriggeredAnimation = false;
        isPaused = false;

        if (animationController != null)
        {
            animationController.speed = 1f;
            ResetAllAnimatorTriggers();
        }

        _conversationSpeakers.Clear();
        _primaryConversationSpeaker = null;
        _isConversationLocked = false;

        if (_sidestepCoroutine != null)
        {
            StopCoroutine(_sidestepCoroutine);
            _sidestepCoroutine = null;
        }

        _hasResumeDestination = false;

        if (agent != null)
        {
            if (!agent.enabled) agent.enabled = true;
            if (!agent.isOnNavMesh && TryGetNavmeshPoint(transform.position, out Vector3 navPos))
                agent.Warp(navPos);
            if (AgentReady())
            {
                agent.isStopped = false;
                agent.ResetPath();
            }
        }

        ResetLocomotionState();

        if (returnToLocomotion && animationController != null)
            ForceReturnToLocomotion();
    }

    public void SetTargetByNPC(NPC npcEnum)
    {
        var c = ResolveNPC(npcEnum);
        if (c == null) { Debug.LogWarning($"{name}: Could not find target NPC '{npcEnum}'."); return; }
        if (c == this) { Debug.LogWarning($"{name}: Tried to target self ({npcEnum})."); return; }
        SetTarget(c.transform);
        _talkTargetController = c;
    }

    private void BeginCommandApproach(NPCState goal)
    {
        void Execute()
        {
            InterruptAllTransientActions();
            useStateMachine = true;
            SetTargetByNPC(debugTargetNPC);
            if (currentTarget == null) return;

            _hasCommand = true;
            _commandGoal = goal;

            if (AgentReady())
            {
                agent.autoBraking = false;
                agent.stoppingDistance = Mathf.Max(arriveDistance, talkRange * 0.8f);
            }

            EnterState(Vector3.Distance(transform.position, currentTarget.position) <= talkRange ? goal : NPCState.Approaching);
        }

        if (TryQueueActionAfterStand(Execute)) return;
        Execute();
    }

    public void DebugApproachTargetNPC()
    {
        void Execute()
        {
            InterruptAllTransientActions();
            useStateMachine = true;
            _hasCommand = false;
            _commandGoal = NPCState.Patrolling;
            SetTargetByNPC(debugTargetNPC);
            if (currentTarget != null) EnterState(NPCState.Approaching);
        }

        if (TryQueueActionAfterStand(Execute)) return;
        Execute();
    }

    public void DebugAttackTargetNPC() => BeginCommandApproach(NPCState.Attacking);
    public void DebugTalkTargetNPC() => BeginCommandApproach(NPCState.Talk);

    void FaceTowards(Vector3 worldPos, float dt, float speed)
    {
        Vector3 to = worldPos - transform.position;
        to.y = 0f;
        if (to.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation,
            Quaternion.LookRotation(to.normalized, Vector3.up), dt * speed);
    }

    void ResetAllAnimatorTriggers()
    {
        if (animationController == null) return;
        foreach (var p in animationController.parameters)
            if (p.type == AnimatorControllerParameterType.Trigger)
                animationController.ResetTrigger(p.name);
    }

    private void ForceReturnToLocomotion()
    {
        if (animationController == null) return;
        ResetAllAnimatorTriggers();
        if (!string.IsNullOrEmpty(locomotionStateName))
        {
            animationController.CrossFadeInFixedTime(locomotionStateName, locomotionCrossfade, locomotionLayer, 0f);
            animationController.Update(0f);
        }
        SnapBlendTreeParamsNow();
    }

    private void SnapBlendTreeParamsNow()
    {
        if (animationController == null) return;
        Vector3 velocity = AgentReady() ? agent.velocity : Vector3.zero;
        Vector3 localVel = transform.InverseTransformDirection(velocity);
        float speed = velocity.magnitude;
        animationController.SetFloat(paramMovingX, Mathf.Clamp(localVel.x, -1f, 1f));
        animationController.SetFloat(paramMovingY, Mathf.Clamp(localVel.z, -1f, 1f));
        animationController.SetFloat(paramBlend, (moveSpeed <= 0.001f) ? 0f : Mathf.Clamp01(speed / moveSpeed));
    }

    void Reset()
    {
        animationController = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Awake()
    {
        EnsureSeatLayerMask();
        EnsureBedLayerMask();
        var mgr = FindFirstObjectByType<NPCManager>();
        if (mgr != null) mgr.RegisterNPC(this);
        else Debug.LogWarning($"{name}: No NPCManager found in scene during Awake.");
    }

    public void Start()
    {
        if (animationController == null) animationController = GetComponentInChildren<Animator>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        ResolveAnimationRoot();

        if (animationController != null)
            animationController.applyRootMotion = false;

        _spawnPoint = transform.position;

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.angularSpeed = angularSpeed;
            agent.acceleration = acceleration;
            agent.stoppingDistance = Mathf.Max(agent.stoppingDistance, arriveDistance);
            agent.autoBraking = true;
            agent.autoRepath = true;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.updateRotation = false;
            agent.avoidancePriority = Mathf.Clamp((Mathf.Abs(gameObject.GetInstanceID()) % 90) + 5, 0, 99);
        }

        if (seatLayerMask.value == 0)
        {
            int seatLayer = LayerMask.NameToLayer("SEAT");
            if (seatLayer >= 0) seatLayerMask = 1 << seatLayer;
        }

        if (bedLayerMask.value == 0)
        {
            int bedLayer = LayerMask.NameToLayer("BED");
            if (bedLayer >= 0) bedLayerMask = 1 << bedLayer;
        }

        if (useStateMachine)
        {
            ForceReturnToLocomotion();
            EnterState(NPCState.Patrolling);
        }
    }

    void Update()
    {
        if (_isConversationLocked)
        {
            if (AgentReady()) agent.isStopped = true;
            ForceIdlePose();
            _primaryConversationSpeaker = GetNearestConversationSpeaker();
            if (_primaryConversationSpeaker != null)
                FaceTowards(_primaryConversationSpeaker.transform.position, Time.deltaTime, turnSmoothing * 1.5f);
            UpdateLocomotionAndFacing();
            return;
        }

        if (useStateMachine && !_isPlayingTriggeredAnimation)
        {
            if (_state == NPCState.Sitting || _state == NPCState.Lying)
                TickFSM(Time.deltaTime);
            else if (EnsureFSMCanRun())
                TickFSM(Time.deltaTime);
        }

        UpdateLocomotionAndFacing();
        PreCollisionRenegotiate();
    }

    bool EnsureFSMCanRun() =>
        agent != null && agent.isActiveAndEnabled && EnsureAgentOnNavMesh("FSM");

    public NPCState GetCurrentState() => _state;

    public void SetTarget(Transform t)
    {
        currentTarget = t;
        if (useStateMachine && !_isPlayingTriggeredAnimation && !_hasCommand &&
            _state != NPCState.Sitting && _state != NPCState.Lying)
        {
            EnterState(NPCState.Alerted);
        }
    }

    public void ClearTarget()
    {
        EndConversationForBoth();
        currentTarget = null;
        _hasCommand = false;
        _hasResumeDestination = false;
        _resumeDestination = Vector3.zero;
        _commandGoal = NPCState.Patrolling;
    }

    void UpdateLocomotionAndFacing()
    {
        if (animationController == null) return;

        bool inSitWalkup = (_state == NPCState.Sitting && _sitPhase == SitPhase.ApproachingFront);
        bool inLieWalkup = (_state == NPCState.Lying && _liePhase == LiePhase.ApproachingFront);

        bool blockSitLocomotion = (_state == NPCState.Sitting && !inSitWalkup);
        bool blockLieLocomotion = (_state == NPCState.Lying && !inLieWalkup);

        bool allowNavLocomotion = AgentReady() && IsNavDriven() && !blockSitLocomotion && !blockLieLocomotion;

        float movingX = 0f, movingY = 0f, blend = 0f;

        if (allowNavLocomotion)
        {
            Vector3 to = agent.steeringTarget - transform.position;
            to.y = 0f;

            Vector3 desiredDir = to.sqrMagnitude > 0.0001f ? to.normalized
                : (agent.desiredVelocity.sqrMagnitude > 0.0001f
                    ? new Vector3(agent.desiredVelocity.x, 0f, agent.desiredVelocity.z).normalized
                    : transform.forward);

            Vector3 v = agent.velocity; v.y = 0f;
            Vector3 dv = agent.desiredVelocity; dv.y = 0f;

            float speed = v.magnitude;
            if (agent.pathPending || (speed < 0.05f && dv.magnitude > 0.05f))
                speed = dv.magnitude;

            float speed01 = Mathf.Clamp01(speed / Mathf.Max(0.01f, moveSpeed));
            float signedAngle = Vector3.SignedAngle(transform.forward, desiredDir, Vector3.up);
            if (Mathf.Abs(signedAngle) < 8f) signedAngle = 0f;

            float turnWeight = (speed < turnInPlaceSpeedThreshold) ? 1f : 0.35f;
            movingX = Mathf.Clamp((signedAngle / Mathf.Max(1f, turnAngleForFullX)) * turnWeight, -1f, 1f);
            movingY = speed01;
            blend = speed01;

            if (speed01 > 0.35f && Mathf.Abs(movingX) < 0.2f)
                movingX = 0f;

            float turnRate = (speed < turnInPlaceSpeedThreshold) ? turnInPlaceSpeed : turnWhileMovingSpeed;
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                Quaternion.LookRotation(desiredDir, Vector3.up), turnRate * Time.deltaTime);
        }

        animationController.SetFloat(paramMovingX, movingX, animDampTime, Time.deltaTime);
        animationController.SetFloat(paramMovingY, movingY, animDampTime, Time.deltaTime);
        animationController.SetFloat(paramBlend, blend, animDampTime, Time.deltaTime);
    }

    private void ResetLocomotionState()
    {
        _hasLastApproachDest = false;
        _hasResumeDestination = false;
        if (animationController != null)
        {
            animationController.SetFloat(paramMovingX, 0f);
            animationController.SetFloat(paramMovingY, 0f);
            animationController.SetFloat(paramBlend, 0f);
        }
    }

    public void ForcePatrol()
    {
        void Execute()
        {
            InterruptAllTransientActions();
            if (animationController != null) animationController.speed = 1f;

            useStateMachine = true;
            _hasCommand = false;
            _commandGoal = NPCState.Patrolling;

            if (agent != null && agent.isActiveAndEnabled)
            {
                if (!agent.enabled) agent.enabled = true;
                if (!agent.isOnNavMesh && TryGetNavmeshPoint(transform.position, out Vector3 navPos))
                    agent.Warp(navPos);
            }

            _hasResumeDestination = false;
            _resumeDestination = Vector3.zero;

            ResetLocomotionState();
            ForceReturnToLocomotion();
            EnterState(NPCState.Patrolling);
        }

        if (TryQueueActionAfterStand(Execute)) return;
        Execute();
    }

    public void RequestSitDown()
    {
        if (_isConversationLocked) return;

        if (_state == NPCState.Sitting)
        {
            if (_sitPhase == SitPhase.SittingIdle || _sitPhase == SitPhase.SitDownPlaying || _sitPhase == SitPhase.StandUpPlaying)
            {
                _pendingPostStandAction = StartSitCommandFresh;
                _executePendingActionAfterStand = true;
                if (_sitPhase != SitPhase.StandUpPlaying) BeginStandUp();
                return;
            }

            StartSitCommandFresh();
            return;
        }

        StartSitCommandFresh();
    }

    public void RequestStandUp()
    {
        if (_state != NPCState.Sitting) return;
        if (_sitPhase == SitPhase.SittingIdle || _sitPhase == SitPhase.SitDownPlaying)
            BeginStandUp();
    }

    public void RequestLieDown()
    {
        if (_isConversationLocked) return;

        if (_state == NPCState.Lying)
        {
            if (_liePhase == LiePhase.LyingIdle || _liePhase == LiePhase.LieDownPlaying || _liePhase == LiePhase.WakeUpPlaying)
            {
                _pendingPostStandAction = StartLieCommandFresh;
                _executePendingActionAfterStand = true;

                if (_liePhase != LiePhase.WakeUpPlaying)
                    BeginWakeUp();

                return;
            }

            StartLieCommandFresh();
            return;
        }

        StartLieCommandFresh();
    }

    public void RequestWakeUp()
    {
        if (_state != NPCState.Lying) return;

        if (_liePhase == LiePhase.LyingIdle || _liePhase == LiePhase.LieDownPlaying)
            BeginWakeUp();
    }

    private void StartLieCommandFresh()
    {
        InterruptAllTransientActions();

        currentTarget = null;
        _talkTargetController = null;
        _registeredAsConversationSpeaker = false;

        useStateMachine = true;
        _hasCommand = true;
        _commandGoal = NPCState.Lying;

        _hasLastApproachDest = false;
        _hasPatrolDestination = false;
        _hasResumeDestination = false;
        _resumeDestination = Vector3.zero;

        if (agent != null)
        {
            if (!agent.enabled) agent.enabled = true;
            if (!agent.isOnNavMesh && TryGetNavmeshPoint(transform.position, out Vector3 navPos))
                agent.Warp(navPos);
        }

        if (AgentReady())
        {
            agent.autoBraking = true;
            agent.stoppingDistance = Mathf.Max(0.05f, arriveDistance);
            agent.isStopped = false;
            agent.ResetPath();
        }

        _state = NPCState.Lying;
        _stateTimer = 0f;
        EnterLying();
    }

    void EnterState(NPCState next)
    {
        if (_state == NPCState.Talk && next != NPCState.Talk)
            UnregisterAsConversationSpeaker();

        _state = next;
        _stateTimer = 0f;

        if (AgentReady())
            agent.isStopped = false;

        switch (_state)
        {
            case NPCState.Patrolling:
                if (AgentReady())
                {
                    agent.speed = moveSpeed;
                    agent.autoBraking = true;
                    agent.isStopped = false;
                    agent.stoppingDistance = Mathf.Max(arriveDistance, 0.05f);
                }
                _hasPatrolDestination = false;
                _patrolChangeTimer = UnityEngine.Random.Range(patrolChangeDirInterval.x, patrolChangeDirInterval.y);
                _patrolArriveTimer = 0f;
                break;

            case NPCState.Alerted:
                if (AgentReady())
                {
                    agent.speed = moveSpeed;
                    agent.isStopped = true;
                    agent.autoBraking = true;
                    agent.stoppingDistance = Mathf.Max(arriveDistance, 0.05f);
                }
                break;

            case NPCState.Approaching:
                if (AgentReady())
                {
                    agent.speed = moveSpeed;
                    if (disableBrakingWhileApproaching) agent.autoBraking = false;
                    agent.isStopped = false;
                    ApplyStoppingDistanceForCurrentMode();
                }
                _approachRepathTimer = 0f;
                _hasLastApproachDest = false;
                if (_hasCommand) PushImmediateCommandDestination();
                break;

            case NPCState.Attacking:
                if (AgentReady())
                {
                    agent.isStopped = true;
                    agent.autoBraking = true;
                    agent.stoppingDistance = Mathf.Max(arriveDistance, 0.05f);
                }
                Debug.Log($"{name} ATTACKING: TODO hook up attack logic/animation.");
                break;

            case NPCState.Seeking:
                if (AgentReady())
                {
                    agent.speed = moveSpeed * Mathf.Max(0.01f, seekingSpeedMultiplier);
                    agent.isStopped = false;
                    agent.stoppingDistance = Mathf.Max(arriveDistance, 0.05f);
                }
                _seekTimer = 0f;
                _hasPatrolDestination = false;
                _patrolChangeTimer = UnityEngine.Random.Range(patrolChangeDirInterval.x, patrolChangeDirInterval.y);
                _patrolArriveTimer = 0f;
                break;

            case NPCState.Talk:
                if (AgentReady())
                {
                    agent.isStopped = true;
                    agent.autoBraking = true;
                    agent.ResetPath();
                    agent.stoppingDistance = Mathf.Max(arriveDistance, 0.05f);
                }
                Debug.Log($"{name} TALK: TODO hook up talk later. Holding idle until new command.");
                break;

            case NPCState.Sitting:
                EnterSitting();
                break;

            case NPCState.Lying:
                EnterLying();
                break;
        }
    }

    void TickFSM(float dt)
    {
        _stateTimer += dt;

        if (!_hasCommand &&
            _state != NPCState.Talk &&
            _state != NPCState.Sitting &&
            _state != NPCState.Lying &&
            currentTarget != null &&
            CanSeeTarget(currentTarget))
        {
            float distToSpawn = Vector3.Distance(_spawnPoint, currentTarget.position);
            if (distToSpawn > activeRadius && !allowApproachOutsideActiveRadius)
            {
                if (_state != NPCState.Seeking) EnterState(NPCState.Seeking);
            }
            else
            {
                if (_state == NPCState.Patrolling || _state == NPCState.Seeking)
                    EnterState(NPCState.Alerted);
            }
        }

        switch (_state)
        {
            case NPCState.Patrolling: TickPatrolling(dt, 1f); break;
            case NPCState.Alerted: TickAlerted(dt); break;
            case NPCState.Approaching: TickApproaching(dt); break;
            case NPCState.Attacking: TickAttacking(dt); break;
            case NPCState.Seeking: TickSeeking(dt); break;
            case NPCState.Talk: TickTalk(dt); break;
            case NPCState.Sitting: TickSitting(dt); break;
            case NPCState.Lying: TickLying(dt); break;
        }
    }

    // ── LYING / BED ───────────────────────────────────────────────────────────

    private void EnterLying()
    {
        _liePhase = LiePhase.SearchingBed;
        _bedRescanT = 0f;
        _lyingT = 0f;
        _lieTriggerT = 0f;
        _wakeTriggerT = 0f;
        _lieTriggerSent = false;
        _wakeTriggerSent = false;
        _snappedToBedLyingPose = false;
        _forcedLiePoseByTimer = false;

        ReleaseBedIfAny();

        if (agent != null)
        {
            if (!agent.enabled) agent.enabled = true;
            if (!agent.isOnNavMesh && TryGetNavmeshPoint(transform.position, out Vector3 navPos))
                agent.Warp(navPos);

            agent.autoBraking = true;
            agent.stoppingDistance = Mathf.Max(0.05f, arriveDistance);

            if (AgentReady())
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
        }
    }

    private void TickLying(float dt)
    {
        _hasCommand = true;
        _commandGoal = NPCState.Lying;

        switch (_liePhase)
        {
            case LiePhase.SearchingBed:
                TickSearchingBed(dt);
                break;

            case LiePhase.ApproachingFront:
                if (AgentReady()) TickApproachBedFront();
                break;

            case LiePhase.Aligning:
                if (_bedTf == null) { FailLyingAndReturnToPatrol(); return; }
                if (AgentReady()) TickAlignToBed(dt, GetBedFacing());
                break;

            case LiePhase.LieDownPlaying:
                TickLieDownPlaying(dt);
                break;

            case LiePhase.LyingIdle:
                if (_bedTf == null) { FailLyingAndReturnToPatrol(); return; }
                TickLyingIdle(dt, GetBedFacing());
                break;

            case LiePhase.WakeUpPlaying:
                TickWakeUpPlaying(dt);
                break;
        }
    }

    private Vector3 GetBedFacing()
    {
        // We want the NPC to stand at the foot facing AWAY from the bed.
        // So derive the direction from the lying marker back toward the foot marker.
        if (_bedFootTf != null && _bedLieTf != null)
        {
            Vector3 awayFromBed = _bedFootTf.position - _bedLieTf.position;
            awayFromBed.y = 0f;

            if (awayFromBed.sqrMagnitude > 0.0001f)
                return awayFromBed.normalized;
        }

        // Fallbacks if one marker is missing
        if (_bedFootTf != null)
        {
            Vector3 f = _bedFootTf.forward;
            f.y = 0f;
            if (f.sqrMagnitude > 0.0001f)
                return f.normalized;
        }

        return transform.forward;
    }

    private void SnapToBedLyingPoseNow()
    {
        Vector3 facing = GetBedFacing();

        // Keep the same direction as when standing at the foot
        transform.rotation = Quaternion.LookRotation(facing, Vector3.up);

        if (snapToBedWhenLying)
        {
            if (_bedLieTf != null)
                transform.position = _bedLieTf.position;
            else
                transform.position = _bedLieWorldPos;
        }

        _snappedToBedLyingPose = true;

        if (bedDebugLogs)
            Debug.Log($"{name} Bed: Snapped to lying pose on bed.");
    }

    private void TickSearchingBed(float dt)
    {
        _bedRescanT -= dt;

        if (_bedRescanT <= 0f)
        {
            _bedRescanT = Mathf.Max(0.05f, bedRescanInterval);
            if (TryAcquireBed())
                return;
        }

        if (AgentReady())
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        ForceIdlePose();
    }

    private bool TryAcquireBed()
    {
        float radius = (bedSearchRadius > 0.01f) ? bedSearchRadius : Mathf.Max(0.1f, activeRadius);
        Bed[] beds = FindObjectsByType<Bed>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (beds == null || beds.Length == 0)
        {
            if (bedDebugLogs) Debug.Log($"{name} Bed: No Bed components found.");
            return false;
        }

        Bed best = null;
        float bestSqr = float.PositiveInfinity;

        foreach (var b in beds)
        {
            if (!b || b.bedFootTransform == null || b.bedLyingTransform == null || b.IsOccupied)
                continue;

            bool matchesMask =
                ((1 << b.gameObject.layer) & bedLayerMask.value) != 0 ||
                ((1 << b.bedFootTransform.gameObject.layer) & bedLayerMask.value) != 0 ||
                ((1 << b.bedLyingTransform.gameObject.layer) & bedLayerMask.value) != 0;

            if (!matchesMask) continue;

            Vector3 bedPos = b.bedFootTransform.position;
            Vector3 d = bedPos - _spawnPoint;
            d.y = 0f;

            if (d.sqrMagnitude > radius * radius)
                continue;

            if (!NavMesh.SamplePosition(bedPos, out _, activeRadius, NavMesh.AllAreas))
            {
                if (bedDebugLogs) Debug.Log($"{name} Bed: REJECT '{b.name}' navmesh_sample_failed");
                continue;
            }

            float sqr = (transform.position - bedPos).sqrMagnitude;
            if (sqr < bestSqr)
            {
                best = b;
                bestSqr = sqr;
            }
        }

        if (best == null)
        {
            Debug.LogWarning($"{name} Bed: No valid/free bed found within range.");
            return false;
        }

        if (!best.TryOccupy(this))
        {
            Debug.LogWarning($"{name} Bed: Bed '{best.name}' refused occupancy.");
            return false;
        }

        _bed = best;
        _bedTf = best.transform;
        _bedFootTf = best.bedFootTransform;
        _bedLieTf = best.bedLyingTransform;

        if (_bedFootTf == null || _bedLieTf == null)
        {
            Debug.LogWarning($"{name} Bed: '{best.name}' is missing bedFootTransform or bedLyingTransform.");
            ReleaseBedIfAny();
            return false;
        }

        Vector3 bedFootPos = _bedFootTf.position;
        Vector3 bedLiePos = _bedLieTf.position;

        if (!NavMesh.SamplePosition(bedLiePos, out NavMeshHit bedLieHit, activeRadius, NavMesh.AllAreas))
        {
            Debug.LogWarning($"{name} Bed: Could not sample lying NavMesh point for '{best.name}'.");
            ReleaseBedIfAny();
            return false;
        }
        _bedNavPos = bedLieHit.position;
        _bedLieWorldPos = bedLiePos;

        if (!NavMesh.SamplePosition(bedFootPos, out NavMeshHit footHit, activeRadius, NavMesh.AllAreas))
        {
            Debug.LogWarning($"{name} Bed: Could not sample foot NavMesh point for '{best.name}'.");
            ReleaseBedIfAny();
            return false;
        }
        _preLieNavPos = footHit.position;

        if (bedDebugLogs)
        {
            Debug.Log($"{name} Bed: ACQUIRED '{_bed.name}'");
            Debug.Log($"{name} Bed Foot = {_bedFootTf.position}, Bed Lie = {_bedLieTf.position}");
        }

        if (!agent.enabled) agent.enabled = true;
        if (!agent.isOnNavMesh && TryGetNavmeshPoint(transform.position, out Vector3 navPos))
            agent.Warp(navPos);

        if (!AgentReady()) return false;

        agent.isStopped = false;
        agent.autoBraking = true;
        agent.stoppingDistance = Mathf.Max(0.05f, arriveDistance);
        agent.ResetPath();
        agent.SetDestination(_preLieNavPos);

        _liePhase = LiePhase.ApproachingFront;
        return true;
    }

    private void TickApproachBedFront()
    {
        agent.isStopped = false;
        agent.autoBraking = true;
        agent.stoppingDistance = Mathf.Max(0.05f, arriveDistance);

        if (!agent.pathPending && (!agent.hasPath || Vector3.Distance(agent.destination, _preLieNavPos) > 0.15f))
            agent.SetDestination(_preLieNavPos);

        float planarDist = new Vector2(transform.position.x - _preLieNavPos.x, transform.position.z - _preLieNavPos.z).magnitude;
        float threshold = Mathf.Max(preLieArriveDistance, agent.stoppingDistance + 0.05f);

        bool navArrived =
            !agent.pathPending &&
            agent.hasPath &&
            !float.IsInfinity(agent.remainingDistance) &&
            agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, 0.05f) + 0.02f;

        if (planarDist <= threshold || navArrived)
        {
            if (bedDebugLogs)
                Debug.Log($"{name} Bed: Arrived at bed foot point. Aligning.");

            agent.isStopped = true;
            agent.ResetPath();
            _liePhase = LiePhase.Aligning;
        }
    }

    private void TickAlignToBed(float dt, Vector3 bedFacing)
    {
        agent.isStopped = true;

        Quaternion targetRot = Quaternion.LookRotation(bedFacing, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, dt * (turnSmoothing * 1.5f));

        float yawDelta = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetRot.eulerAngles.y));

        if (bedDebugLogs) Debug.Log($"{name} Bed: Aligning yawDelta={yawDelta:F1}");

        if (yawDelta <= Mathf.Max(0.5f, bedAlignYawToleranceDeg))
        {
            if (bedDebugLogs) Debug.Log($"{name} Bed: Aligned at foot. LieDown.");
            BeginLieDown();
        }
    }

    private void BeginLieDown()
    {
        CacheAnimationRootLocalXZ();
        DetachAgentForAnimation();

        Vector3 facing = GetBedFacing();

        // Stand on the foot marker before animation
        if (_bedFootTf != null)
            transform.position = _bedFootTf.position;

        transform.rotation = Quaternion.LookRotation(facing, Vector3.up);

        GroundTransformToNavmesh(transform.position, bodyRecoverySampleRadius);

        if (animationController != null)
        {
            animationController.SetFloat(paramMovingX, 0f);
            animationController.SetFloat(paramMovingY, 0f);
            animationController.SetFloat(paramBlend, 0f);

            ResetAllAnimatorTriggers();

            if (useLieDownTriggerParam && !string.IsNullOrWhiteSpace(lieDownTriggerParam))
                animationController.SetTrigger(lieDownTriggerParam);
            else
                animationController.CrossFadeInFixedTime(lieDownStateName, lieCrossfade, lieAnimLayer, 0f);

            if (bedDebugLogs)
                Debug.Log($"{name} Bed: Starting LieDown from foot marker.");
        }

        _lieTriggerT = 0f;
        _lieTriggerSent = true;
        _liePhase = LiePhase.LieDownPlaying;
    }

    private void TickLieDownPlaying(float dt)
    {
        if (animationController == null) return;

        _lieTriggerT += dt;
        AnimatorStateInfo info = animationController.GetCurrentAnimatorStateInfo(lieAnimLayer);

        bool inLieDown = !string.IsNullOrWhiteSpace(lieDownStateName) && info.IsName(lieDownStateName);
        bool inLieIdle = !string.IsNullOrWhiteSpace(lieIdleStateName) && info.IsName(lieIdleStateName);

        if (!_snappedToBedLyingPose)
        {
            bool shouldSnap =
                inLieIdle ||
                (inLieDown && !animationController.IsInTransition(lieAnimLayer) && info.normalizedTime >= 0.80f) ||
                (!_forcedLiePoseByTimer && _lieTriggerT >= lieDownSnapDelay);

            if (shouldSnap)
            {
                SnapToBedLyingPoseNow();

                if (_lieTriggerT >= lieDownSnapDelay)
                    _forcedLiePoseByTimer = true;
            }
        }

        if (inLieIdle)
        {
            _liePhase = LiePhase.LyingIdle;
            _lyingT = 0f;

            if (bedDebugLogs) Debug.Log($"{name} Bed: LyingIdle.");
            return;
        }

        if (_lieTriggerSent && _lieTriggerT >= lieTriggerFallbackDelay)
        {
            if (!inLieDown && !inLieIdle && !string.IsNullOrWhiteSpace(lieDownStateName))
            {
                if (bedDebugLogs) Debug.Log($"{name} Bed: Fallback Play('{lieDownStateName}').");
                animationController.Play(lieDownStateName, lieAnimLayer, 0f);
            }

            _lieTriggerSent = false;
        }

        if (_snappedToBedLyingPose && _lieTriggerT >= Mathf.Max(lieDownSnapDelay, 0.9f))
        {
            _liePhase = LiePhase.LyingIdle;
            _lyingT = 0f;

            if (bedDebugLogs)
                Debug.LogWarning($"{name} Bed: Forced LyingIdle after timed snap.");
        }
    }

    private void TickLyingIdle(float dt, Vector3 bedFacing)
    {
        ForceIdlePose();
        transform.rotation = GetPlanarLookRotation(bedFacing);

        if (snapToBedWhenLying)
        {
            if (_bedLieTf != null)
                transform.position = _bedLieTf.position;
            else if (_bedTf != null)
                transform.position = _bedLieWorldPos;
        }

        if (autoWakeAfterSeconds > 0f)
        {
            _lyingT += dt;
            if (_lyingT >= autoWakeAfterSeconds)
                BeginWakeUp();
        }
    }

    private void BeginWakeUp()
    {
        if (_liePhase == LiePhase.WakeUpPlaying) return;

        transform.rotation = GetPlanarLookRotation(GetBedFacing());

        if (snapToBedWhenLying)
        {
            if (_bedLieTf != null)
                transform.position = _bedLieTf.position;
            else
                transform.position = _bedLieWorldPos;
        }

        DetachAgentForAnimation();

        if (animationController == null)
        {
            FinishWakeUpToPatrol();
            return;
        }

        ResetAllAnimatorTriggers();
        animationController.speed = 1f;

        if (useWakeUpTriggerParam && !string.IsNullOrWhiteSpace(wakeUpTriggerParam))
            animationController.SetTrigger(wakeUpTriggerParam);
        else if (!string.IsNullOrWhiteSpace(wakeUpStateName))
            animationController.CrossFadeInFixedTime(wakeUpStateName, lieCrossfade, lieAnimLayer, 0f);

        _wakeTriggerT = 0f;
        _wakeTriggerSent = true;
        _liePhase = LiePhase.WakeUpPlaying;
    }

    private void TickWakeUpPlaying(float dt)
    {
        if (animationController == null)
        {
            FinishWakeUpToPatrol();
            return;
        }

        _wakeTriggerT += dt;

        AnimatorStateInfo info = animationController.GetCurrentAnimatorStateInfo(lieAnimLayer);

        bool inWakeUp = !string.IsNullOrWhiteSpace(wakeUpStateName) && info.IsName(wakeUpStateName);
        bool inLieIdle = !string.IsNullOrWhiteSpace(lieIdleStateName) && info.IsName(lieIdleStateName);

        if (inWakeUp && !animationController.IsInTransition(lieAnimLayer) && info.normalizedTime >= 1f)
        {
            FinishWakeUpToPatrol();
            return;
        }

        if (_wakeTriggerSent && _wakeTriggerT >= wakeTriggerFallbackDelay)
        {
            if (!inWakeUp && !string.IsNullOrWhiteSpace(wakeUpStateName))
            {
                if (bedDebugLogs) Debug.Log($"{name} Bed: Fallback Play('{wakeUpStateName}').");
                animationController.Play(wakeUpStateName, lieAnimLayer, 0f);
            }

            _wakeTriggerSent = false;
        }

        if (_wakeTriggerT >= Mathf.Max(0.25f, triggerMaxDuration))
        {
            if (bedDebugLogs) Debug.LogWarning($"{name} Bed: WakeUp timeout fallback.");
            FinishWakeUpToPatrol();
            return;
        }

        if (_wakeTriggerT > 0.20f && !animationController.IsInTransition(lieAnimLayer) && !inWakeUp && !inLieIdle)
        {
            if (bedDebugLogs) Debug.Log($"{name} Bed: WakeUp exited state machine branch, forcing finish.");
            FinishWakeUpToPatrol();
        }
    }

    private void FinishWakeUpToPatrol()
    {
        Vector3 preferred = _preLieNavPos != Vector3.zero
            ? _preLieNavPos
            : (_bedNavPos != Vector3.zero ? _bedNavPos : transform.position);

        ReleaseBedIfAny();

        _hasCommand = false;
        _commandGoal = NPCState.Patrolling;
        _liePhase = LiePhase.None;
        _state = NPCState.Patrolling;

        StartCoroutine(FinishWakeUpGroundingRoutine(preferred));
    }

    private IEnumerator FinishWakeUpGroundingRoutine(Vector3 preferred)
    {
        RestoreStandingBodyAt(preferred, bodyRecoverySampleRadius);
        yield return null;

        RestoreStandingBodyAt(preferred, bodyRecoverySampleRadius);
        yield return null;

        RestoreStandingBodyAt(transform.position, bodyRecoverySampleRadius);

        RestoreAnimationRootLocalXZ();

        if (animationController != null)
        {
            animationController.speed = 1f;
            ForceReturnToLocomotion();
            animationController.Update(0f);
            ForceIdlePose();
        }

        _hasCommand = false;
        _commandGoal = NPCState.Patrolling;
        _liePhase = LiePhase.None;
        _state = NPCState.Patrolling;

        if (!ExecutePendingPostStandAction())
            EnterState(NPCState.Patrolling);
    }

    private void FailLyingAndReturnToPatrol()
    {
        Vector3 preferred = _preLieNavPos != Vector3.zero
            ? _preLieNavPos
            : (_bedNavPos != Vector3.zero ? _bedNavPos : transform.position);

        ReleaseBedIfAny();

        _hasCommand = false;
        _commandGoal = NPCState.Patrolling;
        _liePhase = LiePhase.None;
        _state = NPCState.Patrolling;

        RestoreStandingBodyAt(preferred, bodyRecoverySampleRadius);
        RestoreAnimationRootLocalXZ();

        if (animationController != null)
            ForceReturnToLocomotion();

        ReattachAgentToNavmeshAtCurrentXZ();
        EnterState(NPCState.Patrolling);
    }

    private void ReleaseBedIfAny()
    {
        _bed?.Release(this);
        _bed = null;
        _bedTf = null;
        _bedFootTf = null;
        _bedLieTf = null;
        _bedNavPos = Vector3.zero;
        _preLieNavPos = Vector3.zero;
        _bedLieWorldPos = Vector3.zero;
        _snappedToBedLyingPose = false;
        _forcedLiePoseByTimer = false;
    }

    void TickPatrolling(float dt, float speedMultiplier)
    {
        if (!AgentReady()) return;
        if (currentTarget != null && CanSeeTarget(currentTarget)) return;

        _patrolChangeTimer -= dt;
        if (_patrolChangeTimer <= 0f)
        {
            _hasPatrolDestination = false;
            _patrolChangeTimer = UnityEngine.Random.Range(patrolChangeDirInterval.x, patrolChangeDirInterval.y);
        }

        if (_hasPatrolDestination && HasArrived(agent, arriveDistance))
        {
            if (_patrolArriveTimer <= 0f)
                _patrolArriveTimer = UnityEngine.Random.Range(patrolArrivePause.x, patrolArrivePause.y);

            agent.isStopped = true;
            _patrolArriveTimer -= dt;

            if (_patrolArriveTimer <= 0f)
            {
                agent.isStopped = false;
                _hasPatrolDestination = false;
            }
            return;
        }

        if (!_hasPatrolDestination && TryPickPatrolPoint(_spawnPoint, activeRadius, out _patrolDestination))
        {
            agent.isStopped = false;
            agent.speed = moveSpeed * Mathf.Max(0.01f, speedMultiplier);
            agent.ResetPath();
            agent.SetDestination(_patrolDestination);
            _hasPatrolDestination = true;
        }
    }

    void TickAlerted(float dt)
    {
        if (AgentReady()) agent.isStopped = true;

        if (currentTarget != null)
        {
            Vector3 to = currentTarget.position - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(to.normalized, Vector3.up), dt * (turnSmoothing * 1.25f));
            }
        }

        if (_stateTimer >= alertedDuration)
        {
            bool inRange = currentTarget != null &&
                (Vector3.Distance(_spawnPoint, currentTarget.position) <= activeRadius || allowApproachOutsideActiveRadius);

            EnterState(inRange ? NPCState.Approaching : NPCState.Seeking);
        }
    }

    private bool TryRepath(ref float timer, Vector3 destination)
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return false;

        timer = Mathf.Max(0.05f, approachRepathInterval);
        float thresholdSqr = approachTargetMoveThreshold * approachTargetMoveThreshold;

        if (!_hasLastApproachDest ||
            !agent.hasPath ||
            agent.pathStatus != NavMeshPathStatus.PathComplete ||
            (destination - _lastApproachDest).sqrMagnitude >= thresholdSqr)
        {
            _lastApproachDest = destination;
            _hasLastApproachDest = true;
            agent.isStopped = false;
            agent.SetDestination(destination);
        }

        return true;
    }

    void TickApproaching(float dt)
    {
        if (!AgentReady()) return;
        ApplyStoppingDistanceForCurrentMode();

        if (!_hasCommand && currentTarget != null &&
            Vector3.Distance(_spawnPoint, currentTarget.position) > activeRadius &&
            !allowApproachOutsideActiveRadius)
        {
            EnterState(NPCState.Seeking);
            return;
        }

        if (currentTarget == null &&
            !(_hasCommand && (_commandGoal == NPCState.Sitting || _commandGoal == NPCState.Lying)))
        {
            _hasCommand = false;
            _commandGoal = NPCState.Patrolling;
            EnterState(NPCState.Patrolling);
            return;
        }

        if (_hasCommand && currentTarget != null)
        {
            float distToTarget = Vector3.Distance(transform.position, currentTarget.position);

            if (_commandGoal == NPCState.Talk && distToTarget <= talkRange)
            {
                EnterState(NPCState.Talk);
                return;
            }

            if (_commandGoal == NPCState.Attacking && distToTarget <= talkRange)
            {
                EnterState(NPCState.Attacking);
                return;
            }

            TryRepath(ref _approachRepathTimer, currentTarget.position);
            return;
        }

        if (currentTarget == null)
        {
            EnterState(NPCState.Patrolling);
            return;
        }

        if (Vector3.Distance(transform.position, currentTarget.position) <= attackRange)
        {
            EnterState(NPCState.Attacking);
            return;
        }

        TryRepath(ref _approachRepathTimer, currentTarget.position);
    }

    void TickAttacking(float dt)
    {
        if (_hasCommand && _commandGoal == NPCState.Attacking)
        {
            if (AgentReady())
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            ForceIdlePose();

            if (currentTarget != null)
                FaceTowards(currentTarget.position, dt, turnSmoothing * 1.25f);

            return;
        }

        if (currentTarget == null) { EnterState(NPCState.Seeking); return; }
        if (Vector3.Distance(_spawnPoint, currentTarget.position) > activeRadius && !allowApproachOutsideActiveRadius)
        {
            EnterState(NPCState.Seeking);
            return;
        }
        if (Vector3.Distance(transform.position, currentTarget.position) > attackRange * 1.15f)
        {
            EnterState(NPCState.Approaching);
            return;
        }

        Vector3 to = currentTarget.position - transform.position;
        to.y = 0f;
        if (to.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(to.normalized, Vector3.up), dt * (turnSmoothing * 1.25f));
        }
    }

    void TickSeeking(float dt)
    {
        _seekTimer += dt;
        if (_seekTimer >= Mathf.Max(0.01f, seekGiveUpSeconds))
        {
            EnterState(NPCState.Patrolling);
            return;
        }

        if (currentTarget != null && CanSeeTarget(currentTarget) &&
            (Vector3.Distance(_spawnPoint, currentTarget.position) <= activeRadius || allowApproachOutsideActiveRadius))
        {
            EnterState(NPCState.Alerted);
            return;
        }

        TickPatrolling(dt, seekingSpeedMultiplier);
    }

    void TickTalk(float dt)
    {
        if (AgentReady()) agent.isStopped = true;
        ForceIdlePose();

        if (currentTarget == null)
        {
            EndConversationForBoth();
            _hasCommand = false;
            _commandGoal = NPCState.Patrolling;
            EnterState(NPCState.Patrolling);
            return;
        }

        FaceTowards(currentTarget.position, dt, turnSmoothing * 1.5f);

        if (_talkTargetController == null)
            _talkTargetController = currentTarget.GetComponentInParent<NPCController>();

        if (_talkTargetController != null && !_registeredAsConversationSpeaker)
        {
            currentTarget = _talkTargetController.transform;
            _talkTargetController.currentTarget = transform;
            _talkTargetController._talkTargetController = this;
            _talkTargetController.BeginConversationAsTarget(this);
            _registeredAsConversationSpeaker = true;
        }
    }

    public void BeginConversationAsTarget(NPCController speaker)
    {
        if (speaker == null) return;

        bool wasEmpty = _conversationSpeakers.Count == 0;
        _conversationSpeakers.Add(speaker);
        _talkTargetController = speaker;
        currentTarget = speaker.transform;

        if (wasEmpty)
        {
            useStateMachine = false;
            if (AgentReady())
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
            ForceIdlePose();
        }

        _isConversationLocked = _conversationSpeakers.Count > 0;
        _primaryConversationSpeaker = GetNearestConversationSpeaker();
    }

    public void EndConversationAsTarget(NPCController speaker)
    {
        if (speaker != null) _conversationSpeakers.Remove(speaker);

        _primaryConversationSpeaker = GetNearestConversationSpeaker();
        _isConversationLocked = _conversationSpeakers.Count > 0;

        if (!_isConversationLocked)
        {
            useStateMachine = true;
            if (AgentReady())
            {
                agent.isStopped = false;
                agent.ResetPath();
            }

            if (_state != NPCState.Sitting && _state != NPCState.Lying && animationController != null)
                ForceReturnToLocomotion();

            if (_state == NPCState.Talk)
                EnterState(NPCState.Patrolling);
        }
    }

    bool CanSeeTarget(Transform t)
    {
        if (t == null) return false;
        Vector3 to = t.position - transform.position;
        if (to.magnitude > visionRange) return false;

        Vector3 flatTo = new Vector3(to.x, 0f, to.z);
        Vector3 fwd = new Vector3(transform.forward.x, 0f, transform.forward.z);

        if (flatTo.sqrMagnitude > 0.0001f &&
            fwd.sqrMagnitude > 0.0001f &&
            Vector3.Angle(fwd.normalized, flatTo.normalized) > visionFOV * 0.5f)
        {
            return false;
        }

        if (requireLineOfSight)
        {
            Vector3 origin = transform.position + Vector3.up * 1.6f;
            Vector3 dir = (t.position + Vector3.up * 1.2f) - origin;
            float d = dir.magnitude;
            if (d > 0.001f && Physics.Raycast(origin, dir / d, out _, d, losBlockers, QueryTriggerInteraction.Ignore))
                return false;
        }

        return true;
    }

    bool TryPickPatrolPoint(Vector3 center, float radius, out Vector3 result)
    {
        result = center;
        for (int i = 0; i < Mathf.Max(1, patrolPointTries); i++)
        {
            Vector2 r = UnityEngine.Random.insideUnitCircle * radius;
            Vector3 candidate = center + new Vector3(r.x, 0f, r.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, patrolSampleRadius, NavMesh.AllAreas) &&
                Vector3.Distance(center, hit.position) <= radius + 0.25f)
            {
                result = hit.position;
                return true;
            }
        }
        return false;
    }

    // ── SITTING ───────────────────────────────────────────────────────────────

    private void EnterSitting()
    {
        _sitPhase = SitPhase.SearchingSeat;
        _seatRescanT = 0f;
        _seatedT = 0f;
        ReleaseSeatIfAny();

        if (agent != null)
        {
            if (!agent.enabled) agent.enabled = true;
            if (!agent.isOnNavMesh && TryGetNavmeshPoint(transform.position, out Vector3 navPos))
                agent.Warp(navPos);

            agent.autoBraking = true;
            agent.stoppingDistance = Mathf.Max(0.05f, arriveDistance);

            if (AgentReady())
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
        }
    }

    private void TickSitting(float dt)
    {
        _hasCommand = true;
        _commandGoal = NPCState.Sitting;

        switch (_sitPhase)
        {
            case SitPhase.SearchingSeat:
                TickSearchingSeat(dt);
                break;

            case SitPhase.ApproachingFront:
                if (AgentReady()) TickApproachFront();
                break;

            case SitPhase.Aligning:
                if (_seatTf == null) { FailSittingAndReturnToPatrol(); return; }
                if (AgentReady()) TickAlign(dt, GetSeatFacing());
                break;

            case SitPhase.Backstepping:
                if (_seatTf == null) { FailSittingAndReturnToPatrol(); return; }
                if (AgentReady()) TickBackstep(dt, _seatNavPos, GetSeatFacing());
                break;

            case SitPhase.SitDownPlaying:
                TickSitDownPlaying(dt);
                break;

            case SitPhase.SittingIdle:
                if (_seatTf == null) { FailSittingAndReturnToPatrol(); return; }
                TickSittingIdle(dt, GetSeatFacing());
                break;

            case SitPhase.StandUpPlaying:
                TickStandUpPlaying(dt);
                break;
        }
    }

    private Vector3 GetSeatFacing()
    {
        if (_seatTf == null) return transform.forward;
        Vector3 f = new Vector3(_seatTf.forward.x, 0f, _seatTf.forward.z);
        return f.sqrMagnitude < 0.0001f ? transform.forward : f.normalized;
    }

    private void TickSearchingSeat(float dt)
    {
        _seatRescanT -= dt;
        if (_seatRescanT <= 0f)
        {
            _seatRescanT = Mathf.Max(0.05f, seatRescanInterval);
            if (TryAcquireSeat()) return;
        }

        if (AgentReady())
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        ForceIdlePose();
    }

    private bool TryAcquireSeat()
    {
        float radius = (seatSearchRadius > 0.01f) ? seatSearchRadius : Mathf.Max(0.1f, activeRadius);
        Seat[] seats = FindObjectsByType<Seat>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (seats == null || seats.Length == 0)
        {
            if (seatDebugLogs) Debug.Log($"{name} Sit: No Seat components found.");
            return false;
        }

        Seat best = null;
        float bestSqr = float.PositiveInfinity;

        foreach (var s in seats)
        {
            if (!s || s.seatTransform == null || !s.IsValid || s.IsOccupied) continue;

            bool matchesMask =
                ((1 << s.gameObject.layer) & seatLayerMask.value) != 0 ||
                ((1 << s.seatTransform.gameObject.layer) & seatLayerMask.value) != 0;
            if (!matchesMask) continue;

            Vector3 seatPos = s.seatTransform.position;
            Vector3 d = seatPos - _spawnPoint;
            d.y = 0f;
            if (d.sqrMagnitude > radius * radius) continue;

            if (!NavMesh.SamplePosition(seatPos, out _, activeRadius, NavMesh.AllAreas))
            {
                if (seatDebugLogs) Debug.Log($"{name} Sit: REJECT '{s.name}' navmesh_sample_failed");
                continue;
            }

            float sqr = (transform.position - seatPos).sqrMagnitude;
            if (sqr < bestSqr)
            {
                best = s;
                bestSqr = sqr;
            }
        }

        if (best == null)
        {
            Debug.LogWarning($"{name} Sit: No valid/free seat found within range.");
            return false;
        }

        if (!best.TryOccupy(this))
        {
            Debug.LogWarning($"{name} Sit: Seat '{best.name}' refused occupancy.");
            return false;
        }

        _seat = best;
        _seatTf = best.seatTransform;

        Vector3 seatForward = new Vector3(_seatTf.forward.x, 0f, _seatTf.forward.z);
        if (seatForward.sqrMagnitude < 0.0001f) seatForward = transform.forward;
        seatForward.Normalize();

        Vector3 seatPos2 = _seatTf.position;
        NavMesh.SamplePosition(seatPos2, out NavMeshHit seatHit, activeRadius, NavMesh.AllAreas);
        _seatNavPos = seatHit.position;

        Vector3 preSitPoint = seatPos2 + seatForward * Mathf.Max(0f, preSitForwardOffset);
        _preSitNavPos = NavMesh.SamplePosition(preSitPoint, out NavMeshHit preHit, activeRadius, NavMesh.AllAreas)
            ? preHit.position
            : _seatNavPos;

        if (seatDebugLogs) Debug.Log($"{name} Sit: ACQUIRED '{_seat.name}'");

        if (!agent.enabled) agent.enabled = true;
        if (!agent.isOnNavMesh && TryGetNavmeshPoint(transform.position, out Vector3 navPos)) agent.Warp(navPos);
        if (!AgentReady()) return false;

        agent.isStopped = false;
        agent.autoBraking = true;
        agent.stoppingDistance = Mathf.Max(0.05f, arriveDistance);
        agent.ResetPath();
        agent.SetDestination(_preSitNavPos);
        _sitPhase = SitPhase.ApproachingFront;
        return true;
    }

    private void TickApproachFront()
    {
        agent.isStopped = false;
        agent.autoBraking = true;
        agent.stoppingDistance = Mathf.Max(0.05f, arriveDistance);

        if (!agent.pathPending && (!agent.hasPath || Vector3.Distance(agent.destination, _preSitNavPos) > 0.15f))
            agent.SetDestination(_preSitNavPos);

        float planarDist = new Vector2(transform.position.x - _preSitNavPos.x, transform.position.z - _preSitNavPos.z).magnitude;
        float threshold = Mathf.Max(preSitArriveDistance, agent.stoppingDistance + 0.05f);

        bool navArrived =
            !agent.pathPending &&
            agent.hasPath &&
            !float.IsInfinity(agent.remainingDistance) &&
            agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, 0.05f) + 0.02f;

        if (planarDist <= threshold || navArrived)
        {
            if (seatDebugLogs) Debug.Log($"{name} Sit: Arrived at pre-sit point. Aligning.");
            agent.isStopped = true;
            agent.ResetPath();
            _sitPhase = SitPhase.Aligning;
        }
    }

    private void TickAlign(float dt, Vector3 seatFacing)
    {
        agent.isStopped = true;
        Quaternion targetRot = Quaternion.LookRotation(seatFacing, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, dt * (turnSmoothing * 1.5f));
        float yawDelta = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetRot.eulerAngles.y));

        if (seatDebugLogs) Debug.Log($"{name} Sit: Aligning yawDelta={yawDelta:F1}");

        if (yawDelta <= Mathf.Max(0.5f, alignYawToleranceDeg))
        {
            if (seatDebugLogs) Debug.Log($"{name} Sit: Aligned. Backstepping.");
            _sitPhase = SitPhase.Backstepping;
        }
    }

    private void TickBackstep(float dt, Vector3 seatNavPos, Vector3 seatFacing)
    {
        agent.isStopped = true;
        transform.position = Vector3.MoveTowards(transform.position, seatNavPos, Mathf.Max(0.01f, backstepSpeed) * dt);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(seatFacing, Vector3.up), dt * (turnSmoothing * 2.0f));

        float dist = new Vector2(transform.position.x - seatNavPos.x, transform.position.z - seatNavPos.z).magnitude;
        if (seatDebugLogs) Debug.Log($"{name} Sit: Backstepping dist={dist:F2}");

        if (dist <= 0.12f)
        {
            if (seatDebugLogs) Debug.Log($"{name} Sit: Seated. SitDown.");
            BeginSitDown();
        }
    }

    private void BeginSitDown()
    {
        CacheAnimationRootLocalXZ();
        DetachAgentForAnimation();

        GroundTransformToNavmesh(_seatNavPos != Vector3.zero ? _seatNavPos : transform.position, bodyRecoverySampleRadius);

        if (snapToSeatWhenSeated && _seatTf != null)
        {
            Vector3 seatP = _seatTf.position;
            transform.position = new Vector3(seatP.x, transform.position.y, seatP.z);
        }

        if (_seatTf != null)
            transform.rotation = Quaternion.LookRotation(GetSeatFacing(), Vector3.up);

        if (animationController != null)
        {
            animationController.SetFloat(paramMovingX, 0f);
            animationController.SetFloat(paramMovingY, 0f);
            animationController.SetFloat(paramBlend, 0f);
            ResetAllAnimatorTriggers();

            if (useSitTriggerParam && !string.IsNullOrWhiteSpace(sitTriggerParam))
                animationController.SetTrigger(sitTriggerParam);
            else
                animationController.CrossFadeInFixedTime(sitDownStateName, sitCrossfade, sitAnimLayer, 0f);

            if (seatDebugLogs) Debug.Log($"{name} Sit: Start SitDown.");
        }

        _sitTriggerSent = true;
        _sitTriggerT = 0f;
        _sitPhase = SitPhase.SitDownPlaying;
    }

    private void TickSitDownPlaying(float dt)
    {
        if (animationController == null) return;

        _sitTriggerT += dt;
        AnimatorStateInfo info = animationController.GetCurrentAnimatorStateInfo(sitAnimLayer);

        if (!string.IsNullOrWhiteSpace(sitIdleStateName) && info.IsName(sitIdleStateName))
        {
            _sitPhase = SitPhase.SittingIdle;
            _seatedT = 0f;
            if (_seatTf != null) transform.rotation = Quaternion.LookRotation(GetSeatFacing(), Vector3.up);
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
                animationController.Play(sitDownStateName, sitAnimLayer, 0f);
            }

            _sitTriggerSent = false;
        }
    }

    private void TickSittingIdle(float dt, Vector3 seatForward)
    {
        ForceIdlePose();
        transform.rotation = Quaternion.LookRotation(seatForward, Vector3.up);

        if (autoStandAfterSeconds > 0f)
        {
            _seatedT += dt;
            if (_seatedT >= autoStandAfterSeconds)
                BeginStandUp();
        }
    }

    private void BeginStandUp()
    {
        if (_sitPhase == SitPhase.StandUpPlaying) return;

        DetachAgentForAnimation();

        if (animationController == null)
        {
            FinishStandUpToPatrol();
            return;
        }

        ResetAllAnimatorTriggers();
        animationController.speed = 1f;

        if (useStandUpTriggerParam && !string.IsNullOrWhiteSpace(standUpTriggerParam))
            animationController.SetTrigger(standUpTriggerParam);
        else if (!string.IsNullOrWhiteSpace(standUpStateName))
            animationController.CrossFadeInFixedTime(standUpStateName, sitCrossfade, sitAnimLayer, 0f);

        _sitPhase = SitPhase.StandUpPlaying;
    }

    private void TickStandUpPlaying(float dt)
    {
        if (animationController == null)
        {
            FinishStandUpToPatrol();
            return;
        }

        AnimatorStateInfo info = animationController.GetCurrentAnimatorStateInfo(sitAnimLayer);
        if (!string.IsNullOrWhiteSpace(standUpStateName) &&
            info.IsName(standUpStateName) &&
            !animationController.IsInTransition(sitAnimLayer) &&
            info.normalizedTime >= 1f)
        {
            FinishStandUpToPatrol();
        }
    }

    private void FinishStandUpToPatrol()
    {
        if (animationController != null)
        {
            animationController.speed = 1f;
            ForceReturnToLocomotion();
            animationController.Update(0f);
        }

        ReleaseSeatIfAny();
        _hasCommand = false;
        _commandGoal = NPCState.Patrolling;
        _sitPhase = SitPhase.None;

        StartCoroutine(FinishStandUpGroundingRoutine());
    }

    private IEnumerator FinishStandUpGroundingRoutine()
    {
        ReattachAgentToNavmeshAtCurrentXZ();
        yield return null;
        ReattachAgentToNavmeshAtCurrentXZ();

        RestoreAnimationRootLocalXZ();

        if (animationController != null)
        {
            animationController.Update(0f);
            ForceIdlePose();
        }

        _hasCommand = false;
        _commandGoal = NPCState.Patrolling;
        _sitPhase = SitPhase.None;
        _state = NPCState.Patrolling;

        if (!ExecutePendingPostStandAction())
            EnterState(NPCState.Patrolling);
    }

    private void FailSittingAndReturnToPatrol()
    {
        ReleaseSeatIfAny();
        _hasCommand = false;
        _commandGoal = NPCState.Patrolling;
        _sitPhase = SitPhase.None;

        RestoreAnimationRootLocalXZ();

        if (animationController != null)
            ForceReturnToLocomotion();

        ReattachAgentToNavmeshAtCurrentXZ();
        EnterState(NPCState.Patrolling);
    }

    private void ReleaseSeatIfAny()
    {
        _seat?.Release(this);
        _seat = null;
        _seatTf = null;
        _seatNavPos = Vector3.zero;
        _preSitNavPos = Vector3.zero;
    }

    void PreCollisionRenegotiate()
    {
        if (!IsNavDriven() || !AgentReady()) return;
        if (useStateMachine &&
            (_state == NPCState.Alerted || _state == NPCState.Attacking || _state == NPCState.Talk || _state == NPCState.Sitting || _state == NPCState.Lying))
            return;
        if (agent.pathPending || !agent.hasPath) return;

        if (_renegotiateCooldownT > 0f)
        {
            _renegotiateCooldownT -= Time.deltaTime;
            return;
        }

        _renegotiateT += Time.deltaTime;
        if (_renegotiateT < renegotiateCheckInterval) return;
        _renegotiateT = 0f;

        Vector3 v = agent.velocity;
        v.y = 0f;
        if (v.sqrMagnitude < 0.0004f) return;

        Vector3 fwd = v.normalized;
        Collider[] hits = Physics.OverlapSphere(transform.position, personalSpaceRadius, npcLayerMask, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0) return;

        NPCController closest = null;
        float bestDist = float.PositiveInfinity;

        foreach (var c in hits)
        {
            if (!c) continue;
            var other = c.GetComponentInParent<NPCController>();
            if (!other || other == this || !other.agent || !other.agent.isOnNavMesh) continue;

            Vector3 toOther = other.transform.position - transform.position;
            toOther.y = 0f;

            float d = toOther.magnitude;
            if (d < 0.0001f || Vector3.Dot(fwd, toOther / d) < inFrontDotThreshold) continue;

            if (d < bestDist)
            {
                bestDist = d;
                closest = other;
            }
        }

        if (!closest) return;

        if (_sidestepCoroutine != null) StopCoroutine(_sidestepCoroutine);

        _resumeDestination = agent.destination;
        _hasResumeDestination = true;
        _sidestepCoroutine = StartCoroutine(SidestepAvoidanceCoroutine(closest));
        _renegotiateCooldownT = renegotiateCooldown;
    }

    IEnumerator SidestepAvoidanceCoroutine(NPCController other)
    {
        if (!AgentReady()) yield break;

        int sign = ((gameObject.GetInstanceID() & 1) == 0) ? 1 : -1;

        Vector3 v = agent.velocity;
        v.y = 0f;
        Vector3 fwd = (v.sqrMagnitude > 0.001f) ? v.normalized : transform.forward;
        Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;

        if (!TrySampleNavPoint(transform.position + right * sidestepDistance * sign, out Vector3 chosen) &&
            !TrySampleNavPoint(transform.position - right * sidestepDistance * sign, out chosen))
            yield break;

        int prevPriority = agent.avoidancePriority;
        agent.avoidancePriority = 0;
        agent.isStopped = false;
        agent.ResetPath();
        agent.SetDestination(chosen);

        float t = 0f;
        while (t < sidestepDuration)
        {
            if (!IsNavDriven()) break;
            while (isPaused) yield return null;
            t += Time.deltaTime;
            yield return null;
        }

        agent.avoidancePriority = prevPriority;

        if (IsNavDriven() && _hasResumeDestination && AgentReady())
        {
            agent.isStopped = false;
            agent.ResetPath();
            agent.SetDestination(_resumeDestination);
        }

        _sidestepCoroutine = null;
    }

    bool TrySampleNavPoint(Vector3 worldPos, out Vector3 snapped)
    {
        snapped = worldPos;
        if (NavMesh.SamplePosition(worldPos, out NavMeshHit hit, 0.75f, NavMesh.AllAreas))
        {
            snapped = hit.position;
            return true;
        }
        return false;
    }

    public void PlaySelectedTrigger()
    {
        if (triggerNames == null || triggerNames.Count == 0)
        {
            Debug.LogWarning($"{name}: No triggerNames defined.");
            return;
        }

        selectedTriggerIndex = Mathf.Clamp(selectedTriggerIndex, 0, triggerNames.Count - 1);
        PlayTrigger(triggerNames[selectedTriggerIndex]);
    }

    public void PlayTrigger(string triggerParam)
    {
        if (string.IsNullOrWhiteSpace(triggerParam))
        {
            Debug.LogWarning($"{name}: Trigger param was empty.");
            return;
        }

        if (animationController == null)
        {
            Debug.LogWarning($"{name}: No Animator assigned.");
            return;
        }

        void Execute()
        {
            if (_triggerCoroutine != null)
            {
                StopCoroutine(_triggerCoroutine);
                _triggerCoroutine = null;
            }

            EndConversationForBoth();
            _triggerCoroutine = StartCoroutine(TriggerRoutine(triggerParam));
        }

        if (TryQueueActionAfterStand(Execute)) return;
        Execute();
    }

    private bool IsRecoveringBodyState()
    {
        bool sittingBusy = _state == NPCState.Sitting && _sitPhase != SitPhase.None;
        bool lyingBusy = _state == NPCState.Lying && _liePhase != LiePhase.None;
        return sittingBusy || lyingBusy;
    }

    private bool TryQueueActionAfterStand(Action action)
    {
        if (IsInConversation()) EndConversationForBoth();
        if (!IsRecoveringBodyState()) return false;

        _pendingPostStandAction = action;
        _executePendingActionAfterStand = true;

        if (_state == NPCState.Sitting)
        {
            if (_sitPhase != SitPhase.StandUpPlaying)
                BeginStandUp();
        }
        else if (_state == NPCState.Lying)
        {
            if (_liePhase != LiePhase.WakeUpPlaying)
                BeginWakeUp();
        }

        return true;
    }

    private bool ExecutePendingPostStandAction()
    {
        if (!_executePendingActionAfterStand) return false;

        Action action = _pendingPostStandAction;
        _executePendingActionAfterStand = false;
        _pendingPostStandAction = null;
        action?.Invoke();
        return true;
    }

    public void PauseTriggeredAnimation()
    {
        if (animationController != null)
            animationController.speed = 0f;
    }

    public void StopTriggeredAnimation()
    {
        if (animationController == null) return;

        if (_triggerCoroutine != null)
        {
            StopCoroutine(_triggerCoroutine);
            _triggerCoroutine = null;
        }

        _isPlayingTriggeredAnimation = false;
        animationController.speed = 1f;
        ResetAllAnimatorTriggers();

        isPaused = _triggerPrevPaused;
        if (_triggerAgentWasValid && AgentReady())
            agent.isStopped = _triggerWasStoppedBefore;

        if (useStateMachine && !isPaused)
            ForceReturnToLocomotion();
    }

    IEnumerator TriggerRoutine(string triggerParam)
    {
        _isPlayingTriggeredAnimation = true;
        _triggerPrevPaused = isPaused;
        _triggerAgentWasValid = AgentReady();
        _triggerWasStoppedBefore = _triggerAgentWasValid && agent.isStopped;
        isPaused = true;

        if (_triggerAgentWasValid)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (animationController == null)
        {
            isPaused = _triggerPrevPaused;
            _isPlayingTriggeredAnimation = false;
            _triggerCoroutine = null;
            yield break;
        }

        animationController.speed = 1f;
        ResetAllAnimatorTriggers();

        AnimatorControllerParameterType? pType = GetAnimatorParameterType(animationController, triggerParam);
        if (pType == null)
        {
            Debug.LogWarning($"{name}: Animator has no parameter '{triggerParam}'.");
            isPaused = _triggerPrevPaused;
            if (_triggerAgentWasValid) agent.isStopped = _triggerWasStoppedBefore;
            _isPlayingTriggeredAnimation = false;
            _triggerCoroutine = null;
            yield break;
        }

        const int layer = 0;
        int startHash = animationController.GetCurrentAnimatorStateInfo(layer).fullPathHash;
        SetAnimatorParamOn(animationController, triggerParam, pType.Value);

        bool entered = false;
        int triggeredHash = startHash;
        float enterT = 0f;
        while (enterT < triggerEnterTimeout)
        {
            AnimatorStateInfo info = animationController.GetCurrentAnimatorStateInfo(layer);
            if (animationController.IsInTransition(layer) || info.fullPathHash != startHash)
            {
                entered = true;
                triggeredHash = info.fullPathHash;
                break;
            }
            enterT += Time.deltaTime;
            yield return null;
        }

        SetAnimatorParamOff(animationController, triggerParam, pType.Value);
        if (pType.Value == AnimatorControllerParameterType.Trigger)
        {
            yield return null;
            animationController.ResetTrigger(triggerParam);
        }

        float t = 0f;
        while (t < triggerMaxDuration)
        {
            AnimatorStateInfo info = animationController.GetCurrentAnimatorStateInfo(layer);
            if (entered && !animationController.IsInTransition(layer))
            {
                if (info.fullPathHash == triggeredHash && info.normalizedTime >= 1f) break;
                if (info.fullPathHash == startHash) break;
            }
            t += Time.deltaTime;
            yield return null;
        }

        if (triggerExitBuffer > 0f)
            yield return new WaitForSeconds(triggerExitBuffer);

        SetAnimatorParamOff(animationController, triggerParam, pType.Value);
        if (pType.Value == AnimatorControllerParameterType.Trigger)
            animationController.ResetTrigger(triggerParam);

        isPaused = _triggerPrevPaused;
        if (_triggerAgentWasValid && AgentReady())
            agent.isStopped = _triggerWasStoppedBefore;

        _isPlayingTriggeredAnimation = false;
        _triggerCoroutine = null;

        if (useStateMachine && !isPaused)
            ForceReturnToLocomotion();
    }

    AnimatorControllerParameterType? GetAnimatorParameterType(Animator anim, string paramName)
    {
        foreach (var p in anim.parameters)
            if (p.name == paramName) return p.type;
        return null;
    }

    void SetAnimatorParamOn(Animator anim, string paramName, AnimatorControllerParameterType type)
    {
        switch (type)
        {
            case AnimatorControllerParameterType.Bool:
                anim.SetBool(paramName, true);
                break;
            case AnimatorControllerParameterType.Trigger:
                anim.ResetTrigger(paramName);
                anim.SetTrigger(paramName);
                break;
            case AnimatorControllerParameterType.Int:
                anim.SetInteger(paramName, 1);
                break;
            case AnimatorControllerParameterType.Float:
                anim.SetFloat(paramName, 1f);
                break;
        }
    }

    void SetAnimatorParamOff(Animator anim, string paramName, AnimatorControllerParameterType type)
    {
        switch (type)
        {
            case AnimatorControllerParameterType.Bool:
                anim.SetBool(paramName, false);
                break;
            case AnimatorControllerParameterType.Trigger:
                anim.ResetTrigger(paramName);
                break;
            case AnimatorControllerParameterType.Int:
                anim.SetInteger(paramName, 0);
                break;
            case AnimatorControllerParameterType.Float:
                anim.SetFloat(paramName, 0f);
                break;
        }
    }

    bool EnsureAgentOnNavMesh(string context)
    {
        if (agent == null || !agent.isActiveAndEnabled)
        {
            Debug.LogWarning($"{name}: Agent missing/disabled ({context}).");
            return false;
        }

        if (agent.isOnNavMesh) return true;

        if (!autoSnapToNavMeshOnGo)
        {
            Debug.LogWarning($"{name}: Agent not on NavMesh ({context}).");
            return false;
        }

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, snapToNavMeshRadius, NavMesh.AllAreas))
        {
            if (!agent.enabled) agent.enabled = true;
            agent.Warp(hit.position);
            return agent.isOnNavMesh;
        }

        Debug.LogWarning($"{name}: Could not snap to NavMesh within {snapToNavMeshRadius}m ({context}).");
        return false;
    }

    static bool HasArrived(NavMeshAgent a, float arriveDist)
    {
        if (a == null || !a.isOnNavMesh || a.pathPending || !a.hasPath || float.IsInfinity(a.remainingDistance))
            return false;
        return a.remainingDistance <= Mathf.Max(arriveDist, a.stoppingDistance);
    }

    void ForceIdlePose()
    {
        if (animationController == null) return;
        animationController.SetFloat(paramMovingX, 0f, animDampTime, Time.deltaTime);
        animationController.SetFloat(paramMovingY, 0f, animDampTime, Time.deltaTime);
        animationController.SetFloat(paramBlend, 0f, animDampTime, Time.deltaTime);
    }

    private float GetCommandStoppingDistance()
    {
        if (!_hasCommand) return Mathf.Max(arriveDistance, 0.05f);
        return (_commandGoal == NPCState.Talk || _commandGoal == NPCState.Attacking)
            ? Mathf.Max(arriveDistance, talkRange * 0.8f)
            : Mathf.Max(arriveDistance, 0.05f);
    }

    private void ApplyStoppingDistanceForCurrentMode()
    {
        if (agent == null) return;
        agent.stoppingDistance = (_state == NPCState.Approaching && _hasCommand)
            ? GetCommandStoppingDistance()
            : Mathf.Max(arriveDistance, 0.05f);
    }

    private void StartSitCommandFresh()
    {
        InterruptAllTransientActions();

        currentTarget = null;
        _talkTargetController = null;
        _registeredAsConversationSpeaker = false;

        useStateMachine = true;
        _hasCommand = true;
        _commandGoal = NPCState.Sitting;

        _hasLastApproachDest = false;
        _hasPatrolDestination = false;
        _hasResumeDestination = false;
        _resumeDestination = Vector3.zero;

        if (agent != null)
        {
            if (!agent.enabled) agent.enabled = true;
            if (!agent.isOnNavMesh && TryGetNavmeshPoint(transform.position, out Vector3 navPos))
                agent.Warp(navPos);
        }

        if (AgentReady())
        {
            agent.autoBraking = true;
            agent.stoppingDistance = Mathf.Max(0.05f, arriveDistance);
            agent.isStopped = false;
            agent.ResetPath();
        }

        _state = NPCState.Sitting;
        _stateTimer = 0f;
        EnterSitting();
    }

    private void PushImmediateCommandDestination()
    {
        if (!AgentReady() || !_hasCommand || currentTarget == null) return;
        _lastApproachDest = currentTarget.position;
        _hasLastApproachDest = true;
        _approachRepathTimer = Mathf.Max(0.05f, approachRepathInterval);
        agent.isStopped = false;
        agent.ResetPath();
        agent.SetDestination(_lastApproachDest);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(NPCController))]
public class NPCControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        NPCController npc = (NPCController)target;

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Triggered Animation Controls", EditorStyles.boldLabel);

        if (npc.triggerNames != null && npc.triggerNames.Count > 0)
            npc.selectedTriggerIndex = EditorGUILayout.Popup("Trigger", npc.selectedTriggerIndex, npc.triggerNames.ToArray());
        else
            EditorGUILayout.HelpBox("Add at least one trigger name to 'triggerNames'.", MessageType.Info);

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Play Trigger")) npc.PlaySelectedTrigger();
            if (GUILayout.Button("Pause Anim")) npc.PauseTriggeredAnimation();
            if (GUILayout.Button("Stop Anim")) npc.StopTriggeredAnimation();
            EditorGUILayout.EndHorizontal();
        }

        if (!Application.isPlaying)
            EditorGUILayout.HelpBox("Buttons work in Play Mode only.", MessageType.None);

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("FSM Controls", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            EditorGUILayout.LabelField("Current State", npc.GetCurrentState().ToString());
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Force Patrol")) npc.ForcePatrol();
            if (GUILayout.Button("Clear Target")) npc.ClearTarget();
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Target Testing (NPC Enum)", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Approach")) npc.DebugApproachTargetNPC();
            if (GUILayout.Button("Attack")) npc.DebugAttackTargetNPC();
            if (GUILayout.Button("Talk")) npc.DebugTalkTargetNPC();
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Sitting Controls", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Request Sit Down")) npc.RequestSitDown();
            if (GUILayout.Button("Request Stand Up")) npc.RequestStandUp();
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Lying Controls", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Request Lie Down")) npc.RequestLieDown();
            if (GUILayout.Button("Request Wake Up")) npc.RequestWakeUp();
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif