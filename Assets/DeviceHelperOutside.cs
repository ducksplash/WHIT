using UnityEngine;
using TMPro;

#if STEAMWORKS_NET
using Steamworks;
#endif

public class DeviceHelperOutside : MonoBehaviour
{
    public HelperDeviceType selectedDeviceType;

    [Header("Mouse/Look Sensitivity Per Device")]
    public float SensitivityForPC = 0.02f;
    public float SensitivityForSteamDeck = 1.5f;
    public float returnableSensitivity;

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
            selectedDeviceType = HelperDeviceType.DesktopPC;
            SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
        }
        else
        {
            ApplyDesktopPC();
        }

    }

    private void ApplySteamDeck()
    {
        selectedDeviceType = HelperDeviceType.SteamOS;
        returnableSensitivity = SensitivityForSteamDeck;
        SetResolution(1280, 800, FullScreenMode.FullScreenWindow);
    }

    private void ApplyDesktopPC()
    {
        selectedDeviceType = HelperDeviceType.DesktopPC;
        returnableSensitivity = SensitivityForPC;
        SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
    }


    public void SetResolution(int width, int height, FullScreenMode mode)
    {
        Screen.SetResolution(width, height, mode);
    }
}

public enum HelperDeviceType
{
    SteamOS,
    DesktopPC
}
