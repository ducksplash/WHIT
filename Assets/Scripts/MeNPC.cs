using System;
using System.Collections.Generic;
using UnityEngine;

public class MeNPC : MonoBehaviour
{
    public List<SkinnedMeshRenderer> npcRendererOutfitOne;
    public List<SkinnedMeshRenderer> npcRendererOutfitTwo;


    private void Start()
    {
        ToggleFirstOutfit(true);
    }

    public void ToggleFirstOutfit(bool overrideOn = false)
    {
        foreach (var meshy in npcRendererOutfitOne)
        {
            if (!overrideOn)
            {
                meshy.enabled = !meshy.enabled;
            }
            else
            {
                meshy.enabled = true;
            }
        }
    }
    public void ToggleSecondOutfit()
    {
        foreach (var meshy in npcRendererOutfitTwo)
        {
            meshy.enabled = !meshy.enabled;
        }
    }
}
