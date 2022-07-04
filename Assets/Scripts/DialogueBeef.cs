using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;



public class DialogueBeef : MonoBehaviour
{

    [Header("Message from:")]
    public string ContactName;
    [Header("Message contents:")]
    [TextArea(5, 10)]
    public string MessageBody;

    [Header("Display time in seconds")]
    public float DisplayTimer = 5.0f;


    [Header("Debug (ignore)")]
    public float forhowlong;



    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3)
        {

            gameObject.GetComponent<Collider>().enabled = false;


            var TheCallingObject = gameObject;
            forhowlong = DisplayTimer;

            other.gameObject.GetComponent<DialogueManager>().NewDialogue(ContactName, MessageBody, DisplayTimer, TheCallingObject);
           

        }
    }






}
