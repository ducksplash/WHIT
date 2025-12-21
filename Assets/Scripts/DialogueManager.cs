using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.HighDefinition;


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
            Debug.Log(DialogInProgress);
        }
        else // incoming dialogue is NOT cutscene, queue it
        {
            await Queuer(dialogueName, displaytimer);
            Debug.Log(DialogInProgress);
        }
            
    }

    public Task Queuer(DialogueName dialogueName, float displaytimer)
    {
        Debug.Log("queued");
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
        }
        
        if (contact == Contacts.System)
        {
            if (!GameMaster.DialogueSeen.Contains(dialogueName))
            {
                messagetimer = displaytimer;

                DialogInProgress = true;
                SystemMessage.text = message;

                await SystemTimer(displaytimer); // Wait asynchronously for the timer to complete

                // log me
                GameMaster.DialogueSeen.Add(dialogueName);
            }
            else
            {
                Debug.Log("already seen System");
            }
        }
        if (contact == Contacts.Nora)
        {
            if (!GameMaster.DialogueSeen.Contains(dialogueName))
            {
                messagetimer = displaytimer;

                DialogInProgress = true;
                NoraMessage.text = "NORA: " + message;

                await NoraTimer(displaytimer); // Wait asynchronously for the timer to complete

                // log me
                GameMaster.DialogueSeen.Add(dialogueName);
            }
            else
            {
                Debug.Log("already seen NORA");
            }
        }
        else
        {
            if (!GameMaster.DialogueSeen.Contains(dialogueName))
            {
                messagetimer = displaytimer;

                DialogInProgress = true;
                ContactName.text = contact.ToString();
                ReceivedMessage.text = message;

                await MessageTimer(displaytimer); // Wait asynchronously for the timer to complete

                // log me
                GameMaster.DialogueSeen.Add(dialogueName);
            }
            else
            {
                Debug.Log("already seen Contact");
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
    KieronToNoraBathroom, 
    NoraLookingAtCorkboard, 
    NoraNeedsHerThings,
    NoraNeedsTestEvidence,
    NoraReadyToGo,
    NoraCollectedNotepad,
    NoraCollectedPhone,
    NoraCollectedTorch,
    KieronToNoraGotPhone,
    KieronToNoraFirstEvidence,
    KieronToNoraTookPhoto,
    
    
    // Nora Meaty
    NoraBathroomLockedFromInside,
    NoraDiesInAFreezer, //
    NoraLookingAtIncinerator, //
    NoraLookingAtBloodstains, //
    NoraLookingAtSkull, //
    NoraReadingManagersEmails, //
    
    // Noroark
    NoraOutsideRoark //
}

