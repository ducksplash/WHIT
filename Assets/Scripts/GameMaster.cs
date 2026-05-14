using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Steamworks;



public class GameMaster : MonoBehaviour
{
    private static GameMaster _instance;
    public static GameMaster Instance => _instance ??= FindObjectOfType<GameMaster>();

    
    private const string TimeZoneId = "GMT Standard Time";
    
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
    public DebugCamera DebugCam;
    public int DefaultFOV;


    public string NORASPCPASSWORD = "1629";
    
    public float MouseSensitivity = 0.2f;
    
    // Game Globals

    public bool PLAYERBUSY;
    public bool INAMEETING;
    public bool POWER_SUPPLY_ENABLED;
    public bool INCINERATOR_ENABLED;
    public bool ONLADDER;
    
    public GAMELEVEL THISLEVEL;
    
    public Scene ThisScene;
    public CanvasGroup DevModeIcon;

    // spawn points
    public Vector3 SPAWNPOINTETV;
    public Vector3 SPAWNPOINTNORASFLAT;
    public Vector3 SPAWNPOINTTAWLEYMEATS;
    public Vector3 SPAWNPOINTROARKOUTSIDE;
    public Vector3 SPAWNPOINTROARKINSIDE;
    
    public static bool GarbageRun;

    private bool SteamLoaded;
    private bool DialoguesLoaded;
    private bool EvidenceLoaded;

    public int nightTimeStartsAt = 17;
    
    // steam init
    private bool m_bInitialized;
    public static bool Initialized => Instance.m_bInitialized;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = 60;
        
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

        // SPAWNPOINTETV = new Vector3(36, -8, 473);
        // SPAWNPOINTNORASFLAT = new Vector3(65, 2, 486);
        // SPAWNPOINTTAWLEYMEATS = new Vector3(71.50f, 12, 282);
        // SPAWNPOINTROARKOUTSIDE = new Vector3(90, 5, 252);
        // SPAWNPOINTROARKINSIDE = new Vector3(69, 16, 310);
        

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
        // here we can maybe retrieve last level player was on and spawn it in accordingly
        
        if (THISLEVEL == GAMELEVEL.MainMenu)
        {
            Debug.Log("Start Main Menu");
        }

        
        switch (THISLEVEL)
        {
            
            case GAMELEVEL.ETVStudio:
                StartLevelETV();
                break;
            case GAMELEVEL.MainMenu:
                StartLevelNorasFlat();
                break;
            case GAMELEVEL.NorasFlat:
                StartLevelNorasFlat();
                break;
            case GAMELEVEL.RoarkOutside:
                StartLevelNorasFlat();
                break;
            case GAMELEVEL.TawleyMeats:
                StartLevelNorasFlat();
                //THISLEVEL = GAMELEVEL.RoarkOutside;
                break;
            case GAMELEVEL.RoarkInside:
                StartLevelNorasFlat();

                //THISLEVEL = GAMELEVEL.RoarkInside;
                break;

        }

    }
    // ====================== MAIN LEVEL ROUTER ======================
    public void StartLevel(GAMELEVEL level)
    {
        THISLEVEL = level;

        switch (level)
        {
            case GAMELEVEL.ETVStudio:
                StartLevelETV();
                break;

            case GAMELEVEL.MainMenu:
            case GAMELEVEL.NorasFlat:
                StartLevelNorasFlat();
                break;

            case GAMELEVEL.TawleyMeats:
                StartLevelTawleyMeats();
                break;

            case GAMELEVEL.RoarkOutside:
                StartLevelRoarkOutside();
                break;

            case GAMELEVEL.RoarkInside:
                StartLevelRoarkInside();
                break;

            case GAMELEVEL.SecretLevel:
                // TODO: Add when ready
                StartLevelNorasFlat();
                break;

            default:
                Debug.LogWarning($"No setup defined for {level}. Falling back to NorasFlat.");
                StartLevelNorasFlat();
                break;
        }
    }

    // ====================== INDIVIDUAL LEVEL STARTS ======================
    public void StartLevelETV()
    {
        THISLEVEL = GAMELEVEL.ETVStudio;
        Player.Instance.Spawn();
        Player.Instance.Me.ToggleWorkOutfit(true);
        StartAudio(AudioProfile.NorasFlat);
        EventManager.GameStartedEvent();
        EventManager.LevelLoaded();
        LoadingManager.SceneFadeIn();
    }

    public void StartLevelNorasFlat()
    {
        
        
        
        THISLEVEL = GAMELEVEL.NorasFlat;
        Player.Instance.Spawn();
        
        // Always start from UTC
        DateTime utcNow = DateTime.UtcNow;

        // Convert to a known timezone (avoids Proton / Steam Deck offset bugs)
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        
        DateTime now = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);

        string anHour = now.Hour.ToString().PadLeft(2, '0');

        if (int.Parse(anHour) > nightTimeStartsAt)
        {
            Player.Instance.Me.TogglePyjamasOutfit(true);
        }
        else
        {
            Player.Instance.Me.ToggleCasualOutfit(true);
        }
        
        
        
        
        StartAudio(AudioProfile.NorasFlat);
        EventManager.GameStartedEvent();
        EventManager.LevelLoaded();
        LoadingManager.SceneFadeIn();
    }

    public void StartLevelTawleyMeats()
    {
        THISLEVEL = GAMELEVEL.TawleyMeats;
        Player.Instance.Spawn();
        Player.Instance.Me.ToggleCasualOutfit(true);
        StartAudio(AudioProfile.TawleyMeats);
        EventManager.GameStartedEvent();
        EventManager.LevelLoaded();
        LoadingManager.SceneFadeIn();
    }

    public void StartLevelRoarkOutside()
    {
        THISLEVEL = GAMELEVEL.RoarkOutside;
        Player.Instance.Spawn();
        Player.Instance.Me.ToggleCasualOutfit(true);
        StartAudio(AudioProfile.RoarkOutside);
        EventManager.GameStartedEvent();
        EventManager.LevelLoaded();
        LoadingManager.SceneFadeIn();
    }

    public void StartLevelRoarkInside()
    {
        THISLEVEL = GAMELEVEL.RoarkInside;
        Player.Instance.Spawn();
        Player.Instance.Me.ToggleCasualOutfit(true);
        StartAudio(AudioProfile.RoarkInside);
        EventManager.GameStartedEvent();
        EventManager.LevelLoaded();
        LoadingManager.SceneFadeIn();
    }

    public void StartAudio(AudioProfile selectedAudioProfile)
    {
        AudioSlave.StopBGA();
        AudioSlave.StopBGM();

        Debug.Log("Playing Audio Profile - " + selectedAudioProfile);

        switch (selectedAudioProfile)
        {
            case AudioProfile.MainMenu:
            case AudioProfile.NorasFlat:
                AudioSlave.PlayBGA(BGAResource.Rain);
                // AudioSlave.PlayBGM(BGMResource.SongOne);   // uncomment when ready
                break;

            // Add other cases as you expand
        }
    }
    
    
    
    
    
    
    
    
    
}

public enum GAMELEVEL
{
    ETVStudio,
    MainMenu,
    NorasFlat,
    TawleyMeats,
    RoarkOutside,
    RoarkInside,
    SecretLevel
}


public enum DevModeScene
{
    ETV,
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