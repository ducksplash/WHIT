using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if STEAMWORKS_NET
using Steamworks;
#endif

public class SteamInputHapticsTester : MonoBehaviour
{
    [Header("Buttons")]
    public Button btnSimpleHapticLeft;
    public Button btnSimpleHapticRight;
    public Button btnSimpleHapticBoth;
    public Button btnSimpleHapticBothSweep;

    public Button btnRumble;                 // sustained
    public Button btnRumbleExtended;         // sustained (all channels)
    public Button btnRumbleBurst;            // punchy impact-style pulses
    public Button btnStopAll;

    [Header("Sliders")]
    [Tooltip("How long to sustain the effect in milliseconds.")]
    public Slider sliderDurationMs;

    [Tooltip("SimpleHaptic: main intensity (0..255).")]
    public Slider sliderIntensity;

    [Tooltip("SimpleHaptic: other intensity (0..255).")]
    public Slider sliderOtherIntensity;

    [Tooltip("SimpleHaptic: main gain dB. IMPORTANT: Steam Deck max is +12. Set slider max to 12.")]
    public Slider sliderGainDb;

    [Tooltip("SimpleHaptic: other gain dB. IMPORTANT: Steam Deck max is +12. Set slider max to 12.")]
    public Slider sliderOtherGainDb;

    [Tooltip("Rumble speed (0..65535).")]
    public Slider sliderRumbleSpeed;

    [Tooltip("Rumble burst ON time in ms.")]
    public Slider sliderBurstOnMs;

    [Tooltip("Rumble burst OFF time in ms.")]
    public Slider sliderBurstOffMs;

    [Tooltip("Rumble burst pulse count.")]
    public Slider sliderBurstCount;

    [Header("Drive Options")]
    [Tooltip("Re-fire interval. 0.01 = 100Hz.")]
    [Range(0.002f, 0.05f)]
    public float driveTickSeconds = 0.01f;

    [Tooltip("Call SteamInput.RunFrame() each Update. Recommended true.")]
    public bool callRunFrameEachUpdate = true;

    [Header("TextMeshPro Readouts")]
    public TMP_Text txtStatus;
    public TMP_Text txtValues;

    Coroutine _driveRoutine;

#if STEAMWORKS_NET
    bool _steamApiReady;
    bool _steamInputReady;
    InputHandle_t _handle;
#endif

    void Awake()
    {
        if (btnSimpleHapticLeft) btnSimpleHapticLeft.onClick.AddListener(Test_SimpleHaptic_Left);
        if (btnSimpleHapticRight) btnSimpleHapticRight.onClick.AddListener(Test_SimpleHaptic_Right);
        if (btnSimpleHapticBoth) btnSimpleHapticBoth.onClick.AddListener(Test_SimpleHaptic_Both);
        if (btnSimpleHapticBothSweep) btnSimpleHapticBothSweep.onClick.AddListener(Test_SimpleHaptic_Both_Sweep);

        if (btnRumble) btnRumble.onClick.AddListener(Test_Rumble);
        if (btnRumbleExtended) btnRumbleExtended.onClick.AddListener(Test_RumbleExtended);
        if (btnRumbleBurst) btnRumbleBurst.onClick.AddListener(Test_RumbleBurst);

        if (btnStopAll) btnStopAll.onClick.AddListener(StopAllHaptics);

        Hook(sliderDurationMs);
        Hook(sliderIntensity);
        Hook(sliderOtherIntensity);
        Hook(sliderGainDb);
        Hook(sliderOtherGainDb);
        Hook(sliderRumbleSpeed);
        Hook(sliderBurstOnMs);
        Hook(sliderBurstOffMs);
        Hook(sliderBurstCount);
    }

    void OnEnable()
    {
        RefreshText();
        InitSteam();
    }

    void OnDisable()
    {
        StopAllHaptics();
        ShutdownSteam();
    }

