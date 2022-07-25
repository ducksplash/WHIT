using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;

public class fridgedeath : MonoBehaviour
{


    public GameObject FridgeDoor;
    public Volume FridgeVol;
    public Image FridgeDoorImg;
    public Fog fridgeFog;
    public bool FogAvailable;

    void Start()
    {
        if (FridgeVol.profile.TryGet<Fog>(out fridgeFog))
        {
            FogAvailable = true;
        }
    }




    public void OnTriggerEnter(Collider other)
    {

        if (other.name.Contains("Player"))
        {

            StartCoroutine(TrapPlayer(other.gameObject));

        }
    }
    
    public IEnumerator TrapPlayer(GameObject Playa)
    {

        yield return new WaitForSeconds(1f);

        Debug.Log("playa fridgin' yo");
        FridgeDoor.GetComponent<innerDoors>().isLocked = true;
        FridgeDoor.GetComponent<innerDoors>().doorAnimator.SetTrigger("closed");
        FridgeDoor.GetComponent<innerDoors>().doorAnimator.SetTrigger("idle");
        FridgeDoor.GetComponent<innerDoors>().doLockedLights();
        FridgeDoorImg.color = Color.red;


        yield return new WaitForSeconds(1f);

        if (FogAvailable)
        {
            fridgeFog.color.value = Color.cyan;

        }

        StartCoroutine(DoDeath(Playa));


    }




    IEnumerator DoDeath(GameObject theplayer)
    {

        yield return new WaitForSeconds(1f);

        var fakegameobject = new GameObject("FakeObject", typeof(BoxCollider));
        fakegameobject.GetComponent<Collider>().enabled = false;

        var msg = "Ah f***.";

        gameObject.GetComponent<DialogueManager>().NewDialogue("NORA", msg, 5, fakegameobject);

        yield return new WaitForSeconds(2f);

        theplayer.GetComponent<FirstPersonCollision>().CauseDeath("being flash frozen");


    }

}
