using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class manhole : MonoBehaviour
{
    // public GameObject manholeobject;
    // public CanvasGroup actioncanvas;
    // public GameObject actionobject;
    // private Transform playerTransform;
    // public float visibilityDelay = 0.5f;
    // public TextMeshProUGUI actiontext;
    // public Image actionprogress;
    // public string NextScene;
    // public float holdTime = 1.0f;
    // private bool isHolding;
    // private float holdTimer;
    // private bool playerinrange;
    //
    //
    // private void Start()
    // {
    //     manholeobject = gameObject;
    //     Debug.Log(manholeobject.name);
    //
    //     actioncanvas.alpha = 0;
    //     actionprogress.fillAmount = 0.0f;
    // }
    //
    //
    // private void Update()
    // {
    //     playerTransform = Camera.main.transform.parent.transform;
    //     actionobject.transform.LookAt(playerTransform);
    //     
    //     if (playerinrange)
    //     {
    //
    //         if (InputManager.GetKey("special"))
    //         {
    //             if (!isHolding)
    //             {
    //                 // Key is just pressed
    //                 isHolding = true;
    //                 holdTimer = 0.0f;
    //                 Debug.Log("Key is being held down");
    //             }
    //             else
    //             {
    //                 // Key is being held down
    //                 holdTimer += Time.deltaTime;
    //                 float progress = Mathf.Clamp01(holdTimer / holdTime);
    //                 actionprogress.fillAmount = progress;
    //                 if (holdTimer >= holdTime)
    //                 {
    //                     
    //                     Debug.Log("Key has been held down for " + holdTime + " seconds");
    //                     TravelCompanion.Instance.ChangeSceneOffTheBooks(NextScene);
    //                 }
    //             }
    //         }
    //         else
    //         {
    //             if (isHolding)
    //             {
    //                 // Key is just released
    //                 isHolding = false;
    //                 actionprogress.fillAmount = 0.0f;
    //             }
    //         }
    //     }
    // }
    //
    // private void OnTriggerEnter(Collider hit)
    // {
    //     if (hit.gameObject.CompareTag("Player"))
    //     {
    //         Invoke("ShowSign", visibilityDelay);
    //     }
    // }
    //
    // private void OnTriggerStay(Collider hit)
    // {
    //     if (hit.gameObject.CompareTag("Player"))
    //     {
    //         Invoke("ShowSign", visibilityDelay);
    //     }
    // }
    //
    // private void OnTriggerExit(Collider hit)
    // {
    //     if (hit.gameObject.CompareTag("Player"))
    //     {
    //         Invoke("HideSign", visibilityDelay);
    //     }
    // }
    //
    // private void ShowSign()
    // {
    //     actiontext.text = "Hold "+InputManager.GetKeyName("special")+"\nTo Enter";
    //     actioncanvas.alpha = 1;
    //     playerinrange = true;
    // }
    //
    // private void HideSign()
    // {
    //     actioncanvas.alpha = 0;
    //     playerinrange = false;
    // }
    //

}
