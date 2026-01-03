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

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        EventManager.OnPhoneOpened += SetDefaultFOV;
        
        SetDefaultFOV();

        StartCoroutine(WaitForGameMaster());

        // Enable actions and subscribe to events
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

    private void SetDefaultFOV()
    {
        zoomAmount = 0;
        UpdateFOV();
    }

    void OnDisable()
    {
        // Unsubscribe and disable actions
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

    private void OnZoomInPerformed(InputAction.CallbackContext context)
    {
        if (!zoomAllowed || GameMaster.Instance.PHONEOUT || GameMaster.Instance.TravelCompanion.CompanionIsOpen)
            return;

        zoomAmount = Mathf.Clamp01(zoomAmount + zoomStep);
        UpdateFOV();
    }

    private void OnZoomOutPerformed(InputAction.CallbackContext context)
    {
        if (!zoomAllowed || GameMaster.Instance.PHONEOUT || GameMaster.Instance.TravelCompanion.CompanionIsOpen)
            return;

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
}
