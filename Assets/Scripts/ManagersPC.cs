using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
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
	public GameObject FolderScreen;
	public GameObject FileViewScreen;
	
	// Get the Doors
	public GameObject StaffDoor;
	public GameObject FridgeDoorUpper;
	public GameObject FridgeDoorLower;
	public GameObject LowerMain;
	
	// Get DoorScreen Buttons
	public GameObject DoorButtonFG;
	public GameObject DoorButtonFB;
	public GameObject DoorButtonCA;
	public GameObject DoorButtonPR;
	public GameObject DoorButtonKG;
	public GameObject DoorButtonKB;
		
		
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


	// get "Files"
	
	public Texture[] theseFiles;	
	
	public Texture theSiteSurvey;
	public Texture theSitePhoto1;
	public Texture theSitePhoto2;
	public Texture theSitePhoto3;
	
	public GameObject theFilesRawImage;
	
	
	// OFF screen
	public GameObject screenCover;
	
	void Start()
    {
        thisComputerName = thisComputer.name;
		
		// Add cctv screens to list
		TextMeshPro DoorButtonFG = GetComponent<TextMeshPro>();
		TextMeshPro DoorButtonFB = GetComponent<TextMeshPro>();
		TextMeshPro DoorButtonCA = GetComponent<TextMeshPro>();
		TextMeshPro DoorButtonPR = GetComponent<TextMeshPro>();
		TextMeshPro DoorButtonKG = GetComponent<TextMeshPro>();
		TextMeshPro DoorButtonKB = GetComponent<TextMeshPro>();		
			
    }


    // Update is called once per frame
    void Update()
    {
		
		
		if (GameMaster.POWER_SUPPLY_ENABLED)
		{
			
			screenCover.GetComponent<Image>().enabled = false;
		
        	
			if (Input.GetMouseButtonDown(1))
			{  
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);  
				RaycastHit hit;  
				if (Physics.Raycast(ray, out hit)) 
				{  
					if (hit.distance <= 10.5f)
					{		

						if (hit.transform.name.Equals(thisComputerName))
						{  
								
							if (!usingComputer)
							{
								usingComputer = true;
								
								PlayerCamera.GetComponent<FirstPersonLook>().enabled = false;
								//Player.GetComponent<FirstPersonCollision>().enabled = true;
								//Player.GetComponent<Jump>().enabled = false;
								GameMaster.FROZEN = true;
				
								CrossHair.GetComponent<Canvas>().enabled = false;
								Cursor.lockState = CursorLockMode.None;
								Cursor.visible = true;	
								
							}
							else
							{
								PlayerCamera.GetComponent<FirstPersonLook>().enabled = true;
								//Player.GetComponent<FirstPersonCollision>().enabled = true;
								//Player.GetComponent<Jump>().enabled = true;
								GameMaster.FROZEN = false;

								CrossHair.GetComponent<Canvas>().enabled = true;
								Cursor.lockState = CursorLockMode.Locked;
								Cursor.visible = false;
								
								usingComputer = false;
							}
						}
					}  				
				}  
			} 
		}
		else
		{
			screenCover.GetComponent<Image>().enabled = false;
		}
		
		
		
    }
	
	
	
	// ChangeScreen function (screen on, screen off)
	void ChangeScreen(GameObject onScreen, GameObject offScreen)
	{
		
		// enable the "On Screen"
		onScreen.GetComponent<CanvasGroup>().alpha = 1.0f;
		onScreen.GetComponent<CanvasGroup>().blocksRaycasts = true;	

		// disable the "Off Screen"
		offScreen.GetComponent<CanvasGroup>().alpha = 0.0f;
		offScreen.GetComponent<CanvasGroup>().blocksRaycasts = false;	
		
	}
	
	public void GoHome()
	{
		// ChangeScreen function (screen on, screen off)
		ChangeScreen(HomeScreen, DoorScreen);
		
		ChangeScreen(HomeScreen, CCTVScreen);
		
		ChangeScreen(HomeScreen, FileScreen);
		
		ChangeScreen(HomeScreen, FileViewScreen);
		
		ChangeScreen(HomeScreen, FolderScreen);
			
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
	private void buttonOp(GameObject theButton, GameObject theDoor)
	{
		if (theDoor.GetComponent<innerDoors>().isLocked)
		{
			theButton.transform.parent.GetComponent<Image>().color = Color.green;
			theDoor.GetComponent<innerDoors>().isLocked = false;
			theDoor.GetComponent<innerDoors>().doLockedLights();
			
		}
		else
		{
			theButton.transform.parent.GetComponent<Image>().color = new Color32(77,76,164,100);
			theDoor.GetComponent<innerDoors>().isLocked = true;
			theDoor.GetComponent<innerDoors>().doLockedLights();
		}
	}
	
	
	public void FridgeG()
	{
		// ButtonOp takes a button (getting the image component) and a door (getting the isLocked variable		
		buttonOp(DoorButtonFG, FridgeDoorUpper);
	}	
	
	public void FridgeB()
	{
		// ButtonOp takes a button (getting the image component) and a door (getting the isLocked variable
		buttonOp(DoorButtonFB, FridgeDoorLower);
	}
	
	public void Canteen()
	{
		// ButtonOp takes a button (getting the image component) and a door (getting the isLocked variable
		buttonOp(DoorButtonCA, StaffDoor);
	}
	
	public void processingB()
	{
		// ButtonOp takes a button (getting the image component) and a door (getting the isLocked variable
		buttonOp(DoorButtonKB, LowerMain);
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
	public Texture[] getFilesArray()
	{
		Texture[] thePCFiles = {theSiteSurvey, theSitePhoto1, theSitePhoto2, theSitePhoto3};
		
		return thePCFiles;
	}	
	
	
	
	public void FilesButton()
	{
		// screen on, off
		ChangeScreen(FileScreen, HomeScreen);
		ChangeScreen(FileScreen, FileViewScreen);
		ChangeScreen(FileScreen, FolderScreen);
	}	
	
	public void FileFolderButton()
	{
		// screen on, off
		ChangeScreen(FolderScreen, FileScreen);
		ChangeScreen(FolderScreen, FileViewScreen);
		
	}
	
	public void FileViewButton(int thisFile)
	{
		// screen on, off
		ChangeScreen(FileViewScreen, FileScreen);
		ChangeScreen(FileViewScreen, FolderScreen);
		
		Texture[] theseFiles = getFilesArray();
		
		theFilesRawImage.GetComponent<RawImage>().texture = theseFiles[thisFile];
		
		
	}
	
	

}
