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
    public TextMeshProUGUI ReceivedMessage;

    public bool DialogInProgress;
    public Image timebar;
    public float messagetimer = 0f;


    void Start()
    {
        DialogManagerCanvas = DialogManager.GetComponent<CanvasGroup>();

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
        DialogInProgress = true;
        messagetimer = displaytimer;

        StartCoroutine(Fader(DialogManagerCanvas, 1));


        ContactName.text = Contact;
        ReceivedMessage.text = message;

        StartCoroutine(MessageTimer(displaytimer, CallingObject));


    }



    public IEnumerator Fader(CanvasGroup ThisCanvas, int direction)
    {
        var counter = 9;

        while (counter > 0)
        {

            yield return new WaitForSeconds(0.05f);

            if (direction == 0)
            {
                ThisCanvas.alpha -= 0.1f;
            }
            else
            {
                ThisCanvas.alpha += 0.1f;
            }

            counter--;
        }

    }




    public IEnumerator MessageTimer(float timevalue, GameObject CalledBy)
    {
        yield return new WaitForSeconds(timevalue);

        StartCoroutine(Fader(DialogManagerCanvas, 0));
        ContactName.text = "";
        ReceivedMessage.text = "";
        yield return new WaitForSeconds(0.5f);
        DialogInProgress = false;
        Destroy(CalledBy);
    }



}
