using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;



public class DialogueBeef : MonoBehaviour
{

    public bool AlreadyTriggered;
    public float forhowlong;


    void Start()
    {

    }









    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3 && !AlreadyTriggered)
        {

            AlreadyTriggered = true;

            // are you ready?

            if (gameObject.name.ToLower().Contains("areyouready".ToLower()))
            {
                var TheCallingObject = gameObject;

                var TheContact = "Kieron";
                var TheMessage = "Before you head out, are you sure you've everything you need?";
                var Timer = 5;

                forhowlong = Timer;

                other.gameObject.GetComponent<DialogueManager>().NewDialogue(TheContact, TheMessage, Timer, TheCallingObject);
            }





            // Manager's office: turn on the power

            if (gameObject.name.ToLower().Contains("managersoffice".ToLower()))
            {
                var TheCallingObject = gameObject;

                var TheContact = "Kieron";
                var TheMessage = "This looks like the manager's office.  That computer probably controls the doors, but you'll need to get down to the basement and turn on the power first.";
                var Timer = 10;

                forhowlong = Timer;

                other.gameObject.GetComponent<DialogueManager>().NewDialogue(TheContact, TheMessage, Timer, TheCallingObject);
            }





            // by the power room door

            if (gameObject.name.ToLower().Contains("powerroom".ToLower()))
            {
                var TheCallingObject = gameObject;

                var TheContact = "Kieron";
                var TheMessage = "The power room; the door seems to be locked... I don't think there's gonna be a key around here, maybe try and bash it with your torch?  I know it's a long shot.";
                var Timer = 10;


                forhowlong = Timer;


                other.gameObject.GetComponent<DialogueManager>().NewDialogue(TheContact, TheMessage, Timer, TheCallingObject);
            }

            StartCoroutine(UnTrigger());

        }
    }

    IEnumerator UnTrigger()
    {

        var TimeToWait = forhowlong + 0.5f;

        yield return new WaitForSeconds(TimeToWait);

        AlreadyTriggered = false;


    }




}
