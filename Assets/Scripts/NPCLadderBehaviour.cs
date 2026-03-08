// NPCLadderBehaviour.cs
// Handles ladder route-finding and traversal (ascent and descent).
// Attach alongside NPCController. Init() is called by NPCController.Start().

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCLadderBehaviour : NPCBehaviourBase
{
    // =========================================================
    // Inspector
    // =========================================================
    [Header("Ladders")]
    public bool   useLadders                  = true;
    public float  ladderApproachArriveDistance = 0.45f;
    public float  ladderClimbSpeed             = 1.6f;
    public float  ladderFacingSlerp            = 18f;
    public string climbUpTriggerName           = "ClimbUpLadder";
    public string climbDownTriggerName         = "ClimbDownLadder";
    public float  ladderNavmeshSnapRadius      = 3f;
    public bool   ladderDebugLogs              = true;
    public string hoistUpTriggerName           = "HoistUp";
    public float  ladderTopStopOffset          = 0.9f;
    public float  hoistDuration                = 0.7f;

    [Tooltip("How far in front of the mount point the NPC stops before aligning and climbing.")]
    public float preLadderApproachOffset = 1.0f;

    [Tooltip("Yaw tolerance in degrees before the NPC is considered aligned with the ladder and snaps on.")]
    public float ladderAlignToleranceDeg = 8f;

    [Tooltip("If the target is within this vertical distance, treat it as the same level and do not use a ladder.")]
    public float sameLevelHeightTolerance = 0.5f;

    // =========================================================
    // Runtime state
    // =========================================================
    public bool IsTraversingLadder => _isTraversingLadder;

    private Coroutine              _ladderCoroutine;
    private bool                   _isTraversingLadder  = false;
    private NPCController.NPCState _stateBeforeLadder   = NPCController.NPCState.Patrolling;
    private Action                 _ladderCompleteAction;

    // =========================================================
    // Public API
    // =========================================================

    /// <summary>
    /// Returns true when a height difference to <paramref name="target"/> requires a ladder.
    /// Sets <paramref name="goingUp"/> accordingly.
    /// </summary>
    public bool NeedLadderForTarget(Transform target, out bool goingUp)
    {
        goingUp = false;

        if (!useLadders || target == null || npc == null)
            return false;

        float deltaY = target.position.y - npc.transform.position.y;

        if (deltaY > sameLevelHeightTolerance)
        {
            goingUp = true;
            return true;
        }

        if (deltaY < -sameLevelHeightTolerance)
        {
            goingUp = false;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Finds the cheapest ladder connecting the NPC's current floor to
    /// <paramref name="destination"/>. Returns false if no valid route exists.
    /// </summary>
    public bool TryFindLadderRoute(
        Vector3 destination,
        out Ladder ladder,
        out bool goingUp,
        out Vector3 approachPoint,
        out Vector3 exitPoint,
        out NavMeshPath approachPath)
    {
        ladder = null;
        goingUp = false;
        approachPoint = Vector3.zero;
        exitPoint = Vector3.zero;
        approachPath = null;

        if (!useLadders || npc == null || !npc.AgentReady())
            return false;

        Vector3 rootPos = npc.transform.position;
        float deltaY = destination.y - rootPos.y;

        if (deltaY > sameLevelHeightTolerance)
            goingUp = true;
        else if (deltaY < -sameLevelHeightTolerance)
            goingUp = false;
        else
            return false;

        Ladder[] ladders = UnityEngine.Object.FindObjectsByType<Ladder>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (ladders == null || ladders.Length == 0)
            return false;

        float bestScore = float.PositiveInfinity;

        for (int i = 0; i < ladders.Length; i++)
        {
            Ladder l = ladders[i];
            if (l == null) continue;

            Transform rawApproachTf;
            Transform rawExitTf;

            if (goingUp)
            {
                if (l.bottomMountPoint == null || l.topMountPoint == null)
                {
                    if (ladderDebugLogs)
                        Debug.Log($"{name}: Reject ladder {l.name} (up) - missing bottom/top mount.");
                    continue;
                }

                rawApproachTf = l.bottomMountPoint;
                rawExitTf = l.topExitPoint != null ? l.topExitPoint : l.topMountPoint;
            }
            else
            {
                if (!l.bidirectional)
                {
                    if (ladderDebugLogs)
                        Debug.Log($"{name}: Reject ladder {l.name} (down) - not bidirectional.");
                    continue;
                }

                if (l.topMountPoint == null || l.bottomMountPoint == null)
                {
                    if (ladderDebugLogs)
                        Debug.Log($"{name}: Reject ladder {l.name} (down) - missing top/bottom mount.");
                    continue;
                }

                rawApproachTf = l.topExitPoint != null ? l.topExitPoint : l.topMountPoint;
                rawExitTf = l.bottomExitPoint != null ? l.bottomExitPoint : l.bottomMountPoint;
            }

            if (rawApproachTf == null || rawExitTf == null)
            {
                if (ladderDebugLogs)
                    Debug.Log($"{name}: Reject ladder {l.name} - null approach or exit transform.");
                continue;
            }

            if (!TryGetNavmeshPointNear(rawApproachTf.position, ladderNavmeshSnapRadius, out Vector3 snappedApproach))
            {
                if (ladderDebugLogs)
                    Debug.Log($"{name}: Reject ladder {l.name} - no navmesh near approach {rawApproachTf.position}.");
                continue;
            }

            if (!TryGetNavmeshPointNear(rawExitTf.position, ladderNavmeshSnapRadius, out Vector3 snappedExit))
            {
                if (ladderDebugLogs)
                    Debug.Log($"{name}: Reject ladder {l.name} - no navmesh near exit {rawExitTf.position}.");
                continue;
            }

            if (!npc.CanReachPosition(snappedApproach, out NavMeshPath pathToLadder))
            {
                if (ladderDebugLogs)
                    Debug.Log($"{name}: Reject ladder {l.name} - cannot reach approach {snappedApproach}.");
                continue;
            }

            if (!npc.CanReachBetween(snappedExit, destination, out NavMeshPath _, ladderNavmeshSnapRadius))
            {
                if (ladderDebugLogs)
                    Debug.Log($"{name}: Reject ladder {l.name} - cannot reach target from exit {snappedExit}.");
                continue;
            }

            float score =
                Vector3.Distance(rootPos, snappedApproach) +
                Vector3.Distance(snappedExit, destination);

            if (ladderDebugLogs)
            {
                Debug.Log(
                    $"{name}: Candidate ladder {l.name} accepted. " +
                    $"goingUp={goingUp}, approach={snappedApproach}, exit={snappedExit}, score={score:F2}");
            }

            if (score < bestScore)
            {
                bestScore = score;
                ladder = l;
                approachPoint = snappedApproach;
                exitPoint = snappedExit;
                approachPath = pathToLadder;
            }
        }

        if (ladderDebugLogs && ladder == null)
        {
            Debug.LogWarning(
                $"{name}: TryFindLadderRoute failed. destination={destination}, deltaY={deltaY:F2}, goingUp={goingUp}");
        }

        return ladder != null;
    }

    /// <summary>
    /// Begins traversal of <paramref name="ladder"/>. Invokes <paramref name="onComplete"/>
    /// once the NPC has landed and the NavMesh agent has been restored.
    /// </summary>
    public void StartLadderTraversal(Ladder ladder, bool goingUp, Action onComplete)
    {
        if (!useLadders || ladder == null || _isTraversingLadder)
        {
            onComplete?.Invoke();
            return;
        }

        _ladderCompleteAction = onComplete;
        _ladderCoroutine = StartCoroutine(LadderTraversalRoutine(ladder, goingUp));
    }

    /// <summary>
    /// Cancels any in-progress traversal immediately and resets all state.
    /// Called by NPCController.InterruptAllTransientActions().
    /// </summary>
    public void InterruptTraversal()
    {
        if (_ladderCoroutine != null)
        {
            StopCoroutine(_ladderCoroutine);
            _ladderCoroutine = null;
        }

        _isTraversingLadder   = false;
        _ladderCompleteAction = null;
    }

    // =========================================================
    // Traversal coroutine
    // =========================================================
    private IEnumerator LadderTraversalRoutine(Ladder ladder, bool goingUp)
    {
        _isTraversingLadder = true;
        _stateBeforeLadder  = npc.GetCurrentState();
        npc.SetStateDirectly(NPCController.NPCState.ClimbingLadder);

        Transform startMount = goingUp ? ladder.bottomMountPoint : ladder.topMountPoint;
        Transform endMount   = goingUp ? ladder.topMountPoint    : ladder.bottomMountPoint;
        Transform endExit    = goingUp
            ? (ladder.topExitPoint    != null ? ladder.topExitPoint    : ladder.topMountPoint)
            : (ladder.bottomExitPoint != null ? ladder.bottomExitPoint : ladder.bottomMountPoint);
        Transform hoistStart = goingUp
            ? (ladder.topHoistStartPoint != null ? ladder.topHoistStartPoint : ladder.topMountPoint)
            : null;

        if (startMount == null || endMount == null || endExit == null)
        {
            if (ladderDebugLogs) Debug.LogWarning($"{name}: Ladder aborted - missing anchors.");
            FinishLadderTraversal();
            yield break;
        }

        if (goingUp && hoistStart == null)
        {
            if (ladderDebugLogs) Debug.LogWarning($"{name}: Ladder aborted - topHoistStartPoint missing.");
            FinishLadderTraversal();
            yield break;
        }

        Vector3    ladderFacing = GetLadderFacingDirection(startMount, ladder);
        Quaternion attachRot    = Quaternion.LookRotation(ladderFacing, Vector3.up);

        if (ladderDebugLogs)
            Debug.Log($"{name}: LadderTraversal ladder={ladder.name} goingUp={goingUp} " +
                      $"facing={ladderFacing} start={startMount.position} " +
                      $"endMount={endMount.position} endExit={endExit.position}");

        // ── Phase 0: Walk to pre-ladder position ──────────────────────────────
        float xzDistToMount = Vector3.Distance(
            new Vector3(Body.position.x, 0f, Body.position.z),
            new Vector3(startMount.position.x, 0f, startMount.position.z));

        bool alreadyAtLadder = xzDistToMount <=
            (ladderApproachArriveDistance + preLadderApproachOffset + 0.25f);

        if (!alreadyAtLadder)
        {
            Vector3 preLadderPos = startMount.position +
                                   ladderFacing * Mathf.Max(0f, preLadderApproachOffset);

            if (!NavMesh.SamplePosition(preLadderPos, out NavMeshHit preHit,
                    ladderNavmeshSnapRadius, NavMesh.AllAreas))
                preHit.position = preLadderPos;

            if (AgentReady())
            {
                Agent.isStopped   = false;
                Agent.autoBraking = true;
                Agent.ResetPath();
                Agent.SetDestination(preHit.position);
            }

            ForceIdlePose();

            while (true)
            {
                if (AgentReady() && !Agent.pathPending &&
                    (!Agent.hasPath ||
                     Vector3.Distance(Agent.destination, preHit.position) > 0.15f))
                    Agent.SetDestination(preHit.position);

                float xzDist = Vector3.Distance(
                    new Vector3(Body.position.x, 0f, Body.position.z),
                    new Vector3(preHit.position.x, 0f, preHit.position.z));

                if (xzDist <= Mathf.Max(ladderApproachArriveDistance, 0.2f)) break;

                yield return null;
            }
        }

        // ── Align to face the ladder ───────────────────────────────────────────
        if (AgentReady())
        {
            Agent.isStopped = true;
            Agent.ResetPath();
        }

        ForceIdlePose();
        ResetAllAnimatorTriggers();

        while (true)
        {
            Body.rotation = Quaternion.Slerp(Body.rotation, attachRot,
                Time.deltaTime * ladderFacingSlerp * 1.5f);

            if (Mathf.Abs(Mathf.DeltaAngle(Body.eulerAngles.y, attachRot.eulerAngles.y))
                <= Mathf.Max(0.5f, ladderAlignToleranceDeg))
                break;

            yield return null;
        }

        // ── Snap to mount and fire climb trigger ───────────────────────────────
        Body.position = startMount.position;
        Body.rotation = attachRot;

        DetachAgentForAnimation();

        string climbTrigger = goingUp ? climbUpTriggerName : climbDownTriggerName;
        if (Anim != null && !string.IsNullOrWhiteSpace(climbTrigger))
        {
            ResetAllAnimatorTriggers();
            Anim.SetTrigger(climbTrigger);
        }

        // Descent only: hold mount position until the animation state actually enters
        if (!goingUp && Anim != null && !string.IsNullOrWhiteSpace(climbTrigger))
        {
            float lockTimer = 0f;
            float lockMax   = Mathf.Max(0.02f, npc.triggerEnterTimeout * 0.5f);
            int   baseHash  = Anim.GetCurrentAnimatorStateInfo(0).fullPathHash;

            while (lockTimer < lockMax)
            {
                Body.position = startMount.position;
                Body.rotation = attachRot;

                AnimatorStateInfo info = Anim.GetCurrentAnimatorStateInfo(0);
                if (Anim.IsInTransition(0) || info.fullPathHash != baseHash)
                    break;

                lockTimer += Time.deltaTime;
                yield return null;
            }

            Body.position = startMount.position;
            Body.rotation = attachRot;
            yield return null;
        }

        // ── Phase 1: Climb ────────────────────────────────────────────────────
        // Up:   startMount → hoistStart  (HoistUp triggered early by proximity)
        // Down: topMount   → bottomMount
        Vector3 climbFrom    = startMount.position;
        Vector3 climbTarget  = goingUp ? hoistStart.position : endMount.position;
        float   climbDist    = Vector3.Distance(climbFrom, climbTarget);
        float   climbDuration = Mathf.Max(0.1f, climbDist / Mathf.Max(0.01f, ladderClimbSpeed));
        bool    hoistTriggered = !goingUp; // descent has no hoist phase

        for (float t = 0f; t < climbDuration; t += Time.deltaTime)
        {
            Body.position = Vector3.Lerp(climbFrom, climbTarget, Mathf.Clamp01(t / climbDuration));
            Body.rotation = Quaternion.Slerp(Body.rotation, attachRot,
                Time.deltaTime * ladderFacingSlerp);

            if (!hoistTriggered)
            {
                float distRemaining = Vector3.Distance(Body.position, hoistStart.position);
                if (distRemaining <= Mathf.Max(0.05f, ladderTopStopOffset))
                {
                    hoistTriggered = true;
                    if (Anim != null && !string.IsNullOrWhiteSpace(hoistUpTriggerName))
                    {
                        ResetAllAnimatorTriggers();
                        Anim.SetTrigger(hoistUpTriggerName);
                        if (ladderDebugLogs)
                            Debug.Log(
                                $"{name}: HoistUp trigger fired at dist={distRemaining:F2} from hoistStart");
                    }
                }
            }

            yield return null;
        }

        Body.position = climbTarget;
        Body.rotation = attachRot;

        // ── Phase 2: Hoist (ascent only) ──────────────────────────────────────
        if (goingUp)
        {
            // Fallback: fire if the proximity-based trigger never tripped (short ladder)
            if (!hoistTriggered && Anim != null && !string.IsNullOrWhiteSpace(hoistUpTriggerName))
            {
                ResetAllAnimatorTriggers();
                Anim.SetTrigger(hoistUpTriggerName);
            }

            Vector3 hoistFrom   = Body.position;
            Vector3 hoistTarget = endExit.position;

            float hoistDist           = Vector3.Distance(hoistFrom, hoistTarget);
            float actualHoistDuration = hoistDist > 0.001f
                ? Mathf.Max(hoistDuration, hoistDist / Mathf.Max(0.01f, ladderClimbSpeed * 0.6f))
                : hoistDuration;

            for (float ht = 0f; ht < actualHoistDuration; ht += Time.deltaTime)
            {
                Body.position = Vector3.Lerp(hoistFrom, hoistTarget,
                    Mathf.Clamp01(ht / actualHoistDuration));
                Body.rotation = Quaternion.Slerp(Body.rotation, attachRot,
                    Time.deltaTime * ladderFacingSlerp);
                yield return null;
            }

            Body.position = hoistTarget;
            Body.rotation = attachRot;
        }
        else
        {
            Body.position = endExit.position;
            Body.rotation = attachRot;
        }

        // ── Land: snap body back to NavMesh ────────────────────────────────────
        yield return null;

        ForceUpright();

        if (!RestoreStandingBodyAt(endExit.position, ladderNavmeshSnapRadius))
        {
            if (Agent != null &&
                TryGetNavmeshPointNear(endExit.position, ladderNavmeshSnapRadius * 2f,
                    out Vector3 fallback))
            {
                Body.position = fallback;
                ForceUpright();
                if (!Agent.enabled) Agent.enabled = true;
                Agent.Warp(fallback);
                Agent.isStopped = false;
                Agent.ResetPath();
            }

            if (ladderDebugLogs)
                Debug.LogWarning(
                    $"{name}: Could not snap to NavMesh at ladder exit {endExit.position}. " +
                    $"Check NavMesh bake or increase ladderNavmeshSnapRadius ({ladderNavmeshSnapRadius}).");
        }

        // ── Kill animation state before releasing FSM (prevents ghost-climb) ──
        if (AgentReady())
        {
            Agent.isStopped = true;
            Agent.ResetPath();
        }

        if (Anim != null)
        {
            ResetAllAnimatorTriggers();
            Anim.SetFloat(ParamMovingX, 0f);
            Anim.SetFloat(ParamMovingY, 0f);
            Anim.SetFloat(ParamBlend,   0f);
            if (!string.IsNullOrEmpty(npc.LocomotionStateName))
                Anim.Play(npc.LocomotionStateName, npc.LocomotionLayer, 0f);
        }

        yield return null;

        if (AgentReady())
            Agent.isStopped = false;

        FinishLadderTraversal();
    }

    private void FinishLadderTraversal()
    {
        _isTraversingLadder = false;
        _ladderCoroutine    = null;
        npc.SetStateDirectly(_stateBeforeLadder);

        Action done = _ladderCompleteAction;
        _ladderCompleteAction = null;
        done?.Invoke();
    }

    // =========================================================
    // Facing direction helper
    // =========================================================
    private Vector3 GetLadderFacingDirection(Transform mountPoint, Ladder ladder)
    {
        if (ladder != null && ladder.ladderMeshTransform != null)
        {
            Vector3 toLadder = ladder.ladderMeshTransform.position - mountPoint.position;
            toLadder.y = 0f;
            if (toLadder.sqrMagnitude > 0.0001f)
                return toLadder.normalized;
        }

        if (mountPoint != null)
        {
            Vector3 fwd = mountPoint.up;
            fwd.y = 0f;
            if (fwd.sqrMagnitude > 0.0001f) return fwd.normalized;

            fwd = mountPoint.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude > 0.0001f) return fwd.normalized;
        }

        if (ladder != null)
        {
            Vector3 fwd = ladder.FacingForward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude > 0.0001f) return fwd.normalized;
        }

        return Body.forward;
    }
}
