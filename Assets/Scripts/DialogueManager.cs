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
    [Header("Dialogue UI")]
    public GameObject DialogManager;
    private CanvasGroup DialogManagerCanvas;

    public TextMeshProUGUI ContactName;
    public Image ContactPhoto;
    public TextMeshProUGUI ReceivedMessage;
    public TextMeshProUGUI NoraMessage;
    public TextMeshProUGUI SystemMessage;

    [Header("Typewriter Animators")]
    public TMPTypewriter SystemMessageWriter;
    public TMPTypewriter NoraMessageWriter;
    public TMPTypewriter ReceivedMessageWriter;

    public Image timebar;
    public float messagetimer = 0f;

    public bool DialogInProgress;

    [Header("Dialogue Data")]
    public List<Dialogue> Dialogues = new();
    public List<OSDText> OSDTexts = new();
    private readonly Dictionary<DialogueName, Dialogue> DialogueDict = new();
    private readonly Dictionary<OSDTextName, OSDText> OSDTextDict = new();

    public List<DialogueName> RepeatableDialogues = new();
    public List<DialogueName> DialogueSeen = new();

    private readonly Dictionary<string, string> EregiDict = new();

    [Header("Cutscene")]
    public Camera mainCamera;
    public Zoom cameraZoom;

    public float originalFieldOfView = 70f;
    public float targetFieldOfView = 40f;
    public float panTime = 5f;
    public float duration = 10f;

    public bool CutsceneInProgress;
    public float elapsedCutsceneTime;

    public Dictionary<string, string> CutSceneSeen = new Dictionary<string, string>();

    public InputActionReference advanceDialogue;

    private bool cutsceneAdvanceRequested;  // kept for any existing references
    private bool advanceRequested;

    private bool _stopCutsceneRotation;

    public bool SeenLoaded;

    private Coroutine _fadeCo;

    private CancellationTokenSource _activeTimedCts;
    private bool _hasActiveTimed;
    private DialogueName _activeTimedDialogueName;
    private bool _activeTimedIsRepeatable;
    private bool _activeTimedWasShown;
    private bool _currentDialogueIsHeld;
    
    private void Start()
    {
        DialogManagerCanvas = DialogManager.GetComponent<CanvasGroup>();

        DialogInProgress = false;
        ClearAllText();

        PopulateDialogues();
        PopulateOSDTexts();
        CreateEregiDictionary();

        if (mainCamera != null)
            originalFieldOfView = mainCamera.fieldOfView;

        if (UInstance.Instance != null)
            UInstance.Instance.cutsceneBarsCanvas.alpha = 0;

        _ = InitSeenAsync();
    }

    private async Task InitSeenAsync()
    {
        try
        {
            await StoredPrefs.WhenLoadedAsync();

            if (StoredPrefs.Instance != null)
                LoadWhatYouSee();
            else
                Debug.LogWarning("DialogueManager: StoredPrefs is ready but Instance is null. Seen lists will remain default.");
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

    private void OnDisable()
    {
        CancelActiveTimedDialogue(markSeen: true);
    }

    private void LateUpdate()
    {
        if (!DialogInProgress) return;
        if (messagetimer > 0) timebar.fillAmount -= 1.0f / messagetimer * Time.deltaTime;
    }

    public Task PlayDialogue(DialogueName dialogueName, float displayTimer, DialogueType type, float cutsceneDuration = -1f, float cutscenePanTime = -1f, GameObject cutsceneTarget = null, bool isZoomable = true, bool holdUntilAdvance = false)
    {
        if (type == DialogueType.normal) return NewDialogue(dialogueName, displayTimer, holdUntilAdvance);

        if (cutsceneTarget == null)
        {
            Debug.LogError("DialogueManager.PlayDialogue: Cutscene requested but cutsceneTarget is null.");
            return Task.CompletedTask;
        }

        float useDuration = cutsceneDuration > 0 ? cutsceneDuration : duration;
        float usePanTime  = cutscenePanTime  > 0 ? cutscenePanTime  : panTime;

        return CutsceneWithDialogue(dialogueName, displayTimer, cutsceneTarget, useDuration, usePanTime, isZoomable, holdUntilAdvance);
    }

    public async Task NewDialogue(DialogueName dialogueName, float displaytimer, bool holdUntilAdvance = false)
    {
        if (!DialogInProgress)
            await CreateDialogue(dialogueName, displaytimer, holdUntilAdvance);
        else
            await Queuer(dialogueName, displaytimer, holdUntilAdvance);
    }

    public Task Queuer(DialogueName dialogueName, float displaytimer, bool holdUntilAdvance = false)
    {
        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(QueuerCoroutine(dialogueName, displaytimer, tcs, holdUntilAdvance));
        return tcs.Task;
    }

    private IEnumerator QueuerCoroutine(DialogueName dialogueName, float displaytimer, TaskCompletionSource<bool> tcs, bool holdUntilAdvance = false)
    {
        yield return new WaitWhile(() => DialogInProgress);
        _ = NewDialogue(dialogueName, displaytimer, holdUntilAdvance);
        tcs.SetResult(true);
    }

    public async Task CreateDialogue(DialogueName dialogueName, float displaytimer, bool holdUntilAdvance)
    {
        if (DialogueSeen.Contains(dialogueName) && !RepeatableDialogues.Contains(dialogueName))
        {
            if (holdUntilAdvance)
            {
                _currentDialogueIsHeld = holdUntilAdvance;
                ClearHeldCutsceneDialogue();
            }
            return;
        }

        Contacts contact = Contacts.System;
        string message = "...";

        if (DialogueDict.TryGetValue(dialogueName, out var selectedDialogue))
        {
            contact = selectedDialogue.Contact;
            message = selectedDialogue.DialogueText;

            if (selectedDialogue.EregiReplace)
                message = GetReplacedString(message);
        }

        if (holdUntilAdvance)
        {
            CancelActiveTimedDialogue(markSeen: true);

            messagetimer = 0f;
            DialogInProgress = true;

            // ✅ TYPEWRITER FIRST (fully awaited)
            if (contact == Contacts.System || contact == Contacts.Unknown)
            {
                await PlayWriterOrFallbackAsync(SystemMessageWriter, SystemMessage, message, CancellationToken.None);
            }
            else if (contact == Contacts.Nora)
            {
                await PlayWriterOrFallbackAsync(NoraMessageWriter, NoraMessage, message, CancellationToken.None);
            }
            else if (contact == Contacts.Ellsworth || contact == Contacts.Presha || contact == Contacts.Kim)
            {
                await PlayWriterOrFallbackWithPrefixAsync(SystemMessageWriter, SystemMessage, contact + ": ", message, CancellationToken.None);
            }
            else
            {
                timebar.fillAmount = 1.0f;
                StartFade(DialogManagerCanvas, 1);
                ContactName.text = contact.ToString();
                await PlayWriterOrFallbackAsync(ReceivedMessageWriter, ReceivedMessage, message, CancellationToken.None);
            }

            MarkDialogueSeen(dialogueName);

            // 🚨 ONLY NOW allow external systems to proceed
            EventManager.DialogueCanProceed(true);

            await WaitForPlayerAdvanceAsync();

            ClearHeldCutsceneDialogue();
            return;
        }

    // ── Normal timed dialogue ─────────────────────────────────────────
        CancelActiveTimedDialogue(markSeen: true);

        _activeTimedCts          = new CancellationTokenSource();
        _hasActiveTimed          = true;
        _activeTimedDialogueName = dialogueName;
        _activeTimedIsRepeatable = RepeatableDialogues.Contains(dialogueName);
        _activeTimedWasShown     = false;

        var token = _activeTimedCts.Token;

        messagetimer     = displaytimer;
        DialogInProgress = true;

        try
        {
            if (contact == Contacts.System)
            {
                _activeTimedWasShown = true;
                await PlayWriterOrFallbackAsync(SystemMessageWriter, SystemMessage, message, token);
                await SystemTimer(displaytimer, token);
            }
            else if (contact == Contacts.Unknown)
            {
                _activeTimedWasShown = true;
                await PlayWriterOrFallbackAsync(SystemMessageWriter, SystemMessage, contact + ": " + message, token);
                await SystemTimer(displaytimer, token);
            }
            else if (contact == Contacts.Nora)
            {
                _activeTimedWasShown = true;
                await PlayWriterOrFallbackAsync(NoraMessageWriter, NoraMessage, message, token);
                await NoraTimer(displaytimer, token);
            }
            else if (contact == Contacts.Ellsworth)
            {
                _activeTimedWasShown = true;
                await PlayWriterOrFallbackWithPrefixAsync(SystemMessageWriter, SystemMessage, contact + ": ", message, token);
                await SystemTimer(displaytimer, token);
            }
            else if (contact == Contacts.Presha)
            {
                _activeTimedWasShown = true;
                await PlayWriterOrFallbackWithPrefixAsync(SystemMessageWriter, SystemMessage, contact + ": ", message, token);
                await SystemTimer(displaytimer, token);
            }
            else if (contact == Contacts.Kim)
            {
                _activeTimedWasShown = true;
                await PlayWriterOrFallbackWithPrefixAsync(SystemMessageWriter, SystemMessage, contact + ": ", message, token);
                await SystemTimer(displaytimer, token);
            }
            else
            {
                ContactName.text     = contact.ToString();
                _activeTimedWasShown = true;
                await PlayWriterOrFallbackAsync(ReceivedMessageWriter, ReceivedMessage, message, token);
                await MessageTimer(displaytimer, token);
            }
        }
        catch (TaskCanceledException)
        {
            return;
        }
        finally
        {
            if (_hasActiveTimed && EqualityComparer<DialogueName>.Default.Equals(_activeTimedDialogueName, dialogueName))
            {
                _hasActiveTimed = false;
                _activeTimedCts?.Dispose();
                _activeTimedCts = null;
            }
        }

        MarkDialogueSeen(dialogueName);
    }



    /// <summary>
    /// Fire-and-forget with an instant prefix (e.g. "Ellsworth: ").
    /// </summary>
    private void PlayWriterOrFallbackWithPrefix(TMPTypewriter writer, TextMeshProUGUI fallback, string prefix, string body)
    {
        if (writer != null)
            _ = writer.PlayTextWithPrefix(prefix, body);
        else
            fallback.text = prefix + body;
    }

    /// <summary>
    /// Awaitable version for timed dialogues. Passes the cancellation token so the
    /// animation stops cleanly if the dialogue is interrupted.
    /// </summary>
    private Task PlayWriterOrFallbackAsync(TMPTypewriter writer, TextMeshProUGUI fallback, string message, CancellationToken token)
    {
        if (writer != null) return writer.PlayText(message, token);

        fallback.text = message;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Awaitable version with an instant prefix (e.g. "Ellsworth: ").
    /// </summary>
    private Task PlayWriterOrFallbackWithPrefixAsync(TMPTypewriter writer, TextMeshProUGUI fallback, string prefix, string body, CancellationToken token)
    {
        if (writer != null) return writer.PlayTextWithPrefix(prefix, body, token);

        fallback.text = prefix + body;
        return Task.CompletedTask;
    }

    // ── Seen / save ───────────────────────────────────────────────────────

    private void MarkDialogueSeen(DialogueName dialogueName)
    {
        if (!DialogueSeen.Contains(dialogueName)) DialogueSeen.Add(dialogueName);
        
        SaveWhatYouSee();
    }

    private void CancelActiveTimedDialogue(bool markSeen)
    {
        if (!_hasActiveTimed) return;

        try { _activeTimedCts?.Cancel(); } catch { }

        if (markSeen && _activeTimedWasShown && !_activeTimedIsRepeatable)
        {
            if (!DialogueSeen.Contains(_activeTimedDialogueName))
                DialogueSeen.Add(_activeTimedDialogueName);

            SaveWhatYouSee(); // save what you see, if you see it, save it. 
        }

        _hasActiveTimed = false;

        try { _activeTimedCts?.Dispose(); } catch { }
        _activeTimedCts = null;
    }


    private Task CutsceneWithDialogue(DialogueName dialogueName, float dialogueDisplayTimer, GameObject targetObject, float cutsceneDuration, float cutscenePanTime, bool isZoomable = true, bool holdUntilAdvance = false)
    {
        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(CutsceneCoroutine(dialogueName, dialogueDisplayTimer, targetObject, cutsceneDuration, cutscenePanTime, tcs, isZoomable, holdUntilAdvance));
        return tcs.Task;
    }

    private IEnumerator CutsceneCoroutine(
        DialogueName dialogueName,
        float dialogueDisplayTimer,
        GameObject targetObject,
        float cutsceneDuration,
        float cutscenePanTime,
        TaskCompletionSource<bool> tcs,
        bool isZoomable = true,
        bool holdUntilAdvance = false)
    {
        if (CutsceneInProgress)
        {
            tcs.SetResult(false);
            yield break;
        }

        GameMaster.Instance.PLAYERBUSY = true;
        CutsceneInProgress = true;
        elapsedCutsceneTime = 0f;
        _stopCutsceneRotation = false;

        StartCoroutine(UInstance.Instance.FadeInCutsceneBars(cutscenePanTime));

        yield return new WaitForSeconds(1f);

        // ✅ CRITICAL FIX: WAIT for dialogue to fully finish typing BEFORE continuing cutscene flow
        var dialogueTask = CreateDialogue(dialogueName, dialogueDisplayTimer, holdUntilAdvance);
        yield return new WaitUntil(() => dialogueTask.IsCompleted);

        float zoomTime = cutsceneDuration * 0.33f;
        float unzoomTime = cutsceneDuration * 0.33f;
        float holdTime = cutsceneDuration - zoomTime - unzoomTime;

        if (cameraZoom != null)
            cameraZoom.enabled = false;

        Coroutine zoomCo = null;

        if (isZoomable)
            zoomCo = StartCoroutine(CutsceneZoomSequence(zoomTime, holdTime, unzoomTime, holdUntilAdvance));

        while (elapsedCutsceneTime < cutsceneDuration && !_stopCutsceneRotation)
        {
            Vector3 targetDirection = targetObject.transform.position - mainCamera.transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            mainCamera.transform.rotation =
                Quaternion.Lerp(mainCamera.transform.rotation, targetRotation, cutscenePanTime * Time.smoothDeltaTime);

            elapsedCutsceneTime += Time.smoothDeltaTime;
            yield return null;
        }

        if (isZoomable)
            yield return zoomCo;

        Vector3 dir = (targetObject.transform.position - mainCamera.transform.position).normalized;
        float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        float pitch = Mathf.Asin(dir.y) * Mathf.Rad2Deg;

        Player.Instance.FirstPersonLook.SetPlayerRotation(new Vector2(yaw, pitch));

        CutsceneInProgress = false;
        GameMaster.Instance.PLAYERBUSY = false;

        SaveWhatYouSee();
        tcs.SetResult(true);
    }

    private IEnumerator CutsceneZoomSequence(float zoomTime, float holdTime, float unzoomTime, bool holdUntilAdvance)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(zoomTime, 0.0001f);
            mainCamera.fieldOfView = Mathf.Lerp(originalFieldOfView, targetFieldOfView, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        
        mainCamera.fieldOfView = targetFieldOfView;

        Debug.Log("holdUntilAdvance "+holdUntilAdvance);
        
        
        if (!holdUntilAdvance)
        {
            yield return new WaitForSeconds(holdTime);
        }
        else
        {
            while (!advanceRequested)
            {
                yield return null;
            }
        }

        Debug.Log("here?");
        
        _stopCutsceneRotation = true;

        ClearHeldCutsceneDialogue();

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(unzoomTime, 0.0001f);
            mainCamera.fieldOfView = Mathf.Lerp(targetFieldOfView, originalFieldOfView, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        mainCamera.fieldOfView = originalFieldOfView;

        StartCoroutine(UInstance.Instance.FadeOutCutsceneBars());

        if (cameraZoom != null)
        {
            cameraZoom.enabled = true;
            cameraZoom.AttachListeners();
        }
    }

    private void ClearHeldCutsceneDialogue()
    {
        messagetimer     = 0f;
        DialogInProgress = false;

        if (_fadeCo != null)
        {
            StopCoroutine(_fadeCo);
            _fadeCo = null;
        }

        ClearAllText();

        if (DialogManagerCanvas != null)
            DialogManagerCanvas.alpha = 0f;
    }

    /// <summary>Stops all typewriter animations and blanks every text field.</summary>
    private void ClearAllText()
    {
        if (SystemMessageWriter   != null) SystemMessageWriter.Clear();
        else if (SystemMessage    != null) SystemMessage.text = "";

        if (NoraMessageWriter     != null) NoraMessageWriter.Clear();
        else if (NoraMessage      != null) NoraMessage.text = "";

        if (ReceivedMessageWriter != null) ReceivedMessageWriter.Clear();
        else if (ReceivedMessage  != null) ReceivedMessage.text = "";

        if (ContactName           != null) ContactName.text = "";
    }

    private void RequestAdvance(InputAction.CallbackContext ctx)
    {
        advanceRequested = true;
        EventManager.DialogueCanProceed(false);
    }

    public IEnumerator WaitForPlayerAdvance()
    {
        advanceRequested = false;

        if (advanceDialogue != null)
        {
            advanceDialogue.action.performed -= RequestAdvance;
            advanceDialogue.action.performed += RequestAdvance;
        }
        else
        {
            Debug.LogWarning("DialogueManager: advanceDialogue not assigned.");
            yield break;
        }

        // ❌ REMOVED: EventManager.DialogueCanProceed(true);
        // This was causing early input validity and race conditions

        yield return new WaitUntil(() => advanceRequested);

        if (advanceDialogue != null)
            advanceDialogue.action.performed -= RequestAdvance;
    }
    public Task WaitForPlayerAdvanceAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(WaitForAdvanceCoroutine(tcs));
        return tcs.Task;
    }

    private IEnumerator WaitForAdvanceCoroutine(TaskCompletionSource<bool> tcs)
    {
        yield return WaitForPlayerAdvance();
        tcs.SetResult(true);
    }

    // ── Populate ──────────────────────────────────────────────────────────

    private void PopulateDialogues()
    {
        DialogueDict.Clear();
        foreach (var d in Dialogues)
        {
            DialogueDict.TryAdd(d.DialogueName, d);
            if (d.repeatable)
                RepeatableDialogues.Add(d.DialogueName);
        }
    }

    private void PopulateOSDTexts()
    {
        OSDTextDict.Clear();
        foreach (var o in OSDTexts)
            OSDTextDict.TryAdd(o.OSDTextName, o);
    }

    private void CreateEregiDictionary()
    {
        EregiDict.Clear();

        if (GameMaster.Instance.DeviceType.selectedDeviceType == PlayerDeviceType.SteamOS)
        {
            EregiDict.TryAdd("+phonekey+",    "X");
            EregiDict.TryAdd("+torchkey+",    "Right Stick Button");
            EregiDict.TryAdd("+camerakey+",   "A");
            EregiDict.TryAdd("+melee+",       "R2");
        }
        else
        {
            EregiDict.TryAdd("+phonekey+",    "P");
            EregiDict.TryAdd("+torchkey+",    "H");
            EregiDict.TryAdd("+camerakey+",   "Enter");
            EregiDict.TryAdd("+melee+",       "Left Click");
        }
    }

    public string GetReplacedString(string message)
    {
        if (string.IsNullOrEmpty(message)) return message;

        foreach (var kvp in EregiDict)
            message = message.Replace(kvp.Key, kvp.Value);

        return message;
    }

    // ── Fade ─────────────────────────────────────────────────────────────

    private void StartFade(CanvasGroup canvas, int direction)
    {
        if (canvas == null) return;
        if (_fadeCo != null) StopCoroutine(_fadeCo);
        _fadeCo = StartCoroutine(Fader(canvas, direction));
    }

    public IEnumerator Fader(CanvasGroup ThisCanvas, int direction)
    {
        var counter = 9;
        if (direction == 0)
        {
            while (counter > 0)
            {
                yield return new WaitForSeconds(0.05f);
                ThisCanvas.alpha -= 0.1f;
                counter--;
            }
        }
        else
        {
            while (counter > 0)
            {
                yield return new WaitForSeconds(0.05f);
                ThisCanvas.alpha += 0.1f;
                counter--;
            }
        }
    }

    // ── Timers ────────────────────────────────────────────────────────────

    public async Task MessageTimer(float timevalue, CancellationToken token)
    {
        timebar.fillAmount = 1.0f;
        StartFade(DialogManagerCanvas, 1);

        await Task.Delay((int)(timevalue * 1000), token);

        StartFade(DialogManagerCanvas, 0);

        if (ReceivedMessageWriter != null) ReceivedMessageWriter.Clear();
        else                               ReceivedMessage.text = "";

        ContactName.text = "";

        await Task.Delay(500, token);
        DialogInProgress = false;
    }

    public async Task NoraTimer(float timevalue, CancellationToken token)
    {
        await Task.Delay((int)(timevalue * 1000), token);

        if (NoraMessageWriter != null) NoraMessageWriter.Clear();
        else                           NoraMessage.text = "";

        await Task.Delay(500, token);
        DialogInProgress = false;
    }

    public async Task SystemTimer(float timevalue, CancellationToken token)
    {
        await Task.Delay((int)(timevalue * 1000), token);

        if (SystemMessageWriter != null) SystemMessageWriter.Clear();
        else                             SystemMessage.text = "";

        await Task.Delay(500, token);
        DialogInProgress = false;
    }

    // ── OSD ──────────────────────────────────────────────────────────────

    public string RetrieveOSDText(OSDTextName requestedOSDText)
    {
        if (OSDTextDict.TryGetValue(requestedOSDText, out var selected))
        {
            string msg = selected.OSDTextString;

            if (selected.EregiReplace)
            {
                foreach (var kvp in EregiDict)
                    if (!string.IsNullOrEmpty(msg))
                        msg = msg.Replace(kvp.Key, kvp.Value);
            }

            return msg;
        }

        return ".";
    }

    // ── Persistence ───────────────────────────────────────────────────────

    public void SaveWhatYouSee()
    {
        if (StoredPrefs.Instance == null) return;
        StoredPrefs.Instance.SetCollection("DialogueSeen",  DialogueSeen,  CollectionType.list);
        StoredPrefs.Instance.SetCollection("CutSceneSeen",  CutSceneSeen,  CollectionType.dictionary);
        StoredPrefs.Instance.Save();
    }

    public void LoadWhatYouSee()
    {
        if (StoredPrefs.Instance == null) return;
        DialogueSeen = StoredPrefs.Instance.GetCollection<List<DialogueName>>("DialogueSeen")              ?? new List<DialogueName>();
        CutSceneSeen = StoredPrefs.Instance.GetCollection<Dictionary<string, string>>("CutSceneSeen") ?? new Dictionary<string, string>();
    }
}
