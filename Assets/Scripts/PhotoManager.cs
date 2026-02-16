using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhotoManager : MonoBehaviour
{
    public PhotoNavver PhotoNavver;
    public InputActionReference goBack;

    private Coroutine photoViewerListenerCo;

    // Guard to prevent double-unhook
    private bool _unhooked;

    private void OnEnable()
    {
        HookEvents();
        DisableNavver(); // safe initial state
    }

    private void Start()
    {
        // If this object starts enabled, Start runs after OnEnable.
        // Keep Start minimal (avoid subscribing here).
    }

    private void OnDisable()
    {
        UnhookEvents();
        StopListenerCoroutine();
    }

    private void OnDestroy()
    {
        UnhookEvents();
        StopListenerCoroutine();
    }

    private void HookEvents()
    {
        UnhookEvents();
        _unhooked = false;

        TerminalEventManager.OnPhotoManagerStarted += PhotoManagerStarted;
        TerminalEventManager.OnPhotoManagerClosed += PhotoManagerClosed;

        TerminalEventManager.OnPhotoViewerClosed += PhotoViewerClosed;
        TerminalEventManager.OnPhotoSelected += PhotoViewerOpened;

        if (goBack != null)
        {
            goBack.action.performed -= ClosePhotoManager;
            // Don’t add yet; EnableNavver() decides when it’s active
        }
    }

    private void UnhookEvents()
    {
        if (_unhooked) return;
        _unhooked = true;

        TerminalEventManager.OnPhotoManagerStarted -= PhotoManagerStarted;
        TerminalEventManager.OnPhotoManagerClosed -= PhotoManagerClosed;

        TerminalEventManager.OnPhotoViewerClosed -= PhotoViewerClosed;
        TerminalEventManager.OnPhotoSelected -= PhotoViewerOpened;

        if (goBack != null)
            goBack.action.performed -= ClosePhotoManager;
    }

    private void StopListenerCoroutine()
    {
        if (photoViewerListenerCo != null)
        {
            StopCoroutine(photoViewerListenerCo);
            photoViewerListenerCo = null;
        }
    }

    private void PhotoManagerStarted()
    {
        // When Photos app opens: enable navver after a frame
        StopListenerCoroutine();
        photoViewerListenerCo = StartCoroutine(AssignListenerNextFrame());
    }

    private IEnumerator AssignListenerNextFrame()
    {
        // If we were disabled/destroyed mid-frame, bail
        if (!this || !gameObject) yield break;

        yield return null;

        // if scene is unloading, objects may already be destroyed
        if (!this || !gameObject) yield break;

        EnableNavver();
    }

    private void PhotoManagerClosed()
    {
        // Photo manager closing usually means back to file manager: restore navver state
        DisableNavver();
    }

    private void DisableNavver()
    {
        // Some scenes may destroy references: always null-check
        if (PhotoNavver != null)
            PhotoNavver.enabled = false;

        if (goBack != null)
            goBack.action.performed -= ClosePhotoManager;

        if (GameMaster.Instance != null && GameMaster.Instance.TerminalEventManager != null)
            GameMaster.Instance.TerminalEventManager.BackButtonOverride(false);
    }

    private void EnableNavver()
    {
        if (goBack != null)
        {
            goBack.action.performed -= ClosePhotoManager;
            goBack.action.performed += ClosePhotoManager;
        }

        if (GameMaster.Instance != null && GameMaster.Instance.TerminalEventManager != null)
            GameMaster.Instance.TerminalEventManager.BackButtonOverride(true);

        if (PhotoNavver != null)
            PhotoNavver.enabled = true;
    }

    public void PhotoViewerOpened(PCPhoto pcPhoto)
    {
        // When a photo viewer opens, disable navver so Back doesn't close manager unexpectedly
        DisableNavver();
    }

    public void PhotoViewerClosed()
    {
        StopListenerCoroutine();
        photoViewerListenerCo = StartCoroutine(AssignListenerNextFrame());
    }

    public void ClosePhotoManager(InputAction.CallbackContext callbackContext)
    {
        // If we’re mid-unload, ignore
        if (!this || !gameObject) return;

        DisableNavver();

        // NOTE: Your original called FileManagerStarted() (seems like it should return to Files UI)
        if (GameMaster.Instance != null && GameMaster.Instance.TerminalEventManager != null)
            GameMaster.Instance.TerminalEventManager.FileManagerStarted();
    }
}

public enum PCPhoto
{
    one,
    two,
    three,
    four,
    five,
    six,
    seven,
    eight,
    nine,
    ten,
    eleven,
    twelve,
    thirteen,
    fourteen,
    fifteen,
    sixteen,
    seventeen,
    eighteen,
    nineteen,
    twenty,
    twentyone
}