    void Update()
    {
#if STEAMWORKS_NET
        if (!_steamApiReady) return;

        SteamAPI.RunCallbacks();

        if (callRunFrameEachUpdate && _steamInputReady)
            SteamInput.RunFrame();
#endif
    }

    // ============================================================
    // Public test methods
    // ============================================================

    public void Test_SimpleHaptic_Left()
    {
#if STEAMWORKS_NET
        if (!EnsureHandle(out var h)) return;
        StartDrive(h, Mode.SimpleLeft);
#else
        SetStatus("STEAMWORKS_NET not defined.");
#endif
    }

    public void Test_SimpleHaptic_Right()
    {
#if STEAMWORKS_NET
        if (!EnsureHandle(out var h)) return;
        StartDrive(h, Mode.SimpleRight);
#else
        SetStatus("STEAMWORKS_NET not defined.");
#endif
    }

    public void Test_SimpleHaptic_Both()
    {
#if STEAMWORKS_NET
        if (!EnsureHandle(out var h)) return;
        StartDrive(h, Mode.SimpleBoth);
#else
        SetStatus("STEAMWORKS_NET not defined.");
#endif
    }

    public void Test_SimpleHaptic_Both_Sweep()
    {
#if STEAMWORKS_NET
        if (!EnsureHandle(out var h)) return;
        StopDrive();

        float dur = DurSec();
        SetStatus($"SimpleHaptic SWEEP dur={dur:0.00}s tick={driveTickSeconds:0.000}s");
        _driveRoutine = StartCoroutine(SimpleSweepRoutine(h, dur));
#else
        SetStatus("STEAMWORKS_NET not defined.");
#endif
    }

    public void Test_Rumble()
    {
#if STEAMWORKS_NET
        if (!EnsureHandle(out var h)) return;
        StartDrive(h, Mode.Rumble);
#else
        SetStatus("STEAMWORKS_NET not defined.");
#endif
    }

    public void Test_RumbleExtended()
    {
#if STEAMWORKS_NET
        if (!EnsureHandle(out var h)) return;
        StartDrive(h, Mode.RumbleExtended);
#else
        SetStatus("STEAMWORKS_NET not defined.");
#endif
    }

    public void Test_RumbleBurst()
    {
#if STEAMWORKS_NET
        if (!EnsureHandle(out var h)) return;
        StopDrive();

        ushort speed = ToUShort(sliderRumbleSpeed, 65535);
        float on = Mathf.Max(0f, (sliderBurstOnMs ? sliderBurstOnMs.value : 25f) / 1000f);
        float off = Mathf.Max(0f, (sliderBurstOffMs ? sliderBurstOffMs.value : 10f) / 1000f);
        int count = Mathf.Clamp(Mathf.RoundToInt(sliderBurstCount ? sliderBurstCount.value : 3f), 1, 50);

        SetStatus($"Rumble BURST speed={speed} on={on*1000f:0}ms off={off*1000f:0}ms x{count}");
        RefreshText();
        _driveRoutine = StartCoroutine(RumbleBurstRoutine(h, speed, on, off, count));
#else
        SetStatus("STEAMWORKS_NET not defined.");
#endif
    }

    public void StopAllHaptics()
    {
        StopDrive();

#if STEAMWORKS_NET
        if (_steamInputReady && _handle.m_InputHandle != 0)
        {
            SteamInput.TriggerVibration(_handle, 0, 0);
            SteamInput.TriggerVibrationExtended(_handle, 0, 0, 0, 0);

            // Send "off" events (with correctly packed 0 gain)
            SteamInput.TriggerSimpleHapticEvent(_handle, EControllerHapticLocation.k_EControllerHapticLocation_Left, 0, PackGainDb(0), 0, PackGainDb(0));
            SteamInput.TriggerSimpleHapticEvent(_handle, EControllerHapticLocation.k_EControllerHapticLocation_Right, 0, PackGainDb(0), 0, PackGainDb(0));
        }
#endif

        SetStatus("Stopped.");
    }

