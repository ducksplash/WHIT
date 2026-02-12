using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;

public class StoredPrefs : MonoBehaviour
{
    public static StoredPrefs Instance;

    [Header("Storage")]
    public bool useEncryption = true;

    public static string FilePath => Path.Combine(Application.persistentDataPath, "PlayerData.json");
    public const string EncryptionKey = "SomeVerySimpleKey";

    private static PlayerData data = new PlayerData();
    private static bool isLoaded = false;

    // ✅ Sticky readiness flag (late subscribers can check)
    public static bool IsReady { get; private set; }

    // Events
    public static event Action OnPrefsSaved;
    public static event Action OnPlayerDataLoaded;

    private const string COLLECTION_PREFIX = "logs:";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // IMPORTANT: Don't fire loaded immediately in Awake,
        // because listeners often subscribe in OnEnable/Start.
        EnsureLoaded();
        StartCoroutine(FireLoadedNextFrame());
        StartCoroutine(NotifyGameMasterWhenReady());
    }

    // ✅ Use this from other scripts instead of raw event subscription.
    // It guarantees the callback runs even if you subscribe late.
    public static void WhenLoaded(Action callback)
    {
        if (callback == null) return;

        if (IsReady)
        {
            callback.Invoke();
            return;
        }

        void Handler()
        {
            OnPlayerDataLoaded -= Handler;
            callback.Invoke();
        }

        OnPlayerDataLoaded += Handler;
    }

    private void EnsureLoaded()
    {
        if (isLoaded) return;
        if (Instance == null) return;

        Load();
        isLoaded = true;
        IsReady = true;
    }

    private System.Collections.IEnumerator FireLoadedNextFrame()
    {
        // wait a frame so OnEnable subscriptions can register
        yield return null;

        OnPlayerDataLoaded?.Invoke();
    }

    private System.Collections.IEnumerator NotifyGameMasterWhenReady()
    {
        // wait until GameMaster exists (don’t assume it’s ready in Awake)
        while (GameMaster.Instance == null || GameMaster.Instance.EventManager == null)
            yield return null;

        GameMaster.Instance.EventManager.PlayerDataLoaded();
    }

    // ===================== GET/SET =====================

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
        return data.PlayerDatum.TryGetValue(key, out var v) && int.TryParse(v, out var r) ? r : defaultValue;
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
            case CollectionType.dictionary:
                if (!(collection is System.Collections.IDictionary)) throw new ArgumentException("Expected Dictionary");
                break;
            case CollectionType.list:
                if (!(collection is System.Collections.IList)) throw new ArgumentException("Expected List");
                break;
            case CollectionType.array:
                if (!collection.GetType().IsArray) throw new ArgumentException("Expected Array");
                break;
        }

        data.PlayerDatum[COLLECTION_PREFIX + key] = JsonConvert.SerializeObject(collection, Formatting.None);
    }

    public T GetCollection<T>(string key) where T : new()
    {
        EnsureLoaded();

        if (!data.PlayerDatum.TryGetValue(COLLECTION_PREFIX + key, out var json) || string.IsNullOrEmpty(json))
            return new T();

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
            if (Instance != null && Instance.useEncryption)
                json = Decrypt(json);

            data = JsonConvert.DeserializeObject<PlayerData>(json) ?? new PlayerData();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"StoredPrefs.Load failed, creating new data. {e}");
            data = new PlayerData();
        }
    }

    public void ResetAll()
    {
        data = new PlayerData();
        isLoaded = false;
        IsReady = false;

        if (File.Exists(FilePath))
            File.Delete(FilePath);

        EnsureLoaded();
        StartCoroutine(FireLoadedNextFrame());
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
