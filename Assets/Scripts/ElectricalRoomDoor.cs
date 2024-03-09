using System.Collections;
using UnityEngine;

public class ElectricalRoomDoor : MonoBehaviour
{
	
	public GameObject theLock;
	public GameObject theDoor;
	public Collider theDoorCollider;
	public int DoorStrikes;
	public int DoorStrikesNeeded = 18;
	private Animator elecDoorAnimator;
	private bool doorHasFallen = false;
	
	
    // Start is called before the first frame update
    void Start()
    {
		elecDoorAnimator = theDoor.GetComponent<Animator>();
	}
    
	void OnTriggerExit(Collider other)
    {
        //Check for a match with the specified name on any GameObject that collides with your GameObject
        if (other.gameObject.tag.Equals("torchcap_implement"))
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
		theDoorCollider.enabled = false;
		doorHasFallen = true;
	}	
}
