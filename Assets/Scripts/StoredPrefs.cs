using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class StoredPrefs : MonoBehaviour
{
    public static StoredPrefs Instance;

    [Header("Storage")]
    public bool useEncryption = true;

    public static string FilePath => Path.Combine(Application.persistentDataPath, "PlayerData.json");
    public const string EncryptionKey = "SomeVerySimpleKey";

    private static PlayerData data = new PlayerData();

    // Event to notify UI when prefs are saved
    public static event Action OnPrefsSaved;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    // ---------- PUBLIC API (PlayerPrefs style) ----------

    public static void SetString(string key, string value)
    {
        data.PlayerDatum[key] = value;
    }

    public static string GetString(string key, string defaultValue = "")
    {
        if (data.PlayerDatum.TryGetValue(key, out string value))
            return value;
        return defaultValue;
    }

    public static void SetInt(string key, int value)
    {
        data.PlayerDatum[key] = value.ToString();
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        if (data.PlayerDatum.TryGetValue(key, out string value) &&
            int.TryParse(value, out int result))
            return result;
        return defaultValue;
    }

    public static void SetFloat(string key, float value)
    {
        data.PlayerDatum[key] = value.ToString(CultureInfo.InvariantCulture);
    }

    public static float GetFloat(string key, float defaultValue = 0f)
    {
        if (data.PlayerDatum.TryGetValue(key, out string value) &&
            float.TryParse(value, out float result))
            return result;
        return defaultValue;
    }

    public static void DeleteKey(string key)
    {
        if (data.PlayerDatum.ContainsKey(key))
        {
            data.PlayerDatum.Remove(key);
            Save();
        }
    }

    public static void Save()
    {
        if (Instance == null) return;
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        if (Instance.useEncryption) json = Encrypt(json);

        string directory = Path.GetDirectoryName(FilePath);
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        File.WriteAllText(FilePath, json);
        OnPrefsSaved?.Invoke();
    }

    public static void ResetAll()
    {
        data = new PlayerData();   // clear memory
        if (File.Exists(FilePath))
            File.Delete(FilePath); // delete file
        Save();                    // create fresh blank file
    }

    // ---------- INTERNAL ----------

    private static void Load()
    {
        if (!File.Exists(FilePath))
        {
            data = new PlayerData();
            Save();
            return;
        }

        try
        {
            string json = File.ReadAllText(FilePath);

            if (Instance != null && Instance.useEncryption)
                json = Decrypt(json);

            data = JsonConvert.DeserializeObject<PlayerData>(json);

            if (data == null)
                data = new PlayerData();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"StoredPrefs load failed: {e}");
            data = new PlayerData();
        }
        
        OnPrefsSaved?.Invoke();
        GameMaster.Instance.EventManager.PlayerDataLoaded();
    }

    // ---------- SIMPLE XOR ENCRYPTION ----------

    private static string Encrypt(string text)
    {
        char[] buffer = text.ToCharArray();
        for (int i = 0; i < buffer.Length; i++)
            buffer[i] = (char)(buffer[i] ^ EncryptionKey[i % EncryptionKey.Length]);
        return new string(buffer);
    }

    public static string Decrypt(string text)
    {
        return Encrypt(text);
    }

    // Get all keys
    public static List<string> GetAllKeys()
    {
        return new List<string>(data.PlayerDatum.Keys);
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(StoredPrefs))]
public class StoredPrefsInspector : Editor
{
    private class KeyEntry
    {
        public string key;
        public string value;
        public string original;
        public bool editing;
    }

    private List<KeyEntry> entries = new();
    private Vector2 scroll;

    private string newKey = "";
    private string newValue = "";
    private bool newBoolValue = false;
    private int newTypeIndex = 0;

    private readonly string[] typeOptions = { "String", "Int", "Float", "Bool" };

    private void OnEnable()
    {
        LoadData();
        StoredPrefs.OnPrefsSaved += OnPrefsSaved;
    }

    private void OnDisable()
    {
        StoredPrefs.OnPrefsSaved -= OnPrefsSaved;
    }

