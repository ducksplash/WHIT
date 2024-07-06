using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : Singleton<DialogueManager>
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

    public bool queueDropFlag;

    void Start()
    {
        DialogManagerCanvas = DialogManager.GetComponent<CanvasGroup>();
        DialogInProgress = false;
        currentDialogueIsCutscene = false;
        queueDropFlag = false;
        NoraMessage.text = "";
        SystemMessage.text = "";
    }



    private void Update()
    {
        if (messagetimer > 0)
        { 
            timebar.fillAmount -= 1.0f / messagetimer * Time.deltaTime;
        }
    }

    public async Task NewDialogue(string Contact, string message, float displaytimer, bool isCutSceneDialogue = false)
    {
        // NOT currently cutscene dialogue
        if (!currentDialogueIsCutscene)
        {
            // if dialogue NOT already in progress, OR incoming dialogue is cutscene, show cutscene
            if (!DialogInProgress || isCutSceneDialogue)
            {
                await CreateDialogue(Contact, message, displaytimer);
                Debug.Log(DialogInProgress);
            }
            else // incoming dialogue is NOT cutscene, queue it
            {
                await Queuer(Contact, message, displaytimer);
                Debug.Log(DialogInProgress);
            }
        }
        else // IS currently cutscene dialogue
        {
            // if incoming dialogue is not cutscene, show dialogue
            if (!isCutSceneDialogue)
            {
                await CreateDialogue(Contact, message, displaytimer);
                Debug.Log(DialogInProgress);
            }
            else // incoming dialogue IS cutscene, queue it.
            {
                await Queuer(Contact, message, displaytimer);
                Debug.Log(DialogInProgress);
            }
        }
    }

    public Task Queuer(string Contact, string message, float displaytimer)
    {
        Debug.Log("queued");
        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(QueuerCoroutine(Contact, message, displaytimer, tcs));
        return tcs.Task;
    }

    private IEnumerator QueuerCoroutine(string Contact, string message, float displaytimer, TaskCompletionSource<bool> tcs)
    {
        yield return new WaitWhile(() => DialogInProgress);
        if (!queueDropFlag)
        {
            NewDialogue(Contact, message, displaytimer);
        }
        else
        {
            // record silently
            GameMaster.DialogueSeen.Add(message, Contact);
            queueDropFlag = false;
        }

        tcs.SetResult(true);
    }

    public async Task CreateDialogue(string Contact, string message, float displaytimer)
    {
        if (Contact.Equals(Contacts.System.ToString()))
        {
            if (!GameMaster.DialogueSeen.ContainsKey(message))
            {
                messagetimer = displaytimer;

                DialogInProgress = true;
                SystemMessage.text = message;

                await SystemTimer(displaytimer); // Wait asynchronously for the timer to complete

                // log me
                GameMaster.DialogueSeen.Add(message, Contact);
            }
        }
        else if (Contact.Equals(Contacts.Nora.ToString()))
        {
            if (!GameMaster.DialogueSeen.ContainsKey(message))
            {
                messagetimer = displaytimer;

                DialogInProgress = true;
                NoraMessage.text = "NORA: " + message;

                await NoraTimer(displaytimer); // Wait asynchronously for the timer to complete

                // log me
                GameMaster.DialogueSeen.Add(message, Contact);
            }
        }
        else
        {
            if (!GameMaster.DialogueSeen.ContainsKey(message))
            {
                messagetimer = displaytimer;

                DialogInProgress = true;
                ContactName.text = Contact;
                ReceivedMessage.text = message;

                await MessageTimer(displaytimer); // Wait asynchronously for the timer to complete

                // log me
                GameMaster.DialogueSeen.Add(message, Contact);
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
