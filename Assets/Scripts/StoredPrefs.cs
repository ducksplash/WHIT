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
    private static readonly TaskCompletionSource<bool> _readyTcs = new TaskCompletionSource<bool>();

    private void Awake()
    {
        if (Instance != null)
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

    // Optional: awaitable form (handy in async code)
    public static Task WhenLoadedAsync()
    {
        if (IsReady) return Task.CompletedTask;
        return _readyTcs.Task;
    }

    private async Task EnsureLoadedAsync()
    {
        if (isLoaded) return;
        if (Instance == null) return;

        await _loadLock.WaitAsync();
        try
        {
            if (isLoaded) return;

            await LoadAsync();

            isLoaded = true;
            IsReady = true;

            // unblock awaiters
            if (!_readyTcs.Task.IsCompleted)
                _readyTcs.TrySetResult(true);
        }
        finally
        {
            _loadLock.Release();
        }
    }

    private System.Collections.IEnumerator FireLoadedNextFrameWhenReady()
    {
        // wait until async load completes
        yield return new WaitUntil(() => IsReady);

        // wait a frame so OnEnable subscriptions can register
        yield return null;

        OnPlayerDataLoaded?.Invoke();
    }

    private System.Collections.IEnumerator NotifyGameMasterWhenReady()
    {
        // wait until our prefs are ready first
        yield return new WaitUntil(() => IsReady);

        // wait until GameMaster exists (don’t assume it’s ready in Awake)
        while (GameMaster.Instance == null || GameMaster.Instance.EventManager == null)
            yield return null;

        GameMaster.Instance.EventManager.PlayerDataLoaded();
    }

    // ===================== GET/SET =====================

    public void SetString(string key, string value)
    {
        _ = EnsureLoadedAsync(); // non-blocking safety
        data.PlayerDatum[key] = value;
    }

    public string GetString(string key, string defaultValue = "")
    {
        // If called early, it will still return defaults until ready.
        // Prefer WhenLoaded/WhenLoadedAsync for strict ordering.
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
            _ = SaveAsync(); // fire-and-forget async save
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

    /// <summary>
    /// Fire-and-forget convenience wrapper (keeps your old call sites working).
    /// Prefer awaiting SaveAsync() when ordering matters.
    /// </summary>
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
            // 1) snapshot on main thread quickly
            PlayerData snapshot = new PlayerData();
            snapshot.PlayerDatum = new Dictionary<string, string>(data.PlayerDatum);

            // 2) heavy work off-thread
            string payload = await Task.Run(() =>
            {
                string json = JsonConvert.SerializeObject(snapshot, Formatting.None);
                if (Instance != null && Instance.useEncryption) json = Encrypt(json);
                return json;
            });

            string dir = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            // 3) async disk write
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
        // ✅ Even if file missing, we still consider load complete and fire loaded event later
        if (!File.Exists(FilePath))
        {
            data = new PlayerData();
            return;
        }

        try
        {
            // ✅ Async read
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
        // Stop anyone else trying to save/load while we reset
        await _loadLock.WaitAsync();
        await _saveLock.WaitAsync();
        try
        {
            data = new PlayerData();
            isLoaded = false;
            IsReady = false;

            // delete file (sync; tiny)
            if (File.Exists(FilePath))
                File.Delete(FilePath);

            // reload to re-arm readiness
            await EnsureLoadedAsync();

            // save empty data
            await SaveAsync();
        }
        finally
        {
            _saveLock.Release();
            _loadLock.Release();
        }

        // Fire loaded event again next frame (so listeners can refresh)
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
