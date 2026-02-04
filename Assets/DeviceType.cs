using UnityEngine;
using TMPro;

#if STEAMWORKS_NET
using Steamworks;
#endif

public class DeviceType : MonoBehaviour
{
    public TextMeshProUGUI devText;
    public PlayerDeviceType selectedDeviceType;

    [Header("Mouse/Look Sensitivity Per Device")]
    public float SensitivityForPC = 0.01f;
    public float SensitivityForSteamDeck = 10f;

    private void Start()
    {
        DetectAndApplyDeviceType();
    }

    private void DetectAndApplyDeviceType()
    {
        // 1) Prefer Steamworks' Deck detection (works even if you're a Windows build on Deck via Proton)
#if STEAMWORKS_NET
        if (SteamManager.Initialized)
        {
            bool isDeck = SteamUtils.IsSteamRunningOnSteamDeck(); // ISteamUtils::IsSteamRunningOnSteamDeck :contentReference[oaicite:1]{index=1}
            if (isDeck)
            {
                ApplySteamDeck();
                return;
            }
        }
#endif

        // 2) Fallbacks (native builds, non-Steam launches, editor, etc.)
        // If you ever ship a native Linux build, this can help.
        if (Application.platform == RuntimePlatform.LinuxPlayer)
        {
            // This might be SteamOS, but not necessarily Deck. Keep it separate if you want.
            devText.text = "Linux";
            selectedDeviceType = PlayerDeviceType.DesktopPC;
            SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
        }
        else
        {
            ApplyDesktopPC();
        }

        SetDeviceType();
    }

    private void ApplySteamDeck()
    {
        devText.text = "Steam";
        selectedDeviceType = PlayerDeviceType.SteamOS;
        SetResolution(1280, 800, FullScreenMode.FullScreenWindow);
        SetDeviceType();
    }

    private void ApplyDesktopPC()
    {
        devText.text = "Windows";
        selectedDeviceType = PlayerDeviceType.DesktopPC;
        SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
        SetDeviceType();
    }

    public void SetDeviceType()
    {
        switch (selectedDeviceType)
        {
            case PlayerDeviceType.SteamOS:
                GameMaster.Instance.MouseSensitivity = SensitivityForSteamDeck;
                Debug.Log("set sensitivity for Steam Deck / SteamOS");
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
