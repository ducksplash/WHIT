using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 


public class phone : MonoBehaviour
{
	public GameObject MobilePhone;
	public Animator DeviceAnim;     
	public GameObject Fader;      
	public GameObject Clock;     
	public GameObject Player;   
	public GameObject Camera;     
	public Slider UnlockSlider;   
	public GameObject LockScreen;   
	public GameObject ContactsScreen;  
	public GameObject DiallerScreen;  
	public GameObject CallingScreen;   
	public GameObject HomeScreen;   
	public GameObject theDialler;     
	private Text DialBar;          
	public Text CallText;       
	public Text CallTitleText;  

	// contacts' nums
	private bool callingContact = false;
	public Text anonNum; 
	public Text kieronNum; 
	public Text maryNum; 
	public Text tomNum; 
	public Text workNum;          
	public bool PT = false;
    // Start is called before the first frame update
	private bool isLocked = true;

	
	
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
		DialBar = theDialler.GetComponent<Text>();
    }







	void changeScreen(GameObject useThisScreen)
	{
		Transform[] allScreens = MobilePhone.GetComponentsInChildren<Transform>();
		
		Transform[] useTheseScreens = useThisScreen.GetComponentsInChildren<Transform>();
		
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
			//child.gameObject.SetActive(false);
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
	
			
/* 			if (useThisScreen.GetComponent<CanvasGroup>())
			{
				
				useThisScreen.GetComponent<CanvasGroup>().alpha = 1.0f;
				useThisScreen.GetComponent<CanvasGroup>().blocksRaycasts = true;	
				

			}
			else if (useThisScreen.GetComponent<TMPro.TextMeshPro>())
			{
				// enable textmesh component
				useThisScreen.GetComponent<TMPro.TextMeshPro>().enabled = true;
			} */
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
		if (Input.GetButtonUp("Phone") && !PT)
		{
			DeviceAnim.SetTrigger("phoneOut");
			PT = true;
			Camera.GetComponent<FirstPersonLook>().enabled = false;
			Player.GetComponent<FirstPersonCollision>().enabled = false;
			Player.GetComponent<Jump>().enabled = false;
			MobilePhone.GetComponentInChildren<CanvasGroup>().alpha = 1.0f;
			
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;	
	
			
		}
		else if (Input.GetButtonUp("Phone") && PT)
		{
			DeviceAnim.SetTrigger("phoneAway");
			PT = false;			
			Camera.GetComponent<FirstPersonLook>().enabled = true;
			Player.GetComponent<FirstPersonCollision>().enabled = true;
			Player.GetComponent<Jump>().enabled = true;
			MobilePhone.GetComponentInChildren<CanvasGroup>().alpha = 0.0f;

			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

			}
		
	


// Dial By Keyb



if (Input.GetKeyUp("0") || Input.GetKeyUp("[0]") )
{
DialBar.text = DialBar.text+"0";
}
if (Input.GetKeyUp("1") || Input.GetKeyUp("[1]") )
{
DialBar.text = DialBar.text+"1";
}
if (Input.GetKeyUp("2") || Input.GetKeyUp("[2]") )
{
DialBar.text = DialBar.text+"2";
}
if (Input.GetKeyUp("3") || Input.GetKeyUp("[3]") )
{
DialBar.text = DialBar.text+"3";
}
if (Input.GetKeyUp("4") || Input.GetKeyUp("[4]") )
{
DialBar.text = DialBar.text+"4";
}
if (Input.GetKeyUp("5") || Input.GetKeyUp("[5]") )
{
DialBar.text = DialBar.text+"5";
}
if (Input.GetKeyUp("6") || Input.GetKeyUp("[6]") )
{
DialBar.text = DialBar.text+"6";
}
if (Input.GetKeyUp("7") || Input.GetKeyUp("[7]") )
{
DialBar.text = DialBar.text+"7";
}
if (Input.GetKeyUp("8") || Input.GetKeyUp("[8]") )
{
DialBar.text = DialBar.text+"8";
}
if (Input.GetKeyUp("9") || Input.GetKeyUp("[9]") )
{
DialBar.text = DialBar.text+"9";
}
if (Input.GetKeyUp("[*]") )
{
DialBar.text = DialBar.text+"*";
}
if (Input.GetKeyUp("#"))
{
DialBar.text = DialBar.text+"#";
}

if (Input.GetKeyUp("backspace") || Input.GetKeyUp("delete") )
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
	
	
}
