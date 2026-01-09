using System.Collections;
using System.Collections.Generic;
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

    public List<Dialogue> Dialogues = new List<Dialogue>();
    private Dictionary<DialogueName, Dialogue> DialogueDict = new Dictionary<DialogueName, Dialogue>();

    private Dictionary<string, string> EregiDict = new Dictionary<string, string>();
    
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
        CreateEregiDictionary();
    }


    private void CreateEregiDictionary()
    {
        // for now we'll do a dumb collection of the replaceables, later we'll try to make it dynamic.

        EregiDict.Add("+phonekey+","Y or P");
        EregiDict.Add("+torchkey+","H or Press R Stick");

    }
    
    
    private void PopulateDialogues()
    {
        foreach (Dialogue Dialogue in Dialogues)
        {
            DialogueDict.TryAdd(Dialogue.DialogueName, Dialogue);
        }
    }


    private void Update()
    {
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

    public async Task CreateDialogue(DialogueName dialogueName, float displaytimer)
    {
        Contacts contact = Contacts.System; 
        string message = "..."; 

        if (DialogueDict.ContainsKey(dialogueName))
        {
            Dialogue selectedDialogue = DialogueDict[dialogueName];

            contact = selectedDialogue.Contact;
            message = selectedDialogue.DialogueText;

            // ✅ Perform replacement only if EregiReplace is true
            if (selectedDialogue.EregiReplace)
            {
                foreach (var kvp in EregiDict)
                {
                    if (!string.IsNullOrEmpty(message))
                        message = message.Replace(kvp.Key, kvp.Value);
                }
            }
        }

        if (contact == Contacts.System)
        {
            if (!GameMaster.Instance.DialogueSeen.Contains(dialogueName))
            {
                messagetimer = displaytimer;
                DialogInProgress = true;
                SystemMessage.text = message;

                await SystemTimer(displaytimer);

                GameMaster.Instance.DialogueSeen.Add(dialogueName);
            }
        }
        else if (contact == Contacts.Nora)
        {
            if (!GameMaster.Instance.DialogueSeen.Contains(dialogueName))
            {
                messagetimer = displaytimer;
                DialogInProgress = true;
                NoraMessage.text = "NORA: " + message;

                await NoraTimer(displaytimer);

                GameMaster.Instance.DialogueSeen.Add(dialogueName);
            }
        }
        else
        {
            if (!GameMaster.Instance.DialogueSeen.Contains(dialogueName))
            {
                messagetimer = displaytimer;
                DialogInProgress = true;
                ContactName.text = contact.ToString();
                ReceivedMessage.text = message;

                await MessageTimer(displaytimer);

                GameMaster.Instance.DialogueSeen.Add(dialogueName);
            }
        }
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


}


public enum DialogueType
{
    Standard,
    Cutscene
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