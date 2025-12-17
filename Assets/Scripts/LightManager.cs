using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    [Header("Nora's Flat")]
    public List<ActiveLight> lightBulbsNorasFlat = new List<ActiveLight>();
    [Header("Tawley Meats")]
    public List<ActiveLight> lightBulbsTawleyMeats = new List<ActiveLight>();
    [Header("Roark Inside")]
    public List<ActiveLight> lightBulbsRoarkInside = new List<ActiveLight>();
    
    void Start()
    {
        EventManager.OnLightSwitchClick += ClickedSwitch;
    }

    private void ClickedSwitch(LightBulbID LightBulbID)
    {
        Debug.Log("Clicked "+LightBulbID);
    }


    public void SelectAppropriateLights()
    {
        switch (GameMaster.Instance.THISLEVEL)
        {
            case GameMaster.GAMELEVEL.MainMenu:
                // na - no lights
                break;
            case GameMaster.GAMELEVEL.NorasFlat:
                // na - no lights
                lightBulbsNorasFlat = new List<ActiveLight>();
                break;
            case GameMaster.GAMELEVEL.TawleyMeats:
                // na - no lights
                break;
            case GameMaster.GAMELEVEL.RoarkInside:
                // na - no lights
                break;
        }
    }
    

}


public enum LightBulbID
{
    NoraFlatBathroom,
    NoraFlatLivingroom,
    NoraFlatKitchen
}

public enum LightBulbLocation
{
    NorasFlat,
    TawleyMeats,
    RoarkOutside,
    RoarkInside
}
