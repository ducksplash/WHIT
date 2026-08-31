using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Steamworks;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class GameMaster : MonoBehaviour
{
    private static GameMaster _instance;
    public static GameMaster Instance => _instance ??= FindObjectOfType<GameMaster>();

    public TimeZoneInfo GameTimeZone { get; private set; }
    
    
    
    GameData saveData = new GameData();

    // Debuggery
    [Header("Debug Mode Toggle")]
    public bool DEBUGGERY;

    public bool EnforceSteam = false;

    public bool InGame;

    public DevModeScene devModeScene = DevModeScene.NorasFlat;

    // components
    [Header("Global Components")]
    public DialogueManager DialogueManager;
    public TravelCompanion TravelCompanion;
    public OnboardingManager OnboardingManager;
    public TerminalEventManager TerminalEventManager;
    public EvidenceManager EvidenceManager;
    public LanguageManager LanguageManager;

    public NorasWardrobe NorasWardrobe;

    public SmokeVignetteController SmokeVignetteController;
    
    public NoraManager NoraManager;
    
    public DeviceType DeviceType;
    public InputManager InputManager;
    public PauseManager PauseManager;
    public Player Player;
    public QuestManager QuestManager;
    public AudioSlave AudioSlave;
    public LoadingManager LoadingManager;
    public DebugCamera DebugCam;
    public int DefaultFOV;
    public int targetFPS = 30;

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

    [Header("Spawn Points")]
    
    [Header("ETV Studio")]
    public Vector3 SPAWNPOINTETV;
    public Vector3 SPAWNROTETV;
    
    [Header("Nora's Old Flat")]
    public Vector3 SPAWNPOINTNORASOLDFLAT;
    public Vector3 SPAWNROTNORASOLDFLAT;
    
    [Header("Train Station")]
    public Vector3 SPAWNPOINTTRAINSTATION;
    public Vector3 SPAWNROTTRAINSTATION;

    [Header("Entering Tawley")]
    public Vector3 SPAWNPOINTENTERINGTAWLEY;
    public Vector3 SPAWNROTENTERINGTAWLEY;

    [Header("Nora's Flat")]
    public Vector3 SPAWNPOINTNORASFLAT;
    public Vector3 SPAWNROTNORASFLAT;

    [Header("Tawley Meats")]
    public Vector3 SPAWNPOINTTAWLEYMEATS;
    public Vector3 SPAWNROTTAWLEYMEATS;
    
    [Header("Tawley Meats Maze")]
    public Vector3 SPAWNPOINTTAWLEYMEATSMAZE;
    public Vector3 SPAWNROTTAWLEYMEATSMAZE;

    [Header("Roark Outside")]
    public Vector3 SPAWNPOINTROARKOUTSIDE;
    public Vector3 SPAWNROTROARKOUTSIDE;

    [Header("Roark Inside")]
    public Vector3 SPAWNPOINTROARKINSIDE;
    public Vector3 SPAWNROTROARKINSIDE;

    public Volume PostProcessingGlobalVolume;

    public static bool GarbageRun;

    private bool SteamLoaded;
    private bool DialoguesLoaded;
    private bool EvidenceLoaded;

    public int nightTimeStartsAt = 17;
    public int nightTimeEndsAt = 6;

    private bool m_bInitialized;
    public static bool Initialized => Instance.m_bInitialized;


    private bool _dialogueManagerReady;
    private bool _evidenceManagerReady;

    public void NotifyDialogueManagerReady()
    {
        _dialogueManagerReady = true;
        TryGetSetGo();
    }

    public void NotifyEvidenceManagerReady()
    {
        _evidenceManagerReady = true;
        TryGetSetGo();
    }


    private void Start()
    {
        InputManager.FullScreen.action.performed += toggleFullScreen;
    }

    void toggleFullScreen(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        // Toggle fullscreen
        Screen.fullScreen = !Screen.fullScreen;
    }

    private void TryGetSetGo()
    {
        if (InGame) return;

        SteamLoaded = SteamManager.Initialized;

        bool steamOk = !EnforceSteam || SteamLoaded;

        if (!steamOk)
        {
            Debug.Log("Could Not Start — Steam not ready");
            return;
        }

        if (!_dialogueManagerReady)
        {
            Debug.Log("Could Not Start — DialogueManager not ready");
            return;
        }

        if (!_evidenceManagerReady)
        {
            Debug.Log("Could Not Start — EvidenceManager not ready");
            return;
        }

        InGame = true;

        Debug.Log("GameMaster: all systems ready, starting level.");

        StartLevel();

#if UNITY_EDITOR
        // If the editor has requested a development scene,
        // allow the normal ETV startup to complete first,
        // then use the normal LoadingManager transition.
        if (EditorPrefs.HasKey("PlayModeStartScene_TargetLevel"))
        {
            string targetLevelName =
                EditorPrefs.GetString("PlayModeStartScene_TargetLevel", string.Empty);

            EditorPrefs.DeleteKey("PlayModeStartScene_TargetLevel");

            if (Enum.TryParse(targetLevelName, out GAMELEVEL targetLevel) &&
                targetLevel != GAMELEVEL.ETVStudio)
            {
                StartCoroutine(LoadDevelopmentLevelAfterStartup(targetLevel));
            }
        }
#endif
    }
    
#if UNITY_EDITOR

    private IEnumerator LoadDevelopmentLevelAfterStartup(GAMELEVEL targetLevel)
    {
        // Wait until the ETV startup has had a chance to finish.
        yield return null;

        Debug.Log(
            $"[Editor Play Shortcut] Bootstrap complete. " +
            $"Loading development level: {targetLevel}");

        if (LoadingManager == null)
        {
            Debug.LogError(
                "[Editor Play Shortcut] LoadingManager is not assigned.");

            yield break;
        }

        LoadingManager.LoadLevel(targetLevel);
    }
#endif

    void Awake()
    {

        SetTargetFrameRate(targetFPS);
        
        
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        GameTimeZone = GetGameTimeZone();
        
        m_bInitialized = SteamAPI.Init();

        if (!m_bInitialized)
        {
            Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed.", this);
        }

        if (DEBUGGERY)
        {
            DevModeIcon.alpha = 1;
        }
    }
    
    
    
    
    public void SetTargetFrameRate(int fps)
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fps;

        // Extra enforcement for stubborn cases
        Time.fixedDeltaTime = 1f / fps;
    }

    // ====================== MAIN LEVEL ROUTER ======================
    public void StartLevel(GAMELEVEL level = GAMELEVEL.ETVStudio)
    {
        THISLEVEL = level;

        System.GC.Collect();
        Resources.UnloadUnusedAssets();

        switch (level)
        {
            case GAMELEVEL.ETVStudio:
                StartLevelETV();
                break;

            case GAMELEVEL.MainMenu:
            case GAMELEVEL.NorasOldFlat:
                StartLevelNorasOldFlat();
                break;

            case GAMELEVEL.FarsetCentralStation:
                StartLevelTrainStation();
                break;
            
            case GAMELEVEL.EnteringTawley:
                StartLevelEnteringTawley();
                break;
            
            case GAMELEVEL.NorasFlat:
                StartLevelNorasFlat();
                break;
            

            case GAMELEVEL.TawleyMeats:
                StartLevelTawleyMeats();
                break;
            
            case GAMELEVEL.TawleyMeatsMaze:
                StartLevelTawleyMeatsMaze();
                break;

            case GAMELEVEL.RoarkOutside:
                StartLevelRoarkOutside();
                break;

            case GAMELEVEL.RoarkInside:
                StartLevelRoarkInside();
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
        
        
        StartAudio(AudioProfile.NorasFlat);
        EventManager.GameStartedEvent();
        EventManager.LevelLoaded();
        
        NoraManager.InitialiseNora(true);
        
        LoadingManager.SceneFadeIn();
        DialogueManager.PlayThought(ThoughtName.StartingWork);
    }


    public void StartLevelNorasOldFlat()
    {
        THISLEVEL = GAMELEVEL.NorasOldFlat;
        
        NoraManager.InitialiseNora();
        

        Player.Instance.Spawn();
        StartAudio(AudioProfile.NorasFlat);
        EventManager.GameStartedEvent();
        EventManager.LevelLoaded();
        LoadingManager.SceneFadeIn();
    }

    
    
    public void StartLevelTrainStation()
    {
        THISLEVEL = GAMELEVEL.FarsetCentralStation;
        
        NoraManager.InitialiseNora(true);
        
        
        Player.Instance.Spawn();
        StartAudio(AudioProfile.FarsetCentralStation);
        EventManager.GameStartedEvent();
        EventManager.LevelLoaded();
        LoadingManager.SceneFadeIn();
    }

    
    public void StartLevelEnteringTawley()
    {
        THISLEVEL = GAMELEVEL.EnteringTawley;
        
        NoraManager.InitialiseNora(true);
        
        
        Player.Instance.Spawn();
        StartAudio(AudioProfile.FarsetCentralStation);
        EventManager.GameStartedEvent();
        EventManager.LevelLoaded();
        LoadingManager.SceneFadeIn();
    }

    
    
    public void StartLevelNorasFlat()
    {
        THISLEVEL = GAMELEVEL.NorasFlat;

        DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GameTimeZone);

        int hour = now.Hour;

        bool isNight;
        if (nightTimeStartsAt < nightTimeEndsAt)
            isNight = hour >= nightTimeStartsAt && hour < nightTimeEndsAt;
        else
            isNight = hour >= nightTimeStartsAt || hour < nightTimeEndsAt;

        if (isNight)
        {
            NorasWardrobe.SetRandomOutfitOfType(OutfitType.Pyjamas);
        }
        else
        {
            NoraManager.InitialiseNora();
        }

        Player.Instance.Spawn();
        StartAudio(AudioProfile.NorasFlat);
        EventManager.GameStartedEvent();
        EventManager.LevelLoaded();
        LoadingManager.SceneFadeIn();
    }

    public void StartLevelTawleyMeats()
    {
        THISLEVEL = GAMELEVEL.TawleyMeats;
        Player.Instance.Spawn();
        NoraManager.InitialiseNora();
        StartAudio(AudioProfile.TawleyMeats);
        EventManager.GameStartedEvent();
        EventManager.LevelLoaded();
        LoadingManager.SceneFadeIn();
        
        
        EventManager.UnlockAchievement(SteamAchievements.WorkMeating);
    }
    public void StartLevelTawleyMeatsMaze()
    {
        THISLEVEL = GAMELEVEL.TawleyMeatsMaze;
        Player.Instance.Spawn();
        NoraManager.InitialiseNora();
        StartAudio(AudioProfile.TawleyMeats);
        EventManager.GameStartedEvent();
        EventManager.LevelLoaded();
        LoadingManager.SceneFadeIn();
        
        EventManager.UnlockAchievement(SteamAchievements.WorkMeatingCancelled);
    }

    public void StartLevelRoarkOutside()
    {
        THISLEVEL = GAMELEVEL.RoarkOutside;
        Player.Instance.Spawn();
        NoraManager.InitialiseNora();
        StartAudio(AudioProfile.RoarkOutside);
        EventManager.GameStartedEvent();
        EventManager.LevelLoaded();
        LoadingManager.SceneFadeIn();
    }

    public void StartLevelRoarkInside()
    {
        THISLEVEL = GAMELEVEL.RoarkInside;
        Player.Instance.Spawn();
        NoraManager.InitialiseNora();
        StartAudio(AudioProfile.RoarkInside);
        EventManager.GameStartedEvent();
        EventManager.LevelLoaded();
        LoadingManager.SceneFadeIn();
    }

    public Dictionary<string, string> CutSceneSeen = new();

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
                break;
        }
    }
    
    private TimeZoneInfo GetGameTimeZone()
    {
        try
        {
            // Windows
            return TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            // Linux / macOS / Steam Deck
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
        }
    }
}

public enum GAMELEVEL
{
    ETVStudio,
    MainMenu,
    NorasFlat,
    NorasOldFlat,
    TawleyMeats,
    RoarkOutside,
    RoarkInside,
    FarsetCentralStation,
    EnteringTawley,
    TawleyMeatsMaze
}

public enum DevModeScene
{
    ETV,
    MainMenu,
    NorasFlat,
    NorasOldFlat,
    TawleyMeats,
    RoarkOutside,
    RoarkInside
}

public enum AudioProfile
{
    MainMenu,
    NorasFlat,
    NorasOldFlat,
    TawleyMeats,
    RoarkOutside,
    RoarkInside,
    FarsetCentralStation,
    EnteringTawley
}

