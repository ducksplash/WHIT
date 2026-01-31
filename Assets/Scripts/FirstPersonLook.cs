using UnityEngine;
using UnityEngine.InputSystem;   // <-- NEW

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField] Transform character;
    [SerializeField] InputActionReference lookAction;
    
    Vector2 currentMouseLook;
    Vector2 appliedMouseDelta;

    public float sensitivity = 1f;
    public float smoothing = 2f;


    private void Awake()
    {
        lookAction = GameMaster.Instance.InputManager.LookAction;
    }
    
    
    private void Start()
    {
        sensitivity = GameMaster.Instance.MouseSensitivity;
        EventManager.OnStartComputer += LookAtPC;
        lookAction.action.Enable();
    }
    
    
    void FixedUpdate()
    {
        if (!GameMaster.Instance.FROZEN)
        {
            transform.localRotation = Quaternion.AngleAxis(-currentMouseLook.y, Vector3.right);
            character.localRotation = Quaternion.AngleAxis(currentMouseLook.x, Vector3.up);
        }
    }

    private void LateUpdate()
    {
        if (GameMaster.Instance.FROZEN) return;

        // Read from Input System
        Vector2 rawMouse = lookAction.action.ReadValue<Vector2>();

        // Apply smoothing + sensitivity
        Vector2 smoothMouseDelta = Vector2.Scale(rawMouse, Vector2.one * sensitivity * smoothing);

        appliedMouseDelta = Vector2.Lerp(appliedMouseDelta, smoothMouseDelta, 1f / smoothing);

        currentMouseLook += appliedMouseDelta;
        currentMouseLook.y = Mathf.Clamp(currentMouseLook.y, -60, 60);
    }

    public void SetPlayerRotation(Vector2 rotation)
    {
        currentMouseLook = rotation;
    }

    public void LookAtPC(Transform ComputerTransform)
    {
        transform.LookAt(ComputerTransform);
    }
}