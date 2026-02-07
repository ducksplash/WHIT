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
    public InputActionReference exitAction;
    public InputActionReference throwAction;
    public InputActionReference rotateAction;   // used for rotation
    public InputActionReference focusAction;

    [Header("Rotation")]
    public float rotateSpeed = 180f; // degrees per second

    private Vector2 rotateInput;

    private Coroutine resetBusyCo;

    // Prevent spamming throw/drop from re-starting the coroutine repeatedly
    private bool _releaseInProgress;

    private bool IsHoldingItem => myHeldItem != null;

    void Awake()
    {
        GameMaster.Instance.PLAYERBUSY = false;
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
        
        exitAction.action.performed += DropItem;
        

        rotateAction.action.performed += OnRotatePerformed;
        rotateAction.action.canceled += OnRotateCanceled;
    }

    void OnDisable()
    {
        pickupDropAction.action.performed -= OnPickupDrop;
        throwAction.action.performed -= OnThrow;
        focusAction.action.performed -= OnFocus;
        exitAction.action.performed -= DropItem;
        rotateAction.action.performed -= OnRotatePerformed;
        rotateAction.action.canceled -= OnRotateCanceled;

        if (GameMaster.Instance != null && !IsHoldingItem) ClearBusyImmediate();
    }

    void OnRotatePerformed(InputAction.CallbackContext ctx)
    {
        rotateInput = ctx.ReadValue<Vector2>();
    }

    void OnRotateCanceled(InputAction.CallbackContext ctx)
    {
        rotateInput = Vector2.zero;
    }

    private void OnPickupDrop(InputAction.CallbackContext ctx)
    {
        // Pickup (only if NOT busy and hovering)
        if (!GameMaster.Instance.PLAYERBUSY && clickable.Instance != null && clickable.Instance.IsHoveringPickup())
        {
            PickupItem(clickable.Instance.GetCurrentTarget());
            return;
        }

        // Drop ONLY if we are holding an item (not just "busy")
        if (IsHoldingItem && !_releaseInProgress)
        {
            DropItem();
        }
    }

    private void OnThrow(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (!IsHoldingItem) return;
        if (_releaseInProgress) return;

        ThrowItem();
    }

    private void OnFocus(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (!IsHoldingItem) return;

        // (Leaving your behavior as-is; this line is a bit odd but unrelated to rotation axis issue)
        myHeldItem.transform.localEulerAngles = transform.forward * -1;
    }

    void FixedUpdate()
    {
        if (!IsHoldingItem) return;
        if (rotateInput == Vector2.zero) return;
        if (handTransform == null) return;

        // Rotate around STABLE axes (handTransform), not the object's own local axes.
        // This prevents "left becomes something else after rotating up".

        float dt = Time.fixedDeltaTime;
        float yawDegrees = -rotateInput.x * rotateSpeed * dt;     // left/right (flipped)
        float pitchDegrees = -rotateInput.y * rotateSpeed * dt;   // up/down (invert if you prefer)

        // World-space axes derived from the hand (or camera if your hand follows camera).
        Vector3 yawAxis = handTransform.up;
        Vector3 pitchAxis = handTransform.right;

        // Apply yaw then pitch in WORLD space so the axes stay consistent.
        myHeldItem.Rotate(yawAxis, yawDegrees, Space.World);
        myHeldItem.Rotate(pitchAxis, pitchDegrees, Space.World);
    }

    public void PickupItem(Transform obj)
    {
        if (GameMaster.Instance.PLAYERBUSY) return;

        // Collectables: don't set busy/override at all.
        if (obj.CompareTag("COLLECTABLE"))
        {
            if (obj.name.Contains("TORCH")) GameMaster.Instance.OnboardingManager.CollectTorch();
            else if (obj.name.Contains("NOTEPAD")) GameMaster.Instance.OnboardingManager.CollectNotepad();
            else if (obj.name.Contains("PHONE")) GameMaster.Instance.OnboardingManager.CollectPhone();

            Debug.Log("Collected: " + obj.name);
            return;
        }

        // Enter holding state
        GameMaster.Instance.PLAYERBUSY = true;
        Player.Instance.MoveOverride = true;

        _releaseInProgress = false;

        myHeldItem = obj;
        if (RotationMenu != null) RotationMenu.alpha = 0.7f;

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

    public void DropItem(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (!IsHoldingItem) return;
        if (_releaseInProgress) return;

        _releaseInProgress = true;

        if (RotationMenu != null) RotationMenu.alpha = 0f;

        myHeldItem.SetParent(defaultparent, true);

        var comp = myHeldItem.GetComponent<GetHeldObjectCollisions>();
        if (comp != null) Destroy(comp);

        Rigidbody rb = myHeldItem.GetComponent<Rigidbody>();
        if (rb != null) rb.constraints = RigidbodyConstraints.None;

        myHeldItem = null;

        StartResetBusyOnce();
    }

    public void ThrowItem()
    {
        if (!IsHoldingItem) return;
        if (_releaseInProgress) return;

        _releaseInProgress = true;

        if (RotationMenu != null) RotationMenu.alpha = 0f;

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

        StartResetBusyOnce();
    }

    private void StartResetBusyOnce()
    {
        if (IsHoldingItem) return;

        if (resetBusyCo != null)
            return;

        resetBusyCo = StartCoroutine(SetNotBusy());
    }

    private IEnumerator SetNotBusy()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        ClearBusyImmediate();
    }

    private void ClearBusyImmediate()
    {
        if (IsHoldingItem)
        {
            _releaseInProgress = false;
            if (resetBusyCo != null)
            {
                StopCoroutine(resetBusyCo);
                resetBusyCo = null;
            }
            return;
        }

        GameMaster.Instance.PLAYERBUSY = false;
        Player.Instance.MoveOverride = false;
        _releaseInProgress = false;

        if (resetBusyCo != null)
        {
            StopCoroutine(resetBusyCo);
            resetBusyCo = null;
        }
    }
}
