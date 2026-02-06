using System;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TravelCompanion : MonoBehaviour
{

    public CanvasGroup TravelCanvas;

	public bool CompanionOpen;
	private GameObject Notepad;
	public CanvasGroup crosshair;
	public CanvasGroup evidencecompanion;
	public CanvasGroup loadingpanel;
	public Image loadingbar;
	public Dictionary<GAMELEVEL, string> AvailableLocations = new Dictionary<GAMELEVEL, string>();
	public TextMeshProUGUI loadingclock;
	public bool TravelClicked;
	private float launchCooldown = 0.5f;
	public GameObject notepadButtonPrefab;
	public RectTransform scrollViewContent;
	public InputActionReference exitButton;
	public InputActionReference exitButtonPhoneKey;


	
	
	private void Start()
    {

	    SceneManager.sceneLoaded += OnSceneLoaded;
        GameMaster.Instance.FROZEN = false;
        Notepad = Player.Instance.TravelNotepad;

        Debug.Log("travel companion");
        
        exitButton?.action.Enable();
        exitButton.action.performed += CloseCompanionInput;
        exitButtonPhoneKey.action.performed += CloseCompanionInput;
        
		InitialiseLocations();
    }

	public bool CompanionIsOpen
	{
		get => CompanionOpen;
		set => CompanionOpen = value;
	}

	private void InitialiseLocations()
	{

		if (scrollViewContent == null) return;
		
		foreach (Transform child in scrollViewContent.transform)
		{
			Destroy(child.gameObject);
		}
		
		AvailableLocations.Clear();
		
        // order matters.
		AvailableLocations.Add(GAMELEVEL.TawleyMeats, "Tawley Meats");
		
		AvailableLocations.Add(GAMELEVEL.RoarkOutside, "Roark Microtech");
		
		AvailableLocations.Add(GAMELEVEL.NorasFlat, "\n...just go home");
		
		
		
		float verticalSpacing = 30f; // Adjust this value to change vertical spacing

		float contentHeight = scrollViewContent.rect.height; // Height of the content area
		float firstButtonHeight = notepadButtonPrefab.GetComponent<RectTransform>().rect.height; // Height of the first button

		// Calculate the initial Y position to start at the top
		float initialYPosition = contentHeight / 2f - firstButtonHeight / 2f;

		float currentYPosition = initialYPosition; // Initial Y position of the first button

		foreach (var availableLocation in AvailableLocations)
		{
			if (!availableLocation.Key.ToString().Equals(GameMaster.Instance.THISLEVEL.ToString()))
			{
				GameObject notePadButtonPrefabInstance = Instantiate(notepadButtonPrefab, scrollViewContent);

				RectTransform buttonTransform = notePadButtonPrefabInstance.GetComponent<RectTransform>();

				// Set the position of the button based on the current Y position
				buttonTransform.anchoredPosition = new Vector2(buttonTransform.anchoredPosition.x, currentYPosition);

				// Increment the Y position for the next button
				currentYPosition -= verticalSpacing;

				NotepadButton newButton = notePadButtonPrefabInstance.GetComponent<NotepadButton>();

				newButton.buttonText = availableLocation.Value;
				newButton.buttonTextElement.text = availableLocation.Value;
				newButton.targetScene = availableLocation.Key;

				Debug.Log(availableLocation.Key);
			}
		}
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}


	private void CloseCompanionInput(InputAction.CallbackContext callbackContext)
	{
		if (CompanionOpen) LaunchCompanion();
	}
	

	public void LaunchCompanion()
	{
		if (GameMaster.Instance.PHONEOUT || GameMaster.Instance.ONPC) return;

		if (!GameMaster.Instance.OnboardingManager.TESTEVIDENCECOLLECTED)
		{
			GameMaster.Instance.OnboardingManager.EvidenceNotCollected();
			return;
		}
		
		if (!GameMaster.Instance.OnboardingManager.ONBOARDINGCOMPLETE)
		{
			GameMaster.Instance.OnboardingManager.NotReadyYet();
			return;
		}
		
		
		if (!CompanionOpen)
		{
			Debug.Log("not open");
			Notepad.transform.localPosition = new Vector3(Notepad.transform.localPosition.x, Notepad.transform.localPosition.y + 1, Notepad.transform.localPosition.z);

			Notepad.SetActive(true);
			TravelCanvas.alpha = 1f;
			TravelCanvas.blocksRaycasts = true;
			GameMaster.Instance.INMENU = true;
			GameMaster.Instance.FROZEN = true;
			CompanionOpen = true;
			evidencecompanion.GetComponent<CanvasGroup>().alpha = 0.0f;
			crosshair.GetComponent<CanvasGroup>().alpha = 0.0f;

			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		else
		{
			Debug.Log("open");
			Notepad.transform.localPosition = new Vector3(Notepad.transform.localPosition.x, Notepad.transform.localPosition.y - 1, Notepad.transform.localPosition.z);

			Notepad.SetActive(false);

			TravelCanvas.alpha = 0f;
			TravelCanvas.blocksRaycasts = false;
			GameMaster.Instance.INMENU = false;
			GameMaster.Instance.FROZEN = false;
			CompanionOpen = false;
			evidencecompanion.GetComponent<CanvasGroup>().alpha = 0.9f;
			crosshair.GetComponent<CanvasGroup>().alpha = 0.9f;
		
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

		}
	}




	public void ChangeScene(GAMELEVEL SceneName)
	{
		Rigidbody rb = Player.Instance.gameObject.GetComponentInParent<Rigidbody>();
		// rb.isKinematic = false;
		// rb.useGravity = false;
		
		GameMaster.Instance.INMENU = false;
		GameMaster.Instance.FROZEN = true;
		StartCoroutine(ChangeSceneAsync(SceneName));
	}


	public void ChangeSceneOffTheBooks(GAMELEVEL SceneName)
	{
		Rigidbody rb = Player.Instance.gameObject.GetComponentInParent<Rigidbody>();
		
		// rb.isKinematic = false;
		// rb.useGravity = false;
		
		GameMaster.Instance.INMENU = false;
		GameMaster.Instance.FROZEN = true;
		StartCoroutine(ChangeSceneAsync(SceneName));
	}



	IEnumerator ChangeSceneAsync(GAMELEVEL levelName)
	{
		
		loadingpanel.alpha = 1;

		Debug.Log("Loading: "+levelName.ToString());
		
		AsyncOperation op = SceneManager.LoadSceneAsync(levelName.ToString());


		var buildDate = "";

		buildDate += System.DateTime.Now.ToString("dddd");
		buildDate += ", ";
		buildDate += System.DateTime.Now.ToString("MMMM d");
		buildDate += MonthDay(System.DateTime.Now.ToString("dd").ToString());
		buildDate += ", ";
		buildDate += System.DateTime.Now.ToString("yyyy");


		loadingclock.text = buildDate;

		Transform PlayerTransform = Player.Instance.gameObject.GetComponentInParent<Transform>();


		Debug.Log("setting pos now");
		

		if (levelName == GAMELEVEL.NorasFlat)
		{	
			PlayerTransform.position = GameMaster.Instance.SPAWNPOINTNORASFLAT;
			Player.Instance.SpawnPoint = GameMaster.Instance.SPAWNPOINTNORASFLAT;
		}

		if (levelName == GAMELEVEL.TawleyMeats)
		{	
			PlayerTransform.position = GameMaster.Instance.SPAWNPOINTTAWLEYMEATS;
			Player.Instance.SpawnPoint = GameMaster.Instance.SPAWNPOINTTAWLEYMEATS;
		}

		if (levelName == GAMELEVEL.RoarkInside)
		{	
			PlayerTransform.position = GameMaster.Instance.SPAWNPOINTROARKINSIDE;
			Player.Instance.SpawnPoint = GameMaster.Instance.SPAWNPOINTROARKINSIDE;
		}

		if (levelName == GAMELEVEL.RoarkOutside)
		{	
			PlayerTransform.position = GameMaster.Instance.SPAWNPOINTROARKOUTSIDE;
			Player.Instance.SpawnPoint = GameMaster.Instance.SPAWNPOINTROARKOUTSIDE;
		}

		Time.timeScale = 0; // or the player falls out the world
		

		while (!op.isDone)
		{
			loadingbar.fillAmount = Mathf.Clamp01(op.progress / .9f);
			yield return null;
		}

		GameMaster.Instance.THISLEVEL = levelName;
		GameMaster.Instance.DialogueManager.queueDropFlag = true;
		loadingpanel.alpha = 0;
		Time.timeScale = 1;
		InitialiseLocations();
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		GameMaster.Instance.INMENU = false;
		GameMaster.Instance.FROZEN = false;
		if (CompanionOpen) LaunchCompanion();
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
