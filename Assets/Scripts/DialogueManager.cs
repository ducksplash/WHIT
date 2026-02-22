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

    private bool cutsceneAdvanceRequested;
    private Contacts _currentCutsceneContact = Contacts.System;

    // stop the rotation loop immediately when we’re done
    private bool _stopCutsceneRotation;

    public bool SeenLoaded;

    // keep track of fade coroutine so we can stop it reliably
    private Coroutine _fadeCo;

    // ✅ NEW: track currently-active timed dialogue so it can be cancelled + finalized if interrupted
    private CancellationTokenSource _activeTimedCts;
    private bool _hasActiveTimed;
    private DialogueName _activeTimedDialogueName;
    private bool _activeTimedIsRepeatable;
    private bool _activeTimedWasShown; // only mark seen if it actually displayed something

    private void Start()
    {
        DialogManagerCanvas = DialogManager.GetComponent<CanvasGroup>();

        DialogInProgress = false;
        NoraMessage.text = "";
        SystemMessage.text = "";

        PopulateDialogues();
        PopulateOSDTexts();
        CreateEregiDictionary();

        if (mainCamera != null)
            originalFieldOfView = mainCamera.fieldOfView;

        if (UInstance.Instance != null)
            UInstance.Instance.cutsceneBarsCanvas.alpha = 0;

        // ✅ Robust: SeenLoaded becomes true when StoredPrefs is done loading,
        // regardless of whether any data exists.
        _ = InitSeenAsync();
    }

    // ✅ This is the key change.
    private async Task InitSeenAsync()
    {
        try
        {
            await StoredPrefs.WhenLoadedAsync();

            // If StoredPrefs is ready but Instance is still null (shouldn't happen with auto-create),
            // guard anyway so SeenLoaded still flips true.
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
            
            GameMaster.Instance.EventManager.PlayerDataLoaded();
        }
    }

    private void OnDisable()
    {
        // clean up any pending timed dialogue
        CancelActiveTimedDialogue(markSeen: true);
    }

    private void LateUpdate()
    {
        if (!DialogInProgress) return;
        if (messagetimer > 0) timebar.fillAmount -= 1.0f / messagetimer * Time.deltaTime;
    }

    public Task PlayDialogue(DialogueName dialogueName, float displayTimer, DialogueType type,
        float cutsceneDuration = -1f, float cutscenePanTime = -1f, GameObject cutsceneTarget = null)
    {
        if (type == DialogueType.normal)
            return NewDialogue(dialogueName, displayTimer);

        if (cutsceneTarget == null)
        {
            Debug.LogError("DialogueManager.PlayDialogue: Cutscene requested but cutsceneTarget is null.");
            return Task.CompletedTask;
        }

        float useDuration = cutsceneDuration > 0 ? cutsceneDuration : duration;
        float usePanTime = cutscenePanTime > 0 ? cutscenePanTime : panTime;

        return CutsceneWithDialogue(dialogueName, displayTimer, cutsceneTarget, useDuration, usePanTime);
    }

    public async Task NewDialogue(DialogueName dialogueName, float displaytimer)
    {
        if (!DialogInProgress)
            await CreateDialogue(dialogueName, displaytimer, holdUntilAdvance: false);
        else
            await Queuer(dialogueName, displaytimer);
    }

    public Task Queuer(DialogueName dialogueName, float displaytimer)
    {
        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(QueuerCoroutine(dialogueName, displaytimer, tcs));
        return tcs.Task;
    }

    private IEnumerator QueuerCoroutine(DialogueName dialogueName, float displaytimer, TaskCompletionSource<bool> tcs)
    {
        yield return new WaitWhile(() => DialogInProgress);
        _ = NewDialogue(dialogueName, displaytimer);
        tcs.SetResult(true);
    }

    public async Task CreateDialogue(DialogueName dialogueName, float displaytimer, bool holdUntilAdvance)
    {
        // If dialogue is already seen and not repeatable:
        if (DialogueSeen.Contains(dialogueName) && !RepeatableDialogues.Contains(dialogueName))
        {
            if (holdUntilAdvance)
            {
                _currentCutsceneContact = Contacts.System;
                ClearHeldCutsceneDialogue(); // safe + idempotent
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

        _currentCutsceneContact = contact;

        // Held cutscene dialogue (press-to-advance style)
        if (holdUntilAdvance)
        {
            // ✅ if a timed dialogue was active, it’s being interrupted; finalize it as seen
            CancelActiveTimedDialogue(markSeen: true);

            messagetimer = 0f;
            DialogInProgress = true;

            if (contact == Contacts.System)
            {
                SystemMessage.text = message;
            }
            else if (contact == Contacts.Nora)
            {
                NoraMessage.text = message;
            }
            else
            {
                timebar.fillAmount = 1.0f;
                StartFade(DialogManagerCanvas, 1);
                ContactName.text = contact.ToString();
                ReceivedMessage.text = message;
            }

            MarkDialogueSeen(dialogueName);
            return;
        }

        // ✅ Normal timed dialogue:
        CancelActiveTimedDialogue(markSeen: true);

        _activeTimedCts = new CancellationTokenSource();
        _hasActiveTimed = true;
        _activeTimedDialogueName = dialogueName;
        _activeTimedIsRepeatable = RepeatableDialogues.Contains(dialogueName);
        _activeTimedWasShown = false;

        var token = _activeTimedCts.Token;

        messagetimer = displaytimer;
        DialogInProgress = true;

        try
        {
            if (contact == Contacts.System)
            {
                SystemMessage.text = message;
                _activeTimedWasShown = true;
                await SystemTimer(displaytimer, token);
            }
            else if (contact == Contacts.Nora)
            {
                NoraMessage.text = message;
                _activeTimedWasShown = true;
                await NoraTimer(displaytimer, token);
            }
            else
            {
                ContactName.text = contact.ToString();
                ReceivedMessage.text = message;
                _activeTimedWasShown = true;
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

    private void MarkDialogueSeen(DialogueName dialogueName)
    {
        if (!DialogueSeen.Contains(dialogueName))
            DialogueSeen.Add(dialogueName);
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

            SaveWhatYouSee();
        }

        _hasActiveTimed = false;

        try { _activeTimedCts?.Dispose(); } catch { }
        _activeTimedCts = null;
    }

    private Task CutsceneWithDialogue(DialogueName dialogueName, float dialogueDisplayTimer, GameObject targetObject,
        float cutsceneDuration, float cutscenePanTime)
    {
        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(CutsceneCoroutine(dialogueName, dialogueDisplayTimer, targetObject, cutsceneDuration, cutscenePanTime, tcs));
        return tcs.Task;
    }

    private IEnumerator CutsceneCoroutine(DialogueName dialogueName, float dialogueDisplayTimer, GameObject targetObject,
        float cutsceneDuration, float cutscenePanTime, TaskCompletionSource<bool> tcs)
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

        if (UInstance.Instance != null) StartCoroutine(UInstance.Instance.FadeInCutsceneBars(cutscenePanTime));

        yield return new WaitForSeconds(1f);

        _ = CreateDialogue(dialogueName, dialogueDisplayTimer, holdUntilAdvance: true);

        float zoomTime = cutsceneDuration * 0.33f;
        float unzoomTime = cutsceneDuration * 0.33f;
        float holdTime = cutsceneDuration - zoomTime - unzoomTime;

        if (cameraZoom != null)
            cameraZoom.enabled = false;

        Coroutine zoomCo = StartCoroutine(CutsceneZoomSequence(zoomTime, holdTime, unzoomTime));

        while (elapsedCutsceneTime < cutsceneDuration && !_stopCutsceneRotation)
        {
            Vector3 targetDirection = targetObject.transform.position - mainCamera.transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            mainCamera.transform.rotation = Quaternion.Lerp(
                mainCamera.transform.rotation,
                targetRotation,
                cutscenePanTime * Time.smoothDeltaTime
            );

            elapsedCutsceneTime += Time.smoothDeltaTime;
            yield return null;
        }

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

    private IEnumerator CutsceneZoomSequence(float zoomTime, float holdTime, float unzoomTime)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(zoomTime, 0.0001f);
            mainCamera.fieldOfView = Mathf.Lerp(originalFieldOfView, targetFieldOfView, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        mainCamera.fieldOfView = targetFieldOfView;

        yield return StartCoroutine(WaitForCutsceneAdvance());

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

        if (UInstance.Instance != null)
            StartCoroutine(UInstance.Instance.FadeOutCutsceneBars());

        if (cameraZoom != null)
        {
            cameraZoom.enabled = true;
            cameraZoom.AttachListeners();
        }
    }

    private void ClearHeldCutsceneDialogue()
    {
        messagetimer = 0f;
        DialogInProgress = false;

        if (_fadeCo != null)
        {
            StopCoroutine(_fadeCo);
            _fadeCo = null;
        }

        if (ContactName != null) ContactName.text = "";
        if (ReceivedMessage != null) ReceivedMessage.text = "";
        if (NoraMessage != null) NoraMessage.text = "";
        if (SystemMessage != null) SystemMessage.text = "";

        if (DialogManagerCanvas != null)
            DialogManagerCanvas.alpha = 0f;
    }

    private void RequestCutsceneAdvance(InputAction.CallbackContext ctx)
    {
        cutsceneAdvanceRequested = true;
    }

    private IEnumerator WaitForCutsceneAdvance()
    {
        cutsceneAdvanceRequested = false;

        if (advanceDialogue != null)
        {
            advanceDialogue.action.performed -= RequestCutsceneAdvance;
            advanceDialogue.action.performed += RequestCutsceneAdvance;
        }
        else
        {
            Debug.LogWarning("DialogueManager: advanceDialogue is not assigned. Cutscenes will never advance.");
        }

        yield return new WaitUntil(() => cutsceneAdvanceRequested);

        if (advanceDialogue != null)
            advanceDialogue.action.performed -= RequestCutsceneAdvance;
    }

    private void PopulateDialogues()
    {
        DialogueDict.Clear();
        foreach (var d in Dialogues)
            DialogueDict.TryAdd(d.DialogueName, d);
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
            EregiDict.TryAdd("+phonekey+", "X");
            EregiDict.TryAdd("+torchkey+", "Right Stick Button");
            EregiDict.TryAdd("+camerakey+", "A");
            EregiDict.TryAdd("+melee+", "R2");
        }
        else
        {
            EregiDict.TryAdd("+phonekey+", "P");
            EregiDict.TryAdd("+torchkey+", "H");
            EregiDict.TryAdd("+camerakey+", "Enter");
            EregiDict.TryAdd("+melee+", "Left Click");
        }
    }

    public string GetReplacedString(string message)
    {
        if (string.IsNullOrEmpty(message))
            return message;

        foreach (var kvp in EregiDict)
            message = message.Replace(kvp.Key, kvp.Value);

        return message;
    }

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

    public async Task MessageTimer(float timevalue, CancellationToken token)
    {
        timebar.fillAmount = 1.0f;
        StartFade(DialogManagerCanvas, 1);

        await Task.Delay((int)(timevalue * 1000), token);

        StartFade(DialogManagerCanvas, 0);
        ContactName.text = "";
        ReceivedMessage.text = "";
        await Task.Delay(500, token);
        DialogInProgress = false;
    }

    public async Task NoraTimer(float timevalue, CancellationToken token)
    {
        await Task.Delay((int)(timevalue * 1000), token);
        NoraMessage.text = "";
        await Task.Delay(500, token);
        DialogInProgress = false;
    }

    public async Task SystemTimer(float timevalue, CancellationToken token)
    {
        await Task.Delay((int)(timevalue * 1000), token);
        SystemMessage.text = "";
        await Task.Delay(500, token);
        DialogInProgress = false;
    }

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

    public void SaveWhatYouSee()
    {
        if (StoredPrefs.Instance == null) return;
        StoredPrefs.Instance.SetCollection("DialogueSeen", DialogueSeen, CollectionType.list);
        StoredPrefs.Instance.SetCollection("CutSceneSeen", CutSceneSeen, CollectionType.dictionary);
        StoredPrefs.Instance.Save();
    }

    public void LoadWhatYouSee()
    {
        if (StoredPrefs.Instance == null) return;
        DialogueSeen = StoredPrefs.Instance.GetCollection<List<DialogueName>>("DialogueSeen") ?? new List<DialogueName>();
        CutSceneSeen = StoredPrefs.Instance.GetCollection<Dictionary<string, string>>("CutSceneSeen") ?? new Dictionary<string, string>();
    }
}

public enum DialogueType
{
    cutscene,
    normal
}

public enum OSDTextName
{
    TakePhoto = 101,
    SavedPhoto = 102
}

public enum DialogueName
{
    None,
    KieronToNoraBathroom = 100,
    NoraAboutKieronBathroom = 101,
    NoraLookingAtCorkboard = 102,
    NoraNeedsHerThings = 103,
    NoraNeedsTestEvidence = 104,
    NoraReadyToGo = 105,
    NoraCollectedNotepad = 106,
    NoraCollectedPhone = 107,
    NoraCollectedTorch = 108,
    KieronToNoraGotPhone = 109,
    KieronToNoraFirstEvidence = 110,
    phoneTutorialFirstPhoto = 111,
    phoneTutorialSomething = 112,
    NoraBathroomLockedFromInside = 200,
    NoraDiesInAFreezer = 201,
    NoraLookingAtIncinerator = 202,
    NoraLookingAtBloodstains = 203,
    NoraLookingAtSkull = 204,
    NoraReadingManagersEmails = 205,
    NoraOutsideRoark = 307
}