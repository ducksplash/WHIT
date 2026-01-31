using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public bool IsPaused;
    public CanvasGroup pauseUnderlay;
    public GameObject VirtualCursor;
    
    
    private void Start()
    {
        GameMaster.Instance.InputManager.PauseAction.action.performed += TogglePause;
    }

    public void TogglePause(InputAction.CallbackContext callbackContext)
    {
        if (GameMaster.Instance.CutsceneManager.CutsceneInProgress) return;
        
        
        
        if (GameMaster.Instance.ONPC || GameMaster.Instance.PHONEOUT)
        {
            if (GameMaster.Instance.ONPC)
            {
                GameMaster.Instance.EventManager.StopComputer();
            }
            if (GameMaster.Instance.PHONEOUT)
            {
                Player.Instance.PlayerPhone.ConsolePhoneButtonAction();
            }
        }
        else
        {
            PauseAction();
        }
        
        Cursor.lockState = IsPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = IsPaused;
        Debug.Log("Cursor.visible "+Cursor.visible);
    }


    private void PauseAction()
    {
        
        IsPaused = !IsPaused;
        GameMaster.Instance.INMENU = IsPaused;
        pauseUnderlay.alpha = IsPaused ? 1 : 0;
        GameMaster.Instance.FROZEN = IsPaused;
        VirtualCursor.SetActive(IsPaused);
        Player.Instance.FirstPersonLook.sensitivity = IsPaused ? GameMaster.Instance.MouseSensitivity * 10 : GameMaster.Instance.MouseSensitivity;
        GameMaster.Instance.EventManager.GamePaused(IsPaused);
        
    }
}
