using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Steamworks;



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
    public TerminalEventManager TerminalEventManager;
    public EvidenceManager EvidenceManager;
    public LanguageManager LanguageManager;
    public DeviceType DeviceType;
    public InputManager InputManager;
    public PauseManager PauseManager;
    public Player Player;
    public int DefaultFOV;


    public string NORASPCPASSWORD = "1629";
    
    public float MouseSensitivity = 0.2f;
    
    // Game Globals

    public bool POWER_SUPPLY_ENABLED;
    public bool INCINERATOR_ENABLED;
    public bool FROZEN;
    public bool ONLADDER;
    public bool INMENU;
    public bool HASITEM;
    public bool PHONEOUT;
    public bool ONPC;
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
    
    // steam init
    private bool m_bInitialized;
    public static bool Initialized => Instance.m_bInitialized;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        
        m_bInitialized = SteamAPI.Init();
        
        if (!m_bInitialized) 
        {
            Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
        }
        else
        {
            CSteamID steamId = SteamUser.GetSteamID();
            string username = SteamFriends.GetPersonaName();
            Debug.Log("Steam User: "+username);
        }
        
        
        Debug.Log($"This script is active in scene: {SceneManager.GetActiveScene().name}");

        SPAWNPOINTNORASFLAT = new Vector3(65, 2, 486);
        SPAWNPOINTTAWLEYMEATS = new Vector3(71.50f, 12, 282);
        SPAWNPOINTROARKOUTSIDE = new Vector3(90, 5, 252);
        SPAWNPOINTROARKINSIDE = new Vector3(69, 16, 310);

        // load historical dialogues and cutscenes
        CutsceneManager.LoadWhatYouSee();
        DialogueManager.LoadWhatYouSee();
        // ALWAYS load existing evidence first


        if (DEBUGGERY)
        {
            DevModeIcon.alpha = 1;
        }
    }


    void Start()
    {
        Application.targetFrameRate = 60;
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
    