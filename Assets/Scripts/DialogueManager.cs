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
    public bool DialogInProgress;
    public Image timebar;
    public float messagetimer = 0f;
    void Start()
    {
        DialogManagerCanvas = DialogManager.GetComponent<CanvasGroup>();

        NoraMessage.text = "";
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
        CreateDialogue(Contact, message, displaytimer, CallingObject);
    }


    public IEnumerator Queuer(string Contact, string message, float displaytimer, GameObject CallingObject)
    {
        yield return new WaitForSeconds(0.5f);
        NewDialogue(Contact, message, displaytimer, CallingObject);
    }



    public void CreateDialogue(string Contact, string message, float displaytimer, GameObject CallingObject)
    {
        if (DialogInProgress)
        {
            StopCoroutine(NoraTimer(displaytimer, CallingObject));
            StopCoroutine(MessageTimer(displaytimer, CallingObject));
        }

        if (Contact == "NORA")
        {
            DialogInProgress = true;
            messagetimer = displaytimer;

            NoraMessage.text = "NORA: " + message;

            StartCoroutine(NoraTimer(displaytimer, CallingObject));
        }
        else
        {
            DialogInProgress = true;
            messagetimer = displaytimer;

            ContactName.text = Contact;
            ReceivedMessage.text = message;
            StartCoroutine(MessageTimer(displaytimer, CallingObject));
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



}
