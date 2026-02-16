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

    public bool EnforceSteam = false;
    
    public DevModeScene devModeScene = DevModeScene.NorasFlat;
    // components
    [Header("Global Components")]
    public DialogueManager DialogueManager;
    // public CutsceneManager CutsceneManager;
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
    public QuestManager QuestManager;
    public AudioSlave AudioSlave;
    public LoadingManager LoadingManager;
    public int DefaultFOV;


    public string NORASPCPASSWORD = "1629";
    
    public float MouseSensitivity = 0.2f;
    
    // Game Globals

    public bool PLAYERBUSY;
    
    public bool POWER_SUPPLY_ENABLED;
    public bool INCINERATOR_ENABLED;
    public bool ONLADDER;
    
    public GAMELEVEL THISLEVEL;
    
    public Scene ThisScene;
    public CanvasGroup DevModeIcon;

    // spawn points
    public Vector3 SPAWNPOINTNORASFLAT;
    public Vector3 SPAWNPOINTTAWLEYMEATS;
    public Vector3 SPAWNPOINTROARKOUTSIDE;
    public Vector3 SPAWNPOINTROARKINSIDE;
    
    public static bool GarbageRun;

    private bool SteamLoaded;
    private bool DialoguesLoaded;
    private bool EvidenceLoaded;
    
    
    // steam init
    private bool m_bInitialized;
    public static bool Initialized => Instance.m_bInitialized;

    void Awake()
    {
        Application.targetFrameRate = 60;
        
        DontDestroyOnLoad(gameObject);
        
        m_bInitialized = SteamAPI.Init();
        
        if (!m_bInitialized) 
        {
            Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
        }
        else
        {
            // GET Steam USER ID etc 
            
            // CSteamID steamId = SteamUser.GetSteamID();
            // string username = SteamFriends.GetPersonaName();
            //Debug.Log("Steam User: "+username);
        }
        
        
        //Debug.Log($"This script is active in scene: {SceneManager.GetActiveScene().name}");

        SPAWNPOINTNORASFLAT = new Vector3(65, 2, 486);
        SPAWNPOINTTAWLEYMEATS = new Vector3(71.50f, 12, 282);
        SPAWNPOINTROARKOUTSIDE = new Vector3(90, 5, 252);
        SPAWNPOINTROARKINSIDE = new Vector3(69, 16, 310);
        

        if (DEBUGGERY)
        {
            DevModeIcon.alpha = 1;
        }

        EventManager.OnPlayerDataLoaded += GetSetGo;
        EventManager.OnEvidenceLoaded += GetSetGo;
    }


    
    //// Level orchestration
    //// Here is where I will author the level during it's 'lifetime'
    ////
    //// LEVEL ONE - NORA'S FLAT
    
    //// BEGIN ONBOARDING IF NOT YET ONBOARDED
    ////
    //// 
    ////




    public void GetSetGo()
    {
        // ensure steam loaded.
        SteamLoaded = SteamManager.Initialized;

        DialoguesLoaded = DialogueManager.SeenLoaded;

        EvidenceLoaded = EvidenceManager.EvidenceLoaded;
        
        // force steam?

        if (EnforceSteam)
        {
            if (SteamLoaded && DialoguesLoaded && EvidenceLoaded)
            {
                StartGame();
            }
            else
            {
                Debug.Log("Could Not Start");
                Debug.Log("SteamLoaded " + SteamLoaded);
                Debug.Log("DialoguesLoaded " + DialoguesLoaded);
                Debug.Log("EvidenceLoaded " + EvidenceLoaded);
            }
        }
        else
        {
            if (DialoguesLoaded && EvidenceLoaded)
            {
                StartGame();
            }
            else
            {
                Debug.Log("Could Not Start");
                Debug.Log("DialoguesLoaded " + DialoguesLoaded);
                Debug.Log("EvidenceLoaded " + EvidenceLoaded);
            }
        }




    }
    
    
    
    

    public void StartGame()
    {
        
        
        if (THISLEVEL == GAMELEVEL.MainMenu)
        {
            Debug.Log("Start Main Menu");
        }

        switch (THISLEVEL)
        {
            
            case GAMELEVEL.MainMenu:

                StartLevelNorasFlat();
                break;
            case GAMELEVEL.NorasFlat:
                StartLevelNorasFlat();
                break;

            case GAMELEVEL.RoarkOutside:

                break;

            case GAMELEVEL.RoarkInside:

                break;

        }

    }
    

    public void StartLevelNorasFlat()
    {
        
        LoadingManager.SceneFadeIn();
        StartAudio(AudioProfile.NorasFlat);
        EventManager.GameStartedEvent();
    }
    
    
    public void StartAudio(AudioProfile selectedAudioProfile)
    {
        // Stop all if playing
        
        AudioSlave.StopBGA();
        AudioSlave.StopBGM();
        
        Debug.Log("Playing Audio Profile - "+selectedAudioProfile);
        
        switch (selectedAudioProfile)
        {
            case AudioProfile.MainMenu:
                // Background music
                AudioSlave.PlayBGM(BGMResource.SongOne);
                // Ambience Track (i.e. Rain)
                AudioSlave.PlayBGA(BGAResource.Rain);
                break;

            case AudioProfile.NorasFlat:
                // Background music
                // AudioSlave.PlayBGM(BGMResource.SongOne);
                // Ambience Track (i.e. Rain)
                AudioSlave.PlayBGA(BGAResource.Rain);
                break;
        }
        
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


public enum DevModeScene
{
    MainMenu,
    NorasFlat,
    TawleyMeats,
    RoarkOutside,
    RoarkInside 
}

public enum AudioProfile
{
    MainMenu,
    NorasFlat,
    TawleyMeats,
    RoarkOutside,
    RoarkInside
}