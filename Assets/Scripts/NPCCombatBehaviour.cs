// NPCCombatBehaviour.cs
// Handles the Attacking FSM state.
// Attach alongside NPCController.

using UnityEngine;

public class NPCCombatBehaviour : NPCBehaviourBase
{
    // =========================================================
    // Tick (called by NPCController while in Attacking state)
    // =========================================================
    public void TickAttacking(float dt)
    {
        // Button-command attack: hold idle, face target
        if (npc.HasCommand && npc.CommandGoal == NPCController.NPCState.Attacking)
        {
            if (AgentReady())
            {
                Agent.isStopped = true;
                Agent.ResetPath();
            }

            ForceIdlePose();

            if (npc.currentTarget != null)
                FaceTowards(npc.currentTarget.position, dt, TurnSmoothing * 1.25f);

            return;
        }

        // Natural AI attack
        if (npc.currentTarget == null)
        {
            EnterState(NPCController.NPCState.Seeking);
            return;
        }

        float distToSpawn = Vector3.Distance(SpawnPoint, npc.currentTarget.position);
        if (distToSpawn > ActiveRadius && !npc.allowApproachOutsideActiveRadius)
        {
            EnterState(NPCController.NPCState.Seeking);
            return;
        }

        float distToTarget = Vector3.Distance(Body.position, npc.currentTarget.position);
        if (distToTarget > npc.attackRange * 1.15f)
        {
            EnterState(NPCController.NPCState.Approaching);
            return;
        }

        FaceTowards(npc.currentTarget.position, dt, TurnSmoothing * 1.25f);
    }

    // =========================================================
    // Private helpers
    // =========================================================
    private void FaceTowards(Vector3 worldPos, float dt, float speed)
    {
        Vector3 to = worldPos - Body.position; to.y = 0f;
        if (to.sqrMagnitude < 0.0001f) return;
        Quaternion targetRot = Quaternion.LookRotation(to.normalized, Vector3.up);
        Body.rotation = Quaternion.Slerp(Body.rotation, targetRot, dt * speed);
    }
}
