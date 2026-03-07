// NPCBehaviourBase.cs
// Abstract base class for NPC behaviour plugins (Sitting, Lying, Talk, Combat).
// Gives each plugin access to shared controller services without circular field duplication.

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public abstract class NPCBehaviourBase : MonoBehaviour
{
    // Set by NPCController.Start() before any behaviour ticks run.
    protected NPCController npc { get; private set; }

    public virtual void Init(NPCController controller)
    {
        npc = controller;
    }

    // ── Shared shorthand properties ─────────────────────────────────────────
    protected Animator        Anim  => npc.animationController;
    protected NavMeshAgent    Agent => npc.agent;
    protected Transform       Body  => npc.transform;

    // ── Shared helpers forwarded from NPCController ──────────────────────────
    protected bool   AgentReady()                                          => npc.AgentReady();
    protected bool   TryGetNavmeshPoint(Vector3 near, out Vector3 navPos)  => npc.TryGetNavmeshPoint(near, out navPos);
    protected bool   TryGetNavmeshPointNear(Vector3 near, float r, out Vector3 navPos) => npc.TryGetNavmeshPointNear(near, r, out navPos);
    protected void   ForceUpright()                                        => npc.ForceUpright();
    protected bool   GroundTransformToNavmesh(Vector3 preferred, float r)  => npc.GroundTransformToNavmesh(preferred, r);
    protected bool   RestoreStandingBodyAt(Vector3 preferred, float r)     => npc.RestoreStandingBodyAt(preferred, r);
    protected void   DetachAgentForAnimation()                             => npc.DetachAgentForAnimation();
    protected void   ReattachAgentToNavmeshAtCurrentXZ()                   => npc.ReattachAgentToNavmeshAtCurrentXZ();
    protected void   ForceReturnToLocomotion()                             => npc.ForceReturnToLocomotion();
    protected void   ForceIdlePose()                                       => npc.ForceIdlePose();
    protected void   ResetAllAnimatorTriggers()                            => npc.ResetAllAnimatorTriggers();
    protected void   EnterState(NPCController.NPCState state)              => npc.EnterState(state);
    protected Quaternion GetPlanarLookRotation(Vector3 fwd)                => npc.GetPlanarLookRotation(fwd);

    // Blend-tree param names (read-only)
    protected string ParamBlend   => npc.paramBlend;
    protected string ParamMovingX => npc.paramMovingX;
    protected string ParamMovingY => npc.paramMovingY;

    // Shared config values that behaviours need
    protected float ArriveDistance  => npc.arriveDistance;
    protected float ActiveRadius    => npc.activeRadius;
    protected float TurnSmoothing   => npc.turnSmoothing;
    protected float TriggerMaxDuration => npc.triggerMaxDuration;
    protected Vector3 SpawnPoint    => npc.SpawnPoint;
}
