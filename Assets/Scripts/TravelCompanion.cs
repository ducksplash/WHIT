using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TravelCompanion : MonoBehaviour
{
    public CanvasGroup TravelCanvas;

	public bool CompanionOpen;
	public GameObject Notepad;
	public CanvasGroup crosshair;
	public TextMeshProUGUI ButtonLocationOneText;



	private void Start()
    {
		Notepad.SetActive(false);
    }




    void Update()
	{

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
						if (hit.distance <= 11f)
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


		if (CompanionOpen)
		{
			if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.P))
			{
				LaunchCompanion();
			}
		}
	}

	void LaunchCompanion()
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


	public void ChangeScene(string SceneName)
	{
		GameMaster.INMENU = false;
		GameMaster.FROZEN = false;
		SceneManager.LoadScene(SceneName);
	}






}
