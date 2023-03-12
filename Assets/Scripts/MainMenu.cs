using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

	public CanvasGroup MainMenuFurniture;

	public GameObject PaperMenuParent;

	public CanvasGroup MainMenuRoot;
	public GameObject IdlePaper;
	public GameObject NewGamePaper;
	public GameObject ProloguePaper;
	public GameObject KeyboardPaper;
	public GameObject HelpPaper;
	public GameObject OptionsPaper;
	public GameObject CreditsPaper;
	public GameObject GameDataPaper;
	public GameObject DatafileDetailsPane;
	public GameObject DatafileDeletePane;
	public GameObject FileButtonParent;
	public GameObject LoadFileButton;
	public GameObject DeleteFileButton;
	public GameObject DeleteSureButton;
	public TextMeshProUGUI FileDateText;
	public GameObject QuitPaper;
	public Slider MusicSlider;
	public Slider SFXSlider;
	public TextMeshProUGUI MusicPercent;
	public TextMeshProUGUI SFXPercent;
	public string KeyboardButtonName;



	public GameObject KeyBindingDialog;
	public GameObject RestoreDefaultsDialog;
	public GameObject ReservedKeysDialog;
	public GameObject KeyBindingKeysParent;

	public TextMeshProUGUI PlayerForwardKeyText1;

	public TextMeshProUGUI PlayerBackwardKeyText1;

	public TextMeshProUGUI PlayerLeftKeyText1;

	public TextMeshProUGUI PlayerRightKeyText1;

	public TextMeshProUGUI PlayerJumpKeyText;
	public TextMeshProUGUI PlayerCrouchKeyText;
	public TextMeshProUGUI PlayerSprintKeyText;
	public TextMeshProUGUI PlayerRespawnKeyText;
	public TextMeshProUGUI PlayerPhoneKeyText;
	public TextMeshProUGUI PlayerCameraKeyText;
	public TextMeshProUGUI PlayerTorchKeyText;
	public TextMeshProUGUI PlayerSpecialKeyText;


	public CanvasGroup loadingpanel;
	public Image loadingbar;
	public TextMeshProUGUI loadingclock;

	
	public bool WaitingForKey;


	public List<string> AcceptableKeys;

	public CanvasGroup BadKeyMsg;

	public UnityEvent KeyWait;


	public string KeySetFunction;

	public void Start()
    {

		MusicPercent.text = "0%";
		SFXPercent.text = "0%";

		SetAcceptableKeys();

		LoadSavedKeys();

	}






	public void ChangeScreen(GameObject useThisScreen)
    {
		Transform[] allScreens = PaperMenuParent.GetComponentsInChildren<Transform>();

		Transform[] useTheseScreens = useThisScreen.GetComponentsInChildren<Transform>();




		var readyForNewScreen = false;


		foreach (Transform screen in allScreens)
		{


			if (screen.GetComponent<CanvasGroup>())
			{


				screen.GetComponent<CanvasGroup>().alpha = 0.0f;
				screen.GetComponent<CanvasGroup>().blocksRaycasts = false;



					if (screen.GetComponent<Image>())
					{
						if (screen.name.ToLower().Contains("idle"))
						{
							Debug.Log("got idle");
							screen.GetComponent<Image>().color = new Color(234, 234, 181, 255);
						}
						else
						{
							screen.GetComponent<Image>().color = Color.white;
						}
					}
				


				readyForNewScreen = true;



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


				if (thisScreen.GetComponent<Image>())
                {

					if (thisScreen.name.ToLower().Contains("idle"))
					{
						Debug.Log("got idle");
					}
					else
					{
						thisScreen.GetComponent<Image>().color = Color.white;
					}

                }

			}

		}
	}



	public void StartNewGame()
	{

		DoPaperMenu();

		ChangeScreen(NewGamePaper);

	}

	public void Prologue()
	{

		DoPaperMenu();

		ChangeScreen(ProloguePaper);

	}

	public void Help()
	{

		DoPaperMenu();

		ChangeScreen(HelpPaper);

	}

	public void Credits()
	{

		DoPaperMenu();

		ChangeScreen(CreditsPaper);

	}

	public void GameData()
	{

		DoPaperMenu();

		ChangeScreen(GameDataPaper);

		FileButtonParent.GetComponent<CanvasGroup>().blocksRaycasts = true;

		DatafileDetailsPane.GetComponent<CanvasGroup>().alpha = 0f;
		DatafileDetailsPane.GetComponent<CanvasGroup>().blocksRaycasts = false;


		DatafileDeletePane.GetComponent<CanvasGroup>().alpha = 0f;
		DatafileDeletePane.GetComponent<CanvasGroup>().blocksRaycasts = false;

	}


	public void SelectFile(int FileNumber)
	{
		FileButtonParent.GetComponent<CanvasGroup>().blocksRaycasts = false;

		Debug.Log(FileNumber);

		// Get date of file (we need to do this later when Save Games is made)
		// This is just a dummy setup to get all the methods ready so that SaveGame can be tested as it's developed.
		// Jam tomorrow etc

		var FileDate = "27/01/97";


		FileDateText.text = FileDate;
		DatafileDetailsPane.GetComponent<CanvasGroup>().alpha = 1f;
		DatafileDetailsPane.GetComponent<CanvasGroup>().blocksRaycasts = true;

		LoadFileButton.GetComponent<Button>().onClick.RemoveAllListeners();
		DeleteFileButton.GetComponent<Button>().onClick.RemoveAllListeners();


		LoadFileButton.GetComponent<Button>().onClick.AddListener(() => { LoadFile(FileNumber); });
		DeleteFileButton.GetComponent<Button>().onClick.AddListener(() => { DeleteFile(FileNumber); });



	}

	public void LoadFile(int FileNumber)
	{


		GameData();

	}

	public void DeleteFile(int FileNumber)
	{



		DatafileDetailsPane.GetComponent<CanvasGroup>().alpha = 0f;
		DatafileDetailsPane.GetComponent<CanvasGroup>().blocksRaycasts = false;

		DatafileDeletePane.GetComponent<CanvasGroup>().alpha = 1f;
		DatafileDeletePane.GetComponent<CanvasGroup>().blocksRaycasts = true;



		DeleteSureButton.GetComponent<Button>().onClick.RemoveAllListeners();
		DeleteSureButton.GetComponent<Button>().onClick.AddListener(() => { DeleteFileSure(FileNumber); });




		Debug.Log("File " + FileNumber + " Soon");

	}

	public void DeleteFileSure(int FileNumber)
	{



		DatafileDetailsPane.GetComponent<CanvasGroup>().alpha = 0f;
		DatafileDetailsPane.GetComponent<CanvasGroup>().blocksRaycasts = false;

		DatafileDeletePane.GetComponent<CanvasGroup>().alpha = 0f;
		DatafileDeletePane.GetComponent<CanvasGroup>().blocksRaycasts = false;


		// replace me with the actial action later 
		Debug.Log("File " + FileNumber + " Deleted");

	}










	public void Options()
	{

		DoPaperMenu();

		ChangeScreen(OptionsPaper);


		MusicSlider.GetComponentInChildren<Image>().color = Color.black;
		SFXSlider.GetComponentInChildren<Image>().color = Color.black;




	}




	public void UpdateMusicSlider()
	{
		//Displays the value of the slider in the console.
		Debug.Log(MusicSlider.value);

		MusicPercent.text = MusicSlider.value.ToString("#0") + "%";


	}

	public void UpdateSFXSlider()
	{
		//Displays the value of the slider in the console.
		Debug.Log(SFXSlider.value);

		SFXPercent.text = SFXSlider.value.ToString("#0") + "%";
	}

	public void DoPaperMenu()
	{



		MainMenuFurniture.GetComponent<CanvasGroup>().alpha = 0.01f;
		MainMenuFurniture.GetComponent<CanvasGroup>().blocksRaycasts = false;
	}

	public void ExitToMain()
	{


		ChangeScreen(IdlePaper);

		MainMenuFurniture.GetComponent<CanvasGroup>().alpha = 0.9f;
		MainMenuFurniture.GetComponent<CanvasGroup>().blocksRaycasts = true;
	}











	public void Keyboard()
	{

		WaitingForKey = false;
		BadKeyMsg.alpha = 0;

		DoPaperMenu();

		ChangeScreen(KeyboardPaper);


		MusicSlider.GetComponentInChildren<Image>().color = Color.black;
		SFXSlider.GetComponentInChildren<Image>().color = Color.black;

		// i keep forgetting to put these in.

		KeyBindingDialog.GetComponent<CanvasGroup>().alpha = 0f;
		KeyBindingDialog.GetComponent<CanvasGroup>().blocksRaycasts = false;

		RestoreDefaultsDialog.GetComponent<CanvasGroup>().alpha = 0f;
		RestoreDefaultsDialog.GetComponent<CanvasGroup>().blocksRaycasts = false;

		ReservedKeysDialog.GetComponent<CanvasGroup>().alpha = 0f;
		ReservedKeysDialog.GetComponent<CanvasGroup>().blocksRaycasts = false;



	}



	public void StartKeyboardListening(string GameFunction)
	{

		Debug.Log(GameFunction);

			BadKeyMsg.alpha = 0;


	    KeyboardButtonName = EventSystem.current.currentSelectedGameObject.name;


		KeyBindingKeysParent.GetComponent<CanvasGroup>().blocksRaycasts = false;

		KeyBindingDialog.GetComponent<CanvasGroup>().alpha = 1f;
		KeyBindingDialog.GetComponent<CanvasGroup>().blocksRaycasts = true;

		KeySetFunction = GameFunction;


		WaitingForKey = true;

	}




	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			ExitToMain();
		}


		if (WaitingForKey)
		{

			if (!Input.GetKey(KeyCode.Escape))
			{

				if (Input.anyKey)
				{

					foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
					{
						if (Input.GetKey(kcode))
                        {
							SaveKey(kcode, KeySetFunction);
                        }

					}

				}

			}

		}

	}



	public void SaveKey(KeyCode thisKey, string GameFunction)
	{
		if (!WaitingForKey)
		{
			return;
		}

		if (!AcceptableKeys.Contains(thisKey.ToString()))
		{
			BadKeyMsg.alpha = 1;
			Debug.Log("This key cannot be used, try another.");
			return;
		}

		bool success;
		switch (GameFunction.ToLower())
		{
			case "forward":
				success = InputManager.SetKey("up", thisKey, BadKeyMsg);
				PlayerForwardKeyText1.text = success ? thisKey.ToString() : PlayerForwardKeyText1.text;
				break;
			case "backward":
				success = InputManager.SetKey("down", thisKey, BadKeyMsg);
				PlayerBackwardKeyText1.text = success ? thisKey.ToString() : PlayerBackwardKeyText1.text;
				break;
			case "left":
				success = InputManager.SetKey("left", thisKey, BadKeyMsg);
				PlayerLeftKeyText1.text = success ? thisKey.ToString() : PlayerLeftKeyText1.text;
				break;
			case "right":
				success = InputManager.SetKey("right", thisKey, BadKeyMsg);
				PlayerRightKeyText1.text = success ? thisKey.ToString() : PlayerRightKeyText1.text;
				break;
			case "jump":
				success = InputManager.SetKey("jump", thisKey, BadKeyMsg);
				PlayerJumpKeyText.text = success ? thisKey.ToString() : PlayerJumpKeyText.text;
				break;
			case "crouch":
				success = InputManager.SetKey("crouch", thisKey, BadKeyMsg);
				PlayerCrouchKeyText.text = success ? thisKey.ToString() : PlayerCrouchKeyText.text;
				break;
			case "sprint":
				success = InputManager.SetKey("sprint", thisKey, BadKeyMsg);
				PlayerSprintKeyText.text = success ? thisKey.ToString() : PlayerSprintKeyText.text;
				break;
			case "respawn":
				success = InputManager.SetKey("respawn", thisKey, BadKeyMsg);
				PlayerRespawnKeyText.text = success ? thisKey.ToString() : PlayerRespawnKeyText.text;
				break;
			case "phone":
				success = InputManager.SetKey("phone", thisKey, BadKeyMsg);
				PlayerPhoneKeyText.text = success ? thisKey.ToString() : PlayerPhoneKeyText.text;
				break;
			case "camera":
				success = InputManager.SetKey("camera", thisKey, BadKeyMsg);
				PlayerCameraKeyText.text = success ? thisKey.ToString() : PlayerCameraKeyText.text;
				break;
			case "torch":
				success = InputManager.SetKey("torch", thisKey, BadKeyMsg);
				PlayerTorchKeyText.text = success ? thisKey.ToString() : PlayerTorchKeyText.text;
				break;
			case "special":
				success = InputManager.SetKey("special", thisKey, BadKeyMsg);
				PlayerSpecialKeyText.text = success ? thisKey.ToString() : PlayerSpecialKeyText.text;
				break;
			default:
				Debug.LogError("Unknown game function: " + GameFunction);
				return;
		}

		if (success)
		{
			BadKeyMsg.alpha = 0;
			KeyBindingKeysParent.GetComponent<CanvasGroup>().blocksRaycasts = true;
			KeyBindingDialog.GetComponent<CanvasGroup>().alpha = 0f;
			KeyBindingDialog.GetComponent<CanvasGroup>().blocksRaycasts = false;
			WaitingForKey = false;
		}
		else
		{
			BadKeyMsg.alpha = 1;
		}
	}

	void LoadSavedKeys()
	{
		// sets key label for menu/settings/keyboard setup

		PlayerForwardKeyText1.text = PlayerPrefs.HasKey("up") ? ((KeyCode)PlayerPrefs.GetInt("up")).ToString() : "W";
		PlayerBackwardKeyText1.text = PlayerPrefs.HasKey("down") ? ((KeyCode)PlayerPrefs.GetInt("down")).ToString() : "S";
		PlayerLeftKeyText1.text = PlayerPrefs.HasKey("left") ? ((KeyCode)PlayerPrefs.GetInt("left")).ToString() : "A";
		PlayerRightKeyText1.text = PlayerPrefs.HasKey("right") ? ((KeyCode)PlayerPrefs.GetInt("right")).ToString() : "D";
		PlayerJumpKeyText.text = PlayerPrefs.HasKey("jump") ? ((KeyCode)PlayerPrefs.GetInt("jump")).ToString() : "SPACE";
		PlayerCrouchKeyText.text = PlayerPrefs.HasKey("crouch") ? ((KeyCode)PlayerPrefs.GetInt("crouch")).ToString() : "LEFT CTRL";
		PlayerSprintKeyText.text = PlayerPrefs.HasKey("sprint") ? ((KeyCode)PlayerPrefs.GetInt("sprint")).ToString() : "LEFT SHIFT";
		PlayerRespawnKeyText.text = PlayerPrefs.HasKey("respawn") ? ((KeyCode)PlayerPrefs.GetInt("respawn")).ToString() : "R";
		PlayerPhoneKeyText.text = PlayerPrefs.HasKey("phone") ? ((KeyCode)PlayerPrefs.GetInt("phone")).ToString() : "P";
		PlayerCameraKeyText.text = PlayerPrefs.HasKey("camera") ? ((KeyCode)PlayerPrefs.GetInt("camera")).ToString() : "X";
		PlayerTorchKeyText.text = PlayerPrefs.HasKey("torch") ? ((KeyCode)PlayerPrefs.GetInt("torch")).ToString() : "H";
		PlayerSpecialKeyText.text = PlayerPrefs.HasKey("special") ? ((KeyCode)PlayerPrefs.GetInt("special")).ToString() : "Z";
	}


	public void SetDefaultKeys()
	{
			KeyBindingKeysParent.GetComponent<CanvasGroup>().blocksRaycasts = false;
			RestoreDefaultsDialog.GetComponent<CanvasGroup>().alpha = 1f;
			RestoreDefaultsDialog.GetComponent<CanvasGroup>().blocksRaycasts = true;
	}


	public void ReservedKeys()
	{
			KeyBindingKeysParent.GetComponent<CanvasGroup>().blocksRaycasts = false;
			ReservedKeysDialog.GetComponent<CanvasGroup>().alpha = 1f;
			ReservedKeysDialog.GetComponent<CanvasGroup>().blocksRaycasts = true;
	}


	public void SetDefaultConfirm()
	{


			KeyBindingKeysParent.GetComponent<CanvasGroup>().blocksRaycasts = true;
			RestoreDefaultsDialog.GetComponent<CanvasGroup>().alpha = 0f;
			RestoreDefaultsDialog.GetComponent<CanvasGroup>().blocksRaycasts = false;

			InputManager.SetDefault();
			LoadSavedKeys();

	}



	void SetAcceptableKeys()
	{
		AcceptableKeys.Add("A");
		AcceptableKeys.Add("B");
		AcceptableKeys.Add("C");
		AcceptableKeys.Add("D");
		AcceptableKeys.Add("E");
		AcceptableKeys.Add("F");
		AcceptableKeys.Add("G");
		AcceptableKeys.Add("H");
		AcceptableKeys.Add("I");
		AcceptableKeys.Add("J");
		AcceptableKeys.Add("K");
		AcceptableKeys.Add("L");
		AcceptableKeys.Add("M");
		AcceptableKeys.Add("N");
		AcceptableKeys.Add("O");
		AcceptableKeys.Add("P");
		AcceptableKeys.Add("Q");
		AcceptableKeys.Add("R");
		AcceptableKeys.Add("S");
		AcceptableKeys.Add("T");
		AcceptableKeys.Add("U");
		AcceptableKeys.Add("V");
		AcceptableKeys.Add("W");
		AcceptableKeys.Add("X");
		AcceptableKeys.Add("Y");
		AcceptableKeys.Add("Z");

		AcceptableKeys.Add("Backspace");
		AcceptableKeys.Add("Delete");
		AcceptableKeys.Add("Tab");
		AcceptableKeys.Add("Clear");
		AcceptableKeys.Add("Pause");
		AcceptableKeys.Add("Space");

		AcceptableKeys.Add("Insert");
		AcceptableKeys.Add("Home");
		AcceptableKeys.Add("End");
		AcceptableKeys.Add("PageUp");
		AcceptableKeys.Add("PageDown");

		AcceptableKeys.Add("Exclaim");
		AcceptableKeys.Add("DoubleQuote");
		AcceptableKeys.Add("Hash");
		AcceptableKeys.Add("Dollar");
		AcceptableKeys.Add("Percent");
		AcceptableKeys.Add("Ampersand");
		AcceptableKeys.Add("Quote");
		AcceptableKeys.Add("LeftParen");
		AcceptableKeys.Add("RightParen");
		AcceptableKeys.Add("Asterisk");

		AcceptableKeys.Add("Plus");
		AcceptableKeys.Add("Comma");
		AcceptableKeys.Add("Minus");
		AcceptableKeys.Add("Period");
		AcceptableKeys.Add("Slash");
		AcceptableKeys.Add("Colon");
		AcceptableKeys.Add("Semicolon");
		AcceptableKeys.Add("Less");
		AcceptableKeys.Add("Equals");
		AcceptableKeys.Add("Greater");

		AcceptableKeys.Add("Question");
		AcceptableKeys.Add("At");
		AcceptableKeys.Add("LeftBracket");
		AcceptableKeys.Add("Backslash");
		AcceptableKeys.Add("RightBracket");
		AcceptableKeys.Add("Caret");
		AcceptableKeys.Add("Underscore");
		AcceptableKeys.Add("BackQuote");
		AcceptableKeys.Add("LeftCurlyBracket");
		AcceptableKeys.Add("RightCurlyBracket");
		AcceptableKeys.Add("Pipe");

		AcceptableKeys.Add("LeftShift");
		AcceptableKeys.Add("LeftControl");
		AcceptableKeys.Add("RightAlt");
		AcceptableKeys.Add("LeftAlt");
		AcceptableKeys.Add("AltGr");

	}









