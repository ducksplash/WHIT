using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Steamworks;

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
    public TMP_InputField PasswordInput;

    public Transform ViewableArea;

    public Collider PCCollider;

    public InputActionReference ClosePCInput;
    public InputActionReference goBack;
    public InputActionReference rightClickExit;

    public Transform PCScreenTransform;
    public Transform PlayerTransform;
    private Quaternion defaultRotation;

    // -------------------- Steam Deck Keyboard --------------------
    private Callback<GamepadTextInputDismissed_t> _gamepadTextDismissed;
    private TMP_InputField _activeSteamInputField;

    // Adjust per field if you want different max lengths
    private const uint STEAM_TEXT_MAX = 64;

    void Start()
    {
        IncorrectPasswordText.gameObject.SetActive(false);

        EventManager.OnStopComputer += OnStopComputer;
        ClosePCInput.action.performed += InputStopComputer;
        goBack.action.performed += InputStopComputer;
        TerminalEventManager.OnPCGridClick += SelectMenuItem;

        PlayerTransform = GameMaster.Instance.Player.transform;
        defaultRotation = PCScreenTransform.rotation;

        // Register Steam callback (requires SteamAPI.RunCallbacks() somewhere, usually in SteamManager)
        if (SteamManager.Initialized) _gamepadTextDismissed = Callback<GamepadTextInputDismissed_t>.Create(OnGamepadTextInputDismissed);

        // Open Deck keyboard when this input field is selected (SteamOS only)
        if (PasswordInput != null)
            PasswordInput.onSelect.AddListener(_ => TryOpenSteamDeckKeyboardFor(PasswordInput, isPassword: true));
    }

    private bool IsSteamOS()
    {
        return GameMaster.Instance.DeviceType.selectedDeviceType == PlayerDeviceType.SteamOS;
    }

    private void TryOpenSteamDeckKeyboardFor(TMP_InputField field, bool isPassword)
    {
        if (field == null) return;
        if (!IsSteamOS()) return;
        if (!SteamManager.Initialized) return;

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

        if (!opened) _activeSteamInputField = null;
    }

    private void OnGamepadTextInputDismissed(GamepadTextInputDismissed_t cb)
    {
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
            _activeSteamInputField.text = "";
            _activeSteamInputField = null;
            return;
        }

        string result;
        if (SteamUtils.GetEnteredGamepadTextInput(out result, STEAM_TEXT_MAX))
        {
            _activeSteamInputField.text = result;

            int end = _activeSteamInputField.text.Length;
            _activeSteamInputField.caretPosition = end;
            _activeSteamInputField.selectionAnchorPosition = end;
            _activeSteamInputField.selectionFocusPosition = end;
            
            // we try and log them on now
            LogOn();
            
            
        }

        _activeSteamInputField = null;
    }


    public void OnStartComputer()
    {
        if (GameMaster.Instance.PHONEOUT) return;
        if (GameMaster.Instance.ONPC) return;

        FacePlayerOnY();

        //Debug.Log("StartPC");
        PCCollider.enabled = false;
        NearestChair.gameObject.SetActive(false);
        GameMaster.Instance.INMENU = true;
        GameMaster.Instance.FROZEN = true;
        GameMaster.Instance.ONPC = true;
        GameMaster.Instance.EventManager.StartComputer(ViewableArea);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        CrosshairCanvas.GetComponent<CanvasGroup>().alpha = 0.0f;

        rightClickExit.action.performed += InputStopComputer;

        ChangeScreen(IsLoggedIn ? ComputerScreen.Desktop : ComputerScreen.LockScreen);
    }

    public void OnStopComputer()
    {
        if (GameMaster.Instance.PHONEOUT) return;
        if (!GameMaster.Instance.ONPC) return;

        RotBack();

        rightClickExit.action.performed -= InputStopComputer;

        //Debug.Log("StopPC");
        PCCollider.enabled = true;
        NearestChair.gameObject.SetActive(true);
        GameMaster.Instance.INMENU = false;
        GameMaster.Instance.FROZEN = false;
        GameMaster.Instance.ONPC = false;
        IncorrectPasswordText.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        CrosshairCanvas.GetComponent<CanvasGroup>().alpha = 1.0f;
    }

    public void InputStopComputer(InputAction.CallbackContext callbackContext)
    {
        GameMaster.Instance.EventManager.StopComputer();
    }

    public void LogOn()
    {

        if (PasswordInput.text.ToString().Trim().Equals(GameMaster.Instance.NORASPCPASSWORD.ToString().Trim()))
        {
            //Debug.Log("in");
            ChangeScreen(ComputerScreen.Desktop);
            IsLoggedIn = true;
        }
        else
        {
            //Debug.Log("no");
            ChangeScreen(ComputerScreen.LockScreen);
            IncorrectPasswordText.gameObject.SetActive(true);
            IsLoggedIn = false;
        }
    }

    public void SelectMenuItem(PCGriddle pcGriddle)
    {
        //Debug.Log("GO TO " + pcGriddle);

        switch (pcGriddle)
        {
            case PCGriddle.None:
                break;

            case PCGriddle.LogOn:
                LogOn();
                break;

            case PCGriddle.Files:
                ChangeScreen(ComputerScreen.Files);
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
                SelectedScreen.blocksRaycasts = true;

                PCScreen thisScreen = AllProgrammeScreens[i].GetComponent<PCScreen>();
                if (thisScreen.PCNavver != null) thisScreen.PCNavver.enabled = true;
            }
        }
    }

    public void CloseAllScreens()
    {
        for (var i = 0; i < AllProgrammeScreens.Count; i++)
        {
            CanvasGroup ThisScreen = AllProgrammeScreens[i].GetComponent<CanvasGroup>();

            ThisScreen.alpha = 0;
            ThisScreen.interactable = false;
            ThisScreen.blocksRaycasts = false;

            PCScreen thisScreen = AllProgrammeScreens[i].GetComponent<PCScreen>();
            if (thisScreen.PCNavver != null) thisScreen.PCNavver.enabled = false;

            //Debug.Log("Closed: " + AllProgrammeScreens[i].gameObject.name);
        }
    }

    public void FacePlayerOnY()
    {
        if (PCScreenTransform == null || PlayerTransform == null) return;

        Vector3 toPlayer = PlayerTransform.position - PCScreenTransform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        // Yaw-only target (world space)
        float targetYaw = Quaternion.LookRotation(toPlayer, Vector3.up).eulerAngles.y;

        // Keep current X/Z exactly, replace only Y
        Vector3 e = PCScreenTransform.rotation.eulerAngles;
        PCScreenTransform.rotation = Quaternion.Euler(e.x, targetYaw, e.z);
    }

    public void RotBack()
    {
        if (PCScreenTransform == null) return;
        PCScreenTransform.rotation = defaultRotation;
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
    Files = 10002,
    Phone = 10003,
    Emails = 10004,
    Web = 10005,
    Hacking = 10006,
    Games = 10007,
    Settings = 10008
}
