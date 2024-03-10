using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class innerDoors : MonoBehaviour
{
    public GameObject thisDoor;
    private string thisDoorName;
    public GameObject thisDoorHinge;
    public string doorLockTag;
    public string doorUnlockTag;
    public Animator doorAnimator;
    public bool isOpen;
    public bool isLocked;
    private Collider theDoorCollider;

    private bool PlayerClicked;
    private TravelCompanion travelCompanion;
    private float clickCooldown = 0.5f; // Cooldown duration in seconds
    private bool clickAvailable = true;
    

    void Start()
    {
        doorAnimator = thisDoorHinge.GetComponent<Animator>();
        thisDoorName = thisDoor.name;
        PlayerClicked = false;

        // Get reference to TravelCompanion instance
        travelCompanion = FindObjectOfType<TravelCompanion>();
        if (travelCompanion == null)
        {
            Debug.LogError("TravelCompanion instance not found!");
        }
    }

    public void doLockedLights()
    {
        // Implementation of doLockedLights method remains unchanged
    }

    IEnumerator DisableColliderMomentarily(float aWeeSecond, Collider theCollider)
    {
        theCollider.enabled = false;
        yield return new WaitForSeconds(aWeeSecond);
        PlayerClicked = false;
        theCollider.enabled = true;
    }

    void Update()
    {
        // Check if click is available
        if (clickAvailable)
        {
            if (Input.GetMouseButtonUp(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 4f))
                {
                    if (hit.transform.gameObject.CompareTag("ExteriorDoor"))
                    {
                        Debug.Log("Travel!");
                        // Launch companion if the player clicks on an exterior door
                        if (travelCompanion != null)
                        {
                            travelCompanion.LaunchCompanion();
                        }
                    }
                    else
                    {
                        if (!isLocked && hit.transform.name.Equals(thisDoorName))
                        {
                            DoDoor(hit);
                        }
                    }
                }

                // Start cooldown
                StartCoroutine(ClickCooldown());
            }
        }
    }

    IEnumerator ClickCooldown()
    {
        // Disable click availability
        clickAvailable = false;
        
        // Wait for cooldown duration
        yield return new WaitForSeconds(clickCooldown);
        
        // Enable click availability
        clickAvailable = true;
    }

    public void DoDoor(RaycastHit hit)
    {
        var thisDoorCollider = hit.transform.GetComponent<Collider>();

        if (!isOpen)
        {
            doorAnimator.SetTrigger("opened");
            StartCoroutine(DisableColliderMomentarily(0.5f, thisDoorCollider));
            isOpen = true;
        }
        else
        {
            doorAnimator.SetTrigger("closed");
            isOpen = false;
            StartCoroutine(DisableColliderMomentarily(0.5f, thisDoorCollider));
            doorAnimator.SetTrigger("idle");
        }
    }
}
