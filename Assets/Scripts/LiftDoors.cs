using System.Collections;
using UnityEngine;

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
        // No need to check if door is fully open in Update method
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterController>() != null && !isDoorOpen)
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

        if (other.GetComponent<CharacterController>() != null && isDoorOpen)
        {
            door_animator.Play("closed");
            isDoorOpen = false;
        }
    }



}
