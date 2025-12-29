using UnityEngine;
using UnityEngine.InputSystem;   // <-- NEW

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField] Transform character;
    [SerializeField] InputActionReference lookAction;   // <-- drag your Look action here

    Vector2 currentMouseLook;
    Vector2 appliedMouseDelta;

    public float sensitivity = 1f;
    public float smoothing = 2f;

    private void Start()
    {
        sensitivity = GameMaster.Instance.MouseSensitivity;
    }
    
    
    private void OnEnable()
    {
        lookAction.action.Enable();
    }

    private void OnDisable()
    {
        lookAction.action.Disable();
    }

    void FixedUpdate()
    {
        if (GameMaster.INMENU)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (!GameMaster.FROZEN)
        {
            transform.localRotation = Quaternion.AngleAxis(-currentMouseLook.y, Vector3.right);
            character.localRotation = Quaternion.AngleAxis(currentMouseLook.x, Vector3.up);
        }
    }

    private void LateUpdate()
    {
        if (GameMaster.FROZEN) return;

        // Read from Input System
        Vector2 rawMouse = lookAction.action.ReadValue<Vector2>();

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