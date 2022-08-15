using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

	public TextMeshProUGUI PaperDate;
	public TextMeshProUGUI PaperDate2;

	public CanvasGroup MainMenuFurniture;

	public GameObject PaperMenuParent;

	public GameObject IdlePaper;
	public GameObject NewGamePaper;
	public GameObject PrologueGamePaper;
	public GameObject ContinuePaper;
	public GameObject HelpPaper;
	public GameObject SettingsPaper;
	public GameObject QuitPaper;



	public void Start()
    {

		// put date on the paper

		var buildDate = "";

		buildDate += System.DateTime.Now.ToString("dddd");
		buildDate += ", ";
		buildDate += System.DateTime.Now.ToString("MMMM d");
		buildDate += MonthDay(System.DateTime.Now.ToString("dd").ToString());
		buildDate += ", ";
		buildDate += System.DateTime.Now.ToString("yyyy");


		PaperDate.text = buildDate;
		PaperDate2.text = buildDate;
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



    }


	public void DoPaperMenu()
	{


		ChangeScreen(NewGamePaper);

		MainMenuFurniture.GetComponent<CanvasGroup>().alpha = 0.01f;
		MainMenuFurniture.GetComponent<CanvasGroup>().blocksRaycasts = false;
	}

	public void ExitToMain()
	{


		ChangeScreen(IdlePaper);

		MainMenuFurniture.GetComponent<CanvasGroup>().alpha = 0.9f;
		MainMenuFurniture.GetComponent<CanvasGroup>().blocksRaycasts = true;
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



