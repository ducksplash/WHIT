// NPCTalkBehaviour.cs
// Handles the Talk FSM state and conversation-target locking.
// Attach alongside NPCController.

using System.Collections.Generic;
using UnityEngine;

public class NPCTalkBehaviour : NPCBehaviourBase
{
    // =========================================================
    // Runtime
    // =========================================================

    // Set by NPCController when it resolves the talk target
    public NPCController TalkTargetController { get; set; }
    public bool RegisteredAsSpeaker           { get; set; }

    private readonly HashSet<NPCController> _conversationSpeakers = new HashSet<NPCController>();
    private NPCController _primaryConversationSpeaker;

    public bool IsConversationLocked => _isConversationLocked;
    private bool _isConversationLocked = false;

    // =========================================================
    // Tick (called by NPCController while in Talk state)
    // =========================================================
    public void TickTalk(float dt)
    {
        if (AgentReady()) Agent.isStopped = true;
        ForceIdlePose();

        if (npc.currentTarget == null)
        {
            UnregisterAsSpeaker();
            npc.HasCommand  = false;
            npc.CommandGoal = NPCController.NPCState.Patrolling;
            EnterState(NPCController.NPCState.Patrolling);
            return;
        }

        FaceTowards(npc.currentTarget.position, dt, TurnSmoothing * 1.5f);

        if (TalkTargetController == null)
            TalkTargetController = npc.currentTarget.GetComponentInParent<NPCController>();

        if (TalkTargetController != null && !RegisteredAsSpeaker)
        {
            TalkTargetController.Talk.BeginConversationAsTarget(npc);
            RegisteredAsSpeaker = true;
        }
    }

    // =========================================================
    // Conversation lock (target side)
    // =========================================================
    public void BeginConversationAsTarget(NPCController speaker)
    {
        if (speaker == null) return;

        bool wasEmpty = _conversationSpeakers.Count == 0;
        _conversationSpeakers.Add(speaker);

        if (wasEmpty)
        {
            npc.useStateMachine = false;

            if (AgentReady())
            {
                Agent.isStopped = true;
                Agent.ResetPath();
            }

            ForceIdlePose();
        }

        _isConversationLocked      = _conversationSpeakers.Count > 0;
        _primaryConversationSpeaker = GetNearestSpeaker();
    }

    public void EndConversationAsTarget(NPCController speaker)
    {
        if (speaker != null)
            _conversationSpeakers.Remove(speaker);

        _primaryConversationSpeaker = GetNearestSpeaker();
        _isConversationLocked       = _conversationSpeakers.Count > 0;

        if (!_isConversationLocked)
        {
            npc.useStateMachine = true;

            if (AgentReady())
            {
                Agent.isStopped = false;
                Agent.ResetPath();
            }

            ForceReturnToLocomotion();

            if (npc.GetCurrentState() == NPCController.NPCState.Talk)
                EnterState(NPCController.NPCState.Patrolling);
        }
    }

    // =========================================================
    // Conversation update (called from NPCController.Update when locked)
    // =========================================================
    public void UpdateConversationLocked(float dt)
    {
        if (AgentReady()) Agent.isStopped = true;
        ForceIdlePose();

        _primaryConversationSpeaker = GetNearestSpeaker();
        if (_primaryConversationSpeaker != null)
            FaceTowards(_primaryConversationSpeaker.transform.position, dt, TurnSmoothing * 1.5f);
    }

    // =========================================================
    // Unregister helper
    // =========================================================
    public void UnregisterAsSpeaker()
    {
        if (TalkTargetController != null && RegisteredAsSpeaker)
            TalkTargetController.Talk.EndConversationAsTarget(npc);

        RegisteredAsSpeaker   = false;
        TalkTargetController  = null;
    }

    public void BreakConversationLockImmediate()
    {
        UnregisterAsSpeaker();

        foreach (var speaker in _conversationSpeakers)
        {
            if (speaker == null) continue;
            speaker.Talk.RegisteredAsSpeaker  = false;
            speaker.Talk.TalkTargetController = null;

            if (speaker.GetCurrentState() == NPCController.NPCState.Talk)
            {
                speaker.HasCommand  = false;
                speaker.CommandGoal = NPCController.NPCState.Patrolling;
                speaker.currentTarget = null;
                speaker.useStateMachine = true;

                if (speaker.AgentReady())
                {
                    speaker.agent.isStopped = false;
                    speaker.agent.ResetPath();
                }

                if (speaker.animationController != null)
                    speaker.ForceReturnToLocomotion();

                speaker.EnterState(NPCController.NPCState.Patrolling);
            }
        }

        _conversationSpeakers.Clear();
        _primaryConversationSpeaker = null;
        _isConversationLocked       = false;
        npc.useStateMachine         = true;

        if (AgentReady())
        {
            Agent.isStopped = false;
            Agent.ResetPath();
        }

        if (Anim != null) ForceReturnToLocomotion();
    }

    public void ClearAllSpeakers()
    {
        _conversationSpeakers.Clear();
        _primaryConversationSpeaker = null;
        _isConversationLocked       = false;
    }

    // =========================================================
    // Private helpers
    // =========================================================
    private NPCController GetNearestSpeaker()
    {
        NPCController best   = null;
        float         bestSqr = float.PositiveInfinity;

        foreach (var s in _conversationSpeakers)
        {
            if (s == null) continue;
            float sqr = (s.transform.position - Body.position).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; best = s; }
        }
        return best;
    }

    private void FaceTowards(Vector3 worldPos, float dt, float speed)
    {
        Vector3 to = worldPos - Body.position; to.y = 0f;
        if (to.sqrMagnitude < 0.0001f) return;
        Quaternion targetRot = Quaternion.LookRotation(to.normalized, Vector3.up);
        Body.rotation = Quaternion.Slerp(Body.rotation, targetRot, dt * speed);
    }
}
