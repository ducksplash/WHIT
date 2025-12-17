using UnityEngine;

public class LightSwitch : MonoBehaviour
{
    public LightBulbID selectedLight;


    public void ToggleLightswitch()
    {
        GameMaster.Instance.EventManager.ClickedLightSwitch(selectedLight);
    }
}