    // ============================================================
    // Drive coroutine
    // ============================================================

#if STEAMWORKS_NET
    enum Mode { SimpleLeft, SimpleRight, SimpleBoth, Rumble, RumbleExtended }

    void StartDrive(InputHandle_t h, Mode mode)
    {
        StopDrive();

        float dur = DurSec();

        byte intensity = ToByte(sliderIntensity, 255);
        byte otherIntensity = ToByte(sliderOtherIntensity, intensity);

        // IMPORTANT: Deck max gain is +12. We hard clamp here so slider max truly maps to +12.
        sbyte gainDb = ToSByteClampedToDeck(sliderGainDb, 0);
        sbyte otherGainDb = ToSByteClampedToDeck(sliderOtherGainDb, gainDb);

        ushort rumbleSpeed = ToUShort(sliderRumbleSpeed, 65535);

        string devInfo = GetDeviceInfoString(h);

        SetStatus($"{mode}: i={intensity} g={gainDb} otherI={otherIntensity} otherG={otherGainDb} rumble={rumbleSpeed} dur={dur:0.00}s tick={driveTickSeconds:0.000}s | {devInfo}");
        RefreshText();

        _driveRoutine = StartCoroutine(DriveRoutine(
            h, mode,
            intensity, PackGainDb(gainDb),
            otherIntensity, PackGainDb(otherGainDb),
            rumbleSpeed, dur
        ));
    }

    IEnumerator DriveRoutine(
        InputHandle_t h,
        Mode mode,
        byte intensity,
        char gainDbPacked,
        byte otherIntensity,
        char otherGainDbPacked,
        ushort rumbleSpeed,
        float duration)
    {
        float end = Time.realtimeSinceStartup + duration;

        while (Time.realtimeSinceStartup < end)
        {
            switch (mode)
            {
                case Mode.SimpleLeft:
                    SteamInput.TriggerSimpleHapticEvent(
                        h, EControllerHapticLocation.k_EControllerHapticLocation_Left,
                        intensity, gainDbPacked, otherIntensity, otherGainDbPacked);
                    break;

                case Mode.SimpleRight:
                    SteamInput.TriggerSimpleHapticEvent(
                        h, EControllerHapticLocation.k_EControllerHapticLocation_Right,
                        intensity, gainDbPacked, otherIntensity, otherGainDbPacked);
                    break;

                case Mode.SimpleBoth:
                    SteamInput.TriggerSimpleHapticEvent(
                        h, EControllerHapticLocation.k_EControllerHapticLocation_Left,
                        intensity, gainDbPacked, otherIntensity, otherGainDbPacked);

                    SteamInput.TriggerSimpleHapticEvent(
                        h, EControllerHapticLocation.k_EControllerHapticLocation_Right,
                        intensity, gainDbPacked, otherIntensity, otherGainDbPacked);
                    break;

                case Mode.Rumble:
                    // Dedicated rumble path (separate from simple haptics)
                    SteamInput.TriggerVibration(h, rumbleSpeed, rumbleSpeed);
                    break;

                case Mode.RumbleExtended:
                    SteamInput.TriggerVibrationExtended(h, rumbleSpeed, rumbleSpeed, rumbleSpeed, rumbleSpeed);
                    break;
            }

            yield return new WaitForSecondsRealtime(driveTickSeconds);
        }

        // Silence everything on exit.
        SteamInput.TriggerVibration(h, 0, 0);
        SteamInput.TriggerVibrationExtended(h, 0, 0, 0, 0);

        SteamInput.TriggerSimpleHapticEvent(h, EControllerHapticLocation.k_EControllerHapticLocation_Left, 0, PackGainDb(0), 0, PackGainDb(0));
        SteamInput.TriggerSimpleHapticEvent(h, EControllerHapticLocation.k_EControllerHapticLocation_Right, 0, PackGainDb(0), 0, PackGainDb(0));

        _driveRoutine = null;
        SetStatus("Done.");
    }