    private void OnPrefsSaved()
    {
        LoadData();
        Repaint(); // Force Inspector to update
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Stored Prefs Tester", EditorStyles.boldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll);

        if (entries.Count == 0)
        {
            EditorGUILayout.HelpBox("No data stored.", MessageType.Info);
        }

        foreach (var entry in entries.ToArray())
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Key", entry.key);

            if (entry.editing)
            {
                entry.value = EditorGUILayout.TextField("Value", entry.value);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Save"))
                {
                    SaveKey(entry.key, entry.value);
                    entry.original = entry.value;
                    entry.editing = false;
                }
                if (GUILayout.Button("Cancel"))
                {
                    entry.value = entry.original;
                    entry.editing = false;
                }
                if (GUILayout.Button("Delete"))
                {
                    if (EditorUtility.DisplayDialog("Delete Key", $"Delete '{entry.key}'?", "Yes", "No"))
                    {
                        DeleteKey(entry.key);
                        LoadData();
                        GUIUtility.ExitGUI();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("Value", entry.value);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Edit")) entry.editing = true;
                if (GUILayout.Button("Delete"))
                {
                    if (EditorUtility.DisplayDialog("Delete Key", $"Delete '{entry.key}'?", "Yes", "No"))
                    {
                        DeleteKey(entry.key);
                        LoadData();
                        GUIUtility.ExitGUI();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);
        DrawAddNewKeySection();

        GUILayout.Space(10);
        if (GUILayout.Button("Open Storage Folder"))
        {
            EditorUtility.RevealInFinder(Path.Combine(Application.persistentDataPath, "PlayerData.json"));
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Reset All"))
        {
            if (EditorUtility.DisplayDialog("Reset Stored Prefs", "Delete ALL saved data?", "Yes", "No"))
            {
                StoredPrefs.ResetAll();
            }
        }
    }

    private void DrawAddNewKeySection()
    {
        EditorGUILayout.LabelField("Add New Key", EditorStyles.boldLabel);

        newTypeIndex = EditorGUILayout.Popup("Type", newTypeIndex, typeOptions);
        newKey = EditorGUILayout.TextField("Key", newKey);

        switch (newTypeIndex)
        {
            case 0:
            case 1:
            case 2:
                newValue = EditorGUILayout.TextField("Value", newValue);
                break;
            case 3:
                newBoolValue = EditorGUILayout.Toggle("Value", newBoolValue);
                break;
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save")) AddNewKey();
        if (GUILayout.Button("Cancel")) ClearNewFields();
        EditorGUILayout.EndHorizontal();
    }

    private void LoadData()
    {
        entries.Clear();

        if (!File.Exists(StoredPrefs.FilePath))
            return;

        string json = File.ReadAllText(StoredPrefs.FilePath);

        if (StoredPrefs.Instance != null && StoredPrefs.Instance.useEncryption)
            json = StoredPrefs.Decrypt(json);

        var loaded = JsonConvert.DeserializeObject<PlayerData>(json);
        if (loaded?.PlayerDatum == null) return;

        foreach (var pair in loaded.PlayerDatum)
        {
            entries.Add(new KeyEntry
            {
                key = pair.Key,
                value = pair.Value,
                original = pair.Value,
                editing = false
            });
        }

        Repaint();
    }

    private void SaveKey(string key, string value)
    {
        StoredPrefs.SetString(key, value);
        StoredPrefs.Save();
    }

    private void DeleteKey(string key)
    {
        StoredPrefs.DeleteKey(key);
    }

    private void AddNewKey()
    {
        if (string.IsNullOrEmpty(newKey))
        {
            EditorUtility.DisplayDialog("Error", "Key cannot be empty.", "OK");
            return;
        }

        switch (newTypeIndex)
        {
            case 0: StoredPrefs.SetString(newKey, newValue); break;
            case 1:
                if (int.TryParse(newValue, out int i)) StoredPrefs.SetInt(newKey, i);
                else { EditorUtility.DisplayDialog("Error", "Invalid Int", "OK"); return; }
                break;
            case 2:
                if (float.TryParse(newValue, out float f)) StoredPrefs.SetFloat(newKey, f);
                else { EditorUtility.DisplayDialog("Error", "Invalid Float", "OK"); return; }
                break;
            case 3:
                StoredPrefs.SetString(newKey, newBoolValue ? "true" : "false");
                break;
        }

        StoredPrefs.Save();
        ClearNewFields();
    }

    private void ClearNewFields()
    {
        newKey = "";
        newValue = "";
        newBoolValue = false;
        newTypeIndex = 0;
    }
}
#endif

[Serializable]
public class PlayerData
{
    public Dictionary<string, string> PlayerDatum = new Dictionary<string, string>();
}



