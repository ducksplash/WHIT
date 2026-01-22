using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Pickup : MonoBehaviour
{
    public Transform defaultparent;
    public Transform myHeldItem;
    public Transform handTransform;
    public Vector3 StartRotation;
    public CanvasGroup RotationMenu;

    [Header("Input (New System)")]
    public InputActionReference pickupDropAction;
    public InputActionReference throwAction;
    public InputActionReference rotateAction;   // used for rotation
    public InputActionReference focusAction;

    private Vector2 rotateInput;

    void Awake()
    {
        GameMaster.Instance.HASITEM = false;
        if (handTransform != null) StartRotation = handTransform.parent.eulerAngles;
    }

    void Start()
    {
        if (handTransform == null && Player.Instance != null)
            handTransform = Player.Instance.playerHand;
    }

    void OnEnable()
    {
        pickupDropAction?.action.Enable();
        throwAction?.action.Enable();
        rotateAction?.action.Enable();
        focusAction?.action.Enable();

        pickupDropAction.action.performed += OnPickupDrop;
        throwAction.action.performed += OnThrow;
        focusAction.action.performed += OnFocus;

        rotateAction.action.performed += OnRotatePerformed;
        rotateAction.action.canceled += OnRotateCanceled;
    }

    void OnDisable()
    {
        pickupDropAction.action.performed -= OnPickupDrop;
        throwAction.action.performed -= OnThrow;
        focusAction.action.performed -= OnFocus;

        rotateAction.action.performed -= OnRotatePerformed;
        rotateAction.action.canceled -= OnRotateCanceled;
    }

    void OnRotatePerformed(InputAction.CallbackContext ctx)
    {
        rotateInput = ctx.ReadValue<Vector2>();
    }

    void OnRotateCanceled(InputAction.CallbackContext ctx)
    {
        rotateInput = Vector2.zero;
    }
    
    

    private void OnPickupDrop(InputAction.CallbackContext ctx = new InputAction.CallbackContext())
    {
        //if (!ctx.performed) return;

        if (!GameMaster.Instance.HASITEM && clickable.Instance != null && clickable.Instance.IsHoveringPickup())
        {
            PickupItem(clickable.Instance.GetCurrentTarget());
        }
        else if (GameMaster.Instance.HASITEM)
        {
            DropItem();
        }
    }

    private void OnThrow(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (GameMaster.Instance.HASITEM) ThrowItem();
    }

    private void OnFocus(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || !GameMaster.Instance.HASITEM || myHeldItem == null) return;
        myHeldItem.transform.localEulerAngles = transform.forward * -1;
    }

    void FixedUpdate()
    {
        if (!GameMaster.Instance.HASITEM || myHeldItem == null) return;

        if (rotateInput != Vector2.zero)
        {
            Vector3 rot = new Vector3(rotateInput.y * 5f, rotateInput.x * 5f, 0f);
            myHeldItem.Rotate(rot * (Time.smoothDeltaTime * 20f), Space.Self);
        }
    }

    public void PickupItem(Transform obj)
    {
        if (GameMaster.Instance.PHONEOUT || GameMaster.Instance.FROZEN || GameMaster.Instance.HASITEM) return;

        GameMaster.Instance.HASITEM = true;
        
        if (obj.CompareTag("COLLECTABLE"))
        {
            if (obj.name.Contains("TORCH")) GameMaster.Instance.OnboardingManager.CollectTorch();
            else if (obj.name.Contains("NOTEPAD")) GameMaster.Instance.OnboardingManager.CollectNotepad();
            else if (obj.name.Contains("PHONE")) GameMaster.Instance.OnboardingManager.CollectPhone();

            GameMaster.Instance.HASITEM = false;
            Debug.Log("Collected: " + obj.name);
            return;
        }

        myHeldItem = obj;
        RotationMenu.alpha = 0.7f;

        obj.SetParent(handTransform, true);
        obj.localPosition = Vector3.zero;
        obj.localRotation = Quaternion.identity;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null) rb.constraints = RigidbodyConstraints.FreezeAll;

        obj.gameObject.AddComponent<GetHeldObjectCollisions>();

        Evidence e = obj.GetComponent<Evidence>();
        if (e != null && e.EvidenceQuality > 1) e.EvidenceQuality--;

        Debug.Log("Picked up holdable: " + obj.name);
    }

    public void DropItem()
    {
        RotationMenu.alpha = 0f;
        if (myHeldItem != null)
        {
            myHeldItem.SetParent(defaultparent, true);

            var comp = myHeldItem.GetComponent<GetHeldObjectCollisions>();
            if (comp != null) Destroy(comp);

            Rigidbody rb = myHeldItem.GetComponent<Rigidbody>();
            if (rb != null) rb.constraints = RigidbodyConstraints.None;

            myHeldItem = null;
            GameMaster.Instance.HASITEM = false;
        }
    }

    public void ThrowItem()
    {
        RotationMenu.alpha = 0f;
        if (myHeldItem == null) return;

        myHeldItem.SetParent(defaultparent, true);

        var comp = myHeldItem.GetComponent<GetHeldObjectCollisions>();
        if (comp != null) Destroy(comp);

        Rigidbody rb = myHeldItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.None;
            rb.AddForce(Camera.main.transform.forward * 200f);
        }

        myHeldItem = null;
        GameMaster.Instance.HASITEM = false;
    }
}
