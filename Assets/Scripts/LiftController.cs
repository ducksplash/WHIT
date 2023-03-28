using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


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
    
    public float liftduration = 7.0f;

    public string currentfloor = "G";
    // display floor name/num

    public TextMeshProUGUI floorTextOutsideG;
    public TextMeshProUGUI floorTextOutside1;
    public TextMeshProUGUI floorTextOutsideB1;
    //public TextMeshProUGUI floorTextOutsideB2;
    public TextMeshProUGUI floorTextInside;



    public GameObject player;


    private void Start()
    {
        floorTextInside.text = currentfloor;
        floorTextOutsideG.text = currentfloor;
        floorTextOutside1.text = currentfloor;
        floorTextOutsideB1.text = currentfloor;
        //floorTextOutsideB2.text = currentfloor;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 2.5f) &&
                (hit.transform.name.Contains("X") || hit.transform.name == "G" || hit.transform.name == "1" ||
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
        // if called from outside
        if (calledFrom == "X1" || calledFrom == "XG" || calledFrom == "XB1" || calledFrom == "XB2")
        {
            if (!isDoorOpen)
            {

                if (calledFrom == "X"+currentfloor)
                {
                    OpenLiftDoors();
                    Debug.Log("same floor");
                }
                else
                {
                    StartCoroutine(CallToFloor(calledFrom));
                    Debug.Log("diff floor");
                }


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




    private IEnumerator CallToFloor(string floorSelected)
    {
        Debug.Log("called: "+floorSelected);
        Vector3 startPosition = carriage.position;
        Vector3 targetPosition = startPosition;
        switch (floorSelected)
        {
            case "XG":
                targetPosition.y = floorStopG.position.y;
                break;
            case "X1":
                targetPosition.y = floorStop1.position.y;
                break;
            case "XB1":
                targetPosition.y = floorStopB1.position.y;
                break;
            case "XB2":
                targetPosition.y = floorStopB2.position.y;
                break;
        }

        // move over time
        float startTime = Time.time;

        
        floorTextOutside1.text = GetFloorDirection(currentfloor,floorSelected.Substring(1));
        floorTextOutsideG.text = GetFloorDirection(currentfloor,floorSelected.Substring(1));
        floorTextOutsideB1.text = GetFloorDirection(currentfloor,floorSelected.Substring(1));

        while (carriage.position != targetPosition)
        {
            float timeFraction = Mathf.Clamp01((Time.time - startTime) / liftduration);
            carriage.position = Vector3.Lerp(startPosition, targetPosition, timeFraction);
            yield return null;
        }

        OpenLiftDoors();

        floorTextInside.text = floorSelected.Substring(1);
        floorTextOutside1.text = floorSelected.Substring(1);
        floorTextOutsideG.text = floorSelected.Substring(1);   
        floorTextOutsideB1.text = floorSelected.Substring(1);        
        currentfloor = floorSelected.Substring(1);
    }







    private IEnumerator GoToFloor(string floorSelected)
    {
        // disable controls
        controlsenabled = false;

        // if doors open then close



        if (isDoorOpen)
        {
            StartCoroutine(CloseLiftDoors());
            yield return new WaitForSeconds(3f);
        }

        floorTextInside.text = GetFloorDirection(currentfloor,floorSelected);



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

        while (carriage.position != targetPosition)
        {
            float timeFraction = Mathf.Clamp01((Time.time - startTime) / liftduration);
            carriage.position = Vector3.Lerp(startPosition, targetPosition, timeFraction);
            yield return null;
        }

        // de-parent player from carriage
        player.transform.SetParent(null);

        // set lift to not moving
        isLiftMoving = false;
        currentfloor = floorSelected;
        floorTextInside.text = floorSelected;
        floorTextOutside1.text = floorSelected;
        floorTextOutsideG.text = floorSelected;    
        floorTextOutsideB1.text = floorSelected;    

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






    public string GetFloorDirection(string currentFloor, string selectedFloor)
    {
        // Define the order of the floors
        string[] floorOrder = { "B2", "B1", "G", "1" };

        // Get the indices of the current and selected floors
        int currentFloorIndex = Array.IndexOf(floorOrder, currentFloor);
        int selectedFloorIndex = Array.IndexOf(floorOrder, selectedFloor);

        // Compare the indices to determine direction
        if (selectedFloorIndex > currentFloorIndex)
        {
            return "\u2191";
        }
        else if (selectedFloorIndex < currentFloorIndex)
        {
            return "\u2193";
        }
        else
        {
            return currentFloor;
        }
    }

}
