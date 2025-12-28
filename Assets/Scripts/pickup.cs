using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using VLB;

public class Pickup : MonoBehaviour
{
    public bool hasobject;
    public Transform defaultparent;
    public Transform myHeldItem;
    public Transform handTransform;
    public bool hasobjectshown;
    public Vector3 StartRotation;
    public CanvasGroup RotationMenu;
    public LayerMask IgnoreLayer;

    [Header("Input (New System)")]
    public InputActionReference pointerPositionAction;
    public InputActionReference pickupDropAction;
    public InputActionReference throwAction;
    public InputActionReference rotateAction;
    public InputActionReference focusAction;

    void Awake()
    {
        hasobject = false;
        if (handTransform != null)
            StartRotation = handTransform.parent.eulerAngles;
    }

    void Start()
    {
        if (handTransform == null && Player.Instance != null)
            handTransform = Player.Instance.playerHand;
    }

    void OnEnable()
    {
        pointerPositionAction?.action.Enable();
        pickupDropAction?.action.Enable();
        throwAction?.action.Enable();
        rotateAction?.action.Enable();
        focusAction?.action.Enable();

        pickupDropAction.action.performed += OnPickupDrop;
        throwAction.action.performed += OnThrow;
        focusAction.action.performed += OnFocus;
    }

    void OnDisable()
    {
        pickupDropAction.action.performed -= OnPickupDrop;
        throwAction.action.performed -= OnThrow;
        focusAction.action.performed -= OnFocus;
    }

    private void OnPickupDrop(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // ⛔ do not pick up if hovering drawers/doors/etc
        if (clickable.Instance != null && clickable.Instance.IsHoveringInteractive())
            return;

        if (!hasobject)
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, Player.Instance.RayCastDistance, ~IgnoreLayer))
                PickupItem(hit);
        }
        else
        {
            DropItem();
        }
    }

    private void OnThrow(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (hasobject)
            ThrowItem();
    }

    private void OnFocus(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || !hasobject || myHeldItem == null) return;
        myHeldItem.transform.localEulerAngles = transform.forward * -1;
    }

    public void PickupItem(RaycastHit hit)
    {
        if (GameMaster.PHONEOUT || GameMaster.FROZEN) return;

        Transform obj = hit.transform;


        if (obj.CompareTag("COLLECTABLE"))
        {
            if (obj.name.Contains("TORCH")) GameMaster.Instance.OnboardingManager.CollectTorch();
            else if (obj.name.Contains("NOTEPAD")) GameMaster.Instance.OnboardingManager.CollectNotepad();
            else if (obj.name.Contains("PHONE")) GameMaster.Instance.OnboardingManager.CollectPhone();


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

        hasobject = true;
        GameMaster.HASITEM = true;

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
            hasobject = false;
            GameMaster.HASITEM = false;
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
        hasobject = false;
        GameMaster.HASITEM = false;
    }
}
