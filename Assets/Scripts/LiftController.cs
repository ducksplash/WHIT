using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LiftController : MonoBehaviour
{
    private bool isDoorOpen = false;
    public Animator inner_door_animator;
    public Animator outer_door_animator;
    public string currentfloor = "G";
    public bool liftismoving;
    public bool doorsmoving;

    public Transform FloorStopG;
    public Transform FloorStop1;
    public Transform FloorStopB1;
    public Transform FloorStopB2;
    public Transform CARRIAGE;

    public GameObject Player;


    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.name == "X" || hit.transform.name == "G" || hit.transform.name == "1" || hit.transform.name == "B1" || hit.transform.name == "B2")
                {
                    if (hit.distance <= 2.5f)
                    {
                        if (!liftismoving && !doorsmoving)
                        {
                            CallTheLift(hit.transform.name);
                        }
                    }
                }

            }
        }
    }



    public void CallTheLift(string calledfrom)
    {


        if (calledfrom == "X")
        {
            if (!isDoorOpen)
            {
                OpenLiftDoors();
            } 
        }
        else
        {
            StartCoroutine(GoToFloor(calledfrom));
        }
        
    }



    public void OpenLiftDoors()
    {
        doorsmoving = true;
        inner_door_animator.Play("opened");     
        outer_door_animator.Play("opened");     
        isDoorOpen = true;
        doorsmoving = false;
    }



    public IEnumerator CloseLiftDoors()
    {

        Debug.Log("close em");
        // Wait until the animation is finished
        while (inner_door_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            // Do nothing, just wait
            yield return null;
        }

        if (isDoorOpen)
        {
            doorsmoving = true;
            inner_door_animator.Play("closed");
            outer_door_animator.Play("closed");
            isDoorOpen = false;
            doorsmoving = false;
        }
    }






    public IEnumerator GoToFloor(string floorselected)
    {

        GameMaster.FROZEN = true;
        
        // close doors
        yield return StartCoroutine(CloseLiftDoors());

        // move lift
        liftismoving = true;



        Player.transform.SetParent(CARRIAGE);
        Vector3 startPosition = CARRIAGE.position;
        Vector3 targetPos = CARRIAGE.position;
        
        if (floorselected == "G")
        {
            targetPos.y = FloorStopG.position.y;
        }
        else if (floorselected == "1")
        {
            targetPos.y = FloorStop1.position.y;
        }
        else if (floorselected == "B1")
        {
            targetPos.y = FloorStopB1.position.y;
        }
        else if (floorselected == "B2")
        {
            targetPos.y = FloorStopB2.position.y;
        }

        float startTime = Time.time;
        float duration = 5.0f;

        while (Time.time - startTime < duration)
        {
            float timeFraction = (Time.time - startTime) / duration;
            CARRIAGE.position = Vector3.Lerp(startPosition, targetPos, timeFraction);
            yield return null;
        }

        CARRIAGE.position = targetPos;

        currentfloor = floorselected;

        Debug.Log("lift called from inside, "+floorselected+" floor");            
        Player.transform.SetParent(null);

        liftismoving = false;

        // open doors
        OpenLiftDoors();


        GameMaster.FROZEN = false;

    }





    private void OnTriggerExit(Collider other)
    {       
        if (other.gameObject.GetComponent<CharacterController>() != null)
        {
            
            StartCoroutine(CloseLiftDoors());

        }
    }



}
