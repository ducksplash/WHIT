using System;
using UnityEngine;

public class TerminalEventManager : MonoBehaviour
{
    public static event Action<PCGriddle> OnPCGridClick = (PCGriddle) => { };
    public static event Action<FileGriddle> OnFileGridClick = (FileGriddle) => { };
    public static event Action OnOverrideClick = () => { };
    
    public void PCGridClick(PCGriddle selectedGridItem)
    {
        OnPCGridClick.Invoke(selectedGridItem);
    }

    public void FileGridClick(FileGriddle selectedGridItem)
    {
        OnFileGridClick.Invoke(selectedGridItem);
    }

    public void OverrideClick()
    {
        OnOverrideClick.Invoke();
    }
}
