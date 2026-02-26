using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;

[CreateAssetMenu(
    fileName = "NPCRoutine",
    menuName = "{!!} Tawley Scriptable Object/NPCRoutine",
    order = 10)]
public class NPCRoutine : ScriptableObject
{

    [Header("NPC Behaviour Routine")]
    
    public Routine RoutineType = Routine.idle;

    public List<NPCBehaviour> RoutineBehaviours = new List<NPCBehaviour>();

    public bool looping;



}
