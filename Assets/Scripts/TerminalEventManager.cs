using System;
using UnityEngine;

public class TerminalEventManager : MonoBehaviour
{
    public static event Action<PCGriddle> OnPCGridClick = (PCGriddle) => { };
    
    public void PCGridClick(PCGriddle selectedGridItem)
    {
        OnPCGridClick.Invoke(selectedGridItem);
    }
}
