using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    [Header("Typewriters")]
    public TMPTypewriter SystemMessageWriter;
    public TMPTypewriter NoraMessageWriter;
    public TMPTypewriter ReceivedMessageWriter;

    [Header("Timer Bar")]
    public Image timebar;
    public float messagetimer;

    // ─── Dialogue Data ─────────────────────────────────────────────────────

    [Header("Dialogue Data")]
    public List<Dialogue> Dialogues   = new();
    public List<OSDText>  OSDTexts    = new();

    public List<DialogueName> RepeatableDialogues = new();
    public List<DialogueName> DialogueSeen        = new();

    
    
    // ─── Cutscene ──────────────────────────────────────────────────────────

    [Header("Cutscene")]
    public Camera mainCamera;
    public Zoom   cameraZoom;
    public float  originalFieldOfView = 70f;
    public float  targetFieldOfView   = 40f;
    public float  panTime             = 5f;
    public float  duration            = 10f;

    // ─── Input ─────────────────────────────────────────────────────────────

    public InputActionReference advanceDialogue;

    // ─── State ─────────────────────────────────────────────────────────────

    public bool DialogInProgress  { get; private set; }
    public bool CutsceneInProgress{ get; private set; }
    public bool SeenLoaded        { get; private set; }

    // ─── Private state ─────────────────────────────────────────────────────

    readonly Dictionary<DialogueName, Dialogue>  _dialogueDict  = new();
    readonly Dictionary<OSDTextName,  OSDText>   _osdTextDict   = new();
    readonly Dictionary<string, string>          _eregiDict     = new();

    // Timed-dialogue cancellation
    CancellationTokenSource _timedCts;
    bool       _hasActiveTimed;
    DialogueName _activeTimedName;
    bool       _activeTimedIsRepeatable;
    bool       _activeTimedWasShown;

    // Cutscene internals
    float _elapsedCutsceneTime;
    bool  _stopCutsceneRotation;

    // Advance-key state
    bool _advanceRequested;

    // Fade coroutine handle
    Coroutine _fadeCo;

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

        if (mainCamera != null)
            originalFieldOfView = mainCamera.fieldOfView;

        if (UInstance.Instance != null)
            UInstance.Instance.cutsceneBarsCanvas.alpha = 0;

        if (PhoneMessageCanvas      != null) PhoneMessageCanvas.alpha      = 0f;
        if (OnscreenDialogueCanvas  != null) OnscreenDialogueCanvas.alpha  = 0f;

        _ = InitSeenAsync();
    }

    void LateUpdate()
    {
        if (!DialogInProgress || messagetimer <= 0) return;
        timebar.fillAmount -= (1f / messagetimer) * Time.deltaTime;
    }

    void OnDisable() => CancelActiveTimed(markSeen: true);

    // ───────────────────────────────────────────────────────────────────────
    // Public entry points
    // ───────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Primary entry point. Queues if a dialogue is already running.
    /// </summary>
    public Task PlayDialogue(
        DialogueName  name,
        float         displayTimer,
        DialogueType  type,
        float         cutsceneDuration  = -1f,
        float         cutscenePanTime   = -1f,
        GameObject    cutsceneTarget    = null,
        bool          isZoomable        = true,
        bool          holdUntilAdvance  = false)
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
                float usePanTime  = cutscenePanTime  > 0 ? cutscenePanTime  : panTime;
                return StartCutscene(name, displayTimer, cutsceneTarget, useDuration, usePanTime, isZoomable, holdUntilAdvance);

            default: // normal + sms both go through the same queue
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
        StartCoroutine(QueueCoroutine(name, timer, type, hold, tcs));
        return tcs.Task;
    }

    IEnumerator QueueCoroutine(DialogueName name, float timer, DialogueType type, bool hold, TaskCompletionSource<bool> tcs)
    {
        yield return new WaitWhile(() => DialogInProgress);
        _ = RunDialogue(name, timer, type, hold);
        tcs.SetResult(true);
    }

    // ───────────────────────────────────────────────────────────────────────
    // Core dialogue runner
    // ───────────────────────────────────────────────────────────────────────

    async Task RunDialogue(DialogueName name, float displayTimer, DialogueType type, bool holdUntilAdvance)
    {
        // Skip already-seen non-repeatable dialogues
        if (DialogueSeen.Contains(name) && !RepeatableDialogues.Contains(name))
        {
            if (holdUntilAdvance) ClearHeldDialogue();
            return;
        }

        // Resolve content
        if (!_dialogueDict.TryGetValue(name, out var data))
        {
            Debug.LogWarning($"DialogueManager: No dialogue found for '{name}'.");
            return;
        }

        string message = data.EregiReplace
            ? GetReplacedString(data.DialogueText)
            : data.DialogueText;

        // ── Hold-to-advance path ───────────────────────────────────────────

        if (holdUntilAdvance)
        {
            CancelActiveTimed(markSeen: true);
            messagetimer     = 0f;
            DialogInProgress = true;

            ShowOnscreenDialogue(data.Contact);
            await TypeOnscreen(data.Contact, message, CancellationToken.None);

            MarkSeen(name);
            EventManager.DialogueCanProceed(true);
            await WaitForPlayerAdvanceAsync();

            ClearHeldDialogue();
            return;
        }

        // ── Timed path ────────────────────────────────────────────────────

        CancelActiveTimed(markSeen: true);

        _timedCts               = new CancellationTokenSource();
        _hasActiveTimed         = true;
        _activeTimedName        = name;
        _activeTimedIsRepeatable= RepeatableDialogues.Contains(name);
        _activeTimedWasShown    = false;
        messagetimer            = displayTimer;
        DialogInProgress        = true;

        var ct = _timedCts.Token;

        try
        {
            switch (type)
            {
                case DialogueType.SMS:
                    await RunSMS(data.Contact, message, displayTimer, ct);
                    break;

                default: // DialogueType.normal
                    await RunNormal(data.Contact, message, displayTimer, ct);
                    break;
            }
        }
        catch (TaskCanceledException) { return; }
        finally
        {
            // Only clean up if we're still the active timed dialogue
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
    // Display helpers — Normal / Onscreen
    // ───────────────────────────────────────────────────────────────────────
// ───────────────────────────────────────────────────────────────────────
// Backwards-compatibility shims
// External scripts that called these members will compile without changes.
// ───────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Legacy entry point used by Evidence, DialogueBeef, OnboardingManager,
    /// Player, Phone etc. Routes into the new queue/play pipeline.
    /// </summary>
    public Task NewDialogue(DialogueName name, float displayTimer, bool holdUntilAdvance = false) => PlayDialogue(name, displayTimer, DialogueType.normal, displayTimer,displayTimer, null, false, holdUntilAdvance);

    /// <summary>
    /// Legacy field read/written by cutscene.cs.
    /// Wraps the private _elapsedCutsceneTime so external code still compiles.
    /// </summary>
    public float elapsedCutsceneTime
    {
        get => elapsedCutsceneTime;
        set => elapsedCutsceneTime = value;
    }

    
    async Task RunNormal(Contacts contact, string message, float displayTimer, CancellationToken ct)
    {
        ShowOnscreenDialogue(contact);
        _activeTimedWasShown = true;

        await TypeOnscreen(contact, message, ct);
        await DialogueTimer(displayTimer, ct);
    }

    /// <summary>Sets speaker name visibility and canvas alpha based on contact.</summary>
    void ShowOnscreenDialogue(Contacts contact)
    {
        bool hasSpeakerName = contact != Contacts.System && contact != Contacts.Unknown;
        DialogueSpeakerNameCanvas.alpha = hasSpeakerName ? 1f : 0f;
        if (hasSpeakerName) DialogueSpeakerName.text = contact.ToString();
        OnscreenDialogueCanvas.alpha = 1f;
    }

    /// <summary>Types the message using the appropriate writer for the contact.</summary>
    Task TypeOnscreen(Contacts contact, string message, CancellationToken ct)
    {
        var writer = contact == Contacts.Nora ? NoraMessageWriter : SystemMessageWriter;
        return PlayWriterOrFallback(writer, SpeakerText, message, ct);
    }

    // ───────────────────────────────────────────────────────────────────────
    // Display helpers — SMS
    // ───────────────────────────────────────────────────────────────────────

    async Task RunSMS(Contacts contact, string message, float displayTimer, CancellationToken ct)
    {
        ContactName.text     = contact.ToString();
        ReceivedMessage.text = message;   // SMS never uses typewriter
        _activeTimedWasShown = true;

        await MessageTimer(displayTimer, ct);
    }

    // ───────────────────────────────────────────────────────────────────────
    // Timer helpers
    // ───────────────────────────────────────────────────────────────────────

    async Task MessageTimer(float duration, CancellationToken ct)
    {
        timebar.fillAmount = 1f;
        StartFade(PhoneMessageCanvas, fadeIn: true);

        await Task.Delay((int)(duration * 1000), ct);

        StartFade(PhoneMessageCanvas, fadeIn: false);
        ReceivedMessage.text = "";
        ContactName.text     = "";

        await Task.Delay(500, ct);
        DialogInProgress = false;
    }

    async Task DialogueTimer(float duration, CancellationToken ct)
    {
        await Task.Delay((int)(duration * 1000), ct);

        if (SystemMessageWriter != null) SystemMessageWriter.Clear();
        else if (SpeakerText != null)    SpeakerText.text = "";

        await Task.Delay(500, ct);
        DialogInProgress = false;
    }

    // ───────────────────────────────────────────────────────────────────────
    // Cutscene
    // ───────────────────────────────────────────────────────────────────────

    Task StartCutscene(DialogueName name, float dialogueTimer, GameObject target, float cutsceneDuration, float cutscenePanTime, bool isZoomable, bool hold)
    {
        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(CutsceneCoroutine(name, dialogueTimer, target, cutsceneDuration, cutscenePanTime, tcs, isZoomable, hold));
        return tcs.Task;
    }

    IEnumerator CutsceneCoroutine(
        DialogueName name, float dialogueTimer, GameObject target,
        float cutsceneDuration, float cutscenePanTime,
        TaskCompletionSource<bool> tcs,
        bool isZoomable, bool hold)
    {
        if (CutsceneInProgress) { tcs.SetResult(false); yield break; }

        GameMaster.Instance.PLAYERBUSY = true;
        CutsceneInProgress      = true;
        _elapsedCutsceneTime    = 0f;
        _stopCutsceneRotation   = false;

        StartCoroutine(UInstance.Instance.FadeInCutsceneBars(cutscenePanTime));

        yield return new WaitForSeconds(1f);

        var dialogueTask = RunDialogue(name, dialogueTimer, DialogueType.normal, hold);
        yield return new WaitUntil(() => dialogueTask.IsCompleted);

        float zoomTime   = cutsceneDuration * 0.33f;
        float unzoomTime = cutsceneDuration * 0.33f;
        float holdTime   = cutsceneDuration - zoomTime - unzoomTime;

        if (cameraZoom != null) cameraZoom.enabled = false;

        Coroutine zoomCo = null;
        if (isZoomable)
            zoomCo = StartCoroutine(CutsceneZoomSequence(zoomTime, holdTime, unzoomTime, hold));

        while (_elapsedCutsceneTime < cutsceneDuration && !_stopCutsceneRotation)
        {
            Vector3    dir      = target.transform.position - mainCamera.transform.position;
            Quaternion rotation = Quaternion.LookRotation(dir);
            mainCamera.transform.rotation = Quaternion.Lerp(
                mainCamera.transform.rotation, rotation, cutscenePanTime * Time.smoothDeltaTime);

            _elapsedCutsceneTime += Time.smoothDeltaTime;
            yield return null;
        }

        if (isZoomable && zoomCo != null) yield return zoomCo;

        Vector3 finalDir = (target.transform.position - mainCamera.transform.position).normalized;
        float yaw   = Mathf.Atan2(finalDir.x, finalDir.z) * Mathf.Rad2Deg;
        float pitch = Mathf.Asin(finalDir.y)               * Mathf.Rad2Deg;
        Player.Instance.FirstPersonLook.SetPlayerRotation(new Vector2(yaw, pitch));

        CutsceneInProgress             = false;
        GameMaster.Instance.PLAYERBUSY = false;

        SaveWhatYouSee();
        tcs.SetResult(true);
    }

    IEnumerator CutsceneZoomSequence(float zoomTime, float holdTime, float unzoomTime, bool hold)
    {
        yield return LerpFOV(originalFieldOfView, targetFieldOfView, zoomTime);

        if (hold)
            yield return new WaitUntil(() => _advanceRequested);
        else
            yield return new WaitForSeconds(holdTime);

        _stopCutsceneRotation = true;
        ClearHeldDialogue();

        yield return LerpFOV(targetFieldOfView, originalFieldOfView, unzoomTime);

        StartCoroutine(UInstance.Instance.FadeOutCutsceneBars());

        if (cameraZoom != null)
        {
            cameraZoom.enabled = true;
            cameraZoom.AttachListeners();
        }
    }

    IEnumerator LerpFOV(float from, float to, float time)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(time, 0.0001f);
            mainCamera.fieldOfView = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
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

    public Task WaitForPlayerAdvanceAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(WaitForAdvanceCoroutine(tcs));
        return tcs.Task;
    }

    IEnumerator WaitForAdvanceCoroutine(TaskCompletionSource<bool> tcs)
    {
        _advanceRequested = false;

        if (advanceDialogue == null)
        {
            Debug.LogWarning("DialogueManager: advanceDialogue not assigned.");
            tcs.SetResult(false);
            yield break;
        }

        advanceDialogue.action.performed -= RequestAdvance;
        advanceDialogue.action.performed += RequestAdvance;

        yield return new WaitUntil(() => _advanceRequested);

        advanceDialogue.action.performed -= RequestAdvance;
        tcs.SetResult(true);
    }

    // ───────────────────────────────────────────────────────────────────────
    // Typewriter helper
    // ───────────────────────────────────────────────────────────────────────

    Task PlayWriterOrFallback(TMPTypewriter writer, TextMeshProUGUI fallback, string message, CancellationToken ct)
    {
        if (writer != null) return writer.PlayText(message, ct);
        fallback.text = message;
        return Task.CompletedTask;
    }

    // ───────────────────────────────────────────────────────────────────────
    // Seen / Repeatable tracking
    // ───────────────────────────────────────────────────────────────────────

    void MarkSeen(DialogueName name)
    {
        if (!DialogueSeen.Contains(name))
            DialogueSeen.Add(name);
        SaveWhatYouSee();
    }

    void CancelActiveTimed(bool markSeen)
    {
        if (!_hasActiveTimed) return;

        try { _timedCts?.Cancel(); } catch { /* ignored */ }

        if (markSeen && _activeTimedWasShown && !_activeTimedIsRepeatable)
        {
            if (!DialogueSeen.Contains(_activeTimedName))
                DialogueSeen.Add(_activeTimedName);
            SaveWhatYouSee();
        }

        _hasActiveTimed = false;
        try { _timedCts?.Dispose(); } catch { /* ignored */ }
        _timedCts = null;
    }

    // ───────────────────────────────────────────────────────────────────────
    // UI clear helpers
    // ───────────────────────────────────────────────────────────────────────

    void ClearHeldDialogue()
    {
        messagetimer     = 0f;
        DialogInProgress = false;

        if (_fadeCo != null) { StopCoroutine(_fadeCo); _fadeCo = null; }

        ClearAllUI();
        if (PhoneMessageCanvas != null) PhoneMessageCanvas.alpha = 0f;
    }

    void ClearAllUI()
    {
        SystemMessageWriter?.Clear();
        NoraMessageWriter?.Clear();
        ReceivedMessageWriter?.Clear();

        if (SpeakerText      != null) SpeakerText.text      = "";
        if (ReceivedMessage  != null) ReceivedMessage.text  = "";
        if (ContactName      != null) ContactName.text      = "";

        if (OnscreenDialogueCanvas   != null) OnscreenDialogueCanvas.alpha   = 0f;
        if (DialogueSpeakerNameCanvas!= null) DialogueSpeakerNameCanvas.alpha= 0f;
    }

    // ───────────────────────────────────────────────────────────────────────
    // Fade helper
    // ───────────────────────────────────────────────────────────────────────

    void StartFade(CanvasGroup canvas, bool fadeIn)
    {
        if (canvas == null) return;
        if (_fadeCo != null) StopCoroutine(_fadeCo);
        _fadeCo = StartCoroutine(FadeCoroutine(canvas, fadeIn));
    }

    IEnumerator FadeCoroutine(CanvasGroup canvas, bool fadeIn)
    {
        float target = fadeIn ? 1f : 0f;
        float step   = (fadeIn ? 1f : -1f) * 0.1f;
        for (int i = 0; i < 9; i++)
        {
            yield return new WaitForSeconds(0.05f);
            canvas.alpha = Mathf.Clamp01(canvas.alpha + step);
        }
        canvas.alpha = target;
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

    public void LoadWhatYouSee()
    {
        if (StoredPrefs.Instance == null) return;
        DialogueSeen = StoredPrefs.Instance.GetCollection<List<DialogueName>>("DialogueSeen")
                       ?? new List<DialogueName>();
    }
}