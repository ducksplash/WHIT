using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class StudioDoor : MonoBehaviour
{
    [Header("Door Animators")]
    [SerializeField] private List<Animator> doorAnimators = new List<Animator>();

    [Header("State")]
    [SerializeField] private bool startOpen = false;

    [Header("Click Interaction")]
    [SerializeField] private string doorLayerName = "Door";
    [SerializeField] private float clickDistance = 6f;
    [SerializeField] private InputActionReference clickAction;

    [Header("Animator Trigger Names")]
    [SerializeField] private string openLeftTrigger = "OpenLeft";
    [SerializeField] private string openRightTrigger = "OpenRight";
    [SerializeField] private string closeLeftTrigger = "CloseLeft";
    [SerializeField] private string closeRightTrigger = "CloseRight";

    [SerializeField] private List<Collider> DoorColliders = new List<Collider>();
    [Header("Collider Timing")]
    private float colliderDisableTime = 0.5f;

    private bool _isAnimating;
    private Coroutine _colliderRoutine;
    private bool _isOpen;
    private int _doorLayer = -1;

    private void Awake()
    {
        _doorLayer = LayerMask.NameToLayer(doorLayerName);
        if (_doorLayer < 0) Debug.LogWarning($"{nameof(StudioDoor)}: Layer '{doorLayerName}' not found.");

        foreach (var doorAnimator in doorAnimators)
        {
            if (doorAnimator.gameObject.GetComponent<Collider>() != null) DoorColliders.Add(doorAnimator.gameObject.GetComponent<Collider>());
        }
    }

    private void Start()
    {
        _isOpen = startOpen;
    }

    private void OnEnable()
    {
        if (clickAction != null)
        {
            clickAction.action.Enable();
            clickAction.action.performed += OnClickPerformed;
        }
    }

    private void OnDisable()
    {
        if (clickAction != null)
        {
            clickAction.action.performed -= OnClickPerformed;
        }
    }

    private void OnClickPerformed(InputAction.CallbackContext _)
    {
        if (Mouse.current == null)
            return;

        Camera cam = Camera.main;
        if (cam == null)
            return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);

        if (!Physics.Raycast(ray, out RaycastHit hit, clickDistance, ~0, QueryTriggerInteraction.Ignore))
            return;

        if (_doorLayer >= 0 && hit.collider.gameObject.layer != _doorLayer)
            return;

        StudioDoor clickedDoor = hit.collider.GetComponentInParent<StudioDoor>();
        if (clickedDoor == null)
            return;

        if (clickedDoor != this)
            return;

        Interact();
    }

    public void Interact()
    {
        if (_isOpen)
            Close();
        else
            Open();
    }

    public void Open()
    {
        if (_isOpen || _isAnimating) return;

        for (int i = 0; i < doorAnimators.Count; i++)
        {
            Animator anim = doorAnimators[i];
            if (anim == null) continue;

            SafeResetTrigger(anim, closeLeftTrigger);
            SafeResetTrigger(anim, closeRightTrigger);
            SafeSetTrigger(anim, openLeftTrigger);
            SafeSetTrigger(anim, openRightTrigger);
        }

        if (_colliderRoutine != null)
            StopCoroutine(_colliderRoutine);

        _colliderRoutine = StartCoroutine(DisableCollidersTemporarily());

        _isOpen = true;
    }

    public void Close()
    {
        if (!_isOpen || _isAnimating) return;

        for (int i = 0; i < doorAnimators.Count; i++)
        {
            Animator anim = doorAnimators[i];
            if (anim == null) continue;

            SafeResetTrigger(anim, openLeftTrigger);
            SafeResetTrigger(anim, openRightTrigger);
            SafeSetTrigger(anim, closeLeftTrigger);
            SafeSetTrigger(anim, closeRightTrigger);
        }

        if (_colliderRoutine != null)
            StopCoroutine(_colliderRoutine);

        _colliderRoutine = StartCoroutine(DisableCollidersTemporarily());

        _isOpen = false;
    }

    private static void SafeSetTrigger(Animator anim, string triggerName)
    {
        if (string.IsNullOrWhiteSpace(triggerName)) return;
        if (!HasParameter(anim, triggerName, AnimatorControllerParameterType.Trigger)) return;
        anim.SetTrigger(triggerName);
    }

    private static void SafeResetTrigger(Animator anim, string triggerName)
    {
        if (string.IsNullOrWhiteSpace(triggerName)) return;
        if (!HasParameter(anim, triggerName, AnimatorControllerParameterType.Trigger)) return;
        anim.ResetTrigger(triggerName);
    }

    private static bool HasParameter(Animator anim, string paramName, AnimatorControllerParameterType type)
    {
        if (anim == null) return false;

        AnimatorControllerParameter[] parameters = anim.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].type == type && parameters[i].name == paramName)
                return true;
        }

        return false;
    }
    
    
    private void SetDoorCollidersEnabled(bool enabled)
    {
        for (int i = 0; i < DoorColliders.Count; i++)
        {
            if (DoorColliders[i] != null)
                DoorColliders[i].enabled = enabled;
        }
    }

    private System.Collections.IEnumerator DisableCollidersTemporarily()
    {
        _isAnimating = true;
        SetDoorCollidersEnabled(false);

        yield return new WaitForSeconds(colliderDisableTime);

        SetDoorCollidersEnabled(true);
        _isAnimating = false;
    }
}