using UnityEngine;
using UnityEngine.UI;

public class OSDButton : MonoBehaviour
{
    public PhoneApps ThisApp;
    public GameObject AppOutline;
    public Button AppButton;

    
    public void ExecuteCommand()
    {
        if (Player.Instance.PlayerPhone.HomeScreen.GetComponentInChildren<CanvasGroup>().alpha > 0.1f)
        {
            Player.Instance.PlayerPhone.OpenApp(ThisApp);
        }
    }
}
