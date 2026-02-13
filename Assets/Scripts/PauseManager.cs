using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public bool IsPaused;
    public CanvasGroup PauseScreenPanel;
    // public GameObject VirtualCursor;
    private float OriginalSensitivity;
    public CanvasGroup HUD;
    public GameObject PauseNavver;
    
    
    private void Start()
    {
        OriginalSensitivity = GameMaster.Instance.MouseSensitivity;
        
        GameMaster.Instance.InputManager.PauseAction.action.performed += TogglePause;
    }

    private void OnDestroy()
    {
        // Prevent duplicate subscriptions if object is recreated
        if (GameMaster.Instance != null && GameMaster.Instance.InputManager != null) GameMaster.Instance.InputManager.PauseAction.action.performed -= TogglePause;
    }



    private void TogglePause(InputAction.CallbackContext callbackContext)
    {
        // just not going to allow pause when busy. making it close everything is awkward, maybe I'll rewrite with events later, but too busy now.
        if (GameMaster.Instance.PLAYERBUSY) return;

        
        if (IsPaused)
        {
            UnpauseGame();
        }
        else
        {
            PauseGame();
        }
        
        
    }



    private void PauseGame()
    {
        IsPaused = true;

        HUD.alpha = 0;
        
        Player.Instance.FirstPersonLook.sensitivity = GameMaster.Instance.MouseSensitivity * 10;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        
        GameMaster.Instance.InputManager.ExitAction.action.performed += TogglePause;
        
        Debug.Log("PAUSED");
        PauseNavver.SetActive(true);
        
        

        // if (VirtualCursor != null)
        // {
        //     if (GameMaster.Instance.DeviceType.selectedDeviceType == PlayerDeviceType.SteamOS) VirtualCursor.SetActive(true);
        // }
        
        GameMaster.Instance.EventManager.GamePaused(true);
        
        ToggleScreenPanel();
    }


    private void UnpauseGame()
    {
                
        Player.Instance.FirstPersonLook.sensitivity = OriginalSensitivity;
        
        HUD.alpha = 1;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        PauseNavver.SetActive(false);
        
        GameMaster.Instance.InputManager.ExitAction.action.performed -= TogglePause;
        
        // if (VirtualCursor != null) VirtualCursor.SetActive(false);
        
        IsPaused = false;

        GameMaster.Instance.EventManager.GamePaused(false);
        
        ToggleScreenPanel();
    }



    private void ToggleScreenPanel()
    {
        PauseScreenPanel.alpha = IsPaused ? 1f : 0f;
        PauseScreenPanel.interactable = IsPaused;
        PauseScreenPanel.blocksRaycasts = IsPaused;
    }
    
}
