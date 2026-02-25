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
    
    // Generic Characters
    ScientistKlaus = 500,
} 

public enum NPCAllegiance
{
    Neutral = 100,
    Enemy = 101,
    Friend = 102,
} 