    IEnumerator SimpleSweepRoutine(InputHandle_t h, float duration)
    {
        float end = Time.realtimeSinceStartup + duration;

        while (Time.realtimeSinceStartup < end)
        {
            float t = 1f - Mathf.Clamp01((end - Time.realtimeSinceStartup) / duration);

            byte intensity = (byte)Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(64, 255, t)), 0, 255);

            // Sweep gain from -20 to +12 (Deck max) - no normalization.
            sbyte gain = (sbyte)Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(-20f, 12f, t)), -128, 12);
            char packed = PackGainDb(gain);

            SteamInput.TriggerSimpleHapticEvent(h, EControllerHapticLocation.k_EControllerHapticLocation_Left, intensity, packed, intensity, packed);
            SteamInput.TriggerSimpleHapticEvent(h, EControllerHapticLocation.k_EControllerHapticLocation_Right, intensity, packed, intensity, packed);

            yield return new WaitForSecondsRealtime(driveTickSeconds);
        }

        SteamInput.TriggerSimpleHapticEvent(h, EControllerHapticLocation.k_EControllerHapticLocation_Left, 0, PackGainDb(0), 0, PackGainDb(0));
        SteamInput.TriggerSimpleHapticEvent(h, EControllerHapticLocation.k_EControllerHapticLocation_Right, 0, PackGainDb(0), 0, PackGainDb(0));

        _driveRoutine = null;
        SetStatus("Sweep done.");
    }

    IEnumerator RumbleBurstRoutine(InputHandle_t h, ushort speed, float on, float off, int count)
    {
        for (int i = 0; i < count; i++)
        {
            SteamInput.TriggerVibration(h, speed, speed);
            if (on > 0f) yield return new WaitForSecondsRealtime(on);

            SteamInput.TriggerVibration(h, 0, 0);
            if (off > 0f) yield return new WaitForSecondsRealtime(off);
        }

        _driveRoutine = null;
        SetStatus("Burst done.");
    }

    void StopDrive()
    {
        if (_driveRoutine != null)
        {
            StopCoroutine(_driveRoutine);
            _driveRoutine = null;
        }
    }

    // Packs signed int8 into Steamworks.NET char param.
    static char PackGainDb(sbyte gainDb)
    {
        unchecked
        {
            byte b = (byte)gainDb; // preserves int8 bits
            return (char)b;
        }
    }

    string GetDeviceInfoString(InputHandle_t h)
    {
        try
        {
            var t = SteamInput.GetInputTypeForHandle(h);
            int gamepadIndex = SteamInput.GetGamepadIndexForController(h);
            return $"type={t} gamepadIndex={gamepadIndex}";
        }
        catch
        {
            return "type=? gamepadIndex=?";
        }
    }
#else
    void StopDrive() { }
#endif

    // ============================================================
    // Steam init / shutdown / handle
    // ============================================================

    void InitSteam()
    {
#if STEAMWORKS_NET
        try
        {
            _steamApiReady = SteamAPI.Init();
            if (!_steamApiReady)
            {
                SetStatus("SteamAPI.Init failed (run via Steam or steam_appid.txt).");
                return;
            }

            _steamInputReady = SteamInput.Init(false);
            if (!_steamInputReady)
            {
                SetStatus("SteamInput.Init failed.");
                return;
            }

            SteamInput.RunFrame(); // required before GetConnectedControllers returns handles
            CacheHandle();

            SetStatus($"SteamInput ready. Handle={_handle.m_InputHandle} {( _handle.m_InputHandle != 0 ? GetDeviceInfoString(_handle) : "" )}");
        }
        catch (System.Exception e)
        {
            SetStatus("Steam init error: " + e.Message);
        }
#else
        SetStatus("STEAMWORKS_NET not defined.");
#endif
    }

    void ShutdownSteam()
    {
#if STEAMWORKS_NET
        try
        {
            if (_steamInputReady) { SteamInput.Shutdown(); _steamInputReady = false; }
            if (_steamApiReady) { SteamAPI.Shutdown(); _steamApiReady = false; }
        }
        catch { }
#endif
    }

