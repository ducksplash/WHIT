using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class TravelCompanion : MonoBehaviour, IPointerClickHandler
{
    public CanvasGroup TravelCanvas;

	public bool CompanionOpen;
	public GameObject Notepad;
	public CanvasGroup crosshair;
	public GameObject Player;
	public CanvasGroup loadingpanel;
	public Image loadingbar;

	private void Start()
    {
		Notepad.SetActive(false);
		loadingbar.fillAmount = 0;

	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Debug.Log("anything??");
		if (eventData.button == PointerEventData.InputButton.Right)
		{
			if (CompanionOpen)
			{
				LaunchCompanion();
			}
		}
	}


	void Update()
	{




			// this first

			if (CompanionOpen)
			{
				if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.P))
				{

					LaunchCompanion();

				}
			}

			// then this

			if (!CompanionOpen)
			{
				if (Input.GetMouseButtonDown(1))
				{


					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					RaycastHit hit;
					if (Physics.Raycast(ray, out hit))
					{
						if (hit.transform.gameObject == gameObject)
						{
							if (hit.distance <= 3f && hit.distance > 1.2f)
							{
								LaunchCompanion();
							}
						}
						else
						{
							Debug.Log("not travel door");
						}

					}
				}
			}

		





	}

	void LaunchCompanion()
    {




		var itemsfound = 0;


		if (GameMaster.PHONECOLLECTED)
		{
			itemsfound += 1;
		}


		if (GameMaster.TORCHCOLLECTED)
		{

			itemsfound += 1;
		}




		if (GameMaster.NOTEPADCOLLECTED)
		{
			itemsfound += 1;
		}






		if (itemsfound > 2)
		{



			if (!Notepad.activeSelf)
			{
				Notepad.transform.localPosition = new Vector3(Notepad.transform.localPosition.x, Notepad.transform.localPosition.y + 1, Notepad.transform.localPosition.z);



				Notepad.SetActive(true);
				TravelCanvas.alpha = 1f;
				TravelCanvas.blocksRaycasts = true;
				GameMaster.INMENU = true;
				GameMaster.FROZEN = true;
				CompanionOpen = true;
				crosshair.GetComponent<CanvasGroup>().alpha = 0.0f;

			}
			else
			{
				Notepad.transform.localPosition = new Vector3(Notepad.transform.localPosition.x, Notepad.transform.localPosition.y - 1, Notepad.transform.localPosition.z);

				Notepad.SetActive(false);

				TravelCanvas.alpha = 0f;
				TravelCanvas.blocksRaycasts = false;
				GameMaster.INMENU = false;
				GameMaster.FROZEN = false;
				CompanionOpen = false;
				crosshair.GetComponent<CanvasGroup>().alpha = 0.9f;

			}


		}
		else
		{
			var dialogstring = "I need my phone, my torch and my notepad.";
			Player.GetComponent<DialogueManager>().NewDialogue("NORA", dialogstring, 3, gameObject);
			gameObject.GetComponent<Collider>().enabled = false;

        }






	}


	public void ChangeScene(string SceneName)
	{
		GameMaster.INMENU = false;
		GameMaster.FROZEN = false;
		StartCoroutine(ChangeSceneAsync(SceneName));
	}




	IEnumerator ChangeSceneAsync(string levelName)
	{

		loadingpanel.alpha = 1;

		AsyncOperation op = SceneManager.LoadSceneAsync(levelName);



		while (!op.isDone)
		{
			loadingbar.fillAmount = Mathf.Clamp01(op.progress / .9f);


			yield return null;
		}
	}









}
