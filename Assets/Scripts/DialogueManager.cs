using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random; // ← Added

public class DialogueManager : MonoBehaviour
{
    // ─── UI References ─────────────────────────────────────────────────────
    [Header("SMS / Phone")]
    public CanvasGroup PhoneMessageCanvas;
    public TextMeshProUGUI ContactName;
    public Image ContactPhoto;
    public TextMeshProUGUI ReceivedMessage;

    [Header("Onscreen Dialogue")]
    public CanvasGroup OnscreenDialogueCanvas;
    public CanvasGroup DialogueSpeakerNameCanvas;
    public TextMeshProUGUI DialogueSpeakerName;
    public TextMeshProUGUI SpeakerText;

    [Header("NoraThoughts")]
    public List<TextMeshProUGUI> ThoughtTexts = new List<TextMeshProUGUI>();
    public CanvasGroup ThoughtCanvas;



    [Header("Typewriters")]
    public TMPTypewriter SystemMessageWriter;
    public TMPTypewriter NoraMessageWriter;
    public TMPTypewriter ReceivedMessageWriter;

    [Header("Timer Bar")]
    public Image timebar;
    public float messagetimer;

    // ─── Dialogue Data ─────────────────────────────────────────────────────
    [Header("Dialogue Data")]
    public List<Dialogue> Dialogues = new();
    public List<OSDText> OSDTexts = new();
    public List<NoraThought> NoraThoughts = new();
    public List<DialogueName> RepeatableDialogues = new();
    public List<DialogueName> DialogueSeen = new();
    public List<ThoughtName> ThoughtsSeen = new();

    // ─── Cutscene ──────────────────────────────────────────────────────────
    [Header("Cutscene")]
    public Camera mainCamera;
    public Zoom cameraZoom;
    public float originalFieldOfView = 70f;
    public float targetFieldOfView = 40f;
    public float panTime = 5f;
    public float duration = 10f;

    // ─── Input ─────────────────────────────────────────────────────────────
    public InputActionReference advanceDialogue;

    // ─── State ─────────────────────────────────────────────────────────────
    public bool DialogInProgress { get; private set; }
    public bool CutsceneInProgress { get; private set; }
    public bool SeenLoaded { get; private set; }

    // ─── Private state ─────────────────────────────────────────────────────
    readonly Dictionary<DialogueName, Dialogue> _dialogueDict = new();
    readonly Dictionary<OSDTextName, OSDText> _osdTextDict = new();
    readonly Dictionary<ThoughtName, NoraThought> _noraThoughtDict = new();
    readonly Dictionary<string, string> _eregiDict = new();

    // Timed-dialogue cancellation
    CancellationTokenSource _timedCts;
    bool _hasActiveTimed;
    DialogueName _activeTimedName;
    bool _activeTimedIsRepeatable;
    bool _activeTimedWasShown;

    // Cutscene internals
    float _elapsedCutsceneTime;
    bool _stopCutsceneRotation;

    // Advance-key state
    bool _advanceRequested;

    // Removed old Coroutine fields (_fadeCo, _activeThoughtCo)

    // ───────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ───────────────────────────────────────────────────────────────────────
    void Start()
    {
        DialogInProgress = false;
        ClearAllUI();
        BuildDialogueDictionary();
        BuildOSDDictionary();
        BuildEregiDictionary();
        BuildNoraThoughtDictionary();

        if (mainCamera != null) originalFieldOfView = mainCamera.fieldOfView;
        if (UInstance.Instance != null) UInstance.Instance.cutsceneBarsCanvas.alpha = 0;
        if (PhoneMessageCanvas != null) PhoneMessageCanvas.alpha = 0f;
        if (OnscreenDialogueCanvas != null) OnscreenDialogueCanvas.alpha = 0f;

        _ = InitSeenAsync();
        CleanThoughts();
    }

    void LateUpdate()
    {
        if (!DialogInProgress || messagetimer <= 0) return;
        timebar.fillAmount -= (1f / messagetimer) * Time.deltaTime;
    }

    void OnDisable() => CancelActiveTimed(markSeen: true);

