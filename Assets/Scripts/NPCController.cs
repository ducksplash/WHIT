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

    [Header("Waypoints")]
    public List<Transform> waypoints = new List<Transform>();
    [Tooltip("Start waypoint index (inclusive).")]
    public int startIndex = 0;
    [Tooltip("End waypoint index (inclusive). If -1, uses last waypoint.")]
    public int endIndex = -1;

    [Header("Stopping")]
    [Tooltip("If true, NPC will stop at intermediate waypoints between the start and end.")]
    public bool stopAtIntermediateWaypoints = true;
    [Tooltip("Seconds to wait when stopping at a waypoint.")]
    public float stopDuration = 2f;
    [Tooltip("How close to a waypoint counts as 'arrived'.")]
    public float arriveDistance = 0.3f;

    [Header("Movement")]
    public float moveSpeed = 1.8f;
    public float angularSpeed = 360f;
    public float acceleration = 8f;

    [Header("NavMesh Placement")]
    [Tooltip("How far to search to snap this NPC onto the NavMesh if it spawns slightly off-mesh.")]
    public float snapToNavMeshRadius = 2f;
    [Tooltip("If true, Go() will try to snap the agent onto the NavMesh automatically.")]
    public bool autoSnapToNavMeshOnGo = true;

    [Header("Animator Params")]
    public string paramBlend = "Blend";
    public string paramMovingX = "MovingX";
    public string paramMovingY = "MovingY";
    public string paramCrouching = "Crouching"; // not used yet
    public string paramGrounded = "Grounded";   // not used yet
    public string paramJump = "Jump";           // not used yet

    // Runtime state
    [NonSerialized] public bool isRunningRoute = false;
    [NonSerialized] public bool isPaused = false;

    int _currentIndex;
    int _resolvedEndIndex;
    Coroutine _routeRoutine;

    void Reset()
    {
        animationController = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    public void Start()
    {
        // MUST be first thing this does:
        GameMaster.Instance.NPCManager.RegisterNPC(this);

        // Then init:
        if (animationController == null) animationController = GetComponentInChildren<Animator>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.angularSpeed = angularSpeed;
            agent.acceleration = acceleration;
            agent.stoppingDistance = Mathf.Max(agent.stoppingDistance, arriveDistance);
            agent.updateRotation = true;
        }
    }

    void Update()
    {
        UpdateAnimatorFromMovement();
    }

    // -----------------------
    // Public controls
    // -----------------------

    public void Go()
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogWarning($"{name}: No waypoints assigned.");
            return;
        }

        if (agent == null)
        {
            Debug.LogWarning($"{name}: No NavMeshAgent found. Add one to use waypoint navigation.");
            return;
        }

        // Ensure the agent is actually placed on a NavMesh (snap if possible)
        if (!EnsureAgentOnNavMesh("Go")) return;

        if (_routeRoutine != null)
            StopCoroutine(_routeRoutine);

        isPaused = false;
        isRunningRoute = true;

        _currentIndex = Mathf.Clamp(startIndex, 0, waypoints.Count - 1);
        _resolvedEndIndex = (endIndex < 0) ? waypoints.Count - 1 : Mathf.Clamp(endIndex, 0, waypoints.Count - 1);

        agent.isStopped = false;
        agent.speed = moveSpeed;
        agent.angularSpeed = angularSpeed;
        agent.acceleration = acceleration;

        _routeRoutine = StartCoroutine(RouteRoutine());
    }

    public void Stop()
    {
        isRunningRoute = false;
        isPaused = false;

        if (_routeRoutine != null)
        {
            StopCoroutine(_routeRoutine);
            _routeRoutine = null;
        }

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    public void Pause()
    {
        if (!isRunningRoute) return;
        if (!EnsureAgentOnNavMesh("Pause")) return;

        isPaused = true;
        agent.isStopped = true;
    }

    public void Resume()
    {
        if (!isRunningRoute) return;
        if (!EnsureAgentOnNavMesh("Resume")) return;

        isPaused = false;
        agent.isStopped = false;
    }

    public void JumpToWaypoint(int index)
    {
        if (waypoints == null || waypoints.Count == 0) return;
        if (agent == null) return;

        if (!EnsureAgentOnNavMesh("JumpToWaypoint")) return;

        index = Mathf.Clamp(index, 0, waypoints.Count - 1);
        _currentIndex = index;

        // If route isn't running, just set destination as a preview (then stop)
        if (!isRunningRoute)
        {
            agent.isStopped = false;
            agent.SetDestination(waypoints[_currentIndex].position);
            agent.isStopped = true;
        }
        else
        {
            agent.SetDestination(waypoints[_currentIndex].position);
        }
    }

    // -----------------------
    // Internal route logic
    // -----------------------

    IEnumerator RouteRoutine()
    {
        // Direction: startIndex -> endIndex (increasing or decreasing)
        int step = (_resolvedEndIndex >= _currentIndex) ? 1 : -1;

        while (isRunningRoute)
        {
            // Pause handling
            while (isPaused)
                yield return null;

            if (!EnsureAgentOnNavMesh("RouteRoutine"))
                yield break;

            Transform target = waypoints[_currentIndex];
            if (target == null)
            {
                Debug.LogWarning($"{name}: Waypoint at index {_currentIndex} is null, skipping.");
                if (_currentIndex == _resolvedEndIndex) break;
                _currentIndex += step;
                continue;
            }

            agent.isStopped = false;
            agent.SetDestination(target.position);

            // Wait until we arrive
            while (isRunningRoute && !isPaused && !HasArrived(agent, arriveDistance))
                yield return null;

            if (!isRunningRoute) yield break;

            // Stop at the waypoint (if on navmesh)
            if (agent.isOnNavMesh)
                agent.isStopped = true;

            bool isEnd = (_currentIndex == _resolvedEndIndex);
            bool isIntermediate = !isEnd;

            // Stop delay at intermediate waypoints
            if (isIntermediate && stopAtIntermediateWaypoints && stopDuration > 0f)
            {
                float t = 0f;
                while (t < stopDuration)
                {
                    if (!isRunningRoute) yield break;
                    while (isPaused) yield return null;

                    t += Time.deltaTime;
                    yield return null;
                }
            }

            if (isEnd)
            {
                isRunningRoute = false;

                if (agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                    agent.ResetPath();
                }
                break;
            }

            _currentIndex += step;
        }

        _routeRoutine = null;
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

        // Try to snap to nearest NavMesh
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, snapToNavMeshRadius, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            return agent.isOnNavMesh;
        }

        Debug.LogWarning($"{name}: Could not find NavMesh within {snapToNavMeshRadius}m to snap ({context}).");
        return false;
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

        if (!isRunningRoute || isPaused)
        {
            movingX = 0f;
            movingY = 0f;
            blend = 0f;
        }

        animationController.SetFloat(paramMovingX, movingX);
        animationController.SetFloat(paramMovingY, movingY);
        animationController.SetFloat(paramBlend, blend);
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
        EditorGUILayout.LabelField("NPC Controls", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Go")) npc.Go();
            if (GUILayout.Button("Pause")) npc.Pause();
            if (GUILayout.Button("Resume")) npc.Resume();
            if (GUILayout.Button("Stop")) npc.Stop();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Jump To Waypoint", EditorStyles.miniBoldLabel);

            if (npc.waypoints != null && npc.waypoints.Count > 0)
            {
                // Quick buttons for first/last + slider for any index
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("First")) npc.JumpToWaypoint(0);
                if (GUILayout.Button("Last")) npc.JumpToWaypoint(npc.waypoints.Count - 1);
                EditorGUILayout.EndHorizontal();

                int idx = EditorGUILayout.IntSlider("Index", 0, 0, npc.waypoints.Count - 1);
                if (GUILayout.Button("Jump"))
                    npc.JumpToWaypoint(idx);
            }
            else
            {
                EditorGUILayout.HelpBox("Assign waypoint Transforms in the Waypoints list to enable navigation.", MessageType.Info);
            }
        }

        EditorGUILayout.Space(6);
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Controls are enabled in Play Mode.", MessageType.None);
        }
    }
}
#endif