using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public abstract class NPCBehaviourBase : MonoBehaviour
{
    protected NPCController npc { get; private set; }
    public virtual void Init(NPCController controller) { npc = controller; }

    protected Animator     Anim  => npc.animationController;
    protected NavMeshAgent Agent => npc.agent;
    protected Transform    Body  => npc.transform;

    protected bool       AgentReady()                                                      => npc.AgentReady();
    protected bool       TryGetNavmeshPoint(Vector3 near, out Vector3 navPos)              => npc.TryGetNavmeshPoint(near, out navPos);
    protected bool       TryGetNavmeshPointNear(Vector3 near, float r, out Vector3 navPos) => npc.TryGetNavmeshPointNear(near, r, out navPos);
    protected void       ForceUpright()                                                    => npc.ForceUpright();
    protected bool       GroundTransformToNavmesh(Vector3 preferred, float r)              => npc.GroundTransformToNavmesh(preferred, r);
    protected bool       RestoreStandingBodyAt(Vector3 preferred, float r)                 => npc.RestoreStandingBodyAt(preferred, r);
    protected void       DetachAgentForAnimation()                                         => npc.DetachAgentForAnimation();
    protected void       ReattachAgentToNavmeshAtCurrentXZ()                               => npc.ReattachAgentToNavmeshAtCurrentXZ();
    protected void       ForceReturnToLocomotion()                                         => npc.ForceReturnToLocomotion();
    protected void       ForceIdlePose()                                                   => npc.ForceIdlePose();
    protected void       ResetAllAnimatorTriggers()                                        => npc.ResetAllAnimatorTriggers();
    protected void       EnterState(NPCState state)                                        => npc.EnterState(state);
    protected Quaternion GetPlanarLookRotation(Vector3 fwd)                                => npc.GetPlanarLookRotation(fwd);

    protected string  ParamBlend         => npc.paramBlend;
    protected string  ParamMovingX       => npc.paramMovingX;
    protected string  ParamMovingY       => npc.paramMovingY;
    protected float   ArriveDistance     => npc.arriveDistance;
    protected float   ActiveRadius       => npc.activeRadius;
    protected float   TurnSmoothing      => npc.turnSmoothing;
    protected float   TriggerMaxDuration => npc.triggerMaxDuration;
    protected Vector3 SpawnPoint         => npc.SpawnPoint;
}