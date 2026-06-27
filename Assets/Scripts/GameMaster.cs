using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine.Rendering;

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

    [Header("Spawn Points")]
    
    [Header("ETV Studio")]
    public Vector3 SPAWNPOINTETV;
    public Vector3 SPAWNROTETV;           // X = Pitch, Y = Yaw, Z = Roll

    [Header("Nora's Flat")]
    public Vector3 SPAWNPOINTNORASFLAT;
    public Vector3 SPAWNROTNORASFLAT;

    [Header("Nora's Old Flat")]
    public Vector3 SPAWNPOINTNORASOLDFLAT;
    public Vector3 SPAWNROTNORASOLDFLAT;

    [Header("Tawley Meats")]
    public Vector3 SPAWNPOINTTAWLEYMEATS;
    public Vector3 SPAWNROTTAWLEYMEATS;

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
    }

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
            Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed.", this);
        }

        if (DEBUGGERY)
        {
            DevModeIcon.alpha = 1;
        }
    }

    // ====================== MAIN LEVEL ROUTER ======================
    public void StartLevel(GAMELEVEL level = GAMELEVEL.ETVStudio)
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
        Player.Instance.NorasWardrobe.SetRandomWorkOutfit();
        StartAudio(AudioProfile.NorasFlat);
        EventManager.GameStartedEvent();
        EventManager.LevelLoaded();
        LoadingManager.SceneFadeIn();
        DialogueManager.PlayThought(ThoughtName.StartingWork);
    }

    public void StartLevelNorasFlat()
    {
        THISLEVEL = GAMELEVEL.NorasFlat;
        Player.Instance.Spawn();

        DateTime utcNow = DateTime.UtcNow;
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        DateTime now = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);
        int hour = now.Hour;

        bool isNight;
        if (nightTimeStartsAt < nightTimeEndsAt)
            isNight = hour >= nightTimeStartsAt && hour < nightTimeEndsAt;
        else
            isNight = hour >= nightTimeStartsAt || hour < nightTimeEndsAt;

        if (isNight)
        {
            Player.Instance.NorasWardrobe.SetRandomPyjamasOutfit();
        }
        else
        {
            Player.Instance.NorasWardrobe.SetRandomMainOutfit();
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
        Player.Instance.NorasWardrobe.SetRandomMainOutfit();
        StartAudio(AudioProfile.TawleyMeats);
        EventManager.GameStartedEvent();
        EventManager.LevelLoaded();
        LoadingManager.SceneFadeIn();
        
        
        EventManager.UnlockAchievement(SteamAchievements.WorkMeating);
    }

    public void StartLevelRoarkOutside()
    {
        THISLEVEL = GAMELEVEL.RoarkOutside;
        Player.Instance.Spawn();
        Player.Instance.NorasWardrobe.SetRandomMainOutfit();
        StartAudio(AudioProfile.RoarkOutside);
        EventManager.GameStartedEvent();
        EventManager.LevelLoaded();
        LoadingManager.SceneFadeIn();
    }

    public void StartLevelRoarkInside()
    {
        THISLEVEL = GAMELEVEL.RoarkInside;
        Player.Instance.Spawn();
        Player.Instance.NorasWardrobe.SetRandomMainOutfit();
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
    SecretLevel
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
    RoarkInside
}