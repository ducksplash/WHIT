using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(
    fileName = "Inputs",
    menuName = "{!!} Tawley Scriptable Object/Inputs",
    order = 10)]
public class Inputs : ScriptableObject
{
    [Header("Input Details")]
    public InputName InputName = InputName.Move;
    
    public string InputActionName;
    
    public string InputKeyController;
    public string InputKeyPC;
    public string AlternativeInputKeyPC;
}