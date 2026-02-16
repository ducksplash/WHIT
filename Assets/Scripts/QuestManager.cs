using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class QuestManager : MonoBehaviour
{
    [Header("Quests")]
    [Tooltip("Populate this list with your QuestText ScriptableObjects (order matters).")]
    public List<QuestText> QuestList = new List<QuestText>();

    [Header("Runtime State (read-only)")]
    [SerializeField] private QuestText _currentQuestAsset;

    /// <summary>
    /// Runtime clone used ONLY for broadcasting to existing systems that expect QuestText.
    /// Reused (not recreated) to avoid GC allocations.
    /// </summary>
    public QuestText CurrentQuest { get; private set; }

    public event Action<QuestText> QuestLoaded;

    private const string PrefKey_CurrentQuest = "CurrentQuest";

    private const string Obj1Key = "Obj1Complete";
    private const string Obj2Key = "Obj2Complete";
    private const string Obj3Key = "Obj3Complete";
    private const string Obj4Key = "Obj4Complete";
    private const string Obj5Key = "Obj5Complete";

    // ✅ NEW KEYS
    private const string QuestStartedKey = "QuestStarted";
    private const string QuestCompletedKey = "QuestCompleted";

    // Fast lookup by enum (avoids LINQ/GC)
    private readonly Dictionary<QuestName, QuestText> _questByEnum = new Dictionary<QuestName, QuestText>();

    // Cache last broadcasted state to avoid redundant broadcasts/writes if nothing changed
    private QuestName _lastQuestEnum;
    private bool _lastO1, _lastO2, _lastO3, _lastO4, _lastO5;
    private bool _lastStarted, _lastCompleted;
    private bool _hasLastSnapshot;

    private void Awake()
    {
        BuildLookup();
    }

    private void OnValidate()
    {
        BuildLookup();
    }

    private void Start()
    {
        StoredPrefs.OnPlayerDataLoaded += ResolveQuestFromPrefs;
    }

    private void OnDisable()
    {
        StoredPrefs.OnPlayerDataLoaded -= ResolveQuestFromPrefs;
    }

    // =====================================================================================
    // PUBLIC API
    // =====================================================================================

    /// <summary>
    /// Assign Quest takes one parameter, an enum, and this is used to select and apply a Quest.
    /// Result: CurrentQuest set, StoredPrefs updated, QuestLoaded invoked.
    /// </summary>
    public void AssignQuest(QuestName questEnum)
    {
        if (!TryGetQuestAsset(questEnum, out var questAsset))
        {
            Debug.LogWarning($"[QuestManager] AssignQuest failed: no QuestText found for {questEnum}");
            return;
        }

        _currentQuestAsset = questAsset;

        if (StoredPrefs.Instance != null)
            StoredPrefs.Instance.SetString(PrefKey_CurrentQuest, questEnum.ToString());

        EnsureRuntimeClone();
        ApplyAssetToRuntimeClone(_currentQuestAsset);

        // Load objective completion
        LoadObjectivesIntoRuntimeClone(questEnum);

        // Load started/completed
        LoadQuestFlagsIntoRuntimeClone(questEnum);

        // Compute completion from objectives (authoritative)
        bool nowCompleted = AreAllObjectivesComplete(CurrentQuest);
        if (nowCompleted && !CurrentQuest.QuestCompleted)
        {
            CurrentQuest.QuestCompleted = true;
            SaveQuestBool_NoSave(questEnum, QuestCompletedKey, true);
        }

        try { StoredPrefs.Instance?.Save(); } catch { /* ignore */ }

        // Mark "started" after first time player has "seen" it (first broadcast)
        MarkStartedIfFirstTime(questEnum);

        BroadcastIfChanged(force: true);
    }

    /// <summary>
    /// UpdateQuestObjectives takes two parameters: a quest enum, and an integer between 1 and 5.
    /// This sets the corresponding objective to completed, saves, and invokes QuestLoaded.
    /// </summary>
    public void UpdateQuestObjectives(QuestName questEnum, int objectiveIndex1To5)
    {
        if (objectiveIndex1To5 < 1 || objectiveIndex1To5 > 5)
        {
            Debug.LogWarning("[QuestManager] UpdateQuestObjectives: objectiveIndex must be 1..5.");
            return;
        }

        if (!TryGetQuestAsset(questEnum, out var questAsset))
        {
            Debug.LogWarning($"[QuestManager] UpdateQuestObjectives failed: no QuestText found for {questEnum}");
            return;
        }

        _currentQuestAsset = questAsset;

        if (StoredPrefs.Instance != null)
        {
            StoredPrefs.Instance.SetString(PrefKey_CurrentQuest, questEnum.ToString());

            switch (objectiveIndex1To5)
            {
                case 1: SaveObjectiveBool_NoSave(questEnum, Obj1Key, true); break;
                case 2: SaveObjectiveBool_NoSave(questEnum, Obj2Key, true); break;
                case 3: SaveObjectiveBool_NoSave(questEnum, Obj3Key, true); break;
                case 4: SaveObjectiveBool_NoSave(questEnum, Obj4Key, true); break;
                case 5: SaveObjectiveBool_NoSave(questEnum, Obj5Key, true); break;
            }
        }

        EnsureRuntimeClone();
        ApplyAssetToRuntimeClone(_currentQuestAsset);

        // Update runtime objective without re-reading everything
        MarkObjectiveOnRuntimeClone(objectiveIndex1To5, true);

        // Load started/completed flags from prefs into runtime
        LoadQuestFlagsIntoRuntimeClone(questEnum);

        // Mark started if first time shown (first broadcast)
        // (If objectives update happens first, it still counts as "seen")
        MarkStartedIfFirstTime(questEnum);

        // If all objectives complete -> mark completed + save
        bool nowCompleted = AreAllObjectivesComplete(CurrentQuest);
        if (nowCompleted && !CurrentQuest.QuestCompleted)
        {
            CurrentQuest.QuestCompleted = true;
            SaveQuestBool_NoSave(questEnum, QuestCompletedKey, true);
        }

        try { StoredPrefs.Instance?.Save(); } catch { /* ignore */ }

        BroadcastIfChanged(force: false);
    }

    // =====================================================================================
    // LOAD / RESOLVE
    // =====================================================================================

    private void ResolveQuestFromPrefs()
    {
        if (QuestList == null || QuestList.Count == 0)
        {
            Debug.LogWarning("[QuestManager] QuestList is empty. Cannot resolve current quest.");
            _currentQuestAsset = null;
            CurrentQuest = null;
            return;
        }

        QuestName resolvedEnum = QuestList[0].QuestName; // fallback
        try
        {
            var stored = StoredPrefs.Instance != null
                ? StoredPrefs.Instance.GetString(PrefKey_CurrentQuest, QuestList[0].QuestName.ToString())
                : QuestList[0].QuestName.ToString();

            if (!string.IsNullOrEmpty(stored) && Enum.TryParse(stored, true, out QuestName parsed))
                resolvedEnum = parsed;
        }
        catch { /* ignore */ }

        if (!TryGetQuestAsset(resolvedEnum, out var questAsset))
        {
            questAsset = FirstNonNullQuest();
            if (questAsset == null)
            {
                Debug.LogWarning("[QuestManager] QuestList contains only nulls.");
                _currentQuestAsset = null;
                CurrentQuest = null;
                return;
            }
            resolvedEnum = questAsset.QuestName;
        }

        _currentQuestAsset = questAsset;

        EnsureRuntimeClone();
        ApplyAssetToRuntimeClone(_currentQuestAsset);
        LoadObjectivesIntoRuntimeClone(resolvedEnum);
        LoadQuestFlagsIntoRuntimeClone(resolvedEnum);

        // If objectives already complete, ensure completed flag is set (authoritative)
        bool nowCompleted = AreAllObjectivesComplete(CurrentQuest);
        if (nowCompleted && !CurrentQuest.QuestCompleted)
        {
            CurrentQuest.QuestCompleted = true;
            SaveQuestBool_NoSave(resolvedEnum, QuestCompletedKey, true);
            try { StoredPrefs.Instance?.Save(); } catch { /* ignore */ }
        }

        BroadcastIfChanged(force: true);

        // Mark started after first time player "sees" it (this broadcast counts)
        MarkStartedIfFirstTime(resolvedEnum);
    }

    // =====================================================================================
    // RUNTIME CLONE (REUSED)
    // =====================================================================================

    private void EnsureRuntimeClone()
    {
        if (CurrentQuest != null) return;
        CurrentQuest = ScriptableObject.CreateInstance<QuestText>();
    }

    private void ApplyAssetToRuntimeClone(QuestText asset)
    {
        if (asset == null || CurrentQuest == null) return;

        CurrentQuest.QuestTitleString = asset.QuestTitleString;

        CurrentQuest.Objective1String = asset.Objective1String;
        CurrentQuest.Objective2String = asset.Objective2String;
        CurrentQuest.Objective3String = asset.Objective3String;
        CurrentQuest.Objective4String = asset.Objective4String;
        CurrentQuest.Objective5String = asset.Objective5String;

        CurrentQuest.QuestName = asset.QuestName;
        CurrentQuest.QuestLanguage = asset.QuestLanguage;
        CurrentQuest.QuestTextColor = asset.QuestTextColor;

        CurrentQuest.IrishQuestTextString = asset.IrishQuestTextString;
        CurrentQuest.FrenchQuestTextString = asset.FrenchQuestTextString;
        CurrentQuest.GermanQuestTextString = asset.GermanQuestTextString;
        CurrentQuest.SpanishQuestTextString = asset.SpanishQuestTextString;
        CurrentQuest.KoreanQuestTextString = asset.KoreanQuestTextString;
        CurrentQuest.ArabicQuestTextString = asset.ArabicQuestTextString;
        CurrentQuest.JapaneseQuestTextString = asset.JapaneseQuestTextString;
        CurrentQuest.ChineseQuestTextString = asset.ChineseQuestTextString;
        CurrentQuest.RussianQuestTextString = asset.RussianQuestTextString;

        // ✅ NEW fields exist on ScriptableObject definition but we DO NOT copy them as "state".
        // Runtime state is loaded from prefs below.
    }

    private void LoadObjectivesIntoRuntimeClone(QuestName questEnum)
    {
        if (CurrentQuest == null) return;

        CurrentQuest.Objective1Complete = LoadObjectiveBool(questEnum, Obj1Key, false);
        CurrentQuest.Objective2Complete = LoadObjectiveBool(questEnum, Obj2Key, false);
        CurrentQuest.Objective3Complete = LoadObjectiveBool(questEnum, Obj3Key, false);
        CurrentQuest.Objective4Complete = LoadObjectiveBool(questEnum, Obj4Key, false);
        CurrentQuest.Objective5Complete = LoadObjectiveBool(questEnum, Obj5Key, false);
    }

    private void LoadQuestFlagsIntoRuntimeClone(QuestName questEnum)
    {
        if (CurrentQuest == null) return;

        CurrentQuest.QuestStarted = LoadQuestBool(questEnum, QuestStartedKey, false);
        CurrentQuest.QuestCompleted = LoadQuestBool(questEnum, QuestCompletedKey, false);
    }

    private void MarkObjectiveOnRuntimeClone(int idx1To5, bool complete)
    {
        if (CurrentQuest == null) return;

        switch (idx1To5)
        {
            case 1: CurrentQuest.Objective1Complete = complete; break;
            case 2: CurrentQuest.Objective2Complete = complete; break;
            case 3: CurrentQuest.Objective3Complete = complete; break;
            case 4: CurrentQuest.Objective4Complete = complete; break;
            case 5: CurrentQuest.Objective5Complete = complete; break;
        }
    }

    private static bool AreAllObjectivesComplete(QuestText q)
    {
        if (q == null) return false;

        // Treat empty objective strings as "not used" (so quests with <5 objectives can still complete)
        bool o1 = string.IsNullOrEmpty(q.Objective1String) || q.Objective1Complete;
        bool o2 = string.IsNullOrEmpty(q.Objective2String) || q.Objective2Complete;
        bool o3 = string.IsNullOrEmpty(q.Objective3String) || q.Objective3Complete;
        bool o4 = string.IsNullOrEmpty(q.Objective4String) || q.Objective4Complete;
        bool o5 = string.IsNullOrEmpty(q.Objective5String) || q.Objective5Complete;

        return o1 && o2 && o3 && o4 && o5;
    }

    private void MarkStartedIfFirstTime(QuestName questEnum)
    {
        if (CurrentQuest == null) return;

        if (!CurrentQuest.QuestStarted)
        {
            CurrentQuest.QuestStarted = true;
            SaveQuestBool_NoSave(questEnum, QuestStartedKey, true);

            // Save once for this transition
            try { StoredPrefs.Instance?.Save(); } catch { /* ignore */ }
        }
    }

    // =====================================================================================
    // PREFS HELPERS (NO LINQ / NO EXTRA SAVE)
    // =====================================================================================

    private static string MakeObjectivePrefKey(QuestName questName, string objectiveKey)
        => $"Quest:{questName}:{objectiveKey}";

    private static bool LoadObjectiveBool(QuestName questName, string objectiveKey, bool defaultValue)
    {
        try
        {
            if (StoredPrefs.Instance == null) return defaultValue;
            int dv = defaultValue ? 1 : 0;
            int v = StoredPrefs.Instance.GetInt(MakeObjectivePrefKey(questName, objectiveKey), dv);
            return v != 0;
        }
        catch
        {
            return defaultValue;
        }
    }

    private static void SaveObjectiveBool_NoSave(QuestName questName, string objectiveKey, bool value)
    {
        try
        {
            if (StoredPrefs.Instance == null) return;
            StoredPrefs.Instance.SetInt(MakeObjectivePrefKey(questName, objectiveKey), value ? 1 : 0);
        }
        catch { /* ignore */ }
    }

    // ✅ NEW: QuestStarted / QuestCompleted helpers
    private static bool LoadQuestBool(QuestName questName, string key, bool defaultValue)
        => LoadObjectiveBool(questName, key, defaultValue);

    private static void SaveQuestBool_NoSave(QuestName questName, string key, bool value)
        => SaveObjectiveBool_NoSave(questName, key, value);

    // =====================================================================================
    // LOOKUP / UTIL
    // =====================================================================================

    private void BuildLookup()
    {
        _questByEnum.Clear();
        if (QuestList == null) return;

        for (int i = 0; i < QuestList.Count; i++)
        {
            var q = QuestList[i];
            if (q == null) continue;
            _questByEnum[q.QuestName] = q;
        }
    }

    private bool TryGetQuestAsset(QuestName questEnum, out QuestText questAsset)
    {
        if (_questByEnum.Count == 0) BuildLookup();
        return _questByEnum.TryGetValue(questEnum, out questAsset) && questAsset != null;
    }

    private QuestText FirstNonNullQuest()
    {
        if (QuestList == null) return null;
        for (int i = 0; i < QuestList.Count; i++)
            if (QuestList[i] != null) return QuestList[i];
        return null;
    }

    // =====================================================================================
    // BROADCAST (AVOID REDUNDANT UI UPDATES)
    // =====================================================================================

    private void BroadcastIfChanged(bool force)
    {
        if (CurrentQuest == null) return;

        var q = CurrentQuest.QuestName;
        bool o1 = CurrentQuest.Objective1Complete;
        bool o2 = CurrentQuest.Objective2Complete;
        bool o3 = CurrentQuest.Objective3Complete;
        bool o4 = CurrentQuest.Objective4Complete;
        bool o5 = CurrentQuest.Objective5Complete;

        bool started = CurrentQuest.QuestStarted;
        bool completed = CurrentQuest.QuestCompleted;

        if (!force && _hasLastSnapshot)
        {
            if (_lastQuestEnum.Equals(q) &&
                _lastO1 == o1 && _lastO2 == o2 && _lastO3 == o3 && _lastO4 == o4 && _lastO5 == o5 &&
                _lastStarted == started && _lastCompleted == completed)
            {
                return;
            }
        }

        _lastQuestEnum = q;
        _lastO1 = o1; _lastO2 = o2; _lastO3 = o3; _lastO4 = o4; _lastO5 = o5;
        _lastStarted = started; _lastCompleted = completed;
        _hasLastSnapshot = true;

        BroadcastQuestLoaded(CurrentQuest);
    }

    private void BroadcastQuestLoaded(QuestText questRuntime)
    {
        try
        {
            if (GameMaster.Instance != null && GameMaster.Instance.EventManager != null)
                GameMaster.Instance.EventManager.QuestLoaded(questRuntime);
        }
        catch { /* ignore */ }

        QuestLoaded?.Invoke(questRuntime);
    }

    // =====================================================================================
    // OPTIONAL: KEEP YOUR OLD API
    // =====================================================================================

    public void UpdateQuest(QuestText questAsset)
    {
        if (questAsset == null) return;
        AssignQuest(questAsset.QuestName);
    }
}

