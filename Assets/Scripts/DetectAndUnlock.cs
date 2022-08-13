using TMPro;
using UnityEngine;

public class DetectAndUnlock : MonoBehaviour
{
    public GameObject LinkedObject;
    public string ObjectType;
    public bool JobDone;

    private void Start()
    {

        ObjectType = "default";

        if (LinkedObject.GetComponentInChildren<innerDoors>() != null)
        {
            ObjectType = "door";
        }



    }



    private void OnTriggerEnter(Collider other)
    {
        if (!JobDone)
        {
            if (other.name.Contains("Player"))
            {
                if (ObjectType == "door")
                {
                    other.gameObject.GetComponent<FirstPersonCollision>().uncrouch();
                    LinkedObject.GetComponentInChildren<innerDoors>().isLocked = false;
                    JobDone = true;


                    var fakegameobject = new GameObject("FakeObject", typeof(BoxCollider));
                    fakegameobject.GetComponent<Collider>().enabled = false;

                    var msg = "Someone made the effort to lock this room from the inside...";

                    other.gameObject.GetComponent<DialogueManager>().NewDialogue("NORA", msg, 5, fakegameobject);


                }
            }
        }
    }




}