using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public bool IsPaused;

    [Header("UI")]
    public CanvasGroup PauseScreenPanel;
    public CanvasGroup HUD;
    public GameObject PauseNavver;

    [Header("Focus Loss")]
    [Tooltip("Pause the game automatically when the player tabs out / minimizes the window")]
    public bool PauseOnFocusLost = true;

    private bool _bound;

    // Store the actual look sensitivity (not GameMaster mouse setting)
    private float _cachedLookSensitivity;
    private bool _hasCachedLookSensitivity;

    private void OnEnable() => BindInputs();

    private void Start()
    {
        BindInputs();
        ApplyPauseStateVisuals();
    }

    private void OnDisable()
    {
        UnbindInputs();
        SafeUnbindExitAction();

        if (IsPaused) RestoreLookSensitivityIfNeeded();
    }

    private void OnDestroy()
    {
        UnbindInputs();
        SafeUnbindExitAction();
    }

    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (GameMaster.Instance.PLAYERBUSY) return;
        if (!PauseOnFocusLost) return;

        var gm = GameMaster.Instance;
        if (gm == null || gm.PLAYERBUSY) return;

        if (!hasFocus)
        {
            // Lost focus → Pause
            if (!IsPaused)
                PauseGame();
        }
        else
        {
            // Regained focus → Unpause (standard behavior)
            if (IsPaused)
                UnpauseGame();
        }
    }

    // ====================== INPUT BINDING ======================

    private void BindInputs()
    {
        if (_bound) return;

        var gm = GameMaster.Instance;
        if (gm == null || gm.InputManager == null || gm.InputManager.PauseAction == null)
            return;

        gm.InputManager.PauseAction.action.performed -= TogglePause;
        gm.InputManager.PauseAction.action.performed += TogglePause;
        gm.InputManager.PauseAction.action.Enable();

        _bound = true;
    }

    private void UnbindInputs()
    {
        if (!_bound) return;

        var gm = GameMaster.Instance;
        if (gm?.InputManager?.PauseAction != null)
            gm.InputManager.PauseAction.action.performed -= TogglePause;

        _bound = false;
    }

    private void SafeBindExitAction()
    {
        var gm = GameMaster.Instance;
        if (gm?.InputManager?.ExitAction == null) return;

        gm.InputManager.ExitAction.action.performed -= TogglePause;
        gm.InputManager.ExitAction.action.performed += TogglePause;
        gm.InputManager.ExitAction.action.Enable();
    }

    private void SafeUnbindExitAction()
    {
        var gm = GameMaster.Instance;
        if (gm?.InputManager?.ExitAction == null) return;
        gm.InputManager.ExitAction.action.performed -= TogglePause;
    }

    // ====================== PAUSE LOGIC ======================

    private void TogglePause(InputAction.CallbackContext callbackContext)
    {
        if (!this || !gameObject) return;

        var gm = GameMaster.Instance;
        if (gm == null || gm.PLAYERBUSY) return;

        if (IsPaused)
            UnpauseGame();
        else
            PauseGame();
    }

    private void PauseGame()
    {
        IsPaused = true;
        CacheLookSensitivityIfNeeded();
        SetLookSensitivity(0f);
        SafeBindExitAction();
        ApplyPauseStateVisuals();
        EventManager.GamePaused(true);
    }

    private void UnpauseGame()
    {
        IsPaused = false;
        SafeUnbindExitAction();
        RestoreLookSensitivityIfNeeded();
        ApplyPauseStateVisuals();
        EventManager.GamePaused(false);
    }

    // ====================== LOOK SENSITIVITY ======================

    private void CacheLookSensitivityIfNeeded()
    {
        if (_hasCachedLookSensitivity) return;

        if (Player.Instance?.FirstPersonLook != null)
        {
            _cachedLookSensitivity = Player.Instance.FirstPersonLook.sensitivity;
            _hasCachedLookSensitivity = true;
        }
    }

    private void RestoreLookSensitivityIfNeeded()
    {
        if (!_hasCachedLookSensitivity) return;

        if (Player.Instance?.FirstPersonLook != null)
            Player.Instance.FirstPersonLook.sensitivity = _cachedLookSensitivity;

        _hasCachedLookSensitivity = false;
    }

    private void SetLookSensitivity(float value)
    {
        if (Player.Instance?.FirstPersonLook != null)
            Player.Instance.FirstPersonLook.sensitivity = value;
    }

    // ====================== VISUALS ======================

    private void ApplyPauseStateVisuals()
    {
        if (!this || !gameObject) return;

        if (HUD != null) 
            HUD.alpha = IsPaused ? 0f : 1f;

        Cursor.lockState = IsPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = IsPaused;

        if (PauseNavver != null) 
            PauseNavver.SetActive(IsPaused);

        if (PauseScreenPanel != null)
        {
            PauseScreenPanel.alpha = IsPaused ? 1f : 0f;
            PauseScreenPanel.interactable = IsPaused;
            PauseScreenPanel.blocksRaycasts = IsPaused;
        }
    }
}