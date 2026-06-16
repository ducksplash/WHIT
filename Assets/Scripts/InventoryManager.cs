using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public CanvasGroup InventoryCanvas;
    public InputActionReference InventoryToggle;

    public bool InventoryActive;

    private float _cachedLookSensitivity;
    private bool _hasCachedSensitivity;

    private void OnEnable()
    {
        if (InventoryToggle != null)
        {
            InventoryToggle.action.performed += ToggleInventory;
            InventoryToggle.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (InventoryToggle != null)
            InventoryToggle.action.performed -= ToggleInventory;

        SafeUnbindExitAction();

        if (InventoryActive)
            RestoreLookSensitivity();
    }

    private void ToggleInventory(InputAction.CallbackContext ctx)
    {
        if (InventoryActive)
            CloseInventory();
        else
            OpenInventory();
    }

    private void OpenInventory()
    {
        InventoryActive = true;

        GameMaster.Instance.PLAYERBUSY = true;

        CacheLookSensitivity();
        SetLookSensitivity(0f);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SafeBindExitAction();

        ApplyVisuals();
    }

    private void CloseInventory()
    {
        InventoryActive = false;

        GameMaster.Instance.PLAYERBUSY = false;

        RestoreLookSensitivity();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SafeUnbindExitAction();

        ApplyVisuals();
    }

    private void ApplyVisuals()
    {
        if (InventoryCanvas == null)
            return;

        InventoryCanvas.alpha = InventoryActive ? 0.85f : 0f;
        InventoryCanvas.blocksRaycasts = InventoryActive;
        InventoryCanvas.interactable = InventoryActive;
    }

    private void CacheLookSensitivity()
    {
        if (_hasCachedSensitivity) return;

        if (Player.Instance?.FirstPersonLook != null)
        {
            _cachedLookSensitivity = Player.Instance.FirstPersonLook.sensitivity;
            _hasCachedSensitivity = true;
        }
    }

    private void RestoreLookSensitivity()
    {
        if (!_hasCachedSensitivity) return;

        if (Player.Instance?.FirstPersonLook != null) Player.Instance.FirstPersonLook.sensitivity = _cachedLookSensitivity;

        _hasCachedSensitivity = false;
    }

    private void SetLookSensitivity(float value)
    {
        if (Player.Instance?.FirstPersonLook != null) Player.Instance.FirstPersonLook.sensitivity = value;
    }

    private void SafeBindExitAction()
    {
        var gm = GameMaster.Instance;

        if (gm?.InputManager?.ExitAction == null)
            return;

        gm.InputManager.ExitAction.action.performed -= ToggleInventory;
        gm.InputManager.ExitAction.action.performed += ToggleInventory;
    }

    private void SafeUnbindExitAction()
    {
        var gm = GameMaster.Instance;

        if (gm?.InputManager?.ExitAction == null)
            return;

        gm.InputManager.ExitAction.action.performed -= ToggleInventory;
    }
}