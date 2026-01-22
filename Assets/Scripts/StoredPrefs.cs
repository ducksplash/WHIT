using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;

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
    private static bool isLoaded = false;

    public static event Action OnPrefsSaved;

    private const string COLLECTION_PREFIX = "logs:";

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureLoaded();
    }

    // ===================== LOAD SAFETY =====================

    private void EnsureLoaded()
    {
        if (isLoaded) return;

        // If called too early, don't lock us into a "loaded" state.
        if (Instance == null)
            return;

        isLoaded = true;
        Load();
    }

    

    public void SetString(string key, string value)
    {
        EnsureLoaded();
        data.PlayerDatum[key] = value;
    }

    public string GetString(string key, string defaultValue = "")
    {
        EnsureLoaded();
        return data.PlayerDatum.TryGetValue(key, out var v) ? v : defaultValue;
    }

    public void SetInt(string key, int value)
    {
        EnsureLoaded();
        data.PlayerDatum[key] = value.ToString();
    }

    public int GetInt(string key, int defaultValue = 0)
    {
        EnsureLoaded();
        return data.PlayerDatum.TryGetValue(key, out var v) && int.TryParse(v, out var r)
            ? r
            : defaultValue;
    }

    public void SetFloat(string key, float value)
    {
        EnsureLoaded();
        data.PlayerDatum[key] = value.ToString(CultureInfo.InvariantCulture);
    }

    public float GetFloat(string key, float defaultValue = 0f)
    {
        EnsureLoaded();
        return data.PlayerDatum.TryGetValue(key, out var v) &&
               float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var r)
            ? r
            : defaultValue;
    }

    public void DeleteKey(string key)
    {
        EnsureLoaded();
        if (data.PlayerDatum.Remove(key)) Save();
    }

    public List<string> GetAllKeys()
    {
        EnsureLoaded();
        return new List<string>(data.PlayerDatum.Keys);
    }

    // ===================== COLLECTION API =====================

    public void SetCollection<T>(string key, T collection, CollectionType type)
    {
        EnsureLoaded();

        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        switch (type)
        {
            case CollectionType.dictionary: if (!(collection is System.Collections.IDictionary)) throw new ArgumentException("Expected Dictionary"); break;
            case CollectionType.list: if (!(collection is System.Collections.IList)) throw new ArgumentException("Expected List"); break;
            case CollectionType.array: if (!collection.GetType().IsArray) throw new ArgumentException("Expected Array"); break;
        }

        data.PlayerDatum[COLLECTION_PREFIX + key] = JsonConvert.SerializeObject(collection, Formatting.None);
    }

    public T GetCollection<T>(string key) where T : new()
    {
        EnsureLoaded();

        if (!data.PlayerDatum.TryGetValue(COLLECTION_PREFIX + key, out var json) ||
            string.IsNullOrEmpty(json))
        {
            return new T(); // SAFE empty collection
        }

        try
        {
            return JsonConvert.DeserializeObject<T>(json) ?? new T();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"GetCollection failed for '{key}': {e}");
            return new T();
        }
    }

    // ===================== SAVE / LOAD =====================

    public void Save()
    {
        Debug.Log("save called");
        EnsureLoaded();

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);

        if (Instance != null && Instance.useEncryption)
            json = Encrypt(json);

        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, json);

        OnPrefsSaved?.Invoke();
    }

    private void Load()
    {
        if (!File.Exists(FilePath))
        {
            data = new PlayerData();
            return;
        }

        try
        {
            string json = File.ReadAllText(FilePath);
            if (Instance.useEncryption)
                json = Decrypt(json);

            data = JsonConvert.DeserializeObject<PlayerData>(json) ?? new PlayerData();
        }
        catch
        {
            data = new PlayerData();
        }

        OnPrefsSaved?.Invoke();
        GameMaster.Instance.EventManager.PlayerDataLoaded();
    }

    public void ResetAll()
    {
        data = new PlayerData();
        isLoaded = true;

        if (File.Exists(FilePath))
            File.Delete(FilePath);

        Save();
    }

    // ===================== ENCRYPTION =====================

    private static string Encrypt(string text)
    {
        char[] buffer = text.ToCharArray();
        for (int i = 0; i < buffer.Length; i++)
            buffer[i] = (char)(buffer[i] ^ EncryptionKey[i % EncryptionKey.Length]);
        return new string(buffer);
    }

    public static string Decrypt(string text) => Encrypt(text);
    
    

}


[Serializable]
public class PlayerData
{
    public Dictionary<string, string> PlayerDatum = new();
}

public enum CollectionType
{
    dictionary,
    list,
    array
}
