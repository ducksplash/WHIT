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
    public Animator DeviceAnim;
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
    public GameObject MessagesScreen;
    public OSDNavver MessagesScreenNavver;
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
    public bool CameraReady;
    public TextMeshProUGUI DialBar;
    public TextMeshProUGUI CallText;
    public TextMeshProUGUI CallTitleText;
    public TextMeshProUGUI CallScreenCallingText;

    public CanvasGroup CrosshairCanvas;

    // contacts' nums
    private bool callingContact = false;
    public TextMeshProUGUI anonNum;
    public TextMeshProUGUI kieronNum;
    public TextMeshProUGUI maryNum;
    public TextMeshProUGUI tomNum;
    public TextMeshProUGUI workNum;

    public TextMeshProUGUI darraghNum;

    // Start is called before the first frame update
    public bool CameraOpen;
    public Light CameraLeftFlash;
    public Light CameraRightFlash;
    public Light TorchLight;
    public int resWidth = 600;
    public int resHeight = 1000;
    public Camera getCamera;
    public bool WaitingForPhone;


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

    public InputActionReference togglePhonePC;

    public InputActionReference InteractAction; // multi-button

    public InputActionReference goBack;

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
    private GameObject currentScreen;

    public bool viewingPhoto;

    public DialogueName phoneTutorialFirstPhoto = DialogueName.phoneTutorialFirstPhoto;

    private Coroutine CallCoroutine;
    
    public List<GalleryItem> galleryItems = new List<GalleryItem>();
    public bool galleryLoaded = false;
    
    // Make Call Coroutine


    private void OnEnable()
    {
        goBack.action.performed += ConsolePhoneButtonAction;
        togglePhonePC.action.performed += ActionTogglePhone;


        listBackButton.action.performed += GalleryBackAction;
        listNextButton.action.performed += GalleryNextAction;


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
        goBack.action.performed -= ConsolePhoneButtonAction;
        togglePhonePC.action.performed -= ActionTogglePhone;


        listBackButton.action.performed -= GalleryBackAction;
        listNextButton.action.performed -= GalleryNextAction;


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
        WaitingForPhone = true;

        DeviceAnim = MobilePhone.GetComponent<Animator>();

        //Debug.Log(Application.persistentDataPath);


        GalleryBigPhoto.GetComponent<CanvasGroup>().alpha = 0f;
        GalleryBigPhoto.GetComponent<CanvasGroup>().blocksRaycasts = false;

        // evidence camera collider
        gameObject.GetComponent<CapsuleCollider>().enabled = false;


        PhoneCamera.GetComponent<Camera>().enabled = false;
        MiniMapCam.SetActive(false);

        LoadGallery();

    }



    void changeScreen(GameObject useThisScreen)
    {
        DisableNavvers();
        
        Transform[] allScreens = MobilePhone.GetComponentsInChildren<Transform>(true);
        Transform[] useTheseScreens = useThisScreen.GetComponentsInChildren<Transform>(true);

        PhoneCamera.GetComponent<Camera>().enabled = false;

        // FIX: prevent Gallery panes from being reset
        foreach (Transform screen in allScreens)
        {
            if (screen.IsChildOf(useThisScreen.transform) && !screen.name.Contains("SCREENPANEL")) continue;

            if (!screen.name.Contains("SCREENPANEL"))
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
            if (tmp) tmp.enabled = true;
        }


        if (HomeScreen.GetComponent<CanvasGroup>()?.alpha > 0.9f)
        {
            HomeScreenNavver.gameObject.SetActive(true);
        }

        if (ContactsScreen.GetComponent<CanvasGroup>()?.alpha > 0.9f)
        {
            ContactsScreenNavver.gameObject.SetActive(true);
        }

        if (DiallerScreen.GetComponent<CanvasGroup>()?.alpha > 0.9f)
        {
            DiallerScreenNavver.gameObject.SetActive(true);
        }

        if (CallingScreen.GetComponent<CanvasGroup>()?.alpha > 0.9f)
        {
            CallingScreenNavver.gameObject.SetActive(true);
        }

        if (GalleryScreen.GetComponent<CanvasGroup>()?.alpha < 0.9f)
        {
            InteractAction.action.performed -= GalleryViewPhoto;
        }

        if (CameraScreen.GetComponent<CanvasGroup>()?.alpha < 0.9f)
        {
            CameraLeftFlash.enabled = false;
            CameraRightFlash.enabled = false;
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
        
        currentScreen = useThisScreen;
        Debug.Log("currentScreen "+currentScreen);
    }

    private void DisableNavvers()
    {
        HomeScreenNavver.gameObject.SetActive(false);
        DiallerScreenNavver.gameObject.SetActive(false);
        ContactsScreenNavver.gameObject.SetActive(false);
        CallingScreenNavver.gameObject.SetActive(false);
        MessagesScreenNavver.gameObject.SetActive(false);
        NotesScreenNavver.gameObject.SetActive(false);
    }
    
    
    
    

    // Update is called once per frame
    void Update()
    {
       //if (!GameMaster.Instance.OnboardingManager.PHONECOLLECTED) return;

        DateTime nowDateTime = DateTime.Now;
        string anHour = nowDateTime.Hour.ToString().PadLeft(2, '0');
        string aMinute = nowDateTime.Minute.ToString().PadLeft(2, '0');

        Clock.GetComponent<TMPro.TextMeshProUGUI>().text = anHour + ":" + aMinute;
    }


    private void ActionTogglePhone(InputAction.CallbackContext callbackContext)
    {
        currentpage = 0;

        if (!GameMaster.Instance.PHONEOUT)
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
        changeScreen(HomeScreen);
        HomeScreenNavver.gameObject.SetActive(true);
        
        GameMaster.Instance.EventManager.PhoneOpened();

        if (!GameMaster.Instance.INMENU && !GameMaster.Instance.HASITEM && !GameMaster.Instance.ISWRITING)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            GameMaster.Instance.PHONEOUT = true;
            GameMaster.Instance.INMENU = true;
            GameMaster.Instance.FROZEN = true;

            if (!GameMaster.Instance.OnboardingManager.PHONEACCESSED)
            {
                GameMaster.Instance.OnboardingManager.OpenedPhone();
            }

            MobilePhone.transform.localPosition = new Vector3(MobilePhone.transform.localPosition.x, MobilePhone.transform.localPosition.y + 1, MobilePhone.transform.localPosition.z);

            MobilePhone.GetComponentInChildren<CanvasGroup>().alpha = 1.0f;

            CrosshairCanvas.GetComponent<CanvasGroup>().alpha = 0.0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            gameObject.GetComponent<CapsuleCollider>().enabled = true;
        }
    }


    public void PutAwayPhone()
    {
        if (!GameMaster.Instance.PHONEOUT) return;
        changeScreen(HomeScreen);
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GameMaster.Instance.PHONEOUT = false;
        GameMaster.Instance.INMENU = false;
        GameMaster.Instance.FROZEN = false;

        gameObject.GetComponent<CapsuleCollider>().enabled = false;
        MobilePhone.transform.localPosition = new Vector3(MobilePhone.transform.localPosition.x, MobilePhone.transform.localPosition.y - 1, MobilePhone.transform.localPosition.z);

        CrosshairCanvas.GetComponent<CanvasGroup>().alpha = 0.9f;

        MobilePhone.GetComponentInChildren<CanvasGroup>().alpha = 0.0f;

        PhoneCamera.GetComponent<PhoneZoom>().DefaultFOV();

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
    }


    public void SelectPhoneGridItem(PhonerGriddle SelectedGridSquare, DialogueName selectedDialogue = DialogueName.None)
    {
        if (!GameMaster.Instance.PHONEOUT) return;


        if (selectedDialogue != DialogueName.None)
        {
            Debug.Log("selectedDialogue:begin:");
            Debug.Log(selectedDialogue);
            Debug.Log("selectedDialogue:end:");
        }
        
        
        Dialogue retrievedDialogue = null;
            
        if (selectedDialogue != DialogueName.None)
        {
            // We have an extra parameter to deal with, a message, let's find it and route it.
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
            // Main Menu
            case PhonerGriddle.Telephone: ContactsButton(); break;
            case PhonerGriddle.Camera: CameraButton(); break;
            case PhonerGriddle.Map: MapsButton(); break;
            case PhonerGriddle.Messages: MessagesButton(); break;
            case PhonerGriddle.Notes: MessagesButton(101); break;
            case PhonerGriddle.Gallery: GalleryButton(); break;
            
            // Sub Menus

            case PhonerGriddle.ViewInboxMessage:
            {
                if (BigMessageScreen.GetComponent<CanvasGroup>().alpha < 0.9f)
                {
                    BigMessage(retrievedDialogue.Contact.ToString(), retrievedDialogue.DialogueText); 
                }
                else
                {
                    CloseBigMessage();
                }
                
            } break;
            
            // Dialler
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
            
            // Contacts
            case PhonerGriddle.Dialpad: DiallerButton(); break;
            case PhonerGriddle.Kieron: CallContact(PhonerGriddle.Kieron); break;
            case PhonerGriddle.Darragh: CallContact(PhonerGriddle.Darragh); break;
            case PhonerGriddle.Mary: CallContact(PhonerGriddle.Mary); break;
            case PhonerGriddle.Tom: CallContact(PhonerGriddle.Tom); break;
            case PhonerGriddle.Work: CallContact(PhonerGriddle.Work); break;
            case PhonerGriddle.Unknown: CallContact(PhonerGriddle.Unknown); break;
        }
    }


    // Contacts menu incorporating CallScreen, Dialler.
    public void ContactsButton()
    {
        changeScreen(ContactsScreen);

        Debug.Log("contacts button");
    }


    // Contacts menu incorporating CallScreen, Dialler.
    public void MapsButton()
    {
        changeScreen(MapsScreen);
        MiniMapCam.SetActive(true);
        GameMaster.Instance.FROZEN = false;

        Debug.Log("maps button");
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

        if (msgtype == PhoneMessageType.SentItems)
        {
            SentItems();
        }

        if (msgtype == PhoneMessageType.Inbox)
        {
            GetMessages();
        }


        Debug.Log("Messages button");
    }


    public void BigMessage(string Sender, string Message, PhoneMessageType msgType = PhoneMessageType.Inbox)
    {
        
        
        BigMessageScreen.GetComponent<CanvasGroup>().alpha = 1;
        BigMessageScreen.GetComponent<CanvasGroup>().blocksRaycasts = true;

        if (msgType == PhoneMessageType.Inbox)
        {
            BigMessageScreen.transform.Find("fromPREFIX").GetComponent<TextMeshProUGUI>().text = "From:";
            BigMessageScreen.transform.Find("fromFROM").GetComponent<TextMeshProUGUI>().text = Sender;
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

    public void CloseBigMessage()
    {
        BigMessageScreen.GetComponent<CanvasGroup>().alpha = 0;
        BigMessageScreen.GetComponent<CanvasGroup>().blocksRaycasts = false;

        BigMessageScreen.transform.Find("fromFROM").GetComponent<TextMeshProUGUI>().text = "";
        BigMessageScreen.transform.Find("allofMSG").GetComponent<TextMeshProUGUI>().text = "";

        Messagelist.transform.parent.parent.GetComponent<CanvasGroup>().alpha = 1f;
        Messagelist.transform.parent.parent.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }


    public void GetMessages()
    {
        CloseBigMessage();
        
        InboxPane.GetComponent<CanvasGroup>().alpha = 1f;
        InboxPane.GetComponent<CanvasGroup>().blocksRaycasts = true;

        SentPane.GetComponent<CanvasGroup>().alpha = 0f;
        SentPane.GetComponent<CanvasGroup>().blocksRaycasts = false;

        NotesScreenNavver.ResetList();
        
        //var itty = 0;
        foreach (DialogueName entry in GameMaster.Instance.DialogueManager.DialogueSeen)
        {
            Dialogue selectedDialogue = GameMaster.Instance.DialogueManager.Dialogues.FirstOrDefault(d => d.DialogueName == entry);

            if (selectedDialogue.Contact != Contacts.Nora)
            {
                var buttonPosition = new Vector3(Messagelist.transform.localPosition.x, Messagelist.transform.localPosition.y, Messagelist.transform.localPosition.z);

                GameObject messageBlock = Instantiate(MessageBlockPrefab, buttonPosition, Quaternion.identity);
                messageBlock.transform.SetParent(Messagelist.transform, false);

                OSDButton messageNavverButton = messageBlock.GetComponent<OSDButton>();
                messageNavverButton.selectedDialogue = entry;
                
                // add navver things. 
                MessagesScreenNavver.GridButtons.Add(messageNavverButton);
                
                

                messageBlock.transform.Find("fromFROM").GetComponent<TextMeshProUGUI>().text = selectedDialogue.Contact.ToString();

                var messageBit = selectedDialogue.DialogueText;

                if (messageBit.Length > 32)
                {
                    messageBit = messageBit.Substring(0, 32) + "...";
                }


                messageBlock.transform.Find("bitofMSG").GetComponent<TextMeshProUGUI>().text = messageBit;

                messageBlock.GetComponent<Button>().onClick.AddListener(delegate { BigMessage(selectedDialogue.Contact.ToString(), selectedDialogue.DialogueText); });
            }
        }
        
        MessagesScreenNavver.gameObject.SetActive(true);
    }


    public void SentItems()
    {
        CloseBigMessage();
        
        InboxPane.GetComponent<CanvasGroup>().alpha = 0f;
        InboxPane.GetComponent<CanvasGroup>().blocksRaycasts = false;

        SentPane.GetComponent<CanvasGroup>().alpha = 1f;
        SentPane.GetComponent<CanvasGroup>().blocksRaycasts = true;


        NotesScreenNavver.ResetList();
        
        //var itty = 0;
        foreach (DialogueName entry in GameMaster.Instance.DialogueManager.DialogueSeen)
        {
            Dialogue selectedDialogue = GameMaster.Instance.DialogueManager.Dialogues.FirstOrDefault(d => d.DialogueName == entry);


            if (selectedDialogue.Contact == Contacts.Nora)
            {
                var buttonPosition = new Vector3(Sentlist.transform.localPosition.x, Sentlist.transform.localPosition.y, Sentlist.transform.localPosition.z);

                GameObject messageBlock = Instantiate(NoteBlockPrefab, buttonPosition, Quaternion.identity);
                messageBlock.transform.SetParent(Sentlist.transform, false);
                
                OSDButton notesNavverButton = messageBlock.GetComponent<OSDButton>();
                notesNavverButton.selectedDialogue = entry;
                
                // add navver things. 
                NotesScreenNavver.GridButtons.Add(notesNavverButton);
                
                messageBlock.transform.Find("fromFROM").GetComponent<TextMeshProUGUI>().text = selectedDialogue.Contact.ToString();

                var messageBit = selectedDialogue.DialogueText;

                if (messageBit.Length > 32)
                {
                    messageBit = messageBit.Substring(0, 32) + "...";
                }


                messageBlock.transform.Find("bitofMSG").GetComponent<TextMeshProUGUI>().text = messageBit;

                messageBlock.GetComponent<Button>().onClick.AddListener(delegate { BigMessage(selectedDialogue.Contact.ToString(), selectedDialogue.DialogueText, PhoneMessageType.SentItems); });
            }
        }
        
        NotesScreenNavver.gameObject.SetActive(true);
    }


    // Contacts menu incorporating CallScreen, Dialler.
    public void CameraButton()
    {
        changeScreen(CameraScreen);


        InteractAction.action.performed += TakePhoto;

        if (TorchLight.enabled)
        {
            Torch.torchToggle = false;
            TorchLight.enabled = false;
        }

        CameraLeftFlash.enabled = true;
        CameraRightFlash.enabled = true;

        GameMaster.Instance.FROZEN = false; // allow player move ToDo: inform player can move

        // todo: input action references
        // string camerakey = InputManager.GetKeyName("camera");

        if (GameMaster.Instance.EvidenceManager.EvidenceFound.Count < 1)
        {
            GameMaster.Instance.DialogueManager.NewDialogue(phoneTutorialFirstPhoto, 5);
        }

        CameraReadyText.GetComponent<CanvasGroup>().alpha = 0;
        CameraSavedText.GetComponent<CanvasGroup>().alpha = 0;
        PhoneCamera.GetComponent<Camera>().enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        CameraOpen = true;
    }

    // GALLERY


    public void GalleryButton()
    {
        changeScreen(GalleryScreen);

        // --- Reset gallery state ---
        viewingPhoto = false;
        SetBigPhotoVisible(false);

        if (!galleryLoaded) LoadGallery();

        CanvasGroup[] panes = GalleryScreen.GetComponentsInChildren<CanvasGroup>(true);

        if (galleryLoaded)
        {
            // Show gallery list panes
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
            // Show empty gallery pane
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

        // IMPORTANT: delay input binding so the same Interact press
        // that opened the gallery does NOT open the big photo
        StartCoroutine(EnableGalleryInteractNextFrame());
    }

    
    private IEnumerator EnableGalleryInteractNextFrame()
    {
        yield return null; 
        InteractAction.action.performed += GalleryViewPhoto;
    }

    private void SetBigPhotoVisible(bool visible)
    {
        var cg = GalleryBigPhoto.GetComponent<CanvasGroup>();
        cg.alpha = visible ? 1f : 0f;
        cg.blocksRaycasts = visible;
    }

    
    
    
    public void GalleryNextAction(InputAction.CallbackContext callbackContext)
    {
        if (GalleryScreen.GetComponent<CanvasGroup>().alpha < 0.9f) return;
        
        if (currentpage < galleryItems.Count - 1)
        {
            currentpage++;
            DisplayGalleryPage(currentpage);
        }
    }

    public void GalleryBackAction(InputAction.CallbackContext callbackContext) 
    {
        if (GalleryScreen.GetComponent<CanvasGroup>().alpha < 0.9f) return;
        
        if (currentpage > 0)
        {
            currentpage--;
            DisplayGalleryPage(currentpage);
        }
    }
    

    public void GalleryBackNext(string direction)
    {
        if (GalleryScreen.GetComponent<CanvasGroup>().alpha < 0.9f) return;
        
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
        if (GalleryScreen.GetComponent<CanvasGroup>().alpha < 0.9f) return;

        viewingPhoto = !viewingPhoto;

        GalleryBigPhoto.GetComponent<CanvasGroup>().alpha = viewingPhoto ? 1f : 0f;
        GalleryBigPhoto.GetComponent<CanvasGroup>().blocksRaycasts = viewingPhoto;
    }

    

    private void DisplayGalleryPage(int page)
    {
        if (page < 0 || page >= galleryItems.Count)
            return;
        
        GalleryItem item = galleryItems[page];

        byte[] imageData = File.ReadAllBytes(item.photoFullPath);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(imageData);

        galleryEvidencePhoto.texture = tex;
        GalleryBigPhoto.GetComponent<RawImage>().texture = tex;

        galleryEvidenceName.text = item.evidenceName;
        galleryEvidenceDate.text = item.date;
        galleryEvidenceDetails.text = item.details;

        galleryBack.interactable = page > 0;
        galleryNext.interactable = page < galleryItems.Count - 1;
    }

    // Dialler.
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
        if (!GameMaster.Instance.PHONEOUT) return;
        DialBar.text += "1";
    }

    public void a2Button(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (!GameMaster.Instance.PHONEOUT) return;
        DialBar.text += "2";
    }

    public void a3Button(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (!GameMaster.Instance.PHONEOUT) return;
        DialBar.text += "3";
    }

    public void a4Button(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (!GameMaster.Instance.PHONEOUT) return;
        DialBar.text += "4";
    }

    public void a5Button(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (!GameMaster.Instance.PHONEOUT) return;
        DialBar.text += "5";
    }

    public void a6Button(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (!GameMaster.Instance.PHONEOUT) return;
        DialBar.text += "6";
    }

    public void a7Button(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (!GameMaster.Instance.PHONEOUT) return;
        DialBar.text += "7";
    }

    public void a8Button(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (!GameMaster.Instance.PHONEOUT) return;
        DialBar.text += "8";
    }

    public void a9Button(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (!GameMaster.Instance.PHONEOUT) return;
        DialBar.text += "9";
    }

    public void a0Button(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (!GameMaster.Instance.PHONEOUT) return;
        DialBar.text += "0";
    }

    public void asButton(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (!GameMaster.Instance.PHONEOUT) return;
        DialBar.text += "*";
    }

    public void ahButton(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (!GameMaster.Instance.PHONEOUT) return;
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
        if (DialBar.text.Length < 1) return;
        
        changeScreen(CallingScreen);
        
        callingContact = true;
        
        CallScreenCallingText.text = "Calling\n";
        CallText.text = DialBar.text;

        if (CallCoroutine != null)
        {
            StopCoroutine(CallCoroutine);
            CallCoroutine = null;
        }
        
        CallCoroutine = StartCoroutine(CallingContact("Calling\n"+DialBar.text));
    }

    
    public void CallContact(PhonerGriddle contactName = PhonerGriddle.Kieron)
    {
        changeScreen(CallingScreen);
        
        callingContact = true;
        
        CallScreenCallingText.text = "Calling\n"+contactName;
        
        if (CallCoroutine != null)
        {
            StopCoroutine(CallCoroutine);
            CallCoroutine = null;
        }
        
        CallCoroutine = StartCoroutine(CallingContact("Calling\n"+contactName));
    }

    private IEnumerator CallingContact(string callingcontactSlug)
    {
        string baseText = callingcontactSlug;
        int maxDots = 3;

        float allowedTime = 5f;
        float elapsedTime = 0f;

        int dotCount = 0;
        float tick = 0.5f;

        // ringing noise
        
        while (elapsedTime < allowedTime)
        {
            CallScreenCallingText.text = baseText + "\n" + new string('.', dotCount);

            dotCount = (dotCount + 1) % (maxDots + 1);

            elapsedTime += tick;
            yield return new WaitForSeconds(tick);
        }

        CallScreenCallingText.text = "No Answer";
        // boop boop boop noise
        yield return new WaitForSeconds(1.5f);
        
        // boot back to contacts
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
        if (GameMaster.Instance.PHONEOUT)
        {
            Debug.Log(HomeScreen.GetComponent<CanvasGroup>().alpha);
            if (HomeScreen.GetComponent<CanvasGroup>().alpha < 0.9f)
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
            if (other.gameObject.layer == 11 || other.gameObject.layer == 13 || other.gameObject.layer == 14)
            {
                if (other.gameObject.GetComponent<Evidence>() != null)
                {
                    if (other.gameObject.GetComponent<Evidence>().PhotographableEvidence)
                    {
                        if (!other.gameObject.GetComponent<Evidence>().EvidenceCollected)
                        {
                            CameraReadyFrame.color = Color.green;
                            CameraReadyText.GetComponent<CanvasGroup>().alpha = 1;


                            // todo: input action references, dialogue
                            //string camerakey = InputManager.GetKeyName("camera");

                            //CameraReadyText.text = "press " + camerakey + " to photograph evidence";
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
            CameraReadyText.GetComponent<CanvasGroup>().alpha = 0;
            CameraReady = false;
            ObservedEvidence = null;
        }
    }

    public void TakePhoto(InputAction.CallbackContext ctx)
    {
        if (!CameraReady || ObservedEvidence == null)
            return;

        var ev = ObservedEvidence.GetComponent<Evidence>();
        if (ev == null)
            return;

        // --- Render camera to texture ---
        var cam = PhoneCamera.GetComponent<Camera>();
        RenderTexture active = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;

        cam.Render();

        Texture2D image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
        image.Apply();

        RenderTexture.active = active;

        byte[] bytes = image.EncodeToPNG();
        Destroy(image);


        string dcimDir = Path.Combine(Application.persistentDataPath, "Phone/0/DCIM");
        string evidenceDir = Path.Combine(Application.persistentDataPath, "Phone/0/Evidence");

        if (!Directory.Exists(dcimDir)) Directory.CreateDirectory(dcimDir);
        if (!Directory.Exists(evidenceDir)) Directory.CreateDirectory(evidenceDir);

        string photoFileName = ev.EvidenceName + ".png";
        string photoPath = Path.Combine(dcimDir, photoFileName);

        // save photo
        File.WriteAllBytes(photoPath, bytes);


        string evidencedate = System.DateTime.Now.ToString("dd/MM/yyyy, HH:mm");

        string quackFileName = ev.EvidenceName + ".quack";
        string quackPath = Path.Combine(evidenceDir, quackFileName);

        string slug = "";
        slug += ev.EvidenceName + "\n";
        slug += photoFileName + "\n";
        slug += evidencedate + "\n";
        slug += ev.EvidenceFake + "\n";
        slug += ev.EvidenceQuality + "\n";
        slug += ev.EvidenceDetails + "\n";

        File.WriteAllText(quackPath, slug);

        
        GameMaster.Instance.EvidenceManager.EvidenceFound.TryAdd(ev.EvidenceName, quackPath);

        ev.CollectEvidence();

        // UI feedback
        CameraReadyFrame.color = Color.black;
        CameraReadyText.GetComponent<CanvasGroup>().alpha = 0;
        CameraSavedText.GetComponent<CanvasGroup>().alpha = 1;
        CameraReady = false;

        StartCoroutine(SavedPhoto());
    }

    
    
    private void LoadGallery()
    {
        galleryItems.Clear();

        string evidencePath = Path.Combine(Application.persistentDataPath, "Phone/0/Evidence");
        string dcimPath = Path.Combine(Application.persistentDataPath, "Phone/0/DCIM");

        if (!Directory.Exists(evidencePath))
            return;

        FileInfo[] files = new DirectoryInfo(evidencePath).GetFiles("*.quack");

        foreach (FileInfo file in files)
        {
            string[] lines = File.ReadAllLines(file.FullName);
            if (lines.Length < 6) continue;

            string photoPath = Path.Combine(dcimPath, lines[1].Trim());
            if (!File.Exists(photoPath)) continue;

            galleryItems.Add(new GalleryItem
            {
                evidenceName = lines[0],
                photoFileName = lines[1],
                date = lines[2],
                details = lines[5],
                photoFullPath = photoPath
            });
        }

        PhotosInGallery = galleryItems.Count;
        galleryLoaded = PhotosInGallery > 0;
    }


    IEnumerator SavedPhoto()
    {
        CameraSavedText.alpha = 1;
        yield return new WaitForSeconds(3f);
        CameraSavedText.alpha = 0;
    }
}


public enum PhonerGriddle
{
    // MAIN MENU
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
    Calendar = 1011,
    
    // SUB Menu Stuff
    ViewInboxMessage = 1100,
    ViewSentMessage = 1101,
    
    // DIAL PAD
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
    
    // CONTACTS
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