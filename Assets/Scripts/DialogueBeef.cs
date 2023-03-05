using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;



public class DialogueBeef : MonoBehaviour
{



    [Header("Dev Placeholder Cube")]
    public GameObject ColliderCube;

    [Header("Message from:")]
    public string ContactName;
    [Header("Message contents:")]
    [TextArea(5, 10)]
    public string MessageBody;

    [Header("Display time in seconds")]
    public float DisplayTimer = 5.0f;


    [Header("Nora Reply?")]
    public bool Noraply;

    [Header("Nora's Reply:")]
    [TextArea(5, 10)]
    public string NoraplyBody;

    [Header("Delay before Nora's reply?")]
    public float Noradelay = 5.0f;


    [Header("Debug (ignore)")]
    public float forhowlong;




void Start() 
    {

        ColliderCube.SetActive(false);

    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3)
        {

            gameObject.GetComponent<Collider>().enabled = false;


            var TheCallingObject = gameObject;
            forhowlong = DisplayTimer;

            other.gameObject.GetComponent<DialogueManager>().NewDialogue(ContactName, MessageBody, DisplayTimer, TheCallingObject);
           

            if (Noraply)
            {

                StartCoroutine(Norasponse(other.gameObject));

            }



        }
    }




    public IEnumerator Norasponse(GameObject ThePlayer)
    {
        yield return new WaitForSeconds(Noradelay);

        ThePlayer.gameObject.GetComponent<DialogueManager>().NewDialogue("NORA", NoraplyBody, 4, gameObject);


    }



}
