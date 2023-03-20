using System.Collections;
using UnityEngine;



public class SlidingDoors : MonoBehaviour
{
    [SerializeField] private float openHeight = 3.0f;
    [SerializeField] private float openSpeed = 2.0f;
    [SerializeField] private float closeSpeed = 2.0f;

    private Vector3 closedPosition;
    private Vector3 openedPosition;

    private enum DoorState { Closed, Opening, Opened, Closing }
    private DoorState doorState;
	public GameObject thedoor;
	public bool isLocked = false;
	

    private void Start()
    {
        closedPosition = thedoor.transform.position;
        openedPosition = closedPosition + new Vector3(0, openHeight, 0);
        doorState = DoorState.Closed;

    }



	private void OnTriggerEnter(Collider other)
	{



		if (other.GetComponent<CharacterController>() != null && doorState == DoorState.Closed && !isLocked)
        {
			Debug.Log("sliding door");

            doorState = DoorState.Opening;
            StartCoroutine(OpenDoor());
        }

	}

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CharacterController>() != null && doorState == DoorState.Opened)
        {
            doorState = DoorState.Closing;
            StartCoroutine(CloseDoor());
        }
    }



    private IEnumerator OpenDoor()
    {
        while (Vector3.Distance(thedoor.transform.position, openedPosition) > 0.01f)
        {
            thedoor.transform.position = Vector3.MoveTowards(thedoor.transform.position, openedPosition, openSpeed * Time.deltaTime);
            yield return null;
        }
        thedoor.transform.position = openedPosition;
        doorState = DoorState.Opened;
    }

    private IEnumerator CloseDoor()
    {
        while (Vector3.Distance(thedoor.transform.position, closedPosition) > 0.01f)
        {
            thedoor.transform.position = Vector3.MoveTowards(thedoor.transform.position, closedPosition, closeSpeed * Time.deltaTime);
            yield return null;
        }
        thedoor.transform.position = closedPosition;
        doorState = DoorState.Closed;
    }
}
