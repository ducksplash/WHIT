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

    [Header("ACT Animation")]
    [Tooltip("How long we wait for the animator to leave the current state after firing the ACT param.")]
    public float actEnterTimeout = 1.0f;

    [Tooltip("If ACT is NOT timed, we'll wait for the animation to finish, but never longer than this.")]
    public float actMaxUnTimedDuration = 8.0f;

    [Tooltip("Small buffer after ACT ends to help blend back cleanly.")]
    public float actExitBuffer = 0.05f;

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

    void Reset()
    {
        animationController = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    public void Start()
    {
        // MUST be first thing this does:

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

        // determine loop return point (first waypoint encountered in the routine)
        _loopReturnToFirstWaypoint = FindFirstWaypointInRoutine(_activeBehaviours);

        agent.isStopped = false;
        agent.speed = moveSpeed;
        agent.angularSpeed = angularSpeed;
        agent.acceleration = acceleration;

        _routineCoroutine = StartCoroutine(RoutineCoroutine());
    }

    NPCRoutine FindRoutineAsset(Routine routineType)
    {
        // Prefer exact match on RoutineType; first match wins
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
            // Play through the behaviours once
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

            // Finished one pass of the routine
            if (_activeRoutineAsset.looping)
            {
                // Requirement: if looping and there were waypoints, walk back to the first waypoint before restarting
                if (_loopReturnToFirstWaypoint.HasValue)
                {
                    yield return GoToSinglePoint(_loopReturnToFirstWaypoint.Value);
                    yield return SmoothStopAndIdle();
                }

                // then loop again
                continue;
            }

            break; // not looping -> finish
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

        // You wired this up:
        GameMaster.Instance.DialogueManager.PlayDialogue(b.selectedDialogue, b.timer, DialogueType.normal);

        float wait = (b.isTimed && b.timer > 0f) ? b.timer : 0f;
        if (wait > 0f)
            yield return WaitSecondsRespectPause(wait);
        else
            yield return null;
    }

    /// <summary>
    /// Proper ACT implementation:
    /// - Stops movement cleanly (so you don't slide during an act).
    /// - Fires an animator parameter named by b.animationState (Trigger/Bool/Int/Float supported).
    /// - Waits either:
    ///     - b.timer (if b.isTimed), OR
    ///     - until the entered ACT state finishes (normalizedTime >= 1) or returns to start state,
    ///       with a safety cap actMaxUnTimedDuration.
    /// - Clears the parameter ASAP to prevent AnyState re-firing loops.
    /// </summary>
    IEnumerator DoAct(NPCBehaviour b)
    {
        // Stop locomotion first so the act doesn't play while walking.
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            yield return SmoothStopAndIdle();

        if (animationController == null)
        {
            Debug.LogWarning($"{name}: ACT requested but Animator is missing.");
            yield break;
        }

        // We expect your NPCBehaviour to hold the animator parameter name in animationState.
        string paramName = b.animationState;
        if (string.IsNullOrWhiteSpace(paramName))
        {
            Debug.LogWarning($"{name}: ACT behaviour '{b.name}' has empty animationState (expected animator parameter name).");
            // fall back to timer if present
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

        // Capture where we started (usually locomotion/blend tree state)
        int startHash = animationController.GetCurrentAnimatorStateInfo(layer).fullPathHash;

        // Fire the param
        SetAnimatorParamOn(animationController, paramName, pType.Value);

        // Wait for animator to react (transition or state change)
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

        // Clear ASAP to avoid AnyState re-trigger loops.
        SetAnimatorParamOff(animationController, paramName, pType.Value);

        // Triggers: extra safety reset next frame too (covers edge cases)
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
                // Prefer: ACT state completes (works for non-looping clips)
                // NOTE: if ACT state loops, normalizedTime keeps increasing; we still break on >= 1
                // but looping states might never exit, so safety cap will end it.
                if (entered && !animationController.IsInTransition(layer) &&
                    info.fullPathHash == actHash && info.normalizedTime >= 1f)
                    break;

                // Or: returned to start locomotion state (common setup)
                if (entered && !animationController.IsInTransition(layer) &&
                    info.fullPathHash == startHash)
                    break;
            }

            t += Time.deltaTime;
            yield return null;
        }

        // Final safety clear (covers bool/int/float/trigger)
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
            while (isPaused) yield return null;

            if (!EnsureAgentOnNavMesh("GoBehaviourStep")) yield break;

            Vector3 target = b.waypointVectors[i];

            agent.isStopped = false;
            agent.SetDestination(target);

            while (isRunningRoutine && !isPaused && !HasArrived(agent, arriveDistance))
                yield return null;

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

        while (isRunningRoutine && !isPaused && !HasArrived(agent, arriveDistance))
            yield return null;

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
    // Animator param helpers (ACT)
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
                // Make trigger "edge-triggered"
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
        if (!a.hasPath) return true;

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

        // Helpful debug: show which routine asset is assigned for the selected enum
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
    }
}
#endif