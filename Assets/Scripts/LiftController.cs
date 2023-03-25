using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LiftController : MonoBehaviour
{
    private bool isDoorOpen = false;
    public Animator door_animator;
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
        door_animator.Play("opened");     
        isDoorOpen = true;
    }




    public void GoToFloor(string floorselected)
    {
        
        //liftismoving = true;
        
        Debug.Log("lift called from inside, "+floorselected+" floor");
        Debug.Log(floorselected);


    }


    private IEnumerator OnTriggerExit(Collider other)
    {       
        if (other.gameObject.GetComponent<CharacterController>() != null)
        {
            Debug.Log("close em");
            // Wait until the animation is finished
            while (door_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            {
                // Do nothing, just wait
                yield return null;
            }

            if (isDoorOpen)
            {
                door_animator.Play("closed");
                isDoorOpen = false;
            }
        }
    }



}
