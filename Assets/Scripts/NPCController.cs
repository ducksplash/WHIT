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

    [Header("Identity")] public NPC thisNPC = NPC.Eimear_Scott;

    [Header("Startup Behaviour")]
    public bool FindSeat;
    public bool PlayAnimation;
    public string startupAnimationTrigger;

    [Header("AI State Machine")] public bool useStateMachine = true;
    public Transform currentTarget;
    public float activeRadius = 15f;

    [Header("Testing / Overrides")] public bool allowApproachOutsideActiveRadius;
    public NPC debugTargetNPC = NPC.Eimear_Scott;

    [Header("Perception")] public float visionRange = 12f;
    [Range(0f, 360f)] public float visionFOV = 120f;
    public bool requireLineOfSight;
    public LayerMask losBlockers = ~0;

    [Header("Combat")] public float attackRange = 1.8f;
    public float alertedDuration = 0.35f;

    [Header("Interaction / Talk")] public float talkRange = 4.0f;

    [Header("Patrol")] public Vector2 patrolChangeDirInterval = new Vector2(3f, 7f);
    public Vector2 patrolArrivePause = new Vector2(0.2f, 1.0f);
    public int patrolPointTries = 10;
    public float patrolSampleRadius = 2.0f;

    [Header("Predefined Patrol")] public bool usePredefinedPatrol;
    public List<Transform> predefinedPatrolPoints = new List<Transform>();
    
    [Tooltip("Wait time after reaching a waypoint before moving to the next one.\nX = min, Y = max. Set both to the same value for a fixed wait.")]
    public Vector2 predefinedPatrolArrivePause = new Vector2(0.5f, 2.0f);
    
    
    [Header("Seeking")] public float seekingSpeedMultiplier = 1.2f;
    public float seekGiveUpSeconds = 6f;
    public float approachRepathInterval = 0.25f;

    [Header("Debug (Runtime)")] [SerializeField]
    private NPCState _state = NPCState.Patrolling;

    [Header("Approach Smoothing")] public float approachTargetMoveThreshold = 0.35f;
    public bool disableBrakingWhileApproaching = true;

    public NPCManager sceneNPCManager;

    [Header("Turn / BlendTree Driving")] [SerializeField]
    private float turnAngleForFullX = 90f;
    [SerializeField] private float turnInPlaceSpeed = 160f;
    [SerializeField] private float turnWhileMovingSpeed = 260f;
    [SerializeField] private float turnInPlaceSpeedThreshold = 0.15f;

    [Header("Components")] public Animator animationController;
    public NavMeshAgent agent;

    [Header("Arrival / Blending")] public float arriveDistance = 0.3f;

    [Header("Movement")] public float moveSpeed = 3f;
    public float angularSpeed = 240f;
    public float acceleration = 8f;

    [Header("Smoothing")] public float animDampTime = 0.15f;
    public float turnSmoothing = 10f;

    [Header("NavMesh Placement")] public float snapToNavMeshRadius = 2f;
    public bool autoSnapToNavMeshOnGo = true;

    [Header("Animator Params")] public string paramBlend = "Blend";
    public string paramMovingX = "MovingX";
    public string paramMovingY = "MovingY";

    [Header("Triggered Animations")] public List<string> triggerNames = new List<string>();
    [HideInInspector] public int selectedTriggerIndex;
    public float triggerEnterTimeout = 1.0f;
    public float triggerMaxDuration = 8.0f;
    public float triggerExitBuffer = 0.05f;
    
    [Header("Upper Body Animations")]
    public List<string> upperBodyTriggerNames = new List<string>();
    [HideInInspector] public int selectedUpperBodyTriggerIndex;
    [Tooltip("Exact name of the Animator layer used for upper-body-only animations.")]
    public string upperBodyLayerName = "UpperLayer";
    [Tooltip("State on the upper body layer to crossfade back to when the animation finishes. " + "Usually an empty clip or a neutral pose. Leave blank to skip the return crossfade.")]
    public string upperBodyReturnStateName = "";

    [Header("Locomotion / Blend Tree Return")] [SerializeField]
    private string locomotionStateName = "Locomotion";
    [SerializeField] private int locomotionLayer;
    [SerializeField] private float locomotionCrossfade = 0.12f;

    [Header("Crowd Avoidance")] public LayerMask npcLayerMask = ~0;
    public float personalSpaceRadius = 0.9f;
    [Range(-1f, 1f)] public float inFrontDotThreshold = 0.35f;
    public float sidestepDistance = 0.9f;
    public float sidestepDuration = 0.7f;
    public float renegotiateCooldown = 0.8f;
    public float renegotiateCheckInterval = 0.15f;

    public Me npcMetaActions;

    [Header("Behaviour Plugins")] [SerializeField]
    private NPCLadderBehaviour  _ladder;
    [SerializeField] private NPCSittingBehaviour _sitting;
    [SerializeField] private NPCLyingBehaviour _lying;
    [SerializeField] private NPCTalkBehaviour _talk;
    [SerializeField] private NPCCombatBehaviour _combat;

    [NonSerialized] public bool isPaused;

    // Off-mesh link manual traversal
    private Coroutine _offMeshLinkCoroutine;
    private bool _isTraversingOffMeshLink;
    private Vector3 _offMeshLinkFakeVelocity; // drives animator during traversal
    public bool IsTraversingOffMeshLink => _isTraversingOffMeshLink;
    
    private enum PatrolAction
    {
        None,
        Sit,
        Sleep,
        Talk
    }

    [Header("Autonomous Patrol Actions")]
    public bool CanSit;
    public bool CanSleep;
    public bool CanTalk;

    [Min(0f)] public float MinSitTime = 20f;
    [Min(0f)] public float MaxSitTime = 180f;

    [Min(0f)] public float MinSleepTime = 25f;
    [Min(0f)] public float MaxSleepTime = 30f;

    [Min(0f)] public float MinTalkTime = 20f;
    [Min(0f)] public float MaxTalkTime = 80f;

    [Tooltip("How often patrol considers doing a random autonomous action.")]
    public Vector2 patrolActionCheckInterval = new Vector2(6f, 14f);

    [Tooltip("Cooldown after finishing an action before that same action can be chosen again.")]
    public float patrolActionCooldownSeconds = 240f;

    [Tooltip("Short cooldown after a failed attempt (e.g. no free seat/bed found).")]
    public float patrolActionFailedRetrySeconds = 20f;

    [Tooltip("How far to search for another NPC to talk to.")]
    public float autonomousTalkSearchRadius = 8f;

    private float _patrolActionTimer;
    private float _sitCooldownT;
    private float _sleepCooldownT;
    private float _talkCooldownT;

    private Coroutine _autoActionCoroutine;
    
    
    public bool HasCommand { get => _hasCommand; set => _hasCommand = value; }
    public NPCState CommandGoal { get => _commandGoal; set => _commandGoal = value; }
    public Vector3 SpawnPoint => _spawnPoint;
    public Vector3 AnimVelocitySmoothed { get => _animVelocitySmoothed; set => _animVelocitySmoothed = value; }
    public NPCTalkBehaviour Talk => _talk;

    // ── Ladder pass-throughs (Sitting/Lying call these on npc directly) ────────
    public bool  IsTraversingLadder          => _ladder != null && _ladder.IsTraversingLadder;
    public float ladderApproachArriveDistance => _ladder != null ? _ladder.ladderApproachArriveDistance : 0.45f;

    // ── Locomotion accessors for NPCLadderBehaviour ────────────────────────────
    public string LocomotionStateName => locomotionStateName;
    public int    LocomotionLayer     => locomotionLayer;

    private Vector3 _spawnPoint;
    private Vector3 _patrolDestination;
    private bool _hasPatrolDestination;
    private int _predefinedPatrolIndex;

    private float _stateTimer;
    private float _patrolChangeTimer;
    private float _patrolArriveTimer;
    private float _approachRepathTimer;
    private float _seekTimer;

    private Vector3 _lastKnownTargetPos;
    private bool _hasLastKnownTargetPos;

    private bool _hasCommand = false;
    private NPCState _commandGoal = NPCState.Patrolling;

    private Action _pendingPostStandAction;
    private bool _executePendingActionAfterStand = false;

    private Vector3 _animVelocitySmoothed = Vector3.zero;
    private Vector3 _resumeDestination;
    private bool _hasResumeDestination;

    private Coroutine _triggerCoroutine;
    private bool _isPlayingTriggeredAnimation = false;
    private bool _triggerPrevPaused;
    private bool _triggerAgentWasValid;
    private bool _triggerWasStoppedBefore;
    private bool _triggerAgentWasEnabled;
    private Coroutine _upperBodyCoroutine;
    private bool _isPlayingUpperBodyAnimation;

    private float _renegotiateT;
    private float _renegotiateCooldownT;
    private Coroutine _sidestepCoroutine;

    
    

    [SerializeField] private bool hasStartupActions;
    [SerializeField] private bool startupActionsConcluded;
    
    
    
    public bool AgentReady() => agent != null && agent.isActiveAndEnabled && agent.enabled && agent.isOnNavMesh;

    public bool TryGetNavmeshPoint(Vector3 near, out Vector3 navPos)
    {
        navPos = near;
        if (NavMesh.SamplePosition(near, out NavMeshHit hit, snapToNavMeshRadius, NavMesh.AllAreas))
        {
            navPos = hit.position;
            return true;
        }

        return false;
    }

    public bool TryGetNavmeshPointNear(Vector3 near, float radius, out Vector3 navPos)
    {
        navPos = near;
        if (NavMesh.SamplePosition(near, out NavMeshHit hit, radius, NavMesh.AllAreas))
        {
            navPos = hit.position;
            return true;
        }

        return false;
    }

    public bool CanReachPosition(Vector3 destination, out NavMeshPath path)
    {
        path = new NavMeshPath();
        if (!AgentReady()) return false;
        bool ok = agent.CalculatePath(destination, path);
        if (!ok || path == null) return false;
        return path.status == NavMeshPathStatus.PathComplete;
    }

    public bool CanReachPosition(Vector3 destination)
    {
        return CanReachPosition(destination, out _);
    }

    public bool CanReachBetween(Vector3 start, Vector3 destination, out NavMeshPath path,
        float snapRadius = 2f)
    {
        path = new NavMeshPath();
        if (!TryGetNavmeshPointNear(start,       snapRadius, out Vector3 startNav)) return false;
        if (!TryGetNavmeshPointNear(destination, snapRadius, out Vector3 destNav))  return false;
        int areaMask = (agent != null) ? agent.areaMask : NavMesh.AllAreas;
        bool ok = NavMesh.CalculatePath(startNav, destNav, areaMask, path);
        return ok && path.status == NavMeshPathStatus.PathComplete;
    }

    // ── Ladder pass-throughs ───────────────────────────────────────────────────
    // Full logic lives in NPCLadderBehaviour. These keep the public API stable
    // so NPCSittingBehaviour and NPCLyingBehaviour need no changes.

    public bool TryFindLadderRoute(
        Vector3 destination, out Ladder ladder, out bool goingUp,
        out Vector3 approachPoint, out Vector3 exitPoint, out NavMeshPath approachPath)
    {
        if (_ladder != null)
            return _ladder.TryFindLadderRoute(destination, out ladder, out goingUp,
                out approachPoint, out exitPoint, out approachPath);

        ladder = null; goingUp = false;
        approachPoint = exitPoint = Vector3.zero; approachPath = null;
        return false;
    }

    public void StartLadderTraversal(Ladder ladder, bool goingUp, Action onComplete)
    {
        if (_ladder != null) _ladder.StartLadderTraversal(ladder, goingUp, onComplete);
        else                 onComplete?.Invoke();
    }
    public void ForceUpright()
    {
        Vector3 e = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, e.y, 0f);
    }

    public bool GroundTransformToNavmesh(Vector3 preferred, float sampleRadius = 3f)
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

    public bool RestoreStandingBodyAt(Vector3 preferred, float sampleRadius)
    {
        if (!GroundTransformToNavmesh(preferred, sampleRadius)) return false;
        if (agent == null) return true;
        if (!agent.enabled) agent.enabled = true;

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

    public void DetachAgentForAnimation()
    {
        if (agent == null)
            return;

        if (!agent.enabled)
            agent.enabled = true;

        // Stop navigation logic
        agent.isStopped = true;
        agent.ResetPath();

        // Prevent NavMeshAgent from overriding transform movement
        agent.updatePosition = false;
        agent.updateRotation = false;

        // Keep internal nav position synced
        agent.nextPosition = transform.position;

        // Optional safety
        agent.velocity = Vector3.zero;
    }

    public void ReattachAgentToNavmeshAtCurrentXZ()
    {
        if (agent == null)
            return;

        if (!agent.enabled)
            agent.enabled = true;

        Vector3 bodyPos = transform.position;

        // Find nearest valid navmesh position
        if (NavMesh.SamplePosition(bodyPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            // IMPORTANT:
            // Warp updates the agent's INTERNAL navmesh position.
            agent.Warp(hit.position);

            // Re-enable normal navmesh syncing
            agent.updatePosition = true;
            agent.updateRotation = true;

            agent.nextPosition = hit.position;

            agent.isStopped = false;
            agent.velocity = Vector3.zero;
        }
        else
        {
            Debug.LogWarning($"{name}: Failed to reattach agent to navmesh.");
        }
    }

    public Quaternion GetPlanarLookRotation(Vector3 forward)
    {
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f) forward = transform.forward;
        forward.Normalize();
        return Quaternion.LookRotation(forward, Vector3.up);
    }

    public void ForceReturnToLocomotion()
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

    public void ForceIdlePose()
    {
        if (animationController == null) return;

        animationController.SetFloat(paramMovingX, 0f, animDampTime, Time.deltaTime);
        animationController.SetFloat(paramMovingY, 0f, animDampTime, Time.deltaTime);
        animationController.SetFloat(paramBlend, 0f, animDampTime, Time.deltaTime);
    }

    public void ResetAllAnimatorTriggers()
    {
        if (animationController == null) return;

        foreach (var p in animationController.parameters)
            if (p.type == AnimatorControllerParameterType.Trigger)
                animationController.ResetTrigger(p.name);
    }

    public void SetStateDirectly(NPCState next) { _state = next; }

    public void ExecutePendingPostStandAction()
    {
        if (!_executePendingActionAfterStand)
        {
            EnterState(GetDefaultWanderState());
            return;
        }

        _executePendingActionAfterStand = false;
        Action action = _pendingPostStandAction;
        _pendingPostStandAction = null;
        action?.Invoke();
    }

    void Reset()
    {
        animationController = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Awake()
    {
        DirectorEvents.OnNPCCommand += ExecuteNPCCommand;
        DirectorEvents.OnNPCUpperBodyAnimation += PlayUpperBodyTrigger;
        
        
        var mgr = FindFirstObjectByType<NPCManager>();
        if (mgr != null) mgr.RegisterNPC(this);
        else Debug.LogWarning($"{name}: No NPCManager found during Awake.");
    }

    public void Start()
    {

        
        
        if (animationController == null) animationController = GetComponentInChildren<Animator>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        if (_ladder  == null) _ladder  = GetComponent<NPCLadderBehaviour>();
        if (_sitting == null) _sitting = GetComponent<NPCSittingBehaviour>();
        if (_lying == null) _lying = GetComponent<NPCLyingBehaviour>();
        if (_talk == null) _talk = GetComponent<NPCTalkBehaviour>();
        if (_combat == null) _combat = GetComponent<NPCCombatBehaviour>();
        if (npcMetaActions == null) npcMetaActions = GetComponent<Me>();

        _ladder?.Init(this);
        _sitting?.Init(this);
        _lying?.Init(this);
        _talk?.Init(this);
        _combat?.Init(this);

        //if (animationController != null) animationController.applyRootMotion = false;

        _spawnPoint = transform.position;

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.autoTraverseOffMeshLink = false;
            agent.angularSpeed = angularSpeed;
            agent.acceleration = acceleration;
            agent.stoppingDistance = Mathf.Max(agent.stoppingDistance, arriveDistance);
            agent.autoBraking = true;
            agent.autoRepath = true;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.updateRotation = false;
            agent.avoidancePriority = Mathf.Clamp((Mathf.Abs(gameObject.GetInstanceID()) % 90) + 5, 0, 99);
        }

        if (useStateMachine)
        {
            ForceReturnToLocomotion();
            EnterState(GetDefaultWanderState());
        }

        
        StartCoroutine(StartupActions());
    }

    
    
    
    private IEnumerator StartupActions()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.5f));

        if (FindSeat)
        {
            RequestSitDown();
            hasStartupActions = true;
        }

        if (PlayAnimation && !string.IsNullOrWhiteSpace(startupAnimationTrigger))
        {
            PlayTrigger(startupAnimationTrigger);
            hasStartupActions = true;
        }

        yield return new WaitForEndOfFrame();

        if (hasStartupActions && FindSeat)
        {
            StartCoroutine(ActionObserver());
        }
    }

    private IEnumerator ActionObserver()
    {
        RequestSitDown();

        while (!startupActionsConcluded)
        {
            yield return new WaitForSeconds(1f);

            var phase = _sitting.Phase;

            if (phase == NPCSittingBehaviour.SitPhase.SittingIdle)
            {
                startupActionsConcluded = true;
                yield break;
            }

            // Only retry if system is idle (not already trying)
            if (phase == NPCSittingBehaviour.SitPhase.None)
            {
                RequestSitDown();
            }
        }
    }
    
    
    private void ExecuteNPCCommand(NPC npc, NPCState npcState)
    {
        if (thisNPC == npc)
        {
            EnterState(npcState);
        }
    }
    
    
    void Update()
    {
        if (_talk != null && _talk.IsConversationLocked)
        {
            _talk.UpdateConversationLocked(Time.deltaTime);
            UpdateLocomotionAndFacing();
            return;
        }

        if (!IsTraversingLadder && !_isPlayingTriggeredAnimation
                                && AgentReady() && agent.isOnOffMeshLink
                                && _offMeshLinkCoroutine == null)
        {
            _offMeshLinkCoroutine = StartCoroutine(TraverseOffMeshLinkRoutine());
        }
        
        if (useStateMachine && !_isPlayingTriggeredAnimation && !IsTraversingLadder)
        {
            if (_state == NPCState.Sitting ||
                _state == NPCState.Lying ||
                _state == NPCState.SeekingSeat ||
                _state == NPCState.SeekingBed)
            {
                TickFSM(Time.deltaTime);
            }
            else if (EnsureFSMCanRun())
            {
                TickFSM(Time.deltaTime);
            }
        }

        UpdateLocomotionAndFacing();
        PreCollisionRenegotiate();
    }

    bool EnsureFSMCanRun()
    {
        if (agent == null || !agent.isActiveAndEnabled) return false;
        if (!EnsureAgentOnNavMesh("FSM")) return false;
        return true;
    }

    public NPCState GetCurrentState() => _state;

    private NPCState GetDefaultWanderState()
    {
        return (usePredefinedPatrol && predefinedPatrolPoints != null && predefinedPatrolPoints.Count > 0)
            ? NPCState.PredefinedPatrol
            : NPCState.Patrolling;
    }

    public void SetTarget(Transform t)
    {
        currentTarget = t;
        if (currentTarget != null)
        {
            _lastKnownTargetPos = currentTarget.position;
            _hasLastKnownTargetPos = true;

            if (useStateMachine && !_isPlayingTriggeredAnimation && !_hasCommand
                && _state != NPCState.SeekingSeat
                && _state != NPCState.Sitting
                && _state != NPCState.SeekingBed
                && _state != NPCState.Lying)
            {
                EnterState(NPCState.Alerted);
            }
        }
    }

    public void ClearTarget()
    {
        _talk?.UnregisterAsSpeaker();
        currentTarget = null;
        _hasCommand = false;
        _hasResumeDestination = false;
        _resumeDestination = Vector3.zero;
        _commandGoal = NPCState.Patrolling;
    }


    private IEnumerator TraverseOffMeshLinkRoutine()
    {
        _isTraversingOffMeshLink = true;

        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 startPos = transform.position;
        Vector3 endPos = data.endPos;

        // Snap end position to navmesh surface
        if (NavMesh.SamplePosition(endPos, out NavMeshHit hit, snapToNavMeshRadius, NavMesh.AllAreas))
            endPos = hit.position;

        Vector3 dir = endPos - startPos;
        dir.y = 0f;
        Vector3 dirNorm = dir.sqrMagnitude > 0.0001f ? dir.normalized : transform.forward;

        float distance = Vector3.Distance(startPos, endPos);
        float duration = distance / Mathf.Max(0.01f, agent.speed);
        float elapsed = 0f;

        // Pre-face the link direction before moving
        while (Mathf.Abs(Mathf.DeltaAngle(
                   transform.eulerAngles.y,
                   Quaternion.LookRotation(dirNorm, Vector3.up).eulerAngles.y)) > 8f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(dirNorm, Vector3.up),
                turnWhileMovingSpeed * Time.deltaTime);
            yield return null;
        }

        // Walk across the link at moveSpeed
        _offMeshLinkFakeVelocity = dirNorm * agent.speed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 pos = Vector3.Lerp(startPos, endPos, t);
            transform.position = pos;
            agent.nextPosition = pos; // keeps agent's internal position synced

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(dirNorm, Vector3.up),
                turnWhileMovingSpeed * Time.deltaTime);

            yield return null;
        }

        // Snap to end, complete link
        transform.position = endPos;
        agent.CompleteOffMeshLink();
        agent.nextPosition = endPos;

        _offMeshLinkFakeVelocity = Vector3.zero;

        // One settle frame before releasing — prevents the FSM reading stale navmesh state
        yield return null;

        _isTraversingOffMeshLink = false;
        _offMeshLinkCoroutine = null;
    }

    public void ForcePatrol()
    {
        if (_autoActionCoroutine != null)
        {
            StopCoroutine(_autoActionCoroutine);
            _autoActionCoroutine = null;
        }
        
        if (TryQueueActionAfterStand(() =>
            {
                InterruptAllTransientActions();
                if (animationController != null) animationController.speed = 1f;
                useStateMachine = true;
                _hasCommand = false;
                _commandGoal = NPCState.Patrolling;

                if (agent != null && agent.isActiveAndEnabled)
                {
                    if (!agent.enabled) agent.enabled = true;
                    if (!agent.isOnNavMesh && TryGetNavmeshPoint(transform.position, out Vector3 np)) agent.Warp(np);
                }

                _hasResumeDestination = false;
                _resumeDestination = Vector3.zero;
                ResetLocomotionState();
                ForceReturnToLocomotion();
                EnterState(GetDefaultWanderState());
            })) return;

        InterruptAllTransientActions();
        if (animationController != null) animationController.speed = 1f;
        useStateMachine = true;
        _hasCommand = false;
        _commandGoal = NPCState.Patrolling;

        if (agent != null && agent.isActiveAndEnabled)
        {
            if (!agent.enabled) agent.enabled = true;
            if (!agent.isOnNavMesh && TryGetNavmeshPoint(transform.position, out Vector3 navPos)) agent.Warp(navPos);
        }

        _hasResumeDestination = false;
        _resumeDestination = Vector3.zero;
        ResetLocomotionState();
        ForceReturnToLocomotion();
        EnterState(GetDefaultWanderState());
    }

    public void RequestSitDown()
    {
        if (_talk != null && _talk.IsConversationLocked) return;
        if (_sitting == null)
        {
            Debug.LogWarning($"{name}: No NPCSittingBehaviour found.");
            return;
        }

        InterruptAllTransientActions();
        useStateMachine = true;
        _hasCommand = true;
        _commandGoal = NPCState.Sitting;
        EnterState(NPCState.SeekingSeat);
    }

    public void RequestStandUp()
    {
        if ((_state != NPCState.SeekingSeat && _state != NPCState.Sitting) || _sitting == null) return;
        
        
        var p = _sitting.Phase;

        if (p == NPCSittingBehaviour.SitPhase.SearchingSeat ||
            p == NPCSittingBehaviour.SitPhase.RoutingToLadder ||
            p == NPCSittingBehaviour.SitPhase.ClimbingLadder ||
            p == NPCSittingBehaviour.SitPhase.ApproachingFront ||
            p == NPCSittingBehaviour.SitPhase.Aligning ||
            p == NPCSittingBehaviour.SitPhase.Backstepping)
        {
            ForcePatrol();
            return;
        }

        if (p == NPCSittingBehaviour.SitPhase.SittingIdle ||
            p == NPCSittingBehaviour.SitPhase.SitDownPlaying)
        {
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            _sitting.BeginStandUp();
        }
    }

    public void RequestLieDown()
    {
        if (_talk != null && _talk.IsConversationLocked) return;
        if (_lying == null)
        {
            Debug.LogWarning($"{name}: No NPCLyingBehaviour found.");
            return;
        }

        if (TryQueueActionAfterStand(StartLieCommandFresh)) return;
        StartLieCommandFresh();
    }

    public void RequestWakeUp()
    {
        if ((_state != NPCState.SeekingBed && _state != NPCState.Lying) || _lying == null) return;
        
        var p = _lying.Phase;
        if (p == NPCLyingBehaviour.LiePhase.LyingIdle || p == NPCLyingBehaviour.LiePhase.LieDownPlaying)
            _lying.BeginWakeUp();
        else if (p == NPCLyingBehaviour.LiePhase.SearchingBed ||
                 p == NPCLyingBehaviour.LiePhase.RoutingToLadder ||
                 p == NPCLyingBehaviour.LiePhase.ClimbingLadder ||
                 p == NPCLyingBehaviour.LiePhase.ApproachingFront ||
                 p == NPCLyingBehaviour.LiePhase.Aligning)
            ForcePatrol();
    }

    private void StartLieCommandFresh()
    {
        InterruptAllTransientActions();

        currentTarget = null;
        if (_talk != null)
        {
            _talk.RegisteredAsSpeaker = false;
            _talk.TalkTargetController = null;
        }

        useStateMachine = true;
        _hasCommand = true;
        _commandGoal = NPCState.Lying;
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

        EnterState(NPCState.SeekingBed);
    }

    public void SetTargetByNPC(NPC npcEnum)
    {
        var c = ResolveNPC(npcEnum);
        if (c == null)
        {
            Debug.LogWarning($"{name}: Could not find NPC '{npcEnum}'.");
            return;
        }

        if (c == this)
        {
            Debug.LogWarning($"{name}: Tried to target self.");
            return;
        }

        SetTarget(c.transform);
        if (_talk != null) _talk.TalkTargetController = c;
    }

    public void DebugApproachTargetNPC()
    {
        if (TryQueueActionAfterStand(() =>
            {
                InterruptAllTransientActions();
                useStateMachine = true;
                _hasCommand = false;
                _commandGoal = NPCState.Patrolling;
                SetTargetByNPC(debugTargetNPC);
                if (currentTarget != null) EnterState(NPCState.Approaching);
            })) return;

        InterruptAllTransientActions();
        useStateMachine = true;
        _hasCommand = false;
        _commandGoal = NPCState.Patrolling;
        SetTargetByNPC(debugTargetNPC);
        if (currentTarget != null) EnterState(NPCState.Approaching);
    }

    public void DebugAttackTargetNPC()
    {
        if (TryQueueActionAfterStand(() =>
            {
                InterruptAllTransientActions();
                useStateMachine = true;
                SetTargetByNPC(debugTargetNPC);
                if (currentTarget == null) return;
                _hasCommand = true;
                _commandGoal = NPCState.Attacking;
                float d = Vector3.Distance(transform.position, currentTarget.position);
                EnterState(d <= talkRange ? NPCState.Attacking : NPCState.Approaching);
            })) return;

        InterruptAllTransientActions();
        useStateMachine = true;
        SetTargetByNPC(debugTargetNPC);
        if (currentTarget == null) return;
        _hasCommand = true;
        _commandGoal = NPCState.Attacking;
        float d2 = Vector3.Distance(transform.position, currentTarget.position);
        EnterState(d2 <= talkRange ? NPCState.Attacking : NPCState.Approaching);
    }

    public void DebugTalkTargetNPC()
    {
        if (TryQueueActionAfterStand(() =>
            {
                InterruptAllTransientActions();
                useStateMachine = true;
                SetTargetByNPC(debugTargetNPC);
                if (currentTarget == null) return;
                _hasCommand = true;
                _commandGoal = NPCState.Talk;
                float d = Vector3.Distance(transform.position, currentTarget.position);
                EnterState(d <= talkRange ? NPCState.Talk : NPCState.Approaching);
            })) return;

        InterruptAllTransientActions();
        useStateMachine = true;
        SetTargetByNPC(debugTargetNPC);
        if (currentTarget == null) return;
        _hasCommand = true;
        _commandGoal = NPCState.Talk;
        float d3 = Vector3.Distance(transform.position, currentTarget.position);
        EnterState(d3 <= talkRange ? NPCState.Talk : NPCState.Approaching);
    }

    public void BeginConversationAsTarget(NPCController speaker)
        => _talk?.BeginConversationAsTarget(speaker);

    public void EndConversationAsTarget(NPCController speaker)
        => _talk?.EndConversationAsTarget(speaker);

    public void EnterState(NPCState next)
    {
        if (_isPlayingTriggeredAnimation) return;

        NPCState prev = _state;

        if (prev == NPCState.Talk && next != NPCState.Talk)
            _talk?.UnregisterAsSpeaker();

        _state = next;
        _stateTimer = 0f;

        if (AgentReady()) agent.isStopped = false;

        switch (_state)
        {
            case NPCState.Patrolling:
                if (AgentReady())
                {
                    agent.speed = moveSpeed;
                    agent.autoBraking = true;
                    agent.isStopped = false;
                }

                _hasPatrolDestination = false;
                _patrolChangeTimer = UnityEngine.Random.Range(patrolChangeDirInterval.x, patrolChangeDirInterval.y);
                _patrolArriveTimer = 0f;
                _patrolActionTimer = UnityEngine.Random.Range(patrolActionCheckInterval.x, patrolActionCheckInterval.y);
                break;

            case NPCState.PredefinedPatrol:
                if (AgentReady())
                {
                    agent.speed = moveSpeed;
                    agent.autoBraking = true;
                    agent.isStopped = false;
                }

                _hasPatrolDestination = false;
                _patrolArriveTimer = 0f;
                _predefinedPatrolIndex = 0;
                break;

            case NPCState.Alerted:
                if (AgentReady())
                {
                    agent.speed = moveSpeed;
                    agent.isStopped = true;
                    agent.autoBraking = true;
                }
                break;

            case NPCState.Approaching:
                if (AgentReady())
                {
                    agent.speed = moveSpeed;
                    if (disableBrakingWhileApproaching) agent.autoBraking = false;
                    agent.isStopped = false;
                }

                _approachRepathTimer = 0f;
                break;

            case NPCState.Attacking:
                if (AgentReady())
                {
                    agent.isStopped = true;
                    agent.autoBraking = true;
                }

                Debug.Log($"{name} ATTACKING: TODO hook up attack logic/animation.");
                break;

            case NPCState.Seeking:
                if (AgentReady())
                {
                    agent.speed = moveSpeed * Mathf.Max(0.01f, seekingSpeedMultiplier);
                    agent.isStopped = false;
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
                }

                Debug.Log($"{name} TALK: holding idle until new command.");
                break;

            case NPCState.SeekingSeat:
                if (prev != NPCState.SeekingSeat && prev != NPCState.Sitting)
                {
                    //agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
                    _sitting?.EnterSitting();
                }
                break;

            case NPCState.Sitting:
                break;

            case NPCState.SeekingBed:
                if (prev != NPCState.SeekingBed && prev != NPCState.Lying)
                    _lying?.EnterLying();
                break;

            case NPCState.Lying:
                break;

            case NPCState.ClimbingLadder:
                break;
        }
    }

    void TickFSM(float dt)
    {
        _stateTimer += dt;

        if (_sitCooldownT > 0f) _sitCooldownT -= dt;
        if (_sleepCooldownT > 0f) _sleepCooldownT -= dt;
        if (_talkCooldownT > 0f) _talkCooldownT -= dt;

        if (currentTarget != null)
        {
            _lastKnownTargetPos = currentTarget.position;
            _hasLastKnownTargetPos = true;
        }

        if (!_hasCommand &&
            _state != NPCState.Talk &&
            _state != NPCState.SeekingSeat &&
            _state != NPCState.Sitting &&
            _state != NPCState.SeekingBed &&
            _state != NPCState.Lying &&
            _state != NPCState.ClimbingLadder)
        {
            if (currentTarget != null && CanSeeTarget(currentTarget))
            {
                float distToSpawn = Vector3.Distance(
                    _spawnPoint,
                    currentTarget.position);

                if (distToSpawn > activeRadius &&
                    !allowApproachOutsideActiveRadius)
                {
                    if (_state != NPCState.Seeking)
                        EnterState(NPCState.Seeking);
                }
                else
                {
                    // PredefinedPatrol deliberately excluded here.
                    //
                    // A predefined patrol should continue following its
                    // waypoint sequence rather than being interrupted simply
                    // because the NPC can see its target.
                    if (_state == NPCState.Patrolling ||
                        _state == NPCState.Seeking)
                    {
                        EnterState(NPCState.Alerted);
                    }
                }
            }
        }

        switch (_state)
        {
            case NPCState.Patrolling:
                TickPatrolling(dt, 1f);
                break;

            case NPCState.PredefinedPatrol:
                TickPredefinedPatrol(dt);
                break;

            case NPCState.Alerted:
                TickAlerted(dt);
                break;

            case NPCState.Approaching:
                TickApproaching(dt);
                break;

            case NPCState.Attacking:
                _combat?.TickAttacking(dt);
                break;

            case NPCState.Seeking:
                TickSeeking(dt);
                break;

            case NPCState.SeekingSeat:

                if (_sitting != null &&
                    _sitting.Phase == NPCSittingBehaviour.SitPhase.SearchingSeat)
                {
                    TickPatrolMovementOnly(dt, 1f);
                }

                _sitting?.Tick(dt);

                if (_sitting == null)
                    break;

                switch (_sitting.Phase)
                {
                    case NPCSittingBehaviour.SitPhase.SitDownPlaying:
                    case NPCSittingBehaviour.SitPhase.SittingIdle:
                    case NPCSittingBehaviour.SitPhase.StandUpPlaying:
                        EnterState(NPCState.Sitting);
                        return;

                    case NPCSittingBehaviour.SitPhase.None:
                        EnterState(GetDefaultWanderState());
                        return;
                }

                break;

            case NPCState.Sitting:
                _sitting?.Tick(dt);

                if (_sitting == null)
                    break;

                switch (_sitting.Phase)
                {
                    case NPCSittingBehaviour.SitPhase.SearchingSeat:
                    case NPCSittingBehaviour.SitPhase.RoutingToLadder:
                    case NPCSittingBehaviour.SitPhase.ClimbingLadder:
                    case NPCSittingBehaviour.SitPhase.ApproachingFront:
                        EnterState(NPCState.SeekingSeat);
                        return;

                    case NPCSittingBehaviour.SitPhase.StandUpPlaying:
                        EnterState(NPCState.Sitting);
                        return;

                    case NPCSittingBehaviour.SitPhase.None:
                        EnterState(GetDefaultWanderState());
                        return;
                }

                break;

            case NPCState.SeekingBed:

                if (_lying != null &&
                    _lying.Phase == NPCLyingBehaviour.LiePhase.SearchingBed)
                {
                    TickPatrolMovementOnly(dt, 1f);
                }

                _lying?.Tick(dt);

                if (_lying == null)
                    break;

                switch (_lying.Phase)
                {
                    case NPCLyingBehaviour.LiePhase.LieDownPlaying:
                    case NPCLyingBehaviour.LiePhase.LyingIdle:
                    case NPCLyingBehaviour.LiePhase.WakeUpPlaying:
                        EnterState(NPCState.Lying);
                        return;

                    case NPCLyingBehaviour.LiePhase.None:
                        EnterState(GetDefaultWanderState());
                        return;
                }

                break;

            case NPCState.Lying:
                _lying?.Tick(dt);

                if (_lying == null)
                    break;

                switch (_lying.Phase)
                {
                    case NPCLyingBehaviour.LiePhase.SearchingBed:
                    case NPCLyingBehaviour.LiePhase.RoutingToLadder:
                    case NPCLyingBehaviour.LiePhase.ClimbingLadder:
                    case NPCLyingBehaviour.LiePhase.ApproachingFront:
                    case NPCLyingBehaviour.LiePhase.Aligning:
                        EnterState(NPCState.SeekingBed);
                        return;

                    case NPCLyingBehaviour.LiePhase.WakeUpPlaying:
                        EnterState(NPCState.Lying);
                        return;

                    case NPCLyingBehaviour.LiePhase.None:
                        EnterState(GetDefaultWanderState());
                        return;
                }

                break;

            case NPCState.Talk:
                _talk?.TickTalk(dt);
                break;

            case NPCState.ClimbingLadder:
                break;
        }
    }

    void TickPatrolling(float dt, float speedMultiplier)
    {
        if (!AgentReady()) return;
        if (currentTarget != null && CanSeeTarget(currentTarget)) return;
        if (_autoActionCoroutine != null) return;

        _patrolChangeTimer -= dt;
        if (_patrolChangeTimer <= 0f)
        {
            _hasPatrolDestination = false;
            _patrolChangeTimer = UnityEngine.Random.Range(patrolChangeDirInterval.x, patrolChangeDirInterval.y);
        }

        _patrolActionTimer -= dt;
        if (_patrolActionTimer <= 0f)
        {
            _patrolActionTimer = UnityEngine.Random.Range(
                patrolActionCheckInterval.x,
                patrolActionCheckInterval.y);

            if (TryStartRandomPatrolAction())
                return;
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

    void TickPredefinedPatrol(float dt)
    {
        if (!AgentReady())
            return;

        if (predefinedPatrolPoints == null || predefinedPatrolPoints.Count == 0)
        {
            EnterState(NPCState.Patrolling);
            return;
        }

        // ── Already have a destination ──────────────────────────────────────
        if (_hasPatrolDestination)
        {
            // First time we detect arrival → start the wait timer
            if (_patrolArriveTimer <= 0f && HasArrived(agent, arriveDistance))
            {
                float min = predefinedPatrolArrivePause.x;
                float max = predefinedPatrolArrivePause.y;
                if (max < min) max = min; // safety

                _patrolArriveTimer = (Mathf.Approximately(min, max))
                    ? min // fixed wait
                    : UnityEngine.Random.Range(min, max);

                agent.isStopped = true;
            }

            // While waiting, stay stopped and count down
            // (do NOT re-require HasArrived – that was the original bug)
            if (_patrolArriveTimer > 0f)
            {
                agent.isStopped = true;
                _patrolArriveTimer -= dt;

                if (_patrolArriveTimer <= 0f)
                {
                    agent.isStopped = false;
                    _hasPatrolDestination = false;

                    _predefinedPatrolIndex =
                        (_predefinedPatrolIndex + 1) % predefinedPatrolPoints.Count;
                }

                return;
            }
        }

        // ── Need a new destination ──────────────────────────────────────────
        if (!_hasPatrolDestination)
        {
            Transform wp = GetValidPredefinedWaypoint();
            if (wp == null)
                return;

            agent.isStopped = false;
            agent.speed = moveSpeed;
            agent.autoBraking = true;

            agent.ResetPath();
            bool destinationSet = agent.SetDestination(wp.position);

            if (destinationSet)
            {
                _hasPatrolDestination = true;
            }
            else
            {
                Debug.LogWarning(
                    $"{name}: Failed to set predefined patrol destination " +
                    $"to waypoint {_predefinedPatrolIndex} '{wp.name}'. " +
                    $"Waypoint position: {wp.position}");

                _hasPatrolDestination = false;
                _predefinedPatrolIndex =
                    (_predefinedPatrolIndex + 1) % predefinedPatrolPoints.Count;
            }
        }
    }

    Transform GetValidPredefinedWaypoint()
    {
        int count = predefinedPatrolPoints.Count;
        for (int i = 0; i < count; i++)
        {
            int idx = (_predefinedPatrolIndex + i) % count;
            if (predefinedPatrolPoints[idx] != null)
            {
                _predefinedPatrolIndex = idx;
                return predefinedPatrolPoints[idx];
            }
        }

        return null;
    }

    void TickAlerted(float dt)
    {
        if (AgentReady()) agent.isStopped = true;

        if (currentTarget != null)
        {
            Vector3 to = currentTarget.position - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(to.normalized, Vector3.up),
                    dt * (turnSmoothing * 1.25f));
        }

        if (_stateTimer >= alertedDuration)
        {
            if (currentTarget != null)
            {
                float dist = Vector3.Distance(_spawnPoint, currentTarget.position);
                EnterState(dist <= activeRadius || allowApproachOutsideActiveRadius
                    ? NPCState.Approaching
                    : NPCState.Seeking);
            }
            else EnterState(NPCState.Seeking);
        }
    }

    void TickApproaching(float dt)
    {
        
        bool shouldGoUp = false;
        
        if (!AgentReady()) return;

        if (!_hasCommand && currentTarget != null)
        {
            float distToSpawn = Vector3.Distance(_spawnPoint, currentTarget.position);
            if (distToSpawn > activeRadius && !allowApproachOutsideActiveRadius)
            {
                EnterState(NPCState.Seeking);
                return;
            }
        }

        if (currentTarget == null && !(_hasCommand && _commandGoal == NPCState.Sitting))
        {
            _hasCommand = false;
            _commandGoal = NPCState.Patrolling;
            EnterState(GetDefaultWanderState());
            return;
        }

        if (currentTarget == null)
        {
            EnterState(GetDefaultWanderState());
            return;
        }

        float distToTarget = Vector3.Distance(transform.position, currentTarget.position);

        if (_hasCommand)
        {
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
        else
        {
            if (distToTarget <= attackRange)
            {
                EnterState(NPCState.Attacking);
                return;
            }
        }

        _approachRepathTimer -= dt;
        if (_approachRepathTimer > 0f)
            return;

        _approachRepathTimer = Mathf.Max(0.05f, approachRepathInterval);

        Vector3 dest = currentTarget.position;
        

        bool needLadder = _ladder != null && _ladder.NeedLadderForTarget(currentTarget, out shouldGoUp);

        if (_ladder != null && _ladder.useLadders && needLadder)
        {
            
            if (TryFindLadderRoute(dest, out Ladder ladder, out bool routeGoingUp,
                    out Vector3 approachPoint, out Vector3 exitPoint, out NavMeshPath _))
            {
                if (routeGoingUp == shouldGoUp)
                {
                    float distToApproach = Vector3.Distance(transform.position, approachPoint);

                    if (distToApproach > ladderApproachArriveDistance + 0.1f)
                    {
                        if (_ladder.ladderDebugLogs)
                            Debug.Log($"{name}: Need ladder. Routing to approach point {approachPoint} on {ladder.name}, goingUp={routeGoingUp}");

                        agent.isStopped = false;
                        agent.SetDestination(approachPoint);
                        return;
                    }

                    if (_ladder.ladderDebugLogs)
                        Debug.Log($"{name}: Need ladder. Starting traversal on {ladder.name}, goingUp={routeGoingUp}");

                    StartLadderTraversal(ladder, routeGoingUp, () =>
                    {
                        if (AgentReady())
                        {
                            agent.isStopped = false;
                            agent.ResetPath();
                            agent.SetDestination(dest);
                        }
                    });
                    return;
                }
            }

            if (_ladder.ladderDebugLogs) Debug.LogWarning($"{name}: Need ladder but no valid ladder route was found. Target={currentTarget.name}");
        }

        agent.isStopped = false;
        agent.SetDestination(dest);
    }

    void TickSeeking(float dt)
    {
        _seekTimer += dt;
        if (_seekTimer >= Mathf.Max(0.01f, seekGiveUpSeconds))
        {
            EnterState(GetDefaultWanderState());
            return;
        }

        if (currentTarget != null && CanSeeTarget(currentTarget))
        {
            float dist = Vector3.Distance(_spawnPoint, currentTarget.position);
            if (dist <= activeRadius || allowApproachOutsideActiveRadius)
            {
                EnterState(NPCState.Alerted);
                return;
            }
        }

        TickPatrolling(dt, seekingSpeedMultiplier);
    }

    void UpdateLocomotionAndFacing()
    {
        if (animationController == null) return;

        bool sitWalkup =
            (_sitting != null) &&
            (_sitting.Phase == NPCSittingBehaviour.SitPhase.RoutingToLadder ||
             _sitting.Phase == NPCSittingBehaviour.SitPhase.ApproachingFront);

        bool lieWalkup =
            (_lying != null) &&
            (_lying.Phase == NPCLyingBehaviour.LiePhase.RoutingToLadder ||
             _lying.Phase == NPCLyingBehaviour.LiePhase.ApproachingFront);

        bool inAnySitBodyPhase =
            (_state == NPCState.SeekingSeat || _state == NPCState.Sitting) &&
            _sitting != null &&
            (_sitting.Phase == NPCSittingBehaviour.SitPhase.Aligning ||
             _sitting.Phase == NPCSittingBehaviour.SitPhase.Backstepping ||
             _sitting.Phase == NPCSittingBehaviour.SitPhase.SitDownPlaying ||
             _sitting.Phase == NPCSittingBehaviour.SitPhase.SittingIdle ||
             _sitting.Phase == NPCSittingBehaviour.SitPhase.StandUpPlaying);

        bool inAnyLieBodyPhase =
            (_state == NPCState.SeekingBed || _state == NPCState.Lying) &&
            _lying != null &&
            (_lying.Phase == NPCLyingBehaviour.LiePhase.Aligning ||
             _lying.Phase == NPCLyingBehaviour.LiePhase.LieDownPlaying ||
             _lying.Phase == NPCLyingBehaviour.LiePhase.LyingIdle ||
             _lying.Phase == NPCLyingBehaviour.LiePhase.WakeUpPlaying);

        bool blockLocomotion =
            IsTraversingLadder ||
            (inAnySitBodyPhase && !sitWalkup) ||
            (inAnyLieBodyPhase && !lieWalkup);

        bool allowNavLocomotion = AgentReady() && IsNavDriven() && !blockLocomotion;
        bool allowLinkLocomotion = _isTraversingOffMeshLink && !blockLocomotion;

        float movingX = 0f, movingY = 0f, blend = 0f;

        if (allowNavLocomotion)
        {
            Vector3 to = agent.steeringTarget - transform.position;
            to.y = 0f;
            Vector3 desiredDir = (to.sqrMagnitude > 0.0001f)
                ? to.normalized
                : (agent.desiredVelocity.sqrMagnitude > 0.0001f ? agent.desiredVelocity.normalized : transform.forward);

            Vector3 v = agent.velocity;
            v.y = 0f;
            Vector3 dv = agent.desiredVelocity;
            dv.y = 0f;
            float speed = v.magnitude;
            if (agent.pathPending || (speed < 0.05f && dv.magnitude > 0.05f)) speed = dv.magnitude;

            float speed01 = Mathf.Clamp01(speed / Mathf.Max(0.01f, moveSpeed));
            float signedAngle = Vector3.SignedAngle(transform.forward, desiredDir, Vector3.up);
            if (Mathf.Abs(signedAngle) < 8f) signedAngle = 0f;

            float turnWeight = (speed < turnInPlaceSpeedThreshold) ? 1f : 0.35f;
            movingX = Mathf.Clamp((signedAngle / Mathf.Max(1f, turnAngleForFullX)) * turnWeight, -1f, 1f);
            movingY = speed01;
            blend = speed01;
            if (speed01 > 0.35f && Mathf.Abs(movingX) < 0.2f) movingX = 0f;

            float turnRate = (speed < turnInPlaceSpeedThreshold) ? turnInPlaceSpeed : turnWhileMovingSpeed;
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(desiredDir, Vector3.up),
                turnRate * Time.deltaTime);
        }
        else if (allowLinkLocomotion)
        {
            // Drive blend tree as if walking straight at full speed
            float speed01 = Mathf.Clamp01(_offMeshLinkFakeVelocity.magnitude / Mathf.Max(0.01f, moveSpeed));
            movingX = 0f;
            movingY = speed01;
            blend   = speed01;
        }

        animationController.SetFloat(paramMovingX, movingX, animDampTime, Time.deltaTime);
        animationController.SetFloat(paramMovingY, movingY, animDampTime, Time.deltaTime);
        animationController.SetFloat(paramBlend, blend, animDampTime, Time.deltaTime);
    }

    public void PlaySelectedTrigger()
    {
        if (triggerNames == null || triggerNames.Count == 0)
        {
            Debug.LogWarning($"{name}: No triggerNames.");
            return;
        }

        selectedTriggerIndex = Mathf.Clamp(selectedTriggerIndex, 0, triggerNames.Count - 1);
        PlayTrigger(triggerNames[selectedTriggerIndex]);
    }

// 1. In PlayTrigger — kill the auto-action coroutine before the trigger starts
    public void PlayTrigger(string triggerParam)
    {
        if (string.IsNullOrWhiteSpace(triggerParam))
        {
            Debug.LogWarning($"{name}: Trigger param empty.");
            return;
        }

        if (animationController == null)
        {
            Debug.LogWarning($"{name}: No Animator.");
            return;
        }

        // Stop any autonomous patrol action — its coroutine calls EnterState
        // which sets agent.isStopped = false, overriding TriggerRoutine's stop.
        if (_autoActionCoroutine != null)
        {
            StopCoroutine(_autoActionCoroutine);
            _autoActionCoroutine = null;
        }

        if (TryQueueActionAfterStand(() =>
            {
                if (_triggerCoroutine != null) { StopCoroutine(_triggerCoroutine); _triggerCoroutine = null; }
                _talk?.UnregisterAsSpeaker();
                _triggerCoroutine = StartCoroutine(TriggerRoutine(triggerParam));
            })) return;

        if (_triggerCoroutine != null) { StopCoroutine(_triggerCoroutine); _triggerCoroutine = null; }
        _talk?.UnregisterAsSpeaker();
        _triggerCoroutine = StartCoroutine(TriggerRoutine(triggerParam));
    }

    public void PauseTriggeredAnimation()
    {
        if (animationController == null) return;
        animationController.speed = 0f;
    }

    public void StopTriggeredAnimation()
    {
        if (_triggerCoroutine != null) { StopCoroutine(_triggerCoroutine); _triggerCoroutine = null; }

        _isPlayingTriggeredAnimation = false;
        if (animationController != null) animationController.speed = 1f;
        ResetAllAnimatorTriggers();
        isPaused = _triggerPrevPaused;

        if (_triggerAgentWasValid && _triggerAgentWasEnabled)
        {
            if (TryGetNavmeshPoint(transform.position, out Vector3 navPos))
            {
                transform.position = navPos;
                agent.enabled = true;
                agent.Warp(navPos);
            }
            else
            {
                agent.enabled = true;
            }
            agent.isStopped = _triggerWasStoppedBefore;
            agent.ResetPath();
        }

        if (useStateMachine && !isPaused) ForceReturnToLocomotion();
    }

    // ── Upper Body Animations ─────────────────────────────────────────────────
    // Play on the UpperLayer only. Does not pause the FSM, stop the agent,
    // or force the NPC to stand up first — safe to call while seated or lying.

    public void PlaySelectedUpperBodyTrigger()
    {
        if (upperBodyTriggerNames == null || upperBodyTriggerNames.Count == 0)
            { Debug.LogWarning($"{name}: No upperBodyTriggerNames."); return; }
        selectedUpperBodyTriggerIndex = Mathf.Clamp(selectedUpperBodyTriggerIndex, 0, upperBodyTriggerNames.Count - 1);
        PlayUpperBodyTrigger(thisNPC, upperBodyTriggerNames[selectedUpperBodyTriggerIndex]);
    }

    public void PlayUpperBodyTrigger(NPC suppliedNPC, string triggerParam)
    {
        if (suppliedNPC != thisNPC) return;

        Debug.Log("play trigger "+ triggerParam + " for " +suppliedNPC);
        
        if (string.IsNullOrWhiteSpace(triggerParam))
        {
            Debug.LogWarning($"{name}: Upper body trigger param empty."); return;
        }

        if (animationController == null)
        {
            Debug.LogWarning($"{name}: No Animator for upper body trigger."); return;
        }

        // StopUpperBodyAnimation zeros the weight and clears flags — raw StopCoroutine
        // does not, leaving weight=1 and _isPlayingUpperBodyAnimation=true on restart.
        StopUpperBodyAnimation();
        _upperBodyCoroutine = StartCoroutine(UpperBodyTriggerRoutine(triggerParam));
    }

    public void StopUpperBodyAnimation()
    {
        if (_upperBodyCoroutine != null) { StopCoroutine(_upperBodyCoroutine); _upperBodyCoroutine = null; }
        _isPlayingUpperBodyAnimation = false;
        if (animationController == null) return;

        int layer = animationController.GetLayerIndex(upperBodyLayerName);
        if (layer >= 0) animationController.SetLayerWeight(layer, 0f);
    }

    IEnumerator UpperBodyTriggerRoutine(string triggerParam)
    {
        if (animationController == null) yield break;

        int layer = animationController.GetLayerIndex(upperBodyLayerName);
        if (layer < 0)
        {
            Debug.LogWarning($"{name}: Animator layer '{upperBodyLayerName}' not found.");
            _upperBodyCoroutine = null;
            yield break;
        }

        AnimatorControllerParameterType? pType = GetAnimatorParameterType(animationController, triggerParam);
        if (pType == null)
        {
            Debug.LogWarning($"{name}: Animator has no param '{triggerParam}' for upper body.");
            _upperBodyCoroutine = null;
            yield break;
        }

        // ── Setup ─────────────────────────────────────────────────────────────
        // StopUpperBodyAnimation() was already called by PlayUpperBodyTrigger so
        // weight is 0 and state is clean. Set weight and yield ONE frame so Unity
        // evaluates the layer at weight 1 before we read startHash.
        // Do NOT call animationController.Update(0f) here — that re-evaluates the
        // entire Animator mid-frame and corrupts internal trigger/transition state
        // on repeated calls.
        _isPlayingUpperBodyAnimation = true;
        animationController.SetLayerWeight(layer, 1f);
        yield return null;

        int startHash = animationController.GetCurrentAnimatorStateInfo(layer).fullPathHash;

        // Fire the trigger, then yield one more frame before polling — gives the
        // Animator one Update cycle to register the parameter change.
        SetAnimatorParamOn(animationController, triggerParam, pType.Value);
        yield return null;

        // ── Wait for transition to start ──────────────────────────────────────
        // Use GetNextAnimatorStateInfo while IsInTransition to capture the DESTINATION
        // hash — GetCurrentAnimatorStateInfo during a transition returns the source state
        // (= startHash), which would cause the completion check to exit immediately.
        bool entered = false;
        int triggeredHash = startHash;
        float enterT = 0f;
        while (enterT < triggerEnterTimeout)
        {
            if (animationController.IsInTransition(layer))
            {
                triggeredHash = animationController.GetNextAnimatorStateInfo(layer).fullPathHash;
                entered = true;
                break;
            }
            var info = animationController.GetCurrentAnimatorStateInfo(layer);
            if (info.fullPathHash != startHash)
            {
                triggeredHash = info.fullPathHash;
                entered = true;
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

        // ── Wait for entry transition to finish ───────────────────────────────
        // Timeout prevents an infinite hang if the Animator never leaves transition.
        float transT = 0f;
        while (animationController.IsInTransition(layer) && transT < 2f)
        {
            transT += Time.deltaTime;
            yield return null;
        }

        // ── Wait for the animation to play through ────────────────────────────
        float t = 0f;
        while (t < triggerMaxDuration)
        {
            // The state machine already transitioned away on its own (e.g. Exit Time) — done.
            if (animationController.IsInTransition(layer))
            {
                float exitTransT = 0f;
                while (animationController.IsInTransition(layer) && exitTransT < 2f)
                {
                    exitTransT += Time.deltaTime;
                    yield return null;
                }
                break;
            }
            var info = animationController.GetCurrentAnimatorStateInfo(layer);
            if (entered && info.fullPathHash == triggeredHash && info.normalizedTime >= 0.95f)
                break;
            t += Time.deltaTime;
            yield return null;
        }

        if (triggerExitBuffer > 0f)
            yield return new WaitForSeconds(triggerExitBuffer);

        // ── Return to idle state and zero weight ──────────────────────────────
        // Crossfading to a designated return state before zeroing the weight prevents
        // the layer from freezing on the last frame of the animation. Without this,
        // startHash == triggeredHash on the next call, breaking transition detection.
        if (!string.IsNullOrEmpty(upperBodyReturnStateName))
        {
            animationController.CrossFadeInFixedTime(upperBodyReturnStateName, 0.12f, layer, 0f);
            float retTransT = 0f;
            while (animationController.IsInTransition(layer) && retTransT < 2f)
            {
                retTransT += Time.deltaTime;
                yield return null;
            }
        }

        animationController.SetLayerWeight(layer, 0f);
        _isPlayingUpperBodyAnimation = false;
        _upperBodyCoroutine = null;
    }

    public bool IsOnOffMeshLink()
    {
        return agent != null && agent.isOnOffMeshLink;
    }
    
    IEnumerator TriggerRoutine(string triggerParam)
    {

        _isPlayingTriggeredAnimation = true;
        _triggerPrevPaused           = isPaused;
        _triggerAgentWasValid        = agent != null && agent.isActiveAndEnabled;
        _triggerAgentWasEnabled      = _triggerAgentWasValid && agent.enabled;
        _triggerWasStoppedBefore     = _triggerAgentWasValid && agent.isStopped;
        isPaused = true;

        if (_triggerAgentWasValid)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;   // full detach — prevents ALL navmesh simulation
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
            Debug.LogWarning($"{name}: Animator has no param '{triggerParam}'.");
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
            var info = animationController.GetCurrentAnimatorStateInfo(layer);
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
            var info = animationController.GetCurrentAnimatorStateInfo(layer);
            if (entered && !animationController.IsInTransition(layer))
            {
                if (info.fullPathHash == triggeredHash && info.normalizedTime >= 1f) break;
                if (info.fullPathHash == startHash) break;
            }

            t += Time.deltaTime;
            yield return null;
        }

        if (triggerExitBuffer > 0f) yield return new WaitForSeconds(triggerExitBuffer);

        SetAnimatorParamOff(animationController, triggerParam, pType.Value);
        if (pType.Value == AnimatorControllerParameterType.Trigger)
            animationController.ResetTrigger(triggerParam);

        isPaused = _triggerPrevPaused;

        if (_triggerAgentWasValid && _triggerAgentWasEnabled)
        {
            if (TryGetNavmeshPoint(transform.position, out Vector3 navPos))
            {
                transform.position = navPos;
                agent.enabled = true;
                agent.Warp(navPos);
            }
            else
            {
                agent.enabled = true;
            }
            agent.isStopped = _triggerWasStoppedBefore;
            agent.ResetPath();
        }

        _isPlayingTriggeredAnimation = false;
        _triggerCoroutine = null;

        if (useStateMachine && !isPaused) ForceReturnToLocomotion();
        _triggerCoroutine = null;

        if (useStateMachine && !isPaused) ForceReturnToLocomotion();
    }

    public void StopAndFace(Vector3 worldPos)
    {
        if (AgentReady())
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        ForceIdlePose();
        Vector3 to = worldPos - transform.position;
        to.y = 0f;
        if (to.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(to.normalized, Vector3.up),
            Time.deltaTime * (turnSmoothing * 1.5f));
    }

    void PreCollisionRenegotiate()
    {
        if (!IsNavDriven() || !AgentReady()) return;
        if (useStateMachine && (_state == NPCState.Alerted || _state == NPCState.Attacking ||
                                _state == NPCState.Talk || _state == NPCState.Sitting ||
                                _state == NPCState.Lying || _state == NPCState.ClimbingLadder)) return;
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

        for (int i = 0; i < hits.Length; i++)
        {
            var other = hits[i]?.GetComponentInParent<NPCController>();
            if (!other || other == this || !other.agent || !other.agent.isOnNavMesh) continue;

            Vector3 toOther = other.transform.position - transform.position;
            toOther.y = 0f;
            float d = toOther.magnitude;
            if (d < 0.0001f) continue;
            if (Vector3.Dot(fwd, toOther / d) < inFrontDotThreshold) continue;
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

        Vector3 p0 = transform.position + right * (sidestepDistance * sign);
        Vector3 p1 = transform.position - right * (sidestepDistance * sign);

        Vector3 chosen;
        if (!TrySampleNavPoint(p0, out chosen) && !TrySampleNavPoint(p1, out chosen)) yield break;

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

    bool IsNavDriven()
    {
        if (isPaused) return false;
        if (_isPlayingTriggeredAnimation) return false;
        if (IsTraversingLadder) return false;
        
        return useStateMachine;
    }

    private bool IsRecoveringBodyState()
    {
        if ((_state == NPCState.SeekingSeat || _state == NPCState.Sitting) &&
            _sitting != null &&
            _sitting.Phase != NPCSittingBehaviour.SitPhase.None)
            return true;

        if ((_state == NPCState.SeekingBed || _state == NPCState.Lying) &&
            _lying != null &&
            _lying.Phase != NPCLyingBehaviour.LiePhase.None)
            return true;

        return false;
    }

    private bool TryQueueActionAfterStand(Action action)
    {
        if (_talk != null && _talk.IsConversationLocked)
            _talk.BreakConversationLockImmediate();

        if (!IsRecoveringBodyState()) return false;

        // If we're only seeking a seat/bed and not actually seated/lying yet,
        // don't queue behind a stand/wake animation that will never happen.
        if (_state == NPCState.SeekingSeat || _state == NPCState.SeekingBed)
            return false;

        _pendingPostStandAction = action;
        _executePendingActionAfterStand = true;

        if (_state == NPCState.Sitting && _sitting != null)
        {
            if (_sitting.Phase != NPCSittingBehaviour.SitPhase.StandUpPlaying)
                _sitting.BeginStandUp();
        }
        else if (_state == NPCState.Lying && _lying != null)
        {
            if (_lying.Phase != NPCLyingBehaviour.LiePhase.WakeUpPlaying)
                _lying.BeginWakeUp();
        }

        return true;
    }

    private void InterruptAllTransientActions(bool returnToLocomotion = true)
    {
        _talk?.UnregisterAsSpeaker();

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

        _talk?.ClearAllSpeakers();

        if (_sidestepCoroutine != null)
        {
            StopCoroutine(_sidestepCoroutine);
            _sidestepCoroutine = null;
        }

        _hasResumeDestination = false;

        _ladder?.InterruptTraversal();

        if (agent != null)
        {
            if (!agent.enabled) agent.enabled = true;
            if (!agent.isOnNavMesh && TryGetNavmeshPoint(transform.position, out Vector3 navPos))
                agent.Warp(navPos);

            agent.updatePosition = true;
            agent.updateRotation = false;

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

    private void ResetLocomotionState()
    {
        _animVelocitySmoothed = Vector3.zero;
        _hasResumeDestination = false;

        if (animationController != null)
        {
            animationController.SetFloat(paramMovingX, 0f);
            animationController.SetFloat(paramMovingY, 0f);
            animationController.SetFloat(paramBlend, 0f);
        }
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

    public NPCController ResolveNPC(NPC npcEnum)
    {
        var list = sceneNPCManager?.NPCList;
        if (list == null) return null;
        for (int i = 0; i < list.Count; i++)
            if (list[i] != null && list[i].thisNPC == npcEnum)
                return list[i];
        return null;
    }

    bool CanSeeTarget(Transform t)
    {
        if (t == null) return false;
        Vector3 to = t.position - transform.position;
        if (to.magnitude > visionRange) return false;

        Vector3 flatTo = to;
        flatTo.y = 0f;
        Vector3 fwd = transform.forward;
        fwd.y = 0f;

        if (flatTo.sqrMagnitude > 0.0001f && fwd.sqrMagnitude > 0.0001f)
            if (Vector3.Angle(fwd.normalized, flatTo.normalized) > visionFOV * 0.5f)
                return false;

        if (requireLineOfSight)
        {
            Vector3 origin = transform.position + Vector3.up * 1.6f;
            Vector3 targetPos = t.position + Vector3.up * 1.2f;
            Vector3 dir = targetPos - origin;
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
            Vector3 cand = center + new Vector3(r.x, 0f, r.y);
            if (NavMesh.SamplePosition(cand, out NavMeshHit hit, patrolSampleRadius, NavMesh.AllAreas))
                if (Vector3.Distance(center, hit.position) <= radius + 0.25f)
                {
                    result = hit.position;
                    return true;
                }
        }

        return false;
    }

    bool EnsureAgentOnNavMesh(string context)
    {
        if (agent == null || !agent.isActiveAndEnabled)
        {
            Debug.LogWarning($"{name}: Agent missing/disabled ({context}).");
            return false;
        }

        if (agent.isOnNavMesh) return true;

        // ── NEW: agent is mid-traversal; leave it alone ───────────────────────
        if (agent.isOnOffMeshLink) return true;

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

        return false;
    }

    static bool HasArrived(NavMeshAgent a, float arriveDist)
    {
        if (a == null || !a.isOnNavMesh || a.pathPending)
            return false;

        // remainingDistance can be 0 or a tiny value even when hasPath becomes false
        float threshold = Mathf.Max(arriveDist, a.stoppingDistance);
        if (a.hasPath && !float.IsInfinity(a.remainingDistance))
            return a.remainingDistance <= threshold;

        // Fallback: if we have no path but we are very close to the last destination
        return a.remainingDistance <= threshold ||
               (a.destination != Vector3.zero &&
                Vector3.Distance(a.transform.position, a.destination) <= threshold + 0.15f);
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

    AnimatorControllerParameterType? GetAnimatorParameterType(Animator anim, string paramName)
    {
        var ps = anim.parameters;
        for (int i = 0; i < ps.Length; i++)
            if (ps[i].name == paramName)
                return ps[i].type;
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
    private IEnumerator AutoSitRoutine()
    {
        float duration = RandomDuration(MinSitTime, MaxSitTime);

        RequestSitDown();

        float timeout = Mathf.Max(2f, seatSearchTimeoutSafe());
        bool becameSeated = false;

        while (timeout > 0f)
        {
            if (_state == NPCState.Patrolling)
                break;

            if (IsActuallySeated())
            {
                becameSeated = true;
                break;
            }

            if (!IsSitPhaseActive())
                break;

            timeout -= Time.deltaTime;
            yield return null;
        }

        if (!becameSeated)
        {
            SetActionCooldown(PatrolAction.Sit, patrolActionFailedRetrySeconds);
            _autoActionCoroutine = null;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            if (!IsActuallySeated())
                break;

            t += Time.deltaTime;
            yield return null;
        }

        if (_sitting != null &&
            _sitting.Phase == NPCSittingBehaviour.SitPhase.SittingIdle)
        {
            RequestStandUp();

            float standTimeout = 8f;
            while (standTimeout > 0f)
            {
                if (_sitting == null || _sitting.Phase == NPCSittingBehaviour.SitPhase.None)
                    break;

                standTimeout -= Time.deltaTime;
                yield return null;
            }
        }

        SetActionCooldown(PatrolAction.Sit, patrolActionCooldownSeconds);
        _autoActionCoroutine = null;
    }

    private IEnumerator AutoSleepRoutine()
    {
        float duration = RandomDuration(MinSleepTime, MaxSleepTime);

        RequestLieDown();

        float timeout = Mathf.Max(2f, bedSearchTimeoutSafe());
        bool becameLying = false;

        while (timeout > 0f)
        {
            if (_state == NPCState.Patrolling)
                break;

            if (IsActuallyLying())
            {
                becameLying = true;
                break;
            }

            if (!IsSleepPhaseActive())
                break;

            timeout -= Time.deltaTime;
            yield return null;
        }

        if (!becameLying)
        {
            SetActionCooldown(PatrolAction.Sleep, patrolActionFailedRetrySeconds);
            _autoActionCoroutine = null;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            if (!IsActuallyLying())
                break;

            t += Time.deltaTime;
            yield return null;
        }

        if (_lying != null &&
            _lying.Phase == NPCLyingBehaviour.LiePhase.LyingIdle)
        {
            RequestWakeUp();

            float wakeTimeout = 10f;
            while (wakeTimeout > 0f)
            {
                if (_lying == null || _lying.Phase == NPCLyingBehaviour.LiePhase.None)
                    break;

                wakeTimeout -= Time.deltaTime;
                yield return null;
            }
        }

        SetActionCooldown(PatrolAction.Sleep, patrolActionCooldownSeconds);
        _autoActionCoroutine = null;
    }
    private float seatSearchTimeoutSafe()
    {
        return 6f;
    }

    private float bedSearchTimeoutSafe()
    {
        return 6f;
    }
    private IEnumerator AutoTalkRoutine()
    {
        if (!TryFindTalkCandidate(out NPCController other) || other == null)
        {
            SetActionCooldown(PatrolAction.Talk, patrolActionFailedRetrySeconds);
            _autoActionCoroutine = null;
            yield break;
        }

        float duration = RandomDuration(MinTalkTime, MaxTalkTime);

        InterruptAllTransientActions();
        useStateMachine = true;

        currentTarget = other.transform;
        _hasCommand = true;
        _commandGoal = NPCState.Talk;

        if (_talk != null)
            _talk.TalkTargetController = other;

        EnterState(Vector3.Distance(transform.position, other.transform.position) <= talkRange
            ? NPCState.Talk
            : NPCState.Approaching);

        float timeout = 10f;
        while (timeout > 0f && _state != NPCState.Talk)
        {
            if (currentTarget == null)
                break;

            timeout -= Time.deltaTime;
            yield return null;
        }

        if (_state != NPCState.Talk)
        {
            ClearTarget();
            EnterState(GetDefaultWanderState());
            SetActionCooldown(PatrolAction.Talk, patrolActionFailedRetrySeconds);
            _autoActionCoroutine = null;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            if (_state != NPCState.Talk || currentTarget == null)
                break;

            t += Time.deltaTime;
            yield return null;
        }

        _talk?.UnregisterAsSpeaker();
        ClearTarget();
        EnterState(GetDefaultWanderState());

        SetActionCooldown(PatrolAction.Talk, patrolActionCooldownSeconds);
        _autoActionCoroutine = null;
    }
    public void RespawnNPC()
    {
        _pendingPostStandAction = null;
        _executePendingActionAfterStand = false;

        currentTarget = null;
        _hasLastKnownTargetPos = false;
        _hasCommand = false;
        _commandGoal = NPCState.Patrolling;

        _hasPatrolDestination = false;
        _hasResumeDestination = false;
        _predefinedPatrolIndex = 0;

        _resumeDestination = Vector3.zero;
        _patrolDestination = Vector3.zero;
        _lastKnownTargetPos = Vector3.zero;

        _stateTimer = 0f;
        _patrolChangeTimer = 0f;
        _patrolArriveTimer = 0f;
        _approachRepathTimer = 0f;
        _seekTimer = 0f;

        _renegotiateT = 0f;
        _renegotiateCooldownT = 0f;

        InterruptAllTransientActions(false);

        // RegisteredAsSpeaker and TalkTargetController are not reset by InterruptAllTransientActions;
        // clear them explicitly here.
        if (_talk != null)
        {
            _talk.RegisteredAsSpeaker = false;
            _talk.TalkTargetController = null;
        }

        if (_sitting != null)
            _sitting.ForceCancelSitting();

        if (_lying != null)
            _lying.ForceCancelLying();

        if (animationController != null)
        {
            animationController.speed = 1f;
            ResetAllAnimatorTriggers();
            animationController.SetFloat(paramMovingX, 0f);
            animationController.SetFloat(paramMovingY, 0f);
            animationController.SetFloat(paramBlend, 0f);
        }

        ResetLocomotionState();
        isPaused = false;
        _state = NPCState.Patrolling;

        Vector3 respawnPos = _spawnPoint;
        if (TryGetNavmeshPointNear(_spawnPoint, snapToNavMeshRadius, out Vector3 navPos))
            respawnPos = navPos;

        transform.position = respawnPos;
        ForceUpright();

        if (agent != null)
        {
            if (!agent.enabled) agent.enabled = true;

            if (agent.isActiveAndEnabled)
            {
                agent.Warp(respawnPos);
                agent.isStopped = false;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }

            agent.speed = moveSpeed;
            agent.angularSpeed = angularSpeed;
            agent.acceleration = acceleration;
            agent.autoBraking = true;
            agent.autoRepath = true;
        }
        
        if (_autoActionCoroutine != null)
        {
            StopCoroutine(_autoActionCoroutine);
            _autoActionCoroutine = null;
        }

        _sitCooldownT = 0f;
        _sleepCooldownT = 0f;
        _talkCooldownT = 0f;
        
        useStateMachine = true;
        ForceReturnToLocomotion();
        EnterState(GetDefaultWanderState());

        Debug.Log($"{name}: Respawned to spawn point {respawnPos}");
    }
    
        private bool TryStartRandomPatrolAction()
    {
        List<PatrolAction> options = new List<PatrolAction>();

        if (CanSit && _sitCooldownT <= 0f && _sitting != null)
            options.Add(PatrolAction.Sit);

        if (CanSleep && _sleepCooldownT <= 0f && _lying != null)
            options.Add(PatrolAction.Sleep);

        if (CanTalk && _talkCooldownT <= 0f && _talk != null && TryFindTalkCandidate(out _))
            options.Add(PatrolAction.Talk);

        if (options.Count == 0)
            return false;

        PatrolAction choice = options[UnityEngine.Random.Range(0, options.Count)];

        switch (choice)
        {
            case PatrolAction.Sit:
                _autoActionCoroutine = StartCoroutine(AutoSitRoutine());
                return true;

            case PatrolAction.Sleep:
                _autoActionCoroutine = StartCoroutine(AutoSleepRoutine());
                return true;

            case PatrolAction.Talk:
                _autoActionCoroutine = StartCoroutine(AutoTalkRoutine());
                return true;
        }

        return false;
    }

    private float RandomDuration(float min, float max)
    {
        if (max < min) max = min;
        return UnityEngine.Random.Range(min, max);
    }

    private bool IsSitPhaseActive()
    {
        if (_sitting == null) return false;

        switch (_sitting.Phase)
        {
            case NPCSittingBehaviour.SitPhase.SearchingSeat:
            case NPCSittingBehaviour.SitPhase.RoutingToLadder:
            case NPCSittingBehaviour.SitPhase.ClimbingLadder:
            case NPCSittingBehaviour.SitPhase.ApproachingFront:
            case NPCSittingBehaviour.SitPhase.Aligning:
            case NPCSittingBehaviour.SitPhase.Backstepping:
            case NPCSittingBehaviour.SitPhase.SitDownPlaying:
            case NPCSittingBehaviour.SitPhase.SittingIdle:
            case NPCSittingBehaviour.SitPhase.StandUpPlaying:
                return true;
        }

        return false;
    }

    private bool IsActuallySeated()
    {
        return _sitting != null &&
               _sitting.Phase == NPCSittingBehaviour.SitPhase.SittingIdle;
    }

    private bool IsSleepPhaseActive()
    {
        if (_lying == null) return false;

        switch (_lying.Phase)
        {
            case NPCLyingBehaviour.LiePhase.SearchingBed:
            case NPCLyingBehaviour.LiePhase.RoutingToLadder:
            case NPCLyingBehaviour.LiePhase.ClimbingLadder:
            case NPCLyingBehaviour.LiePhase.ApproachingFront:
            case NPCLyingBehaviour.LiePhase.Aligning:
            case NPCLyingBehaviour.LiePhase.LieDownPlaying:
            case NPCLyingBehaviour.LiePhase.LyingIdle:
            case NPCLyingBehaviour.LiePhase.WakeUpPlaying:
                return true;
        }

        return false;
    }

    private bool IsActuallyLying()
    {
        return _lying != null &&
               _lying.Phase == NPCLyingBehaviour.LiePhase.LyingIdle;
    }
    private bool TryFindTalkCandidate(out NPCController other)
    {
        other = null;
        if (sceneNPCManager == null || sceneNPCManager.NPCList == null) return false;

        float bestSqr = float.PositiveInfinity;
        Vector3 p = transform.position;
        float maxSqr = autonomousTalkSearchRadius * autonomousTalkSearchRadius;

        for (int i = 0; i < sceneNPCManager.NPCList.Count; i++)
        {
            NPCController c = sceneNPCManager.NPCList[i];
            if (c == null || c == this) continue;
            if (!c.gameObject.activeInHierarchy) continue;
            if (c.GetCurrentState() != NPCState.Patrolling) continue;
            if (c._autoActionCoroutine != null) continue;
            if (c.currentTarget != null) continue;
            if (c._talk != null && c._talk.IsConversationLocked) continue;

            Vector3 d = c.transform.position - p;
            d.y = 0f;
            float sqr = d.sqrMagnitude;
            if (sqr > maxSqr) continue;

            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                other = c;
            }
        }

        return other != null;
    }

    private void SetActionCooldown(PatrolAction action, float seconds)
    {
        switch (action)
        {
            case PatrolAction.Sit:
                _sitCooldownT = seconds;
                break;
            case PatrolAction.Sleep:
                _sleepCooldownT = seconds;
                break;
            case PatrolAction.Talk:
                _talkCooldownT = seconds;
                break;
        }
    }
    
    
    private void TickPatrolMovementOnly(float dt, float speedMultiplier)
    {
        if (!AgentReady()) return;
        if (currentTarget != null && CanSeeTarget(currentTarget)) return;

        _patrolChangeTimer -= dt;
        if (_patrolChangeTimer <= 0f)
        {
            _hasPatrolDestination = false;
            _patrolChangeTimer = UnityEngine.Random.Range(
                patrolChangeDirInterval.x,
                patrolChangeDirInterval.y);
        }

        if (_hasPatrolDestination && HasArrived(agent, arriveDistance))
        {
            if (_patrolArriveTimer <= 0f)
                _patrolArriveTimer = UnityEngine.Random.Range(
                    patrolArrivePause.x,
                    patrolArrivePause.y);

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
}

#if UNITY_EDITOR
[CustomEditor(typeof(NPCController))]
[CanEditMultipleObjects]
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
            if (GUILayout.Button("Pause Anim"))   npc.PauseTriggeredAnimation();
            if (GUILayout.Button("Stop Anim"))    npc.StopTriggeredAnimation();
            EditorGUILayout.EndHorizontal();
        }
        if (!Application.isPlaying) EditorGUILayout.HelpBox("Buttons work in Play Mode only.", MessageType.None);

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("FSM Controls", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            EditorGUILayout.LabelField("Current State", npc.GetCurrentState().ToString());
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Force Patrol")) npc.ForcePatrol();
            if (GUILayout.Button("Clear Target")) npc.ClearTarget();
            if (GUILayout.Button("Respawn"))      npc.RespawnNPC();
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Target Testing (NPC Enum)", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Approach")) npc.DebugApproachTargetNPC();
            if (GUILayout.Button("Attack"))   npc.DebugAttackTargetNPC();
            if (GUILayout.Button("Talk"))     npc.DebugTalkTargetNPC();
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
        EditorGUILayout.LabelField("Startup Animation", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (GUILayout.Button("Play Startup Animation", GUILayout.Height(35)))
            {
                if (!string.IsNullOrWhiteSpace(npc.startupAnimationTrigger))
                    npc.PlayTrigger(npc.startupAnimationTrigger);
                else
                    EditorUtility.DisplayDialog("No Trigger Set",
                        "Set 'Startup Animation Trigger' in the Inspector first.", "OK");
            }
        }
        
        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Lying Controls", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Request Lie Down")) npc.RequestLieDown();
            if (GUILayout.Button("Request Wake Up"))  npc.RequestWakeUp();
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Upper Body Animation Controls", EditorStyles.boldLabel);
        if (npc.upperBodyTriggerNames != null && npc.upperBodyTriggerNames.Count > 0)
            npc.selectedUpperBodyTriggerIndex = EditorGUILayout.Popup("Upper Body Trigger", npc.selectedUpperBodyTriggerIndex, npc.upperBodyTriggerNames.ToArray());
        else
            EditorGUILayout.HelpBox("Add trigger names to 'upperBodyTriggerNames'.", MessageType.Info);
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Play Upper Body")) npc.PlaySelectedUpperBodyTrigger();
            if (GUILayout.Button("Stop Upper Body")) npc.StopUpperBodyAnimation();
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif

public enum NPCState
{
    Patrolling,
    Alerted,
    Approaching,
    Attacking,
    Seeking,
    Talk,
    SeekingSeat,
    Sitting,
    SeekingBed,
    Lying,
    ClimbingLadder,
    PredefinedPatrol
}