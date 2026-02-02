using TMPro;
using UnityEngine;
public class DeviceType : MonoBehaviour
{
    //This is the Text for the Label at the top of the screen
    string m_DeviceType;
    public TextMeshProUGUI devText;
    public PlayerDeviceType selectedDeviceType;

    
    [Header("Mouse Sensitivity Per Device")]
    public float SensitivityForPC = 0.5f;
    public float SensitivityForSteam = 5f;
    
    
    void Awake()
    {
        
        if (SystemInfo.operatingSystem.ToLower().Contains("windows"))
        {
            devText.text = "Windows";
            selectedDeviceType = PlayerDeviceType.DesktopPC;
            SetResolution(1920,1080, FullScreenMode.FullScreenWindow);
        }


        if (SystemInfo.operatingSystem.ToLower().Contains("steam"))
        {
            devText.text = "SteamOS";
            selectedDeviceType = PlayerDeviceType.SteamOS;
            SetResolution(1280,800, FullScreenMode.FullScreenWindow);
        }
        
        SetDeviceType();
        
    }


    public void SetDeviceType()
    {
        switch (selectedDeviceType)
        {
            case PlayerDeviceType.SteamOS:
                GameMaster.Instance.MouseSensitivity = SensitivityForSteam;
                Debug.Log("set sensitivity for SteamOS");
                break;

            case PlayerDeviceType.DesktopPC:
                GameMaster.Instance.MouseSensitivity = SensitivityForPC;
                Debug.Log("set sensitivity for Desktop PC");
                break;
        }
    }
    
    
    public void SetResolution(int width, int height, FullScreenMode mode)
    {
        Screen.SetResolution(width, height, mode);
    }

    
    
}

public enum PlayerDeviceType
{
    SteamOS,
    DesktopPC
}