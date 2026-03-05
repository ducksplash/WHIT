using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class StoredPrefs : MonoBehaviour
{
    public static StoredPrefs Instance;

    [Header("Storage")]
    public bool useEncryption = true;

    public static string FilePath => Path.Combine(Application.persistentDataPath, "PlayerData.json");
    public const string EncryptionKey = "SomeVerySimpleKey";

    private static PlayerData data = new PlayerData();

    // Load state
    private static bool isLoaded = false;

    // ✅ Sticky readiness flag (late subscribers can check)
    public static bool IsReady { get; private set; }

    // Events
    public static event Action OnPrefsSaved;
    public static event Action OnPlayerDataLoaded;

    private const string COLLECTION_PREFIX = "logs:";

    // Async coordination
    private static readonly SemaphoreSlim _loadLock = new SemaphoreSlim(1, 1);
    private static readonly SemaphoreSlim _saveLock = new SemaphoreSlim(1, 1);

    // ✅ continuations async (safer)
    private static TaskCompletionSource<bool> _readyTcs =
        new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

    // ------------------------------------------------------------
    // ✅ AUTO-CREATE IF MISSING
    // ------------------------------------------------------------
    private static void EnsureInstanceExists()
    {
        if (Instance != null) return;

        // Try find in scene first (including inactive)
        Instance = FindFirstObjectByType<StoredPrefs>(FindObjectsInactive.Include);
        if (Instance != null) return;

        // Create if not present
        var go = new GameObject("[StoredPrefs]");
        Instance = go.AddComponent<StoredPrefs>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Kick off async load (non-blocking).
        _ = EnsureLoadedAsync();

        // Fire events only after load is complete (and next frame so others can subscribe).
        StartCoroutine(FireLoadedNextFrameWhenReady());
        StartCoroutine(NotifyGameMasterWhenReady());
    }

    // ✅ Use this from other scripts instead of raw event subscription.
    // It guarantees the callback runs even if you subscribe late.
    public static void WhenLoaded(Action callback)
    {
        if (callback == null) return;

        // ✅ guarantee StoredPrefs exists
        EnsureInstanceExists();

        // ✅ kick load
        _ = Instance.EnsureLoadedAsync();

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

    // Optional: awaitable form
    public static Task WhenLoadedAsync()
    {
        EnsureInstanceExists();
        _ = Instance.EnsureLoadedAsync();

        if (IsReady) return Task.CompletedTask;
        return _readyTcs.Task;
    }

    private void MarkReady()
    {
        IsReady = true;

        if (!_readyTcs.Task.IsCompleted)
            _readyTcs.TrySetResult(true);
    }

    private async Task EnsureLoadedAsync()
    {
        // If already loaded, still ensure readiness is marked
        if (isLoaded)
        {
            if (!IsReady) MarkReady();
            return;
        }

        await _loadLock.WaitAsync();
        try
        {
            if (isLoaded)
            {
                if (!IsReady) MarkReady();
                return;
            }

            await LoadAsync();

            isLoaded = true;
            MarkReady();
        }
        finally
        {
            _loadLock.Release();
        }
    }

    private System.Collections.IEnumerator FireLoadedNextFrameWhenReady()
    {
        yield return new WaitUntil(() => IsReady);
        yield return null; // allow OnEnable subscriptions
        OnPlayerDataLoaded?.Invoke();
    }

    private System.Collections.IEnumerator NotifyGameMasterWhenReady()
    {
        yield return new WaitUntil(() => IsReady);

        while (GameMaster.Instance == null)
            yield return null;

        EventManager.PlayerDataLoaded();
    }

    // ===================== GET/SET =====================

    public void SetString(string key, string value)
    {
        _ = EnsureLoadedAsync();
        data.PlayerDatum[key] = value;
    }

    public string GetString(string key, string defaultValue = "")
    {
        return data.PlayerDatum.TryGetValue(key, out var v) ? v : defaultValue;
    }

    public void SetInt(string key, int value)
    {
        _ = EnsureLoadedAsync();
        data.PlayerDatum[key] = value.ToString();
    }

    public int GetInt(string key, int defaultValue = 0)
    {
        return data.PlayerDatum.TryGetValue(key, out var v) && int.TryParse(v, out var r) ? r : defaultValue;
    }

    public void SetFloat(string key, float value)
    {
        _ = EnsureLoadedAsync();
        data.PlayerDatum[key] = value.ToString(CultureInfo.InvariantCulture);
    }

    public float GetFloat(string key, float defaultValue = 0f)
    {
        return data.PlayerDatum.TryGetValue(key, out var v) &&
               float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var r)
            ? r
            : defaultValue;
    }

    public void DeleteKey(string key)
    {
        _ = EnsureLoadedAsync();
        if (data.PlayerDatum.Remove(key))
            _ = SaveAsync();
    }

    public List<string> GetAllKeys()
    {
        return new List<string>(data.PlayerDatum.Keys);
    }

    // ===================== COLLECTION API =====================

    public void SetCollection<T>(string key, T collection, CollectionType type)
    {
        _ = EnsureLoadedAsync();

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

    // ===================== SAVE / LOAD (ASYNC) =====================

    public void Save()
    {
        _ = SaveAsync();
    }

    public async Task SaveAsync()
    {
        await EnsureLoadedAsync();

        await _saveLock.WaitAsync();
        try
        {
            PlayerData snapshot = new PlayerData();
            snapshot.PlayerDatum = new Dictionary<string, string>(data.PlayerDatum);

            string payload = await Task.Run(() =>
            {
                string json = JsonConvert.SerializeObject(snapshot, Formatting.None);
                if (Instance != null && Instance.useEncryption) json = Encrypt(json);
                return json;
            });

            string dir = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            await File.WriteAllTextAsync(FilePath, payload);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"StoredPrefs.SaveAsync failed: {e}");
        }
        finally
        {
            _saveLock.Release();
        }

        OnPrefsSaved?.Invoke();
    }

    private async Task LoadAsync()
    {
        // ✅ Missing file is still a "successful load"
        if (!File.Exists(FilePath))
        {
            data = new PlayerData();
            return;
        }

        try
        {
            string json = await File.ReadAllTextAsync(FilePath).ConfigureAwait(false);

            if (Instance != null && Instance.useEncryption)
                json = Decrypt(json);

            data = JsonConvert.DeserializeObject<PlayerData>(json) ?? new PlayerData();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"StoredPrefs.LoadAsync failed, creating new data. {e}");
            data = new PlayerData();
        }
    }

    public void ResetAll()
    {
        _ = ResetAllAsync();
    }

    public async Task ResetAllAsync()
    {
        await _loadLock.WaitAsync();
        await _saveLock.WaitAsync();
        try
        {
            data = new PlayerData();
            isLoaded = false;
            IsReady = false;

            _readyTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            if (File.Exists(FilePath))
                File.Delete(FilePath);

            await EnsureLoadedAsync();
            await SaveAsync();
        }
        finally
        {
            _saveLock.Release();
            _loadLock.Release();
        }

        if (Instance != null)
        {
            Instance.StopCoroutine(nameof(FireLoadedNextFrameWhenReady));
            Instance.StartCoroutine(Instance.FireLoadedNextFrameWhenReady());
        }
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