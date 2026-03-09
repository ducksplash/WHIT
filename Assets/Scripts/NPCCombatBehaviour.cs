using UnityEngine;

public class NPCCombatBehaviour : NPCBehaviourBase
{
    public void TickAttacking(float dt)
    {
        if (npc.HasCommand && npc.CommandGoal == NPCController.NPCState.Attacking)
        {
            if (AgentReady()) { Agent.isStopped = true; Agent.ResetPath(); }
            ForceIdlePose();
            if (npc.currentTarget != null) FaceTowards(npc.currentTarget.position, dt, TurnSmoothing * 1.25f);
            return;
        }
        if (npc.currentTarget == null) { EnterState(NPCController.NPCState.Seeking); return; }
        if (Vector3.Distance(SpawnPoint, npc.currentTarget.position) > ActiveRadius && !npc.allowApproachOutsideActiveRadius)
        { EnterState(NPCController.NPCState.Seeking); return; }
        if (Vector3.Distance(Body.position, npc.currentTarget.position) > npc.attackRange * 1.15f)
        { EnterState(NPCController.NPCState.Approaching); return; }
        FaceTowards(npc.currentTarget.position, dt, TurnSmoothing * 1.25f);
    }

    private void FaceTowards(Vector3 worldPos, float dt, float speed)
    {
        Vector3 to = worldPos - Body.position; to.y = 0f;
        if (to.sqrMagnitude < 0.0001f) return;
        Body.rotation = Quaternion.Slerp(Body.rotation, Quaternion.LookRotation(to.normalized, Vector3.up), dt * speed);
    }
}