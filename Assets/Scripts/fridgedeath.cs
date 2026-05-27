// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Rendering;
// using UnityEngine.Rendering.HighDefinition;
// using UnityEngine.UI;
//
// public class fridgedeath : MonoBehaviour
// {
//
//
//     public GameObject FridgeDoor;
//     public Volume FridgeVol;
//     public Image FridgeDoorImg;
//     public Fog fridgeFog;
//     public bool FogAvailable;
//     public ParticleSystem FridgeParte;
//     public bool FridgePower;
//     public bool CoolDown;
//     public DialogueName selectedDialogue;
//
//
//     void Start()
//     {
//         Debug.Log("todo: Refactor fridge to use new Door script");
//
//         FridgeParte = GetComponentInChildren<ParticleSystem>();
//
//         FridgeParte.gameObject.SetActive(false);
//         FridgeVol.gameObject.SetActive(false);
//
//         if (FridgeVol.profile.TryGet<Fog>(out fridgeFog))
//         {
//             FogAvailable = true;
//         }
//
//     }
//
//
//     private void Update()
//     {
//         return;
//         if (!FridgePower)
//         {
//             if (GameMaster.Instance.POWER_SUPPLY_ENABLED)
//             {
//                 FridgeParte.gameObject.SetActive(true);
//                 FridgeVol.gameObject.SetActive(true);
//
//                 FridgePower = true;
//             }
//         }
//     }
//
//
//     public void OnTriggerEnter(Collider other)
//     {
//         if (!CoolDown)
//         {
//             if (other.CompareTag("Player"))
//             {
//                 StartCoroutine(TrapPlayer());
//                 gameObject.GetComponent<Collider>().enabled = false;
//
//             }
//         }
//     }
//     
//     public IEnumerator TrapPlayer()
//     {
//
//
//         Debug.Log("playa fridgin' yo");
//
//         FridgeDoorImg.color = Color.red;
//
//
//         yield return new WaitForSeconds(2f);
//
//         if (FogAvailable)
//         {
//             fridgeFog.color.value = Color.cyan;
//
//         }
//
//         StartCoroutine(DoDeath());
//
//
//     }
//
//
//     IEnumerator DoDeath()
//     {
//
//         yield return new WaitForSeconds(1f);
//         
//         GameMaster.Instance.DialogueManager.NewDialogue(selectedDialogue, 5);
//
//         yield return new WaitForSeconds(2f);
//
//         Player.Instance.CauseDeath("being flash frozen");
//         
//         StartCoroutine(DoCooldown());
//
//     }
//
//
//     IEnumerator DoCooldown()
//     {
//         yield return new WaitForSeconds(15f);
//
//         gameObject.GetComponent<Collider>().enabled = true;
//     }
//
// }
