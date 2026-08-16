using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class OnboardingManager : MonoBehaviour
{
    // Completed Onboarding Steps
    public bool DEBUGGERY;
    public bool TORCHCOLLECTED;
    public bool NOTEPADCOLLECTED;
    public bool PHONECOLLECTED;
    public bool PHONEACCESSED;
    public bool TESTEVIDENCECOLLECTED;
    public bool ONBOARDINGCOMPLETE;

    
    [Header("Physical Objects in Noras Flat")]
    public GameObject phonePickup;
    public GameObject notepadPickup;
    public GameObject torchPickup;

    
    [Header("Onboarding Dialogues")]
    public ThoughtName needMyThings = ThoughtName.NoraNeedsHerThings;
    public ThoughtName pickupPhoneThought = ThoughtName.CollectedPhone;
    public ThoughtName pickupTorchThought = ThoughtName.CollectedTorch;
    public ThoughtName pickupNotepadThought = ThoughtName.CollectedNotepad;
    public ThoughtName needTestEvidence = ThoughtName.NoraNeedsTestEvidence;
    public DialogueName phoneTutorialGotPhone = DialogueName.KieronToNoraGotPhone;
    public DialogueName phoneTutorialFirstEvidence = DialogueName.KieronToNoraFirstEvidence;
    public ThoughtName readyMessage = ThoughtName.NoraReadyToGo;
    
    [Header("First Evidence")]
    public EvidenceName FirstOnboardingEvidence = EvidenceName.Wine;

    // public Image MyFirstEvidence;
    // public TextMeshProUGUI EvidenceDesc;

    private bool _restoredThisScene;
    
    private void Awake()
    {
        EventManager.OnEvidenceLoaded += RunOnboardingChecks;
        EventManager.OnEvidenceCollected += RunOnboardingChecks;
        EventManager.OnLevelLoaded += OnboardingCheckEvent;
    }


    private void Start()
    {
        EventManager.OnRegisterNotepad += RegisterNotepad;
        EventManager.OnRegisterTorch += RegisterTorch;
        EventManager.OnRegisterPhone += RegisterPhone;
    }

    // when player enters the scene we'll add it to the obm
    public void RegisterNotepad(GameObject notepadObject)
    {
        notepadPickup = notepadObject;
        RunOnboardingChecks();
    }

    public void RegisterTorch(GameObject torchObject)
    {
        torchPickup = torchObject;
        RunOnboardingChecks();
    }

    public void RegisterPhone(GameObject phoneObject)
    {
        phonePickup = phoneObject;
        RunOnboardingChecks();
        
    }

    
    private void OnEnable()
    {
        EventManager.OnEvidenceLoaded += RunOnboardingChecks;
        EventManager.OnEvidenceCollected += RunOnboardingChecks;

        // EventManager.OnPlayerDataLoaded += RunOnboardingChecks;

        StartCoroutine(DeferredInit());
    }

    private void OnDisable()
    {
        EventManager.OnEvidenceLoaded -= RunOnboardingChecks;
        EventManager.OnEvidenceCollected -= RunOnboardingChecks;
        // EventManager.OnPlayerDataLoaded -= RunOnboardingChecks;
    }


    private void OnboardingCheckEvent()
    {

        //if (GameMaster.Instance.THISLEVEL != GAMELEVEL.NorasFlat) return;
        
        StartCoroutine(DeferredInit());
    }

    private System.Collections.IEnumerator DeferredInit()
    {
        // wait a frame so other Awake/OnEnable happen first
        yield return null;

        // optionally wait until singletons exist (max a few frames)
        int safety = 30;
        while ((StoredPrefs.Instance == null || GameMaster.Instance == null) && safety-- > 0)
            yield return null;

        RunOnboardingChecks();
    }


    
    private void RunOnboardingChecks()
    {
        if (StoredPrefs.Instance == null || GameMaster.Instance == null) return;

        // read saved flags
        ONBOARDINGCOMPLETE = StoredPrefs.Instance.GetInt("ONBOARDINGCOMPLETE", 0) != 0;
        TORCHCOLLECTED = StoredPrefs.Instance.GetInt("TORCHCOLLECTED", 0) != 0;
        NOTEPADCOLLECTED = StoredPrefs.Instance.GetInt("NOTEPADCOLLECTED", 0) != 0;
        PHONECOLLECTED = StoredPrefs.Instance.GetInt("PHONECOLLECTED", 0) != 0;
        TESTEVIDENCECOLLECTED = StoredPrefs.Instance.GetInt("TESTEVIDENCECOLLECTED", 0) != 0;
        PHONEACCESSED = StoredPrefs.Instance.GetInt("PHONEACCESSED", 0) != 0;

        if (GameMaster.Instance.THISLEVEL != GAMELEVEL.NorasFlat) return;

        if (!_restoredThisScene)
        {
            _restoredThisScene = true;

            if (PHONECOLLECTED) RestorePhoneCollected();
            if (TORCHCOLLECTED) RestoreTorchCollected();
            if (NOTEPADCOLLECTED) RestoreNotepadCollected();
        }

        
        
        
        if (phonePickup != null) phonePickup.SetActive(!PHONECOLLECTED);

        if (torchPickup != null) torchPickup.SetActive(!TORCHCOLLECTED);

        if (notepadPickup != null) notepadPickup.SetActive(!NOTEPADCOLLECTED);

        EventManager.UpdateCorkboard();
        
    }

    
    

    IEnumerator NoraReady()
    {
        yield return new WaitForSeconds(3);
        GameMaster.Instance.DialogueManager.PlayThought(readyMessage);
    }

    public async void CollectTorch()
    {
        //Debug.Log("CollectTorch");
        TORCHCOLLECTED = true;
        Player.Instance.CombatEnabled = true;
        
        EventManager.SlideTickerIn();
        

        GameMaster.Instance.DialogueManager.PlayThought(pickupTorchThought);
        
        GameMaster.Instance.QuestManager.UpdateQuestObjectives(QuestName.GetReadyForWork, 3);

        torchPickup.SetActive(false);
        
        StoredPrefs.Instance.SetInt("TORCHCOLLECTED", TORCHCOLLECTED ? 1 : 0);
        StoredPrefs.Instance.Save();

        EventManager.SlideTickerOut();
        EventManager.TorchCollectedEvent();
        CheckOnboardingStatus();
        
    }

    public async void CollectNotepad()
    {
        //Debug.Log("CollectNotepad");
        NOTEPADCOLLECTED = true;

        EventManager.SlideTickerIn();

        GameMaster.Instance.DialogueManager.PlayThought(pickupNotepadThought);

        GameMaster.Instance.QuestManager.UpdateQuestObjectives(QuestName.GetReadyForWork, 2);
        
        notepadPickup.SetActive(false);

        
        StoredPrefs.Instance.SetInt("NOTEPADCOLLECTED", NOTEPADCOLLECTED ? 1 : 0);
        StoredPrefs.Instance.Save();

        EventManager.SlideTickerOut();
        
        EventManager.NotepadCollectedEvent();
        CheckOnboardingStatus();
    }

    public async void CollectPhone()
    {
        //Debug.Log("CollectPhone");
        
        EventManager.SlideTickerIn();
        
        
        PHONECOLLECTED = true;

        GameMaster.Instance.DialogueManager.PlayThought(pickupPhoneThought);

        GameMaster.Instance.QuestManager.UpdateQuestObjectives(QuestName.GetReadyForWork, 1);
        
        phonePickup.SetActive(false);

        StoredPrefs.Instance.SetInt("PHONECOLLECTED", PHONECOLLECTED ? 1 : 0);
        StoredPrefs.Instance.Save();
        
        
        EventManager.SlideTickerOut();

        CheckOnboardingStatus();
        EventManager.PhoneCollectedEvent();
    }


    public void OpenedPhone()
    {
        if (PHONEACCESSED) return;
        //Debug.Log("OpenedPhone");
        PHONEACCESSED = true;
        GameMaster.Instance.DialogueManager.NewDialogue(phoneTutorialGotPhone, 6);
        StoredPrefs.Instance.SetInt("PHONEACCESSED", PHONEACCESSED ? 1 : 0); StoredPrefs.Instance.Save();
        CheckOnboardingStatus();
    }



    public void CollectTestEvidence()
    {
        GameMaster.Instance.DialogueManager.NewDialogue(phoneTutorialFirstEvidence, 5);
        
        StoredPrefs.Instance.SetInt("TESTEVIDENCECOLLECTED", 1); 
        StoredPrefs.Instance.Save();
        
        EventManager.UnlockAchievement(SteamAchievements.NewsHound);
        
        CheckOnboardingStatus();
    }

    
    
    

    public void EvidenceNotCollected()
    {
        GameMaster.Instance.DialogueManager.PlayThought(needTestEvidence);
    }

    public void NotReadyYet()
    {
        GameMaster.Instance.DialogueManager.PlayThought(needMyThings);
    }


    
    public void CheckOnboardingStatus()
    {
        //Debug.Log("CheckOnboardingStatus");
        
        if (TORCHCOLLECTED && NOTEPADCOLLECTED && PHONECOLLECTED)
        {
            if (TESTEVIDENCECOLLECTED)
            {
                ONBOARDINGCOMPLETE = true;
                StartCoroutine(NoraReady());
                StoredPrefs.Instance.SetInt("ONBOARDINGCOMPLETE", ONBOARDINGCOMPLETE ? 1 : 0);
                StoredPrefs.Instance.Save();

                //Debug.Log("ONBOARDINGCOMPLETE");
            }
        }

        EventManager.UpdateCorkboard();
    }

    public void GarbageRun()
    {

        var keys = StoredPrefs.Instance.GetAllKeys();

        foreach (var key in keys)
        {
            if (key.StartsWith("Evidence/")) StoredPrefs.Instance.DeleteKey(key);
        }

        StoredPrefs.Instance.SetInt("EQLevelNorasFlat", 0);
        StoredPrefs.Instance.SetInt("EQLevel1", 0);
        StoredPrefs.Instance.SetInt("EQLevel2", 0);

        string dcim = Application.persistentDataPath + "/Phone/0/Evidence/";
        if (Directory.Exists(dcim)) Directory.Delete(dcim, true);

        Directory.CreateDirectory(dcim);

        StoredPrefs.Instance.Save();

        GameMaster.Instance.EvidenceManager.EvidenceFound.Clear();
    }

    public void DeepClean()
    {
        if (GameMaster.Instance.PLAYERBUSY) return;
        string root = Application.persistentDataPath;
        
        try
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[OnboardingEditor] Failed to delete persistent data: {e}");
        }

        Directory.CreateDirectory(root);

        if (GameMaster.Instance != null) { if (GameMaster.Instance.EvidenceManager != null) GameMaster.Instance.EvidenceManager.EvidenceFound.Clear(); }

        ResetSceneObjects();
    }

    private void ResetSceneObjects()
    {
        if (phonePickup != null) phonePickup.SetActive(true);
        if (notepadPickup != null) notepadPickup.SetActive(true);
        if (torchPickup != null) torchPickup.SetActive(true);
        
        
        EventManager.UpdateCorkboard();
    }
    

    private void RestorePhoneCollected()
    {
        PHONECOLLECTED = true;
        if (phonePickup != null) phonePickup.SetActive(false);
        
        //GameMaster.Instance.EventManager.PhoneCollectedEvent();
    }

    private void RestoreTorchCollected()
    {
        TORCHCOLLECTED = true;
        Player.Instance.CombatEnabled = true;
        if (torchPickup != null) torchPickup.SetActive(false);

        EventManager.TorchCollectedEvent();
    }

    private void RestoreNotepadCollected()
    {
        NOTEPADCOLLECTED = true;

        if (notepadPickup != null) notepadPickup.SetActive(false);

        EventManager.NotepadCollectedEvent();
    }


}

