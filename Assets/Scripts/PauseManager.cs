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

    private void OnDestroy()
    {
        // Prevent duplicate subscriptions if object is recreated
        if (GameMaster.Instance != null && GameMaster.Instance.InputManager != null)
            GameMaster.Instance.InputManager.PauseAction.action.performed -= TogglePause;
    }

    public void TogglePause(InputAction.CallbackContext callbackContext)
    {
        if (!callbackContext.performed) return;
        if (GameMaster.Instance.CutsceneManager.CutsceneInProgress) return;

        // If we're paused, ALWAYS unpause on ESC (even if PLAYERBUSY flags got weird)
        if (IsPaused)
        {
            SetPaused(false);
            return;
        }

        // If player is "busy", ESC should close that UI and NOT toggle pause.
        if (GameMaster.Instance.PLAYERBUSY || GameMaster.Instance.TravelCompanion.CompanionIsOpen)
        {
            CloseBusyUI();
            return;
        }

        // Normal pause toggle
        SetPaused(true);
    }

    private void CloseBusyUI()
    {
        // Close computer/phone if they are up
        if (GameMaster.Instance.PLAYERBUSY)
        {
            GameMaster.Instance.EventManager.StopComputer();
            GameMaster.Instance.EventManager.StopPhone();
        }

        // Close companion if open
        if (GameMaster.Instance.TravelCompanion.CompanionIsOpen)
        {
            GameMaster.Instance.TravelCompanion.LaunchCompanion();
        }

        // Ensure pause UI is not showing
        if (IsPaused)
            SetPaused(false);

        // Restore gameplay cursor state
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetPaused(bool paused)
    {
        IsPaused = paused;

        GameMaster.Instance.PLAYERBUSY = paused;

        if (pauseUnderlay != null)
        {
            pauseUnderlay.alpha = paused ? 1f : 0f;
            pauseUnderlay.interactable = paused;
            pauseUnderlay.blocksRaycasts = paused;
        }

        if (VirtualCursor != null)
            VirtualCursor.SetActive(paused);

        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;

        // (This line looks odd in your original: sensitivity * 10 when paused.)
        // Kept behavior but you may actually want 0 when paused.
        Player.Instance.FirstPersonLook.sensitivity =
            paused ? GameMaster.Instance.MouseSensitivity * 10f : GameMaster.Instance.MouseSensitivity;

        GameMaster.Instance.EventManager.GamePaused(paused);

        Debug.Log("Pause set to " + paused + " Cursor.visible " + Cursor.visible);
    }
}
