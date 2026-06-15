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
            _statsReady = true;
            Log("Steam is ready.");
        }
#endif

        EventManager.OnUnlockAchievement += UnlockAchievement;
    }

    private void OnDestroy()
    {
        EventManager.OnUnlockAchievement -= UnlockAchievement;
    }

#if STEAMWORKS_NET

    public void UnlockAchievement(SteamAchievements achievement)
    {
        if (!EnsureStatsReady()) return;
        string apiName = achievement.ToString();
        bool ok = SteamUserStats.SetAchievement(apiName);
        Log($"SetAchievement('{apiName}') => {ok}");
        SteamUserStats.StoreStats();
    }

    public void ClearAchievement(SteamAchievements achievement)
    {
        if (!EnsureStatsReady()) return;
        string apiName = achievement.ToString();
        bool ok = SteamUserStats.ClearAchievement(apiName);
        Log($"ClearAchievement('{apiName}') => {ok}");
        SteamUserStats.StoreStats();
    }

    public void ClearAll()
    {
        if (!EnsureStatsReady()) return;

        uint count = SteamUserStats.GetNumAchievements();
        int cleared = 0;

        for (uint i = 0; i < count; i++)
        {
            string name = SteamUserStats.GetAchievementName(i);
            if (string.IsNullOrEmpty(name)) continue;
            if (SteamUserStats.ClearAchievement(name)) cleared++;
        }

        Log($"Cleared {cleared} achievements for current user.");
        SteamUserStats.StoreStats();
    }

    
    
    #if UNITY_EDITOR
    
    // Removed "For All Players" — impossible via Steam API
    public void ResetAllAchievementsCurrentUser()
    {
        if (!EnsureStatsReady()) return;

        if (EditorUtility.DisplayDialog("Destructive Action",
            "This will RESET ALL achievements for the CURRENTLY LOGGED IN Steam user.\n\nThis cannot be undone easily.\n\nContinue?",
            "Yes, Reset All", "Cancel"))
        {
            ClearAll();
        }
    }

    public void PrintAchievementState(SteamAchievements achievement)
    {
        if (!EnsureStatsReady()) return;
        string apiName = achievement.ToString();
        bool ok = SteamUserStats.GetAchievement(apiName, out bool achieved);
        Log($"[{apiName}] Achieved = {achieved}");
    }

    #endif
    
    
    
    private bool EnsureStatsReady()
    {
        if (!IsSteamReady()) return false;
        if (!_statsReady)
        {
            Debug.LogWarning("SteamAchievementTool: Stats not ready yet.");
            return false;
        }
        return true;
    }

    private void OnUserStatsReceived(UserStatsReceived_t data)
    {
        if (data.m_nGameID != (ulong)SteamUtils.GetAppID()) return;
        if ((ulong)SteamUser.GetSteamID() != data.m_steamIDUser.m_SteamID) return;
        _statsReady = true;
    }

    private bool IsSteamReady()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("SteamAchievementTool: Steam not initialized.");
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

// ====================== EDITOR ======================
#if UNITY_EDITOR
[CustomEditor(typeof(SteamAchievementTool))]
public class SteamAchievementToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var t = (SteamAchievementTool)target;

        GUILayout.Space(12);
        EditorGUILayout.LabelField("Achievement Controls", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            foreach (SteamAchievements ach in System.Enum.GetValues(typeof(SteamAchievements)))
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(ach.ToString(), GUILayout.Width(160));

                if (GUILayout.Button("Unlock", GUILayout.Width(70)))
                    CallSafe(t, nameof(t.UnlockAchievement), ach);

                if (GUILayout.Button("Clear", GUILayout.Width(70)))
                    CallSafe(t, nameof(t.ClearAchievement), ach);

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Clear ALL Achievements", GUILayout.Height(30))) CallSafe(t, nameof(t.ClearAll));
            
        }

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to use the buttons.", MessageType.Info);
        }
    }

    private static void CallSafe(SteamAchievementTool t, string methodName, object param = null)
    {
        if (t == null) return;
        var method = t.GetType().GetMethod(methodName);
        if (method != null)
            method.Invoke(t, param != null ? new object[] { param } : null);
        else
            Debug.LogError($"Method not found: {methodName}");
    }
}
#endif

public enum SteamAchievements
{
    NewsHound,
    WorkMeating,
    Primadonna
    // Add new achievements here
}