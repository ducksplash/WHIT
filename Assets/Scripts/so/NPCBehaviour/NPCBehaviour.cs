using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;

[CreateAssetMenu(
    fileName = "NPCBehaviour",
    menuName = "{!!} Tawley Scriptable Object/NPCBehaviour",
    order = 10)]
public class NPCBehaviour : ScriptableObject
{

    [Header("NPC Behaviour")]
    
    public Behaviour BehaviourType = Behaviour.idle;

    // if Behaviour is Dialogue, use this dialogue
    public DialogueName selectedDialogue; // an enum

    // if Behaviour is Act, use this Animator, note that this needs a reference to be added for specified NPC prior to playback
    public Animator npcAnimator; // 
    
    // animation state to blend into
    public string animationState = "idle";
    
    // If Behaviour is Go, use these waypoints
    public List<Vector3> waypointVectors = new List<Vector3>(); 
    
    // Optional
    public bool isTimed;

    // If timed checked, run the 'Behaviour' for this amount of time, 
    public float timer;
    
    
    
}