public enum TextColour
{
    red, green, blue, white, black, yellow, cyan, magenta, gray, grey, clear
}

public enum QuestName
{
    GetReadyForWork,
    FindSomeEvidence
}

public static class TextColours
{
    public static Color ToColor(this TextColour colour)
    {
        return colour switch
        {
            TextColour.white => Color.white,
            TextColour.black => Color.black,
            TextColour.red => Color.red,
            TextColour.green => Color.green,
            TextColour.blue => Color.blue,
            TextColour.yellow => Color.yellow,
            TextColour.cyan => Color.cyan,
            TextColour.magenta => Color.magenta,
            TextColour.grey => Color.grey,
            TextColour.clear => Color.clear,
            _ => Color.white
        };
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(QuestManager))]
public class QuestManagerEditor : Editor
{
    private QuestName _selectedQuestEnum;
    private int _objectiveIndex = 1;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var mgr = (QuestManager)target;
        if (mgr == null) return;

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Quest Debug Controls (Play Mode)", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to use debug buttons.", MessageType.Info);
            return;
        }

        _selectedQuestEnum = (QuestName)EditorGUILayout.EnumPopup("QuestName", _selectedQuestEnum);

        if (GUILayout.Button("AssignQuest(QuestName)")) mgr.AssignQuest(_selectedQuestEnum);

        EditorGUILayout.Space(6);
        _objectiveIndex = EditorGUILayout.IntSlider("Objective Index", _objectiveIndex, 1, 5);

        if (GUILayout.Button("UpdateQuestObjectives(QuestName, Index)"))
            mgr.UpdateQuestObjectives(_selectedQuestEnum, _objectiveIndex);

        if (mgr.CurrentQuest != null)
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("CurrentQuest Snapshot", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Quest:", mgr.CurrentQuest.QuestName.ToString());
            EditorGUILayout.LabelField("Started:", mgr.CurrentQuest.QuestStarted ? "True" : "False");
            EditorGUILayout.LabelField("Completed:", mgr.CurrentQuest.QuestCompleted ? "True" : "False");
            EditorGUILayout.LabelField("Obj1:", mgr.CurrentQuest.Objective1Complete ? "Complete" : "Incomplete");
            EditorGUILayout.LabelField("Obj2:", mgr.CurrentQuest.Objective2Complete ? "Complete" : "Incomplete");
            EditorGUILayout.LabelField("Obj3:", mgr.CurrentQuest.Objective3Complete ? "Complete" : "Incomplete");
            EditorGUILayout.LabelField("Obj4:", mgr.CurrentQuest.Objective4Complete ? "Complete" : "Incomplete");
            EditorGUILayout.LabelField("Obj5:", mgr.CurrentQuest.Objective5Complete ? "Complete" : "Incomplete");
        }
    }
}
#endif
