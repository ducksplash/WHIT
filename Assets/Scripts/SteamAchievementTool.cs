using UnityEngine;
using System;

#if STEAMWORKS_NET
using Steamworks;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SteamAchievementTool : MonoBehaviour
{
    [Header("Achievement to test")]
    public SteamAchievements achievement = SteamAchievements.NewsHound;

    [Header("Options")]
    public bool verboseLogs = true;

#if STEAMWORKS_NET
    private bool _statsReady;
    private Callback<UserStatsReceived_t> _cbUserStatsReceived;
#endif

    private void Awake()
    {
#if STEAMWORKS_NET
        _cbUserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
#endif
    }

    private void Start()
    {
#if STEAMWORKS_NET
        if (IsSteamReady())
        {
            // In modern Steamworks.NET, stats are fetched automatically on init.
            // Mark ready immediately; OnUserStatsReceived will also set this if the
            // callback fires, but we don't need to wait for it.
            _statsReady = true;
            Log("Steam is ready. Stats assumed available.");
        }
#else
        Debug.LogWarning("SteamAchievementTool: STEAMWORKS_NET symbol not defined. Add it in Player Settings > Scripting Define Symbols.");
#endif

        EventManager.OnUnlockAchievement += UnlockAchievement;
    }

    private void OnDestroy()
    {
        EventManager.OnUnlockAchievement -= UnlockAchievement;
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

    public void UnlockAchievement(SteamAchievements unlockableCheevo)
    {
        if (!EnsureStatsReady()) return;

        string cheevo = unlockableCheevo.ToString();

        bool ok = SteamUserStats.SetAchievement(cheevo);
        Log($"SetAchievement('{cheevo}') => {ok}");

        bool stored = SteamUserStats.StoreStats();
        Log($"StoreStats() => {stored}");
    }

    public void ClearSelected()
    {
        if (!EnsureStatsReady()) return;

        string apiName = achievement.ToString();

        bool ok = SteamUserStats.ClearAchievement(apiName);
        Log($"ClearAchievement('{apiName}') => {ok}");

        bool stored = SteamUserStats.StoreStats();
        Log($"StoreStats() => {stored}");
    }

    public void ClearAll()
    {
        if (!EnsureStatsReady()) return;

        uint count = SteamUserStats.GetNumAchievements();
        Log($"GetNumAchievements() => {count}");

        int cleared = 0;

        for (uint i = 0; i < count; i++)
        {
            string name = SteamUserStats.GetAchievementName(i);
            if (string.IsNullOrEmpty(name)) continue;

            if (SteamUserStats.ClearAchievement(name))
                cleared++;
        }

        Log($"Cleared {cleared}/{count} achievements. Calling StoreStats()...");

        bool stored = SteamUserStats.StoreStats();
        Log($"StoreStats() => {stored}");
    }

    public void PrintSelectedState()
    {
        if (!EnsureStatsReady()) return;

        string apiName = achievement.ToString();

        bool ok = SteamUserStats.GetAchievement(apiName, out bool achieved);
        Log($"GetAchievement('{apiName}') ok={ok} achieved={achieved}");
    }

    private bool EnsureStatsReady()
    {
        if (!IsSteamReady()) return false;

        if (!_statsReady)
        {
            Debug.LogWarning("SteamAchievementTool: Stats not ready yet. Try again in a moment.");
            return false;
        }

        return true;
    }

    private void OnUserStatsReceived(UserStatsReceived_t data)
    {
        if (data.m_nGameID != (ulong)SteamUtils.GetAppID()) return;
        if ((ulong)SteamUser.GetSteamID() != data.m_steamIDUser.m_SteamID) return;

        _statsReady = true;
        Log("UserStatsReceived callback fired: stats confirmed ready.");
    }

    private bool IsSteamReady()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("SteamAchievementTool: Steam is not initialized. Run through Steam or ensure steam_appid.txt is present for local testing.");
            return false;
        }
        return true;
    }

    private void Log(string msg)
    {
        if (verboseLogs) Debug.Log($"[SteamAchievementTool] {msg}");
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(SteamAchievementTool))]
public class SteamAchievementToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var t = (SteamAchievementTool)target;

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

    private static void CallSafe(SteamAchievementTool t, string methodName)
    {
        if (t == null) return;

        var m = t.GetType().GetMethod(methodName);
        if (m != null) m.Invoke(t, null);
        else Debug.LogError($"SteamAchievementToolEditor: Method not found: {methodName}");
    }
}
#endif

public enum SteamAchievements
{
    NewsHound,
    WorkMeating,
    Primadonna
}