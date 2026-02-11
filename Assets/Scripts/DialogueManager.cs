using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
    private readonly SemaphoreSlim dialogueSaveLock = new(1, 1);

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
    }

    private void Update()
    {
        if (!DialogInProgress) return;

        if (messagetimer > 0)
            timebar.fillAmount -= 1.0f / messagetimer * Time.deltaTime;
    }

    // dialogueName, displayTimer, type, cutsceneDuration, cutscenePanTime, cutsceneTarget
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
        if (DialogueSeen.Contains(dialogueName) && !RepeatableDialogues.Contains(dialogueName))
            return;

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

        // Held cutscene dialogue: display like normal, but don't auto-clear.
        if (holdUntilAdvance)
        {
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
                StartCoroutine(Fader(DialogManagerCanvas, 1));
                ContactName.text = contact.ToString();
                ReceivedMessage.text = message;
            }

            DialogueSeen.Add(dialogueName);
            SaveWhatYouSee();
            dialogueSaveLock.Release();
            return;
        }

        // Normal timed dialogue (unchanged)
        messagetimer = displaytimer;
        DialogInProgress = true;

        if (contact == Contacts.System)
        {
            SystemMessage.text = message;
            await SystemTimer(displaytimer);
        }
        else if (contact == Contacts.Nora)
        {
            NoraMessage.text = message;
            await NoraTimer(displaytimer);
        }
        else
        {
            ContactName.text = contact.ToString();
            ReceivedMessage.text = message;
            await MessageTimer(displaytimer);
        }

        DialogueSeen.Add(dialogueName);
        SaveWhatYouSee();
        dialogueSaveLock.Release();
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

        if (UInstance.Instance != null)
            StartCoroutine(UInstance.Instance.FadeInCutsceneBars(cutscenePanTime));

        yield return new WaitForSeconds(1f);

        // dialogue displayed as normal, but held until player advances
        _ = CreateDialogue(dialogueName, dialogueDisplayTimer, holdUntilAdvance: true);

        // Zoom timings (UNCHANGED)
        float zoomTime = cutsceneDuration * 0.33f;
        float unzoomTime = cutsceneDuration * 0.33f;
        float holdTime = cutsceneDuration - zoomTime - unzoomTime;

        if (cameraZoom != null)
            cameraZoom.enabled = false;

        // IMPORTANT FIX: we must WAIT for the zoom sequence (which includes the player-advance gate)
        Coroutine zoomCo = StartCoroutine(CutsceneZoomSequence(zoomTime, holdTime, unzoomTime));

        // Rotate camera toward target over duration (UNCHANGED)
        while (elapsedCutsceneTime < cutsceneDuration)
        {
            Vector3 targetDirection = targetObject.transform.position - mainCamera.transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            mainCamera.transform.rotation = Quaternion.Lerp(
                mainCamera.transform.rotation,
                targetRotation,
                cutscenePanTime * Time.smoothDeltaTime
            );

            elapsedCutsceneTime += Time.smoothDeltaTime;
            yield return new WaitForEndOfFrame();
        }

        // Snap player look (UNCHANGED)
        Vector3 dir = (targetObject.transform.position - mainCamera.transform.position).normalized;
        float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        float pitch = Mathf.Asin(dir.y) * Mathf.Rad2Deg;
        Player.Instance.FirstPersonLook.SetPlayerRotation(new Vector2(yaw, pitch));

        // IMPORTANT FIX: ensure cutscene only "finishes" after zoom-out / cleanup is done
        yield return zoomCo;

        CutsceneInProgress = false;
        SaveWhatYouSee();
        tcs.SetResult(true);
    }

    private IEnumerator CutsceneZoomSequence(float zoomTime, float holdTime, float unzoomTime)
    {
        float t = 0f;

        // ZOOM IN (unchanged)
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(zoomTime, 0.0001f);
            mainCamera.fieldOfView = Mathf.Lerp(originalFieldOfView, targetFieldOfView, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        mainCamera.fieldOfView = targetFieldOfView;

        // CHANGE: wait for player press instead of holdTime
        yield return StartCoroutine(WaitForCutsceneAdvance());

        // Clear dialogue phase happens only when pressed
        ClearHeldCutsceneDialogue();

        // ZOOM OUT (unchanged)
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

        GameMaster.Instance.PLAYERBUSY = false;
    }

    private void ClearHeldCutsceneDialogue()
    {
        if (!DialogInProgress) return;

        if (_currentCutsceneContact != Contacts.System && _currentCutsceneContact != Contacts.Nora)
        {
            StartCoroutine(Fader(DialogManagerCanvas, 0));
            ContactName.text = "";
            ReceivedMessage.text = "";
        }
        else if (_currentCutsceneContact == Contacts.Nora)
        {
            NoraMessage.text = "";
        }
        else
        {
            SystemMessage.text = "";
        }

        DialogInProgress = false;
        messagetimer = 0f;
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

    public async Task MessageTimer(float timevalue)
    {
        timebar.fillAmount = 1.0f;
        StartCoroutine(Fader(DialogManagerCanvas, 1));

        await Task.Delay((int)(timevalue * 1000));

        StartCoroutine(Fader(DialogManagerCanvas, 0));
        ContactName.text = "";
        ReceivedMessage.text = "";
        await Task.Delay(500);
        DialogInProgress = false;
    }

    public async Task NoraTimer(float timevalue)
    {
        await Task.Delay((int)(timevalue * 1000));
        NoraMessage.text = "";
        await Task.Delay(500);
        DialogInProgress = false;
    }

    public async Task SystemTimer(float timevalue)
    {
        await Task.Delay((int)(timevalue * 1000));
        SystemMessage.text = "";
        await Task.Delay(500);
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
        StoredPrefs.Instance.SetCollection("DialogueSeen", DialogueSeen, CollectionType.list);
        StoredPrefs.Instance.Save();

        StoredPrefs.Instance.SetCollection("CutSceneSeen", CutSceneSeen, CollectionType.dictionary);
        StoredPrefs.Instance.Save();
    }

    public void LoadWhatYouSee()
    {
        DialogueSeen = StoredPrefs.Instance.GetCollection<List<DialogueName>>("DialogueSeen");
        CutSceneSeen = StoredPrefs.Instance.GetCollection<Dictionary<string, string>>("CutSceneSeen");
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
