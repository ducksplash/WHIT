using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public List<ActiveLight> lightBulbList;
    
    
    void Start()
    {
        // Select appropriate light set
    }




    // public SelectAppropriateLights()
    // {
    //     switch (GameMaster.THISLEVEL)
    //     {
    //         case N :
    //         nuNum = "st";
    //         break;
    //         case "2":
    //         nuNum = "nd";
    //         break;
    //         case "3":
    //         nuNum = "rd";
    //         break;
    //     }
    // }
    

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
