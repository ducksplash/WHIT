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
    public List<CanvasGroup> AllProgrammeScreens;

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
    public CanvasGroup CrosshairCanvas;

    public bool IsLoggedIn;
    public bool ComputerOpen;
    public TMP_InputField PasswordInput;

    public Transform ViewableArea;

    public Collider PCCollider;

    public List<PCNavver> PCScreenNavvers = new List<PCNavver>();
    

    public InputActionReference goBack;
    public InputActionReference rightClickExit;

    public Transform PCScreenTransform;
    public Transform PlayerTransform;
    private Quaternion defaultRotation;

    private Callback<GamepadTextInputDismissed_t> _gamepadTextDismissed;
    private TMP_InputField _activeSteamInputField;

    // Steam modal tracking
    private bool _steamTextSessionOpen;

    private const uint STEAM_TEXT_MAX = 64;

    void Start()
    {
        IncorrectPasswordText.gameObject.SetActive(false);

        EventManager.OnStopComputer += OnStopComputer;
        
        if (goBack != null)
        {
            goBack.action.performed -= InputGoBack;
            goBack.action.performed += InputGoBack;
        }

        TerminalEventManager.OnPCGridClick += SelectMenuItem;

        PlayerTransform = GameMaster.Instance.Player.transform;
        defaultRotation = PCScreenTransform.rotation;

        if (SteamManager.Initialized)
            _gamepadTextDismissed = Callback<GamepadTextInputDismissed_t>.Create(OnGamepadTextInputDismissed);

        if (PasswordInput != null)
        {
            PasswordInput.onSelect.AddListener(_ => OnPasswordSelected());
            PasswordInput.onDeselect.AddListener(_ => OnPasswordDeselected());
            PasswordInput.onSubmit.AddListener(_ => OnPasswordSubmitted());
        }

        DisableNavvers();
        
        
        TerminalEventManager.OnCloseToDesktop += CloseToDesktop;
        
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
        ComputerOpen = true;
        
        FacePlayerOnY();

        PCCollider.enabled = false;
        NearestChair.gameObject.SetActive(false);
        GameMaster.Instance.EventManager.StartComputer(ViewableArea);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        CrosshairCanvas.GetComponent<CanvasGroup>().alpha = 0.0f;

        // rightClickExit ALWAYS closes PC while on PC
        if (rightClickExit != null)
        {
            rightClickExit.action.performed -= InputClosePC;
            rightClickExit.action.performed += InputClosePC;
        }

        ChangeScreen(IsLoggedIn ? ComputerScreen.Desktop : ComputerScreen.LockScreen);
    }

    public void OnStopComputer()
    {
        if (!ComputerOpen) return;
        if (!GameMaster.Instance.PLAYERBUSY) return;

        
        RotBack();

        if (rightClickExit != null) rightClickExit.action.performed -= InputClosePC;
        
        GameMaster.Instance.TerminalEventManager.FileManagerClosed();
        
        ChangeScreen(IsLoggedIn ? ComputerScreen.Desktop : ComputerScreen.LockScreen);
        
        PCCollider.enabled = true;
        NearestChair.gameObject.SetActive(true);
        IncorrectPasswordText.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        CrosshairCanvas.GetComponent<CanvasGroup>().alpha = 1.0f;

        ClosePasswordFieldCompletely();        
        ComputerOpen = false;
        GameMaster.Instance.PLAYERBUSY = false;
        DisableNavvers();
    }

    private bool CanHandlePCInput(InputAction.CallbackContext ctx)
    {
        // Only handle inputs while the PC is actually open
        if (!GameMaster.Instance.PLAYERBUSY) return false;
        if (!ctx.performed) return false;
        return true;
    }

    // goBack: step back until Desktop, THEN close PC if pressed again on Desktop/Lock
    public void InputGoBack(InputAction.CallbackContext callbackContext)
    {
        if (!CanHandlePCInput(callbackContext))
            return;

        HandleBackStepOnly();
    }

    // rightClickExit: always close PC immediately
    public void InputClosePC(InputAction.CallbackContext callbackContext)
    {
        if (ComputerOpen) return;
        GameMaster.Instance.EventManager.StopComputer();
    }

    /// <summary>
    /// Step back behaviour:
    /// - If in Files and NOT at User folder: go to User folder (stay in Files)
    /// - Else if in Files at User: go to Desktop
    /// - Else if in any other app screen: go to Desktop
    /// - Else if already at Desktop or Lock: close PC
    /// </summary>
    public void HandleBackStepOnly()
    {
        // 1) Files screen: sub-step back inside file manager first
        if (CurrentScreen == ComputerScreen.Files)
        {
            PCFileManager fm = GetFileManager();
            if (fm != null && fm.CurrentFolder != FileGriddle.User)
            {
                fm.ChangeScreen(FolderScreen.User);
                return;
            }

            // At Files root -> go to Desktop
            ChangeScreen(ComputerScreen.Desktop);
            return;
        }

        // 2) Any other non-root screen -> go to Desktop
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
        for (int i = 0; i < AllProgrammeScreens.Count; i++)
        {
            PCScreen pc = AllProgrammeScreens[i].GetComponent<PCScreen>();
            if (pc != null && pc.ThisPCScreen == ComputerScreen.Files)
                return AllProgrammeScreens[i].GetComponent<PCFileManager>();
        }
        return null;
    }

    public void LogOn()
    {
        if (PasswordInput.text.ToString().Trim().Equals(GameMaster.Instance.NORASPCPASSWORD.ToString().Trim()))
        {
            ChangeScreen(ComputerScreen.Desktop);
            IsLoggedIn = true;
        }
        else
        {
            ChangeScreen(ComputerScreen.LockScreen);
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
        Debug.Log("selected menu item: "+pcGriddle);
        
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
        
        for (var i = 0; i < AllProgrammeScreens.Count; i++)
        {
            if (AllProgrammeScreens[i].GetComponent<PCScreen>().ThisPCScreen.Equals(ScreenToOpen))
            {
                CanvasGroup SelectedScreen = AllProgrammeScreens[i].GetComponent<CanvasGroup>();

                SelectedScreen.alpha = 1;
                SelectedScreen.interactable = true;
                SelectedScreen.interactable = true;
                SelectedScreen.blocksRaycasts = true;

                PCScreen thisScreen = AllProgrammeScreens[i].GetComponent<PCScreen>();
                
                if (thisScreen.PCNavver != null) thisScreen.PCNavver.enabled = true;
                CurrentScreen = ScreenToOpen;
            }
        }
    }

    public void CloseAllScreens()
    {
        Debug.Log("Close screens");
        for (var i = 0; i < AllProgrammeScreens.Count; i++)
        {
            CanvasGroup ThisScreen = AllProgrammeScreens[i].GetComponent<CanvasGroup>();

            ThisScreen.alpha = 0;
            ThisScreen.interactable = false;
            ThisScreen.blocksRaycasts = false;

            PCScreen thisScreen = AllProgrammeScreens[i].GetComponent<PCScreen>();
            if (thisScreen.PCNavver != null) thisScreen.PCNavver.enabled = false;
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
        for (var i = 0; i < PCScreenNavvers.Count; i++)
        {
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
