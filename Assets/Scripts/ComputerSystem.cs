using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Steamworks;
using UnityEngine.EventSystems;

public class ComputerSystem : MonoBehaviour
{
    public ComputerScreen CurrentScreen = ComputerScreen.LockScreen;

    public CanvasGroup LockScreen;
    public CanvasGroup DesktopScreen;
    public List<CanvasGroup> AllProgrammeScreens = new List<CanvasGroup>();

    [Header("Furniture")]
    public GameObject NearestChair;

    [Header("Lock Screen Texts")]
    public TextMeshProUGUI IncorrectPasswordText;

    [Header("Desktop Texts")]
    public TextMeshProUGUI FilesText;
    public TextMeshProUGUI EmailText;
    public TextMeshProUGUI PhoneText;
    public TextMeshProUGUI WebText;
    public TextMeshProUGUI HackingText;
    public TextMeshProUGUI GamesText;
    public TextMeshProUGUI SettingsText;

    [Header("File Manager Texts")]
    public TextMeshProUGUI AddressBarText;

    public TextMeshProUGUI FavouritesText;
    public List<TextMeshProUGUI> DocumentsText;
    public List<TextMeshProUGUI> DownloadsText;
    public List<TextMeshProUGUI> PhotosText;
    public List<TextMeshProUGUI> VideosText;
    public List<TextMeshProUGUI> AudioText;

    public TextMeshProUGUI SystemText;

    public bool IsLoggedIn;
    public bool ComputerOpen;
    public TMP_InputField PasswordInput;

    public Transform ViewableArea;

    public Collider PCCollider;

    public List<PCNavver> PCScreenNavvers = new List<PCNavver>();

    public InputActionReference goBack;
    public InputActionReference escapeExit;

    public InputActionReference StepBackInputRightClick;

    public Transform PCScreenTransform;
    public Transform PlayerTransform;
    private Quaternion defaultRotation;

    private Callback<GamepadTextInputDismissed_t> _gamepadTextDismissed;
    private TMP_InputField _activeSteamInputField;

    // Steam modal tracking
    private bool _steamTextSessionOpen;

    public bool backButtonOverride;

    private const uint STEAM_TEXT_MAX = 64;

    // ✅ NEW: robust unsubscription guard
    private bool _unhooked;

    private void OnEnable()
    {
        // It’s safer to hook in OnEnable and unhook in OnDisable when scene objects are reloaded.
        HookEvents();
    }

    private void OnDisable()
    {
        UnhookEvents();
    }

    private void OnDestroy()
    {
        UnhookEvents();
    }

    private void Start()
    {
        if (IncorrectPasswordText != null)
            IncorrectPasswordText.gameObject.SetActive(false);

        PlayerTransform = GameMaster.Instance.Player.transform;
        defaultRotation = PCScreenTransform.rotation;

        if (SteamManager.Initialized)
            _gamepadTextDismissed = Callback<GamepadTextInputDismissed_t>.Create(OnGamepadTextInputDismissed);

        if (PasswordInput != null)
        {
            // NOTE: we remove listeners in UnhookEvents() using RemoveAllListeners() for safety.
            PasswordInput.onSelect.AddListener(_ => OnPasswordSelected());
            PasswordInput.onDeselect.AddListener(_ => OnPasswordDeselected());
            PasswordInput.onSubmit.AddListener(_ => OnPasswordSubmitted());
        }

        DisableNavvers();

        GameMaster.Instance.TerminalEventManager.FileManagerClosed();
        GameMaster.Instance.TerminalEventManager.VideoManagerClosed();
    }

    private void HookEvents()
    {
        // Prevent double-hook if OnEnable called twice
        UnhookEvents();
        _unhooked = false;

        // ✅ Static/global events MUST be unhooked, so we hook them here
        EventManager.OnStopComputer += OnStopComputer;

        TerminalEventManager.OnPCGridClick += SelectMenuItem;
        TerminalEventManager.OnCloseToDesktop += CloseToDesktop;

        // ✅ FIX: NO LAMBDA (can't unsubscribe). Use named handler.
        TerminalEventManager.OnBackButtonOverride += OnBackButtonOverrideChanged;

        if (goBack != null)
        {
            goBack.action.performed -= InputGoBack;
            goBack.action.performed += InputGoBack;
        }

        // Note: StepBackInputRightClick / escapeExit are subscribed dynamically while PC is open,
        // but we still force-remove them here as a safety net.
        if (StepBackInputRightClick != null)
            StepBackInputRightClick.action.performed -= InputGoBackBack;

        if (escapeExit != null)
            escapeExit.action.performed -= InputGoBackBack;
    }

