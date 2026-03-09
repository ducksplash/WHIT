using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCLadderBehaviour : NPCBehaviourBase
{
    [Header("Ladders")]
    public bool   useLadders                   = true;
    public float  ladderApproachArriveDistance  = 0.45f;
    public float  ladderClimbSpeed              = 1.6f;
    public float  ladderFacingSlerp             = 18f;
    public string climbUpTriggerName            = "ClimbUpLadder";
    public string climbDownTriggerName          = "ClimbDownLadder";
    public float  ladderNavmeshSnapRadius       = 3f;
    public bool   ladderDebugLogs               = true;
    public string hoistUpTriggerName            = "HoistUp";
    public float  ladderTopStopOffset           = 0.9f;
    public float  hoistDuration                 = 0.7f;
    [Tooltip("How far in front of the mount point the NPC stops before aligning and climbing.")]
    public float  preLadderApproachOffset       = 1.0f;
    [Tooltip("Yaw tolerance in degrees before the NPC is considered aligned with the ladder and snaps on.")]
    public float  ladderAlignToleranceDeg       = 8f;
    [Tooltip("If the target is within this vertical distance, treat it as the same level and do not use a ladder.")]
    public float  sameLevelHeightTolerance      = 0.5f;

    [Header("Climb Down Entry")]
    public string beginClimbDownTriggerName     = "BeginClimbDown";
    public string beginClimbDownStateName       = "BeginClimbDown";
    public float  beginClimbDownDuration        = 0.65f;

    [Header("Climb Down Loop")]
    public string climbDownStateName            = "ClimbDownLadder";
    public float  climbDownTriggerFallbackDelay = 0.08f;

    [Header("Climb Down Path")]
    [Range(0.1f, 0.9f)]
    public float climbDownHorizontalPortion     = 0.55f;

    public bool IsTraversingLadder => _isTraversingLadder;

    private Coroutine              _ladderCoroutine;
    private bool                   _isTraversingLadder;
    private NPCController.NPCState _stateBeforeLadder = NPCController.NPCState.Patrolling;
    private Action                 _ladderCompleteAction;

    public bool NeedLadderForTarget(Transform target, out bool goingUp)
    {
        goingUp = false;
        if (!useLadders || target == null) return false;
        float dy = target.position.y - Body.position.y;
        if (dy >  sameLevelHeightTolerance) { goingUp = true;  return true; }
        if (dy < -sameLevelHeightTolerance) { goingUp = false; return true; }
        return false;
    }

    public bool TryFindLadderRoute(Vector3 destination, out Ladder ladder, out bool goingUp,
        out Vector3 approachPoint, out Vector3 exitPoint, out NavMeshPath approachPath)
    {
        ladder = null; goingUp = false;
        approachPoint = exitPoint = Vector3.zero; approachPath = null;

        if (!useLadders || !AgentReady()) return false;

        float deltaY = destination.y - Body.position.y;
        if      (deltaY >  sameLevelHeightTolerance) goingUp = true;
        else if (deltaY < -sameLevelHeightTolerance) goingUp = false;
        else    return false;

        Ladder[] ladders = UnityEngine.Object.FindObjectsByType<Ladder>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (ladders == null || ladders.Length == 0) return false;

        float bestScore = float.PositiveInfinity;
        for (int i = 0; i < ladders.Length; i++)
        {
            Ladder l = ladders[i];
            if (l == null) continue;

            Transform rawApproach, rawExit;
            if (goingUp)
            {
                if (l.bottomMountPoint == null || l.topMountPoint == null)
                    { if (ladderDebugLogs) Debug.Log($"{name}: Reject {l.name} (up) missing mounts."); continue; }
                rawApproach = l.bottomMountPoint;
                rawExit     = l.topExitPoint != null ? l.topExitPoint : l.topMountPoint;
            }
            else
            {
                if (!l.bidirectional)
                    { if (ladderDebugLogs) Debug.Log($"{name}: Reject {l.name} (down) not bidirectional."); continue; }
                if (l.topMountPoint == null || l.bottomMountPoint == null)
                    { if (ladderDebugLogs) Debug.Log($"{name}: Reject {l.name} (down) missing mounts."); continue; }
                rawApproach = l.topExitPoint    != null ? l.topExitPoint    : l.topMountPoint;
                rawExit     = l.bottomExitPoint != null ? l.bottomExitPoint : l.bottomMountPoint;
            }

            if (rawApproach == null || rawExit == null)
                { if (ladderDebugLogs) Debug.Log($"{name}: Reject {l.name} null approach/exit."); continue; }

            if (!TryGetNavmeshPointNear(rawApproach.position, ladderNavmeshSnapRadius, out Vector3 snappedApproach))
                { if (ladderDebugLogs) Debug.Log($"{name}: Reject {l.name} no navmesh near approach."); continue; }

            if (!TryGetNavmeshPointNear(rawExit.position, ladderNavmeshSnapRadius, out Vector3 snappedExit))
                { if (ladderDebugLogs) Debug.Log($"{name}: Reject {l.name} no navmesh near exit."); continue; }

            if (!npc.CanReachPosition(snappedApproach, out NavMeshPath pathToLadder))
                { if (ladderDebugLogs) Debug.Log($"{name}: Reject {l.name} cannot reach approach."); continue; }

            if (!npc.CanReachBetween(snappedExit, destination, out NavMeshPath _, ladderNavmeshSnapRadius))
                { if (ladderDebugLogs) Debug.Log($"{name}: Reject {l.name} cannot reach target from exit."); continue; }

            float score = Vector3.Distance(Body.position, snappedApproach) + Vector3.Distance(snappedExit, destination);
            if (ladderDebugLogs) Debug.Log($"{name}: Candidate {l.name} accepted. goingUp={goingUp} score={score:F2}");

            if (score < bestScore)
            {
                bestScore = score; ladder = l;
                approachPoint = snappedApproach; exitPoint = snappedExit; approachPath = pathToLadder;
            }
        }

        if (ladderDebugLogs && ladder == null)
            Debug.LogWarning($"{name}: TryFindLadderRoute failed. dest={destination} deltaY={deltaY:F2} goingUp={goingUp}");
        return ladder != null;
    }

    public void StartLadderTraversal(Ladder ladder, bool goingUp, Action onComplete, Vector3 alreadyAtApproach = default)
    {
        if (!useLadders || ladder == null || _isTraversingLadder) { onComplete?.Invoke(); return; }
        _ladderCompleteAction = onComplete;
        _ladderCoroutine = StartCoroutine(LadderTraversalRoutine(ladder, goingUp, alreadyAtApproach));
    }

    public void InterruptTraversal()    => ForceResetLadderState();
    public void ForceResetLadderState()
    {
        if (_ladderCoroutine != null) { StopCoroutine(_ladderCoroutine); _ladderCoroutine = null; }
        _isTraversingLadder = false;
        _ladderCompleteAction = null;
    }

    private IEnumerator LadderTraversalRoutine(Ladder ladder, bool goingUp, Vector3 alreadyAtApproach = default)
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
            { if (ladderDebugLogs) Debug.LogWarning($"{name}: Ladder aborted - missing anchors."); FinishLadderTraversal(); yield break; }
        if (goingUp && hoistStart == null)
            { if (ladderDebugLogs) Debug.LogWarning($"{name}: Ladder aborted - topHoistStartPoint missing."); FinishLadderTraversal(); yield break; }

        Vector3    ladderFacing = GetLadderFacingDirection(startMount, ladder);
        Quaternion attachRot    = Quaternion.LookRotation(ladderFacing, Vector3.up);

        if (ladderDebugLogs)
            Debug.Log($"{name}: LadderTraversal {ladder.name} goingUp={goingUp} facing={ladderFacing}");

        bool alreadyAtLadder;
        if (alreadyAtApproach != default)
        {
            float d = Vector3.Distance(new Vector3(Body.position.x, 0f, Body.position.z),
                                       new Vector3(alreadyAtApproach.x, 0f, alreadyAtApproach.z));
            alreadyAtLadder = d <= ladderApproachArriveDistance + 0.1f;
        }
        else
        {
            float d = Vector3.Distance(new Vector3(Body.position.x, 0f, Body.position.z),
                                       new Vector3(startMount.position.x, 0f, startMount.position.z));
            alreadyAtLadder = d <= ladderApproachArriveDistance + preLadderApproachOffset + 0.25f;
        }

        if (!alreadyAtLadder)
        {
            Vector3 preLadderPos = startMount.position + ladderFacing * Mathf.Max(0f, preLadderApproachOffset);
            if (!NavMesh.SamplePosition(preLadderPos, out NavMeshHit preHit, ladderNavmeshSnapRadius, NavMesh.AllAreas))
                preHit.position = preLadderPos;

            if (AgentReady()) { Agent.isStopped = false; Agent.autoBraking = true; Agent.ResetPath(); Agent.SetDestination(preHit.position); }
            ForceIdlePose();

            while (true)
            {
                if (AgentReady() && !Agent.pathPending && (!Agent.hasPath || Vector3.Distance(Agent.destination, preHit.position) > 0.15f))
                    Agent.SetDestination(preHit.position);
                float xzDist = Vector3.Distance(new Vector3(Body.position.x, 0f, Body.position.z),
                                                new Vector3(preHit.position.x, 0f, preHit.position.z));
                if (xzDist <= Mathf.Max(ladderApproachArriveDistance, 0.2f)) break;
                yield return null;
            }
        }

        if (AgentReady()) { Agent.isStopped = true; Agent.ResetPath(); }
        ForceIdlePose();
        ResetAllAnimatorTriggers();

        while (true)
        {
            Body.rotation = Quaternion.Slerp(Body.rotation, attachRot, Time.deltaTime * ladderFacingSlerp * 1.5f);
            if (Mathf.Abs(Mathf.DeltaAngle(Body.eulerAngles.y, attachRot.eulerAngles.y)) <= Mathf.Max(0.5f, ladderAlignToleranceDeg)) break;
            yield return null;
        }
        if (AgentReady()) { Agent.isStopped = true; Agent.ResetPath(); }

        if (goingUp)
        {
            Body.position = startMount.position;
            Body.rotation = attachRot;
            DetachAgentForAnimation();

            if (Anim != null && !string.IsNullOrWhiteSpace(climbUpTriggerName))
                { ResetAllAnimatorTriggers(); Anim.SetTrigger(climbUpTriggerName); }

            Vector3 climbFrom = startMount.position, climbTarget = hoistStart.position;
            float   climbDur  = Mathf.Max(0.1f, Vector3.Distance(climbFrom, climbTarget) / Mathf.Max(0.01f, ladderClimbSpeed));
            bool    hoistTriggered = false;

            for (float t = 0f; t < climbDur; t += Time.deltaTime)
            {
                Body.position = Vector3.Lerp(climbFrom, climbTarget, Mathf.Clamp01(t / climbDur));
                Body.rotation = Quaternion.Slerp(Body.rotation, attachRot, Time.deltaTime * ladderFacingSlerp);
                if (!hoistTriggered && Vector3.Distance(Body.position, hoistStart.position) <= Mathf.Max(0.05f, ladderTopStopOffset))
                {
                    hoistTriggered = true;
                    if (Anim != null && !string.IsNullOrWhiteSpace(hoistUpTriggerName))
                    {
                        ResetAllAnimatorTriggers(); Anim.SetTrigger(hoistUpTriggerName);
                        if (ladderDebugLogs) Debug.Log($"{name}: HoistUp fired.");
                    }
                }
                yield return null;
            }
            Body.position = climbTarget; Body.rotation = attachRot;

            if (!hoistTriggered && Anim != null && !string.IsNullOrWhiteSpace(hoistUpTriggerName))
                { ResetAllAnimatorTriggers(); Anim.SetTrigger(hoistUpTriggerName); }

            Vector3 hoistFrom = Body.position, hoistTarget = endExit.position;
            float   hoistDist = Vector3.Distance(hoistFrom, hoistTarget);
            float   actualHoistDur = hoistDist > 0.001f
                ? Mathf.Max(hoistDuration, hoistDist / Mathf.Max(0.01f, ladderClimbSpeed * 0.6f))
                : hoistDuration;

            for (float ht = 0f; ht < actualHoistDur; ht += Time.deltaTime)
            {
                Body.position = Vector3.Lerp(hoistFrom, hoistTarget, Mathf.Clamp01(ht / actualHoistDur));
                Body.rotation = Quaternion.Slerp(Body.rotation, attachRot, Time.deltaTime * ladderFacingSlerp);
                yield return null;
            }
            Body.position = hoistTarget; Body.rotation = attachRot;
        }
        else
        {
            Vector3    standPos = Body.position;
            Quaternion standRot = Body.rotation;
            Transform  hoistStartTf = ladder.topHoistStartPoint != null ? ladder.topHoistStartPoint : ladder.topMountPoint;
            Vector3    mountPos = hoistStartTf.position;
            Vector3    horizontalMid = new Vector3(mountPos.x, standPos.y, mountPos.z);

            DetachAgentForAnimation();

            int   animLayer = 0;
            bool  beginTriggered = false, beginDone = false;
            float beginFallbackT = 0f;
            int   beginStartHash = 0;
            float entryDuration  = Mathf.Max(0.01f, beginClimbDownDuration);
            float hPortion       = Mathf.Clamp(climbDownHorizontalPortion, 0.1f, 0.9f);

            for (float t = 0f; t < entryDuration; t += Time.deltaTime)
            {
                float u = Mathf.Clamp01(t / entryDuration);
                Body.position = u < hPortion
                    ? Vector3.Lerp(standPos, horizontalMid, u / hPortion)
                    : Vector3.Lerp(horizontalMid, mountPos, (u - hPortion) / Mathf.Max(0.0001f, 1f - hPortion));
                Body.rotation = Quaternion.Slerp(standRot, attachRot, u);

                if (!beginTriggered && u >= hPortion)
                {
                    beginTriggered = true;
                    beginStartHash = Anim != null ? Anim.GetCurrentAnimatorStateInfo(animLayer).fullPathHash : 0;
                    if (Anim != null && !string.IsNullOrWhiteSpace(beginClimbDownTriggerName))
                        { ResetAllAnimatorTriggers(); Anim.SetTrigger(beginClimbDownTriggerName); }
                }

                if (Anim != null && beginTriggered && !beginDone)
                {
                    beginFallbackT += Time.deltaTime;
                    AnimatorStateInfo info = Anim.GetCurrentAnimatorStateInfo(animLayer);
                    bool entered = Anim.IsInTransition(animLayer)
                        || (!string.IsNullOrWhiteSpace(beginClimbDownStateName) && info.IsName(beginClimbDownStateName))
                        || info.fullPathHash != beginStartHash;
                    if (entered) { beginDone = true; }
                    else if (beginFallbackT >= Mathf.Max(0.01f, climbDownTriggerFallbackDelay))
                    {
                        if (!string.IsNullOrWhiteSpace(beginClimbDownStateName))
                            { if (ladderDebugLogs) Debug.Log($"{name}: Descent fallback Play('{beginClimbDownStateName}')"); Anim.Play(beginClimbDownStateName, animLayer, 0f); Anim.Update(0f); }
                        beginDone = true;
                    }
                }
                yield return null;
            }
            Body.position = mountPos; Body.rotation = attachRot;
            yield return null;

            bool  climbDone = false;
            float climbFallbackT = 0f;
            int   climbStartHash = Anim != null ? Anim.GetCurrentAnimatorStateInfo(animLayer).fullPathHash : 0;

            if (Anim != null && !string.IsNullOrWhiteSpace(climbDownTriggerName))
                { ResetAllAnimatorTriggers(); Anim.SetTrigger(climbDownTriggerName); }

            float holdMax = Mathf.Max(0.05f, npc.triggerEnterTimeout * 0.5f);
            for (float holdT = 0f; holdT < holdMax; holdT += Time.deltaTime)
            {
                Body.position = mountPos; Body.rotation = attachRot;
                if (Anim != null && !climbDone)
                {
                    climbFallbackT += Time.deltaTime;
                    AnimatorStateInfo info = Anim.GetCurrentAnimatorStateInfo(animLayer);
                    bool entered = Anim.IsInTransition(animLayer)
                        || (!string.IsNullOrWhiteSpace(climbDownStateName) && info.IsName(climbDownStateName))
                        || info.fullPathHash != climbStartHash;
                    if (entered) { climbDone = true; }
                    else if (climbFallbackT >= Mathf.Max(0.01f, climbDownTriggerFallbackDelay))
                    {
                        if (!string.IsNullOrWhiteSpace(climbDownStateName))
                            { if (ladderDebugLogs) Debug.Log($"{name}: Descent fallback Play('{climbDownStateName}')"); Anim.Play(climbDownStateName, animLayer, 0f); Anim.Update(0f); }
                        climbDone = true;
                    }
                }
                yield return null;
            }

            Vector3 cFrom = mountPos, cTarget = endMount.position;
            float   cDur  = Mathf.Max(0.1f, Vector3.Distance(cFrom, cTarget) / Mathf.Max(0.01f, ladderClimbSpeed));
            for (float t = 0f; t < cDur; t += Time.deltaTime)
            {
                Body.position = Vector3.Lerp(cFrom, cTarget, Mathf.Clamp01(t / cDur));
                Body.rotation = Quaternion.Slerp(Body.rotation, attachRot, Time.deltaTime * ladderFacingSlerp);
                yield return null;
            }
            Body.position = endExit.position; Body.rotation = attachRot;
        }

        yield return null;
        ForceUpright();

        if (!RestoreStandingBodyAt(endExit.position, ladderNavmeshSnapRadius))
        {
            if (Agent != null && TryGetNavmeshPointNear(endExit.position, ladderNavmeshSnapRadius * 2f, out Vector3 fallback))
            {
                Body.position = fallback; ForceUpright();
                if (!Agent.enabled) Agent.enabled = true;
                Agent.Warp(fallback); Agent.isStopped = false; Agent.ResetPath();
            }
            if (ladderDebugLogs)
                Debug.LogWarning($"{name}: Could not snap to NavMesh at ladder exit {endExit.position}.");
        }

        if (AgentReady()) { Agent.isStopped = true; Agent.ResetPath(); }

        if (Anim != null)
        {
            ResetAllAnimatorTriggers();
            Anim.SetFloat(ParamMovingX, 0f); Anim.SetFloat(ParamMovingY, 0f); Anim.SetFloat(ParamBlend, 0f);
            if (!string.IsNullOrEmpty(npc.LocomotionStateName))
                Anim.Play(npc.LocomotionStateName, npc.LocomotionLayer, 0f);
        }

        yield return null;
        if (AgentReady()) Agent.isStopped = false;
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

    private Vector3 GetLadderFacingDirection(Transform mountPoint, Ladder ladder)
    {
        if (ladder?.ladderMeshTransform != null)
        {
            Vector3 toLadder = ladder.ladderMeshTransform.position - mountPoint.position; toLadder.y = 0f;
            if (toLadder.sqrMagnitude > 0.0001f) return toLadder.normalized;
        }
        if (mountPoint != null)
        {
            Vector3 fwd = mountPoint.up; fwd.y = 0f;
            if (fwd.sqrMagnitude > 0.0001f) return fwd.normalized;
            fwd = mountPoint.forward; fwd.y = 0f;
            if (fwd.sqrMagnitude > 0.0001f) return fwd.normalized;
        }
        if (ladder != null)
        {
            Vector3 fwd = ladder.FacingForward; fwd.y = 0f;
            if (fwd.sqrMagnitude > 0.0001f) return fwd.normalized;
        }
        return Body.forward;
    }
}