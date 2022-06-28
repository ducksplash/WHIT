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
		if (TravelCanvas.alpha == 0)
		{
			TravelCanvas.alpha = 0.9f;
			TravelCanvas.blocksRaycasts = true;
			GameMaster.INMENU = true;
			GameMaster.FROZEN = true;
			CompanionOpen = true;
		}
		else
		{
			TravelCanvas.alpha = 0f;
			TravelCanvas.blocksRaycasts = false;
			GameMaster.INMENU = false;
			GameMaster.FROZEN = false;
			CompanionOpen = false;
		}
	}


	public void ChangeScene(string SceneName)
	{
		GameMaster.INMENU = false;
		GameMaster.FROZEN = false;
		SceneManager.LoadScene(SceneName);
	}



}
