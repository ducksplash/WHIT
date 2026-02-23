using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Zoom : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float defaultFOV = 60f;

    // NOTE: This is the "zoomed" FOV used by scroll zoom (not the auto-zoom target).
    public float maxZoom = 10f;

    [Range(0f, 1f)]
    public float zoomAmount = 0f;

    public float zoomStep = 0.1f; // Amount to change per input press

    [Header("Input Actions")]
    public InputActionReference zoomInInput;  // Zoom in (scroll up / trigger)
    public InputActionReference zoomOutInput; // Zoom out (scroll down / trigger)

    public bool zoomAllowed = false;

    [Header("Auto Zoom (Devices)")]
    [Tooltip("When looking at a PC device, target FOV will be defaultFOV * this factor (smaller = more zoom).")]
    [Range(0.05f, 1f)]
    public float pcAutoZoomFactor = 0.40f;

    [Tooltip("Phone auto-zoom should be milder. Around 1/3 of PC zoom intensity.")]
    [Range(0.05f, 1f)]
    public float phoneAutoZoomFactor = 0.80f;

    [Tooltip("Seconds to blend FOV when auto-zooming.")]
    public float autoZoomDuration = 0.15f;

    private Camera cam;
    private Coroutine zoomRoutine;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        EventManager.OnStartComputer += AutoZoomInPC;
        EventManager.OnStopComputer += OnDeviceClosed;

        EventManager.OnStartPhone += AutoZoomInPhone;
        EventManager.OnStopPhone += OnDeviceClosed;
    }

    private void OnDestroy()
    {
        // Prevent duplicate subscriptions / multiple zoom scripts fighting.
        EventManager.OnStartComputer -= AutoZoomInPC;
        EventManager.OnStopComputer -= OnDeviceClosed;

        EventManager.OnStartPhone -= AutoZoomInPhone;
        EventManager.OnStopPhone -= OnDeviceClosed;

        EventManager.OnPhoneOpened -= SetDefaultFOV;
    }

    private void Start()
    {
        EventManager.OnPhoneOpened += SetDefaultFOV;

        SetDefaultFOV();
        StartCoroutine(WaitForGameMaster());
        AttachListeners();
    }

    private void OnEnable()
    {
        AttachListeners();
    }

    public void AttachListeners()
    {
        if (zoomInInput != null)
        {
            zoomInInput.action.Enable();
            zoomInInput.action.performed -= OnZoomInPerformed;
            zoomInInput.action.performed += OnZoomInPerformed;
        }

        if (zoomOutInput != null)
        {
            zoomOutInput.action.Enable();
            zoomOutInput.action.performed -= OnZoomOutPerformed;
            zoomOutInput.action.performed += OnZoomOutPerformed;
        }
    }

    private void OnDisable()
    {
        if (zoomInInput != null)
        {
            zoomInInput.action.performed -= OnZoomInPerformed;
            zoomInInput.action.Disable();
        }

        if (zoomOutInput != null)
        {
            zoomOutInput.action.performed -= OnZoomOutPerformed;
            zoomOutInput.action.Disable();
        }
    }

    // Hard reset (immediate)
    private void SetDefaultFOV()
    {
        if (zoomRoutine != null) StopCoroutine(zoomRoutine);
        zoomRoutine = null;

        zoomAmount = 0f;
        UpdateFOV();
    }

    private void OnZoomInPerformed(InputAction.CallbackContext context)
    {
        if (!Player.Instance.ZoomOverride)
        {
            if (!zoomAllowed || GameMaster.Instance.PLAYERBUSY || GameMaster.Instance.PauseManager.IsPaused || GameMaster.Instance.TravelCompanion.CompanionOpen)
                return;
        }

        zoomAmount = Mathf.Clamp01(zoomAmount + zoomStep);
        UpdateFOV();
    }

    private void OnZoomOutPerformed(InputAction.CallbackContext context)
    {
        if (!Player.Instance.ZoomOverride)
        {
            if (!zoomAllowed || GameMaster.Instance.PLAYERBUSY || GameMaster.Instance.PauseManager.IsPaused || GameMaster.Instance.TravelCompanion.CompanionOpen)
                return;
        }

        zoomAmount = Mathf.Clamp01(zoomAmount - zoomStep);
        UpdateFOV();
    }

    private void UpdateFOV()
    {
        cam.fieldOfView = Mathf.Lerp(defaultFOV, maxZoom, zoomAmount);
    }

    private IEnumerator WaitForGameMaster()
    {
        while (GameMaster.Instance == null)
            yield return null;

        zoomAllowed = true;
    }

    // ------------------------------------------------------------
    // ✅ Auto Zoom handlers
    // ------------------------------------------------------------

    public void AutoZoomInPC(Transform pcTransform)
    {
        if (!zoomAllowed) return;

        float targetFOV = defaultFOV * pcAutoZoomFactor;
        StartAutoZoomTo(targetFOV);
    }

    public void AutoZoomInPhone(Transform phoneTransform)
    {
        if (!zoomAllowed) return;

        float targetFOV = defaultFOV * phoneAutoZoomFactor;
        StartAutoZoomTo(targetFOV);
    }

    // Called by stop events
    private void OnDeviceClosed()
    {
        // Always revert to default when closing a device.
        // IMPORTANT: do this with unscaled time so it works during pause/timeScale=0.
        if (!zoomAllowed) return;

        StartAutoZoomTo(defaultFOV);
    }

    private void StartAutoZoomTo(float targetFOV)
    {
        if (zoomRoutine != null) StopCoroutine(zoomRoutine);
        zoomRoutine = StartCoroutine(SmoothFOV_Unscaled(targetFOV, autoZoomDuration));
    }

    // ✅ Uses unscaled time so device close can restore FOV even if the game is paused.
    private IEnumerator SmoothFOV_Unscaled(float targetFOV, float duration)
    {
        float startFOV = cam.fieldOfView;

        if (duration <= 0f)
        {
            cam.fieldOfView = targetFOV;
            zoomAmount = Mathf.InverseLerp(defaultFOV, maxZoom, cam.fieldOfView);
            zoomRoutine = null;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / duration);

            cam.fieldOfView = Mathf.Lerp(startFOV, targetFOV, a);
            yield return null;
        }

        cam.fieldOfView = targetFOV;

        // Keep scroll zoom in sync
        zoomAmount = Mathf.InverseLerp(defaultFOV, maxZoom, cam.fieldOfView);

        zoomRoutine = null;
    }
}