#if STEAMWORKS_NET
    void CacheHandle()
    {
        _handle = new InputHandle_t(0);

        var handles = new InputHandle_t[Constants.STEAM_INPUT_MAX_COUNT];
        int count = SteamInput.GetConnectedControllers(handles);

        if (count > 0)
            _handle = handles[0];
    }

    bool EnsureHandle(out InputHandle_t handle)
    {
        handle = new InputHandle_t(0);

        if (!_steamApiReady || !_steamInputReady)
        {
            SetStatus("SteamInput not ready.");
            return false;
        }

        CacheHandle();

        if (_handle.m_InputHandle == 0)
        {
            SetStatus("No Steam Input controller found (launch via Steam on Deck).");
            return false;
        }

        handle = _handle;
        return true;
    }
#endif

    // ============================================================
    // Slider helpers (NO NORMALIZATION)
    // ============================================================

    float DurSec()
    {
        float ms = sliderDurationMs ? sliderDurationMs.value : 500f;
        return Mathf.Clamp(ms / 1000f, 0f, 10f);
    }

    static ushort ToUShort(Slider s, ushort fallback)
    {
        if (!s) return fallback;
        return (ushort)Mathf.Clamp(Mathf.RoundToInt(s.value), 0, 65535);
    }

    static byte ToByte(Slider s, byte fallback)
    {
        if (!s) return fallback;
        return (byte)Mathf.Clamp(Mathf.RoundToInt(s.value), 0, 255);
    }

    // Deck-specific clamp: [-128 .. +12]
    static sbyte ToSByteClampedToDeck(Slider s, sbyte fallback)
    {
        if (!s) return fallback;
        int v = Mathf.RoundToInt(s.value);
        v = Mathf.Clamp(v, -128, 12);
        return (sbyte)v;
    }

    void Hook(Slider s)
    {
        if (s) s.onValueChanged.AddListener(_ => RefreshText());
    }

    void RefreshText()
    {
        if (!txtValues) return;

        byte intensity = ToByte(sliderIntensity, 255);
        byte otherIntensity = ToByte(sliderOtherIntensity, intensity);
        sbyte gain = ToSByteClampedToDeck(sliderGainDb, 0);
        sbyte otherGain = ToSByteClampedToDeck(sliderOtherGainDb, gain);
        ushort rumble = ToUShort(sliderRumbleSpeed, 65535);

        float onMs = sliderBurstOnMs ? sliderBurstOnMs.value : 25f;
        float offMs = sliderBurstOffMs ? sliderBurstOffMs.value : 10f;
        int count = Mathf.Clamp(Mathf.RoundToInt(sliderBurstCount ? sliderBurstCount.value : 3f), 1, 50);

        txtValues.text =
            $"Duration: {(sliderDurationMs ? sliderDurationMs.value : 500f):0} ms\n" +
            $"Simple: intensity={intensity} otherIntensity={otherIntensity}\n" +
            $"Gain: {gain} dB (max +12)  OtherGain: {otherGain} dB (max +12)\n" +
            $"RumbleSpeed: {rumble}\n" +
            $"Burst: on={onMs:0}ms off={offMs:0}ms x{count}\n" +
            $"tick={driveTickSeconds:0.000}s RunFrame={callRunFrameEachUpdate}";
    }

    void SetStatus(string msg)
    {
        if (txtStatus) txtStatus.text = msg;
        Debug.Log("[SteamInputHapticsTester] " + msg);
    }
}