#if UNITY_EDITOR

[CustomEditor(typeof(OnboardingManager))]
public class OnboardingManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        OnboardingManager mgr = (OnboardingManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(":: Onboarding Debug Tools", EditorStyles.boldLabel);

        DrawStatus(mgr);
        DrawTestActions(mgr);
        DrawResetTools(mgr);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    private void DrawStatus(OnboardingManager mgr)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Current Runtime Status", EditorStyles.miniBoldLabel);

        EditorGUILayout.Toggle("Torch Collected", mgr.TORCHCOLLECTED);
        EditorGUILayout.Toggle("Notepad Collected", mgr.NOTEPADCOLLECTED);
        EditorGUILayout.Toggle("Phone Collected", mgr.PHONECOLLECTED);
        EditorGUILayout.Toggle("Phone Accessed", mgr.PHONEACCESSED);
        EditorGUILayout.Toggle("Test Evidence Collected", mgr.TESTEVIDENCECOLLECTED);
        EditorGUILayout.Toggle("Onboarding Complete", mgr.ONBOARDINGCOMPLETE);
    }

    private void DrawTestActions(OnboardingManager mgr)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Simulate Steps", EditorStyles.miniBoldLabel);

        if (GUILayout.Button("Collect Torch"))
            mgr.CollectTorch();

        if (GUILayout.Button("Collect Notepad"))
            mgr.CollectNotepad();

        if (GUILayout.Button("Collect Phone"))
            mgr.CollectPhone();

        if (GUILayout.Button("Open Phone"))
            mgr.OpenedPhone();

        if (GUILayout.Button("Collect Test Evidence"))
            mgr.CollectTestEvidence();
    }

    private void DrawResetTools(OnboardingManager mgr)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Danger Zone", EditorStyles.boldLabel);

        GUI.backgroundColor = Color.red;

        if (GUILayout.Button("RESET ALL ONBOARDING + DELETE ALL SAVE DATA"))
        {
            if (EditorUtility.DisplayDialog(
                "Full Data Wipe",
                "This will DELETE EVERYTHING in Application.persistentDataPath.\n\nThis cannot be undone.",
                "Delete Everything",
                "Cancel"))
            {
                FullPersistentDataWipe(mgr);
            }
        }

        GUI.backgroundColor = Color.white;
    }

    public void FullPersistentDataWipe(OnboardingManager mgr)
    {
        string root = Application.persistentDataPath;
        
        try
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[OnboardingEditor] Failed to delete persistent data: {e}");
        }

        Directory.CreateDirectory(root);

        if (GameMaster.Instance != null) { if (GameMaster.Instance.EvidenceManager != null) GameMaster.Instance.EvidenceManager.EvidenceFound.Clear(); }

        ResetSceneObjects(mgr);

    }

    private void ResetSceneObjects(OnboardingManager mgr)
    {
        if (mgr.phonePickup != null) mgr.phonePickup.SetActive(true);
        if (mgr.notepadPickup != null) mgr.notepadPickup.SetActive(true);
        if (mgr.torchPickup != null) mgr.torchPickup.SetActive(true);

        
        EventManager.UpdateCorkboard();
    }
}
#endif