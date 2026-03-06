using System.Collections.Generic;
using UnityEngine;

public class MeNPC : MonoBehaviour
{
    public List<SkinnedMeshRenderer> npcRenderer;


    public void ToggleRenderer()
    {
        foreach (var meshy in npcRenderer)
        {
            meshy.enabled = !meshy.enabled;
        }
    }
}
