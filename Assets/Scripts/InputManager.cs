using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    // Input Manager
    [Header("Player Movement")]
    public InputActionReference LookAction;
    public InputActionReference MoveAction;
    
    [Header("Pause")]
    public InputActionReference PauseAction;
}
