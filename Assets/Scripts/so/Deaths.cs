using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Deaths", menuName = "{!!} Tawley Scriptable Object/Death", order = 10)]
public class Deaths : ScriptableObject
{
    [Header("Deaths Details")] 
    public Death DeathType = Death.Electrocuted;

    public string DeathText;


    
    
}