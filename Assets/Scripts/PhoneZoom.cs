using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhoneZoom : MonoBehaviour
{
    [Header("Normal Camera Zoom Settings")]
    public float defaultFOV = 60f;
    public float maxZoomFOV = 10f;        // Fully zoomed in (narrow FOV)

    [Header("Selfie Mode Zoom Settings")]
    [Tooltip("Default FOV when starting selfie mode")]
    public float selfieDefaultFOV = 70f;
    [Tooltip("Maximum zoom FOV in selfie mode (usually wider for selfies)")]
    public float selfieMaxZoomFOV = 110f;   // Fully "zoomed in" in selfie = wider angle

    [Range(0f, 1f)]
    public float zoomAmount = 0f;
    public float zoomStep = 0.1f;

    [Header("Input Actions")]
    public InputActionReference zoomInInput;  
    public InputActionReference zoomOutInput; 

    public bool zoomAllowed = false;

    public Camera cam;
    private Phone phone;

    void Start()
    {
        // Initialize with normal defaults
        defaultFOV = cam.fieldOfView;
        
        phone = GetComponentInParent<Phone>();

        StartCoroutine(WaitForGameMaster());

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

    void OnDisable()
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

    private void OnZoomInPerformed(InputAction.CallbackContext context)
    {
        if (!zoomAllowed || !GameMaster.Instance.PLAYERBUSY || phone == null || !phone.CameraOpen)
            return;

        AdjustZoom(zoomStep);
    }

    private void OnZoomOutPerformed(InputAction.CallbackContext context)
    {
        if (!zoomAllowed || !GameMaster.Instance.PLAYERBUSY || phone == null || !phone.CameraOpen)
            return;

        AdjustZoom(-zoomStep);
    }

    private void AdjustZoom(float delta)
    {
        bool isSelfie = Player.Instance?.PlayerPhone?.TakingSelfie == true;

        if (isSelfie)
        {
            // In selfie mode: invert direction and use selfie bounds
            zoomAmount = Mathf.Clamp01(zoomAmount - delta);
        }
        else
        {
            // Normal mode
            zoomAmount = Mathf.Clamp01(zoomAmount + delta);
        }

        UpdateFOV();
    }

    public void DefaultFOV()
    {
        zoomAmount = 0f;
        UpdateFOV();
    }

    private void UpdateFOV()
    {
        bool isSelfie = Player.Instance?.PlayerPhone?.TakingSelfie == true;

        if (isSelfie)
        {
            cam.fieldOfView = Mathf.Lerp(selfieDefaultFOV, selfieMaxZoomFOV, zoomAmount);
        }
        else
        {
            cam.fieldOfView = Mathf.Lerp(defaultFOV, maxZoomFOV, zoomAmount);
        }
    }

    private IEnumerator WaitForGameMaster()
    {
        while (GameMaster.Instance == null)
            yield return null;

        zoomAllowed = true;
    }
}