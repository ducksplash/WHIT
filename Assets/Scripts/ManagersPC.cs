using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManagersPC : MonoBehaviour
{
	
	// Get the main pieces
	public GameObject thisComputer;
	private string thisComputerName;
	public bool usingComputer = false;
	public GameObject Player;   
	public GameObject PlayerCamera;   
	public GameObject CrossHair;  

	// Get the screens
	public GameObject HomeScreen;
	public GameObject DoorScreen;
	public GameObject LightScreen;
	public GameObject CCTVScreen;
	public GameObject FileScreen;
	public GameObject FileEditScreen;
	public GameObject EmailScreen;
	public GameObject EmailViewScreen;
	public GameObject EmailViewScreen2;

	// Get the Doors
	public GameObject StaffDoor;
	public GameObject FridgeDoorUpper;
	public GameObject FridgeDoorLower;
	public GameObject LowerMain;

	// Get door map icons

	public Image FridgeDoorImg;
	public Image CanteenDoorImg;
	public Image MaintenanceDoorImg;
	public Image Processing2DoorImg;
	public TextMeshProUGUI DoorFeedbackText;

	// Get DoorScreen Buttons
	public GameObject DoorButtonFG;
	public GameObject DoorButtonFB;
	public GameObject DoorButtonCA;
	public GameObject DoorButtonPR;


	// Get Cameras Actual
	public Camera CAM0;
	public Camera CAM1;
	public Camera CAM2;
	public Camera CAM3;
	public Camera CAM4;
	public Camera CAM5;
	public Camera CAM6;
	public Camera CAM7;
	public Camera CAM8;
	
	
	// Get CCTV Screens
	public GameObject[] CCTVScreens;
	public GameObject[] CCTVSCameras;	 
	public GameObject CCTV0;
	public GameObject CCTV1;
	public GameObject CCTV2;
	public GameObject CCTV3;
	public GameObject CCTV4;
	public GameObject CCTV5;
	public GameObject CCTV6;
	public GameObject CCTV7;
	public GameObject CCTV8;

	private GameObject CurrentCCTV;

	public Collider EvidenceCollider;
	public Collider EvidenceCollider2;

	// OFF screen
	public GameObject screenCover;




	public string existingCode;
	public string newCode;

	public TextAsset brokenCode;
	public TextAsset fixedCode;

	public TMP_InputField UserEnteredCode;

	public Button SaveFileButton;
	public Button ResetFileButton;
	public Button CancelButton;
	public CanvasGroup CompileFailedScreen;
	public CanvasGroup CompileSucceededScreen;


	void Start()
    {





		existingCode = brokenCode.text;

		newCode = fixedCode.text;


		//var printOldCode = existingCode.Replace("\r", "").Replace("\n", "").Replace("\n", "");
		//var printNewCode = newCode.Replace("\r", "").Replace("\n", "").Replace("\n", "");


		//Debug.Log(printOldCode);




		FridgeDoorImg.color = Color.red;
		CanteenDoorImg.color = Color.red;
		MaintenanceDoorImg.color = Color.red;
		Processing2DoorImg.color = Color.red;

		thisComputerName = thisComputer.name;
		
		// Add cctv screens to list
		TextMeshPro DoorButtonFG = GetComponent<TextMeshPro>();
		TextMeshPro DoorButtonFB = GetComponent<TextMeshPro>();
		TextMeshPro DoorButtonCA = GetComponent<TextMeshPro>();


		EvidenceCollider.enabled = false;
		EvidenceCollider2.enabled = false;
	}


    // Update is called once per frame
    void Update()
    {
		
		
		if (GameMaster.POWER_SUPPLY_ENABLED)
		{
			
			screenCover.GetComponent<Image>().enabled = false;



			// this before that
			if (usingComputer)
			{

				if (Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.P))
				{
					TogglePC();
				}
			}

			if (!usingComputer)
			{

				if (Input.GetMouseButtonUp(1))
				{
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					RaycastHit hit;
					if (Physics.Raycast(ray, out hit))
					{
						if (hit.distance <= 10.5f)
						{

							if (hit.transform.name.Equals(thisComputerName))
							{

								TogglePC();

							}
						}
					}
				}

			}
		}
		else
		{
			screenCover.GetComponent<Image>().enabled = true;
		}
		
		
		
    }












	public void TogglePC()
    {

		if (!usingComputer)
		{
			usingComputer = true;
			thisComputer.GetComponent<Collider>().enabled = false;

			PlayerCamera.GetComponent<FirstPersonLook>().enabled = false;
			//Player.GetComponent<FirstPersonCollision>().enabled = true;
			//Player.GetComponent<Jump>().enabled = false;
			GameMaster.FROZEN = true;
			GameMaster.INMENU = true;

			CrossHair.GetComponent<Canvas>().enabled = false;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

		}
		else
		{
			PlayerCamera.GetComponent<FirstPersonLook>().enabled = true;
			thisComputer.GetComponent<Collider>().enabled = true;
			//Player.GetComponent<FirstPersonCollision>().enabled = true;
			//Player.GetComponent<Jump>().enabled = true;
			GameMaster.FROZEN = false;
			GameMaster.INMENU = false;

			CrossHair.GetComponent<Canvas>().enabled = true;
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

			usingComputer = false;
		}


	}




	// ChangeScreen function (screen on, screen off)
	void ChangeScreen(GameObject onScreen, GameObject offScreen)
	{
		// disable the "Off Screen"
		offScreen.GetComponent<CanvasGroup>().alpha = 0.0f;
		offScreen.GetComponent<CanvasGroup>().blocksRaycasts = false;	
		
		// enable the "On Screen"
		onScreen.GetComponent<CanvasGroup>().alpha = 1.0f;
		onScreen.GetComponent<CanvasGroup>().blocksRaycasts = true;


	}
	
	public void GoHome()
	{
		// ChangeScreen function (screen on, screen off)
		ChangeScreen(HomeScreen, DoorScreen);
		
		ChangeScreen(HomeScreen, CCTVScreen);



		ChangeScreen(HomeScreen, FileScreen);



		ChangeScreen(HomeScreen, EmailScreen);

		ChangeScreen(HomeScreen, EmailViewScreen);

		ChangeScreen(HomeScreen, EmailViewScreen2);

		EvidenceCollider.enabled = false;
		EvidenceCollider2.enabled = false;

	}	
	
	
	
	// Populate and return an array of the screens onto which the Cameras are projected.
	// These are RawImage objects with RenderTextures set as the material
	public GameObject[] getCCTVArray()
	{
		GameObject[] theseScreens = {CCTV0, CCTV1, CCTV2, CCTV3, CCTV4, CCTV5, CCTV6, CCTV7, CCTV8};
		
		return theseScreens;
	}	
	
	// Populate and return an array of Camera objects
	public Camera[] getCameraArray()
	{
		Camera[] theseCameras = {CAM0, CAM1, CAM2, CAM3, CAM4, CAM5, CAM6, CAM7, CAM8};
		
		return theseCameras;
	}
	
	
	// Open the CCTV Select Screen
	// As this is used when first accessing CCTV, and upon return, we disable everything.
	// This doesn't matter when first entering CCTV, but certainly matters when leaving.
	// A < HOME < button is deliberately left out, to force the user to go back the way they came
	public void GoCCTV()
	{
		
		// ChangeScreen function (screen on, screen off)
		ChangeScreen(CCTVScreen, HomeScreen);

		CCTVScreens = getCCTVArray();

		Camera[] CCTVSCameras = getCameraArray();


	
		for (int i = 0; i < CCTVScreens.Length; i++)
		{
			CCTVScreens[i].GetComponent<CanvasGroup>().alpha = 0f;
			CCTVScreens[i].GetComponent<CanvasGroup>().blocksRaycasts = false;			
		}		
			
		for (int i = 0; i < CCTVSCameras.Length; i++)
		{
			CCTVSCameras[i].GetComponent<Camera>().enabled = false;		
		}

		
	}	
	
	public void CamBack(int Screen)
	{
		
		CCTVScreens = getCCTVArray();

		Camera[] CCTVSCameras = getCameraArray();
		
		if (Screen < 9 && Screen > 0)
		{
			
		CCTVSCameras[Screen].GetComponent<Camera>().enabled = false;
		CCTVSCameras[Screen-1].GetComponent<Camera>().enabled = true;		
	
		CCTVScreens[Screen].GetComponent<CanvasGroup>().alpha = 0f;
		CCTVScreens[Screen].GetComponent<CanvasGroup>().blocksRaycasts = false;		
		
		CCTVScreens[Screen-1].GetComponent<CanvasGroup>().blocksRaycasts = true;	
		CCTVScreens[Screen-1].GetComponent<CanvasGroup>().alpha = 1f;

		}			
	}		
	
	
	public void CamForward(int Screen)
	{
		CCTVScreens = getCCTVArray();

		Camera[] CCTVSCameras = getCameraArray();
		
		if (Screen >= 0 && Screen < 9)
		{
		CCTVSCameras[Screen].GetComponent<Camera>().enabled = false;
		
		CCTVSCameras[Screen+1].GetComponent<Camera>().enabled = true;	

		
		CCTVScreens[Screen+1].GetComponent<CanvasGroup>().alpha = 1f;	
		CCTVScreens[Screen+1].GetComponent<CanvasGroup>().blocksRaycasts = true;
		CCTVScreens[Screen].GetComponent<CanvasGroup>().alpha = 0f;
		CCTVScreens[Screen].GetComponent<CanvasGroup>().blocksRaycasts = false;
		}
			
	}	
	
	
	
	
	public void DoorsButton()
	{
		ChangeScreen(DoorScreen, HomeScreen);
	}	
	
	
	// I belong to the four functions below
	private void buttonOp(GameObject theButton, GameObject theDoor, Image dooricon, string FeedbackName)
	{
		if (theDoor.GetComponent<innerDoors>().isLocked)
		{
			theButton.transform.GetComponent<Image>().color = Color.green;
			theDoor.GetComponent<innerDoors>().isLocked = false;
			theDoor.GetComponent<innerDoors>().doLockedLights();
			dooricon.color = Color.green;

			DoorFeedbackText.text = "** " +FeedbackName+" Door Unlocked"+" **";

		}
		else
		{
			theButton.transform.GetComponent<Image>().color = new Color32(77,76,164,100);
			theDoor.GetComponent<innerDoors>().isLocked = true;
			theDoor.GetComponent<innerDoors>().doLockedLights();
			dooricon.color = Color.red;
			DoorFeedbackText.text = "** " + FeedbackName + " Door Locked" + " **";
		}
	}
	
	
	public void FridgeG()
	{
		buttonOp(DoorButtonFG, FridgeDoorUpper, FridgeDoorImg, "Fridge");
	}	
	
	public void FridgeB()
	{
		buttonOp(DoorButtonFB, FridgeDoorLower, MaintenanceDoorImg, "Maintenance");
	}

	public void Canteen()
	{
		buttonOp(DoorButtonCA, StaffDoor, CanteenDoorImg, "Canteen");
	}


	public void Processing()
	{
		buttonOp(DoorButtonPR, LowerMain, Processing2DoorImg, "Processing 2");
	}
	
	public void CCTV(int Screen)
	{	
		CCTVScreens = getCCTVArray();

		Camera[] CCTVSCameras = getCameraArray();
	
		CCTVSCameras[Screen].GetComponent<Camera>().enabled = true;		
	
		CurrentCCTV = CCTVScreens[Screen];
	
		ChangeScreen(CurrentCCTV,CCTVScreen);
	
	}


	// File System

	// populate the list of files



	public void FilesButton()
	{
		// screen on, off
		ChangeScreen(FileScreen, HomeScreen);
		ChangeScreen(FileScreen, FileEditScreen);

		EvidenceCollider.enabled = false;
		EvidenceCollider2.enabled = false;

		CompileFailedScreen.alpha = 0f;
		CompileFailedScreen.blocksRaycasts = false;

		CompileSucceededScreen.alpha = 0f;
		CompileSucceededScreen.blocksRaycasts = false;

	}



	public void EditFile()
	{
		GameMaster.ISWRITING = true;
		ChangeScreen(FileEditScreen, FileScreen);

		CompileFailedScreen.alpha = 0f;
		CompileFailedScreen.blocksRaycasts = false;

		CompileSucceededScreen.alpha = 0f;
		CompileSucceededScreen.blocksRaycasts = false;

		SaveFileButton.interactable = true;
		ResetFileButton.interactable = true;
		CancelButton.interactable = true;

	}


	public void ResetFile()
	{
		UserEnteredCode.text = brokenCode.text;
	}


	public void SaveCode()
	{

		GameMaster.ISWRITING = false;

		var printNewCode = newCode.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "").Replace("\r\n", "").Trim().ToString();
		var printUserCode = UserEnteredCode.text.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "").Replace("\r\n", "").Trim().ToString();

		if (String.Equals(printUserCode, printNewCode, StringComparison.InvariantCultureIgnoreCase))
        {

			Debug.Log("match");

			DoorButtonPR.GetComponent<Button>().interactable = true;

			CompileSucceededScreen.alpha = 1f;
			CompileSucceededScreen.blocksRaycasts = true;

			SaveFileButton.interactable = false;
			ResetFileButton.interactable = false;
			CancelButton.interactable = false;
		}
		else
		{
			Debug.Log("no match");

			CompileFailedScreen.alpha = 1f;
			CompileFailedScreen.blocksRaycasts = true;

			DoorButtonPR.GetComponent<Button>().interactable = false;

			SaveFileButton.interactable = false;
			ResetFileButton.interactable = false;
			CancelButton.interactable = false;



		}




	}



	public void EmailsButton()
	{
		// screen on, off
		ChangeScreen(EmailScreen, HomeScreen);
		ChangeScreen(EmailScreen, EmailViewScreen);
		ChangeScreen(EmailScreen, EmailViewScreen2);

		EvidenceCollider.enabled = false;
		EvidenceCollider2.enabled = false;
	}
	public void EmailViewButton(int emailno = 0)
	{
		if (emailno == 0)
		{
			ChangeScreen(EmailViewScreen, EmailScreen);
			EvidenceCollider.enabled = true;
			EvidenceCollider2.enabled = false;
		}
		if (emailno == 1)
		{
			ChangeScreen(EmailViewScreen2, EmailScreen);
			EvidenceCollider2.enabled = true;
			EvidenceCollider.enabled = false;
		}

	}





}