    private void UnhookEvents()
    {
        if (_unhooked) return;
        _unhooked = true;

        EventManager.OnStopComputer -= OnStopComputer;

        TerminalEventManager.OnPCGridClick -= SelectMenuItem;
        TerminalEventManager.OnCloseToDesktop -= CloseToDesktop;
        TerminalEventManager.OnBackButtonOverride -= OnBackButtonOverrideChanged;

        if (goBack != null)
            goBack.action.performed -= InputGoBack;

        if (StepBackInputRightClick != null)
            StepBackInputRightClick.action.performed -= InputGoBackBack;

        if (escapeExit != null)
            escapeExit.action.performed -= InputGoBackBack;

        // Clean TMP listeners so destroyed objects don't keep references
        if (PasswordInput != null)
        {
            PasswordInput.onSelect.RemoveAllListeners();
            PasswordInput.onDeselect.RemoveAllListeners();
            PasswordInput.onSubmit.RemoveAllListeners();
        }
    }

    private void OnBackButtonOverrideChanged(bool value)
    {
        backButtonOverride = value;
    }

    private bool IsSteamOS()
    {
        return GameMaster.Instance.DeviceType.selectedDeviceType == PlayerDeviceType.SteamOS;
    }

    private void OnPasswordSelected()
    {
        if (!_steamTextSessionOpen) TryOpenSteamDeckKeyboardFor(PasswordInput, isPassword: true);
    }

    private void OnPasswordDeselected()
    {
        if (!_steamTextSessionOpen) _activeSteamInputField = null;
    }

    private void OnPasswordSubmitted() { LogOn(); }

    private void TryOpenSteamDeckKeyboardFor(TMP_InputField field, bool isPassword)
    {
        if (field == null) return;
        if (!IsSteamOS()) return;
        if (!SteamManager.Initialized) return;
        if (_steamTextSessionOpen) return;
        if (!ComputerOpen) return;

        _activeSteamInputField = field;

        var mode = isPassword
            ? EGamepadTextInputMode.k_EGamepadTextInputModePassword
            : EGamepadTextInputMode.k_EGamepadTextInputModeNormal;

        bool opened = SteamUtils.ShowGamepadTextInput(
            mode,
            EGamepadTextInputLineMode.k_EGamepadTextInputLineModeSingleLine,
            "Enter text",
            STEAM_TEXT_MAX,
            field.text
        );

        if (opened)
        {
            _steamTextSessionOpen = true;
        }
        else
        {
            _activeSteamInputField = null;
            _steamTextSessionOpen = false;
        }
    }

    private void OnGamepadTextInputDismissed(GamepadTextInputDismissed_t cb)
    {
        _steamTextSessionOpen = false;

        if (!cb.m_bSubmitted)
        {
            _activeSteamInputField = null;
            return;
        }

        if (_activeSteamInputField == null)
            return;

        uint len = SteamUtils.GetEnteredGamepadTextLength();
        if (len == 0)
        {
            _activeSteamInputField.SetTextWithoutNotify(string.Empty);
            _activeSteamInputField.ForceLabelUpdate();
            _activeSteamInputField = null;
            return;
        }

        if (SteamUtils.GetEnteredGamepadTextInput(out string result, STEAM_TEXT_MAX))
        {
            _activeSteamInputField.SetTextWithoutNotify(result);
            _activeSteamInputField.ForceLabelUpdate();

            int end = _activeSteamInputField.text.Length;
            _activeSteamInputField.caretPosition = end;
            _activeSteamInputField.selectionAnchorPosition = end;
            _activeSteamInputField.selectionFocusPosition = end;

            LogOn();
        }

        _activeSteamInputField = null;
    }

    public void OnStartComputer()
    {
        if (ComputerOpen) return;
        if (GameMaster.Instance.PLAYERBUSY) return;

        GameMaster.Instance.PLAYERBUSY = true;
        Player.Instance.ZoomOverride = true;

        FacePlayerOnY();

        if (PCCollider != null) PCCollider.enabled = false;
        if (NearestChair != null) NearestChair.gameObject.SetActive(false);

        GameMaster.Instance.EventManager.StartComputer(ViewableArea);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (Player.Instance.CrossHair != null)
            Player.Instance.CrossHair.GetComponent<CanvasGroup>().alpha = 0.0f;

        if (StepBackInputRightClick != null)
        {
            StepBackInputRightClick.action.performed -= InputGoBackBack;
            StepBackInputRightClick.action.performed += InputGoBackBack;
        }

        if (escapeExit != null)
        {
            escapeExit.action.performed -= InputGoBackBack;
            escapeExit.action.performed += InputGoBackBack;
        }

        ChangeScreen(IsLoggedIn ? ComputerScreen.Desktop : ComputerScreen.LockScreen);
        ComputerOpen = true;
    }

