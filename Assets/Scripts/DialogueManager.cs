using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class DialogueManager : MonoBehaviour
{
    
    public GameObject DialogManager;
    private CanvasGroup DialogManagerCanvas;
    public TextMeshProUGUI ContactName;
    public Image ContactPhoto;
    public TextMeshProUGUI ReceivedMessage;
    public TextMeshProUGUI NoraMessage;
    public TextMeshProUGUI SystemMessage;
    public bool DialogInProgress;
    public Image timebar;
    public float messagetimer = 0f;
    public bool currentDialogueIsCutscene;
    private readonly SemaphoreSlim dialogueSaveLock = new(1, 1);

    public List<Dialogue> Dialogues = new List<Dialogue>();
    public List<OSDText> OSDTexts = new List<OSDText>();
    private Dictionary<DialogueName, Dialogue> DialogueDict = new Dictionary<DialogueName, Dialogue>();
    public List<DialogueName> RepeatableDialogues = new List<DialogueName>();
    private Dictionary<OSDTextName, OSDText> OSDTextDict = new Dictionary<OSDTextName, OSDText>();
    
    private Dictionary<string, string> EregiDict = new Dictionary<string, string>();
    
    [Header("History")]
    // Dialog log

    // The main purpose is to prevent duplicates.
    // A secondary use is within the phone, as a message log.
    // The main dictionary is split into NoraSpeak - The player dialogue, and 'Messages' (from others)
    // 

    public List<DialogueName> DialogueSeen = new List<DialogueName>();
    
    public bool queueDropFlag;

    void Start()
    {

        DialogManagerCanvas = DialogManager.GetComponent<CanvasGroup>();
        DialogInProgress = false;
        currentDialogueIsCutscene = false;
        queueDropFlag = false;
        NoraMessage.text = "";
        SystemMessage.text = "";
        
        PopulateDialogues();
        PopulateOSDTexts();
        CreateEregiDictionary();
    }


    private void CreateEregiDictionary()
    {
        // for now we'll do a dumb collection of the replaceables, later we'll try to make it dynamic.
        // we can use new Input Manager to supply input names

        if (GameMaster.Instance.DeviceType.selectedDeviceType == PlayerDeviceType.SteamOS)
        {
            EregiDict.TryAdd("+phonekey+","X");
            EregiDict.TryAdd("+torchkey+","Right Stick Button");
            EregiDict.TryAdd("+camerakey+","A");
            EregiDict.TryAdd("+melee+","R2");
        }
        else
        {
            EregiDict.TryAdd("+phonekey+","P");
            EregiDict.TryAdd("+torchkey+","H");
            EregiDict.TryAdd("+camerakey+","Enter");
            EregiDict.TryAdd("+melee+","Left Click");
        }


    }
    
    
    private void PopulateDialogues()
    {
        foreach (Dialogue Dialogue in Dialogues)
        {
            DialogueDict.TryAdd(Dialogue.DialogueName, Dialogue);
        }
    }

    
    private void PopulateOSDTexts()
    {
        foreach (OSDText osdText in OSDTexts)
        {
            OSDTextDict.TryAdd(osdText.OSDTextName, osdText);
        }
    }


    private void Update()
    {
        if (!DialogInProgress) return;
        
        if (messagetimer > 0)
        { 
            timebar.fillAmount -= 1.0f / messagetimer * Time.deltaTime;
        }
    }

    public async Task NewDialogue(DialogueName dialogueName, float displaytimer, bool isCutSceneDialogue = false)
    {
        // if dialogue NOT already in progress, OR incoming dialogue is cutscene, show cutscene
        if (!DialogInProgress)
        {
            await CreateDialogue(dialogueName, displaytimer);
        }
        else // incoming dialogue is NOT cutscene, queue it
        {
            await Queuer(dialogueName, displaytimer);
        }
            
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

        NewDialogue(dialogueName, displaytimer);
        
        tcs.SetResult(true);
    }

    public async Task CreateDialogue(DialogueName dialogueName, float displaytimer, bool isCutSceneDialogue = false)
    {
        if (DialogueSeen.Contains(dialogueName))
        {
            if (!RepeatableDialogues.Contains(dialogueName)) return;
        }

        Contacts contact = Contacts.System;
        string message = "...";

        if (DialogueDict.ContainsKey(dialogueName))
        {
            Dialogue selectedDialogue = DialogueDict[dialogueName];
            contact = selectedDialogue.Contact;
            message = selectedDialogue.DialogueText;

            if (selectedDialogue.EregiReplace)
            {
                message = GetReplacedString(message);
            }
        }

        messagetimer = displaytimer;
        DialogInProgress = true;

        if (contact == Contacts.System)
        {
            SystemMessage.text = message;
            await SystemTimer(displaytimer);
        }
        else if (contact == Contacts.Nora)
        {
            NoraMessage.text = "NORA: " + message;
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

        dialogueSaveLock.Release(); // if using lock - otherwise remove
    }



    public string GetReplacedString(string message)
    {
        if (string.IsNullOrEmpty(message))
            return message;

        foreach (var kvp in EregiDict)
        {
            message = message.Replace(kvp.Key, kvp.Value);
        }

        Debug.Log("Replaced message: " + message);
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

        await Task.Delay((int)(timevalue * 1000)); // Delay asynchronously for the specified time in milliseconds

        StartCoroutine(Fader(DialogManagerCanvas, 0));
        ContactName.text = "";
        ReceivedMessage.text = "";
        await Task.Delay(500); // Delay asynchronously for 500 milliseconds
        DialogInProgress = false;
    }

    public async Task NoraTimer(float timevalue)
    {
        await Task.Delay((int)(timevalue * 1000)); // Delay asynchronously for the specified time in milliseconds

        NoraMessage.text = "";
        await Task.Delay(500); // Delay asynchronously for 500 milliseconds
        DialogInProgress = false;
    }

    public async Task SystemTimer(float timevalue)
    {
        await Task.Delay((int)(timevalue * 1000)); // Delay asynchronously for the specified time in milliseconds
        
        SystemMessage.text = "";
        await Task.Delay(500); // Delay asynchronously for 500 milliseconds
        DialogInProgress = false;
    }


    public string RetrieveOSDText(OSDTextName requestedOSDText)
    {
        string messageString = ".";
        
        
        if (OSDTextDict.ContainsKey(requestedOSDText))
        {
            OSDText selectedOSDText = OSDTextDict[requestedOSDText];
            messageString = selectedOSDText.OSDTextString;
            
            
            if (selectedOSDText.EregiReplace)
            {
                foreach (var kvp in EregiDict)
                {
                    if (!string.IsNullOrEmpty(messageString)) messageString = messageString.Replace(kvp.Key, kvp.Value);
                }
            }
        }

        return messageString;
    }
    
    
    
        
    
    public void SaveWhatYouSee()
    {
        Debug.Log("save what you see");
        
        StoredPrefs.Instance.SetCollection("DialogueSeen", DialogueSeen, CollectionType.list);
        StoredPrefs.Instance.Save();
        //StoredPrefs.SetCollection("CutSceneSeen", CutSceneSeen, CollectionType.dictionary);
        
    }
    public void LoadWhatYouSee()
    {
        Debug.Log("LoadWhatYouSee");
        
        DialogueSeen = StoredPrefs.Instance.GetCollection<List<DialogueName>>("DialogueSeen");
        //CutSceneSeen = StoredPrefs.GetCollection<Dictionary<string,string>>("CutSceneSeen");
        
        Debug.Log("LoadedWhatYouSee");
    }

}


public enum DialogueType
{
    Standard,
    Cutscene
}

public enum OSDTextName
{
    TakePhoto = 101,
    SavedPhoto = 102
}

public enum DialogueName
{
    None,
    // Nora's Flat
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
    
    
    // Nora Meaty
    NoraBathroomLockedFromInside = 200,
    NoraDiesInAFreezer = 201, //
    NoraLookingAtIncinerator = 202, //
    NoraLookingAtBloodstains = 203, //
    NoraLookingAtSkull = 204, //
    NoraReadingManagersEmails = 205, //
    
    // Noroark
    NoraOutsideRoark = 307 //
}

#if UNITY_EDITOR


[CustomEditor(typeof(DialogueManager))]
public class DialogueManagerEditor : Editor
{
    private DialogueName selectedDialogue = DialogueName.None;
    private DialogueType dialogueType = DialogueType.Standard;
    private Languages language = Languages.EN;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.Label("Test Dialogue", EditorStyles.boldLabel);

        selectedDialogue = (DialogueName)EditorGUILayout.EnumPopup("Dialogue", selectedDialogue);
        language = (Languages)EditorGUILayout.EnumPopup("Language", language); // Stub, not implemented

        if (GUILayout.Button("Play Dialogue"))
        {
            DialogueManager manager = (DialogueManager)target;
            manager.NewDialogue(selectedDialogue, 5f, dialogueType == DialogueType.Cutscene);
        }
    }
}
#endif