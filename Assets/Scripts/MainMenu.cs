using TMPro;
using UnityEditor.Events;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

	public CanvasGroup MainMenuFurniture;

	public GameObject PaperMenuParent;

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


	public TextMeshProUGUI PlayerForwardKeyText1;
	public TextMeshProUGUI PlayerForwardKeyText2;

	public TextMeshProUGUI PlayerBackwardKeyText1;
	public TextMeshProUGUI PlayerBackwardKeyText2;

	public TextMeshProUGUI PlayerLeftKeyText1;
	public TextMeshProUGUI PlayerLeftKeyText2;

	public TextMeshProUGUI PlayerRightKeyText1;
	public TextMeshProUGUI PlayerRightKeyText2;


	public bool WaitingForKey;


	public List<string> AcceptableKeys;


	public void Start()
    {

		MusicPercent.text = "0%";
		SFXPercent.text = "0%";

		SetAcceptableKeys();

		SetDefaultKeys();

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
					Debug.Log("A key or mouse click has been detected");
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



		Debug.Log("File " + FileNumber + " Deleted");

	}










	public void Options()
	{

		DoPaperMenu();

		ChangeScreen(OptionsPaper);


		MusicSlider.GetComponentInChildren<Image>().color = Color.black;
		SFXSlider.GetComponentInChildren<Image>().color = Color.black;




	}


	public void Keyboard()
	{

		DoPaperMenu();

		ChangeScreen(KeyboardPaper);



		MusicSlider.GetComponentInChildren<Image>().color = Color.black;
		SFXSlider.GetComponentInChildren<Image>().color = Color.black;




	}



	public void StartKeyboardListening(string GameFunction)
	{

		Debug.Log(GameFunction);


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


















	void SetDefaultKeys()
	{

		if (PlayerPrefs.GetString("PlayerForwardKey1") == "")
		{
			PlayerForwardKeyText1.text = "W";
		}

		if (PlayerPrefs.GetString("PlayerForwardKey2") == "")
		{
			char upArrow = '\u2191';
			PlayerForwardKeyText2.text = upArrow.ToString();
		}




		if (PlayerPrefs.GetString("PlayerBackwardKey1") == "")
		{
			PlayerBackwardKeyText1.text = "S";
		}

		if (PlayerPrefs.GetString("PlayerBackwardKey2") == "")
		{
			char upArrow = '\u2193';
			PlayerBackwardKeyText2.text = upArrow.ToString();
		}





		if (PlayerPrefs.GetString("PlayerLeftKey1") == "")
		{
			PlayerLeftKeyText1.text = "A";
		}

		if (PlayerPrefs.GetString("PlayerLeftKey2") == "")
		{
			char upArrow = '\u2190';
			PlayerLeftKeyText2.text = upArrow.ToString();
		}






		if (PlayerPrefs.GetString("PlayerRightKey1") == "")
		{
			PlayerRightKeyText1.text = "D";
		}

		if (PlayerPrefs.GetString("PlayerRightKey2") == "")
		{
			char upArrow = '\u2192';
			PlayerRightKeyText2.text = upArrow.ToString();
		}







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
		AcceptableKeys.Add("Return");
		AcceptableKeys.Add("Pause");
		AcceptableKeys.Add("Space");

		AcceptableKeys.Add("UpArrow");
		AcceptableKeys.Add("DownArrow");
		AcceptableKeys.Add("LeftArrow");
		AcceptableKeys.Add("RightArrow");
		AcceptableKeys.Add("Insert");
		AcceptableKeys.Add("Home");
		AcceptableKeys.Add("End");
		AcceptableKeys.Add("PageUp");
		AcceptableKeys.Add("PageDown");

		AcceptableKeys.Add("Alpha0");
		AcceptableKeys.Add("Alpha1");
		AcceptableKeys.Add("Alpha2");
		AcceptableKeys.Add("Alpha3");
		AcceptableKeys.Add("Alpha4");
		AcceptableKeys.Add("Alpha5");
		AcceptableKeys.Add("Alpha6");
		AcceptableKeys.Add("Alpha7");
		AcceptableKeys.Add("Alpha8");
		AcceptableKeys.Add("Alpha9");

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

		AcceptableKeys.Add("RightShift");
		AcceptableKeys.Add("LeftShift");
		AcceptableKeys.Add("RightControl");
		AcceptableKeys.Add("LeftControl");
		AcceptableKeys.Add("RightAlt");
		AcceptableKeys.Add("LeftAlt");
		AcceptableKeys.Add("AltGr");

	}
}