    public void OnStopComputer()
    {
        if (!ComputerOpen) return;
        if (!GameMaster.Instance.PLAYERBUSY) return;

        Player.Instance.ZoomOverride = false;

        RotBack();

        GameMaster.Instance.TerminalEventManager.FileManagerClosed();
        GameMaster.Instance.TerminalEventManager.VideoManagerClosed();

        ChangeScreen(IsLoggedIn ? ComputerScreen.Desktop : ComputerScreen.LockScreen);

        if (PCCollider != null) PCCollider.enabled = true;
        if (NearestChair != null) NearestChair.gameObject.SetActive(true);

        if (IncorrectPasswordText != null)
            IncorrectPasswordText.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (Player.Instance.CrossHair != null)
            Player.Instance.CrossHair.GetComponent<CanvasGroup>().alpha = 1.0f;

        if (StepBackInputRightClick != null)
            StepBackInputRightClick.action.performed -= InputGoBackBack;

        if (escapeExit != null)
            escapeExit.action.performed -= InputGoBackBack;

        ClosePasswordFieldCompletely();
        ComputerOpen = false;
        GameMaster.Instance.PLAYERBUSY = false;
        DisableNavvers();
    }

    private bool CanHandlePCInput(InputAction.CallbackContext ctx)
    {
        if (!GameMaster.Instance.PLAYERBUSY) return false;
        if (!ctx.performed) return false;
        return true;
    }

    public void InputGoBack(InputAction.CallbackContext callbackContext)
    {
        if (!ComputerOpen) return;
        if (backButtonOverride) return;
        if (!CanHandlePCInput(callbackContext)) return;

        HandleBackStepOnly();
    }

    public void InputGoBackBack(InputAction.CallbackContext callbackContext)
    {
        if (!ComputerOpen) return;
        if (!CanHandlePCInput(callbackContext)) return;

        HandleBackStepOnly();
    }

    public void InputClosePC(InputAction.CallbackContext callbackContext)
    {
        if (ComputerOpen) return;
        GameMaster.Instance.EventManager.StopComputer();
    }

    public void HandleBackStepOnly()
    {
        if (CurrentScreen == ComputerScreen.Files)
        {
            PCFileManager fm = GetFileManager();
            if (fm != null && fm.CurrentFolder != FileGriddle.User)
            {
                fm.FileManagerNavver.enabled = true;
                fm.ChangeScreen(FolderScreen.User);
                return;
            }

            ChangeScreen(ComputerScreen.Desktop);
            return;
        }

        if (CurrentScreen != ComputerScreen.Desktop && CurrentScreen != ComputerScreen.LockScreen)
        {
            ChangeScreen(ComputerScreen.Desktop);
        }
        else
        {
            GameMaster.Instance.EventManager.StopComputer();
        }
    }

    private PCFileManager GetFileManager()
    {
        // ✅ Remove destroyed entries first (prevents MissingReferenceException after scene reload)
        AllProgrammeScreens.RemoveAll(cg => cg == null);

        for (int i = 0; i < AllProgrammeScreens.Count; i++)
        {
            var cg = AllProgrammeScreens[i];
            if (cg == null) continue;

            PCScreen pc = cg.GetComponent<PCScreen>();
            if (pc != null && pc.ThisPCScreen == ComputerScreen.Files)
                return cg.GetComponent<PCFileManager>();
        }
        return null;
    }

    public void LogOn()
    {
        if (PasswordInput == null) return;

        if (PasswordInput.text.ToString().Trim().Equals(GameMaster.Instance.NORASPCPASSWORD.ToString().Trim()))
        {
            ChangeScreen(ComputerScreen.Desktop);
            IsLoggedIn = true;
        }
        else
        {
            ChangeScreen(ComputerScreen.LockScreen);

            if (IncorrectPasswordText != null)
                IncorrectPasswordText.gameObject.SetActive(true);

            IsLoggedIn = false;
            ClosePasswordFieldCompletely();
        }
    }

