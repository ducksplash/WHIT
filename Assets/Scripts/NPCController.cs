// NPCController.cs
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
    // =========================================================
    // FSM
    // =========================================================
    public enum NPCState
    {
        Patrolling,
        Alerted,
        Approaching,
        Attacking,
        Seeking,
        Talk,
        Sitting
    }

    [Header("Identity")]
    public NPC thisNPC = NPC.EimearScott;

    [Header("AI State Machine")]
    [Tooltip("If true, the NPC runs the FSM.")]
    public bool useStateMachine = true;

    [Tooltip("Current target the NPC can spot/follow/attack. Set this externally (or via another system).")]
    public Transform currentTarget;

    [Tooltip("Patrol/seeking area radius from starting point.")]
    public float activeRadius = 15f;

    [Header("Testing / Overrides")]
    [Tooltip("TESTING: If enabled, the NPC is allowed to chase/approach targets even when they are outside Active Radius.")]
    public bool allowApproachOutsideActiveRadius = false;

    [Tooltip("TESTING: Pick a target NPC by enum (uses NPCManager list).")]
    public NPC debugTargetNPC = NPC.EimearScott;

    [Header("Perception")]
    [Tooltip("How far the NPC can spot a target.")]
    public float visionRange = 12f;

    [Tooltip("Field of view in degrees.")]
    [Range(0f, 360f)] public float visionFOV = 120f;

    [Tooltip("If true, requires line of sight (raycast) to spot/keep target.")]
    public bool requireLineOfSight = false;

    [Tooltip("Layers that block line of sight (only used if requireLineOfSight = true).")]
    public LayerMask losBlockers = ~0;

    [Header("Combat")]
    [Tooltip("Distance at which the NPC switches to Attacking (natural AI, not button-command).")]
    public float attackRange = 1.8f;

    [Tooltip("Seconds to stay 'Alerted' before Approaching.")]
    public float alertedDuration = 0.35f;

    [Header("Interaction / Talk")]
    [Tooltip("Talk distance (and also the button-command 'attack radius' per your test spec).")]
    public float talkRange = 4.0f;

    [Header("Patrol")]
    [Tooltip("How often (randomly) the NPC changes direction while patrolling (seconds).")]
    public Vector2 patrolChangeDirInterval = new Vector2(3f, 7f);

    [Tooltip("Idle pause after reaching a patrol point (seconds).")]
    public Vector2 patrolArrivePause = new Vector2(0.2f, 1.0f);

    [Tooltip("How many attempts to find a valid navmesh point for patrol.")]
    public int patrolPointTries = 10;

    [Tooltip("NavMesh sampling radius for each patrol attempt.")]
    public float patrolSampleRadius = 2.0f;

    [Header("Seeking")]
    [Tooltip("Seeking is like patrolling but faster.")]
    public float seekingSpeedMultiplier = 1.2f;

    [Tooltip("Give up seeking after this many seconds, then resume patrolling.")]
    public float seekGiveUpSeconds = 6f;

    [Tooltip("How often to refresh the approach destination while following (seconds).")]
    public float approachRepathInterval = 0.25f;

    [Header("Debug (Runtime)")]
    [SerializeField] private NPCState _state = NPCState.Patrolling;

    [Header("Approach Smoothing")]
    [Tooltip("Only repath if the target moved at least this far since our last SetDestination.")]
    public float approachTargetMoveThreshold = 0.35f;

    [Tooltip("Disable autoBraking while chasing a moving target to prevent stop/start.")]
    public bool disableBrakingWhileApproaching = true;

    public NPCManager sceneNPCManager;

    Vector3 _lastApproachDest;
    bool _hasLastApproachDest;

    // FSM runtime
    Vector3 _spawnPoint;
    Vector3 _patrolDestination;
    bool _hasPatrolDestination;

    float _stateTimer;
    float _patrolChangeTimer;
    float _patrolArriveTimer;
    float _approachRepathTimer;
    float _seekTimer;

    Vector3 _lastKnownTargetPos;
    bool _hasLastKnownTargetPos;

    bool _isConversationLocked = false;
    NPCController _conversationSpeaker;

    bool _hasCommand = false;
    NPCState _commandGoal = NPCState.Patrolling;

    NPCController _talkTargetController;

    [Header("Components")]
    public Animator animationController;
    public NavMeshAgent agent;

    [Header("Arrival / Blending")]
    [Tooltip("How close to a destination counts as 'arrived'.")]
    public float arriveDistance = 0.3f;

    [Tooltip("Extra time to let the agent settle after arriving before beginning stop blending.")]
    public float arriveSettleTime = 0.05f;

    [Tooltip("After arriving, allow the agent to naturally slow for this long before forcing stop.")]
    public float stopFadeOutTime = 0.35f;

    [Tooltip("When agent velocity is below this, we treat the NPC as idle.")]
    public float idleVelocityThreshold = 0.05f;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float angularSpeed = 240f;
    public float acceleration = 8f;

    [Header("Smoothing")]
    [Tooltip("Seconds to smooth blend tree params.")]
    public float animDampTime = 0.15f;

    [Tooltip("How quickly the NPC turns to face movement direction when agent.updateRotation = false.")]
    public float turnSmoothing = 10f;

    [Header("NavMesh Placement")]
    [Tooltip("How far to search to snap this NPC onto the NavMesh if it spawns slightly off-mesh.")]
    public float snapToNavMeshRadius = 2f;

    [Tooltip("If true, will try to snap the agent onto the NavMesh automatically.")]
    public bool autoSnapToNavMeshOnGo = true;

    [Header("Animator Params")]
    public string paramBlend = "Blend";
    public string paramMovingX = "MovingX";
    public string paramMovingY = "MovingY";

    // -----------------------
    // Triggered Animations (Naurani-style)
    // -----------------------
    [Header("Triggered Animations (Naurani-style)")]
    [Tooltip("Animator Trigger parameter names you want to be able to fire manually.")]
    public List<string> triggerNames = new List<string>();

    [HideInInspector] public int selectedTriggerIndex = 0;

    [Tooltip("How long we wait for the animator to change state after firing a trigger.")]
    public float triggerEnterTimeout = 1.0f;

    [Tooltip("Safety cap if the triggered animation never completes (e.g. loops).")]
    public float triggerMaxDuration = 8.0f;

    [Tooltip("Small buffer after trigger ends before resuming navmesh.")]
    public float triggerExitBuffer = 0.05f;

    // -----------------------
    // Locomotion / Blend Tree Return
    // -----------------------
    [Header("Locomotion / Blend Tree Return")]
    [Tooltip("Name of your locomotion state (the state that contains the blend tree).")]
    [SerializeField] private string locomotionStateName = "Locomotion";
    [SerializeField] private int locomotionLayer = 0;
    [SerializeField] private float locomotionCrossfade = 0.12f;

    // Runtime state
    [NonSerialized] public bool isPaused = false;

    // Trigger play coroutine
    Coroutine _triggerCoroutine;
    bool _isPlayingTriggeredAnimation = false;

    [Header("Crowd Avoidance / Pre-collision renegotiate")]
    [Tooltip("Layer that NPC root objects are on (for overlap checks).")]
    public LayerMask npcLayerMask = ~0;

    [Tooltip("How close another NPC can get before we renegotiate.")]
    public float personalSpaceRadius = 0.9f;

    [Tooltip("Only react if the other NPC is roughly in front of us (dot threshold).")]
    [Range(-1f, 1f)]
    public float inFrontDotThreshold = 0.35f;

    [Tooltip("How far to sidestep when avoiding an imminent bump.")]
    public float sidestepDistance = 0.9f;

    [Tooltip("How long to follow the sidestep before resuming original destination.")]
    public float sidestepDuration = 0.7f;

    [Tooltip("Cooldown between renegotiations (prevents spam).")]
    public float renegotiateCooldown = 0.8f;

    [Tooltip("How often to run the proximity check.")]
    public float renegotiateCheckInterval = 0.15f;

    float _renegotiateT;
    float _renegotiateCooldownT;
    Coroutine _sidestepCoroutine;

    Vector3 _resumeDestination;
    bool _hasResumeDestination;

    // Trigger snapshot state
    bool _triggerPrevPaused;
    bool _triggerAgentWasValid;
    bool _triggerHadPathBefore;
    bool _triggerWasStoppedBefore;

    Vector3 _animVelocitySmoothed = Vector3.zero;

    // =========================================================
    // SITTING
    // =========================================================

    [SerializeField] private Transform modelRoot;

    private Vector3 _seatNavPos;
    private Vector3 _preSitNavPos;
    [SerializeField] private float seatNavSampleRadius = 6.0f;

    [SerializeField] private float sitTriggerFallbackDelay = 0.08f;
    private float _sitTriggerT = 0f;
    private bool _sitTriggerSent = false;

    private enum SitPhase
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

    [Header("Sitting")]
    [Tooltip("Seat objects must be on this layer (e.g. a layer named 'Seat').")]
    [SerializeField] private LayerMask seatLayerMask;

    [SerializeField] private bool seatDebugLogs = false;

    [Tooltip("If Seat colliders are triggers, keep this as Collide.")]
    [SerializeField] private QueryTriggerInteraction seatQueryTriggers = QueryTriggerInteraction.Collide;

    [Tooltip("How long to try finding a free seat before giving up.")]
    [SerializeField] private float seatSearchTimeout = 4.0f;

    [Tooltip("How often to rescan for seats while searching.")]
    [SerializeField] private float seatRescanInterval = 0.35f;

    [Tooltip("Search radius for seats (0 uses activeRadius).")]
    [SerializeField] private float seatSearchRadius = 0f;

    [Tooltip("We first approach a point in FRONT of the seat so we can turn + backstep onto it.")]
    [SerializeField] private float preSitForwardOffset = 0.45f;

    [Tooltip("How close to pre-sit point before aligning.")]
    [SerializeField] private float preSitArriveDistance = 0.35f;

    [Tooltip("How close yaw must be (degrees) before backstep.")]
    [SerializeField] private float alignYawToleranceDeg = 6f;

    [Tooltip("How far to step back into the seat just before sitting.")]
    [SerializeField] private float backstepDistance = 0.25f;

    [Tooltip("Speed for backstep (units/sec).")]
    [SerializeField] private float backstepSpeed = 0.8f;

    [Tooltip("If true, snap to seat transform position/rotation once seated (recommended).")]
    [SerializeField] private bool snapToSeatWhenSeated = true;

    [Tooltip("Optional: auto-stand after this many seconds. 0 = never auto-stand.")]
    [SerializeField] private float autoStandAfterSeconds = 0f;

    [Header("Sitting Animations")]
    [Tooltip("Animator state name for standing->sitting.")]
    [SerializeField] private string sitDownStateName = "SitDown";

    [Tooltip("Animator state name for seated idle.")]
    [SerializeField] private string sitIdleStateName = "SitIdle";

    [Tooltip("Animator layer for sit states.")]
    [SerializeField] private int sitAnimLayer = 0;

    [Tooltip("Crossfade time for sit transitions.")]
    [SerializeField] private float sitCrossfade = 0.10f;

    [Tooltip("If true, stand up by reversing SitDown (anim.speed=-1) from end to start.")]
    [SerializeField] private bool standUpByReversingSitDown = true;

    [Tooltip("If you prefer a trigger param instead of CrossFade state name for sit down.")]
    [SerializeField] private bool useSitTriggerParam = false;

    [SerializeField] private string sitTriggerParam = "SitDown";

    // Sitting runtime
    private SitPhase _sitPhase = SitPhase.None;
    private Seat _seat;
    private Transform _seatTf;

    private float _seatSearchT;
    private float _seatRescanT;
    private float _seatedT;

    private Vector3 _preSitPoint;

    // =========================================================
    // Small shared helpers (FSM)
    // =========================================================

    bool IsNavDriven()
    {
        if (isPaused) return false;
        if (_isPlayingTriggeredAnimation) return false;
        return useStateMachine;
    }

    public NPCController ResolveNPC(NPC npcEnum)
    {
        var list = sceneNPCManager != null ? sceneNPCManager.NPCList : null;
        if (list == null) return null;

        for (int i = 0; i < list.Count; i++)
        {
            var c = list[i];
            if (c != null && c.thisNPC == npcEnum)
                return c;
        }
        return null;
    }

    public void SetTargetByNPC(NPC npcEnum)
    {
        var c = ResolveNPC(npcEnum);
        if (c == null)
        {
            Debug.LogWarning($"{name}: Could not find target NPC '{npcEnum}' in NPCManager list.");
            return;
        }

        if (c == this)
        {
            Debug.LogWarning($"{name}: Tried to target self ({npcEnum}).");
            return;
        }

        SetTarget(c.transform);
        _talkTargetController = c;
    }

    public void DebugApproachTargetNPC()
    {
        useStateMachine = true;
        ForceReturnToLocomotion();

        _hasCommand = false;
        _commandGoal = NPCState.Patrolling;

        SetTargetByNPC(debugTargetNPC);

        if (currentTarget != null)
            EnterState(NPCState.Approaching);
    }

    public void DebugAttackTargetNPC()
    {
        useStateMachine = true;
        ForceReturnToLocomotion();

        SetTargetByNPC(debugTargetNPC);
        if (currentTarget == null) return;

        _hasCommand = true;
        _commandGoal = NPCState.Attacking;

        float d = Vector3.Distance(transform.position, currentTarget.position);
        EnterState(d <= talkRange ? NPCState.Attacking : NPCState.Approaching);
    }

    public void DebugTalkTargetNPC()
    {
        useStateMachine = true;
        ForceReturnToLocomotion();

        SetTargetByNPC(debugTargetNPC);
        if (currentTarget == null) return;

        _hasCommand = true;
        _commandGoal = NPCState.Talk;

        float d = Vector3.Distance(transform.position, currentTarget.position);
        EnterState(d <= talkRange ? NPCState.Talk : NPCState.Approaching);
    }

    public void StopAndFace(Vector3 worldPos)
    {
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        ForceIdlePose();

        Vector3 to = worldPos - transform.position;
        to.y = 0f;
        if (to.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(to.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * (turnSmoothing * 1.5f));
    }

    void FaceTowards(Vector3 worldPos, float dt, float speed)
    {
        Vector3 to = worldPos - transform.position;
        to.y = 0f;
        if (to.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(to.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, dt * speed);
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

        Vector3 velocity = Vector3.zero;
        if (agent != null && agent.enabled && agent.isOnNavMesh)
            velocity = agent.velocity;

        Vector3 localVel = transform.InverseTransformDirection(velocity);
        float speed = velocity.magnitude;

        float movingY = Mathf.Clamp(localVel.z, -1f, 1f);
        float movingX = Mathf.Clamp(localVel.x, -1f, 1f);
        float blend = (moveSpeed <= 0.001f) ? 0f : Mathf.Clamp01(speed / moveSpeed);

        animationController.SetFloat(paramMovingX, movingX);
        animationController.SetFloat(paramMovingY, movingY);
        animationController.SetFloat(paramBlend, blend);
    }

    void Reset()
    {
        animationController = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Awake()
    {
        var mgr = FindFirstObjectByType<NPCManager>();
        if (mgr != null)
        {
            mgr.RegisterNPC(this);
        }
        else
        {
            Debug.LogWarning($"{name}: No NPCManager found in scene during Awake. Will retry in Start.");
        }
    }

    public void Start()
    {
        if (animationController == null) animationController = GetComponentInChildren<Animator>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();

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
            if (seatLayer >= 0)
                seatLayerMask = 1 << seatLayer;
            else
                seatLayerMask = 1 << 21;
        }

        if (modelRoot == null && animationController != null) modelRoot = animationController.transform;

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
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
                agent.isStopped = true;

            ForceIdlePose();

            if (_conversationSpeaker != null)
                FaceTowards(_conversationSpeaker.transform.position, Time.deltaTime, turnSmoothing * 1.5f);

            UpdateAnimatorFromMovement();
            return;
        }

        if (useStateMachine && !_isPlayingTriggeredAnimation)
        {
            if (_state == NPCState.Sitting)
                TickFSM(Time.deltaTime);
            else if (EnsureFSMCanRun())
                TickFSM(Time.deltaTime);
        }

        UpdateAnimatorFromMovement();
        UpdateFacing();
        PreCollisionRenegotiate();
    }

    bool EnsureFSMCanRun()
    {
        if (agent == null || !agent.isActiveAndEnabled) return false;
        if (!EnsureAgentOnNavMesh("FSM")) return false;
        return true;
    }

    // =========================================================
    // FSM: public hooks
    // =========================================================

    public NPCState GetCurrentState() => _state;

    public void SetTarget(Transform t)
    {
        currentTarget = t;
        if (currentTarget != null)
        {
            _lastKnownTargetPos = currentTarget.position;
            _hasLastKnownTargetPos = true;

            if (useStateMachine && !_isPlayingTriggeredAnimation && !_hasCommand && _state != NPCState.Sitting)
                EnterState(NPCState.Alerted);
        }
    }

    public void ClearTarget()
    {
        if (_talkTargetController != null) _talkTargetController.EndConversationAsTarget();
        currentTarget = null;
        _talkTargetController = null;
        _hasCommand = false;
        _commandGoal = NPCState.Patrolling;
    }

    public void ForcePatrol()
    {
        useStateMachine = true;

        if (_talkTargetController != null) _talkTargetController.EndConversationAsTarget();
        _hasCommand = false;
        _commandGoal = NPCState.Patrolling;
        _talkTargetController = null;

        if (_state == NPCState.Sitting)
            ReleaseSeatIfAny();

        _sitPhase = SitPhase.None;

        ForceReturnToLocomotion();
        EnterState(NPCState.Patrolling);
    }

    // =========================================================
    // Sitting public API
    // =========================================================

    public void RequestSitDown()
    {
        if (_isConversationLocked) return;

        useStateMachine = true;
        _hasCommand = true;
        _commandGoal = NPCState.Sitting;

        EnterState(NPCState.Sitting);
    }

    public void RequestStandUp()
    {
        if (_state != NPCState.Sitting) return;

        if (_sitPhase == SitPhase.SittingIdle || _sitPhase == SitPhase.SitDownPlaying)
            BeginStandUp();
    }

    // =========================================================
    // FSM: core
    // =========================================================

    void EnterState(NPCState next)
    {
        NPCState prev = _state;
        _state = next;
        _stateTimer = 0f;

        if (agent == null) return;
        agent.isStopped = false;

        switch (_state)
        {
            case NPCState.Patrolling:
                agent.speed = moveSpeed;
                _hasPatrolDestination = false;
                _patrolChangeTimer = UnityEngine.Random.Range(patrolChangeDirInterval.x, patrolChangeDirInterval.y);
                _patrolArriveTimer = 0f;
                agent.autoBraking = true;
                break;

            case NPCState.Alerted:
                agent.speed = moveSpeed;
                agent.isStopped = true;
                agent.autoBraking = true;
                break;

            case NPCState.Approaching:
                agent.speed = moveSpeed;
                _approachRepathTimer = 0f;
                _hasLastApproachDest = false;

                if (disableBrakingWhileApproaching)
                    agent.autoBraking = false;
                break;

            case NPCState.Attacking:
                agent.isStopped = true;
                agent.autoBraking = true;
                Debug.Log($"{name} ATTACKING: TODO hook up attack logic/animation.");
                break;

            case NPCState.Seeking:
                agent.speed = moveSpeed * Mathf.Max(0.01f, seekingSpeedMultiplier);
                _seekTimer = 0f;
                _hasPatrolDestination = false;
                _patrolChangeTimer = UnityEngine.Random.Range(patrolChangeDirInterval.x, patrolChangeDirInterval.y);
                _patrolArriveTimer = 0f;
                break;

            case NPCState.Talk:
                agent.isStopped = true;
                agent.autoBraking = true;
                agent.ResetPath();
                Debug.Log($"{name} TALK: TODO hook up talk later. Holding idle until new command.");
                break;

            case NPCState.Sitting:
                agent.isStopped = false;
                agent.autoBraking = true;

                // FIX: Only initialise sitting once when first entering this state.
                // Previously this called EnterSitting() unconditionally, which called
                // ReleaseSeatIfAny() every time — wiping _seat mid-approach.
                if (prev != NPCState.Sitting)
                    EnterSitting();

                break;
        }
    }

    void TickFSM(float dt)
    {
        _stateTimer += dt;

        if (currentTarget != null)
        {
            _lastKnownTargetPos = currentTarget.position;
            _hasLastKnownTargetPos = true;
        }

        if (!_hasCommand && _state != NPCState.Talk && _state != NPCState.Sitting)
        {
            if (currentTarget != null && CanSeeTarget(currentTarget))
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
        }

        switch (_state)
        {
            case NPCState.Patrolling:   TickPatrolling(dt, speedMultiplier: 1f); break;
            case NPCState.Alerted:      TickAlerted(dt); break;
            case NPCState.Approaching:  TickApproaching(dt); break;
            case NPCState.Attacking:    TickAttacking(dt); break;
            case NPCState.Seeking:      TickSeeking(dt); break;
            case NPCState.Talk:         TickTalk(dt); break;
            case NPCState.Sitting:      TickSitting(dt); break;
        }
    }

    void TickPatrolling(float dt, float speedMultiplier)
    {
        if (agent == null || !agent.isOnNavMesh) return;

        if (currentTarget != null && CanSeeTarget(currentTarget))
            return;

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

        if (!_hasPatrolDestination)
        {
            if (TryPickPatrolPoint(_spawnPoint, activeRadius, out _patrolDestination))
            {
                agent.isStopped = false;
                agent.speed = moveSpeed * Mathf.Max(0.01f, speedMultiplier);
                agent.ResetPath();
                agent.SetDestination(_patrolDestination);
                _hasPatrolDestination = true;
            }
        }
    }

    void TickAlerted(float dt)
    {
        if (agent != null) agent.isStopped = true;

        if (currentTarget != null)
        {
            Vector3 to = currentTarget.position - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(to.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, dt * (turnSmoothing * 1.25f));
            }
        }

        if (_stateTimer >= alertedDuration)
        {
            if (currentTarget != null)
            {
                float distToSpawn = Vector3.Distance(_spawnPoint, currentTarget.position);

                if (distToSpawn <= activeRadius || allowApproachOutsideActiveRadius)
                    EnterState(NPCState.Approaching);
                else
                    EnterState(NPCState.Seeking);
            }
            else
            {
                EnterState(NPCState.Seeking);
            }
        }
    }

    void TickApproaching(float dt)
    {
        if (agent == null || !agent.isOnNavMesh) return;

        if (!_hasCommand && currentTarget != null)
        {
            float distToSpawn = Vector3.Distance(_spawnPoint, currentTarget.position);
            if (distToSpawn > activeRadius && !allowApproachOutsideActiveRadius)
            {
                EnterState(NPCState.Seeking);
                return;
            }
        }

        // FIX: Removed the sit-specific approach block from TickApproaching entirely.
        // Previously it called EnterState(NPCState.Sitting) when arriving at pre-sit point,
        // which triggered EnterSitting() → ReleaseSeatIfAny(), wiping the acquired seat.
        // The sit approach is now handled entirely within TickSitting/TickApproachFront.

        if (currentTarget == null && !(_hasCommand && _commandGoal == NPCState.Sitting))
        {
            _hasCommand = false;
            _commandGoal = NPCState.Patrolling;
            EnterState(NPCState.Patrolling);
            return;
        }

        if (_hasCommand)
        {
            if (currentTarget != null)
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
            }

            _approachRepathTimer -= dt;
            if (_approachRepathTimer <= 0f && currentTarget != null)
            {
                _approachRepathTimer = Mathf.Max(0.05f, approachRepathInterval);
                agent.isStopped = false;
                agent.SetDestination(currentTarget.position);
            }

            return;
        }

        if (currentTarget == null)
        {
            EnterState(NPCState.Patrolling);
            return;
        }

        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (dist <= attackRange)
        {
            EnterState(NPCState.Attacking);
            return;
        }

        _approachRepathTimer -= dt;
        if (_approachRepathTimer <= 0f)
        {
            _approachRepathTimer = Mathf.Max(0.05f, approachRepathInterval);
            agent.isStopped = false;
            agent.SetDestination(currentTarget.position);
        }
    }

    void TickAttacking(float dt)
    {
        if (_hasCommand && _commandGoal == NPCState.Attacking)
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            ForceIdlePose();

            if (currentTarget != null)
                FaceTowards(currentTarget.position, dt, turnSmoothing * 1.25f);

            return;
        }

        if (currentTarget == null)
        {
            EnterState(NPCState.Seeking);
            return;
        }

        float distToSpawn = Vector3.Distance(_spawnPoint, currentTarget.position);
        if (distToSpawn > activeRadius && !allowApproachOutsideActiveRadius)
        {
            EnterState(NPCState.Seeking);
            return;
        }

        float distToTarget = Vector3.Distance(transform.position, currentTarget.position);
        if (distToTarget > attackRange * 1.15f)
        {
            EnterState(NPCState.Approaching);
            return;
        }

        Vector3 to = currentTarget.position - transform.position;
        to.y = 0f;
        if (to.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(to.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, dt * (turnSmoothing * 1.25f));
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

        if (currentTarget != null && CanSeeTarget(currentTarget))
        {
            float distToSpawn = Vector3.Distance(_spawnPoint, currentTarget.position);
            if (distToSpawn <= activeRadius || allowApproachOutsideActiveRadius)
            {
                EnterState(NPCState.Alerted);
                return;
            }
        }

        TickPatrolling(dt, speedMultiplier: seekingSpeedMultiplier);
    }

    void TickTalk(float dt)
    {
        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = true;

        ForceIdlePose();

        if (currentTarget == null)
        {
            _hasCommand = false;
            _commandGoal = NPCState.Patrolling;
            EnterState(NPCState.Patrolling);
            return;
        }

        FaceTowards(currentTarget.position, dt, turnSmoothing * 1.5f);

        if (_talkTargetController == null)
            _talkTargetController = currentTarget.GetComponentInParent<NPCController>();

        if (_talkTargetController != null)
            _talkTargetController.BeginConversationAsTarget(this);
    }

    public void BeginConversationAsTarget(NPCController speaker)
    {
        if (speaker == null) return;

        if (!_isConversationLocked)
        {
            useStateMachine = false;

            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            ForceIdlePose();
        }

        _isConversationLocked = true;
        _conversationSpeaker = speaker;
    }

    public void EndConversationAsTarget()
    {
        _isConversationLocked = false;
        _conversationSpeaker = null;
    }

    bool CanSeeTarget(Transform t)
    {
        if (t == null) return false;

        Vector3 to = t.position - transform.position;
        float dist = to.magnitude;
        if (dist > visionRange) return false;

        Vector3 flatTo = to; flatTo.y = 0f;
        Vector3 fwd = transform.forward; fwd.y = 0f;

        if (flatTo.sqrMagnitude > 0.0001f && fwd.sqrMagnitude > 0.0001f)
        {
            float angle = Vector3.Angle(fwd.normalized, flatTo.normalized);
            if (angle > visionFOV * 0.5f) return false;
        }

        if (requireLineOfSight)
        {
            Vector3 origin = transform.position + Vector3.up * 1.6f;
            Vector3 targetPos = t.position + Vector3.up * 1.2f;
            Vector3 dir = (targetPos - origin);
            float d = dir.magnitude;
            if (d > 0.001f)
            {
                dir /= d;
                if (Physics.Raycast(origin, dir, out _, d, losBlockers, QueryTriggerInteraction.Ignore))
                    return false;
            }
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

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, patrolSampleRadius, NavMesh.AllAreas))
            {
                if (Vector3.Distance(center, hit.position) <= radius + 0.25f)
                {
                    result = hit.position;
                    return true;
                }
            }
        }

        return false;
    }

    // =========================================================
    // SITTING — fixed sequence
    // =========================================================

    private void EnterSitting()
    {
        _sitPhase = SitPhase.SearchingSeat;
        _seatSearchT = 0f;
        _seatRescanT = 0f;
        _seatedT = 0f;

        // Only release a seat here if we're genuinely starting fresh.
        // Do NOT call this when re-entering Sitting state mid-sequence.
        ReleaseSeatIfAny();

        if (agent && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
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
                // FIX: Agent must be valid for nav phases
                if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;
                TickApproachFront();
                break;

            case SitPhase.Aligning:
                if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;
                if (_seatTf == null) { FailSittingAndReturnToPatrol(); return; }
                {
                    Vector3 sf = GetSeatFacing();
                    TickAlign(dt, sf);
                }
                break;

            case SitPhase.Backstepping:
                if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;
                if (_seatTf == null) { FailSittingAndReturnToPatrol(); return; }
                {
                    Vector3 sf = GetSeatFacing();
                    TickBackstep(dt, _seatNavPos, sf);
                }
                break;

            case SitPhase.SitDownPlaying:
                TickSitDownPlaying(dt);
                break;

            case SitPhase.SittingIdle:
                if (_seatTf == null) { FailSittingAndReturnToPatrol(); return; }
                {
                    Vector3 seatPos = _seatTf.position;
                    Vector3 sf = GetSeatFacing();
                    TickSittingIdle(dt, seatPos, sf);
                }
                break;

            case SitPhase.StandUpPlaying:
                TickStandUpPlaying(dt);
                break;
        }
    }

    // Returns the direction the NPC should FACE when seated.
    // The seat's forward is the direction a person faces OUT of the seat,
    // so we face that direction. Adjust this if your seats use a different convention.
    private Vector3 GetSeatFacing()
    {
        if (_seatTf == null) return transform.forward;
        Vector3 f = _seatTf.forward;
        f.y = 0f;
        if (f.sqrMagnitude < 0.0001f) return transform.forward;
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
                return; // TryAcquireSeat sets phase to ApproachingFront
        }

        // Keep standing still while searching
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        ForceIdlePose();
    }
    
        private bool TryAcquireSeat()
    {
        float radius = (seatSearchRadius > 0.01f) ? seatSearchRadius : Mathf.Max(0.1f, activeRadius);
        Vector3 center = _spawnPoint;

        Seat[] seats = FindObjectsByType<Seat>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (seats == null || seats.Length == 0)
        {
            if (seatDebugLogs) Debug.Log($"{name} Sit: No Seat components found (including inactive).");
            return false;
        }

        Seat best = null;
        float bestSqr = float.PositiveInfinity;

        for (int i = 0; i < seats.Length; i++)
        {
            Seat s = seats[i];
            if (!s) continue;

            // Must match seat layer mask (either seat GO or its seatTransform GO can be on SEAT)
            bool matchesMask =
                (((1 << s.gameObject.layer) & seatLayerMask.value) != 0) ||
                (s.seatTransform != null && (((1 << s.seatTransform.gameObject.layer) & seatLayerMask.value) != 0));

            if (!matchesMask) continue;
            if (!s.IsValid) continue;
            if (s.seatTransform == null) continue;
            if (s.IsOccupied) continue;

            Vector3 seatPos = s.seatTransform.position;

            // Must be within radius of spawn (your spec)
            Vector3 d = seatPos - center;
            d.y = 0f;
            if (d.sqrMagnitude > radius * radius) continue;

            // Must have some navmesh nearby (use BIG radius)
            if (!NavMesh.SamplePosition(seatPos, out NavMeshHit seatHit, seatNavSampleRadius, NavMesh.AllAreas))
            {
                if (seatDebugLogs)
                    Debug.Log($"{name} Sit: REJECT '{s.name}' reason=navmesh_sample_failed seatPos={seatPos} sampleR={seatNavSampleRadius}");
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
            Debug.LogWarning($"{name} Sit: Seats found, but none had NavMesh within {seatNavSampleRadius}m (or were valid/free/in-range/matching mask).");
            return false;
        }

        if (!best.TryOccupy(this))
        {
            Debug.LogWarning($"{name} Sit: Seat '{best.name}' refused occupancy (TryOccupy false).");
            return false;
        }

        _seat = best;
        _seatTf = best.seatTransform;

        // --- Compute seat forward / pre-sit point (world authoring) ---
        Vector3 seatForward = _seatTf.forward;
        seatForward.y = 0f;
        if (seatForward.sqrMagnitude < 0.0001f) seatForward = transform.forward;
        seatForward.Normalize();

        Vector3 seatPos2 = _seatTf.position;
        _preSitPoint = seatPos2 + seatForward * Mathf.Max(0f, preSitForwardOffset);

        // --- Snap BOTH target points to navmesh ---
        NavMesh.SamplePosition(seatPos2, out NavMeshHit seatHit2, seatNavSampleRadius, NavMesh.AllAreas);
        _seatNavPos = seatHit2.position;

        if (!NavMesh.SamplePosition(_preSitPoint, out NavMeshHit preHit, seatNavSampleRadius, NavMesh.AllAreas))
            _preSitNavPos = _seatNavPos; // fallback
        else
            _preSitNavPos = preHit.position;

        if (seatDebugLogs)
            Debug.Log($"{name} Sit: ACQUIRED '{_seat.name}' seatPos={seatPos2} seatNav={_seatNavPos} preSit={_preSitPoint} preSitNav={_preSitNavPos}");

        // Approach like Talk/Attack
        agent.enabled = true;
        agent.isStopped = false;
        agent.autoBraking = true;
        agent.ResetPath();
        agent.SetDestination(_preSitNavPos);

        _sitPhase = SitPhase.ApproachingFront;
        return true;
    }

    // FIX: TickApproachFront no longer uses HasArrived (which requires hasPath=true and
    // would never trigger when the path auto-clears near the destination).
    // Instead we use a plain distance check to _preSitNavPos.
    private void TickApproachFront()
    {
        agent.isStopped = false;

        // Re-issue destination if we don't have one or it drifted
        if (!agent.pathPending && (!agent.hasPath || Vector3.Distance(agent.destination, _preSitNavPos) > 0.15f))
            agent.SetDestination(_preSitNavPos);

        float dist = Vector3.Distance(
            new Vector3(transform.position.x, 0f, transform.position.z),
            new Vector3(_preSitNavPos.x, 0f, _preSitNavPos.z)
        );

        float threshold = Mathf.Max(preSitArriveDistance, arriveDistance);

        if (dist <= threshold)
        {
            if (seatDebugLogs)
                Debug.Log($"{name} Sit: Arrived at pre-sit point (dist={dist:F2}). Transitioning to Aligning.");

            agent.isStopped = true;
            agent.ResetPath();
            _sitPhase = SitPhase.Aligning;
        }
    }

    private void TickAlign(float dt, Vector3 seatFacing)
    {
        agent.isStopped = true;

        // FIX: Face the seat-facing direction (the direction a person faces when seated).
        // The NPC will then backstep into the seat, ending up correctly oriented.
        Quaternion targetRot = Quaternion.LookRotation(seatFacing, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, dt * (turnSmoothing * 1.5f));

        float yawDelta = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetRot.eulerAngles.y));

        if (seatDebugLogs)
            Debug.Log($"{name} Sit: Aligning — yawDelta={yawDelta:F1} tolerance={alignYawToleranceDeg}");

        if (yawDelta <= Mathf.Max(0.5f, alignYawToleranceDeg))
        {
            if (seatDebugLogs)
                Debug.Log($"{name} Sit: Aligned. Transitioning to Backstepping.");
            _sitPhase = SitPhase.Backstepping;
        }
    }

    private void TickBackstep(float dt, Vector3 seatNavPos, Vector3 seatFacing)
    {
        agent.isStopped = true;

        // Move backward (opposite to seatFacing) toward the seat position
        float step = Mathf.Max(0.01f, backstepSpeed) * dt;
        transform.position = Vector3.MoveTowards(transform.position, seatNavPos, step);

        // Maintain facing direction during backstep
        Quaternion targetRot = Quaternion.LookRotation(seatFacing, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, dt * (turnSmoothing * 2.0f));

        float dist = Vector3.Distance(
            new Vector3(transform.position.x, 0f, transform.position.z),
            new Vector3(seatNavPos.x, 0f, seatNavPos.z)
        );

        if (seatDebugLogs)
            Debug.Log($"{name} Sit: Backstepping — dist to seat={dist:F2}");

        if (dist <= 0.12f)
        {
            if (seatDebugLogs)
                Debug.Log($"{name} Sit: Backstepped into seat. Beginning SitDown.");
            BeginSitDown();
        }
    }

    private void BeginSitDown()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.enabled = false;
        }

        // Snap facing before playing animation
        if (_seatTf != null)
        {
            Vector3 f = GetSeatFacing();
            transform.rotation = Quaternion.LookRotation(f, Vector3.up);
        }

        if (animationController != null)
        {
            _animVelocitySmoothed = Vector3.zero;

            animationController.SetFloat(paramMovingX, 0f);
            animationController.SetFloat(paramMovingY, 0f);
            animationController.SetFloat(paramBlend, 0f);

            ResetAllAnimatorTriggers();
            animationController.CrossFadeInFixedTime(sitDownStateName, sitCrossfade, sitAnimLayer, 0f);

            if (seatDebugLogs)
                Debug.Log($"{name} Sit: CrossFade to '{sitDownStateName}'.");
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

        // Reached sit idle — done
        if (!string.IsNullOrWhiteSpace(sitIdleStateName) && info.IsName(sitIdleStateName))
        {
            _sitPhase = SitPhase.SittingIdle;
            _seatedT = 0f;

            if (snapToSeatWhenSeated && _seatTf != null)
            {
                transform.position = _seatTf.position;
                transform.rotation = Quaternion.LookRotation(GetSeatFacing(), Vector3.up);
            }

            if (seatDebugLogs)
                Debug.Log($"{name} Sit: Now in SittingIdle.");
            return;
        }

        // Fallback: force state by name if we're stuck in locomotion
        if (_sitTriggerSent && _sitTriggerT >= sitTriggerFallbackDelay)
        {
            bool inSitDown = !string.IsNullOrWhiteSpace(sitDownStateName) && info.IsName(sitDownStateName);
            bool inSitIdle = !string.IsNullOrWhiteSpace(sitIdleStateName) && info.IsName(sitIdleStateName);

            if (!inSitDown && !inSitIdle)
            {
                if (seatDebugLogs)
                    Debug.Log($"{name} Sit: Fallback — forcing Play('{sitDownStateName}').");
                animationController.Play(sitDownStateName, sitAnimLayer, 0f);
            }

            _sitTriggerSent = false;
        }
    }

    private void TickSittingIdle(float dt, Vector3 seatPos, Vector3 seatForward)
    {
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        ForceIdlePose();

        if (snapToSeatWhenSeated)
        {
            transform.position = seatPos;
            transform.rotation = Quaternion.LookRotation(seatForward, Vector3.up);
        }

        if (autoStandAfterSeconds > 0f)
        {
            _seatedT += dt;
            if (_seatedT >= autoStandAfterSeconds)
                BeginStandUp();
        }
    }

    private void BeginStandUp()
    {
        agent.isStopped = true;
        agent.ResetPath();

        if (animationController == null)
        {
            FinishStandUp();
            return;
        }

        ResetAllAnimatorTriggers();

        if (standUpByReversingSitDown && !string.IsNullOrWhiteSpace(sitDownStateName))
        {
            animationController.speed = -1f;
            animationController.Play(sitDownStateName, sitAnimLayer, 1f);
            animationController.Update(0f);
            _sitPhase = SitPhase.StandUpPlaying;
            return;
        }

        animationController.speed = 1f;
        ForceReturnToLocomotion();
        FinishStandUp();
    }

    private void TickStandUpPlaying(float dt)
    {
        if (animationController == null)
        {
            FinishStandUp();
            return;
        }

        AnimatorStateInfo info = animationController.GetCurrentAnimatorStateInfo(sitAnimLayer);

        if (!animationController.IsInTransition(sitAnimLayer) && info.normalizedTime <= 0.02f)
        {
            animationController.speed = 1f;
            ForceReturnToLocomotion();
            FinishStandUp();
        }
    }

    private void FinishStandUp()
    {
        if (agent != null)
        {
            agent.enabled = true;
            if (agent.isActiveAndEnabled)
                agent.Warp(_seatNavPos);
        }

        ReleaseSeatIfAny();

        _hasCommand = false;
        _commandGoal = NPCState.Patrolling;

        _sitPhase = SitPhase.None;
        EnterState(NPCState.Patrolling);
    }

    private void FailSittingAndReturnToPatrol()
    {
        ReleaseSeatIfAny();

        _hasCommand = false;
        _commandGoal = NPCState.Patrolling;

        _sitPhase = SitPhase.None;
        EnterState(NPCState.Patrolling);
    }

    private void ReleaseSeatIfAny()
    {
        if (_seat != null)
            _seat.Release(this);

        _seat = null;
        _seatTf = null;
    }

    // =========================================================
    // Crowd avoidance / renegotiate
    // =========================================================

    void PreCollisionRenegotiate()
    {
        if (!IsNavDriven()) return;
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        if (useStateMachine && (_state == NPCState.Alerted || _state == NPCState.Attacking || _state == NPCState.Talk || _state == NPCState.Sitting))
            return;

        if (agent.pathPending) return;
        if (!agent.hasPath) return;

        if (_renegotiateCooldownT > 0f)
        {
            _renegotiateCooldownT -= Time.deltaTime;
            return;
        }

        _renegotiateT += Time.deltaTime;
        if (_renegotiateT < renegotiateCheckInterval) return;
        _renegotiateT = 0f;

        if (agent.velocity.sqrMagnitude < 0.0004f) return;

        Vector3 v = agent.velocity;
        v.y = 0f;
        if (v.sqrMagnitude < 0.0004f) return;

        Vector3 fwd = v.normalized;
        Vector3 center = transform.position;

        Collider[] hits = Physics.OverlapSphere(center, personalSpaceRadius, npcLayerMask, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0) return;

        NPCController closest = null;
        float bestDist = float.PositiveInfinity;

        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i];
            if (!c) continue;

            var other = c.GetComponentInParent<NPCController>();
            if (!other || other == this) continue;
            if (!other.agent || !other.agent.isOnNavMesh) continue;

            Vector3 toOther = other.transform.position - center;
            toOther.y = 0f;
            float d = toOther.magnitude;
            if (d < 0.0001f) continue;

            float dot = Vector3.Dot(fwd, toOther / d);
            if (dot < inFrontDotThreshold) continue;

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
        if (agent == null || !agent.isOnNavMesh) yield break;

        int sign = ((gameObject.GetInstanceID() & 1) == 0) ? 1 : -1;

        Vector3 v = agent.velocity; v.y = 0f;
        Vector3 fwd = (v.sqrMagnitude > 0.001f) ? v.normalized : transform.forward;

        Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;
        Vector3 sidestep = right * (sidestepDistance * sign);

        Vector3 p0 = transform.position + sidestep;
        Vector3 p1 = transform.position - sidestep;

        Vector3 chosen;
        if (!TrySampleNavPoint(p0, out chosen))
        {
            if (!TrySampleNavPoint(p1, out chosen))
                yield break;
        }

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

        if (IsNavDriven() && _hasResumeDestination && agent.isOnNavMesh)
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

    // =========================================================
    // Triggered Animations API
    // =========================================================

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

        if (_triggerCoroutine != null)
        {
            StopCoroutine(_triggerCoroutine);
            _triggerCoroutine = null;
        }

        _triggerCoroutine = StartCoroutine(TriggerRoutine(triggerParam));
    }

    public void PauseTriggeredAnimation()
    {
        if (animationController == null) return;
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

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            agent.isStopped = _triggerWasStoppedBefore;

        if (useStateMachine && !isPaused)
            ForceReturnToLocomotion();
    }

    IEnumerator TriggerRoutine(string triggerParam)
    {
        _isPlayingTriggeredAnimation = true;

        _triggerPrevPaused = isPaused;

        _triggerAgentWasValid = (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh);
        _triggerHadPathBefore = _triggerAgentWasValid && agent.hasPath;
        _triggerWasStoppedBefore = _triggerAgentWasValid && agent.isStopped;

        isPaused = true;

        if (_triggerAgentWasValid)
            agent.isStopped = true;

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
            Debug.LogWarning($"{name}: Animator has no parameter named '{triggerParam}'.");
            isPaused = _triggerPrevPaused;

            if (_triggerAgentWasValid)
                agent.isStopped = _triggerWasStoppedBefore;

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

            if (entered && !animationController.IsInTransition(layer) && info.fullPathHash == triggeredHash)
            {
                if (info.normalizedTime >= 1f)
                    break;
            }

            if (entered && !animationController.IsInTransition(layer) && info.fullPathHash == startHash)
                break;

            t += Time.deltaTime;
            yield return null;
        }

        if (triggerExitBuffer > 0f)
            yield return new WaitForSeconds(triggerExitBuffer);

        SetAnimatorParamOff(animationController, triggerParam, pType.Value);
        if (pType.Value == AnimatorControllerParameterType.Trigger)
            animationController.ResetTrigger(triggerParam);

        isPaused = _triggerPrevPaused;

        if (_triggerAgentWasValid)
            agent.isStopped = _triggerWasStoppedBefore;

        _isPlayingTriggeredAnimation = false;
        _triggerCoroutine = null;

        if (useStateMachine && !isPaused)
            ForceReturnToLocomotion();
    }

    AnimatorControllerParameterType? GetAnimatorParameterType(Animator anim, string paramName)
    {
        var ps = anim.parameters;
        for (int i = 0; i < ps.Length; i++)
            if (ps[i].name == paramName) return ps[i].type;
        return null;
    }

    void SetAnimatorParamOn(Animator anim, string paramName, AnimatorControllerParameterType type)
    {
        switch (type)
        {
            case AnimatorControllerParameterType.Bool: anim.SetBool(paramName, true); break;
            case AnimatorControllerParameterType.Trigger:
                anim.ResetTrigger(paramName);
                anim.SetTrigger(paramName);
                break;
            case AnimatorControllerParameterType.Int: anim.SetInteger(paramName, 1); break;
            case AnimatorControllerParameterType.Float: anim.SetFloat(paramName, 1f); break;
        }
    }

    void SetAnimatorParamOff(Animator anim, string paramName, AnimatorControllerParameterType type)
    {
        switch (type)
        {
            case AnimatorControllerParameterType.Bool: anim.SetBool(paramName, false); break;
            case AnimatorControllerParameterType.Trigger: anim.ResetTrigger(paramName); break;
            case AnimatorControllerParameterType.Int: anim.SetInteger(paramName, 0); break;
            case AnimatorControllerParameterType.Float: anim.SetFloat(paramName, 0f); break;
        }
    }

    // -----------------------
    // NavMesh helpers
    // -----------------------

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
            Debug.LogWarning($"{name}: Agent is not on a NavMesh ({context}).");
            return false;
        }

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, snapToNavMeshRadius, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            return agent.isOnNavMesh;
        }

        Debug.LogWarning($"{name}: Could not find NavMesh within {snapToNavMeshRadius}m to snap ({context}).");
        return false;
    }

    // FIX: HasArrived no longer returns true when hasPath is false.
    // When hasPath is false it means either the path was never computed
    // or was reset — not that the agent arrived. Use plain distance checks
    // (as in TickApproachFront) for situations where the path may auto-clear.
    static bool HasArrived(NavMeshAgent a, float arriveDist)
    {
        if (a == null) return false;
        if (!a.isOnNavMesh) return false;
        if (a.pathPending) return false;
        if (!a.hasPath) return false;
        if (float.IsInfinity(a.remainingDistance)) return false;

        return a.remainingDistance <= Mathf.Max(arriveDist, a.stoppingDistance);
    }

    // -----------------------
    // Animator driving
    // -----------------------

    void ForceIdlePose()
    {
        if (animationController == null) return;

        animationController.SetFloat(paramMovingX, 0f, animDampTime, Time.deltaTime);
        animationController.SetFloat(paramMovingY, 0f, animDampTime, Time.deltaTime);
        animationController.SetFloat(paramBlend, 0f, animDampTime, Time.deltaTime);
    }

    void UpdateAnimatorFromMovement()
    {
        if (animationController == null) return;

        Vector3 vel = Vector3.zero;

        bool inSitWalkup = (_state == NPCState.Sitting && _sitPhase == SitPhase.ApproachingFront);

        // Only kill motion driving once we're past the walk-up phase
        if (_state == NPCState.Sitting && !inSitWalkup)
        {
            vel = Vector3.zero;
            _animVelocitySmoothed = Vector3.zero;
        }
        else if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            Vector3 v = agent.velocity;
            Vector3 dv = agent.desiredVelocity;

            bool isApproachLike =
                useStateMachine &&
                (_state == NPCState.Approaching || _state == NPCState.Seeking || _state == NPCState.Patrolling || inSitWalkup);

            // If path is pending or velocity hasn't kicked in yet, use desiredVelocity so anim starts immediately
            if (isApproachLike && (agent.pathPending || (v.sqrMagnitude < 0.0004f && dv.sqrMagnitude > 0.0004f)))
                vel = dv;
            else
                vel = v;
        }

        float smooth = (animDampTime <= 0.0001f) ? 0.0001f : animDampTime;
        _animVelocitySmoothed = Vector3.Lerp(_animVelocitySmoothed, vel, Time.deltaTime / smooth);

        Vector3 localVel = transform.InverseTransformDirection(_animVelocitySmoothed);
        float speed = _animVelocitySmoothed.magnitude;

        float movingY = Mathf.Clamp(localVel.z, -1f, 1f);
        float movingX = Mathf.Clamp(localVel.x, -1f, 1f);
        float blend = (moveSpeed <= 0.001f) ? 0f : Mathf.Clamp01(speed / moveSpeed);

        // Do not zero params during the sitting walk-up; only after we start align/backstep/sit.
        if (!IsNavDriven() || (_state == NPCState.Sitting && !inSitWalkup))
        {
            movingX = 0f;
            movingY = 0f;
            blend = 0f;
        }

        animationController.SetFloat(paramMovingX, movingX, animDampTime, Time.deltaTime);
        animationController.SetFloat(paramMovingY, movingY, animDampTime, Time.deltaTime);
        animationController.SetFloat(paramBlend, blend, animDampTime, Time.deltaTime);
    }

    void UpdateFacing()
    {
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;
        if (!IsNavDriven()) return;

        bool inSitWalkup = (_state == NPCState.Sitting && _sitPhase == SitPhase.ApproachingFront);

        // Skip facing only for "locked" states — but allow it during Sitting walk-up
        if (useStateMachine && (_state == NPCState.Alerted || _state == NPCState.Attacking || _state == NPCState.Talk))
            return;

        if (useStateMachine && _state == NPCState.Sitting && !inSitWalkup)
            return;

        // Prefer desiredVelocity so we turn immediately (velocity can lag)
        Vector3 v = agent.desiredVelocity;
        if (v.sqrMagnitude < 0.0004f) v = agent.velocity;

        v.y = 0f;
        if (v.sqrMagnitude < 0.0004f) return;

        Quaternion targetRot = Quaternion.LookRotation(v.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSmoothing);
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
        {
            npc.selectedTriggerIndex = EditorGUILayout.Popup(
                "Trigger",
                npc.selectedTriggerIndex,
                npc.triggerNames.ToArray()
            );
        }
        else
        {
            EditorGUILayout.HelpBox("Add at least one trigger name to 'triggerNames'.", MessageType.Info);
        }

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
    }
}
#endif
