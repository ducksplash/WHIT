using System;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public List<NPCController> NPCList = new List<NPCController>();
    public List<GameObject> NPCPrefabs = new List<GameObject>();

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

        if (!NPCList.Contains(thisNPC))
        {
            thisNPC.sceneNPCManager = this;
            NPCList.Add(thisNPC);
            Debug.Log($"{thisNPC} added");
        }
    }

    public void UnregisterNPC(NPCController thisNPC)
    {
        if (thisNPC == null) return;
        NPCList.Remove(thisNPC);
    }

    public GameObject GetPrefabForNPC(NPC npcType)
    {
        for (int i = 0; i < NPCPrefabs.Count; i++)
        {
            GameObject prefab = NPCPrefabs[i];
            if (prefab == null) continue;

            NPCController controller = prefab.GetComponent<NPCController>();
            if (controller == null)
                controller = prefab.GetComponentInChildren<NPCController>();

            if (controller == null) continue;

            if (controller.thisNPC == npcType)
                return prefab;
        }

        Debug.LogWarning($"{nameof(NPCManager)}: No prefab found for NPC enum '{npcType}'.");
        return null;
    }

    public List<NPC> GetSpawnableNPCs()
    {
        List<NPC> result = new List<NPC>();
        HashSet<NPC> seen = new HashSet<NPC>();

        for (int i = 0; i < NPCPrefabs.Count; i++)
        {
            GameObject prefab = NPCPrefabs[i];
            if (prefab == null) continue;

            NPCController controller = prefab.GetComponent<NPCController>();
            if (controller == null)
                controller = prefab.GetComponentInChildren<NPCController>();

            if (controller == null) continue;

            NPC npcType = controller.thisNPC;
            if (seen.Add(npcType))
                result.Add(npcType);
        }

        return result;
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
    Dale = 508,
    Nora = 999,
    ZTESTNora = 998,
    ZTESTEimear = 997,
    ZTESTKim_Shae = 996,
    ZTESTShauna = 995,
    ZTESTPresha = 994,
    ZTESTJasmine = 993,
    ZTESTSaoirse = 992,
    ZTESTDiane = 991,
    ZTESTMairead = 990,
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