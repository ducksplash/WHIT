using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class Manhole : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup actioncanvas;
    public GameObject actionobject;
    public TextMeshProUGUI actiontext;
    public Image actionprogress;

    [Header("Gameplay")]
    public GAMELEVEL NextScene;
    public float visibilityDelay = 0.5f;
    public float holdTime = 1.0f;

    [Header("Input")]
    public InputActionReference interactAction;

    private Transform playerTransform;
    private bool isHolding = false;
    private float holdTimer = 0f;
    private bool playerInRange = false;
    //
    // private void Start()
    // {
    //     actioncanvas.alpha = 0f;
    //     actionprogress.fillAmount = 0f;
    // }
    //
    // private void OnEnable()
    // {
    //     if (interactAction != null)
    //         interactAction.action.Enable();
    // }
    //
    // private void OnDisable()
    // {
    //     if (interactAction != null)
    //         interactAction.action.Disable();
    // }
    //
    // private void Update()
    // {
    //     if (playerTransform == null)
    //         playerTransform = Camera.main.transform.parent;
    //
    //     actionobject.transform.LookAt(playerTransform);
    //
    //     if (!playerInRange || interactAction == null) return;
    //
    //     bool isPressed = interactAction.action.ReadValue<float>() > 0f;
    //
    //     if (isPressed)
    //     {
    //         if (!isHolding)
    //         {
    //             isHolding = true;
    //             holdTimer = 0f;
    //             Debug.Log("Started holding interact key");
    //         }
    //         else
    //         {
    //             holdTimer += Time.deltaTime;
    //             float progress = Mathf.Clamp01(holdTimer / holdTime);
    //             actionprogress.fillAmount = progress;
    //
    //             if (holdTimer >= holdTime)
    //             {
    //                 Debug.Log($"Held interact key for {holdTime} seconds. Triggering scene change.");
    //                 GameMaster.Instance.TravelCompanion.ChangeSceneOffTheBooks(NextScene);
    //
    //                 // Reset to prevent multiple triggers
    //                 holdTimer = 0f;
    //                 isHolding = false;
    //                 actionprogress.fillAmount = 0f;
    //             }
    //         }
    //     }
    //     else
    //     {
    //         if (isHolding)
    //         {
    //             // Key released before hold time
    //             isHolding = false;
    //             holdTimer = 0f;
    //             actionprogress.fillAmount = 0f;
    //             Debug.Log("Released interact key before hold completed");
    //         }
    //     }
    // }
    //
    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag("Player"))
    //         Invoke(nameof(ShowSign), visibilityDelay);
    // }
    //
    // private void OnTriggerStay(Collider other)
    // {
    //     if (other.CompareTag("Player"))
    //         Invoke(nameof(ShowSign), visibilityDelay);
    // }
    //
    // private void OnTriggerExit(Collider other)
    // {
    //     if (other.CompareTag("Player"))
    //         Invoke(nameof(HideSign), visibilityDelay);
    // }
    //
    // private void ShowSign()
    // {
    //     if (!playerInRange)
    //     {
    //         actiontext.text = $"Hold {interactAction?.action?.name ?? "Interact"}\nTo Enter";
    //         actioncanvas.alpha = 1f;
    //         playerInRange = true;
    //     }
    // }
    //
    // private void HideSign()
    // {
    //     actioncanvas.alpha = 0f;
    //     playerInRange = false;
    //     isHolding = false;
    //     holdTimer = 0f;
    //     actionprogress.fillAmount = 0f;
    // }
}
