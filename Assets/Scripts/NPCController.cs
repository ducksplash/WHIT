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
    public enum NPCState
    {
        Patrolling,
        Alerted,
        Approaching,
        Attacking,
        Seeking,
        Talk,
        Sitting,
        Lying,
        ClimbingLadder
    }

    [Header("Identity")]
    public NPC thisNPC = NPC.Eimear_Scott;

    [Header("AI State Machine")]
    public bool useStateMachine = true;
    public Transform currentTarget;
    public float activeRadius = 15f;

    [Header("Testing / Overrides")]
    public bool allowApproachOutsideActiveRadius = false;
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

    [SerializeField] float faceMinSpeed = 0.08f;
    [SerializeField] float faceMinDistance = 0.15f;
    [SerializeField] float faceMaxDegPerSec = 540f;

    [Header("Turn / BlendTree Driving")]
    [SerializeField] private float turnAngleForFullX = 90f;
    [SerializeField] private float turnInPlaceSpeed = 160f;
    [SerializeField] private float turnWhileMovingSpeed = 260f;
    [SerializeField] private float turnInPlaceSpeedThreshold = 0.15f;

    [Header("Components")]
    public Animator animationController;
    public NavMeshAgent agent;

    [Header("Arrival / Blending")]
    public float arriveDistance = 0.3f;
    public float arriveSettleTime = 0.05f;
    public float stopFadeOutTime = 0.35f;
    public float idleVelocityThreshold = 0.05f;

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

    [Header("Crowd Avoidance")]
    public LayerMask npcLayerMask = ~0;
    public float personalSpaceRadius = 0.9f;
    [Range(-1f, 1f)] public float inFrontDotThreshold = 0.35f;
    public float sidestepDistance = 0.9f;
    public float sidestepDuration = 0.7f;
    public float renegotiateCooldown = 0.8f;
    public float renegotiateCheckInterval = 0.15f;

    public MeNPC npcMetaActions;

    [Header("Ladders")]
    public bool useLadders = true;
    public float ladderApproachArriveDistance = 0.45f;
    public float ladderClimbSpeed = 1.6f;
    public float ladderFacingSlerp = 18f;
    public float ladderMinHeightDeltaForUp = 0.25f;
    public string climbUpTriggerName = "ClimbUpLadder";
    public string climbDownTriggerName = "ClimbDownLadder";
    public float ladderNavmeshSnapRadius = 3f;
    public bool ladderDebugLogs = true;

    [Header("Behaviour Plugins")]
    [SerializeField] private NPCSittingBehaviour _sitting;
    [SerializeField] private NPCLyingBehaviour _lying;
    [SerializeField] private NPCTalkBehaviour _talk;
    [SerializeField] private NPCCombatBehaviour _combat;

    [NonSerialized] public bool isPaused = false;

    public bool HasCommand { get => _hasCommand; set => _hasCommand = value; }
    public NPCState CommandGoal { get => _commandGoal; set => _commandGoal = value; }
    public Vector3 SpawnPoint => _spawnPoint;
    public Vector3 AnimVelocitySmoothed { get => _animVelocitySmoothed; set => _animVelocitySmoothed = value; }
    public NPCTalkBehaviour Talk => _talk;
    public bool IsTraversingLadder => _isTraversingLadder;

    private Vector3 _spawnPoint;
    private Vector3 _patrolDestination;
    private bool _hasPatrolDestination;

    private float _stateTimer;
    private float _patrolChangeTimer;
    private float _patrolArriveTimer;
    private float _approachRepathTimer;
    private float _seekTimer;

    private Vector3 _lastKnownTargetPos;
    private bool _hasLastKnownTargetPos;

    private bool _hasCommand = false;
    private NPCState _commandGoal = NPCState.Patrolling;

    private Vector3 _lastApproachDest;
    private bool _hasLastApproachDest;

    private Action _pendingPostStandAction;
    private bool _executePendingActionAfterStand = false;

    private Vector3 _animVelocitySmoothed = Vector3.zero;
    private Vector3 _resumeDestination;
    private bool _hasResumeDestination;

    private Coroutine _triggerCoroutine;
    private bool _isPlayingTriggeredAnimation = false;
    private bool _triggerPrevPaused;
    private bool _triggerAgentWasValid;
    private bool _triggerHadPathBefore;
    private bool _triggerWasStoppedBefore;

    private float _renegotiateT;
    private float _renegotiateCooldownT;
    private Coroutine _sidestepCoroutine;

    private Coroutine _ladderCoroutine;
    private bool _isTraversingLadder = false;
    private NPCState _stateBeforeLadder = NPCState.Patrolling;
    private Action _ladderCompleteAction;

    public bool AgentReady()
        => agent != null && agent.isActiveAndEnabled && agent.enabled && agent.isOnNavMesh;

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

        if (!AgentReady())
            return false;

        bool ok = agent.CalculatePath(destination, path);
        if (!ok || path == null)
            return false;

        return path.status == NavMeshPathStatus.PathComplete;
    }

    public bool CanReachPosition(Vector3 destination)
    {
        return CanReachPosition(destination, out _);
    }

    public bool CanReachBetween(Vector3 start, Vector3 destination, out NavMeshPath path)
    {
        path = new NavMeshPath();

        if (!TryGetNavmeshPointNear(start, ladderNavmeshSnapRadius, out Vector3 startNav))
            return false;

        if (!TryGetNavmeshPointNear(destination, ladderNavmeshSnapRadius, out Vector3 destNav))
            return false;

        int areaMask = (agent != null) ? agent.areaMask : NavMesh.AllAreas;
        bool ok = NavMesh.CalculatePath(startNav, destNav, areaMask, path);

        return ok && path.status == NavMeshPathStatus.PathComplete;
    }

    public bool TryFindLadderRoute(
        Vector3 destination,
        out Ladder ladder,
        out bool goingUp,
        out Vector3 approachPoint,
        out Vector3 exitPoint,
        out NavMeshPath approachPath)
    {
        ladder = null;
        approachPoint = Vector3.zero;
        exitPoint = Vector3.zero;
        approachPath = null;

        if (!useLadders || !AgentReady())
        {
            goingUp = false;
            return false;
        }

        goingUp = destination.y > transform.position.y + ladderMinHeightDeltaForUp;

        Ladder[] ladders = FindObjectsByType<Ladder>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (ladders == null || ladders.Length == 0)
            return false;

        float bestScore = float.PositiveInfinity;

        for (int i = 0; i < ladders.Length; i++)
        {
            Ladder l = ladders[i];
            if (l == null) continue;

            Transform approachTf;
            Transform mountStartTf;
            Transform mountEndTf;
            Transform exitTf;

            if (goingUp)
            {
                if (l.bottomMountPoint == null || l.topMountPoint == null) continue;
                approachTf   = l.bottomMountPoint;
                mountStartTf = l.bottomMountPoint;
                mountEndTf   = l.topMountPoint;
                exitTf       = l.topExitPoint != null ? l.topExitPoint : l.topMountPoint;
            }
            else
            {
                if (!l.bidirectional) continue;
                if (l.topMountPoint == null || l.bottomMountPoint == null) continue;
                approachTf   = l.topMountPoint;
                mountStartTf = l.topMountPoint;
                mountEndTf   = l.bottomMountPoint;
                exitTf       = l.bottomExitPoint != null ? l.bottomExitPoint : l.bottomMountPoint;
            }

            if (!CanReachPosition(approachTf.position, out NavMeshPath pathToLadder))
                continue;

            if (!CanReachBetween(exitTf.position, destination, out NavMeshPath _))
                continue;

            float score =
                Vector3.Distance(transform.position, approachTf.position) +
                Vector3.Distance(exitTf.position, destination);

            if (score < bestScore)
            {
                bestScore = score;
                ladder = l;
                approachPoint = approachTf.position;
                exitPoint = exitTf.position;
                approachPath = pathToLadder;
            }
        }

        return ladder != null;
    }

    public void StartLadderTraversal(Ladder ladder, bool goingUp, Action onComplete)
    {
        if (!useLadders || ladder == null || _isTraversingLadder)
        {
            onComplete?.Invoke();
            return;
        }

        if (_ladderCoroutine != null)
            StopCoroutine(_ladderCoroutine);

        _ladderCompleteAction = onComplete;
        _ladderCoroutine = StartCoroutine(LadderTraversalRoutine(ladder, goingUp));
    }

    private IEnumerator LadderTraversalRoutine(Ladder ladder, bool goingUp)
    {
        _isTraversingLadder = true;
        _stateBeforeLadder = _state;
        _state = NPCState.ClimbingLadder;

        Transform startMount = goingUp ? ladder.bottomMountPoint : ladder.topMountPoint;
        Transform endMount   = goingUp ? ladder.topMountPoint    : ladder.bottomMountPoint;
        Transform endExit    = goingUp
            ? (ladder.topExitPoint != null ? ladder.topExitPoint : ladder.topMountPoint)
            : (ladder.bottomExitPoint != null ? ladder.bottomExitPoint : ladder.bottomMountPoint);

        if (startMount == null || endMount == null || endExit == null)
        {
            if (ladderDebugLogs) Debug.LogWarning($"{name}: Ladder traversal aborted - missing ladder anchors.");
            _isTraversingLadder = false;
            _ladderCoroutine = null;
            _state = _stateBeforeLadder;
            _ladderCompleteAction?.Invoke();
            _ladderCompleteAction = null;
            yield break;
        }

        if (ladderDebugLogs)
            Debug.Log($"{name}: StartLadderTraversal ladder={ladder.name} goingUp={goingUp}");

        if (AgentReady())
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        ForceIdlePose();
        ResetAllAnimatorTriggers();

        Vector3 ladderFacing = ladder.FacingForward;
        if (ladderFacing.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(ladderFacing, Vector3.up);
            transform.rotation = targetRot;
        }

        transform.position = startMount.position;

        DetachAgentForAnimation();

        if (animationController != null)
        {
            string trigger = goingUp ? climbUpTriggerName : climbDownTriggerName;
            if (!string.IsNullOrWhiteSpace(trigger))
                animationController.SetTrigger(trigger);
        }

        float distance = Vector3.Distance(startMount.position, endMount.position);
        float duration = Mathf.Max(0.1f, distance / Mathf.Max(0.01f, ladderClimbSpeed));

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);

            transform.position = Vector3.Lerp(startMount.position, endMount.position, u);

            if (ladderFacing.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(ladderFacing, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * ladderFacingSlerp);
            }

            yield return null;
        }

        transform.position = endExit.position;
        RestoreStandingBodyAt(endExit.position, ladderNavmeshSnapRadius);

        if (animationController != null)
        {
            ResetAllAnimatorTriggers();
            ForceReturnToLocomotion();
            ForceIdlePose();
        }

        _isTraversingLadder = false;
        _ladderCoroutine = null;
        _state = _stateBeforeLadder;

        Action done = _ladderCompleteAction;
        _ladderCompleteAction = null;
        done?.Invoke();
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
        if (agent == null) return;
        if (AgentReady())
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
        agent.velocity = Vector3.zero;
        agent.enabled = false;
    }

    public void ReattachAgentToNavmeshAtCurrentXZ()
    {
        if (agent == null) return;
        Vector3 desired = transform.position;
        if (TryGetNavmeshPoint(desired, out Vector3 navPos))
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
            Debug.LogWarning($"{name}: Could not find NavMesh near {desired} to reattach agent.");
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
            EnterState(NPCState.Patrolling);
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
        var mgr = FindFirstObjectByType<NPCManager>();
        if (mgr != null) mgr.RegisterNPC(this);
        else Debug.LogWarning($"{name}: No NPCManager found during Awake.");
    }

    public void Start()
    {
        if (animationController == null) animationController = GetComponentInChildren<Animator>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        if (_sitting == null) _sitting = GetComponent<NPCSittingBehaviour>();
        if (_lying == null) _lying = GetComponent<NPCLyingBehaviour>();
        if (_talk == null) _talk = GetComponent<NPCTalkBehaviour>();
        if (_combat == null) _combat = GetComponent<NPCCombatBehaviour>();

        _sitting?.Init(this);
        _lying?.Init(this);
        _talk?.Init(this);
        _combat?.Init(this);

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

        if (useStateMachine)
        {
            ForceReturnToLocomotion();
            EnterState(NPCState.Patrolling);
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

        if (useStateMachine && !_isPlayingTriggeredAnimation && !_isTraversingLadder)
        {
            if (_state == NPCState.Sitting || _state == NPCState.Lying)
                TickFSM(Time.deltaTime);
            else if (EnsureFSMCanRun())
                TickFSM(Time.deltaTime);
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

    public void SetTarget(Transform t)
    {
        currentTarget = t;
        if (currentTarget != null)
        {
            _lastKnownTargetPos = currentTarget.position;
            _hasLastKnownTargetPos = true;

            if (useStateMachine && !_isPlayingTriggeredAnimation && !_hasCommand
                && _state != NPCState.Sitting && _state != NPCState.Lying)
                EnterState(NPCState.Alerted);
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

    public void ForcePatrol()
    {
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
            EnterState(NPCState.Patrolling);
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
        EnterState(NPCState.Patrolling);
    }

    public void RequestSitDown()
    {
        if (_talk != null && _talk.IsConversationLocked) return;
        if (_sitting == null) { Debug.LogWarning($"{name}: No NPCSittingBehaviour found."); return; }

        InterruptAllTransientActions();
        useStateMachine = true;
        _hasCommand = true;
        _commandGoal = NPCState.Sitting;
        EnterState(NPCState.Sitting);
    }

    public void RequestStandUp()
    {
        if (_state != NPCState.Sitting || _sitting == null) return;

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
            _sitting.BeginStandUp();
        }
    }

    public void RequestLieDown()
    {
        if (_talk != null && _talk.IsConversationLocked) return;
        if (_lying == null) { Debug.LogWarning($"{name}: No NPCLyingBehaviour found."); return; }

        if (TryQueueActionAfterStand(StartLieCommandFresh)) return;
        StartLieCommandFresh();
    }

    public void RequestWakeUp()
    {
        if (_state != NPCState.Lying || _lying == null) return;
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
        _lying?.EnterLying();
    }

    public void SetTargetByNPC(NPC npcEnum)
    {
        var c = ResolveNPC(npcEnum);
        if (c == null) { Debug.LogWarning($"{name}: Could not find NPC '{npcEnum}'."); return; }
        if (c == this) { Debug.LogWarning($"{name}: Tried to target self."); return; }
        SetTarget(c.transform);
        if (_talk != null) _talk.TalkTargetController = c;
    }

    public void DebugApproachTargetNPC()
    {
        if (TryQueueActionAfterStand(() =>
        {
            InterruptAllTransientActions();
            useStateMachine = true; _hasCommand = false; _commandGoal = NPCState.Patrolling;
            SetTargetByNPC(debugTargetNPC);
            if (currentTarget != null) EnterState(NPCState.Approaching);
        })) return;

        InterruptAllTransientActions();
        useStateMachine = true; _hasCommand = false; _commandGoal = NPCState.Patrolling;
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
            _hasCommand = true; _commandGoal = NPCState.Attacking;
            float d = Vector3.Distance(transform.position, currentTarget.position);
            EnterState(d <= talkRange ? NPCState.Attacking : NPCState.Approaching);
        })) return;

        InterruptAllTransientActions();
        useStateMachine = true;
        SetTargetByNPC(debugTargetNPC);
        if (currentTarget == null) return;
        _hasCommand = true; _commandGoal = NPCState.Attacking;
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
            _hasCommand = true; _commandGoal = NPCState.Talk;
            float d = Vector3.Distance(transform.position, currentTarget.position);
            EnterState(d <= talkRange ? NPCState.Talk : NPCState.Approaching);
        })) return;

        InterruptAllTransientActions();
        useStateMachine = true;
        SetTargetByNPC(debugTargetNPC);
        if (currentTarget == null) return;
        _hasCommand = true; _commandGoal = NPCState.Talk;
        float d3 = Vector3.Distance(transform.position, currentTarget.position);
        EnterState(d3 <= talkRange ? NPCState.Talk : NPCState.Approaching);
    }

    public void BeginConversationAsTarget(NPCController speaker)
        => _talk?.BeginConversationAsTarget(speaker);

    public void EndConversationAsTarget(NPCController speaker)
        => _talk?.EndConversationAsTarget(speaker);

    public void EnterState(NPCState next)
    {
        NPCState prev = _state;

        if (prev == NPCState.Talk && next != NPCState.Talk)
            _talk?.UnregisterAsSpeaker();

        _state = next;
        _stateTimer = 0f;

        if (AgentReady()) agent.isStopped = false;

        switch (_state)
        {
            case NPCState.Patrolling:
                if (AgentReady()) { agent.speed = moveSpeed; agent.autoBraking = true; agent.isStopped = false; }
                _hasPatrolDestination = false;
                _patrolChangeTimer = UnityEngine.Random.Range(patrolChangeDirInterval.x, patrolChangeDirInterval.y);
                _patrolArriveTimer = 0f;
                break;

            case NPCState.Alerted:
                if (AgentReady()) { agent.speed = moveSpeed; agent.isStopped = true; agent.autoBraking = true; }
                break;

            case NPCState.Approaching:
                if (AgentReady())
                {
                    agent.speed = moveSpeed;
                    if (disableBrakingWhileApproaching) agent.autoBraking = false;
                    agent.isStopped = false;
                }
                _approachRepathTimer = 0f;
                _hasLastApproachDest = false;
                break;

            case NPCState.Attacking:
                if (AgentReady()) { agent.isStopped = true; agent.autoBraking = true; }
                Debug.Log($"{name} ATTACKING: TODO hook up attack logic/animation.");
                break;

            case NPCState.Seeking:
                if (AgentReady()) { agent.speed = moveSpeed * Mathf.Max(0.01f, seekingSpeedMultiplier); agent.isStopped = false; }
                _seekTimer = 0f;
                _hasPatrolDestination = false;
                _patrolChangeTimer = UnityEngine.Random.Range(patrolChangeDirInterval.x, patrolChangeDirInterval.y);
                _patrolArriveTimer = 0f;
                break;

            case NPCState.Talk:
                if (AgentReady()) { agent.isStopped = true; agent.autoBraking = true; agent.ResetPath(); }
                Debug.Log($"{name} TALK: holding idle until new command.");
                break;

            case NPCState.Sitting:
                if (prev != NPCState.Sitting) _sitting?.EnterSitting();
                break;

            case NPCState.Lying:
                if (prev != NPCState.Lying) _lying?.EnterLying();
                break;

            case NPCState.ClimbingLadder:
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

        if (!_hasCommand && _state != NPCState.Talk && _state != NPCState.Sitting && _state != NPCState.Lying && _state != NPCState.ClimbingLadder)
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
            case NPCState.Patrolling: TickPatrolling(dt, 1f); break;
            case NPCState.Alerted: TickAlerted(dt); break;
            case NPCState.Approaching: TickApproaching(dt); break;
            case NPCState.Attacking: _combat?.TickAttacking(dt); break;
            case NPCState.Seeking: TickSeeking(dt); break;
            case NPCState.Talk: _talk?.TickTalk(dt); break;
            case NPCState.Sitting:
                _hasCommand = true;
                _commandGoal = NPCState.Sitting;
                _sitting?.Tick(dt);
                break;
            case NPCState.Lying:
                _hasCommand = true;
                _commandGoal = NPCState.Lying;
                _lying?.Tick(dt);
                break;
            case NPCState.ClimbingLadder:
                break;
        }
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
        if (AgentReady()) agent.isStopped = true;

        if (currentTarget != null)
        {
            Vector3 to = currentTarget.position - transform.position; to.y = 0f;
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
                    ? NPCState.Approaching : NPCState.Seeking);
            }
            else EnterState(NPCState.Seeking);
        }
    }

    void TickApproaching(float dt)
    {
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
            EnterState(NPCState.Patrolling);
            return;
        }

        if (_hasCommand)
        {
            if (currentTarget != null)
            {
                float dist = Vector3.Distance(transform.position, currentTarget.position);
                if (_commandGoal == NPCState.Talk && dist <= talkRange) { EnterState(NPCState.Talk); return; }
                if (_commandGoal == NPCState.Attacking && dist <= talkRange) { EnterState(NPCState.Attacking); return; }
            }

            _approachRepathTimer -= dt;
            if (_approachRepathTimer <= 0f && currentTarget != null)
            {
                _approachRepathTimer = Mathf.Max(0.05f, approachRepathInterval);
                Vector3 newDest = currentTarget.position;
                if (!_hasLastApproachDest || (newDest - _lastApproachDest).sqrMagnitude >= approachTargetMoveThreshold * approachTargetMoveThreshold)
                {
                    _lastApproachDest = newDest;
                    _hasLastApproachDest = true;
                    agent.isStopped = false;
                    agent.SetDestination(newDest);
                }
            }
            return;
        }

        if (currentTarget == null)
        {
            EnterState(NPCState.Patrolling);
            return;
        }

        float d2 = Vector3.Distance(transform.position, currentTarget.position);
        if (d2 <= attackRange)
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

        bool inSitWalkup = (_state == NPCState.Sitting && _sitting != null &&
                            (_sitting.IsRoutingToLadder || _sitting.IsApproachingFront));
        bool inLieWalkup = (_state == NPCState.Lying && _lying != null &&
                            (_lying.IsRoutingToLadder || _lying.IsApproachingFront));

        bool blockLocomotion =
            _isTraversingLadder ||
            (_state == NPCState.Sitting && !inSitWalkup) ||
            (_state == NPCState.Lying && !inLieWalkup);

        bool allowNavLocomotion = AgentReady() && IsNavDriven() && !blockLocomotion;

        float movingX = 0f, movingY = 0f, blend = 0f;

        if (allowNavLocomotion)
        {
            Vector3 to = agent.steeringTarget - transform.position; to.y = 0f;
            Vector3 desiredDir = (to.sqrMagnitude > 0.0001f) ? to.normalized
                : (agent.desiredVelocity.sqrMagnitude > 0.0001f ? agent.desiredVelocity.normalized : transform.forward);

            Vector3 v = agent.velocity; v.y = 0f;
            Vector3 dv = agent.desiredVelocity; dv.y = 0f;
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

        animationController.SetFloat(paramMovingX, movingX, animDampTime, Time.deltaTime);
        animationController.SetFloat(paramMovingY, movingY, animDampTime, Time.deltaTime);
        animationController.SetFloat(paramBlend, blend, animDampTime, Time.deltaTime);
    }

    public void PlaySelectedTrigger()
    {
        if (triggerNames == null || triggerNames.Count == 0) { Debug.LogWarning($"{name}: No triggerNames."); return; }
        selectedTriggerIndex = Mathf.Clamp(selectedTriggerIndex, 0, triggerNames.Count - 1);
        PlayTrigger(triggerNames[selectedTriggerIndex]);
    }

    public void PlayTrigger(string triggerParam)
    {
        if (string.IsNullOrWhiteSpace(triggerParam)) { Debug.LogWarning($"{name}: Trigger param empty."); return; }
        if (animationController == null) { Debug.LogWarning($"{name}: No Animator."); return; }

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
        if (animationController == null) return;
        if (_triggerCoroutine != null) { StopCoroutine(_triggerCoroutine); _triggerCoroutine = null; }
        _isPlayingTriggeredAnimation = false;
        animationController.speed = 1f;
        ResetAllAnimatorTriggers();
        isPaused = _triggerPrevPaused;
        if (_triggerAgentWasValid && AgentReady()) agent.isStopped = _triggerWasStoppedBefore;
        if (useStateMachine && !isPaused) ForceReturnToLocomotion();
    }

    IEnumerator TriggerRoutine(string triggerParam)
    {
        _isPlayingTriggeredAnimation = true;
        _triggerPrevPaused = isPaused;
        _triggerAgentWasValid = AgentReady();
        _triggerHadPathBefore = _triggerAgentWasValid && agent.hasPath;
        _triggerWasStoppedBefore = _triggerAgentWasValid && agent.isStopped;
        isPaused = true;

        if (_triggerAgentWasValid) { agent.isStopped = true; agent.ResetPath(); }

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
        if (_triggerAgentWasValid && AgentReady()) agent.isStopped = _triggerWasStoppedBefore;
        _isPlayingTriggeredAnimation = false;
        _triggerCoroutine = null;

        if (useStateMachine && !isPaused) ForceReturnToLocomotion();
    }

    public void StopAndFace(Vector3 worldPos)
    {
        if (AgentReady()) { agent.isStopped = true; agent.ResetPath(); }
        ForceIdlePose();
        Vector3 to = worldPos - transform.position; to.y = 0f;
        if (to.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(to.normalized, Vector3.up),
            Time.deltaTime * (turnSmoothing * 1.5f));
    }

    void PreCollisionRenegotiate()
    {
        if (!IsNavDriven() || !AgentReady()) return;
        if (useStateMachine && (_state == NPCState.Alerted || _state == NPCState.Attacking
            || _state == NPCState.Talk || _state == NPCState.Sitting || _state == NPCState.Lying || _state == NPCState.ClimbingLadder)) return;
        if (agent.pathPending || !agent.hasPath) return;

        if (_renegotiateCooldownT > 0f) { _renegotiateCooldownT -= Time.deltaTime; return; }

        _renegotiateT += Time.deltaTime;
        if (_renegotiateT < renegotiateCheckInterval) return;
        _renegotiateT = 0f;

        Vector3 v = agent.velocity; v.y = 0f;
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

            Vector3 toOther = other.transform.position - transform.position; toOther.y = 0f;
            float d = toOther.magnitude;
            if (d < 0.0001f) continue;
            if (Vector3.Dot(fwd, toOther / d) < inFrontDotThreshold) continue;
            if (d < bestDist) { bestDist = d; closest = other; }
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
        Vector3 v = agent.velocity; v.y = 0f;
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
        if (_isTraversingLadder) return false;
        return useStateMachine;
    }

    private bool IsRecoveringBodyState()
    {
        if (_state == NPCState.Sitting && _sitting != null && _sitting.Phase != NPCSittingBehaviour.SitPhase.None) return true;
        if (_state == NPCState.Lying && _lying != null && _lying.Phase != NPCLyingBehaviour.LiePhase.None) return true;
        return false;
    }

    private bool TryQueueActionAfterStand(Action action)
    {
        if (_talk != null && _talk.IsConversationLocked)
            _talk.BreakConversationLockImmediate();

        if (!IsRecoveringBodyState()) return false;

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

        if (_triggerCoroutine != null) { StopCoroutine(_triggerCoroutine); _triggerCoroutine = null; }
        _isPlayingTriggeredAnimation = false;
        isPaused = false;

        if (animationController != null)
        {
            animationController.speed = 1f;
            ResetAllAnimatorTriggers();
        }

        _talk?.ClearAllSpeakers();

        if (_sidestepCoroutine != null) { StopCoroutine(_sidestepCoroutine); _sidestepCoroutine = null; }
        _hasResumeDestination = false;

        if (_ladderCoroutine != null) { StopCoroutine(_ladderCoroutine); _ladderCoroutine = null; }
        _isTraversingLadder = false;
        _ladderCompleteAction = null;

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
        if (returnToLocomotion && animationController != null) ForceReturnToLocomotion();
    }

    private void ResetLocomotionState()
    {
        _animVelocitySmoothed = Vector3.zero;
        _hasLastApproachDest = false;
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
            if (list[i] != null && list[i].thisNPC == npcEnum) return list[i];
        return null;
    }

    bool CanSeeTarget(Transform t)
    {
        if (t == null) return false;
        Vector3 to = t.position - transform.position;
        if (to.magnitude > visionRange) return false;

        Vector3 flatTo = to; flatTo.y = 0f;
        Vector3 fwd = transform.forward; fwd.y = 0f;

        if (flatTo.sqrMagnitude > 0.0001f && fwd.sqrMagnitude > 0.0001f)
            if (Vector3.Angle(fwd.normalized, flatTo.normalized) > visionFOV * 0.5f) return false;

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

        Debug.LogWarning($"{name}: Could not find NavMesh within {snapToNavMeshRadius}m ({context}).");
        return false;
    }

    static bool HasArrived(NavMeshAgent a, float arriveDist)
    {
        if (a == null || !a.isOnNavMesh || a.pathPending || !a.hasPath || float.IsInfinity(a.remainingDistance))
            return false;
        return a.remainingDistance <= Mathf.Max(arriveDist, a.stoppingDistance);
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
            if (ps[i].name == paramName) return ps[i].type;
        return null;
    }

    void SetAnimatorParamOn(Animator anim, string paramName, AnimatorControllerParameterType type)
    {
        switch (type)
        {
            case AnimatorControllerParameterType.Bool: anim.SetBool(paramName, true); break;
            case AnimatorControllerParameterType.Trigger: anim.ResetTrigger(paramName); anim.SetTrigger(paramName); break;
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
        if (!Application.isPlaying) EditorGUILayout.HelpBox("Buttons work in Play Mode only.", MessageType.None);

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