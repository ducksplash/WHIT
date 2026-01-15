using System;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
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

    [Header("UI Elements")]
    public CanvasGroup phoneTick;
    public CanvasGroup notepadTick;
    public CanvasGroup torchTick;
    public CanvasGroup evidenceTick;

    [Header("Onboarding Dialogues")]
    public DialogueName needMyThings = DialogueName.NoraNeedsHerThings;
    public DialogueName pickupPhoneDialogue = DialogueName.NoraCollectedPhone;
    public DialogueName pickupTorchDialogue = DialogueName.NoraCollectedTorch;
    public DialogueName pickupNotepadDialogue = DialogueName.NoraCollectedNotepad;
    public DialogueName needTestEvidence = DialogueName.NoraNeedsTestEvidence;
    public DialogueName phoneTutorialGotPhone = DialogueName.KieronToNoraGotPhone;
    public DialogueName phoneTutorialFirstEvidence = DialogueName.KieronToNoraFirstEvidence;
    public DialogueName readyMessage = DialogueName.NoraReadyToGo;

    [Header("First Evidence")]
    public EvidenceName FirstOnboardingEvidence = EvidenceName.WineBottle;

    public Image MyFirstEvidence;
    public TextMeshProUGUI EvidenceDesc;

    private void Awake()
    {
        EventManager.OnEvidenceLoaded += RunOnboardingChecks;
        EventManager.OnEvidenceCollected += RunOnboardingChecks;
    }

    private void RunOnboardingChecks()
    {
        ONBOARDINGCOMPLETE = StoredPrefs.Instance.GetInt("ONBOARDINGCOMPLETE", 0) != 0;

        TORCHCOLLECTED = StoredPrefs.Instance.GetInt("TORCHCOLLECTED", 0) != 0;
        NOTEPADCOLLECTED = StoredPrefs.Instance.GetInt("NOTEPADCOLLECTED", 0) != 0;
        PHONECOLLECTED = StoredPrefs.Instance.GetInt("PHONECOLLECTED", 0) != 0;
        TESTEVIDENCECOLLECTED = StoredPrefs.Instance.GetInt("TESTEVIDENCECOLLECTED", 0) != 0;
        PHONEACCESSED = StoredPrefs.Instance.GetInt("PHONEACCESSED", 0) != 0;

        if (GameMaster.Instance.THISLEVEL != GAMELEVEL.NorasFlat)
            return;

        phonePickup.SetActive(!PHONECOLLECTED);
        phoneTick.alpha = PHONECOLLECTED ? 1 : 0;

        torchPickup.SetActive(!TORCHCOLLECTED);
        torchTick.alpha = TORCHCOLLECTED ? 1 : 0;

        notepadPickup.SetActive(!NOTEPADCOLLECTED);
        notepadTick.alpha = NOTEPADCOLLECTED ? 1 : 0;

        evidenceTick.alpha = TESTEVIDENCECOLLECTED ? 1 : 0;

        UpdateChalkboard();
    }

    public void UpdateChalkboard()
    {
        Debug.Log("[Onboarding] UpdateChalkboard");

        EvidenceName evidenceName = FirstOnboardingEvidence;

        if (!GameMaster.Instance.EvidenceManager.EvidenceFound.TryGetValue(evidenceName, out string quackPath))
        {
            Debug.Log($"[Onboarding] Evidence not collected yet: {evidenceName}");
            return;
        }

        if (!File.Exists(quackPath))
        {
            Debug.LogError($"[Onboarding] Evidence file missing: {quackPath}");
            return;
        }

        string[] lines = File.ReadAllLines(quackPath);

        if (lines.Length < 6)
        {
            Debug.LogError($"[Onboarding] Malformed .quack file: {quackPath}");
            return;
        }

        EvidenceRecord record = new EvidenceRecord
        {
            Name = lines[0],
            Photo = lines[1],
            DateFound = lines[2],
            IsFake = bool.TryParse(lines[3], out var fake) && fake,
            Quality = int.TryParse(lines[4], out var q) ? q : 0,
            Details = lines[5]
        };

        string photoPath = Path.Combine(
            Application.persistentDataPath,
            "Phone/0/DCIM",
            record.Photo
        );

        Debug.Log($"[Onboarding] Loading image: {photoPath}");

        if (!File.Exists(photoPath))
        {
            Debug.LogError($"[Onboarding] Evidence image missing: {photoPath}");
            return;
        }

        byte[] bytes = File.ReadAllBytes(photoPath);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.LoadImage(bytes);

        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100f
        );

        MyFirstEvidence.sprite = sprite;
        MyFirstEvidence.color = Color.white;
        MyFirstEvidence.enabled = true;

        EvidenceDesc.text = record.Details;
        evidenceTick.alpha = 1;

        Debug.Log("[Onboarding] Chalkboard updated successfully");
    }

    IEnumerator NoraReady()
    {
        yield return new WaitForSeconds(5);
        GameMaster.Instance.DialogueManager.NewDialogue(readyMessage, 5);
    }

    public async void CollectTorch()
    {
        Debug.Log("CollectTorch");
        TORCHCOLLECTED = true;

        var coroutine = GameMaster.Instance.CutsceneManager.ExecuteCutscene(6, 1, torchPickup, pickupTorchDialogue);
        await coroutine.AsTask(GameMaster.Instance);
        torchPickup.SetActive(false); 
        
        if (torchTick) torchTick.alpha = 1;
        StoredPrefs.Instance.SetInt("TORCHCOLLECTED", TORCHCOLLECTED ? 1 : 0); StoredPrefs.Instance.Save();
        GameMaster.Instance.EventManager.TorchCollectedEvent();
        CheckOnboardingStatus();
    }


    public async void CollectNotepad()
    {
        Debug.Log("CollectNotepad");
        NOTEPADCOLLECTED = true;
        var coroutine = GameMaster.Instance.CutsceneManager.ExecuteCutscene(6, 1, notepadPickup, pickupNotepadDialogue);
        await coroutine.AsTask(GameMaster.Instance);
        notepadPickup.SetActive(false); 
        if (notepadTick) notepadTick.alpha = 1;
        StoredPrefs.Instance.SetInt("NOTEPADCOLLECTED", NOTEPADCOLLECTED ? 1 : 0); StoredPrefs.Instance.Save();
        GameMaster.Instance.EventManager.NotepadCollectedEvent();
        CheckOnboardingStatus();
    }


    public async void CollectPhone()
    {
        Debug.Log("CollectPhone");
        PHONECOLLECTED = true;
        var coroutine = GameMaster.Instance.CutsceneManager.ExecuteCutscene(6, 1, phonePickup, pickupPhoneDialogue);
        await coroutine.AsTask(GameMaster.Instance);
        phonePickup.SetActive(false); 
        if (phoneTick) phoneTick.alpha = 1;
        StoredPrefs.Instance.SetInt("PHONECOLLECTED", PHONECOLLECTED ? 1 : 0); StoredPrefs.Instance.Save();
        GameMaster.Instance.EventManager.PhoneCollectedEvent();
        CheckOnboardingStatus();
    }


    public void OpenedPhone()
    {
        Debug.Log("OpenedPhone");
        PHONEACCESSED = true;
        GameMaster.Instance.DialogueManager.NewDialogue(phoneTutorialGotPhone, 6);
        StoredPrefs.Instance.SetInt("PHONEACCESSED", PHONEACCESSED ? 1 : 0); StoredPrefs.Instance.Save();
        CheckOnboardingStatus();
    }



    public void CollectTestEvidence()
    {
        Debug.Log("CollectTestEvidence");
        TESTEVIDENCECOLLECTED = true;
        GameMaster.Instance.DialogueManager.NewDialogue(phoneTutorialFirstEvidence, 5);
        
        StoredPrefs.Instance.SetInt("TESTEVIDENCECOLLECTED", TESTEVIDENCECOLLECTED ? 1 : 0); StoredPrefs.Instance.Save();
        
        CheckOnboardingStatus();
    }

    
    
    

    public void EvidenceNotCollected()
    {
        GameMaster.Instance.DialogueManager.NewDialogue(needTestEvidence, 5);
    }

    public void NotReadyYet()
    {
        
        GameMaster.Instance.DialogueManager.NewDialogue(needMyThings, 5);
        
    }


    
    public void CheckOnboardingStatus()
    {
        Debug.Log("CheckOnboardingStatus");
        
        
        if (TORCHCOLLECTED && NOTEPADCOLLECTED && PHONECOLLECTED)
        {

            if (TESTEVIDENCECOLLECTED)
            {
                ONBOARDINGCOMPLETE = true;
                StartCoroutine(NoraReady());
                StoredPrefs.Instance.SetInt("ONBOARDINGCOMPLETE", ONBOARDINGCOMPLETE ? 1 : 0);
                StoredPrefs.Instance.Save();

                Debug.Log("ONBOARDINGCOMPLETE");
            }
        }

        UpdateChalkboard();
    }

    public void GarbageRun()
    {
        Debug.Log("GarbageRun");

        // ---- CLEAR EVIDENCE KEYS ----
        var keys = StoredPrefs.Instance.GetAllKeys();

        foreach (var key in keys)
        {
            if (key.StartsWith("Evidence/")) StoredPrefs.Instance.DeleteKey(key);
        }

        // ---- RESET EQ ----
        StoredPrefs.Instance.SetInt("EQLevelNorasFlat", 0);
        StoredPrefs.Instance.SetInt("EQLevel1", 0);
        StoredPrefs.Instance.SetInt("EQLevel2", 0);

        // ---- OPTIONAL: clear DCIM images ----
        string dcim = Application.persistentDataPath + "/Phone/0/Evidence/";
        if (Directory.Exists(dcim)) Directory.Delete(dcim, true);

        Directory.CreateDirectory(dcim);

        StoredPrefs.Instance.Save();

        // Clear runtime dictionary
        GameMaster.Instance.EvidenceManager.EvidenceFound.Clear();
    }

    public void DeepClean()
    {
        string root = Application.persistentDataPath;

        Debug.Log($"[OnboardingEditor] FULL WIPE: {root}");

        try
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[OnboardingEditor] Failed to delete persistent data: {e}");
        }

        // Recreate base directory so systems don’t crash
        Directory.CreateDirectory(root);

        // Clear runtime state
        if (GameMaster.Instance != null)
        {
            if (GameMaster.Instance.EvidenceManager != null) GameMaster.Instance.EvidenceManager.EvidenceFound.Clear();
        }

        // Reset scene objects safely
        ResetSceneObjects();

        Debug.Log("[OnboardingEditor] Persistent data wipe complete.");
    }

    private void ResetSceneObjects()
    {
        if (phonePickup) phonePickup.SetActive(true);
        if (notepadPickup) notepadPickup.SetActive(true);
        if (torchPickup) torchPickup.SetActive(true);

        if (phoneTick) phoneTick.alpha = 0;
        if (notepadTick) notepadTick.alpha = 0;
        if (torchTick) torchTick.alpha = 0;
        if (evidenceTick) evidenceTick.alpha = 0;
    }
    


}

