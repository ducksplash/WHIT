using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class VideoManager : MonoBehaviour
{
    public PCVideo CurrentVideo = PCVideo.tigger;

    public VidNavver VidNavver;
    public VidPlayerNavver VidPlayerNavver;

    public VideoSystem theVideoSystem;
    public CanvasGroup videoPlayerPanel;

    public InputActionReference goBack;

    private Coroutine videoPlayerListenerCo;

    private void Start()
    {
        TerminalEventManager.OnVideoManagerStarted += VideoManagerStarted;
        TerminalEventManager.OnVideoManagerClosed += VideoManagerClosed;

        TerminalEventManager.OnVideoSelected += OpenVideoPlayer;     // single subscription

        TerminalEventManager.OnVideoPlayerClosed += CloseVideoPlayer; // if something else requests close

        // start hidden/inactive
        HideAllVideoUI();
    }

    private void OnDestroy()
    {
        TerminalEventManager.OnVideoManagerStarted -= VideoManagerStarted;
        TerminalEventManager.OnVideoManagerClosed -= VideoManagerClosed;

        TerminalEventManager.OnVideoSelected -= OpenVideoPlayer;
        TerminalEventManager.OnVideoPlayerClosed -= CloseVideoPlayer;

        if (goBack != null)
        {
            goBack.action.performed -= CloseVideoManager;
            goBack.action.performed -= GoBackFromPlayer;
        }
    }

    private void VideoManagerStarted()
    {
        // Ensure player UI is closed when entering video manager
        CloseVideoPlayer();

        if (videoPlayerListenerCo != null)
        {
            StopCoroutine(videoPlayerListenerCo);
            videoPlayerListenerCo = null;
        }

        videoPlayerListenerCo = StartCoroutine(EnableListNextFrame());
    }

    private IEnumerator EnableListNextFrame()
    {
        yield return new WaitForEndOfFrame();
        ShowVideoListUI();
    }

    private void VideoManagerClosed()
    {
        HideAllVideoUI();

        if (videoPlayerListenerCo != null)
        {
            StopCoroutine(videoPlayerListenerCo);
            videoPlayerListenerCo = null;
        }

        // let ComputerSystem handle back again
        GameMaster.Instance.TerminalEventManager.BackButtonOverride(false);
    }

    public void OpenVideoPlayer(PCVideo pcVideo)
    {
        Debug.Log("open video player, supply video " + pcVideo);

        CurrentVideo = pcVideo;

        // load and show player
        theVideoSystem.LoadVideo(pcVideo);

        videoPlayerPanel.alpha = 1;
        videoPlayerPanel.blocksRaycasts = true;
        videoPlayerPanel.interactable = true;

        ShowVideoPlayerUI();
    }

    public void CloseVideoPlayer()
    {
        // stop playback (should NOT recursively fire close events)
        if (theVideoSystem != null)
            theVideoSystem.StopVideo();

        // hide player panel
        videoPlayerPanel.alpha = 0;
        videoPlayerPanel.blocksRaycasts = false;
        videoPlayerPanel.interactable = false;

        // back to list state (if manager is still active)
        ShowVideoListUI();
    }

    // --- UI state helpers ---

    private void ShowVideoListUI()
    {
        // Navver states
        if (VidNavver != null) VidNavver.enabled = true;
        if (VidPlayerNavver != null) VidPlayerNavver.enabled = false;

        // Back overrides ComputerSystem back while we're in this app
        GameMaster.Instance.TerminalEventManager.BackButtonOverride(true);

        // Back closes the video manager (returns to file manager)
        if (goBack != null)
        {
            goBack.action.performed -= GoBackFromPlayer;
            goBack.action.performed -= CloseVideoManager;
            goBack.action.performed += CloseVideoManager;
        }
    }

    private void ShowVideoPlayerUI()
    {
        if (VidNavver != null) VidNavver.enabled = false;
        if (VidPlayerNavver != null) VidPlayerNavver.enabled = true;

        GameMaster.Instance.TerminalEventManager.BackButtonOverride(true);

        // Back closes player first (returns to list)
        if (goBack != null)
        {
            goBack.action.performed -= CloseVideoManager;
            goBack.action.performed -= GoBackFromPlayer;
            goBack.action.performed += GoBackFromPlayer;
        }
    }

    private void HideAllVideoUI()
    {
        if (VidNavver != null) VidNavver.enabled = false;
        if (VidPlayerNavver != null) VidPlayerNavver.enabled = false;

        videoPlayerPanel.alpha = 0;
        videoPlayerPanel.blocksRaycasts = false;
        videoPlayerPanel.interactable = false;

        if (goBack != null)
        {
            goBack.action.performed -= CloseVideoManager;
            goBack.action.performed -= GoBackFromPlayer;
        }
    }

    // goBack when player open
    private void GoBackFromPlayer(InputAction.CallbackContext ctx)
    {
        CloseVideoPlayer();
    }

    // goBack when in list
    private void CloseVideoManager(InputAction.CallbackContext ctx)
    {
        HideAllVideoUI();

        // return to file manager (same as your photo manager)
        GameMaster.Instance.TerminalEventManager.FileManagerStarted();
        GameMaster.Instance.TerminalEventManager.BackButtonOverride(false);
    }
}
