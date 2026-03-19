using System;
using TMPro;
using UnityEngine;

public class PCOwner : MonoBehaviour
{
    public TextMeshProUGUI pcOwnerText;
    public NPC PCOwningNPC;


    private void Start()
    {
        
        pcOwnerText.text = PCOwningNPC.ToString().Replace("_"," ");
    }
}
