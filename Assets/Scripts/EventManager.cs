using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{    
    public static event Action<LightBulbID> OnLightSwitchClick = (LightBulbID) => { };
    //public static event Action OnLightSwitchClick = () => { };
    //public static event Action<TempSessionDataClass> OnDistributionGameSessionEnded = (sessionData) => { };
    
    public void ClickedLightSwitch(LightBulbID LightBulbID)
    {
        OnLightSwitchClick.Invoke(LightBulbID);
    }
    
}