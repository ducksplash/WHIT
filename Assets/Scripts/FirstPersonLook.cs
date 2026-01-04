using UnityEngine;
using UnityEngine.InputSystem;   // <-- NEW

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField] Transform character;
    [SerializeField] InputActionReference lookActionPC;
    [SerializeField] InputActionReference lookActionConsole;

    [SerializeField] InputActionReference selectedLookAction;
    
    Vector2 currentMouseLook;
    Vector2 appliedMouseDelta;

    public float sensitivity = 1f;
    public float smoothing = 2f;

    private void Start()
    {
        sensitivity = GameMaster.Instance.MouseSensitivity;
        
        if (GameMaster.Instance.DeviceType.selectedDeviceType == PlayerDeviceType.DesktopPC)
        {
            lookActionPC.action.Enable();
            lookActionConsole.action.Disable();
            Debug.Log("set pc");
            selectedLookAction = lookActionPC;
        }
        else
        {
            lookActionPC.action.Disable();
            lookActionConsole.action.Enable();
            Debug.Log("set console");
            selectedLookAction = lookActionConsole;
        }
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
        Vector2 rawMouse = selectedLookAction.action.ReadValue<Vector2>();

        // Apply smoothing + sensitivity
        Vector2 smoothMouseDelta =
            Vector2.Scale(rawMouse, Vector2.one * sensitivity * smoothing);

        appliedMouseDelta = Vector2.Lerp(appliedMouseDelta, smoothMouseDelta, 1f / smoothing);

        currentMouseLook += appliedMouseDelta;
        currentMouseLook.y = Mathf.Clamp(currentMouseLook.y, -60, 60);
    }

    public void SetPlayerRotation(Vector2 rotation)
    {
        currentMouseLook = rotation;
    }
}