//// start game



    public void StartGame()
	{
		GameMaster.INMENU = false;
		GameMaster.FROZEN = false;
		loadingpanel.alpha = 1;
		MainMenuRoot.alpha = 0;
		StartCoroutine(ChangeSceneAsync("NorasFlat"));
	}




	IEnumerator ChangeSceneAsync(string levelName)
	{


		AsyncOperation op = SceneManager.LoadSceneAsync(levelName);


		var buildDate = "";

		buildDate += System.DateTime.Now.ToString("dddd");
		buildDate += ", ";
		buildDate += System.DateTime.Now.ToString("MMMM d");
		buildDate += MonthDay(System.DateTime.Now.ToString("dd").ToString());
		buildDate += ", ";
		buildDate += System.DateTime.Now.ToString("yyyy");


		loadingclock.text = buildDate;


		while (!op.isDone)
		{
			loadingbar.fillAmount = Mathf.Clamp01(op.progress / .9f);


			yield return null;
		}
	}





	public string MonthDay(string day)
	{
		string nuNum = "th";
		if (int.Parse(day) < 11 || int.Parse(day) > 20)
		{
			day = day.ToCharArray()[^1].ToString();
			switch (day)
			{
				case "1":
					nuNum = "st";
					break;
				case "2":
					nuNum = "nd";
					break;
				case "3":
					nuNum = "rd";
					break;
			}
		}
		return nuNum;
	}






}



