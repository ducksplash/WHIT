using System.Collections;
using UnityEngine;

public class ElectricalRoomDoor : MonoBehaviour
{
	
	public GameObject theLock;
	public GameObject theDoor;
	public GameObject theTorchCap;
	public int DoorStrikes;
	public int DoorStrikesNeeded = 18;
	private string doorName;
	private string torchCapName;
	private string lockName;
	private Animator elecDoorAnimator;
	private bool doorHasFallen = false;
	
	
    // Start is called before the first frame update
    void Start()
    {
		
		doorName = theDoor.name;
		torchCapName = theTorchCap.name;
		lockName = theLock.name;
		elecDoorAnimator = theDoor.GetComponent<Animator>();
		
    }
	
	
	
	void OnTriggerExit(Collider other)
    {
        //Check for a match with the specified name on any GameObject that collides with your GameObject
        if (other.gameObject.name.Equals(torchCapName))
        {
			
			if (DoorStrikes <= DoorStrikesNeeded)
			{
			DoorStrikes++;
			}
			else
			{
				
				if (!doorHasFallen)
				{
					killTheDoor();
				}
				
			}
        }
    }
	
	
	void killTheDoor()
	{
		
		elecDoorAnimator.SetTrigger("doorFall");
		doorHasFallen = true;
	}	
}