    // ───────────────────────────────────────────────────────────────────────
    // Thoughts (fully converted)
    // ───────────────────────────────────────────────────────────────────────
    public async void PlayThought(ThoughtName thoughtName)
    {
        while (!SeenLoaded) await Task.Yield();

        if (ThoughtsSeen.Contains(thoughtName))
        {
            Debug.Log("Thought already seen: " + thoughtName);
            return;
        }

        if (!_noraThoughtDict.TryGetValue(thoughtName, out var data))
        {
            Debug.LogWarning($"DialogueManager: No thought found for '{thoughtName}'.");
            return;
        }

        if (ThoughtTexts == null || ThoughtTexts.Count == 0)
        {
            Debug.LogWarning("DialogueManager: ThoughtTexts list is empty.");
            return;
        }

        List<string> lines = data.EregiReplace ? data.NoraThoughtString.ConvertAll(GetReplacedString) : new List<string>(data.NoraThoughtString);

        if (lines.Count == 0) return;

        CleanThoughts();

        await ThoughtSequence(lines, thoughtName, data.thoughtFinalFadeDuration, data.thoughtFadeDuration, data.thoughtFadeInDuration, data.thoughtStaggerDelay);
    }

    async UniTask ThoughtSequence(List<string> lines, ThoughtName thoughtName, float thoughtFinalFadeDuration, float thoughtFadeDuration, float thoughtFadeInDuration, float thoughtStaggerDelay)
    {
        ThoughtCanvas.alpha = 1f;

        var available = new List<int>();

        int PickNextIndex()
        {
            if (available.Count == 0)
            {
                for (int i = 0; i < ThoughtTexts.Count; i++) available.Add(i);
                for (int i = available.Count - 1; i > 0; i--)
                {
                    int j = Random.Range(0, i + 1);
                    (available[i], available[j]) = (available[j], available[i]);
                }
            }
            int pick = available[0];
            available.RemoveAt(0);
            return pick;
        }

        for (int i = 0; i < lines.Count; i++)
        {
            bool isFinal = i == lines.Count - 1;
            float fadeOut = isFinal ? thoughtFinalFadeDuration : thoughtFadeDuration;
            int boxIndex = PickNextIndex();

            _ = ShowThought(ThoughtTexts[boxIndex], lines[i], thoughtFadeInDuration, fadeOut);

            if (!isFinal)
                await UniTask.Delay(TimeSpan.FromSeconds(thoughtStaggerDelay), ignoreTimeScale: false);
            else
                await UniTask.Delay(TimeSpan.FromSeconds(thoughtFadeInDuration + fadeOut), ignoreTimeScale: false);
        }

        ThoughtCanvas.alpha = 0f;
        MarkThoughtSeen(thoughtName);
    }

    async UniTask ShowThought(TextMeshProUGUI box, string text, float fadeInTime, float fadeOutTime)
    {
        box.text = text;
        box.color = Color.clear;

        await LerpColor(box, Color.clear, Color.white, fadeInTime);
        box.color = Color.white;

        await LerpColor(box, Color.white, Color.clear, fadeOutTime);

        box.color = Color.clear;
        box.text = "";
    }

