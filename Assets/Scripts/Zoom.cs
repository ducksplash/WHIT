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
    public float zoomStep = 0.1f; // Amount to change per input press

    [Header("Input Actions")]
    public InputActionReference zoomInInput;  // Zoom in (scroll up / trigger)
    public InputActionReference zoomOutInput; // Zoom out (scroll down / trigger)

    public bool zoomAllowed = false;

    private Camera cam;
    private Coroutine zoomRoutine;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        EventManager.OnStartComputer += AutoZoomIn;
        EventManager.OnStopComputer += AutoZoomOut;
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
            zoomInInput.action.performed += OnZoomInPerformed;
        }

        if (zoomOutInput != null)
        {
            zoomOutInput.action.Enable();
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

    private void SetDefaultFOV()
    {
        if (zoomRoutine != null) StopCoroutine(zoomRoutine);

        zoomAmount = 0f;
        UpdateFOV();
    }

    private void OnZoomInPerformed(InputAction.CallbackContext context)
    {
        if (!zoomAllowed || GameMaster.Instance.PLAYERBUSY || GameMaster.Instance.PauseManager.IsPaused || GameMaster.Instance.TravelCompanion.CompanionIsOpen) return;

        zoomAmount = Mathf.Clamp01(zoomAmount + zoomStep);
        UpdateFOV();
    }

    private void OnZoomOutPerformed(InputAction.CallbackContext context)
    {
        if (!zoomAllowed || GameMaster.Instance.PLAYERBUSY || GameMaster.Instance.PauseManager.IsPaused || GameMaster.Instance.TravelCompanion.CompanionIsOpen) return;

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

    public void AutoZoomIn(Transform pcTransform)
    {
        if (!zoomAllowed) return;

        float targetFOV = defaultFOV * 0.40f;

        if (zoomRoutine != null) StopCoroutine(zoomRoutine);
        zoomRoutine = StartCoroutine(SmoothFOV(targetFOV, 0.15f));
    }

    public void AutoZoomOut()
    {
        if (!zoomAllowed) return;

        if (zoomRoutine != null) StopCoroutine(zoomRoutine);
        zoomRoutine = StartCoroutine(SmoothFOV(defaultFOV, 0.15f));
    }

    private IEnumerator SmoothFOV(float targetFOV, float duration)
    {
        float startFOV = cam.fieldOfView;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            cam.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t / duration);
            yield return null;
        }

        cam.fieldOfView = targetFOV;

        // Keep scroll zoom in sync
        zoomAmount = Mathf.InverseLerp(defaultFOV, maxZoom, cam.fieldOfView);
    }
}
