using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LiftDoors : MonoBehaviour
{
    private bool isDoorOpen = false;
    private Animator door_animator;

    private void Start()
    {
        // Get the Animator component
        door_animator = GetComponent<Animator>();        

    }







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
                    Debug.Log(hit.transform.name);
                    if (hit.transform.name.Contains("liftcallbutton"))
                    {
                    Debug.Log("klik");
                        char floorchar = hit.transform.name[hit.transform.name.Length - 1];
                        CallTheLift(floorchar);
                    }
                }
            }
        }
    }












    public void CallTheLift(char calledfrom)
    {
        Debug.Log(calledfrom);


        if (!isDoorOpen)
        {
            door_animator.Play("opened");     
            isDoorOpen = true;
        } 

    }

    private IEnumerator OnTriggerExit(Collider other)
    {       
         Debug.Log("close em");

        // Wait until the animation is finished
        while (door_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        if (isDoorOpen)
        {
            door_animator.Play("closed");
            isDoorOpen = false;
        }
    }



}
