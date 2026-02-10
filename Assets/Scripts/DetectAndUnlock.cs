// using TMPro;
// using UnityEngine;
//
//
//  Detects player presence in bathroom at tawley meats.
// Commented out for refactor
//
// public class DetectAndUnlock : MonoBehaviour
// {
//     public GameObject LinkedObject;
//     public string ObjectType;
//     public bool JobDone;
//
//     private void Start()
//     {
//         ObjectType = "default";
//
//         if (LinkedObject.GetComponentInChildren<innerDoors>() != null)
//         {
//             ObjectType = "door";
//         }
//     }
//
//
//
//     private void OnTriggerEnter(Collider other)
//     {
//         if (!JobDone)
//         {
//             if (other.CompareTag("Player"))
//             {
//                 if (ObjectType == "door")
//                 {
//                     Player.Instance.uncrouch();
//                     LinkedObject.GetComponentInChildren<innerDoors>().isLocked = false;
//                     JobDone = true;
//
//                     var msg = "Someone made the effort to lock this room from the inside...";
//                     GameMaster.Instance.DialogueManager.NewDialogue(Contacts.Nora.ToString(), msg, 5);
//                 }
//             }
//         }
//     }
//
//
//
//
// }