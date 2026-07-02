using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
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

    public static bool IsReady { get; private set; }

    public static event Action OnPrefsSaved;
    public static event Action OnPlayerDataLoaded;

    private const string COLLECTION_PREFIX = "logs:";

    private static readonly SemaphoreSlim _loadLock = new SemaphoreSlim(1, 1);
    private static readonly SemaphoreSlim _saveLock = new SemaphoreSlim(1, 1);

    private static UniTaskCompletionSource<bool> _readyTcs = new UniTaskCompletionSource<bool>();

    private CancellationTokenSource _lifetimeCts = new CancellationTokenSource();

    private static void EnsureInstanceExists()
    {
        if (Instance != null) return;

        Instance = FindFirstObjectByType<StoredPrefs>(FindObjectsInactive.Include);
        if (Instance != null) return;

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

        EnsureLoadedAsync().Forget();

        FireLoadedNextFrameWhenReady().Forget();
        NotifyGameMasterWhenReady().Forget();
    }

    public static void WhenLoaded(Action callback)
    {
        if (callback == null) return;

        EnsureInstanceExists();
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

    public static UniTask WhenLoadedAsync()
    {
        EnsureInstanceExists();
        _ = Instance.EnsureLoadedAsync();
        return IsReady ? UniTask.CompletedTask : _readyTcs.Task;
    }

    private void MarkReady()
    {
        IsReady = true;
        if (_readyTcs.Task.Status == UniTaskStatus.Pending)
            _readyTcs.TrySetResult(true);
    }

    private async UniTask EnsureLoadedAsync()
    {
        if (isLoaded)
        {
            if (!IsReady) MarkReady();
            return;
        }

        await _loadLock.WaitAsync(_lifetimeCts.Token);

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

    private async UniTask FireLoadedNextFrameWhenReady()
    {
        await UniTask.WaitUntil(() => IsReady);
        await UniTask.Yield(PlayerLoopTiming.Update);
        OnPlayerDataLoaded?.Invoke();
    }

    private async UniTask NotifyGameMasterWhenReady()
    {
        await UniTask.WaitUntil(() => IsReady);
        await UniTask.WaitUntil(() => GameMaster.Instance != null);
        EventManager.PlayerDataLoaded();
    }

    // ===================== GET / SET =====================
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
               ? r : defaultValue;
    }

    public void DeleteKey(string key)
    {
        _ = EnsureLoadedAsync();
        if (data.PlayerDatum.Remove(key))
            _ = SaveAsync();
    }

    public List<string> GetAllKeys() => new List<string>(data.PlayerDatum.Keys);

    // ===================== COLLECTION API =====================
    public void SetCollection<T>(string key, T collection, CollectionType type)
    {
        _ = EnsureLoadedAsync();
        if (collection == null) throw new ArgumentNullException(nameof(collection));

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

    // ===================== SAVE / LOAD =====================
    public void Save() => SaveAsync().Forget();

    public async UniTask SaveAsync()
    {
        await EnsureLoadedAsync();
        await _saveLock.WaitAsync(_lifetimeCts.Token);

        try
        {
            PlayerData snapshot = new PlayerData { PlayerDatum = new Dictionary<string, string>(data.PlayerDatum) };

            string payload = await UniTask.RunOnThreadPool(() =>
            {
                string json = JsonConvert.SerializeObject(snapshot, Formatting.None);
                if (useEncryption) json = Encrypt(json);
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

    private async UniTask LoadAsync()
    {
        if (!File.Exists(FilePath))
        {
            data = new PlayerData();
            return;
        }

        try
        {
            string json = await File.ReadAllTextAsync(FilePath);

            if (useEncryption)
                json = Decrypt(json);

            data = JsonConvert.DeserializeObject<PlayerData>(json) ?? new PlayerData();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"StoredPrefs.LoadAsync failed, creating new data. {e}");
            data = new PlayerData();
        }
    }

    public void ResetAll() => ResetAllAsync().Forget();

    public async UniTask ResetAllAsync()
    {
        await _loadLock.WaitAsync(_lifetimeCts.Token);
        await _saveLock.WaitAsync(_lifetimeCts.Token);

        try
        {
            data = new PlayerData();
            isLoaded = false;
            IsReady = false;
            _readyTcs = new UniTaskCompletionSource<bool>();

            if (File.Exists(FilePath))
                File.Delete(FilePath);

            await EnsureLoadedAsync();
            await SaveAsync();
        }
        catch (Exception e)
        {
            Debug.Log("reset failed, dumping semaphores anyway.\n"+e);
            _saveLock.Release();
            _loadLock.Release();
        }
        finally
        {
            _saveLock.Release();
            _loadLock.Release();
        }
        
    }

    private static string Encrypt(string text)
    {
        char[] buffer = text.ToCharArray();
        
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (char)(buffer[i] ^ EncryptionKey[i % EncryptionKey.Length]);
        }

        return new string(buffer);
    }

    private static string Decrypt(string text) => Encrypt(text);

    private void OnDestroy()
    {
        _lifetimeCts?.Cancel();
        _lifetimeCts?.Dispose();
    }
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