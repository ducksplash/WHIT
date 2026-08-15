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

    [Header("Zoom Step Settings")]
    public float firstPersonZoomStep = 0.1f;
    public float thirdPersonZoomStep = 0.25f;

    [Header("Continuous Zoom Speeds")]
    public float firstPersonContinuousZoomSpeed = 0.8f;
    public float thirdPersonContinuousZoomSpeed = 1.5f;

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

    [Header("Anti-Overshoot Protection")]
    public float tpToFpIgnoreTime = 1.0f;
    public float fpToTpIgnoreTime = 0.5f;

    public bool zoomAllowed = false;

    private Coroutine continuousZoomRoutine;
    private Coroutine holdTimerRoutine;

    private bool isZoomingIn;
    private bool isZoomingOut;

    private bool _autoZoomActive;
    private bool _forceFirstPersonMode;

    private float _ignoreInputUntilTime;

    public bool IsThirdPersonActive { get; private set; }

    private bool IsInteractionLocked => _autoZoomActive || _forceFirstPersonMode || Time.time < _ignoreInputUntilTime;

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
        ForceFirstPerson();
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

    private void OnDisable()
    {
        DetachListeners();
    }

    private IEnumerator WaitForGameMaster()
    {
        while (GameMaster.Instance == null)
            yield return null;

        zoomAllowed = true;
    }

    private void OnZoomInStarted(InputAction.CallbackContext context)
    {
        if (IsInteractionLocked) return;
        if (!CanZoom()) return;

        isZoomingIn = true;
        isZoomingOut = false;

        ApplyZoomStep(firstPersonZoomStep);
        if (holdTimerRoutine != null) StopCoroutine(holdTimerRoutine);
        holdTimerRoutine = StartCoroutine(HoldTimer(true));
    }

    private void OnZoomOutStarted(InputAction.CallbackContext context)
    {
        if (IsInteractionLocked) return;
        if (!CanZoom()) return;

        isZoomingIn = false;
        isZoomingOut = true;

        ApplyZoomStep(-thirdPersonZoomStep);
        if (holdTimerRoutine != null) StopCoroutine(holdTimerRoutine);
        holdTimerRoutine = StartCoroutine(HoldTimer(false));
    }

    private void ApplyZoomStep(float step)
    {
        float oldZoom = zoomAmount;
        zoomAmount = Mathf.Clamp(zoomAmount + step, thirdPersonMaxZoom, 1f);

        // Force exact zero when entering First Person
        if (oldZoom < 0f && zoomAmount >= 0f)
        {
            zoomAmount = 0f;
            _ignoreInputUntilTime = Time.time + tpToFpIgnoreTime;
        }
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

        if (((zoomingIn && isZoomingIn) || (!zoomingIn && isZoomingOut)) && !IsInteractionLocked)
        {
            if (continuousZoomRoutine != null)
                StopCoroutine(continuousZoomRoutine);

            continuousZoomRoutine = StartCoroutine(ContinuousZoom(zoomingIn));
        }
    }

    private IEnumerator ContinuousZoom(bool zoomIn)
    {
        while (((zoomIn && isZoomingIn) || (!zoomIn && isZoomingOut)) && !IsInteractionLocked)
        {
            float speed = (zoomIn ? firstPersonContinuousZoomSpeed : thirdPersonContinuousZoomSpeed) * Time.deltaTime;

            if (zoomIn)
                zoomAmount = Mathf.Clamp(zoomAmount + speed, thirdPersonMaxZoom, 1f);
            else
                zoomAmount = Mathf.Clamp(zoomAmount - speed, thirdPersonMaxZoom, 1f);

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
                return false;
        }
        return true;
    }

    private void Update()
    {
        if (IsInteractionLocked) return;

        UpdateCameraAndFOV();
    }

    private void UpdateCameraAndFOV()
    {
        bool wasThirdPerson = IsThirdPersonActive;

        if (_forceFirstPersonMode)
        {
            IsThirdPersonActive = false;
            zoomAmount = 0f;
        }
        else
        {
            IsThirdPersonActive = zoomAmount < 0f;

            // Force exact zero when transitioning to First Person
            if (wasThirdPerson && !IsThirdPersonActive)
            {
                zoomAmount = 0f;
                _ignoreInputUntilTime = Time.time + tpToFpIgnoreTime;
            }
        }

        if (FirstPersonCamera != null)
            FirstPersonCamera.enabled = !IsThirdPersonActive;

        if (ThirdPersonCamera != null)
            ThirdPersonCamera.enabled = IsThirdPersonActive;

        if (!IsThirdPersonActive && FirstPersonCamera != null)
        {
            float fpT = Mathf.Clamp01(zoomAmount);
            FirstPersonCamera.fieldOfView = Mathf.Lerp(defaultFOV, maxZoom, fpT);
        }

        if (IsThirdPersonActive && ThirdPersonCamera != null)
        {
            float tpT = Mathf.InverseLerp(0f, thirdPersonMaxZoom, zoomAmount);
            ThirdPersonCamera.fieldOfView = Mathf.Lerp(thirdPersonMinFOV, thirdPersonMaxFOV, tpT);
        }

        if (wasThirdPerson && !IsThirdPersonActive)
            EventManager.ResetThirdPersonState();
    }

    private void SetDefaultFOV()
    {
        StopAllCoroutines();
        zoomAmount = 0f;
        _ignoreInputUntilTime = 0f;
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

        bool wasThirdPerson = IsThirdPersonActive;
        IsThirdPersonActive = false;

        if (ThirdPersonCamera != null) ThirdPersonCamera.enabled = false;
        if (FirstPersonCamera != null) FirstPersonCamera.enabled = true;

        if (wasThirdPerson)
            EventManager.ResetThirdPersonState();

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
        float startFOV = FirstPersonCamera != null ? FirstPersonCamera.fieldOfView : defaultFOV;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / duration);

            if (FirstPersonCamera != null)
                FirstPersonCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, a);

            yield return null;
        }

        if (FirstPersonCamera != null)
            FirstPersonCamera.fieldOfView = targetFOV;

        _autoZoomActive = false;
    }

    // Public method called from ThirdPersonCameraCollision
    public void ReportCameraCloseness(float closeness) { }

    private void ForceFirstPerson()
    {
        zoomAmount = 0f;
        UpdateCameraAndFOV();
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
        EventManager.OnCrouch -= OnCrouch;
        EventManager.OnUnCrouch -= OnUncrouch;
    }
}