using System.Collections;
using UnityEngine;

public class LiftController : MonoBehaviour
{
    private bool isDoorOpen = false;
    private bool isLiftMoving = false;
    private bool areDoorsMoving = false;

    public Animator innerDoorAnimator;
    public Animator outerDoorAnimator;

    public Transform floorStopG;
    public Transform floorStop1;
    public Transform floorStopB1;
    public Transform floorStopB2;
    public Transform carriage;
    public bool controlsenabled = true;

    public GameObject player;

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 2.5f) &&
                (hit.transform.name == "X" || hit.transform.name == "G" || hit.transform.name == "1" ||
                 hit.transform.name == "B1" || hit.transform.name == "B2"))
                {
                    if (!isLiftMoving && !areDoorsMoving)
                    {
                        CallTheLift(hit.transform.name);
                    }
                }
        }
    }

    private void CallTheLift(string calledFrom)
    {
        if (calledFrom == "X")
        {
            if (!isDoorOpen)
            {
                OpenLiftDoors();
            }
        }
        else
        {
            StartCoroutine(GoToFloor(calledFrom));
        }
    }

    private void OpenLiftDoors()
    {
        areDoorsMoving = true;
        innerDoorAnimator.Play("opened");
        outerDoorAnimator.Play("opened");
        isDoorOpen = true;
        areDoorsMoving = false;
    }

    private IEnumerator CloseLiftDoors()
    {
        while (innerDoorAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        if (isDoorOpen)
        {
            areDoorsMoving = true;
            innerDoorAnimator.Play("closed");
            outerDoorAnimator.Play("closed");
            isDoorOpen = false;
            areDoorsMoving = false;
        }
    }

    private IEnumerator GoToFloor(string floorSelected)
    {
        // wait for doors to close...
        StartCoroutine(CloseLiftDoors());
        
        controlsenabled = false;

        yield return new WaitForSeconds(3f);

        // ...then move the lift

        // set lift to moving
        isLiftMoving = true;

        // parent player to lift carriage
        player.transform.SetParent(carriage);

        // set destination
        Vector3 startPosition = carriage.position;
        Vector3 targetPosition = startPosition;
        switch (floorSelected)
        {
            case "G":
                targetPosition.y = floorStopG.position.y;
                break;
            case "1":
                targetPosition.y = floorStop1.position.y;
                break;
            case "B1":
                targetPosition.y = floorStopB1.position.y;
                break;
            case "B2":
                targetPosition.y = floorStopB2.position.y;
                break;
        }

        // move over time
        float startTime = Time.time;
        float duration = 5.0f;

        while (carriage.position != targetPosition)
        {
            float timeFraction = Mathf.Clamp01((Time.time - startTime) / duration);
            carriage.position = Vector3.Lerp(startPosition, targetPosition, timeFraction);
            yield return null;
        }

        // de-parent player from carriage
        player.transform.SetParent(null);

        // set lift to not moving
        isLiftMoving = false;

        // open doors
        OpenLiftDoors();
        controlsenabled = true;

        yield break;
    }



    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<CharacterController>() != null)
        {
            StartCoroutine(CloseLiftDoors());
        }
    }
}
