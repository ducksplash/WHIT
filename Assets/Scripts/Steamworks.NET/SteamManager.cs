// The SteamManager is designed to work with Steamworks.NET
// This file is released into the public domain.
// Where that dedication is not recognized you are granted a perpetual,
// irrevocable license to copy and modify this file as you see fit.
//
// Version: 1.0.13 (modified)

#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using UnityEngine;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif

[DisallowMultipleComponent]
public class SteamManager : MonoBehaviour
{
#if !DISABLESTEAMWORKS
    protected static bool s_EverInitialized = false;

    protected static SteamManager s_instance;
    protected static SteamManager Instance
    {
        get
        {
            if (s_instance == null)
            {
                return new GameObject("SteamManager").AddComponent<SteamManager>();
            }
            else
            {
                return s_instance;
            }
        }
    }

    protected bool m_bInitialized = false;
    public static bool Initialized => Instance.m_bInitialized;

    protected SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

    [Header("Steam Behaviour")]
    [Tooltip("If TRUE: game will quit if Steam is not available / Steam API can't init.\nIf FALSE: game will continue without Steam.")]
    public bool enforceSteam = false;

    [Tooltip("Your Steam AppID. Used only when enforceSteam is enabled (RestartAppIfNecessary).")]
    public uint appId = 4071480;

    [AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
    protected static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
    {
        Debug.LogWarning(pchDebugText);
    }

#if UNITY_2019_3_OR_NEWER
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void InitOnPlayMode()
    {
        s_EverInitialized = false;
        s_instance = null;
    }
#endif

    protected virtual void Awake()
    {
        // Only one instance of SteamManager at a time!
        if (s_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        s_instance = this;

        if (s_EverInitialized)
        {
            throw new System.Exception("Tried to Initialize the SteamAPI twice in one session!");
        }

        DontDestroyOnLoad(gameObject);

        if (!Packsize.Test())
        {
            Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
        }

        if (!DllCheck.Test())
        {
            Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
        }

        // -----------------------------
        // OPTIONAL DRM / Steam-enforced launch
        // -----------------------------
        // Only do RestartAppIfNecessary when we are enforcing Steam.
        // This prevents dev builds / direct exe runs from auto-quitting.
        if (enforceSteam)
        {
            try
            {
                if (SteamAPI.RestartAppIfNecessary((AppId_t)appId))
                {
                    Debug.Log("[Steamworks.NET] Shutting down because RestartAppIfNecessary returned true. Steam will restart the application.");
                    Application.Quit();
                    return;
                }
            }
            catch (System.DllNotFoundException e)
            {
                Debug.LogError("[Steamworks.NET] Could not load steam_api dll/so/dylib.\n" + e, this);

                // If Steam is required, quit. Otherwise continue without Steam.
                if (enforceSteam)
                {
                    Application.Quit();
                }
                return;
            }
        }

        // -----------------------------
        // Try init Steam. If it fails:
        // - enforceSteam=true -> quit
        // - enforceSteam=false -> continue without Steam
        // -----------------------------
        try
        {
            m_bInitialized = SteamAPI.Init();
        }
        catch (System.DllNotFoundException e)
        {
            Debug.LogWarning("[Steamworks.NET] Steam binaries missing, continuing without Steam.\n" + e, this);
            m_bInitialized = false;
        }

        if (!m_bInitialized)
        {
            Debug.LogWarning("[Steamworks.NET] SteamAPI_Init() failed. Continuing without Steam (enforceSteam is " + enforceSteam + ").", this);

            if (enforceSteam)
            {
                Application.Quit();
            }
            return;
        }

        s_EverInitialized = true;
    }

    protected virtual void OnEnable()
    {
        if (s_instance == null)
        {
            s_instance = this;
        }

        if (!m_bInitialized)
        {
            return;
        }

        if (m_SteamAPIWarningMessageHook == null)
        {
            m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
            SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
        }
    }

    protected virtual void OnDestroy()
    {
        if (s_instance != this)
        {
            return;
        }

        s_instance = null;

        if (!m_bInitialized)
        {
            return;
        }

        SteamAPI.Shutdown();
    }

    protected virtual void Update()
    {
        if (!m_bInitialized)
        {
            return;
        }

        SteamAPI.RunCallbacks();
    }

#else
    public static bool Initialized => false;
#endif // !DISABLESTEAMWORKS
}
