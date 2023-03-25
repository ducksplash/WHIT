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


    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.distance <= 5.5f)
                {
                    CallTheLift(hit.transform.name);
                }
            }
        }
    }



    public void CallTheLift(string calledfrom)
    {

        if (!liftismoving)
        {
            if (calledfrom == "X" || calledfrom == currentfloor)
            {
                if (!isDoorOpen)
                {
                    OpenLiftDoors();
                    Debug.Log("lift called from outside [X] or same as current floor");
                } 
            }
            else
            {
                GoToFloor(calledfrom);

            }
        }
    }



    public void OpenLiftDoors()
    {
        inner_door_animator.Play("opened");     
        outer_door_animator.Play("opened");     
        isDoorOpen = true;
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
            inner_door_animator.Play("closed");
            outer_door_animator.Play("closed");
            isDoorOpen = false;
        }
    }




    public void GoToFloor(string floorselected)
    {
        
        //liftismoving = true;
        
        Debug.Log("lift called from inside, "+floorselected+" floor");
        Debug.Log(floorselected);


    }


    private void OnTriggerExit(Collider other)
    {       
        if (other.gameObject.GetComponent<CharacterController>() != null)
        {
            
            StartCoroutine(CloseLiftDoors());

        }
    }



}
