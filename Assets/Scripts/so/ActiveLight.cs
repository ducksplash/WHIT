using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LightBulb", menuName = "ScriptableObjects/ActiveLight")]
public class ActiveLight : ScriptableObject
{
    public List<Light> lights = new List<Light>();
    public bool isOn = false;
    public LightBulbID lightBulbID;
}