// File: OnboardingManagerEditor.cs
// Place in folder named "Editor"
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

        Debug.Log($"[OnboardingEditor] FULL WIPE: {root}");

        try
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[OnboardingEditor] Failed to delete persistent data: {e}");
        }

        // Recreate base directory so systems don’t crash
        Directory.CreateDirectory(root);

        // Clear runtime state
        if (GameMaster.Instance != null)
        {
            if (GameMaster.Instance.EvidenceManager != null) GameMaster.Instance.EvidenceManager.EvidenceFound.Clear();
        }

        // Reset scene objects safely
        ResetSceneObjects(mgr);

        Debug.Log("[OnboardingEditor] Persistent data wipe complete.");
    }

    private void ResetSceneObjects(OnboardingManager mgr)
    {
        if (mgr.phonePickup) mgr.phonePickup.SetActive(true);
        if (mgr.notepadPickup) mgr.notepadPickup.SetActive(true);
        if (mgr.torchPickup) mgr.torchPickup.SetActive(true);

        if (mgr.phoneTick) mgr.phoneTick.alpha = 0;
        if (mgr.notepadTick) mgr.notepadTick.alpha = 0;
        if (mgr.torchTick) mgr.torchTick.alpha = 0;
        if (mgr.evidenceTick) mgr.evidenceTick.alpha = 0;
    }
}
#endif