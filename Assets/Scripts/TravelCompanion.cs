using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Image = UnityEngine.UI.Image;

public class TravelCompanion : Singleton<TravelCompanion>//, IPointerClickHandler
{
 //    public CanvasGroup TravelCanvas;
 //
	// [SerializeField] private bool CompanionOpen;
	// private GameObject Notepad;
	// public CanvasGroup crosshair;
	// public CanvasGroup evidencecompanion;
	// public CanvasGroup loadingpanel;
	// public Image loadingbar;
	// public Dictionary<GameScene, string> AvailableLocations = new Dictionary<GameScene, string>();
	// public TextMeshProUGUI loadingclock;
	// public bool TravelClicked;
	// private bool launchAvailable = true; // Flag to track if launch is available
	// private float launchCooldown = 0.5f;
	// public GameObject notepadButtonPrefab;
	// public RectTransform scrollViewContent;	
	// private void Start()
 //    {
	//     SceneManager.sceneLoaded += OnSceneLoaded;
 //        GameMaster.FROZEN = false;
 //        Notepad = PlayerInstance.Instance.TravelNotepad;
 //
 //        
	// 	//Notepad.SetActive(false);
	// 	//TravelClicked = false;
	// 	//loadingbar.fillAmount = 0;
 //
	// 	InitialiseLocations();
 //    }
 //
	// public bool CompanionIsOpen
	// {
	// 	get => CompanionOpen;
	// 	set => CompanionOpen = value;
	// }
 //
	// private void InitialiseLocations()
	// {
	// 	
	// 	foreach (Transform child in scrollViewContent.transform)
	// 	{
	// 		Destroy(child.gameObject);
	// 	}
	// 	
	// 	AvailableLocations.Clear();
	// 	
 //        // order matters.
	// 	AvailableLocations.Add(GameScene.TawleyMeats, "Tawley Meats");
	// 	
	// 	AvailableLocations.Add(GameScene.RoarkOutside, "Roark Microtech");
	// 	
	// 	AvailableLocations.Add(GameScene.NorasFlat, "\n...just go home");
	// 	
	// 	
	// 	
	// 	float verticalSpacing = 30f; // Adjust this value to change vertical spacing
 //
	// 	float contentHeight = scrollViewContent.rect.height; // Height of the content area
	// 	float firstButtonHeight = notepadButtonPrefab.GetComponent<RectTransform>().rect.height; // Height of the first button
 //
	// 	// Calculate the initial Y position to start at the top
	// 	float initialYPosition = contentHeight / 2f - firstButtonHeight / 2f;
 //
	// 	float currentYPosition = initialYPosition; // Initial Y position of the first button
 //
	// 	foreach (var availableLocation in AvailableLocations)
	// 	{
	// 		if (!availableLocation.Key.ToString().Equals(GameMaster.THISLEVEL))
	// 		{
	// 			GameObject notePadButtonPrefabInstance = Instantiate(notepadButtonPrefab, scrollViewContent);
 //
	// 			RectTransform buttonTransform = notePadButtonPrefabInstance.GetComponent<RectTransform>();
 //
	// 			// Set the position of the button based on the current Y position
	// 			buttonTransform.anchoredPosition = new Vector2(buttonTransform.anchoredPosition.x, currentYPosition);
 //
	// 			// Increment the Y position for the next button
	// 			currentYPosition -= verticalSpacing;
 //
	// 			NotepadButton newButton = notePadButtonPrefabInstance.GetComponent<NotepadButton>();
 //
	// 			newButton.buttonText = availableLocation.Value;
	// 			newButton.buttonTextElement.text = availableLocation.Value;
	// 			newButton.targetScene = availableLocation.Key;
 //
	// 			Debug.Log(availableLocation.Key);
	// 		}
	// 	}
	// }
 //
	// private void OnDestroy()
	// {
	// 	// Unsubscribe from the sceneLoaded event
	// 	SceneManager.sceneLoaded -= OnSceneLoaded;
	// }
 //
	// void Update()
	// {
	// 		// this first
	// 	if (CompanionOpen)
	// 	{
	// 		if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.P))
	// 		{
	// 			LaunchCompanion();
	// 		}
	// 	}
	// }
 //
	// public void LaunchCompanion()
	// {
	// 	if (launchAvailable)
	// 	{
	// 		Debug.Log("launch companion");
	// 		if (GameMaster.THISLEVEL.Equals(GameScene.NorasFlat.ToString()))
	// 		{
 //
	// 			// if debuggery, override
	// 			int evidenceOverride = GameMaster.Instance.DEBUGGERY ? 100 : GameMaster.EvidenceFound.Count;
 //
	// 			if (evidenceOverride > 0)
	// 			{
 //
 //
	// 				var itemsfound = 0;
 //
 //
	// 				if (GameMaster.PHONECOLLECTED)
	// 				{
	// 					itemsfound += 1;
	// 				}
 //
 //
	// 				if (GameMaster.TORCHCOLLECTED)
	// 				{
 //
	// 					itemsfound += 1;
	// 				}
 //
 //
 //
 //
	// 				if (GameMaster.NOTEPADCOLLECTED)
	// 				{
	// 					itemsfound += 1;
	// 				}
 //
 //
	// 				if (itemsfound > 2)
	// 				{
 //
 //
 //
	// 					if (!CompanionOpen)
	// 					{
	// 						Notepad.transform.localPosition = new Vector3(Notepad.transform.localPosition.x,
	// 							Notepad.transform.localPosition.y + 1, Notepad.transform.localPosition.z);
 //
 //
 //
	// 						Notepad.SetActive(true);
	// 						TravelCanvas.alpha = 1f;
	// 						TravelCanvas.blocksRaycasts = true;
	// 						GameMaster.INMENU = true;
	// 						GameMaster.FROZEN = true;
	// 						CompanionOpen = true;
	// 						evidencecompanion.GetComponent<CanvasGroup>().alpha = 0.0f;
	// 						crosshair.GetComponent<CanvasGroup>().alpha = 0.0f;
 //
	// 					}
	// 					else
	// 					{
	// 						Notepad.transform.localPosition = new Vector3(Notepad.transform.localPosition.x,
	// 							Notepad.transform.localPosition.y - 1, Notepad.transform.localPosition.z);
 //
	// 						Notepad.SetActive(false);
 //
	// 						TravelCanvas.alpha = 0f;
	// 						TravelCanvas.blocksRaycasts = false;
	// 						GameMaster.INMENU = false;
	// 						GameMaster.FROZEN = false;
	// 						CompanionOpen = false;
	// 						evidencecompanion.GetComponent<CanvasGroup>().alpha = 0.9f;
	// 						crosshair.GetComponent<CanvasGroup>().alpha = 0.9f;
 //
	// 					}
 //
 //
	// 				}
	// 				else
	// 				{
	// 					var dialogstring = "I need my phone, my torch and my notepad.";
	// 					DialogueManager.Instance.NewDialogue(Contacts.Nora.ToString(), dialogstring, 6);
	// 					gameObject.GetComponent<Collider>().enabled = false;
 //
	// 				}
 //
	// 			}
	// 			else
	// 			{
	// 				var dialogstring =
	// 					"I haven't checked that I can photograph evidence with my phone yet, I should open the camera app and try on the wine bottle on my desk.";
	// 				DialogueManager.Instance.NewDialogue(Contacts.Nora.ToString(), dialogstring, 6);
	// 			}
 //
	// 		}
	// 		else
	// 		{
	// 			Debug.Log("not nora's flat");
 //
	// 			if (!CompanionOpen)
	// 			{
	// 				Debug.Log("not open");
	// 				Notepad.transform.localPosition = new Vector3(Notepad.transform.localPosition.x,
	// 					Notepad.transform.localPosition.y + 1, Notepad.transform.localPosition.z);
 //
 //
	// 				Notepad.SetActive(true);
	// 				TravelCanvas.alpha = 1f;
	// 				TravelCanvas.blocksRaycasts = true;
	// 				GameMaster.INMENU = true;
	// 				GameMaster.FROZEN = true;
	// 				CompanionOpen = true;
	// 				evidencecompanion.GetComponent<CanvasGroup>().alpha = 0.0f;
	// 				crosshair.GetComponent<CanvasGroup>().alpha = 0.0f;
 //
	// 			}
	// 			else
	// 			{
	// 				Debug.Log("open");
	// 				Notepad.transform.localPosition = new Vector3(Notepad.transform.localPosition.x,
	// 					Notepad.transform.localPosition.y - 1, Notepad.transform.localPosition.z);
 //
	// 				Notepad.SetActive(false);
 //
	// 				TravelCanvas.alpha = 0f;
	// 				TravelCanvas.blocksRaycasts = false;
	// 				GameMaster.INMENU = false;
	// 				GameMaster.FROZEN = false;
	// 				CompanionOpen = false;
	// 				evidencecompanion.GetComponent<CanvasGroup>().alpha = 0.9f;
	// 				crosshair.GetComponent<CanvasGroup>().alpha = 0.9f;
 //
	// 			}
	// 		}
	// 		
	// 		
	// 		StartCoroutine(LaunchCooldown());
	// 	}
	// }
	//
	//
	// IEnumerator LaunchCooldown()
	// {
	// 	// Disable launch availability
	// 	launchAvailable = false;
 //
	// 	// Wait for cooldown duration
	// 	yield return new WaitForSeconds(launchCooldown);
 //
	// 	// Enable launch availability
	// 	launchAvailable = true;
	// }
 //
	// public void ChangeScene(string SceneName)
	// {
	// 	Rigidbody rb = PlayerInstance.Instance.gameObject.GetComponentInParent<Rigidbody>();
	// 	// rb.isKinematic = false;
	// 	// rb.useGravity = false;
	// 	
	// 	GameMaster.INMENU = false;
	// 	GameMaster.FROZEN = true;
	// 	StartCoroutine(ChangeSceneAsync(SceneName));
	// }
 //
 //
	// public void ChangeSceneOffTheBooks(string SceneName)
	// {
	// 	Rigidbody rb = PlayerInstance.Instance.gameObject.GetComponentInParent<Rigidbody>();
	// 	
	// 	// rb.isKinematic = false;
	// 	// rb.useGravity = false;
	// 	
	// 	GameMaster.INMENU = false;
	// 	GameMaster.FROZEN = true;
	// 	StartCoroutine(ChangeSceneAsync(SceneName));
	// }
 //
 //
 //
	// IEnumerator ChangeSceneAsync(string levelName)
	// {
 //
	// 	
	// 	LaunchCompanion();
	// 	CompanionOpen = false;
	// 	
	// 	loadingpanel.alpha = 1;
 //
	// 	Debug.Log("Loading: "+levelName);
	// 	
	// 	AsyncOperation op = SceneManager.LoadSceneAsync(levelName);
 //
 //
	// 	var buildDate = "";
 //
	// 	buildDate += System.DateTime.Now.ToString("dddd");
	// 	buildDate += ", ";
	// 	buildDate += System.DateTime.Now.ToString("MMMM d");
	// 	buildDate += MonthDay(System.DateTime.Now.ToString("dd").ToString());
	// 	buildDate += ", ";
	// 	buildDate += System.DateTime.Now.ToString("yyyy");
 //
 //
	// 	loadingclock.text = buildDate;
 //
	// 	Transform PlayerTransform = PlayerInstance.Instance.gameObject.GetComponentInParent<Transform>();
 //
 //
	// 	Debug.Log("setting pos now");
	// 	
 //
	// 	if (levelName.Equals(GameScene.NorasFlat.ToString()))
	// 	{	
	// 		PlayerTransform.position = GameMaster.Instance.SPAWNPOINTNORASFLAT;
	// 		FirstPersonCollision.Instance.SpawnPoint = GameMaster.Instance.SPAWNPOINTNORASFLAT;
	// 	}
 //
	// 	if (levelName.Equals(GameScene.TawleyMeats.ToString()))
	// 	{	
	// 		PlayerTransform.position = GameMaster.Instance.SPAWNPOINTTAWLEYMEATS;
	// 		FirstPersonCollision.Instance.SpawnPoint = GameMaster.Instance.SPAWNPOINTTAWLEYMEATS;
	// 	}
 //
	// 	if (levelName.Equals(GameScene.RoarkInside.ToString()))
	// 	{	
	// 		PlayerTransform.position = GameMaster.Instance.SPAWNPOINTROARKINSIDE;
	// 		FirstPersonCollision.Instance.SpawnPoint = GameMaster.Instance.SPAWNPOINTROARKINSIDE;
	// 	}
 //
	// 	if (levelName.Equals(GameScene.RoarkOutside.ToString()))
	// 	{	
	// 		PlayerTransform.position = GameMaster.Instance.SPAWNPOINTROARKOUTSIDE;
	// 		FirstPersonCollision.Instance.SpawnPoint = GameMaster.Instance.SPAWNPOINTROARKOUTSIDE;
	// 	}
 //
	// 	Time.timeScale = 0; // or the player falls out the world
	// 	
 //
	// 	while (!op.isDone)
	// 	{
	// 		loadingbar.fillAmount = Mathf.Clamp01(op.progress / .9f);
	// 		yield return null;
	// 	}
 //
	// 	GameMaster.THISLEVEL = levelName;
	// 	DialogueManager.Instance.queueDropFlag = true;
	// 	loadingpanel.alpha = 0;
	// 	Time.timeScale = 1;
	// 	InitialiseLocations();
	// }
 //
	// private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	// {
	// 	// Reset companion state when a new scene is loaded
	// 	if (CompanionOpen)
	// 	{
	// 		LaunchCompanion(); // Launch companion if
	// 	}
	// }
 //
	// public string MonthDay(string day)
	// {
	// 	string nuNum = "th";
	// 	if (int.Parse(day) < 11 || int.Parse(day) > 20)
	// 	{
	// 		day = day.ToCharArray()[^1].ToString();
	// 		switch (day)
	// 		{
	// 			case "1":
	// 				nuNum = "st";
	// 				break;
	// 			case "2":
	// 				nuNum = "nd";
	// 				break;
	// 			case "3":
	// 				nuNum = "rd";
	// 				break;
	// 		}
	// 	}
	// 	return nuNum;
	// }
 //
 //
	public enum GameScene
	{
		MainMenu,
		NorasFlat,
		TawleyMeats,
		RoarkOutside,
		RoarkInside
	}
	
	
	
}