    async UniTask LerpColor(TextMeshProUGUI text, Color from, Color to, float duration)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(duration, 0.0001f);
            text.color = Color.Lerp(from, to, Mathf.Clamp01(t));
            await UniTask.Yield(PlayerLoopTiming.Update);
        }
        text.color = to;
    }

    
    /// <summary>
    /// Legacy entry point used by Evidence, DialogueBeef, OnboardingManager, 
    /// Player, Phone etc. Routes into the new queue/play pipeline.
    /// </summary>
    public Task NewDialogue(DialogueName name, float displayTimer, bool holdUntilAdvance = false, DialogueType type = DialogueType.normal)
    {
        return PlayDialogue(name, displayTimer, type, displayTimer, displayTimer, null, false, holdUntilAdvance);
    }
    
    // ───────────────────────────────────────────────────────────────────────
    // Public entry points
    // ───────────────────────────────────────────────────────────────────────
    public Task PlayDialogue(DialogueName name, float displayTimer, DialogueType type,
        float cutsceneDuration = -1f, float cutscenePanTime = -1f,
        GameObject cutsceneTarget = null, bool isZoomable = true, bool holdUntilAdvance = false)
    {
        switch (type)
        {
            case DialogueType.cutscene:
                if (cutsceneTarget == null)
                {
                    Debug.LogError("DialogueManager: Cutscene requested but cutsceneTarget is null.");
                    return Task.CompletedTask;
                }
                float useDuration = cutsceneDuration > 0 ? cutsceneDuration : duration;
                float usePanTime = cutscenePanTime > 0 ? cutscenePanTime : panTime;
                return StartCutscene(name, displayTimer, cutsceneTarget, useDuration, usePanTime, isZoomable, holdUntilAdvance);

            default:
                return EnqueueOrPlay(name, displayTimer, type, holdUntilAdvance);
        }
    }

    // ───────────────────────────────────────────────────────────────────────
    // Queueing
    // ───────────────────────────────────────────────────────────────────────
    Task EnqueueOrPlay(DialogueName name, float timer, DialogueType type, bool hold)
    {
        if (!DialogInProgress)
            return RunDialogue(name, timer, type, hold);

        var tcs = new TaskCompletionSource<bool>();
        _ = QueueCoroutine(name, timer, type, hold, tcs);
        return tcs.Task;
    }

    async UniTask QueueCoroutine(DialogueName name, float timer, DialogueType type, bool hold, TaskCompletionSource<bool> tcs)
    {
        await UniTask.WaitWhile(() => DialogInProgress);
        _ = RunDialogue(name, timer, type, hold);
        tcs.SetResult(true);
    }

    // ───────────────────────────────────────────────────────────────────────
    // Core dialogue runner
    // ───────────────────────────────────────────────────────────────────────
    async Task RunDialogue(DialogueName name, float displayTimer, DialogueType type, bool holdUntilAdvance)
    {
        if (DialogueSeen.Contains(name) && !RepeatableDialogues.Contains(name))
        {
            if (holdUntilAdvance) ClearHeldDialogue();
            return;
        }

        if (!_dialogueDict.TryGetValue(name, out var data))
        {
            Debug.LogWarning($"DialogueManager: No dialogue found for '{name}'.");
            return;
        }

        string message = data.EregiReplace ? GetReplacedString(data.DialogueText) : data.DialogueText;
        DialogueType resolvedType = data.DialogueType;

        if (holdUntilAdvance)
        {
            CancelActiveTimed(markSeen: true);
            messagetimer = 0f;
            DialogInProgress = true;
            ShowOnscreenDialogue(data.Contact);
            await TypeOnscreen(data.Contact, message, CancellationToken.None);
            MarkSeen(name);
            EventManager.DialogueCanProceed(true);
            await WaitForPlayerAdvanceAsync();
            ClearHeldDialogue();
            return;
        }

        CancelActiveTimed(markSeen: true);
        _timedCts = new CancellationTokenSource();
        _hasActiveTimed = true;
        _activeTimedName = name;
        _activeTimedIsRepeatable = RepeatableDialogues.Contains(name);
        _activeTimedWasShown = false;

        messagetimer = displayTimer;
        DialogInProgress = true;
        var ct = _timedCts.Token;

        try
        {
            switch (resolvedType)
            {
                case DialogueType.SMS:
                    await RunSMS(data.Contact, message, displayTimer, ct);
                    break;
                default:
                    await RunNormal(data.Contact, message, displayTimer, ct);
                    break;
            }
        }
        catch (TaskCanceledException) { return; }
        finally
        {
            if (_hasActiveTimed && EqualityComparer<DialogueName>.Default.Equals(_activeTimedName, name))
            {
                _hasActiveTimed = false;
                _timedCts?.Dispose();
                _timedCts = null;
            }
        }

        MarkSeen(name);
    }

    // ───────────────────────────────────────────────────────────────────────
    // Display helpers
    // ───────────────────────────────────────────────────────────────────────
    async Task RunNormal(Contacts contact, string message, float displayTimer, CancellationToken ct)
    {
        ShowOnscreenDialogue(contact);
        _activeTimedWasShown = true;
        await TypeOnscreen(contact, message, ct);
        await DialogueTimer(displayTimer, ct);
    }

    void ShowOnscreenDialogue(Contacts contact)
    {
        bool hasSpeakerName = contact != Contacts.System && contact != Contacts.Unknown;
        DialogueSpeakerNameCanvas.alpha = hasSpeakerName ? 1f : 0f;
        if (hasSpeakerName) DialogueSpeakerName.text = contact.ToString();
        OnscreenDialogueCanvas.alpha = 1f;
    }

    Task TypeOnscreen(Contacts contact, string message, CancellationToken ct)
    {
        var writer = contact == Contacts.Nora ? NoraMessageWriter : SystemMessageWriter;
        return PlayWriterOrFallback(writer, SpeakerText, message, ct);
    }

    async Task RunSMS(Contacts contact, string message, float displayTimer, CancellationToken ct)
    {
        Debug.Log("[SMS] START");
        DialogInProgress = true;

        OnscreenDialogueCanvas.alpha = 0f;
        DialogueSpeakerNameCanvas.alpha = 0f;

        PhoneMessageCanvas.alpha = 1f;
        ContactName.text = contact.ToString();
        ReceivedMessage.text = message;

        try
        {
            await Task.Delay(Mathf.RoundToInt(displayTimer * 1000), ct);
        }
        catch (TaskCanceledException) { return; }

        PhoneMessageCanvas.alpha = 0f;
        ContactName.text = "";
        ReceivedMessage.text = "";
        DialogInProgress = false;
        Debug.Log("[SMS] END");
    }

    async Task DialogueTimer(float duration, CancellationToken ct)
    {
        await Task.Delay((int)(duration * 1000), ct);

        if (SystemMessageWriter != null) SystemMessageWriter.Clear();
        else if (SpeakerText != null) SpeakerText.text = "";

        await Task.Delay(500, ct);
        DialogInProgress = false;
    }

    // ───────────────────────────────────────────────────────────────────────
    // Cutscene (fully converted)
    // ───────────────────────────────────────────────────────────────────────
    async Task StartCutscene(DialogueName name, float dialogueTimer, GameObject target,
        float cutsceneDuration, float cutscenePanTime, bool isZoomable, bool hold)
    {
        if (CutsceneInProgress) return;

        var tcs = new TaskCompletionSource<bool>();
        await CutsceneCoroutine(name, dialogueTimer, target, cutsceneDuration, cutscenePanTime, tcs, isZoomable, hold);
        tcs.SetResult(true); // not strictly needed anymore but kept for compatibility
    }

    async UniTask CutsceneCoroutine(DialogueName name, float dialogueTimer, GameObject target,
        float cutsceneDuration, float cutscenePanTime, TaskCompletionSource<bool> tcs,
        bool isZoomable, bool hold)
    {
        if (CutsceneInProgress) return;

        GameMaster.Instance.PLAYERBUSY = true;
        CutsceneInProgress = true;
        _elapsedCutsceneTime = 0f;
        _stopCutsceneRotation = false;

        await UInstance.Instance.FadeInCutsceneBars(cutscenePanTime);
        await UniTask.Delay(1000);

        var dialogueTask = RunDialogue(name, dialogueTimer, DialogueType.normal, hold);
        await dialogueTask;

        float zoomTime = cutsceneDuration * 0.33f;
        float unzoomTime = cutsceneDuration * 0.33f;
        float holdTime = cutsceneDuration - zoomTime - unzoomTime;

        if (cameraZoom != null) cameraZoom.enabled = false;

        if (isZoomable)
            await CutsceneZoomSequence(zoomTime, holdTime, unzoomTime, hold);

        while (_elapsedCutsceneTime < cutsceneDuration && !_stopCutsceneRotation)
        {
            Vector3 dir = target.transform.position - mainCamera.transform.position;
            Quaternion rotation = Quaternion.LookRotation(dir);
            mainCamera.transform.rotation = Quaternion.Lerp(
                mainCamera.transform.rotation,
                rotation,
                cutscenePanTime * Time.smoothDeltaTime);

            _elapsedCutsceneTime += Time.smoothDeltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        Vector3 finalDir = (target.transform.position - mainCamera.transform.position).normalized;
        float yaw = Mathf.Atan2(finalDir.x, finalDir.z) * Mathf.Rad2Deg;
        float pitch = Mathf.Asin(finalDir.y) * Mathf.Rad2Deg;

        Player.Instance.FirstPersonLook.SetPlayerRotation(new Vector2(yaw, pitch));

        CutsceneInProgress = false;
        GameMaster.Instance.PLAYERBUSY = false;
        SaveWhatYouSee();
    }

    async UniTask CutsceneZoomSequence(float zoomTime, float holdTime, float unzoomTime, bool hold)
    {
        await LerpFOV(originalFieldOfView, targetFieldOfView, zoomTime);

        if (hold)
            await UniTask.WaitUntil(() => _advanceRequested);
        else
            await UniTask.Delay(TimeSpan.FromSeconds(holdTime), ignoreTimeScale: false);

        _stopCutsceneRotation = true;
        ClearHeldDialogue();

        await LerpFOV(targetFieldOfView, originalFieldOfView, unzoomTime);

        await UInstance.Instance.FadeOutCutsceneBars();

        if (cameraZoom != null)
        {
            cameraZoom.enabled = true;
            cameraZoom.AttachListeners();
        }
    }

    async UniTask LerpFOV(float from, float to, float time)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(time, 0.0001f);
            mainCamera.fieldOfView = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            await UniTask.Yield(PlayerLoopTiming.Update);
        }
        mainCamera.fieldOfView = to;
    }

    // ───────────────────────────────────────────────────────────────────────
    // Advance input
    // ───────────────────────────────────────────────────────────────────────
    void RequestAdvance(InputAction.CallbackContext ctx)
    {
        _advanceRequested = true;
        EventManager.DialogueCanProceed(false);
    }

    public async Task WaitForPlayerAdvanceAsync()
    {
        _advanceRequested = false;

        if (advanceDialogue == null)
        {
            Debug.LogWarning("DialogueManager: advanceDialogue not assigned.");
            return;
        }

        advanceDialogue.action.performed -= RequestAdvance;
        advanceDialogue.action.performed += RequestAdvance;

        await UniTask.WaitUntil(() => _advanceRequested);

        advanceDialogue.action.performed -= RequestAdvance;
    }

    // ───────────────────────────────────────────────────────────────────────
    // Typewriter helper
    // ───────────────────────────────────────────────────────────────────────
    Task PlayWriterOrFallback(TMPTypewriter writer, TextMeshProUGUI fallback, string message, CancellationToken ct)
    {
        if (writer != null)
            return writer.PlayText(message, ct);

        fallback.text = message;
        return Task.CompletedTask;
    }

    // ───────────────────────────────────────────────────────────────────────
    // Seen / Repeatable tracking, UI Clear, etc. (unchanged except minor cleanups)
    // ───────────────────────────────────────────────────────────────────────
    void MarkSeen(DialogueName dialogueName)
    {
        if (!DialogueSeen.Contains(dialogueName)) DialogueSeen.Add(dialogueName);
        SaveWhatYouSee();
    }

    void MarkThoughtSeen(ThoughtName thoughtName)
    {
        if (!ThoughtsSeen.Contains(thoughtName)) ThoughtsSeen.Add(thoughtName);
        SaveWhatYouThought();
    }

    void CancelActiveTimed(bool markSeen)
    {
        if (!_hasActiveTimed) return;

        try { _timedCts?.Cancel(); } catch { }
        if (markSeen && _activeTimedWasShown && !_activeTimedIsRepeatable)
        {
            if (!DialogueSeen.Contains(_activeTimedName))
                DialogueSeen.Add(_activeTimedName);
            SaveWhatYouSee();
        }

        _hasActiveTimed = false;
        try { _timedCts?.Dispose(); } catch { }
        _timedCts = null;
    }

    void ClearHeldDialogue()
    {
        messagetimer = 0f;
        DialogInProgress = false;
        ClearAllUI();
        if (PhoneMessageCanvas != null) PhoneMessageCanvas.alpha = 0f;
    }

    void ClearAllUI()
    {
        SystemMessageWriter?.Clear();
        NoraMessageWriter?.Clear();
        ReceivedMessageWriter?.Clear();
        if (SpeakerText != null) SpeakerText.text = "";
        if (ReceivedMessage != null) ReceivedMessage.text = "";
        if (ContactName != null) ContactName.text = "";
        if (PhoneMessageCanvas != null) PhoneMessageCanvas.alpha = 0f;
        if (OnscreenDialogueCanvas != null) OnscreenDialogueCanvas.alpha = 0f;
        if (DialogueSpeakerNameCanvas != null) DialogueSpeakerNameCanvas.alpha = 0f;
    }

    void CleanThoughts()
    {
        ThoughtCanvas.alpha = 0;
        foreach (var textBox in ThoughtTexts)
        {
            textBox.color = Color.clear;
            textBox.text = "";
        }
    }
    // ───────────────────────────────────────────────────────────────────────
    // OSD
    // ───────────────────────────────────────────────────────────────────────

    public string RetrieveOSDText(OSDTextName name)
    {
        if (!_osdTextDict.TryGetValue(name, out var entry)) return ".";

        string msg = entry.OSDTextString;
        if (entry.EregiReplace) msg = GetReplacedString(msg);
        return msg;
    }

    // ───────────────────────────────────────────────────────────────────────
    // NoraThoughtses
    // ───────────────────────────────────────────────────────────────────────

    public List<string> RetrieveNoraThoughtText(ThoughtName name)
    {
        if (!_noraThoughtDict.TryGetValue(name, out var entry)) return new List<string> { "." };
        
        return entry.NoraThoughtString;
        
    }

    // ───────────────────────────────────────────────────────────────────────
    // Eregi replacement
    // ───────────────────────────────────────────────────────────────────────

    public string GetReplacedString(string message)
    {
        if (string.IsNullOrEmpty(message)) return message;
        foreach (var kvp in _eregiDict)
            message = message.Replace(kvp.Key, kvp.Value);
        return message;
    }

    

    // ───────────────────────────────────────────────────────────────────────
    // Initialisation helpers
    // ───────────────────────────────────────────────────────────────────────

    
    async Task InitSeenAsync()
    {
        try
        {
            await StoredPrefs.WhenLoadedAsync();
            if (StoredPrefs.Instance != null)
                LoadWhatYouSee();
            else
                Debug.LogWarning("DialogueManager: StoredPrefs ready but Instance is null.");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"DialogueManager.InitSeenAsync failed: {e}");
        }
        finally
        {
            SeenLoaded = true;
            EventManager.PlayerDataLoaded();
        }
    }

    void BuildDialogueDictionary()
    {
        _dialogueDict.Clear();
        RepeatableDialogues.Clear();
        foreach (var d in Dialogues)
        {
            _dialogueDict.TryAdd(d.DialogueName, d);
            if (d.repeatable) RepeatableDialogues.Add(d.DialogueName);
        }
    }

    void BuildOSDDictionary()
    {
        _osdTextDict.Clear();
        foreach (var o in OSDTexts)
            _osdTextDict.TryAdd(o.OSDTextName, o);
    }


    void BuildNoraThoughtDictionary()
    {
        _noraThoughtDict.Clear();
        foreach (var o in NoraThoughts)
            _noraThoughtDict.TryAdd(o.ThoughtName, o);
    }

    void BuildEregiDictionary()
    {
        _eregiDict.Clear();
        bool isSteam = GameMaster.Instance.DeviceType.selectedDeviceType == PlayerDeviceType.SteamOS;

        _eregiDict["+phonekey+"]  = isSteam ? "X"                  : "P";
        _eregiDict["+torchkey+"]  = isSteam ? "Right Stick Button" : "H";
        _eregiDict["+camerakey+"] = isSteam ? "A"                  : "Enter";
        _eregiDict["+melee+"]     = isSteam ? "R2"                 : "Left Click";
    }

    // ───────────────────────────────────────────────────────────────────────
    // Persistence
    // ───────────────────────────────────────────────────────────────────────

    public void SaveWhatYouSee()
    {
        if (StoredPrefs.Instance == null) return;
        StoredPrefs.Instance.SetCollection("DialogueSeen", DialogueSeen, CollectionType.list);
        StoredPrefs.Instance.Save();
    }
    public void SaveWhatYouThought()
    {
        if (StoredPrefs.Instance == null) return;
        StoredPrefs.Instance.SetCollection("ThoughtsSeen", ThoughtsSeen, CollectionType.list);
        StoredPrefs.Instance.Save();
    }

    public void LoadWhatYouSee()
    {
        if (StoredPrefs.Instance == null) return;
        DialogueSeen = StoredPrefs.Instance.GetCollection<List<DialogueName>>("DialogueSeen") ?? new List<DialogueName>();
        ThoughtsSeen = StoredPrefs.Instance.GetCollection<List<ThoughtName>>("ThoughtsSeen") ?? new List<ThoughtName>();
    }
}