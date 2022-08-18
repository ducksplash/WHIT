using TMPro;
using UnityEngine;
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
	public GameObject QuitPaper;
	public Slider MusicSlider;
	public Slider SFXSlider;

	public TextMeshProUGUI MusicPercent;
	public TextMeshProUGUI SFXPercent;



	public void Start()
    {

		MusicPercent.text = "0%";
		SFXPercent.text = "0%";

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
							screen.GetComponent<Image>().color = new Color(234,234,181,255);
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






}



