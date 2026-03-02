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
    [Header("Identity")]
    public NPC thisNPC = NPC.EimearScott;

    [Header("Components")]
    public Animator animationController;
    public NavMeshAgent agent;

    [Header("Routines (Scriptable Objects)")]
    [Tooltip("Assign NPCRoutine ScriptableObjects here.")]
    public List<NPCRoutine> routines = new List<NPCRoutine>();

    [Tooltip("Select which routine to play (by enum).")]
    public Routine selectedRoutine = Routine.idle;

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

    [Tooltip("If true, Play/Go will try to snap the agent onto the NavMesh automatically.")]
    public bool autoSnapToNavMeshOnGo = true;

    [Header("Animator Params")]
    public string paramBlend = "Blend";
    public string paramMovingX = "MovingX";
    public string paramMovingY = "MovingY";

    [Header("ACT Animation (Routine Behaviour)")]
    [Tooltip("How long we wait for the animator to leave the current state after firing the ACT param.")]
    public float actEnterTimeout = 1.0f;

    [Tooltip("If ACT is NOT timed, we'll wait for the animation to finish, but never longer than this.")]
    public float actMaxUnTimedDuration = 8.0f;

    [Tooltip("Small buffer after ACT ends to help blend back cleanly.")]
    public float actExitBuffer = 0.05f;

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

    // Runtime state
    public bool playRoutineOnStart = false;
    [NonSerialized] public bool isRunningRoutine = false;
    [NonSerialized] public bool isPaused = false;

    bool _reverseAnimations = false;
    Coroutine _routineCoroutine;

    NPCRoutine _activeRoutineAsset;
    List<NPCBehaviour> _activeBehaviours;

    // For looping routines: we must return to the FIRST waypoint before restarting
    Vector3? _loopReturnToFirstWaypoint;

    // Trigger play coroutine
    Coroutine _triggerCoroutine;
    bool _isPlayingTriggeredAnimation = false;

    // Trigger restore snapshot
    bool _triggerSnapshotValid = false;
    bool _triggerPrevPaused;
    bool _triggerAgentWasValid;
    bool _triggerHadPathBefore;
    bool _triggerWasStoppedBefore;
    int _triggerStartStateHash; // animator state before trigger

    void Reset()
    {
        animationController = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    public void Start()
    {
        if (GameMaster.Instance != null)
        {
            GameMaster.Instance.NPCManager.RegisterNPC(this);
        }

        if (animationController == null) animationController = GetComponentInChildren<Animator>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();

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
        }

        if (playRoutineOnStart) PlaySelectedRoutine();
    }

    void Update()
    {
        UpdateAnimatorFromMovement();
        UpdateFacing();
    }

    // -----------------------
    // Routine controls
    // -----------------------

    public void PlaySelectedRoutine()
    {
        _reverseAnimations = false;
        StartRoutineInternal(selectedRoutine);
    }

    public void ReverseSelectedRoutine()
    {
        _reverseAnimations = true;
        StartRoutineInternal(selectedRoutine);
    }

    public void PlayRoutine(Routine routineType)
    {
        _reverseAnimations = false;
        StartRoutineInternal(routineType);
    }

    public void ReverseRoutine(Routine routineType)
    {
        _reverseAnimations = true;
        StartRoutineInternal(routineType);
    }

    public void StopRoutine()
    {
        isRunningRoutine = false;
        isPaused = false;

        if (_routineCoroutine != null)
        {
            StopCoroutine(_routineCoroutine);
            _routineCoroutine = null;
        }

        _activeRoutineAsset = null;
        _activeBehaviours = null;
        _loopReturnToFirstWaypoint = null;

        HardStopAgent("StopRoutine");
        ForceIdlePose();
    }

    void StartRoutineInternal(Routine routineType)
    {
        if (routines == null || routines.Count == 0)
        {
            Debug.LogWarning($"{name}: No NPCRoutine assets assigned.");
            return;
        }

        NPCRoutine routineAsset = FindRoutineAsset(routineType);
        if (routineAsset == null)
        {
            Debug.LogWarning($"{name}: No NPCRoutine asset found for routine type '{routineType}'.");
            return;
        }

        if (routineAsset.RoutineBehaviours == null || routineAsset.RoutineBehaviours.Count == 0)
        {
            Debug.LogWarning($"{name}: Routine '{routineType}' has no behaviours.");
            return;
        }

        if (agent == null)
        {
            Debug.LogWarning($"{name}: No NavMeshAgent found. Add one to use 'go' behaviours.");
            return;
        }

        if (!EnsureAgentOnNavMesh("StartRoutine")) return;

        if (_routineCoroutine != null)
            StopCoroutine(_routineCoroutine);

        isPaused = false;
        isRunningRoutine = true;

        selectedRoutine = routineType;
        _activeRoutineAsset = routineAsset;
        _activeBehaviours = routineAsset.RoutineBehaviours;

        _loopReturnToFirstWaypoint = FindFirstWaypointInRoutine(_activeBehaviours);

        agent.isStopped = false;
        agent.speed = moveSpeed;
        agent.angularSpeed = angularSpeed;
        agent.acceleration = acceleration;

        _routineCoroutine = StartCoroutine(RoutineCoroutine());
    }

    NPCRoutine FindRoutineAsset(Routine routineType)
    {
        for (int i = 0; i < routines.Count; i++)
        {
            NPCRoutine r = routines[i];
            if (r != null && r.RoutineType == routineType)
                return r;
        }
        return null;
    }

    Vector3? FindFirstWaypointInRoutine(List<NPCBehaviour> behaviours)
    {
        if (behaviours == null) return null;

        for (int i = 0; i < behaviours.Count; i++)
        {
            NPCBehaviour b = behaviours[i];
            if (b == null) continue;

            if (b.BehaviourType == Behaviour.go && b.waypointVectors != null && b.waypointVectors.Count > 0)
                return b.waypointVectors[0];
        }

        return null;
    }

    IEnumerator RoutineCoroutine()
    {
        if (_activeRoutineAsset == null || _activeBehaviours == null || _activeBehaviours.Count == 0)
        {
            isRunningRoutine = false;
            _routineCoroutine = null;
            yield break;
        }

        while (isRunningRoutine)
        {
            for (int i = 0; i < _activeBehaviours.Count; i++)
            {
                if (!isRunningRoutine) yield break;

                while (isPaused)
                    yield return null;

                NPCBehaviour b = _activeBehaviours[i];
                if (b == null)
                {
                    Debug.LogWarning($"{name}: Routine item {i} is null, skipping.");
                    continue;
                }

                switch (b.BehaviourType)
                {
                    case Behaviour.idle: yield return DoIdle(b); break;
                    case Behaviour.say:  yield return DoSay(b);  break;
                    case Behaviour.go:   yield return DoGo(b);   break;
                    case Behaviour.act:  yield return DoAct(b);  break;
                    case Behaviour.die:  yield return DoDie(b);  break;
                    default:
                        Debug.LogWarning($"{name}: Unknown behaviour type {b.BehaviourType} at index {i}.");
                        break;
                }
            }

            if (_activeRoutineAsset.looping)
            {
                if (_loopReturnToFirstWaypoint.HasValue)
                {
                    yield return GoToSinglePoint(_loopReturnToFirstWaypoint.Value);
                    yield return SmoothStopAndIdle();
                }

                continue;
            }

            break;
        }

        isRunningRoutine = false;
        _routineCoroutine = null;
        _activeRoutineAsset = null;
        _activeBehaviours = null;
        _loopReturnToFirstWaypoint = null;

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            yield return SmoothStopAndIdle();

        ForceIdlePose();
    }

    // -----------------------
    // Behaviour executors
    // -----------------------

    IEnumerator DoIdle(NPCBehaviour b)
    {
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            yield return SmoothStopAndIdle();

        float wait = (b.isTimed && b.timer > 0f) ? b.timer : 0f;
        if (wait > 0f)
            yield return WaitSecondsRespectPause(wait);
        else
            yield return null;
    }

    IEnumerator DoSay(NPCBehaviour b)
    {
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            yield return SmoothStopAndIdle();

        GameMaster.Instance.DialogueManager.PlayDialogue(b.selectedDialogue, b.timer, DialogueType.normal);

        float wait = (b.isTimed && b.timer > 0f) ? b.timer : 0f;
        if (wait > 0f)
            yield return WaitSecondsRespectPause(wait);
        else
            yield return null;
    }

    IEnumerator DoAct(NPCBehaviour b)
    {
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            yield return SmoothStopAndIdle();

        if (animationController == null)
        {
            Debug.LogWarning($"{name}: ACT requested but Animator is missing.");
            yield break;
        }

        string paramName = b.animationState;
        if (string.IsNullOrWhiteSpace(paramName))
        {
            Debug.LogWarning($"{name}: ACT behaviour '{b.name}' has empty animationState.");
            float fallbackWait = (b.isTimed && b.timer > 0f) ? b.timer : 0f;
            if (fallbackWait > 0f) yield return WaitSecondsRespectPause(fallbackWait);
            else yield return null;
            yield break;
        }

        AnimatorControllerParameterType? pType = GetAnimatorParameterType(animationController, paramName);
        if (pType == null)
        {
            Debug.LogWarning($"{name}: Animator has no parameter named '{paramName}' (ACT behaviour '{b.name}').");
            float fallbackWait = (b.isTimed && b.timer > 0f) ? b.timer : 0f;
            if (fallbackWait > 0f) yield return WaitSecondsRespectPause(fallbackWait);
            else yield return null;
            yield break;
        }

        const int layer = 0;
        int startHash = animationController.GetCurrentAnimatorStateInfo(layer).fullPathHash;

        SetAnimatorParamOn(animationController, paramName, pType.Value);

        bool entered = false;
        int actHash = startHash;

        float enterT = 0f;
        while (isRunningRoutine && enterT < actEnterTimeout)
        {
            while (isPaused) yield return null;

            AnimatorStateInfo info = animationController.GetCurrentAnimatorStateInfo(layer);

            if (animationController.IsInTransition(layer) || info.fullPathHash != startHash)
            {
                entered = true;
                actHash = info.fullPathHash;
                break;
            }

            enterT += Time.deltaTime;
            yield return null;
        }

        SetAnimatorParamOff(animationController, paramName, pType.Value);

        if (pType.Value == AnimatorControllerParameterType.Trigger)
        {
            yield return null;
            animationController.ResetTrigger(paramName);
        }

        bool hasTimer = b.isTimed && b.timer > 0f;
        float maxWait = hasTimer ? b.timer : actMaxUnTimedDuration;

        float t = 0f;
        while (isRunningRoutine && t < maxWait)
        {
            while (isPaused) yield return null;

            AnimatorStateInfo info = animationController.GetCurrentAnimatorStateInfo(layer);

            if (!hasTimer)
            {
                if (entered && !animationController.IsInTransition(layer) &&
                    info.fullPathHash == actHash && info.normalizedTime >= 1f)
                    break;

                if (entered && !animationController.IsInTransition(layer) &&
                    info.fullPathHash == startHash)
                    break;
            }

            t += Time.deltaTime;
            yield return null;
        }

        SetAnimatorParamOff(animationController, paramName, pType.Value);
        if (pType.Value == AnimatorControllerParameterType.Trigger)
            animationController.ResetTrigger(paramName);

        if (actExitBuffer > 0f)
            yield return WaitSecondsRespectPause(actExitBuffer);
        else
            yield return null;
    }

    IEnumerator DoDie(NPCBehaviour b)
    {
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            yield return SmoothStopAndIdle();

        Debug.Log($"{name} DIE: (hook up death later) Behaviour asset = {b.name}");

        float wait = (b.isTimed && b.timer > 0f) ? b.timer : 0f;
        if (wait > 0f)
            yield return WaitSecondsRespectPause(wait);
        else
            yield return null;
    }

    IEnumerator DoGo(NPCBehaviour b)
    {
        if (!EnsureAgentOnNavMesh("GoBehaviour")) yield break;

        if (b.waypointVectors == null || b.waypointVectors.Count == 0)
        {
            Debug.LogWarning($"{name}: GO behaviour has no waypointVectors in {b.name}.");
            yield break;
        }

        agent.isStopped = false;

        for (int i = 0; i < b.waypointVectors.Count; i++)
        {
            if (!isRunningRoutine) yield break;

            while (isPaused)
                yield return null;

            if (!EnsureAgentOnNavMesh("GoBehaviourStep")) yield break;

            Vector3 target = b.waypointVectors[i];

            agent.isStopped = false;
            agent.SetDestination(target);

            while (isRunningRoutine && !HasArrived(agent, arriveDistance))
            {
                while (isPaused) yield return null;
                yield return null;
            }

            if (!isRunningRoutine) yield break;

            if (arriveSettleTime > 0f)
            {
                float tSettle = 0f;
                while (tSettle < arriveSettleTime)
                {
                    if (!isRunningRoutine) yield break;
                    while (isPaused) yield return null;

                    tSettle += Time.deltaTime;
                    yield return null;
                }
            }

            yield return SmoothStopAndIdle();

            if (b.isTimed && b.timer > 0f)
                yield return WaitSecondsRespectPause(b.timer);
        }
    }

    IEnumerator GoToSinglePoint(Vector3 target)
    {
        if (!EnsureAgentOnNavMesh("LoopReturn")) yield break;

        agent.isStopped = false;
        agent.SetDestination(target);

        while (isRunningRoutine && !HasArrived(agent, arriveDistance))
        {
            while (isPaused) yield return null;
            yield return null;
        }

        if (!isRunningRoutine) yield break;

        if (arriveSettleTime > 0f)
        {
            float tSettle = 0f;
            while (tSettle < arriveSettleTime)
            {
                if (!isRunningRoutine) yield break;
                while (isPaused) yield return null;

                tSettle += Time.deltaTime;
                yield return null;
            }
        }
    }

    IEnumerator SmoothStopAndIdle()
    {
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
            yield break;

        agent.isStopped = true;

        float t = 0f;
        while (t < stopFadeOutTime)
        {
            if (!isRunningRoutine) yield break;
            while (isPaused) yield return null;

            if (agent.velocity.sqrMagnitude <= (idleVelocityThreshold * idleVelocityThreshold))
                break;

            t += Time.deltaTime;
            yield return null;
        }

        agent.ResetPath();
    }

    IEnumerator WaitSecondsRespectPause(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            if (!isRunningRoutine) yield break;

            while (isPaused)
                yield return null;

            t += Time.deltaTime;
            yield return null;
        }
    }

    // -----------------------
    // Triggered Animations API (Naurani merged)
    // -----------------------

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

        // Restore routine/agent and snap animator back to pre-trigger state
        RestoreAfterTriggeredAnimation(forceAnimatorBackToStartState: true);

        // Helps blend tree settle immediately
        ForceIdlePose();
    }

    void RestoreAfterTriggeredAnimation(bool forceAnimatorBackToStartState)
    {
        if (_triggerSnapshotValid)
            isPaused = _triggerPrevPaused;

        if (_triggerSnapshotValid && _triggerAgentWasValid && agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            if (isRunningRoutine && _triggerHadPathBefore && !_triggerWasStoppedBefore)
                agent.isStopped = false;
            else
                agent.isStopped = _triggerWasStoppedBefore;
        }

        if (forceAnimatorBackToStartState && _triggerSnapshotValid && animationController != null)
        {
            animationController.speed = 1f;

            if (_triggerStartStateHash != 0)
            {
                animationController.Play(_triggerStartStateHash, 0, 0f);
                animationController.Update(0f);
            }
        }

        _isPlayingTriggeredAnimation = false;
        _triggerSnapshotValid = false;
    }

    IEnumerator TriggerRoutine(string triggerParam)
    {
        _isPlayingTriggeredAnimation = true;

        bool prevPaused = isPaused;

        bool agentWasValid = (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh);
        bool hadPathBefore = agentWasValid && agent.hasPath;
        bool wasStoppedBefore = agentWasValid && agent.isStopped;

        isPaused = true;

        if (agentWasValid)
        {
            // IMPORTANT: do NOT ResetPath, we want to resume where we were going
            agent.isStopped = true;
        }

        AnimatorControllerParameterType? pType = GetAnimatorParameterType(animationController, triggerParam);
        if (pType == null)
        {
            Debug.LogWarning($"{name}: Animator has no parameter named '{triggerParam}'.");
            isPaused = prevPaused;
            if (agentWasValid) agent.isStopped = wasStoppedBefore;
            _isPlayingTriggeredAnimation = false;
            yield break;
        }

        const int layer = 0;
        int startHash = animationController.GetCurrentAnimatorStateInfo(layer).fullPathHash;

        // Snapshot state so StopTriggeredAnimation restores correctly
        _triggerSnapshotValid = true;
        _triggerPrevPaused = prevPaused;
        _triggerAgentWasValid = agentWasValid;
        _triggerHadPathBefore = hadPathBefore;
        _triggerWasStoppedBefore = wasStoppedBefore;
        _triggerStartStateHash = startHash;

        animationController.speed = 1f;

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

        _triggerCoroutine = null;
        RestoreAfterTriggeredAnimation(forceAnimatorBackToStartState: false);
    }

    // -----------------------
    // Animator param helpers
    // -----------------------

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

    // -----------------------
    // NavMesh safety helpers
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

    void HardStopAgent(string context)
    {
        if (agent == null || !agent.isActiveAndEnabled) return;
        if (!agent.isOnNavMesh) return;

        agent.isStopped = true;
        agent.ResetPath();
    }

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

        Vector3 velocity = Vector3.zero;
        if (agent != null && agent.enabled && agent.isOnNavMesh)
            velocity = agent.velocity;

        Vector3 localVel = transform.InverseTransformDirection(velocity);
        float speed = velocity.magnitude;

        float movingY = Mathf.Clamp(localVel.z, -1f, 1f);
        float movingX = Mathf.Clamp(localVel.x, -1f, 1f);
        float blend = (moveSpeed <= 0.001f) ? 0f : Mathf.Clamp01(speed / moveSpeed);

        if (_reverseAnimations && speed > 0.01f)
            movingY = -Mathf.Abs(movingY);

        if (!isRunningRoutine || isPaused)
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
        if (!isRunningRoutine || isPaused) return;

        Vector3 v = agent.velocity;
        v.y = 0f;

        if (v.sqrMagnitude < 0.01f) return;

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

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Routine Controls", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Play Selected")) npc.PlaySelectedRoutine();
            if (GUILayout.Button("Reverse Selected")) npc.ReverseSelectedRoutine();
            if (GUILayout.Button("Stop")) npc.StopRoutine();
            EditorGUILayout.EndHorizontal();
        }

        if (!Application.isPlaying)
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.HelpBox("Routine controls are enabled in Play Mode.", MessageType.None);
        }

        if (npc.routines != null && npc.routines.Count > 0)
        {
            NPCRoutine found = null;
            for (int i = 0; i < npc.routines.Count; i++)
            {
                var r = npc.routines[i];
                if (r != null && r.RoutineType == npc.selectedRoutine)
                {
                    found = r;
                    break;
                }
            }

            if (found == null)
                EditorGUILayout.HelpBox($"No NPCRoutine asset in 'routines' matches selectedRoutine = {npc.selectedRoutine}.", MessageType.Warning);
            else
                EditorGUILayout.HelpBox($"Selected routine asset: {found.name} (looping={found.looping}, behaviours={found.RoutineBehaviours?.Count ?? 0})", MessageType.Info);
        }

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
    }
}
#endif