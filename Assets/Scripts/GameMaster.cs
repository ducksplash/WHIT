using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.UI;
using TMPro;


public class GameMaster : MonoBehaviour
{
    private static GameMaster _instance;
    public static GameMaster Instance => _instance ??= FindObjectOfType<GameMaster>();

    
    GameData saveData = new GameData();
    
    // Debuggery
    [Header("Debug Mode Toggle")]
    public bool DEBUGGERY;

    // components
    [Header("Global Components")]
    public DialogueManager DialogueManager;
    public CutsceneManager CutsceneManager;
    public TravelCompanion TravelCompanion;
    public Pickup Pickup;
    public OnboardingManager OnboardingManager;
    public EventManager EventManager;
    public EvidenceManager EvidenceManager;
    public LanguageManager LanguageManager;
    public DeviceType DeviceType;
    public int DefaultFOV;


    public float MouseSensitivity = 0.2f;
    
    // Game Globals

    public bool POWER_SUPPLY_ENABLED;
    public bool INCINERATOR_ENABLED;
    public bool FROZEN;
    public bool ONLADDER;
    public bool INMENU;
    public bool HASITEM;
    public bool PHONEOUT;
    public bool  ISWRITING;
    public bool ONBOARDINGCOMPLETED;
    
    public GAMELEVEL THISLEVEL;
    
    public Scene ThisScene;
    public CanvasGroup DevModeIcon;

    // spawn points
    public Vector3 SPAWNPOINTNORASFLAT;
    public Vector3 SPAWNPOINTTAWLEYMEATS;
    public Vector3 SPAWNPOINTROARKOUTSIDE;
    public Vector3 SPAWNPOINTROARKINSIDE;

    
    public static bool GarbageRun;
    
    
    // Evidence Quotient
    
    
    public static int EQThisLevel;
    public static int ExpectedEQThisLevel;

    // EQ Expected for the different levels (due to be refactored)
    public static int ExpectedEQ_Level0 = 1;
    public static int ExpectedEQ_Level1 = 18;
    public static int ExpectedEQ_Level2 = 19;


    
    // Dialog log

    // The main purpose is to prevent duplicates.
    // A secondary use is within the phone, as a message log.
    // The main dictionary is split into NoraSpeak - The player dialogue, and 'Messages' (from others)
    // 

    public List<DialogueName> DialogueSeen = new List<DialogueName>();

    // now again for cutscenes

    public static Dictionary<string, string> CutSceneSeen = new Dictionary<string, string>();

    // Evidence Log
    // Again, mainly preventing duplication
    // With the secondary use being in the phone again, as an "Evidence Log" in the "Gallery App"

    public Dictionary<string, string> EvidenceFound = new Dictionary<string, string>();

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        Debug.Log($"This script is active in scene: {SceneManager.GetActiveScene().name}");

        SPAWNPOINTNORASFLAT = new Vector3(65, 2, 486);
        SPAWNPOINTTAWLEYMEATS = new Vector3(71.50f, 12, 282);
        SPAWNPOINTROARKOUTSIDE = new Vector3(90, 5, 252);
        SPAWNPOINTROARKINSIDE = new Vector3(69, 16, 310);

        // ALWAYS load existing evidence first
        LoadExistingEvidence();

        // Mark collected evidence objects in scene
        foreach (var evidence in EvidenceFound)
        {
            GameObject obj = GameObject.Find(evidence.Key);
            if (obj != null && obj.TryGetComponent<Evidence>(out var ev))
            {
                ev.EvidenceCollected = true;
            }
        }



        if (DEBUGGERY)
        {
            DevModeIcon.alpha = 1;
        }
    }


    void Start()
    {


        Application.targetFrameRate = 60;






        if (THISLEVEL == GAMELEVEL.TawleyMeats)
        {

            ExpectedEQThisLevel = ExpectedEQ_Level1;
            ExpectedEQThisLevel = ExpectedEQ_Level1;



        }



        if (THISLEVEL == GAMELEVEL.RoarkInside)
        {

            ExpectedEQThisLevel = ExpectedEQ_Level2;
            ExpectedEQThisLevel = ExpectedEQ_Level2;



        }

        // 1; Tawley Meats
        //  if (THISLEVEL == "1")
        //  {
        //      INCINERATOR_ENABLED = true;
        //  }

    }



    private void LoadExistingEvidence()
    {
        EvidenceFound.Clear();

        // Get all StoredPrefs keys
        List<string> keys = StoredPrefs.GetAllKeys();

        foreach (string key in keys)
        {
            // We only care about Evidence entries
            if (!key.StartsWith("Evidence/"))
                continue;

            // Evidence/<ID> → extract ID
            string evidenceId = key.Substring("Evidence/".Length);

            if (!EvidenceFound.ContainsKey(evidenceId))
            {
                EvidenceFound.Add(evidenceId, key);
                Debug.Log($"Loaded evidence: {evidenceId}");
            }
        }

        Debug.Log("Init Evidence (StoredPrefs) - " + EvidenceFound.Count);
        EventManager.PlayerDataLoaded();
    }

}

public enum GAMELEVEL
{
    MainMenu,
    NorasFlat,
    TawleyMeats,
    RoarkOutside,
    RoarkInside
}
    
public static class CoroutineExtensions
{
    public static Task AsTask(this IEnumerator coroutine, MonoBehaviour runner)
    {
        var tcs = new TaskCompletionSource<bool>();

        runner.StartCoroutine(Wrap());

        IEnumerator Wrap()
        {
            yield return coroutine;
            tcs.SetResult(true);
        }

        return tcs.Task;
    }
}
    