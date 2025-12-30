using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;

public class VirtualCursor : MonoBehaviour
{
    [Header("Cursor Settings")]
    public float speed = 1200f;
    public RectTransform cursorVisual;

    private Vector2 cursorPos;
    private InputSystemUIInputModule uiModule;
    private Mouse virtualMouse;
    private InputAction pointAction;

    void Start()
    {
        TryFindModule();

        // Create the virtual mouse
        virtualMouse = InputSystem.AddDevice<Mouse>("VirtualMouse");

        cursorPos = new Vector2(Screen.width / 2f, Screen.height / 2f);

        // Create an action bound to the virtual mouse position
        pointAction = new InputAction(
            name: "VirtualPoint",
            type: InputActionType.Value,
            binding: "<VirtualMouse>/position"
        );

        pointAction.Enable();

        if (uiModule != null)
        {
            // Wrap into a Reference and assign
            uiModule.point = InputActionReference.Create(pointAction);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void OnDestroy()
    {
        if (pointAction != null)
            pointAction.Disable();

        if (virtualMouse != null)
            InputSystem.RemoveDevice(virtualMouse);
    }

    void TryFindModule()
    {
        var es = EventSystem.current;

        if (es == null)
        {
            Debug.LogError("No EventSystem found!");
            return;
        }

        uiModule = es.GetComponent<InputSystemUIInputModule>();

        if (uiModule == null)
            uiModule = FindObjectOfType<InputSystemUIInputModule>();

        if (uiModule == null)
            Debug.LogError("No InputSystemUIInputModule found.");
    }

    void Update()
    {
        if (virtualMouse == null || uiModule == null)
            return;

        Vector2 move = Vector2.zero;

        // Trackpad / mouse
        if (Mouse.current != null)
            move += Mouse.current.delta.ReadValue();

        // Gamepad stick
        if (Gamepad.current != null)
            move += Gamepad.current.rightStick.ReadValue() * 20f;

        cursorPos += move * speed * Time.unscaledDeltaTime;
        cursorPos.x = Mathf.Clamp(cursorPos.x, 0, Screen.width);
        cursorPos.y = Mathf.Clamp(cursorPos.y, 0, Screen.height);

        var state = new MouseState { position = cursorPos };
        InputSystem.QueueStateEvent(virtualMouse, state);
        InputSystem.Update();

        if (cursorVisual != null)
            cursorVisual.position = cursorPos;
    }
}
