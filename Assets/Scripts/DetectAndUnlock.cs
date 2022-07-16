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
                    LinkedObject.GetComponentInChildren<innerDoors>().isLocked = false;
                    JobDone = true;
                }
            }
        }
    }




}