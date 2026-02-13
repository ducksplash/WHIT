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
    
    [Header("Back/Exit")]
    public InputActionReference ExitAction;
}



public enum InputName
{
    Move, // Left Stick
    Look, // Right Stick
    Jump, // B, Spacebar, right shift
    Crouch,
    WalkSlowly, // Y
    Use,
    Melee,
    Torch,
    Phone,
    Submit,
    NavigateLeft,
    NavigateRight,
    NavigateUp,
    NavigateDown,
    ZoonIn,
    ZoomOut,
    PickupDrop,
    // pc specific
    MoveForward,
    MoveBackward,
    StrafeLeft,
    StrafeRight,
    PauseResume,
    BackExit,
    Navigate
    
}