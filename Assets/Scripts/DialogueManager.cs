using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;



public class DialogueManager : MonoBehaviour
{
    public GameObject DialogManager;
    public CanvasGroup DialogManagerCanvas;
    public TextMeshProUGUI ContactName;
    public Image ContactPhoto;
    public TextMeshProUGUI ReceivedMessage;
    public TextMeshProUGUI NoraMessage;
    public TextMeshProUGUI SystemMessage;
    public bool DialogInProgress;
    public Image timebar;
    public float messagetimer = 0f;




    void Start()
    {
        DialogManagerCanvas = DialogManager.GetComponent<CanvasGroup>();

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



    public void NewDialogue(string Contact, string message, float displaytimer, GameObject CallingObject)
    {
        if (!DialogInProgress)
        {
            CreateDialogue(Contact, message, displaytimer, CallingObject);
            Debug.Log(DialogInProgress);
        }
        else
        {
            StartCoroutine(Queuer(Contact, message, displaytimer, CallingObject));
            Debug.Log(DialogInProgress);
        }
    }




    public IEnumerator Queuer(string Contact, string message, float displaytimer, GameObject CallingObject)
    {
        Debug.Log("queued");
        yield return new WaitWhile(() => DialogInProgress);
        NewDialogue(Contact, message, displaytimer, CallingObject);
    }





    public void CreateDialogue(string Contact, string message, float displaytimer, GameObject CallingObject)
    {

        if (Contact == "SYSTEM")
        {
            if (!GameMaster.DialogueSeen.ContainsKey(message))
            {

                messagetimer = displaytimer;

                DialogInProgress = true;
                SystemMessage.text = message;

                StartCoroutine(SystemTimer(displaytimer, CallingObject));

                // log me
                GameMaster.DialogueSeen.Add(message, Contact);
            }

        }
        else if (Contact == "NORA")
        {
            if (!GameMaster.DialogueSeen.ContainsKey(message))
            {

                messagetimer = displaytimer;

                DialogInProgress = true;
                NoraMessage.text = "NORA: " + message;

                StartCoroutine(NoraTimer(displaytimer, CallingObject));

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
                StartCoroutine(MessageTimer(displaytimer, CallingObject));

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







    public IEnumerator MessageTimer(float timevalue, GameObject CalledBy)
    {
        timebar.fillAmount = 1.0f;
        StartCoroutine(Fader(DialogManagerCanvas, 1));

        yield return new WaitForSeconds(timevalue);

        StartCoroutine(Fader(DialogManagerCanvas, 0));
        ContactName.text = "";
        ReceivedMessage.text = "";
        yield return new WaitForSeconds(0.5f);
        DialogInProgress = false;
        Destroy(CalledBy);
    }




    public IEnumerator NoraTimer(float timevalue, GameObject CalledBy)
    {

        yield return new WaitForSeconds(timevalue);

        if (CalledBy != null)
        {
            CalledBy.GetComponent<Collider>().enabled = true;
        }

        NoraMessage.text = "";
        yield return new WaitForSeconds(0.5f);
        DialogInProgress = false;
    }




    public IEnumerator SystemTimer(float timevalue, GameObject CalledBy)
    {

        yield return new WaitForSeconds(timevalue);

        if (CalledBy != null)
        {
            CalledBy.GetComponent<Collider>().enabled = true;
        }

        SystemMessage.text = "";
        yield return new WaitForSeconds(0.5f);
        DialogInProgress = false;        

    }



}
