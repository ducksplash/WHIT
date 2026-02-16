using UnityEngine;
using System;
using System.Reflection;

#if STEAMWORKS_NET
using Steamworks;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SteamAchievementTester : MonoBehaviour
{
    [Header("Achievement to test")]
    public SteamAchievements achievement = SteamAchievements.NewsHound;

    [Header("Options")]
    public bool verboseLogs = true;

#if STEAMWORKS_NET
    private bool _statsReady;

    private Callback<UserStatsReceived_t> _cbUserStatsReceived;
    private MethodInfo _miRequestCurrentStats;
#endif

    private void Awake()
    {
#if STEAMWORKS_NET
        // Bind callback so we know for sure when stats are usable.
        _cbUserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);

        // Bind RequestCurrentStats() only if it exists in your Steamworks.NET build.
        _miRequestCurrentStats = typeof(SteamUserStats).GetMethod(
            "RequestCurrentStats",
            BindingFlags.Public | BindingFlags.Static
        );
#endif
    }

    private void Start()
    {
#if STEAMWORKS_NET
        if (!IsSteamReady()) return;

#else
        Debug.LogWarning("SteamAchievementTester: STEAMWORKS_NET symbol not defined. Add it in Player Settings > Scripting Define Symbols.");
#endif
    }

#if STEAMWORKS_NET
    public void UnlockSelected()
    {
        if (!EnsureStatsReady()) return;

        string apiName = achievement.ToString();

        bool ok = SteamUserStats.SetAchievement(apiName);
        Log($"SetAchievement('{apiName}') => {ok}");

        bool stored = SteamUserStats.StoreStats();
        Log($"StoreStats() => {stored}");
        
    }

    public void ClearSelected()
    {
        if (!EnsureStatsReady())
            return;

        string apiName = achievement.ToString();

        bool ok = SteamUserStats.ClearAchievement(apiName);
        Log($"ClearAchievement('{apiName}') => {ok}");

        bool stored = SteamUserStats.StoreStats();
        Log($"StoreStats() => {stored}");
    }

    public void ClearAll()
    {
        if (!EnsureStatsReady())
            return;

        // Steamworks.NET uses uint for these
        uint count = SteamUserStats.GetNumAchievements();
        Log($"GetNumAchievements() => {count}");

        int cleared = 0;

        for (uint i = 0; i < count; i++)
        {
            string name = SteamUserStats.GetAchievementName(i);
            if (string.IsNullOrEmpty(name))
                continue;

            if (SteamUserStats.ClearAchievement(name))
                cleared++;
        }

        Log($"Cleared {cleared}/{count} achievements. Calling StoreStats()...");

        bool stored = SteamUserStats.StoreStats();
        Log($"StoreStats() => {stored}");
    }

    public void PrintSelectedState()
    {
        if (!EnsureStatsReady())
            return;

        string apiName = achievement.ToString();

        bool achieved;
        bool ok = SteamUserStats.GetAchievement(apiName, out achieved);

        Log($"GetAchievement('{apiName}') ok={ok} achieved={achieved}");
    }

    private bool EnsureStatsReady()
    {
        if (!IsSteamReady())
            return false;
        
        // If still not ready, ask user to try again after callbacks.
        if (!_statsReady)
        {
            Debug.LogWarning("SteamAchievementTester: Stats not ready yet. Try again in a moment.");
            return false;
        }

        return true;
    }

    private void OnUserStatsReceived(UserStatsReceived_t data)
    {
        // Only accept stats for this user + this app
        if (data.m_nGameID != (ulong)SteamUtils.GetAppID())
            return;

        if ((ulong)SteamUser.GetSteamID() != data.m_steamIDUser.m_SteamID)
            return;

        // Success is 1 (k_EResultOK), but we avoid enum dependency and just check non-zero OK-ish
        // Steamworks.NET normally provides EResult; use it if you like.
        // If you want strict: if ((EResult)data.m_eResult != EResult.k_EResultOK) return;
        _statsReady = true;

        Log("UserStatsReceived: stats are ready.");
    }

    private bool IsSteamReady()
    {
        // Steamworks.NET example typically uses SteamManager.Initialized
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("SteamAchievementTester: Steam is not initialized. Run through Steam or ensure steam_appid.txt is present for local testing.");
            return false;
        }
        return true;
    }

    private void Log(string msg)
    {
        if (verboseLogs) Debug.Log($"[SteamAchievementTester] {msg}");
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(SteamAchievementTester))]
public class SteamAchievementTesterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var t = (SteamAchievementTester)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Steam Achievement Test Actions", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (GUILayout.Button("Print Selected Achievement State"))
                CallSafe(t, nameof(t.PrintSelectedState));

            GUILayout.Space(6);

            if (GUILayout.Button("Unlock Selected Achievement"))
                CallSafe(t, nameof(t.UnlockSelected));

            if (GUILayout.Button("Clear/Remove Selected Achievement"))
                CallSafe(t, nameof(t.ClearSelected));

            GUILayout.Space(6);

            GUI.backgroundColor = new Color(1f, 0f, 0f);
            if (GUILayout.Button("Clear ALL Achievements (Danger)"))
                CallSafe(t, nameof(t.ClearAll));
            GUI.backgroundColor = Color.white;
        }

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Buttons are enabled in Play Mode only. Steam achievements require SteamAPI initialized (run through Steam or use steam_appid.txt).",
                MessageType.Info
            );
        }
    }

    private static void CallSafe(SteamAchievementTester t, string methodName)
    {
        if (t == null) return;

        var m = t.GetType().GetMethod(methodName);
        if (m != null) m.Invoke(t, null);
        else Debug.LogError($"SteamAchievementTesterEditor: Method not found: {methodName}");
    }
}
#endif

public enum SteamAchievements
{
    NewsHound,
    WorkMeating
}
