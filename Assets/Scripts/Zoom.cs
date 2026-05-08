using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Zoom : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float defaultFOV = 60f;
    public float maxZoom = 10f;

    [Range(0f, 1f)]
    public float zoomAmount = 0f;

    public float zoomStep = 0.1f;           // Per tap
    public float continuousZoomSpeed = 0.8f; // Per second while holding (in zoomAmount space)

    [Header("Hold Settings")]
    public float holdThreshold = 0.5f;      // Time before continuous zoom starts

    [Header("Input Actions")]
    public InputActionReference zoomInInput;
    public InputActionReference zoomOutInput;

    public bool zoomAllowed = false;

    [Header("Auto Zoom (Devices)")]
    [Range(0.05f, 1f)] public float pcAutoZoomFactor = 0.40f;
    [Range(0.05f, 1f)] public float phoneAutoZoomFactor = 0.80f;
    public float autoZoomDuration = 0.15f;

    private Camera cam;
    private Coroutine zoomRoutine;
    private Coroutine continuousZoomRoutine;

    // Hold tracking
    private Coroutine holdTimerRoutine;
    private bool isZoomingIn = false;
    private bool isZoomingOut = false;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        EventManager.OnStartComputer += AutoZoomInPC;
        EventManager.OnStopComputer += OnDeviceClosed;
        
        EventManager.OnStartPhone += AutoZoomInPhone;
        EventManager.OnStopPhone += OnDeviceClosed;
        
        EventManager.OnStartNotepad += AutoZoomInPhone;
        EventManager.OnStopNotepad += OnDeviceClosed;
    }

    private void Start()
    {
        EventManager.OnPhoneOpened += SetDefaultFOV;
        SetDefaultFOV();
        StartCoroutine(WaitForGameMaster());
        AttachListeners();
    }
    private IEnumerator WaitForGameMaster()
    {
        while (GameMaster.Instance == null)
            yield return null;

        zoomAllowed = true;
    }
    public void AttachListeners()
    {
        if (zoomInInput != null)
        {
            zoomInInput.action.Enable();
            zoomInInput.action.started += OnZoomInStarted;
            zoomInInput.action.canceled += OnZoomCanceled;
        }

        if (zoomOutInput != null)
        {
            zoomOutInput.action.Enable();
            zoomOutInput.action.started += OnZoomOutStarted;
            zoomOutInput.action.canceled += OnZoomCanceled;
        }
    }

    private void OnDisable()
    {
        if (zoomInInput != null)
        {
            zoomInInput.action.started -= OnZoomInStarted;
            zoomInInput.action.canceled -= OnZoomCanceled;
            zoomInInput.action.Disable();
        }

        if (zoomOutInput != null)
        {
            zoomOutInput.action.started -= OnZoomOutStarted;
            zoomOutInput.action.canceled -= OnZoomCanceled;
            zoomOutInput.action.Disable();
        }
    }

    // ====================== HOLD LOGIC ======================

    private void OnZoomInStarted(InputAction.CallbackContext context)
    {
        if (!CanZoom()) return;

        isZoomingIn = true;
        isZoomingOut = false;

        // Immediate single step
        zoomAmount = Mathf.Clamp01(zoomAmount + zoomStep);
        UpdateFOV();

        // Start hold timer for continuous zoom
        if (holdTimerRoutine != null) StopCoroutine(holdTimerRoutine);
        holdTimerRoutine = StartCoroutine(HoldTimer(true));
    }

    private void OnZoomOutStarted(InputAction.CallbackContext context)
    {
        if (!CanZoom()) return;

        isZoomingOut = true;
        isZoomingIn = false;

        // Immediate single step
        zoomAmount = Mathf.Clamp01(zoomAmount - zoomStep);
        UpdateFOV();

        if (holdTimerRoutine != null) StopCoroutine(holdTimerRoutine);
        holdTimerRoutine = StartCoroutine(HoldTimer(false));
    }

    private void OnZoomCanceled(InputAction.CallbackContext context)
    {
        isZoomingIn = false;
        isZoomingOut = false;

        if (holdTimerRoutine != null)
        {
            StopCoroutine(holdTimerRoutine);
            holdTimerRoutine = null;
        }

        if (continuousZoomRoutine != null)
        {
            StopCoroutine(continuousZoomRoutine);
            continuousZoomRoutine = null;
        }
    }

    private IEnumerator HoldTimer(bool zoomingIn)
    {
        yield return new WaitForSeconds(holdThreshold);

        // If still holding after threshold → start continuous zoom
        if ((zoomingIn && isZoomingIn) || (!zoomingIn && isZoomingOut))
        {
            if (continuousZoomRoutine != null)
                StopCoroutine(continuousZoomRoutine);

            continuousZoomRoutine = StartCoroutine(ContinuousZoom(zoomingIn));
        }
    }

    private IEnumerator ContinuousZoom(bool zoomIn)
    {
        while ((zoomIn && isZoomingIn) || (!zoomIn && isZoomingOut))
        {
            if (zoomIn)
                zoomAmount = Mathf.Clamp01(zoomAmount + continuousZoomSpeed * Time.deltaTime);
            else
                zoomAmount = Mathf.Clamp01(zoomAmount - continuousZoomSpeed * Time.deltaTime);

            UpdateFOV();
            yield return null;
        }
    }

    private bool CanZoom()
    {
        if (!Player.Instance.ZoomOverride)
        {
            if (!zoomAllowed || GameMaster.Instance.PLAYERBUSY || 
                GameMaster.Instance.PauseManager.IsPaused || 
                GameMaster.Instance.TravelCompanion.CompanionOpen)
                return false;
        }
        return true;
    }

    private void UpdateFOV()
    {
        cam.fieldOfView = Mathf.Lerp(defaultFOV, maxZoom, zoomAmount);
    }

    // ====================== EXISTING AUTO ZOOM ======================

    private void SetDefaultFOV()
    {
        if (zoomRoutine != null) StopCoroutine(zoomRoutine);
        if (continuousZoomRoutine != null) StopCoroutine(continuousZoomRoutine);
        if (holdTimerRoutine != null) StopCoroutine(holdTimerRoutine);

        zoomAmount = 0f;
        UpdateFOV();
    }

    public void AutoZoomInPC(Transform pcTransform) => StartAutoZoom(defaultFOV * pcAutoZoomFactor);
    public void AutoZoomInPhone(Transform phoneTransform) => StartAutoZoom(defaultFOV * phoneAutoZoomFactor);

    private void OnDeviceClosed() => StartAutoZoom(defaultFOV);

    private void StartAutoZoom(float targetFOV)
    {
        if (zoomRoutine != null) StopCoroutine(zoomRoutine);
        if (continuousZoomRoutine != null) StopCoroutine(continuousZoomRoutine);
        if (holdTimerRoutine != null) StopCoroutine(holdTimerRoutine);

        zoomRoutine = StartCoroutine(SmoothFOV_Unscaled(targetFOV, autoZoomDuration));
    }

    private IEnumerator SmoothFOV_Unscaled(float targetFOV, float duration)
    {
        float startFOV = cam.fieldOfView;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / duration);

            cam.fieldOfView = Mathf.Lerp(startFOV, targetFOV, a);
            yield return null;
        }

        cam.fieldOfView = targetFOV;
        zoomAmount = Mathf.InverseLerp(defaultFOV, maxZoom, cam.fieldOfView);
        zoomRoutine = null;
    }

    private void OnDestroy()
    {
        EventManager.OnStartComputer -= AutoZoomInPC;
        EventManager.OnStopComputer -= OnDeviceClosed;
        EventManager.OnStartPhone -= AutoZoomInPhone;
        EventManager.OnStopPhone -= OnDeviceClosed;
        EventManager.OnStartNotepad -= AutoZoomInPhone;
        EventManager.OnStopNotepad -= OnDeviceClosed;
        EventManager.OnPhoneOpened -= SetDefaultFOV;
    }
}