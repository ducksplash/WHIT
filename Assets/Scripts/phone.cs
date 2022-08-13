using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class phone : MonoBehaviour
{
	public GameObject MobilePhone;
	public Animator DeviceAnim;     
	public GameObject Fader;      
	public GameObject Clock;     
	public GameObject Player;
	public GameObject Camera;
	public GameObject PhoneCamera;
	public Slider UnlockSlider;   
	public GameObject LockScreen;
	public GameObject ContactsScreen;
	public GameObject CameraScreen;
	public GameObject DiallerScreen;
	public GameObject GalleryScreen;
	public GameObject CallingScreen;
	public GameObject MapsScreen;
	public GameObject MessagesScreen;
	public GameObject InboxPane;
	public GameObject SentPane;
	public GameObject BigMessageScreen;
	public GameObject HomeScreen;   
	public GameObject theDialler;

	public GameObject Messagelist;
	public GameObject Sentlist;
	public GameObject MessageBlockPrefab;
	public GameObject NoteBlockPrefab;

	public Image CameraReadyFrame;
	public TextMeshProUGUI CameraReadyText;
	public TextMeshProUGUI CameraSavedText;
	public bool CameraReady;
	private Text DialBar;          
	public Text CallText;       
	public Text CallTitleText;
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
	private bool isLocked = true;
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

	// Make Call Coroutine

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
	
	
	
	
	
	
    void Start()
    {
		WaitingForPhone = true;

		DeviceAnim = MobilePhone.GetComponent<Animator>();
		DialBar = theDialler.GetComponentInChildren<Text>();

		//Debug.Log(Application.persistentDataPath);


		GalleryBigPhoto.GetComponent<CanvasGroup>().alpha = 0f;
		GalleryBigPhoto.GetComponent<CanvasGroup>().blocksRaycasts = false;

		// evidence camera collider
		gameObject.GetComponent<CapsuleCollider>().enabled = false; 
		
		
		PhoneCamera.GetComponent<Camera>().enabled = false;
		MiniMapCam.SetActive(false);

	}







	void changeScreen(GameObject useThisScreen)
	{



		Transform[] allScreens = MobilePhone.GetComponentsInChildren<Transform>();
		
		Transform[] useTheseScreens = useThisScreen.GetComponentsInChildren<Transform>();

		PhoneCamera.GetComponent<Camera>().enabled = false;

		MiniMapCam.SetActive(false);
		CameraOpen = false;


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
	}







    // Update is called once per frame
    void Update()
    {



		if (GameMaster.PHONECOLLECTED)
		{


			DateTime nowDateTime = DateTime.Now;
			string anHour = nowDateTime.Hour.ToString().PadLeft(2, '0');
			string aMinute = nowDateTime.Minute.ToString().PadLeft(2, '0');

			Clock.GetComponent<TMPro.TextMeshProUGUI>().text = anHour + ":" + aMinute;


			if (isLocked == true)
			{
				Fader.GetComponent<CanvasGroup>().alpha = 0.0f;

				if (UnlockSlider.value == 10)
				{
					changeScreen(HomeScreen);
					// disable LockScreen
					isLocked = false;
					Fader.GetComponent<CanvasGroup>().alpha = 1.0f;
				}
			}


			if (Input.GetButtonUp("Phone"))
			{

				TogglePhone();

			}


			if (GameMaster.PHONEOUT && Input.GetKeyUp(KeyCode.Escape))
			{
				TogglePhone();
			}


			// take photos

			if (Input.GetKey(KeyCode.X) && CameraReady)
			{

				TakePhoto();
			}




			// Dial By Keyb
			// need to refactor this


			if ((Input.GetKeyUp("0") || Input.GetKeyUp("[0]")) && GameMaster.PHONEOUT)
			{
				DialBar.text = DialBar.text + "0";
			}
			if ((Input.GetKeyUp("1") || Input.GetKeyUp("[1]")) && GameMaster.PHONEOUT)
			{
				DialBar.text = DialBar.text + "1";
			}
			if ((Input.GetKeyUp("2") || Input.GetKeyUp("[2]")) && GameMaster.PHONEOUT)
			{
				DialBar.text = DialBar.text + "2";
			}
			if ((Input.GetKeyUp("3") || Input.GetKeyUp("[3]")) && GameMaster.PHONEOUT)
			{
				DialBar.text = DialBar.text + "3";
			}
			if ((Input.GetKeyUp("4") || Input.GetKeyUp("[4]")) && GameMaster.PHONEOUT)
			{
				DialBar.text = DialBar.text + "4";
			}
			if ((Input.GetKeyUp("5") || Input.GetKeyUp("[5]")) && GameMaster.PHONEOUT)
			{
				DialBar.text = DialBar.text + "5";
			}
			if ((Input.GetKeyUp("6") || Input.GetKeyUp("[6]")) && GameMaster.PHONEOUT)
			{
				DialBar.text = DialBar.text + "6";
			}
			if ((Input.GetKeyUp("7") || Input.GetKeyUp("[7]")) && GameMaster.PHONEOUT)
			{
				DialBar.text = DialBar.text + "7";
			}
			if ((Input.GetKeyUp("8") || Input.GetKeyUp("[8]")) && GameMaster.PHONEOUT)
			{
				DialBar.text = DialBar.text + "8";
			}
			if ((Input.GetKeyUp("9") || Input.GetKeyUp("[9]")) && GameMaster.PHONEOUT)
			{
				DialBar.text = DialBar.text + "9";
			}
			if ((Input.GetKeyUp("[*]")) && GameMaster.PHONEOUT)
			{
				DialBar.text = DialBar.text + "*";
			}
			if ((Input.GetKeyUp("#")) && GameMaster.PHONEOUT)
			{
				DialBar.text = DialBar.text + "#";
			}

			if ((Input.GetKeyUp("backspace") || Input.GetKeyUp("delete")) && GameMaster.PHONEOUT)
			{
				if (DialBar.text.Length > 0)
				{
					String SubString = DialBar.text.Substring(0, DialBar.text.Length - 1);
					DialBar.text = SubString;
				}
			}


		}



	}






	public void TogglePhone()
	{
		currentpage = 0;




		if (!GameMaster.PHONEOUT)
		{

			if (!GameMaster.INMENU && !GameMaster.HASITEM && !GameMaster.ISWRITING)
			{

				var dialogstring = "I see you found your phone.\n\nTake a few minutes to explore this, and if you miss anything, check \"Messages\" for my past texts and \"Notes\" for your own comments.";
				var fakegameobject = new GameObject("FakeObjectPhone", typeof(BoxCollider));
				fakegameobject.GetComponent<Collider>().enabled = false;
				Player.GetComponent<DialogueManager>().NewDialogue("Kieron", dialogstring, 10, fakegameobject);




				MobilePhone.transform.localPosition = new Vector3(MobilePhone.transform.localPosition.x, MobilePhone.transform.localPosition.y + 1, MobilePhone.transform.localPosition.z);



				//Camera.GetComponent<FirstPersonLook>().enabled = false;
				//FirstPersonCollision.FROZEN = true;
				MobilePhone.GetComponentInChildren<CanvasGroup>().alpha = 1.0f;

				CrosshairCanvas.GetComponent<CanvasGroup>().alpha = 0.0f;
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;

				GameMaster.PHONEOUT = true;
				GameMaster.INMENU = true;
				gameObject.GetComponent<CapsuleCollider>().enabled = true;
			}
		}
		else
		{
			changeScreen(HomeScreen);

			GameMaster.PHONEOUT = false;
			GameMaster.INMENU = false;
			gameObject.GetComponent<CapsuleCollider>().enabled = false;
			MobilePhone.transform.localPosition = new Vector3(MobilePhone.transform.localPosition.x, MobilePhone.transform.localPosition.y - 1, MobilePhone.transform.localPosition.z);

			CrosshairCanvas.GetComponent<CanvasGroup>().alpha = 0.9f;
			//Camera.GetComponent<FirstPersonLook>().enabled = true;
			//FirstPersonCollision.FROZEN = false;
			MobilePhone.GetComponentInChildren<CanvasGroup>().alpha = 0.0f;



			if (!TorchLight.enabled)
			{
				Torch.torchToggle = true;
				TorchLight.enabled = true;
			}

			CameraLeftFlash.enabled = false;
			CameraRightFlash.enabled = false;

			//Cursor.lockState = CursorLockMode.Locked;
			//Cursor.visible = false;
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

		Debug.Log("maps button");

	}



	public void MessagesButton(string msgtype)
	{


		for (int i = 0; i < Messagelist.transform.childCount; i++)
		{
			Destroy(Messagelist.transform.GetChild(i).gameObject);
		}


		for (int i = 0; i < Sentlist.transform.childCount; i++)
		{
			Destroy(Sentlist.transform.GetChild(i).gameObject);
		}


		changeScreen(MessagesScreen);

		if (msgtype == "sent")
		{

			SentItems();

		}

		if (msgtype == "inbox")
		{

			GetMessages();

		}



		Debug.Log("Messages button");

	}




	public void BigMessage(string Sender, string Message, string xtype="Inbox")
	{


		BigMessageScreen.GetComponent<CanvasGroup>().alpha = 1;
		BigMessageScreen.GetComponent<CanvasGroup>().blocksRaycasts = true;

		if (xtype == "Inbox")
		{
			BigMessageScreen.transform.Find("fromPREFIX").GetComponent<TextMeshProUGUI>().text = "From:";
			BigMessageScreen.transform.Find("fromFROM").GetComponent<TextMeshProUGUI>().text = Sender;
		}

		if (xtype == "Sent")
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
		foreach (KeyValuePair<string, string> entry in GameMaster.DialogueSeen)
		{
			if (!entry.Value.Contains("NORA"))
			{
				var buttonPosition = new Vector3(Messagelist.transform.localPosition.x, Messagelist.transform.localPosition.y, Messagelist.transform.localPosition.z);

				GameObject messageBlock = Instantiate(MessageBlockPrefab, buttonPosition, Quaternion.identity);
				messageBlock.transform.SetParent(Messagelist.transform, false);

				Debug.Log(entry.Key);
				Debug.Log("---");
				Debug.Log(entry.Value);


				messageBlock.transform.Find("fromFROM").GetComponent<TextMeshProUGUI>().text = entry.Value;

				var messageBit = entry.Key;

				if (messageBit.Length > 32)
				{
					messageBit = messageBit.Substring(0, 32) + "...";
				}


				messageBlock.transform.Find("bitofMSG").GetComponent<TextMeshProUGUI>().text = messageBit;

				messageBlock.GetComponent<Button>().onClick.AddListener(delegate
				{
					BigMessage(entry.Value, entry.Key);
				});
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
		foreach (KeyValuePair<string, string> entry in GameMaster.DialogueSeen)
		{
			if (entry.Value.Contains("NORA"))
			{

				var buttonPosition = new Vector3(Sentlist.transform.localPosition.x, Sentlist.transform.localPosition.y, Sentlist.transform.localPosition.z);

				GameObject messageBlock = Instantiate(NoteBlockPrefab, buttonPosition, Quaternion.identity);
				messageBlock.transform.SetParent(Sentlist.transform, false);

				Debug.Log(entry.Key);
				Debug.Log("---");
				Debug.Log(entry.Value);


				messageBlock.transform.Find("fromFROM").GetComponent<TextMeshProUGUI>().text = entry.Value;

				var messageBit = entry.Key;

				if (messageBit.Length > 32)
				{
					messageBit = messageBit.Substring(0, 32) + "...";
				}


				messageBlock.transform.Find("bitofMSG").GetComponent<TextMeshProUGUI>().text = messageBit;

				messageBlock.GetComponent<Button>().onClick.AddListener(delegate
				{
					BigMessage(entry.Value, entry.Key, "Sent");
				});
			}
		}
	}







	// Contacts menu incorporating CallScreen, Dialler.
	public void CameraButton()
	{



		changeScreen(CameraScreen);

		if (TorchLight.enabled)
		{
			Torch.torchToggle = false;
			TorchLight.enabled = false;
		}

		CameraLeftFlash.enabled = true;
		CameraRightFlash.enabled = true;


		if (GameMaster.EvidenceFound.Count < 1)
		{
			var dialogstring = "First photo?\n\nWhen evidence is in view, the camera frame will turn green and you just have to press X.\n\nNot green? Not evidence.\n\nYou may have to crouch.";
			var fakegameobject = new GameObject("FakeObjectPhone", typeof(BoxCollider));
			fakegameobject.GetComponent<Collider>().enabled = false;
			Player.GetComponent<DialogueManager>().NewDialogue("Kieron", dialogstring, 10, fakegameobject);
		}

		CameraReadyText.GetComponent<CanvasGroup>().alpha = 0;
		CameraSavedText.GetComponent<CanvasGroup>().alpha = 0;
		PhoneCamera.GetComponent<Camera>().enabled = true;
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


	public void GalleryBackNext(string direction)
	{

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
		var filepath = Application.persistentDataPath + "/Phone/0/Evidence/";


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

			if (page+1 >= PhotosInGallery)
			{
				galleryNext.interactable = false;
			}
			else
			{
				galleryNext.interactable = true;
			}



			MemoryStream dest = new MemoryStream();

			var photopath = Application.persistentDataPath + "/Phone/0/DCIM/";
			


			byte[] imageData = File.ReadAllBytes(photopath + lines[1]);

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

			changeScreen(DiallerScreen);

		}
	}





	public void a1Button()
			{
				DialBar.text = DialBar.text+"1";
			}				
			public void a2Button()
			{
				DialBar.text = DialBar.text+"2";
			}				
			public void a3Button()
			{
				DialBar.text = DialBar.text+"3";
			}				
			public void a4Button()
			{
				DialBar.text = DialBar.text+"4";
			}				
			public void a5Button()
			{
				DialBar.text = DialBar.text+"5";
			}				
			public void a6Button()
			{
				DialBar.text = DialBar.text+"6";
			}				
			public void a7Button()
			{
				DialBar.text = DialBar.text+"7";
			}				
			public void a8Button()
			{
				DialBar.text = DialBar.text+"8";
			}				
			public void a9Button()
			{
				DialBar.text = DialBar.text+"9";
			}				
			public void a0Button()
			{
				DialBar.text = DialBar.text+"0";
			}					
			public void asButton()
			{
				DialBar.text = DialBar.text+"*";
			}					
			public void ahButton()
			{
				DialBar.text = DialBar.text+"#";
			}
			public void backspaceButton()
			{
				if (DialBar.text.Length > 0)
				{
				String SubString = DialBar.text.Substring(0,DialBar.text.Length-1);
				DialBar.text = SubString;
				}
			}	
			
	public void callButton()
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

		currentpage = 0;

		changeScreen(HomeScreen);
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



	public void TakePhoto()
	{


		if (ObservedEvidence != null)
		{

			if (GameMaster.EvidenceFound.Count < 1)
			{
				var dialogstring = "Brilliant!\n\nYou can open the Gallery app on your phone to see all the evidence you've collected.";
				var fakegameobject = new GameObject("FakeObjectPhone", typeof(BoxCollider));
				fakegameobject.GetComponent<Collider>().enabled = false;
				Player.GetComponent<DialogueManager>().NewDialogue("Kieron", dialogstring, 5, fakegameobject);

			}


			RenderTexture activeRenderTexture = RenderTexture.active;
			RenderTexture.active = PhoneCamera.GetComponent<Camera>().targetTexture;

			PhoneCamera.GetComponent<Camera>().Render();


			Texture2D image = new Texture2D(PhoneCamera.GetComponent<Camera>().targetTexture.width, PhoneCamera.GetComponent<Camera>().targetTexture.height);
			image.ReadPixels(new Rect(0, 0, PhoneCamera.GetComponent<Camera>().targetTexture.width, PhoneCamera.GetComponent<Camera>().targetTexture.height), 0, 0);
			image.Apply();
			RenderTexture.active = activeRenderTexture;

			byte[] bytes = image.EncodeToPNG();
			Destroy(image);



			var photoname = ObservedEvidence.name;

			var filepath = Application.persistentDataPath + "/Phone/0/DCIM/";

			System.IO.FileInfo file = new System.IO.FileInfo(filepath);


			DirectoryInfo di = Directory.CreateDirectory(filepath);


			var filenameString = file.FullName + photoname + ".png";

			var nameForEvidenceFile = photoname + ".png";

			System.IO.File.WriteAllBytes(filenameString, bytes);


			ObservedEvidence.GetComponent<Evidence>().CollectEvidence(Player);

			CameraReadyFrame.color = Color.black;
			CameraReadyText.GetComponent<CanvasGroup>().alpha = 0;
			CameraSavedText.GetComponent<CanvasGroup>().alpha = 1;
			CameraReady = false;

			StartCoroutine(SavedPhoto());

		}


	}





	IEnumerator SavedPhoto()
    {

		CameraSavedText.alpha = 1;


		yield return new WaitForSeconds(3f);


		CameraSavedText.alpha = 0;




	}






}
