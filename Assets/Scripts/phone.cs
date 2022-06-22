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
	public GameObject CallingScreen;   
	public GameObject HomeScreen;   
	public GameObject theDialler;
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
	public bool PT = false;
    // Start is called before the first frame update
	private bool isLocked = true;
	public bool CameraOpen;
	public Light CameraLeftFlash;
	public Light CameraRightFlash;
	public Light TorchLight;
	public int resWidth = 600;
	public int resHeight = 1000;
	public Camera getCamera;


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
        DeviceAnim = MobilePhone.GetComponent<Animator>();
		DialBar = theDialler.GetComponentInChildren<Text>();

		//Debug.Log(Application.persistentDataPath);
	}







	void changeScreen(GameObject useThisScreen)
	{
		Transform[] allScreens = MobilePhone.GetComponentsInChildren<Transform>();
		
		Transform[] useTheseScreens = useThisScreen.GetComponentsInChildren<Transform>();

		PhoneCamera.GetComponent<Camera>().enabled = false;
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
						Debug.Log("TMPro.TextMeshProUGUI");
						readyForNewScreen = true;
					}
				}
				else
				{
					Debug.Log("don't got it");
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
					Debug.Log("CanvGrp.ON");

				}
				else if (thisScreen.GetComponent<TMPro.TextMeshPro>())
				{
					thisScreen.GetComponent<TMPro.TextMeshPro>().enabled = true;
					Debug.Log("TMPro.ON");
				}
				else if (thisScreen.GetComponent<TMPro.TextMeshProUGUI>())
				{
					thisScreen.GetComponent<TMPro.TextMeshProUGUI>().enabled = true;
					Debug.Log("TMPro.ON");
				}
				else
				{
					Debug.Log("don't got it");
				}
			}

		}
	}







    // Update is called once per frame
    void Update()
    {
				
		DateTime nowDateTime = DateTime.Now;
		string anHour = nowDateTime.Hour.ToString().PadLeft(2, '0');
		string aMinute = nowDateTime.Minute.ToString().PadLeft(2, '0');

		Clock.GetComponent<TMPro.TextMeshProUGUI>().text = anHour +":"+ aMinute;
		
		
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

			if (!PT)
			{				




				MobilePhone.transform.localPosition = new Vector3(MobilePhone.transform.localPosition.x, MobilePhone.transform.localPosition.y+1, MobilePhone.transform.localPosition.z);



				//Camera.GetComponent<FirstPersonLook>().enabled = false;
				//FirstPersonCollision.FROZEN = true;
				MobilePhone.GetComponentInChildren<CanvasGroup>().alpha = 1.0f;

				CrosshairCanvas.GetComponent<CanvasGroup>().alpha = 0.0f;
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;

				PT = true;
			}
			else
            {
				PT = false;
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

				changeScreen(HomeScreen);

				//Cursor.lockState = CursorLockMode.Locked;
				//Cursor.visible = false;
			}
			
		}




		// take photos

		if (Input.GetKey(KeyCode.X) && CameraReady)
		{

			TakePhoto();
		}









		// Dial By Keyb



if ((Input.GetKeyUp("0") || Input.GetKeyUp("[0]")) && PT)
{
DialBar.text = DialBar.text+"0";
}
if ((Input.GetKeyUp("1") || Input.GetKeyUp("[1]")) && PT)
{
DialBar.text = DialBar.text+"1";
}
if ((Input.GetKeyUp("2") || Input.GetKeyUp("[2]")) && PT)
{
DialBar.text = DialBar.text+"2";
}
if ((Input.GetKeyUp("3") || Input.GetKeyUp("[3]")) && PT)
{
DialBar.text = DialBar.text+"3";
}
if ((Input.GetKeyUp("4") || Input.GetKeyUp("[4]")) && PT)
{
DialBar.text = DialBar.text+"4";
}
if ((Input.GetKeyUp("5") || Input.GetKeyUp("[5]")) && PT)
{
DialBar.text = DialBar.text+"5";
}
if ((Input.GetKeyUp("6") || Input.GetKeyUp("[6]")) && PT)
{
DialBar.text = DialBar.text+"6";
}
if ((Input.GetKeyUp("7") || Input.GetKeyUp("[7]")) && PT)
{
DialBar.text = DialBar.text+"7";
}
if ((Input.GetKeyUp("8") || Input.GetKeyUp("[8]")) && PT)
{
DialBar.text = DialBar.text+"8";
}
if ((Input.GetKeyUp("9") || Input.GetKeyUp("[9]")) && PT)
{
DialBar.text = DialBar.text+"9";
}
if ((Input.GetKeyUp("[*]")) && PT)
{
DialBar.text = DialBar.text+"*";
}
if ((Input.GetKeyUp("#")) && PT)
{
DialBar.text = DialBar.text+"#";
}

if ((Input.GetKeyUp("backspace") || Input.GetKeyUp("delete")) && PT)
{
	if (DialBar.text.Length > 0)
	{
	String SubString = DialBar.text.Substring(0,DialBar.text.Length-1);
	DialBar.text = SubString;
	}
}

	}

	// Contacts menu incorporating CallScreen, Dialler.
	public void ContactsButton()
	{
		changeScreen(ContactsScreen);

		Debug.Log("contacts button");

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

		CameraReadyText.GetComponent<CanvasGroup>().alpha = 0;
		CameraSavedText.GetComponent<CanvasGroup>().alpha = 0;
		PhoneCamera.GetComponent<Camera>().enabled = true;
		CameraOpen = true;

		Debug.Log("camera button");

		



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
		CallText.text = "What did you expect to find?\nan IMEI number?\nFeckin eejit!";
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
		changeScreen(HomeScreen);
	}


	private void OnTriggerEnter(Collider other)
	{

		if (CameraOpen)
		{
			if (other.gameObject.layer == 13)
			{
				if (other.gameObject.GetComponent<Evidence>().PhotographableEvidence)
				{
					CameraReadyFrame.color = Color.green;
					CameraReadyText.GetComponent<CanvasGroup>().alpha = 1;
					CameraReady = true;
					Debug.Log("photographable evidence out of view");
					ObservedEvidence = other.gameObject;
				}
			}


			if (other.gameObject.layer == 11)
			{
				if (other.gameObject.GetComponent<Evidence>().PhotographableEvidence)
				{
					CameraReadyFrame.color = Color.green;
					CameraReadyText.GetComponent<CanvasGroup>().alpha = 1;
					CameraReady = true;
					Debug.Log("photographable evidence out of view");
					ObservedEvidence = other.gameObject;
				}
			}




		}

	}
	private void OnTriggerExit(Collider other)
	{
		if (CameraOpen)
		{
			if (other.gameObject.layer == 13)
			{

				CameraReadyFrame.color = Color.black;
				CameraReadyText.GetComponent<CanvasGroup>().alpha = 0;
				CameraReady = false;
				Debug.Log("photographable evidence out of view");
				ObservedEvidence = null;

			}


			if (other.gameObject.layer == 11)
			{

				CameraReadyFrame.color = Color.black;
				CameraReadyText.GetComponent<CanvasGroup>().alpha = 0;
				CameraReady = false;
				Debug.Log("photographable evidence out of view");
				ObservedEvidence = null;

			}
		}


	}



	public void TakePhoto()
	{


		if (ObservedEvidence != null)
		{


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

			ObservedEvidence.GetComponent<Evidence>().PhotographableEvidence = false;
			ObservedEvidence.GetComponent<Evidence>().EvidenceCollected = true;
			ObservedEvidence.GetComponent<Evidence>().CollectEvidence(nameForEvidenceFile);

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
