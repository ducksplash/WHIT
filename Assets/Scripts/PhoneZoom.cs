using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhoneZoom : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float defaultFOV = 60f;
    public float maxZoom = 10f;
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

        zoomAmount = Mathf.Clamp01(zoomAmount + zoomStep);
        UpdateFOV();
    }

    private void OnZoomOutPerformed(InputAction.CallbackContext context)
    {
        if (!zoomAllowed || !GameMaster.Instance.PLAYERBUSY || phone == null || !phone.CameraOpen)
            return;

        zoomAmount = Mathf.Clamp01(zoomAmount - zoomStep);
        UpdateFOV();
    }

    public void DefaultFOV()
    {
        zoomAmount = 0;
        cam.fieldOfView = Mathf.Lerp(defaultFOV, maxZoom, zoomAmount);
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
