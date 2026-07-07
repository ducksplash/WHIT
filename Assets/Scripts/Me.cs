using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Me : MonoBehaviour
{
    [Header("Character Details")]
    public string ThisCharacterName;

    [Header("NPC Controller")]
    public NPCController npcController;
    
    private void Start()
    {
        try
        {
            npcController = GetComponent<NPCController>();
            ThisCharacterName = npcController.thisNPC.ToString().Replace("_", " ");
        }
        catch
        {
            ThisCharacterName = "Not Found";
        }
        
    }


    public void ToggleWorkOutfit()
    {
        Debug.Log("npc outfits todo");
    }

    public void ToggleCasualOutfit()
    {
        Debug.Log("npc outfits todo");
    }

    public void ToggleThirdOutfit()
    {
        Debug.Log("npc outfits todo");
    }

}