using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class TravelCompanion : Singleton<TravelCompanion>, IPointerClickHandler
{
    public CanvasGroup TravelCanvas;

	public bool CompanionOpen;
	public GameObject Notepad;
	public CanvasGroup crosshair;
	public CanvasGroup evidencecompanion;
	public CanvasGroup loadingpanel;
	public Image loadingbar;
	public TextMeshProUGUI loadingclock;

	private void Start()
    {
        GameMaster.FROZEN = false;
		Notepad.SetActive(false);
		loadingbar.fillAmount = 0;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		
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
	}

	public void LaunchCompanion()
    {
	    
		if (GameMaster.THISLEVEL == "NorasFlat")
		{

			// if debuggery, override
			int evidenceOverride = GameMaster.Instance.DEBUGGERY ? 100 : GameMaster.EvidenceFound.Count;
			
			if (evidenceOverride > 0)
			{

				Debug.Log(GameMaster.THISLEVEL);

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
						evidencecompanion.GetComponent<CanvasGroup>().alpha = 0.0f;
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
						evidencecompanion.GetComponent<CanvasGroup>().alpha = 0.9f;
						crosshair.GetComponent<CanvasGroup>().alpha = 0.9f;

					}


				}
				else
				{
					var dialogstring = "I need my phone, my torch and my notepad.";
					DialogueManager.Instance.NewDialogue(Contacts.Nora.ToString(), dialogstring, 6);
					gameObject.GetComponent<Collider>().enabled = false;

				}

			}
			else
			{
				var dialogstring = "I haven't checked that I can photograph evidence with my phone yet, I should open the camera app and try on the wine bottle on my desk.";
				DialogueManager.Instance.NewDialogue(Contacts.Nora.ToString(), dialogstring, 6);
			}

		}
		else
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
				evidencecompanion.GetComponent<CanvasGroup>().alpha = 0.0f;
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
				evidencecompanion.GetComponent<CanvasGroup>().alpha = 0.9f;
				crosshair.GetComponent<CanvasGroup>().alpha = 0.9f;

			}
		}
		
	}


	public void ChangeScene(string SceneName)
	{
		Rigidbody rb = PlayerInstance.Instance.gameObject.GetComponentInParent<Rigidbody>();
		rb.isKinematic = true;
		rb.useGravity = false;
		GameMaster.INMENU = false;
		GameMaster.FROZEN = true;
		
		StartCoroutine(ChangeSceneAsync(SceneName));
	}


	public void ChangeSceneOffTheBooks(string SceneName)
	{
		
		GameMaster.INMENU = false;
		GameMaster.FROZEN = true;
		StartCoroutine(ChangeSceneAsync(SceneName));
	}



	IEnumerator ChangeSceneAsync(string levelName)
	{

		loadingpanel.alpha = 1;

		
		AsyncOperation op = SceneManager.LoadSceneAsync(levelName);


		var buildDate = "";

		buildDate += System.DateTime.Now.ToString("dddd");
		buildDate += ", ";
		buildDate += System.DateTime.Now.ToString("MMMM d");
		buildDate += MonthDay(System.DateTime.Now.ToString("dd").ToString());
		buildDate += ", ";
		buildDate += System.DateTime.Now.ToString("yyyy");


		loadingclock.text = buildDate;

		Transform PlayerTransform = PlayerInstance.Instance.gameObject.GetComponentInParent<Transform>();
		
		
		

		if (levelName.Equals(GameScene.NorasFlat.ToString()))
		{	
			PlayerTransform.position = GameMaster.Instance.SPAWNPOINTNORASFLAT;
			FirstPersonCollision.Instance.SpawnPoint = GameMaster.Instance.SPAWNPOINTNORASFLAT;
		}

		if (levelName.Equals(GameScene.TawleyMeats.ToString()))
		{	
			PlayerTransform.position = GameMaster.Instance.SPAWNPOINTTAWLEYMEATS;
			FirstPersonCollision.Instance.SpawnPoint = GameMaster.Instance.SPAWNPOINTTAWLEYMEATS;
		}

		if (levelName.Equals(GameScene.RoarkInside.ToString()))
		{	
			PlayerTransform.position = GameMaster.Instance.SPAWNPOINTROARKINSIDE;
			FirstPersonCollision.Instance.SpawnPoint = GameMaster.Instance.SPAWNPOINTROARKINSIDE;
		}

		if (levelName.Equals(GameScene.RoarkOutside.ToString()))
		{	
			PlayerTransform.position = GameMaster.Instance.SPAWNPOINTROARKOUTSIDE;
			FirstPersonCollision.Instance.SpawnPoint = GameMaster.Instance.SPAWNPOINTROARKOUTSIDE;
		}


		Rigidbody rb = PlayerInstance.Instance.gameObject.GetComponentInParent<Rigidbody>();
		// rb.isKinematic = false;
		// rb.useGravity = true;

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


	public enum GameScene
	{
		MainMenu,
		NorasFlat,
		TawleyMeats,
		RoarkOutside,
		RoarkInside
	}
	
	
	
}
