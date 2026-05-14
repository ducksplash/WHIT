using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Zoom : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float defaultFOV = 60f;
    public float maxZoom = 10f;

    [Tooltip("Entering below 0 enables third person")]
    public float thirdPersonMinZoom = -0.5f;

    [Tooltip("Maximum third person zoom out")]
    public float thirdPersonMaxZoom = -1.5f;

    [Range(-2f, 1f)]
    public float zoomAmount = 0f;

    public float zoomStep = 0.1f;
    public float continuousZoomSpeed = 0.8f;

    [Header("Hold Settings")]
    public float holdThreshold = 0.5f;

    [Header("Cameras")]
    public Camera FirstPersonCamera;
    public Camera ThirdPersonCamera;

    [Header("Input Actions")]
    public InputActionReference zoomInInput;
    public InputActionReference zoomOutInput;

    [Header("Third Person FOV")]
    public float thirdPersonMinFOV = 60f;

    [Tooltip("FOV when fully zoomed out in third person")]
    public float thirdPersonMaxFOV = 85f;

    [Header("Auto Zoom (Devices)")]
    [Range(0.05f, 1f)] public float pcAutoZoomFactor = 0.40f;
    [Range(0.05f, 1f)] public float phoneAutoZoomFactor = 0.80f;
    public float autoZoomDuration = 0.15f;

    public bool zoomAllowed = false;

    private Coroutine zoomRoutine;
    private Coroutine continuousZoomRoutine;
    private Coroutine holdTimerRoutine;

    private bool isZoomingIn;
    private bool isZoomingOut;

    private bool _autoZoomActive;
    private bool _forceFirstPersonMode;

    public bool IsThirdPersonActive { get; private set; }

    private bool IsInteractionLocked =>
        _autoZoomActive || _forceFirstPersonMode;

    private void Awake()
    {
        if (FirstPersonCamera == null)
            FirstPersonCamera = GetComponent<Camera>();

        EventManager.OnStartComputer += AutoZoomInPC;
        EventManager.OnStopComputer += OnDeviceClosed;

        EventManager.OnStartPhone += AutoZoomInPhone;
        EventManager.OnStopPhone += OnDeviceClosed;

        EventManager.OnStartNotepad += AutoZoomInPhone;
        EventManager.OnStopNotepad += OnDeviceClosed;

        EventManager.OnCrouch += OnCrouch;
        EventManager.OnUnCrouch += OnUncrouch;
        
    }
    private void OnCrouch()
    {
        _forceFirstPersonMode = true;

        zoomAmount = 0f; // immediately force FP state
        UpdateCameraAndFOV();
    }

    private void OnUncrouch()
    {
        _forceFirstPersonMode = false;
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
        DetachListeners();
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
        DetachListeners();
    }

    private void DetachListeners()
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

    public void ReportCameraCloseness(float closeness) { }

    private void OnZoomInStarted(InputAction.CallbackContext context)
    {
        if (IsInteractionLocked) return;
        if (!CanZoom()) return;

        isZoomingIn = true;
        isZoomingOut = false;

        zoomAmount = Mathf.Clamp(
            zoomAmount + zoomStep,
            thirdPersonMaxZoom,
            1f
        );

        if (holdTimerRoutine != null)
            StopCoroutine(holdTimerRoutine);

        holdTimerRoutine = StartCoroutine(HoldTimer(true));
    }

    private void OnZoomOutStarted(InputAction.CallbackContext context)
    {
        if (IsInteractionLocked) return;
        if (!CanZoom()) return;

        isZoomingIn = false;
        isZoomingOut = true;

        zoomAmount = Mathf.Clamp(
            zoomAmount - zoomStep,
            thirdPersonMaxZoom,
            1f
        );

        if (holdTimerRoutine != null)
            StopCoroutine(holdTimerRoutine);

        holdTimerRoutine = StartCoroutine(HoldTimer(false));
    }

    private void OnZoomCanceled(InputAction.CallbackContext context)
    {
        isZoomingIn = false;
        isZoomingOut = false;

        if (holdTimerRoutine != null)
            StopCoroutine(holdTimerRoutine);

        if (continuousZoomRoutine != null)
            StopCoroutine(continuousZoomRoutine);
    }

    private IEnumerator HoldTimer(bool zoomingIn)
    {
        yield return new WaitForSeconds(holdThreshold);

        if ((zoomingIn && isZoomingIn) ||
            (!zoomingIn && isZoomingOut))
        {
            if (continuousZoomRoutine != null)
                StopCoroutine(continuousZoomRoutine);

            continuousZoomRoutine =
                StartCoroutine(ContinuousZoom(zoomingIn));
        }
    }

    private IEnumerator ContinuousZoom(bool zoomIn)
    {
        while ((zoomIn && isZoomingIn) ||
               (!zoomIn && isZoomingOut))
        {
            if (zoomIn)
            {
                zoomAmount = Mathf.Clamp(
                    zoomAmount + continuousZoomSpeed * Time.deltaTime,
                    thirdPersonMaxZoom,
                    1f
                );
            }
            else
            {
                zoomAmount = Mathf.Clamp(
                    zoomAmount - continuousZoomSpeed * Time.deltaTime,
                    thirdPersonMaxZoom,
                    1f
                );
            }

            yield return null;
        }
    }

    private bool CanZoom()
    {
        if (!Player.Instance.ZoomOverride)
        {
            if (!zoomAllowed ||
                GameMaster.Instance.PLAYERBUSY ||
                GameMaster.Instance.PauseManager.IsPaused ||
                GameMaster.Instance.TravelCompanion.CompanionOpen)
            {
                return false;
            }
        }

        return true;
    }

    private void Update()
    {
        if (IsInteractionLocked)
            return;

        UpdateCameraAndFOV();
    }

    private void UpdateCameraAndFOV()
    {
        if (_forceFirstPersonMode)
        {
            IsThirdPersonActive = false;
            zoomAmount = 0f;
        }
        else
        {
            IsThirdPersonActive = zoomAmount < 0f;
        }

        if (FirstPersonCamera != null)
            FirstPersonCamera.enabled = !IsThirdPersonActive;

        if (ThirdPersonCamera != null)
            ThirdPersonCamera.enabled = IsThirdPersonActive;

        if (!IsThirdPersonActive && FirstPersonCamera != null)
        {
            float fpT = Mathf.Clamp01(zoomAmount);

            FirstPersonCamera.fieldOfView =
                Mathf.Lerp(defaultFOV, maxZoom, fpT);
        }

        if (IsThirdPersonActive && ThirdPersonCamera != null)
        {
            float tpT =
                Mathf.InverseLerp(0f, thirdPersonMaxZoom, zoomAmount);

            ThirdPersonCamera.fieldOfView =
                Mathf.Lerp(thirdPersonMinFOV, thirdPersonMaxFOV, tpT);
        }
    }

    private void SetDefaultFOV()
    {
        StopAllCoroutines();
        zoomAmount = 0f;
        UpdateCameraAndFOV();
    }

    public void AutoZoomInPC(Transform pcTransform)
    {
        EnterInteractionMode(defaultFOV * pcAutoZoomFactor);
    }

    public void AutoZoomInPhone(Transform phoneTransform)
    {
        EnterInteractionMode(defaultFOV * phoneAutoZoomFactor);
    }

    private void OnDeviceClosed()
    {
        ExitInteractionMode();
    }

    private void EnterInteractionMode(float targetFOV)
    {
        StopAllCoroutines();

        _autoZoomActive = true;
        _forceFirstPersonMode = true;

        zoomAmount = 0f;

        if (ThirdPersonCamera != null)
            ThirdPersonCamera.enabled = false;

        if (FirstPersonCamera != null)
            FirstPersonCamera.enabled = true;

        StartCoroutine(SmoothFOV(targetFOV, autoZoomDuration));
    }

    private void ExitInteractionMode()
    {
        StopAllCoroutines();

        _autoZoomActive = false;
        _forceFirstPersonMode = false;

        zoomAmount = 0f;

        UpdateCameraAndFOV();
    }

    private IEnumerator SmoothFOV(float targetFOV, float duration)
    {
        float startFOV =
            FirstPersonCamera != null
                ? FirstPersonCamera.fieldOfView
                : defaultFOV;

        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;

            float a = Mathf.Clamp01(t / duration);

            if (FirstPersonCamera != null)
            {
                FirstPersonCamera.fieldOfView =
                    Mathf.Lerp(startFOV, targetFOV, a);
            }

            yield return null;
        }

        if (FirstPersonCamera != null)
            FirstPersonCamera.fieldOfView = targetFOV;

        _autoZoomActive = false;
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