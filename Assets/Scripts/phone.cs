using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Cursor = UnityEngine.Cursor;
using Image = UnityEngine.UI.Image;

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
    public GameObject MapsScreen;
    public GameObject MessagesScreen;
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

    //public InputActionReference SubmitAction; // multi-button

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

    public bool viewingPhoto;

    public DialogueName phoneTutorialFirstPhoto = DialogueName.phoneTutorialFirstPhoto;

    // Make Call Coroutine


    private void Awake()
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
        
        
        CallTitleText.text = "Phone";
    }


    IEnumerator CallContact()
    {
        // disable DiallerScreen
        DiallerScreen.GetComponent<CanvasGroup>().alpha = 0.0f;
        DiallerScreen.GetComponent<CanvasGroup>().blocksRaycasts = false;

        // disable ContactsScreen
        ContactsScreen.GetComponent<CanvasGroup>().alpha = 0.0f;
        ContactsScreen.GetComponent<CanvasGroup>().blocksRaycasts = false;

        // enable CallingScreen
        CallingScreen.GetComponent<CanvasGroup>().alpha = 1.0f;
        CallingScreen.GetComponent<CanvasGroup>().blocksRaycasts = true;

        callingContact = true;

        yield return null;
    }


    void changeScreen(GameObject useThisScreen)
    {
        Transform[] allScreens = MobilePhone.GetComponentsInChildren<Transform>();

        Transform[] useTheseScreens = useThisScreen.GetComponentsInChildren<Transform>();

        PhoneCamera.GetComponent<Camera>().enabled = false;
        
        if (MiniMapCam.activeSelf)
        {
            MiniMapCam.SetActive(false);
            GameMaster.Instance.FROZEN = true;
        }

        if (CameraOpen)
        {
            CameraOpen = false;
            GameMaster.Instance.FROZEN = true;
            //SubmitAction.action.performed -= TakePhoto;
        }


        CameraLeftFlash.enabled = false;
        CameraRightFlash.enabled = false;


        var readyForNewScreen = false;


        foreach (Transform screen in allScreens)
        {
            if (!screen.name.Contains("SCREENPANEL"))
            {
                if (screen.GetComponent<CanvasGroup>())
                {
                    screen.GetComponent<CanvasGroup>().alpha = 0.0f;
                    screen.GetComponent<CanvasGroup>().blocksRaycasts = false;

                    readyForNewScreen = true;
                }
                else if (screen.GetComponent<TMPro.TextMeshPro>())
                {
                    if (!screen.name.Contains("clock") && !screen.name.Contains("network"))
                    {
                        screen.GetComponent<TMPro.TextMeshPro>().enabled = false;
                        //screen.GetComponent<CanvasGroup>().blocksRaycasts = false;	
                        Debug.Log("TMPro.TextMeshPro");
                        readyForNewScreen = true;
                    }
                }
                else if (screen.GetComponent<TMPro.TextMeshProUGUI>())
                {
                    if (!screen.name.Contains("clock") && !screen.name.Contains("network"))
                    {
                        screen.GetComponent<TMPro.TextMeshProUGUI>().enabled = false;
                        //screen.GetComponent<CanvasGroup>().blocksRaycasts = false;	
                        readyForNewScreen = true;
                    }
                }
                else
                {
                    readyForNewScreen = true;
                }
            }
        }


        if (readyForNewScreen)
        {
            foreach (Transform thisScreen in useTheseScreens)
            {
                if (thisScreen.GetComponent<CanvasGroup>())
                {
                    thisScreen.GetComponent<CanvasGroup>().alpha = 1.0f;
                    thisScreen.GetComponent<CanvasGroup>().blocksRaycasts = true;
                }
                else if (thisScreen.GetComponent<TMPro.TextMeshPro>())
                {
                    thisScreen.GetComponent<TMPro.TextMeshPro>().enabled = true;
                }
                else if (thisScreen.GetComponent<TMPro.TextMeshProUGUI>())
                {
                    thisScreen.GetComponent<TMPro.TextMeshProUGUI>().enabled = true;
                }
                else
                {
                    //shush you
                    //Debug.Log("don't got it");
                }
            }
        }
        
        
        DisableNavvers();
        

        if (HomeScreen.GetComponent<CanvasGroup>().alpha > 0.9f)
        {
            HomeScreenNavver.gameObject.SetActive(true);
        }

        if (DiallerScreen.GetComponent<CanvasGroup>().alpha > 0.9f)
        {
            DiallerScreenNavver.gameObject.SetActive(true);
        }

        if (ContactsScreen.GetComponent<CanvasGroup>().alpha > 0.9f)
        {
            ContactsScreenNavver.gameObject.SetActive(true);
        }

        
    }

    private void DisableNavvers()
    {
        HomeScreenNavver.gameObject.SetActive(false);
        DiallerScreenNavver.gameObject.SetActive(false);
        ContactsScreenNavver.gameObject.SetActive(false);
    }
    
    
    
    

    // Update is called once per frame
    void Update()
    {
        if (!GameMaster.Instance.OnboardingManager.PHONECOLLECTED) return;

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
        DisableNavvers();
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

        CameraLeftFlash.enabled = false;
        CameraRightFlash.enabled = false;
    }


    public void SelectPhoneGridItem(PhonerGriddle SelectedGridSquare)
    {
        if (!GameMaster.Instance.PHONEOUT) return;
        
        switch (SelectedGridSquare)
        {
            // Main Menu
            case PhonerGriddle.Telephone: ContactsButton(); break;
            case PhonerGriddle.Camera: CameraButton(); break;
            case PhonerGriddle.Map: MapsButton(); break;
            case PhonerGriddle.Messages: MessagesButton(); break;
            case PhonerGriddle.Gallery: GalleryButton(); break;
            
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
            case PhonerGriddle.NumCall: callButton(); break;
            
            // Contacts
            case PhonerGriddle.Dialpad: DiallerButton(); break;
            case PhonerGriddle.Kieron: callKieron(); break;
            case PhonerGriddle.Darragh: callDarragh(); break;
            case PhonerGriddle.Mary: callMary(); break;
            case PhonerGriddle.Tom: callTom(); break;
            case PhonerGriddle.Work: callWork(); break;
            case PhonerGriddle.Unknown: callAnon(); break;
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

        //var itty = 0;
        foreach (DialogueName entry in GameMaster.Instance.DialogueSeen)
        {
            Dialogue selectedDialogue = GameMaster.Instance.DialogueManager.Dialogues.FirstOrDefault(d => d.DialogueName == entry);

            if (selectedDialogue.Contact != Contacts.Nora)
            {
                var buttonPosition = new Vector3(Messagelist.transform.localPosition.x, Messagelist.transform.localPosition.y, Messagelist.transform.localPosition.z);

                GameObject messageBlock = Instantiate(MessageBlockPrefab, buttonPosition, Quaternion.identity);
                messageBlock.transform.SetParent(Messagelist.transform, false);


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
    }


    public void SentItems()
    {
        CloseBigMessage();

        InboxPane.GetComponent<CanvasGroup>().alpha = 0f;
        InboxPane.GetComponent<CanvasGroup>().blocksRaycasts = false;

        SentPane.GetComponent<CanvasGroup>().alpha = 1f;
        SentPane.GetComponent<CanvasGroup>().blocksRaycasts = true;

        //var itty = 0;
        foreach (DialogueName entry in GameMaster.Instance.DialogueSeen)
        {
            Dialogue selectedDialogue = GameMaster.Instance.DialogueManager.Dialogues.FirstOrDefault(d => d.DialogueName == entry);


            if (selectedDialogue.Contact == Contacts.Nora)
            {
                var buttonPosition = new Vector3(Sentlist.transform.localPosition.x, Sentlist.transform.localPosition.y, Sentlist.transform.localPosition.z);

                GameObject messageBlock = Instantiate(NoteBlockPrefab, buttonPosition, Quaternion.identity);
                messageBlock.transform.SetParent(Sentlist.transform, false);


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
    }


    // Contacts menu incorporating CallScreen, Dialler.
    public void CameraButton()
    {
        changeScreen(CameraScreen);


        //SubmitAction.action.performed += TakePhoto;

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

        if (GameMaster.Instance.EvidenceFound.Count < 1)
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


        if (TorchLight.enabled)
        {
            Torch.torchToggle = false;
            TorchLight.enabled = false;
        }

        // lets get the files
        // we're only counting them here, the fishing will take place elsewhere
        var filepath = Application.persistentDataPath + "/Phone/0/Evidence/";


        DirectoryInfo dir = new DirectoryInfo(filepath);
        if (dir.Exists)
        {
            FileInfo[] info = dir.GetFiles("*.quack");

            if (info.Length > 0)
            {
                gotfiles = true;
            }
            else
            {
                gotfiles = false;
            }
        }
        else
        {
            gotfiles = false;
        }


        // get the panels
        CanvasGroup[] GalleryPanes = GalleryScreen.GetComponentsInChildren<CanvasGroup>();


        // gallery got stuff?

        // we have to temp it cos we haven't built all the back end for the photos and evidence yet :P


        // got at least one photo?
        if (gotfiles)
        {
            foreach (CanvasGroup screen in GalleryPanes)
            {
                if (screen.name.Contains("GALLERYPANE"))
                {
                    screen.alpha = 1;
                    screen.blocksRaycasts = true;
                    GalleryGetContent();
                }
                else
                {
                    screen.alpha = 0;
                    screen.blocksRaycasts = false;
                }
            }

            GalleryScreen.GetComponent<CanvasGroup>().alpha = 1;
            GalleryScreen.GetComponent<CanvasGroup>().blocksRaycasts = true;
            //SubmitAction.action.performed += GalleryViewPhoto;
        }
        else
        {
            foreach (CanvasGroup screen in GalleryPanes)
            {
                if (screen.name.Contains("EMPTYPANE"))
                {
                    screen.alpha = 1;
                    screen.blocksRaycasts = true;
                }
                else
                {
                    screen.alpha = 0;
                    screen.blocksRaycasts = false;
                }
            }

            GalleryScreen.GetComponent<CanvasGroup>().alpha = 1;
            GalleryScreen.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
    }

    public void GalleryNextAction(InputAction.CallbackContext callbackContext)
    {
        if (!GameMaster.Instance.PHONEOUT) return;

        int next = currentpage + 1;

        if (next < PhotosInGallery)
        {
            currentpage = next;
            GalleryGetContent(currentpage);
        }
    }


    public void GalleryBackAction(InputAction.CallbackContext callbackContext)
    {
        if (!GameMaster.Instance.PHONEOUT) return;

        int prev = currentpage - 1;

        if (prev >= 0)
        {
            currentpage = prev;
            GalleryGetContent(currentpage);
        }
    }


    public void GalleryBackNext(string direction)
    {
        if (GalleryScreen.GetComponent<CanvasGroup>().alpha < 0f) return; 
        
        if (direction.Equals("back"))
        {
            Debug.Log("This Page: " + currentpage + "\nThis Page -1 : " + (currentpage - 1));
            if (currentpage > 0)
            {
                GalleryGetContent(currentpage - 1);
                currentpage--;
            }
        }

        if (direction.Equals("next"))
        {
            if (currentpage < PhotosInGallery)
            {
                GalleryGetContent(currentpage + 1);
                currentpage++;
            }
        }
    }


    public void GalleryViewPhoto(InputAction.CallbackContext callbackContext)
    {
        if (!GameMaster.Instance.PHONEOUT) return;

        viewingPhoto = !viewingPhoto;

        if (viewingPhoto)
        {
            GalleryClosePhoto();
        }
        else
        {
            GalleryEnlargePhoto();
        }
    }


    public void GalleryEnlargePhoto()
    {
        GalleryBigPhoto.GetComponent<CanvasGroup>().alpha = 1f;
        GalleryBigPhoto.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }


    public void GalleryClosePhoto()
    {
        GalleryBigPhoto.GetComponent<CanvasGroup>().alpha = 0f;
        GalleryBigPhoto.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }


    void GalleryGetContent(int page = 0)
    {
        // lets get the files
        var filepath = Path.Combine(Application.persistentDataPath, "Phone/0/Evidence");


        DirectoryInfo dir = new DirectoryInfo(filepath);
        if (dir.Exists)
        {
            FileInfo[] info = dir.GetFiles("*.quack");

            var lines = System.IO.File.ReadAllLines(info[page].FullName);

            PhotosInGallery = info.Length;


            if (page < 1)
            {
                galleryBack.interactable = false;
            }
            else
            {
                galleryBack.interactable = true;
            }

            if (page + 1 >= PhotosInGallery)
            {
                galleryNext.interactable = false;
            }
            else
            {
                galleryNext.interactable = true;
            }


            MemoryStream dest = new MemoryStream();

            var photopath = Path.Combine(Application.persistentDataPath, "Phone/0/DCIM");

            string fileName = lines[1].Trim();

            string fullPhotoPath = Path.Combine(photopath, fileName);

            if (!File.Exists(fullPhotoPath))
            {
                Debug.LogError("PHOTO NOT FOUND: " + fullPhotoPath);
                return;
            }

            byte[] imageData = File.ReadAllBytes(fullPhotoPath);

            //Create new Texture2D
            Texture2D tempTexture = new Texture2D(100, 100);

            //Load the Image Byte to Texture2D
            tempTexture.LoadImage(imageData);


            var finaltexture = tempTexture;

            //Load to Rawmage?
            galleryEvidencePhoto.texture = finaltexture;
            GalleryBigPhoto.GetComponent<RawImage>().texture = finaltexture;


            // the script that saves these
            // is in "Evidence.cs".
            galleryEvidenceName.text = lines[0];
            galleryEvidenceDate.text = lines[2];
            galleryEvidenceDate.text = lines[2];
            galleryEvidenceDetails.text = lines[5];
        }
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

    public void callButton(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (DialBar.text == "*#06#")
        {
            CallTitleText.text = "Seriously?";
            CallText.text = "What did you expect?\na real IMEI number?\n\neejit.";
        }
        else if (DialBar.text == "999" || DialBar.text == "112" || DialBar.text == "911")
        {
            CallTitleText.text = "That won't work";
            CallText.text = "If you have a real emergency, please use a real phone.";
        }
        else
        {
            CallTitleText.text = "Calling...";
            CallText.text = DialBar.text;
        }


        changeScreen(CallingScreen);
    }


    public void callAnon()
    {
        CallTitleText.text = "Calling\n?";
        CallText.text = anonNum.text;

        StartCoroutine("CallContact");
    }


    public void callDarragh()
    {
        CallTitleText.text = "Calling\nDarragh";
        CallText.text = darraghNum.text;

        StartCoroutine("CallContact");
    }


    public void callKieron()
    {
        CallTitleText.text = "Calling\nKieron";
        CallText.text = kieronNum.text;

        StartCoroutine("CallContact");
    }


    public void callMary()
    {
        CallTitleText.text = "Calling\nMary";
        CallText.text = maryNum.text;

        StartCoroutine("CallContact");
    }


    public void callTom()
    {
        CallTitleText.text = "Calling\nTom";
        CallText.text = tomNum.text;

        StartCoroutine("CallContact");
    }


    public void callWork()
    {
        CallTitleText.text = "Calling\nWork";
        CallText.text = workNum.text;

        StartCoroutine("CallContact");
    }


    public void BackButton()
    {
        Debug.Log("BAKK");
        Debug.Log(HomeScreen.GetComponent<CanvasGroup>().alpha);
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

        // register runtime lookup
        if (!GameMaster.Instance.EvidenceFound.ContainsKey(ev.EvidenceName))
            GameMaster.Instance.EvidenceFound.Add(ev.EvidenceName, quackPath);

        // mark collected + gameplay effects
        ev.CollectEvidence();

        // UI feedback
        CameraReadyFrame.color = Color.black;
        CameraReadyText.GetComponent<CanvasGroup>().alpha = 0;
        CameraSavedText.GetComponent<CanvasGroup>().alpha = 1;
        CameraReady = false;

        StartCoroutine(SavedPhoto());
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