using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class Phone : MonoBehaviour
{
    public GameObject MobilePhone;
    public Animator PlayerAnim;
    public GameObject Clock;
    public GameObject Camera;
    public GameObject PhoneCamera;
    public GameObject HomeScreen;
    public OSDNavver HomeScreenNavver;
    public GameObject ContactsScreen;
    public OSDNavver ContactsScreenNavver;
    public GameObject CameraScreen;
    public GameObject DiallerScreen;
    public OSDNavver DiallerScreenNavver;
    public GameObject GalleryScreen;
    public GameObject CallingScreen;
    public OSDNavver CallingScreenNavver;
    public GameObject MapsScreen;
    public OSDNavver MapsScreenNavver;
    public GameObject MessagesScreen;
    public OSDNavver MessagesScreenNavver;
    public GameObject KelliScreen;
    public OSDNavver KelliScreenNavver;
    public OSDNavver NotesScreenNavver;
    public GameObject InboxPane;
    public GameObject SentPane;
    public GameObject BigMessageScreen;
    public GameObject theDialler;
    public GameObject Messagelist;
    public GameObject Sentlist;
    public GameObject MessageBlockPrefab;
    public GameObject NoteBlockPrefab;
    public Image CameraReadyFrame;
    public TextMeshProUGUI CameraReadyText;
    public TextMeshProUGUI CameraSavedText;
    public TextMeshProUGUI CameraReadyTextBG;
    public TextMeshProUGUI CameraSavedTextBG;
    public bool CameraReady;
    public TextMeshProUGUI DialBar;
    public TextMeshProUGUI CallText;
    public TextMeshProUGUI CallTitleText;
    public TextMeshProUGUI CallScreenCallingText;
    public CanvasGroup CrosshairCanvas;
    private bool callingContact = false;
    public TextMeshProUGUI anonNum;
    public TextMeshProUGUI kieronNum;
    public TextMeshProUGUI maryNum;
    public TextMeshProUGUI tomNum;
    public TextMeshProUGUI workNum;
    public TextMeshProUGUI darraghNum;
    public bool CameraOpen;
    public Light CameraFlash;
    public Light TorchLight;
    public int resWidth = 600;
    public int resHeight = 1000;
    public Camera getCamera;
    private bool BigMessageOpen;
    private bool BigMessageCanOpen;
    public GameObject MiniMapCam;
    public bool gotfiles;
    public int currentpage;
    public int nextpage;
    public int lastpage;
    public RawImage galleryEvidencePhoto;
    public GameObject GalleryBigPhoto;
    public TextMeshProUGUI galleryEvidenceName;
    public TextMeshProUGUI galleryEvidenceDate;
    public TextMeshProUGUI galleryEvidenceDetails;
    public Button galleryBack;
    public Button galleryNext;
    public int PhotosInGallery;
    private GameObject ObservedEvidence;
    public InputActionReference TogglePhoneInput;
    public InputActionReference InteractAction;
    public InputActionReference TriggerAction;
    public InputActionReference StepBackInput;
    public InputActionReference StepBackInputRightClick;
    public InputActionReference escapeExit;
    public InputActionReference listBackButton;
    public InputActionReference listNextButton;
    public InputActionReference numPad0;
    public InputActionReference numPad1;
    public InputActionReference numPad2;
    public InputActionReference numPad3;
    public InputActionReference numPad4;
    public InputActionReference numPad5;
    public InputActionReference numPad6;
    public InputActionReference numPad7;
    public InputActionReference numPad8;
    public InputActionReference numPad9;
    public InputActionReference numPadStar;
    public InputActionReference numPadHash;
    public InputActionReference numBackSpace;
    public InputActionReference toggleCameraSelfie;
    private GameObject currentScreen;
    public bool viewingPhoto;
    public DialogueName phoneTutorialFirstPhoto = DialogueName.phoneTutorialFirstPhoto;
    private Coroutine CallCoroutine;
    private Coroutine InputLockCoroutine;
    public List<GalleryItem> galleryItems = new List<GalleryItem>();
    public bool galleryLoaded = false;
    public CanvasGroup phoneCanvasGroup;
    public MeshRenderer phoneMeshRenderer;
    private CapsuleCollider phoneCollider;
    public Camera rearCameraComponent;
    public Camera selfieCameraComponent;
    public Camera phoneCameraComponent;
    private PhoneZoom phoneZoom;
    private TextMeshProUGUI clockText;
    private CanvasGroup galleryBigPhotoCanvas;
    private RawImage galleryBigPhotoRaw;
    private CanvasGroup homeScreenGroup;
    private CanvasGroup contactsScreenGroup;
    private CanvasGroup diallerScreenGroup;
    private CanvasGroup callingScreenGroup;
    private CanvasGroup mapsScreenGroup;
    private CanvasGroup messagesScreenGroup;
    private CanvasGroup kelliScreenGroup;
    private CanvasGroup cameraScreenGroup;
    private CanvasGroup galleryScreenGroup;
    private CanvasGroup inboxPaneGroup;
    private CanvasGroup sentPaneGroup;
    private CanvasGroup bigMessageScreenGroup;
    private CanvasGroup cameraReadyTextGroup;
    private CanvasGroup cameraReadyTextBgGroup;
    private CanvasGroup cameraSavedTextGroup;
    private CanvasGroup cameraSavedTextBgGroup;
    private Texture2D galleryTexture;
    private int lastClockMinute = -1;
    private int lastClockHour = -1;

    private bool PhoneOpened;
    public bool TakingSelfie;
    [SerializeField] private float photoInputLockDuration = 0.15f;
    private bool _photoInputLocked;
    
    public CapturingSelfieWithFriends CapturingSelfieWithFriends;

    
    private void Awake()
    {
        phoneCollider = GetComponent<CapsuleCollider>();
        clockText = Clock != null ? Clock.GetComponent<TextMeshProUGUI>() : null;
        
        if (PhoneCamera != null)
        {
            phoneCameraComponent = rearCameraComponent;
            phoneZoom = PhoneCamera.GetComponent<PhoneZoom>();
        }

        if (GalleryBigPhoto != null)
        {
            galleryBigPhotoCanvas = GalleryBigPhoto.GetComponent<CanvasGroup>();
            galleryBigPhotoRaw = GalleryBigPhoto.GetComponent<RawImage>();
        }

        homeScreenGroup = HomeScreen != null ? HomeScreen.GetComponent<CanvasGroup>() : null;
        contactsScreenGroup = ContactsScreen != null ? ContactsScreen.GetComponent<CanvasGroup>() : null;
        diallerScreenGroup = DiallerScreen != null ? DiallerScreen.GetComponent<CanvasGroup>() : null;
        callingScreenGroup = CallingScreen != null ? CallingScreen.GetComponent<CanvasGroup>() : null;
        mapsScreenGroup = MapsScreen != null ? MapsScreen.GetComponent<CanvasGroup>() : null;
        messagesScreenGroup = MessagesScreen != null ? MessagesScreen.GetComponent<CanvasGroup>() : null;
        kelliScreenGroup = KelliScreen != null ? KelliScreen.GetComponent<CanvasGroup>() : null;
        cameraScreenGroup = CameraScreen != null ? CameraScreen.GetComponent<CanvasGroup>() : null;
        galleryScreenGroup = GalleryScreen != null ? GalleryScreen.GetComponent<CanvasGroup>() : null;
        inboxPaneGroup = InboxPane != null ? InboxPane.GetComponent<CanvasGroup>() : null;
        sentPaneGroup = SentPane != null ? SentPane.GetComponent<CanvasGroup>() : null;
        bigMessageScreenGroup = BigMessageScreen != null ? BigMessageScreen.GetComponent<CanvasGroup>() : null;
        cameraReadyTextGroup = CameraReadyText != null ? CameraReadyText.GetComponent<CanvasGroup>() : null;
        cameraReadyTextBgGroup = CameraReadyTextBG != null ? CameraReadyTextBG.GetComponent<CanvasGroup>() : null;
        cameraSavedTextGroup = CameraSavedText != null ? CameraSavedText.GetComponent<CanvasGroup>() : null;
        cameraSavedTextBgGroup = CameraSavedTextBG != null ? CameraSavedTextBG.GetComponent<CanvasGroup>() : null;
    }

    private void OnEnable()
    {
        StepBackInput.action.performed += ActionStepBack;
        TogglePhoneInput.action.performed += ActionTogglePhone;
        EventManager.OnStopPhone += EventStopPhone;

        listBackButton.action.performed += GalleryBackAction;
        listNextButton.action.performed += GalleryNextAction;
        toggleCameraSelfie.action.performed += ToggleSelfieCam;

        numPad0.action.performed += a0Button;
        numPad1.action.performed += a1Button;
        numPad2.action.performed += a2Button;
        numPad3.action.performed += a3Button;
        numPad4.action.performed += a4Button;
        numPad5.action.performed += a5Button;
        numPad6.action.performed += a6Button;
        numPad7.action.performed += a7Button;
        numPad8.action.performed += a8Button;
        numPad9.action.performed += a9Button;
        numPadStar.action.performed += asButton;
        numPadHash.action.performed += ahButton;
        numBackSpace.action.performed += backspaceButton;
    }

    private void OnDisable()
    {
        StepBackInput.action.performed -= ActionStepBack;
        TogglePhoneInput.action.performed -= ActionTogglePhone;

        listBackButton.action.performed -= GalleryBackAction;
        listNextButton.action.performed -= GalleryNextAction;

        toggleCameraSelfie.action.performed -= ToggleSelfieCam;
        numPad0.action.performed -= a0Button;
        numPad1.action.performed -= a1Button;
        numPad2.action.performed -= a2Button;
        numPad3.action.performed -= a3Button;
        numPad4.action.performed -= a4Button;
        numPad5.action.performed -= a5Button;
        numPad6.action.performed -= a6Button;
        numPad7.action.performed -= a7Button;
        numPad8.action.performed -= a8Button;
        numPad9.action.performed -= a9Button;
        numPadStar.action.performed -= asButton;
        numPadHash.action.performed -= ahButton;
        numBackSpace.action.performed -= backspaceButton;
    }

    void Start()
    {
        if (galleryBigPhotoCanvas != null)
        {
            galleryBigPhotoCanvas.alpha = 0f;
            galleryBigPhotoCanvas.blocksRaycasts = false;
        }

        if (phoneCollider != null) phoneCollider.enabled = false;

        if (phoneCameraComponent != null) phoneCameraComponent.enabled = false;
        MiniMapCam.SetActive(false);

        LoadGallery();
        
        phoneCanvasGroup.alpha = 0f;
        phoneMeshRenderer.enabled = false;
    }
    
    
    private IEnumerator PhotoInputLock()
    {
        _photoInputLocked = true;

        yield return new WaitForSeconds(photoInputLockDuration);

        _photoInputLocked = false;
    }
    
    private void ActionStepBack(InputAction.CallbackContext callbackContext)
    {
        if (!callbackContext.performed) return;

        // If phone is not open, StepBack should open it EXACTLY like toggle.
        if (!PhoneOpened)
        {
            ActionTogglePhone(callbackContext);
            return;
        }

        // If phone IS open: step back through UI
        if (homeScreenGroup != null && homeScreenGroup.alpha < 0.9f)
        {
            changeScreen(HomeScreen);
        }
        else
        {
            PutAwayPhone();
        }
    }
    
    void changeScreen(GameObject useThisScreen)
    {
        if (currentScreen == useThisScreen) return;

        DisableNavvers();

        BigMessageCanOpen = false;
        BigMessageOpen = false;

        Transform[] allScreens = MobilePhone.GetComponentsInChildren<Transform>(true);
        Transform[] useTheseScreens = useThisScreen.GetComponentsInChildren<Transform>(true);

        if (phoneCameraComponent != null) phoneCameraComponent.enabled = false;

        foreach (Transform screen in allScreens)
        {
            if (screen.IsChildOf(useThisScreen.transform) && !screen.name.Contains("SCREENPANEL"))
                continue;

            if (!screen.name.Contains("SCREENPANEL") && !screen.name.Contains("BIGOPENMSG"))
            {
                CanvasGroup cg = screen.GetComponent<CanvasGroup>();
                if (cg)
                {
                    cg.alpha = 0f;
                    cg.blocksRaycasts = false;
                    cg.interactable = false;
                }

                var tmp = screen.GetComponent<TextMeshProUGUI>();
                if (tmp && !screen.name.Contains("clock") && !screen.name.Contains("network"))
                    tmp.enabled = false;
            }
        }

        foreach (Transform screen in useTheseScreens)
        {
            CanvasGroup cg = screen.GetComponent<CanvasGroup>();
            if (cg)
            {
                cg.alpha = 1f;
                cg.blocksRaycasts = true;
                cg.interactable = true;
            }

            var tmp = screen.GetComponent<TextMeshProUGUI>();
            if (tmp)
                tmp.enabled = true;
        }

        if (homeScreenGroup != null && homeScreenGroup.alpha > 0.9f)
        {
            HomeScreenNavver.gameObject.SetActive(true);
        }

        if (bigMessageScreenGroup != null && bigMessageScreenGroup.alpha > 0.9f)
        {
            bigMessageScreenGroup.alpha = 0;
        }

        if (contactsScreenGroup != null && contactsScreenGroup.alpha > 0.9f)
        {
            ContactsScreenNavver.gameObject.SetActive(true);
        }

        if (diallerScreenGroup != null && diallerScreenGroup.alpha > 0.9f)
        {
            DiallerScreenNavver.gameObject.SetActive(true);
        }

        if (callingScreenGroup != null && callingScreenGroup.alpha > 0.9f)
        {
            CallingScreenNavver.gameObject.SetActive(true);
        }

        if (galleryScreenGroup != null && galleryScreenGroup.alpha < 0.9f)
        {
            InteractAction.action.performed -= GalleryViewPhoto;
        }

        if (mapsScreenGroup != null && mapsScreenGroup.alpha > 0.9f)
        {

            MapsScreenNavver.gameObject.SetActive(true);
        }

        if (sentPaneGroup != null && sentPaneGroup.alpha < 0.9f)
        {

            NotesScreenNavver.gameObject.SetActive(false);
        }

        if (inboxPaneGroup != null && inboxPaneGroup.alpha < 0.9f)
        {

            NotesScreenNavver.gameObject.SetActive(false);
        }

        if (cameraScreenGroup != null && cameraScreenGroup.alpha < 0.9f)
        {
            CameraFlash.enabled = false;
        }

        if (useThisScreen != GalleryScreen)
        {
            InteractAction.action.performed -= GalleryViewPhoto;
            viewingPhoto = false;
            SetBigPhotoVisible(false);
        }

        if (CallCoroutine != null)
        {
            StopCoroutine(CallCoroutine);
            CallCoroutine = null;
        }

        if (useThisScreen != CameraScreen)
        {
            CameraOpen = false;
            CameraReady = false;
            CameraSavedText.text = "";
            CameraSavedTextBG.text = "";                            
            CameraReadyText.text = "";
            CameraReadyTextBG.text = "";
            
            Player.Instance.MoveOverride = false;
            
            TriggerAction.action.performed -= TakePhoto;
            InteractAction.action.performed -= TakePhoto;

            SelfieCamOff();
            
            EventManager.CameraClosed();
        }

        if (useThisScreen != KelliScreen)
        {
            EventManager.KelliStopped();
            KelliScreen.GetComponent<KelliLinux>().ResetKelli();
            KelliScreenNavver.gameObject.SetActive(false);
        }
        
        if (useThisScreen != MapsScreen)
        {
            MiniMapCam.SetActive(false);
        }

        currentScreen = useThisScreen;
        
        if (PhoneOpened)
        {
            if (useThisScreen == CameraScreen) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; } else { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        }

        
        Debug.Log("currentScreen " + currentScreen);
    }

    private void DisableNavvers()
    {
        HomeScreenNavver.gameObject.SetActive(false);
        DiallerScreenNavver.gameObject.SetActive(false);
        ContactsScreenNavver.gameObject.SetActive(false);
        CallingScreenNavver.gameObject.SetActive(false);
        MessagesScreenNavver.gameObject.SetActive(false);
        KelliScreenNavver.gameObject.SetActive(false);
        NotesScreenNavver.gameObject.SetActive(false);
        MapsScreenNavver.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (!PhoneOpened) return; 
        
        if (clockText == null) return;

        DateTime nowDateTime = DateTime.Now;
        if (nowDateTime.Minute != lastClockMinute || nowDateTime.Hour != lastClockHour)
        {
            lastClockMinute = nowDateTime.Minute;
            lastClockHour = nowDateTime.Hour;
            clockText.text = nowDateTime.ToString("HH:mm");
        }
    }

    private void EventStopPhone()
    {
        PutAwayPhone();
    }
    
    private void ActionTogglePhone(InputAction.CallbackContext callbackContext)
    {
        currentpage = 0;

        if (!PhoneOpened)
        {
            TakeOutPhone();
        }
        else
        {
            PutAwayPhone();
        }
    }

    private void TakeOutPhone()
    {
        if (PhoneOpened) return;
        if (GameMaster.Instance.PLAYERBUSY) return;
        if (GameMaster.Instance.PauseManager.IsPaused) return;

        GameMaster.Instance.PLAYERBUSY = true;
        PhoneOpened = true;
        
        Player.Instance.TogglePhone(false);
        
        // Ensure phone UI starts at home
        changeScreen(HomeScreen);
        HomeScreenNavver.gameObject.SetActive(true);

        EventManager.PhoneOpened();
        GameMaster.Instance.OnboardingManager.OpenedPhone();
        
        
        StepBackInputRightClick.action.performed += ActionStepBack;
        escapeExit.action.performed += ActionStepBack;

        
        
        phoneCanvasGroup.alpha = 1f;
        phoneMeshRenderer.enabled = true;
        

        if (phoneCollider != null) phoneCollider.enabled = true;

        // ✅ IMPORTANT: always set UI cursor mode when phone opens
        CrosshairCanvas.GetComponent<CanvasGroup>().alpha = 0.0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(StopAnimating());
    }


    private IEnumerator StopAnimating()
    {
        Animator anim = GameMaster.Instance.Player.Noranimator;

        // Zero the blend parameters so the locomotion tree drives toward idle.
        // anim.speed stays at 1 so the blend actually plays out.
        anim.SetFloat("Speed", 0f);
        anim.SetFloat("MoveX", 0f);
        anim.SetFloat("MoveY", 0f);

        // Wait long enough for the damp blend to settle into the idle pose.
        // animDampTime is 0.12s, so 0.25s is comfortably past it.
        yield return new WaitForSeconds(0.25f);

        // Pose is now stable — freeze and show the phone.
        anim.speed = 0f;
        EventManager.StartPhone(transform);
        FacePlayerOnY();
    }
    
    
    public void FacePlayerOnY()
    {
        Vector3 toPlayer = Player.Instance.transform.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        float targetYaw = Quaternion.LookRotation(toPlayer, Vector3.up).eulerAngles.y;

        transform.rotation = Quaternion.Euler(0, targetYaw, 0);
    }
    
    
    public void PutAwayPhone()
    {
        if (!PhoneOpened) return;
        
        if (_photoInputLocked) return;

        changeScreen(HomeScreen);

        // clear busy FIRST so Player/Animator isn't gated this frame
        GameMaster.Instance.PLAYERBUSY = false;
        PhoneOpened = false;

        Player.Instance.TogglePhone(true);

        StepBackInputRightClick.action.performed -= ActionStepBack;
        escapeExit.action.performed -= ActionStepBack;

        
        GameMaster.Instance.Player.Noranimator.speed = 1;
        
        if (phoneCollider != null) phoneCollider.enabled = false;

        
        phoneCanvasGroup.alpha = 0f;
        phoneMeshRenderer.enabled = false;

        if (phoneZoom != null) phoneZoom.DefaultFOV();

        if (!TorchLight.enabled)
        {
            Torch.torchToggle = true;
            TorchLight.enabled = true;
        }

        if (CallCoroutine != null)
        {
            StopCoroutine(CallCoroutine);
            CallCoroutine = null;
        }

        CrosshairCanvas.GetComponent<CanvasGroup>().alpha = 0.9f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        EventManager.StopPhone();
    }


    public void SelectPhoneGridItem(PhonerGriddle SelectedGridSquare, DialogueName selectedDialogue = DialogueName.None)
    {
        if (!PhoneOpened) return;

        if (selectedDialogue != DialogueName.None)
        {
            Debug.Log("selectedDialogue:begin:");
            Debug.Log(selectedDialogue);
            Debug.Log("selectedDialogue:end:");
        }

        Dialogue retrievedDialogue = null;

        if (selectedDialogue != DialogueName.None)
        {
            retrievedDialogue = GameMaster.Instance.DialogueManager.Dialogues.FirstOrDefault(d => d.DialogueName == selectedDialogue);
        }

        if (selectedDialogue != DialogueName.None)
        {
            Debug.Log("msg select::");
            Debug.Log(retrievedDialogue.DialogueName);
            Debug.Log(retrievedDialogue.DialogueText);
            Debug.Log("msg select::");
        }

        switch (SelectedGridSquare)
        {

            case PhonerGriddle.Telephone: ContactsButton(); break;
            case PhonerGriddle.Camera: CameraButton(); break;
            case PhonerGriddle.Map: MapsButton(); break;
            case PhonerGriddle.Messages: MessagesButton(); break;
            case PhonerGriddle.Notes: MessagesButton(101); break;
            case PhonerGriddle.Gallery: GalleryButton(); break;

            case PhonerGriddle.ViewInboxMessage:
                {
                    if (!BigMessageCanOpen)
                        return;

                    if (!BigMessageOpen)
                    {
                        BigMessageOpen = true;
                        MessagesScreenNavver.gameObject.SetActive(false);
                        string dialogueText = GameMaster.Instance.DialogueManager.GetReplacedString(retrievedDialogue.DialogueText
                        );
                        BigMessage(retrievedDialogue.Contact.ToString(), dialogueText);

                        InteractAction.action.performed += CloseBigMessage;
                    }
                }
                break;

            case PhonerGriddle.ViewSentMessage:

            {
                if (!BigMessageCanOpen)
                    return;

                if (!BigMessageOpen)
                {
                    BigMessageOpen = true;
                    NotesScreenNavver.gameObject.SetActive(false);
                    InteractAction.action.performed += CloseBigMessage;

                    string dialogueText = GameMaster.Instance.DialogueManager.GetReplacedString(
                        retrievedDialogue.DialogueText
                    );
                    BigMessage(
                        retrievedDialogue.Contact.ToString(),
                        dialogueText,
                        PhoneMessageType.SentItems
                    );
                }
            }
                break;

            case PhonerGriddle.Kelli: KelliButton(); break;


            case PhonerGriddle.Num0: a0Button(); break;
            case PhonerGriddle.Num1: a1Button(); break;
            case PhonerGriddle.Num2: a2Button(); break;
            case PhonerGriddle.Num3: a3Button(); break;
            case PhonerGriddle.Num4: a4Button(); break;
            case PhonerGriddle.Num5: a5Button(); break;
            case PhonerGriddle.Num6: a6Button(); break;
            case PhonerGriddle.Num7: a7Button(); break;
            case PhonerGriddle.Num8: a8Button(); break;
            case PhonerGriddle.Num9: a9Button(); break;
            case PhonerGriddle.NumStar: asButton(); break;
            case PhonerGriddle.NumHash: ahButton(); break;
            case PhonerGriddle.NumBack: backspaceButton(); break;
            case PhonerGriddle.NumCall: CallNumber(); break;

            case PhonerGriddle.Dialpad: DiallerButton(); break;
            case PhonerGriddle.Kieron: CallContact(PhonerGriddle.Kieron); break;
            case PhonerGriddle.Darragh: CallContact(PhonerGriddle.Darragh); break;
            case PhonerGriddle.Mary: CallContact(PhonerGriddle.Mary); break;
            case PhonerGriddle.Tom: CallContact(PhonerGriddle.Tom); break;
            case PhonerGriddle.Work: CallContact(PhonerGriddle.Work); break;
            case PhonerGriddle.Unknown: CallContact(PhonerGriddle.Unknown); break;
        }
    }

    public void ContactsButton()
    {
        changeScreen(ContactsScreen);

        Debug.Log("contacts button");
    }

    public void MapsButton()
    {
        changeScreen(MapsScreen);
        MiniMapCam.SetActive(true);

        Debug.Log("maps button");
    }
    public void KelliButton()
    {
        changeScreen(KelliScreen);

        EventManager.KelliStarted();
        
        Debug.Log("kelli button");
    }

    public void MessagesButton(int messageType = 100)
    {
        PhoneMessageType msgtype = (PhoneMessageType)messageType;

        for (int i = 0; i < Messagelist.transform.childCount; i++)
        {
            Destroy(Messagelist.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < Sentlist.transform.childCount; i++)
        {
            Destroy(Sentlist.transform.GetChild(i).gameObject);
        }

        changeScreen(MessagesScreen);

        if (msgtype == PhoneMessageType.Inbox)
        {
            GetMessages();
        }

        if (msgtype == PhoneMessageType.SentItems)
        {
            SentItems();
        }

        Debug.Log("Messages button");
    }

    public void BigMessage(
        string Sender,
        string Message,
        PhoneMessageType msgType = PhoneMessageType.Inbox
    )
    {
        Debug.Log("bigmessage called");
        BigMessageScreen.GetComponent<CanvasGroup>().alpha = 1;
        BigMessageScreen.GetComponent<CanvasGroup>().blocksRaycasts = true;

        if (msgType == PhoneMessageType.Inbox)
        {
            BigMessageScreen.transform.Find("fromPREFIX").GetComponent<TextMeshProUGUI>().text =
                "From:";
            BigMessageScreen.transform.Find("fromFROM").GetComponent<TextMeshProUGUI>().text =
                Sender;
        }

        if (msgType == PhoneMessageType.SentItems)
        {
            BigMessageScreen.transform.Find("fromPREFIX").GetComponent<TextMeshProUGUI>().text = "";
            BigMessageScreen.transform.Find("fromFROM").GetComponent<TextMeshProUGUI>().text = "";
        }

        BigMessageScreen.transform.Find("allofMSG").GetComponent<TextMeshProUGUI>().text = Message;

        Messagelist.transform.parent.parent.GetComponent<CanvasGroup>().alpha = 0f;
        Messagelist.transform.parent.parent.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnCloseBigMessage()
    {
        CloseBigMessage();
    }

    public void CloseBigMessage(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        Debug.Log("CloseBigMessage called");

        BigMessageScreen.GetComponent<CanvasGroup>().alpha = 0;
        BigMessageScreen.GetComponent<CanvasGroup>().blocksRaycasts = false;

        BigMessageScreen.transform.Find("fromFROM").GetComponent<TextMeshProUGUI>().text = "";
        BigMessageScreen.transform.Find("allofMSG").GetComponent<TextMeshProUGUI>().text = "";

        Messagelist.transform.parent.parent.GetComponent<CanvasGroup>().alpha = 1f;
        Messagelist.transform.parent.parent.GetComponent<CanvasGroup>().blocksRaycasts = true;

        if (InboxPane.GetComponent<CanvasGroup>().alpha > 0.9f)
        {
            MessagesScreenNavver.gameObject.SetActive(true);
            Debug.Log("Reactivated MessagesScreenNavver");
        }
        else if (SentPane.GetComponent<CanvasGroup>().alpha > 0.9f)
        {
            NotesScreenNavver.gameObject.SetActive(true);
            Debug.Log("Reactivated NotesScreenNavver");
        }

        BigMessageOpen = false;
        InteractAction.action.performed -= CloseBigMessage;
    }

    public void GetMessages()
    {
        // Defensive null checks (helps you find bad wiring in the Inspector too)
        if (InboxPane == null || SentPane == null || Messagelist == null || MessageBlockPrefab == null || MessagesScreenNavver == null)
        {
            Debug.LogError("Phone.GetMessages: Missing references (InboxPane/SentPane/Messagelist/MessageBlockPrefab/MessagesScreenNavver).");
            return;
        }

        InboxPane.GetComponent<CanvasGroup>().alpha = 1f;
        InboxPane.GetComponent<CanvasGroup>().blocksRaycasts = true;

        SentPane.GetComponent<CanvasGroup>().alpha = 0f;
        SentPane.GetComponent<CanvasGroup>().blocksRaycasts = false;

        MessagesScreenNavver.ResetList();
        DisableNavvers();

        var dm = GameMaster.Instance?.DialogueManager;
        if (dm == null || dm.DialogueSeen == null || dm.Dialogues == null)
        {
            Debug.LogError("Phone.GetMessages: DialogueManager / DialogueSeen / Dialogues is null.");
            return;
        }

        // DON'T reverse in place (every call flips order). Use a reverse iterator instead.
        for (int i = dm.DialogueSeen.Count - 1; i >= 0; i--)
        {
            DialogueName entry = dm.DialogueSeen[i];

            Dialogue selectedDialogue = dm.Dialogues.FirstOrDefault(d => d.DialogueName == entry);
            if (selectedDialogue == null)
            {
                Debug.LogWarning($"Phone.GetMessages: Dialogue '{entry}' was seen but not found in Dialogues list. Skipping.");
                continue;
            }

            if (selectedDialogue.Contact == Contacts.Nora)
                continue;

            GameObject messageBlock = Instantiate(MessageBlockPrefab, Messagelist.transform, false);

            OSDButton messageNavverButton = messageBlock.GetComponent<OSDButton>();
            if (messageNavverButton == null)
            {
                Debug.LogError("Phone.GetMessages: MessageBlockPrefab is missing OSDButton component.");
                Destroy(messageBlock);
                continue;
            }

            messageNavverButton.selectedDialogue = entry;
            MessagesScreenNavver.GridButtons.Add(messageNavverButton);

            var from = messageBlock.transform.Find("fromFROM")?.GetComponent<TextMeshProUGUI>();
            var bit = messageBlock.transform.Find("bitofMSG")?.GetComponent<TextMeshProUGUI>();

            if (from == null || bit == null)
            {
                Debug.LogError("Phone.GetMessages: Prefab is missing 'fromFROM' or 'bitofMSG' TextMeshProUGUI.");
                Destroy(messageBlock);
                continue;
            }

            from.text = selectedDialogue.Contact.ToString();

            string messageBit = selectedDialogue.DialogueText ?? "";
            if (messageBit.Length > 32) messageBit = messageBit.Substring(0, 32) + "...";
            bit.text = messageBit;
        }

        MessagesScreenNavver.gameObject.SetActive(true);
        StartCoroutine(EnableMessagesInteractNextFrame());
    }

    public void SentItems()
    {
        // Defensive reference checks
        if (SentPane == null || InboxPane == null || Sentlist == null || NoteBlockPrefab == null || NotesScreenNavver == null)
        {
            Debug.LogError("Phone.SentItems: Missing references (SentPane/InboxPane/Sentlist/NoteBlockPrefab/NotesScreenNavver).");
            return;
        }

        NotesScreenNavver.ResetList();
        DisableNavvers();

        // Toggle panes
        InboxPane.GetComponent<CanvasGroup>().alpha = 0f;
        InboxPane.GetComponent<CanvasGroup>().blocksRaycasts = false;

        SentPane.GetComponent<CanvasGroup>().alpha = 1f;
        SentPane.GetComponent<CanvasGroup>().blocksRaycasts = true;

        var dm = GameMaster.Instance?.DialogueManager;
        if (dm == null || dm.DialogueSeen == null || dm.Dialogues == null)
        {
            Debug.LogError("Phone.SentItems: DialogueManager / DialogueSeen / Dialogues is null.");
            return;
        }

        // Iterate in reverse WITHOUT mutating the list
        for (int i = dm.DialogueSeen.Count - 1; i >= 0; i--)
        {
            DialogueName entry = dm.DialogueSeen[i];

            Dialogue selectedDialogue = dm.Dialogues.FirstOrDefault(d => d.DialogueName == entry);
            if (selectedDialogue == null)
            {
                Debug.LogWarning($"Phone.SentItems: Dialogue '{entry}' was seen but not found in Dialogues list. Skipping.");
                continue;
            }

            // Sent items are Nora in your logic
            if (selectedDialogue.Contact != Contacts.Nora)
                continue;

            // Instantiate under Sentlist
            GameObject messageBlock = Instantiate(NoteBlockPrefab, Sentlist.transform, false);

            OSDButton notesNavverButton = messageBlock.GetComponent<OSDButton>();
            if (notesNavverButton == null)
            {
                Debug.LogError("Phone.SentItems: NoteBlockPrefab is missing OSDButton component.");
                Destroy(messageBlock);
                continue;
            }

            notesNavverButton.selectedDialogue = entry;
            NotesScreenNavver.GridButtons.Add(notesNavverButton);

            // Grab TMP fields safely
            var from = messageBlock.transform.Find("fromFROM")?.GetComponent<TextMeshProUGUI>();
            var bit = messageBlock.transform.Find("bitofMSG")?.GetComponent<TextMeshProUGUI>();

            if (from == null || bit == null)
            {
                Debug.LogError("Phone.SentItems: Prefab is missing 'fromFROM' or 'bitofMSG' TextMeshProUGUI.");
                Destroy(messageBlock);
                continue;
            }

            from.text = selectedDialogue.Contact.ToString();

            string messageBit = selectedDialogue.DialogueText ?? "";
            if (messageBit.Length > 32) messageBit = messageBit.Substring(0, 32) + "...";
            bit.text = messageBit;
        }

        NotesScreenNavver.gameObject.SetActive(true);
        StartCoroutine(EnableMessagesInteractNextFrame());
    }

    public void CameraButton()
    {
        changeScreen(CameraScreen);

        
        if (TorchLight.enabled)
        {
            Torch.torchToggle = false;
            TorchLight.enabled = false;
        }
        
        EventManager.CameraOpen();

        phoneCameraComponent.enabled = true;
        
        CameraFlash.enabled = true;

        if (GameMaster.Instance.EvidenceManager.EvidenceFound.Count < 1)
        {
            GameMaster.Instance.DialogueManager.NewDialogue(phoneTutorialFirstPhoto, 5);
        }

        if (cameraReadyTextGroup != null) cameraReadyTextGroup.alpha = 0;
        if (cameraSavedTextGroup != null) cameraSavedTextGroup.alpha = 0;
        if (cameraReadyTextBgGroup != null) cameraReadyTextBgGroup.alpha = 0;
        if (cameraSavedTextBgGroup != null) cameraSavedTextBgGroup.alpha = 0;
        
        CameraOpen = true;
        Player.Instance.MoveOverride = true;

        DoInputLock();
        
        InteractAction.action.performed += TakePhoto;
        TriggerAction.action.performed += TakePhoto;
    }

    public void GalleryButton()
    {
        changeScreen(GalleryScreen);

        viewingPhoto = false;
        SetBigPhotoVisible(false);

        if (!galleryLoaded)
            LoadGallery();

        CanvasGroup[] panes = GalleryScreen.GetComponentsInChildren<CanvasGroup>(true);

        if (galleryLoaded)
        {

            foreach (CanvasGroup pane in panes)
            {
                if (pane.CompareTag("GALLERYPANE"))
                {
                    pane.alpha = 1f;
                    pane.blocksRaycasts = true;
                    pane.interactable = true;
                }
                else if (pane.CompareTag("EMPTYPANE"))
                {
                    pane.alpha = 0f;
                    pane.blocksRaycasts = false;
                    pane.interactable = false;
                }
            }

            currentpage = 0;
            DisplayGalleryPage(currentpage);
        }
        else
        {

            foreach (CanvasGroup pane in panes)
            {
                if (pane.CompareTag("EMPTYPANE"))
                {
                    pane.alpha = 1f;
                    pane.blocksRaycasts = true;
                }
                else if (pane.CompareTag("GALLERYPANE"))
                {
                    pane.alpha = 0f;
                    pane.blocksRaycasts = false;
                }
            }
        }

        StartCoroutine(EnableGalleryInteractNextFrame());
    }

    private IEnumerator EnableMessagesInteractNextFrame()
    {

        yield return null;
        BigMessageCanOpen = true;
    }

    private IEnumerator EnableGalleryInteractNextFrame()
    {
        yield return null;
        InteractAction.action.performed += GalleryViewPhoto;
    }


    public void EnlargePhoto()
    {
        viewingPhoto = !viewingPhoto;
        
        SetBigPhotoVisible(viewingPhoto);
    }

    private void SetBigPhotoVisible(bool visible)
    {
        if (galleryBigPhotoCanvas == null) return;
        galleryBigPhotoCanvas.alpha = visible ? 1f : 0f;
        galleryBigPhotoCanvas.blocksRaycasts = visible;
    }

    public void GalleryNextAction(InputAction.CallbackContext callbackContext)
    {
        if (galleryScreenGroup != null && galleryScreenGroup.alpha < 0.9f)
            return;

        if (currentpage < galleryItems.Count - 1)
        {
            currentpage++;
            DisplayGalleryPage(currentpage);
        }
    }

    public void GalleryBackAction(InputAction.CallbackContext callbackContext)
    {
        if (galleryScreenGroup != null && galleryScreenGroup.alpha < 0.9f)
            return;

        if (currentpage > 0)
        {
            currentpage--;
            DisplayGalleryPage(currentpage);
        }
    }

    public void GalleryBackNext(string direction)
    {
        if (galleryScreenGroup != null && galleryScreenGroup.alpha < 0.9f)
            return;

        if (direction.Equals("back"))
        {
            Debug.Log("This Page: " + currentpage + "\nThis Page -1 : " + (currentpage - 1));
            if (currentpage > 0)
            {
                DisplayGalleryPage(currentpage - 1);
                currentpage--;
            }
        }

        if (direction.Equals("next"))
        {
            if (currentpage < PhotosInGallery)
            {
                DisplayGalleryPage(currentpage + 1);
                currentpage++;
            }
        }
    }

    public void GalleryViewPhoto(InputAction.CallbackContext ctx)
    {
        if (galleryScreenGroup != null && galleryScreenGroup.alpha < 0.9f)
            return;

        viewingPhoto = !viewingPhoto;

        if (galleryBigPhotoCanvas != null)
        {
            galleryBigPhotoCanvas.alpha = viewingPhoto ? 1f : 0f;
            galleryBigPhotoCanvas.blocksRaycasts = viewingPhoto;
        }
    }

    private void DisplayGalleryPage(int page)
    {
        if (page < 0 || page >= galleryItems.Count)
            return;

        GalleryItem item = galleryItems[page];

        byte[] imageData = File.ReadAllBytes(item.photoFullPath);
        if (galleryTexture == null)
            galleryTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);
        galleryTexture.LoadImage(imageData);

        galleryEvidencePhoto.texture = galleryTexture;
        if (galleryBigPhotoRaw != null)
            galleryBigPhotoRaw.texture = galleryTexture;

        galleryEvidenceName.text = item.evidenceName;
        galleryEvidenceDate.text = item.date;
        galleryEvidenceDetails.text = item.details;

        galleryBack.interactable = page > 0;
        galleryNext.interactable = page < galleryItems.Count - 1;
    }

    public void DiallerButton()
    {
        Debug.Log("dialler button");
        DialBar.text = "";

        if (callingContact == true)
        {
            changeScreen(ContactsScreen);

            callingContact = false;
        }
        else
        {
            DisableNavvers();
            DiallerScreenNavver.gameObject.SetActive(true);
            changeScreen(DiallerScreen);
        }
    }

    public void a1Button(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (!PhoneOpened) return;
        DialBar.text += "1";
    }

    public void a2Button(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (!PhoneOpened) return;
        DialBar.text += "2";
    }

    public void a3Button(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (!PhoneOpened) return;
        DialBar.text += "3";
    }

    public void a4Button(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (!PhoneOpened) return;
        DialBar.text += "4";
    }

    public void a5Button(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (!PhoneOpened) return;
        DialBar.text += "5";
    }

    public void a6Button(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (!PhoneOpened) return;
        DialBar.text += "6";
    }

    public void a7Button(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (!PhoneOpened) return;
        DialBar.text += "7";
    }

    public void a8Button(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (!PhoneOpened) return;
        DialBar.text += "8";
    }

    public void a9Button(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (!PhoneOpened) return;
        DialBar.text += "9";
    }

    public void a0Button(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (!PhoneOpened) return;
        DialBar.text += "0";
    }

    public void asButton(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (!PhoneOpened) return;
        DialBar.text += "*";
    }

    public void ahButton(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (!PhoneOpened) return;
        DialBar.text += "#";
    }

    public void backspaceButton(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (DialBar.text.Length > 0)
        {
            String SubString = DialBar.text.Substring(0, DialBar.text.Length - 1);
            DialBar.text = SubString;
        }
    }

    public void CallNumber()
    {
        if (DialBar.text.Length < 1)
            return;

        changeScreen(CallingScreen);

        callingContact = true;

        CallScreenCallingText.text = "Calling\n";
        CallText.text = DialBar.text;

        if (CallCoroutine != null)
        {
            StopCoroutine(CallCoroutine);
            CallCoroutine = null;
        }

        CallCoroutine = StartCoroutine(CallingContact("Calling\n" + DialBar.text));
    }

    public void CallContact(PhonerGriddle contactName = PhonerGriddle.Kieron)
    {
        changeScreen(CallingScreen);

        callingContact = true;

        CallScreenCallingText.text = "Calling\n" + contactName;

        if (CallCoroutine != null)
        {
            StopCoroutine(CallCoroutine);
            CallCoroutine = null;
        }

        CallCoroutine = StartCoroutine(CallingContact("Calling\n" + contactName));
    }

    private IEnumerator CallingContact(string callingcontactSlug)
    {
        string baseText = callingcontactSlug;
        int maxDots = 3;

        float allowedTime = 5f;
        float elapsedTime = 0f;

        int dotCount = 0;
        float tick = 0.5f;

        while (elapsedTime < allowedTime)
        {
            CallScreenCallingText.text = baseText + "\n" + new string('.', dotCount);

            dotCount = (dotCount + 1) % (maxDots + 1);

            elapsedTime += tick;
            yield return new WaitForSeconds(tick);
        }

        CallScreenCallingText.text = "No Answer";

        yield return new WaitForSeconds(1.5f);

        changeScreen(ContactsScreen);
        callingContact = false;
    }

    public void BackButton()
    {
        Debug.Log("BAKK");

        if (CallCoroutine != null)
        {
            StopCoroutine(CallCoroutine);
            CallCoroutine = null;
        }

        if (HomeScreen.GetComponent<CanvasGroup>().alpha < 0.9f)
        {
            Debug.Log("changeScreen");

            changeScreen(HomeScreen);
        }
        else
        {
            Debug.Log("PutAwayPhone");
            PutAwayPhone();
        }

        InteractAction.action.performed -= GalleryViewPhoto;
    }

    public void ConsolePhoneButtonAction(InputAction.CallbackContext callbackContext)
    {
        if (PhoneOpened)
        {
            if (homeScreenGroup != null && homeScreenGroup.alpha < 0.9f)
            {
                changeScreen(HomeScreen);
            }
            else
            {
                PutAwayPhone();
            }
        }
        else
        {
            TakeOutPhone();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (CameraOpen)
        {
            if (
                other.gameObject.layer == 11
                || other.gameObject.layer == 13
                || other.gameObject.layer == 14
            )
            {
                if (other.gameObject.GetComponent<Evidence>() != null)
                {
                    if (other.gameObject.GetComponent<Evidence>().PhotographableEvidence)
                    {
                        if (!other.gameObject.GetComponent<Evidence>().EvidenceCollected)
                        {
                            CameraReadyFrame.color = Color.green;
                            if (cameraReadyTextGroup != null) cameraReadyTextGroup.alpha = 1;
                            if (cameraReadyTextBgGroup != null) cameraReadyTextBgGroup.alpha = 1;

                            CameraSavedText.text = "";
                            CameraSavedTextBG.text = "";
        
                            CameraReadyText.text = GameMaster.Instance.DialogueManager.RetrieveOSDText(OSDTextName.TakePhoto);
                            CameraReadyTextBG.text = GameMaster.Instance.DialogueManager.RetrieveOSDText(OSDTextName.TakePhoto);
                            CameraReady = true;
                            ObservedEvidence = other.gameObject;
                        }
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (CameraOpen)
        {
            CameraReadyFrame.color = Color.black;
            if (cameraReadyTextGroup != null) cameraReadyTextGroup.alpha = 0;
            if (cameraReadyTextBgGroup != null) cameraReadyTextBgGroup.alpha = 0;
            CameraReady = false;
            ObservedEvidence = null;
        }
    }

    


    private void SelfieCamOff()
    {
        // force switch back when closing phone or camera
        
        TakingSelfie = false;
        
        phoneCameraComponent.enabled = false;
        phoneCameraComponent = rearCameraComponent;
        phoneCameraComponent.enabled = true;
    }


    private void ToggleSelfieCam(InputAction.CallbackContext callbackContext)
    {
        TakingSelfie = !TakingSelfie;

        if (TakingSelfie)
        {
            phoneCameraComponent.enabled = false;
            phoneCameraComponent = selfieCameraComponent;
            phoneCameraComponent.enabled = true;
        }
        else
        {
            phoneCameraComponent.enabled = false;
            phoneCameraComponent = rearCameraComponent;
            phoneCameraComponent.enabled = true;
        }
        
    }






    private void DoInputLock()
    {
        if (InputLockCoroutine != null)
        {
            StopCoroutine(InputLockCoroutine);
            InputLockCoroutine = null;
        }

        InputLockCoroutine = StartCoroutine(PhotoInputLock());
    }
    
    
    
    
    public void TakePhoto(InputAction.CallbackContext ctx)
    {
        if (!CameraOpen) return;

        if (_photoInputLocked) return;

        DoInputLock();
        
        bool isEvidence = false;

        var cam = phoneCameraComponent;
        if (cam == null) return;

        if (CameraReady && ObservedEvidence != null)
        {
            isEvidence = true;
            // Evidence in frame — record it as evidence (existing behaviour)
            var ev = ObservedEvidence.GetComponent<Evidence>();
            if (ev != null)
            {
                GameMaster.Instance.EvidenceManager.RecordEvidence(cam, ev);

                CameraReadyFrame.color = Color.black;
                if (cameraReadyTextGroup != null) cameraReadyTextGroup.alpha = 0;
                if (cameraReadyTextBgGroup != null) cameraReadyTextBgGroup.alpha = 0;
                CameraReady = false;
            }
        }
        else
        {
            // No evidence in frame — take a plain photo
            isEvidence = false;
            StartCoroutine(CaptureAndSavePhoto(cam));
        }

        StartCoroutine(SavedPhoto(isEvidence));
    }
    
    private IEnumerator CaptureAndSavePhoto(Camera cam)
    {
        yield return new WaitForEndOfFrame();

        Debug.Log($"Photo camera: {cam.name}");


        if (TakingSelfie)
        {
            GameObject[] boys = GameObject.FindGameObjectsWithTag("TheBoys");
            
            
            foreach (GameObject target in boys)
            {
                if (IsInCameraFrame(cam, target))
                {
                    EventManager.UnlockAchievement(SteamAchievements.Primadonna);
                }
            }
        }


        string photosDir = Path.Combine(Application.persistentDataPath, "Phone/0/Photos");
        Directory.CreateDirectory(photosDir);

        string fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        string fullPath = Path.Combine(photosDir, fileName);

        RenderTexture originalTarget = cam.targetTexture;

        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        cam.targetTexture = rt;
        cam.Render();

        RenderTexture.active = rt;

        Texture2D photo = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        photo.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        photo.Apply();

        cam.targetTexture = originalTarget;
        RenderTexture.active = null;

        Destroy(rt);

        byte[] bytes = photo.EncodeToPNG();
        File.WriteAllBytes(fullPath, bytes);

        Destroy(photo);
    }
    
    private bool IsInCameraFrame(Camera cam, GameObject obj)
    {
        Renderer rend = obj.GetComponentInChildren<Renderer>();
        if (rend == null) return false;

        // Sample multiple points on the bounds, not just center
        Bounds bounds = rend.bounds;
        Vector3[] samplePoints = new Vector3[]
        {
            bounds.center,
            bounds.center + Vector3.up   * bounds.extents.y * 0.8f,
            bounds.center + Vector3.down * bounds.extents.y * 0.8f,
            bounds.center + Vector3.left * bounds.extents.x * 0.8f,
            bounds.center + Vector3.right* bounds.extents.x * 0.8f,
        };

        // Exclude the player AND the target object itself from blocking the ray
        int mask = ~LayerMask.GetMask("player");

        foreach (Vector3 point in samplePoints)
        {
            Vector3 viewport = cam.WorldToViewportPoint(point);

            // Must be in front of and within the camera frame
            if (viewport.z <= 0 || viewport.x < 0.05f || viewport.x > 0.95f
                || viewport.y < 0.05f || viewport.y > 0.95f)
                continue;

            // Check line of sight from camera to this point
            Vector3 direction = point - cam.transform.position;
            float distance = direction.magnitude;

            bool blocked = false;

            if (Physics.Raycast(cam.transform.position, direction.normalized, out RaycastHit hit, distance - 0.1f, mask))
            {
                // Blocked if the hit object is NOT part of our target
                if (hit.transform != obj.transform && !hit.transform.IsChildOf(obj.transform))
                {
                    blocked = true;
                }
            }

            // At least one visible, unblocked point on the target is enough
            if (!blocked)
                return true;
        }

        return false;
    }
    
    
    
    private void LoadGallery()
    {
        galleryItems.Clear();

        string photosPath = Path.Combine(Application.persistentDataPath, "Phone/0/Photos");
        string evidencePath = Path.Combine(Application.persistentDataPath, "Phone/0/Evidence");
        string dcimPath = Path.Combine(Application.persistentDataPath, "Phone/0/DCIM");

        // Load plain photos — no metadata, placeholders for now
        if (Directory.Exists(photosPath))
        {
            FileInfo[] photoFiles = new DirectoryInfo(photosPath).GetFiles("*.png");

            foreach (FileInfo file in photoFiles)
            {
                galleryItems.Add(new GalleryItem
                {
                    evidenceName  = "empty",
                    photoFileName = "empty",
                    date          = "empty",
                    details       = "empty",
                    photoFullPath = file.FullName
                });
            }
        }

        // Load evidence photos — full metadata from .quack files
        if (Directory.Exists(evidencePath))
        {
            FileInfo[] files = new DirectoryInfo(evidencePath).GetFiles("*.quack");

            foreach (FileInfo file in files)
            {
                string[] lines = File.ReadAllLines(file.FullName);
                if (lines.Length < 6) continue;

                string photoPath = Path.Combine(dcimPath, lines[1].Trim());
                if (!File.Exists(photoPath)) continue;

                galleryItems.Add(new GalleryItem
                {
                    evidenceName  = lines[0],
                    photoFileName = lines[1],
                    date          = lines[2],
                    details       = lines[5],
                    photoFullPath = photoPath
                });
            }
        }

        PhotosInGallery = galleryItems.Count;
        galleryLoaded   = PhotosInGallery > 0;
    }

    IEnumerator SavedPhoto(bool isEvidence)
    {
        CameraReadyText.text = "";
        CameraReadyTextBG.text = "";
        
        string feedbackText = isEvidence ? GameMaster.Instance.DialogueManager.RetrieveOSDText(OSDTextName.SavedEvidence) : GameMaster.Instance.DialogueManager.RetrieveOSDText(OSDTextName.SavedPhoto);
        
        CameraSavedText.text = feedbackText;
        CameraSavedTextBG.text = feedbackText;
        
        
        
        
        

        if (cameraSavedTextGroup != null) cameraSavedTextGroup.alpha = 1;
        if (cameraSavedTextBgGroup != null)cameraSavedTextBgGroup.alpha = 1;

        yield return new WaitForSeconds(0.5f);
        LoadGallery();
        yield return new WaitForSeconds(3f);
        if (cameraSavedTextGroup != null) cameraSavedTextGroup.alpha = 0;
        if (cameraSavedTextBgGroup != null) cameraSavedTextBgGroup.alpha = 0;
    }
}

public enum PhonerGriddle
{

    Telephone = 1000,
    Messages = 1001,
    Email = 1002,
    Camera = 1003,
    Gallery = 1004,
    Notes = 1005,
    Backup = 1006,
    Settings = 1007,
    Map = 1008,
    Recorder = 1009,
    Help = 1010,
    Kelli = 1011,

    ViewInboxMessage = 1100,
    ViewSentMessage = 1101,

    Num0 = 0,
    Num1 = 1,
    Num2 = 2,
    Num3 = 3,
    Num4 = 4,
    Num5 = 5,
    Num6 = 6,
    Num7 = 7,
    Num8 = 8,
    Num9 = 9,
    NumStar = 10,
    NumHash = 11,
    NumCall = 12,
    NumBack = 13,

    Dialpad = 2100,
    Kieron = 2000,
    Darragh = 2001,
    Mary = 2002,
    Tom = 2003,
    Work = 2004,
    Unknown = 2005
}

[System.Serializable]
public enum PhoneMessageType
{
    Inbox = 100,
    SentItems = 101
}

[Serializable]
public class GalleryItem
{
    public string evidenceName;
    public string photoFileName;
    public string date;
    public string details;
    public string photoFullPath;
}
