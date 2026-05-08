using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("Player Movement")]
    public InputActionReference LookAction;
    public InputActionReference MoveAction;

    [Header("Pause")]
    public InputActionReference PauseAction;

    [Header("Back/Exit")]
    public InputActionReference ExitAction;

    [Header("Submit/Advance")]
    public InputActionReference SubmitAction;

    [Header("Input Definitions")]
    public List<InputSO> Inputs = new List<InputSO>();

    
    public Dictionary<string, string> InputEregiDict = new();

    private void Start()
    {
        CreateEregiDictionary();
    }

    private void CreateEregiDictionary()
    {
        InputEregiDict.Clear();

        bool steam = isSteamDeck();

        foreach (var input in Inputs)
        {
            if (input == null || string.IsNullOrEmpty(input.EregiReplaceString)) continue;

            string keyValue = steam ? input.InputKeySteam : input.InputKeyDesktop;
            InputEregiDict.TryAdd(input.EregiReplaceString, keyValue);
        }
    }

    /// <summary>
    /// Returns the platform-appropriate key string for a given InputName.
    /// </summary>
    public string ReturnInputName(InputName selectedInputName = InputName.Use)
    {
        bool steam = isSteamDeck();

        foreach (var input in Inputs)
        {
            if (input == null) continue;
            if (input.InputName != selectedInputName) continue;

            return steam ? input.InputKeySteam : input.InputKeyDesktop;
        }

        return "null";
    }



    public bool isSteamDeck()
    {
        return GameMaster.Instance.DeviceType.selectedDeviceType == PlayerDeviceType.SteamOS;
    }
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