    private void ClosePasswordFieldCompletely()
    {
        if (PasswordInput == null) return;

        _activeSteamInputField = null;

        PasswordInput.DeactivateInputField();
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);

        PasswordInput.SetTextWithoutNotify(string.Empty);
        PasswordInput.caretPosition = 0;
        PasswordInput.selectionAnchorPosition = 0;
        PasswordInput.selectionFocusPosition = 0;
        PasswordInput.ForceLabelUpdate();
    }

    public void SelectMenuItem(PCGriddle pcGriddle)
    {
        Debug.Log("selected menu item: " + pcGriddle);

        GameMaster.Instance.TerminalEventManager.FileManagerClosed();
        switch (pcGriddle)
        {
            case PCGriddle.None:
                break;

            case PCGriddle.Desktop:
                ChangeScreen(ComputerScreen.Desktop);
                break;

            case PCGriddle.LogOn:
                LogOn();
                break;

            case PCGriddle.Files:
                ChangeScreen(ComputerScreen.Files);
                GameMaster.Instance.TerminalEventManager.FileManagerStarted();
                break;

            case PCGriddle.Phone:
                break;

            case PCGriddle.Web:
                break;

            case PCGriddle.Hacking:
                break;

            case PCGriddle.Games:
                break;

            case PCGriddle.Settings:
                break;
        }
    }

    public void ChangeScreen(ComputerScreen ScreenToOpen)
    {
        CloseAllScreens();

        // ✅ Remove destroyed entries first
        AllProgrammeScreens.RemoveAll(cg => cg == null);

        for (int i = 0; i < AllProgrammeScreens.Count; i++)
        {
            var cg = AllProgrammeScreens[i];
            if (cg == null) continue;

            PCScreen pcScreen = cg.GetComponent<PCScreen>();
            if (pcScreen != null && pcScreen.ThisPCScreen.Equals(ScreenToOpen))
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;

                if (pcScreen.PCNavver != null)
                    pcScreen.PCNavver.enabled = true;

                CurrentScreen = ScreenToOpen;
                break;
            }
        }
    }

    public void CloseAllScreens()
    {
        Debug.Log("Close screens");

        // ✅ Fix: destroyed references can remain in the list after scene changes
        AllProgrammeScreens.RemoveAll(cg => cg == null);

        for (int i = 0; i < AllProgrammeScreens.Count; i++)
        {
            var cg = AllProgrammeScreens[i];
            if (cg == null) continue;

            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;

            var pc = cg.GetComponent<PCScreen>();
            if (pc != null && pc.PCNavver != null)
                pc.PCNavver.enabled = false;
        }
    }

    public void FacePlayerOnY()
    {
        if (PCScreenTransform == null || PlayerTransform == null) return;

        Vector3 toPlayer = PlayerTransform.position - PCScreenTransform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        float targetYaw = Quaternion.LookRotation(toPlayer, Vector3.up).eulerAngles.y;

        Vector3 e = PCScreenTransform.rotation.eulerAngles;
        PCScreenTransform.rotation = Quaternion.Euler(e.x, targetYaw, e.z);
    }

    public void RotBack()
    {
        if (PCScreenTransform == null) return;
        PCScreenTransform.rotation = defaultRotation;
    }

    public void OnPasswordFieldClicked()
    {
        PasswordInput?.ActivateInputField();
        PasswordInput?.Select();

        if (!_steamTextSessionOpen)
            TryOpenSteamDeckKeyboardFor(PasswordInput, isPassword: true);
    }

    private void DisableNavvers()
    {
        for (int i = 0; i < PCScreenNavvers.Count; i++)
        {
            if (PCScreenNavvers[i] != null)
                PCScreenNavvers[i].enabled = false;
        }
    }

    public void CloseToDesktop()
    {
        SelectMenuItem(PCGriddle.Desktop);
    }
}

public enum ComputerScreen
{
    LockScreen = 1000,
    Desktop = 1001,
    Files = 1002,
    Phone = 1003,
    Emails = 1004,
    Web = 1005,
    Hacking = 1006,
    Games = 1007,
    Settings = 1008
}

public enum PCGriddle
{
    None = 4000,
    LogOn = 4001,
    LockScreen = 10001,
    Desktop = 10002,
    Phone = 10003,
    Emails = 10004,
    Web = 10005,
    Hacking = 10006,
    Games = 10007,
    Settings = 10008,
    Files = 10009,
}
