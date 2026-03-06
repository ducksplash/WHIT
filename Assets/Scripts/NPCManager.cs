using System;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public List<NPCController> NPCList = new List<NPCController>();

    private void Awake()
    {
        EventManager.OnRegisterNPC += RegisterNPC;
    }

    private void OnDisable()
    {
        EventManager.OnRegisterNPC -= RegisterNPC;
    }

    public void RegisterNPC(NPCController thisNPC)
    {
        if (thisNPC == null) return;

        Debug.Log("attempting to register: " + thisNPC);

        if (!NPCList.Contains(thisNPC))
        {
            thisNPC.sceneNPCManager = this;
            NPCList.Add(thisNPC);
            Debug.Log(thisNPC + " added");
        }
    }
}

public enum NPC
{
    
    // Storyline Characters
    Eimear_Scott = 100,
    Kim_Shae = 101,
    Hollow = 102,
    Kieron_Scott = 103,
    Tom_Oneill = 104,
    Ellsworth_Ohanlon = 105,
    Michael_Devlin = 106,
    
    
    Klaus = 500,
    Mike = 501,
    Shauna = 502,
    Presha = 503,
    Jasmine = 504,
    Saoirse = 505,
    Diane = 506,
    Mairead = 507,
    AlternativeNora = 999,
    
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