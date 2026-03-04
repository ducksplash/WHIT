using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public List<NPCController> NPCList = new List<NPCController>();
    


    public void RegisterNPC(NPCController thisNPC)
    {
        if (NPCList.Contains(thisNPC))
        {
            Debug.Log("found");
        }
        else
        {
            
            NPCList.Add(thisNPC);
        }
    }
}



public enum NPC
{
    
    // Storyline Characters
    EimearScott = 100,
    KimShae = 101,
    HollowMan = 102,
    KieronScott = 103,
    TomOneill = 104,
    EllsworthOhanlon = 105,
    MichaelDevlin = 106,
    AlternativeNora = 999,
    
    // Generic Characters
    ScientistKlaus = 500,
    HomelessManOne = 501,
    Shauna = 502,
    Presha = 503,
    Jasmine = 504,
    Saoirse = 505,
} 

public enum NPCAllegiance
{
    Neutral = 100,
    Enemy = 101,
    Friend = 102,
}

public enum Behaviour
{
    idle = 100,
    go = 101,
    say = 102,
    act = 103,
    die = 104
    
}
public enum Routine
{
    idle = 100,
    Patrol = 101,
    ScientistPatrolWarnPlayer = 102,
    HomelessSleeping = 103,
    HomelessSittingIdling = 103,
    HomelessStandingIdling = 104,
    HomelessStaggering = 105,
    
    
}