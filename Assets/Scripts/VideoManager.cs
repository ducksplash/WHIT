using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class VideoManager : MonoBehaviour
{
    
    public PCVideo CurrentVideo = PCVideo.testone;
    public VidNavver VidNavver;
    public VidPlayerNavver VidPlayerNavver;
    public VideoSystem theVideoSystem;
    public CanvasGroup videoPlayerPanel;
    private Coroutine videoPlayerListenerCo;
    

    void Start()
    {
        TerminalEventManager.OnVideoManagerStarted += VideoManagerStarted;
        TerminalEventManager.OnVideoManagerClosed += VideoManagerClosed;
        TerminalEventManager.OnVideoPlayerClosed += CloseVideoPlayer;
        VidNavver.enabled = false;
    }
    

    private void VideoManagerStarted()
    {
        CloseVideoPlayer(); // initialising video system to 'off' just in case i left the canvasgroup wrong in the editor :);

        if (videoPlayerListenerCo != null)
        {
            StopCoroutine(videoPlayerListenerCo);
            videoPlayerListenerCo = null;
        }

        videoPlayerListenerCo = StartCoroutine(AssignListener());
    }


    private IEnumerator AssignListener()
    {
        yield return new WaitForEndOfFrame();
        
        VidNavver.enabled = true;
        TerminalEventManager.OnVideoSelected += OpenVideoPlayer;
    }
    

    private void VideoManagerClosed()
    {
        TerminalEventManager.OnVideoSelected -= OpenVideoPlayer;
        VidNavver.enabled = false;
    }
    
    
    
    
    
    public void OpenVideoPlayer(PCVideo pcVideo)
    {
        Debug.Log("open video player, supply video "+pcVideo);
        theVideoSystem.LoadVideo(pcVideo);
        videoPlayerPanel.alpha = 1;
        videoPlayerPanel.blocksRaycasts = true;
        videoPlayerPanel.interactable = true;
        VidNavver.enabled = false;
        VidPlayerNavver.enabled = true;

    }


    public void CloseVideoPlayer()
    {
        theVideoSystem.StopVideo();
        videoPlayerPanel.alpha = 0;
        videoPlayerPanel.blocksRaycasts = false;
        videoPlayerPanel.interactable = false;
        VidNavver.enabled = true;
        VidPlayerNavver.enabled = false;
    }

    

    public void CloseToDesktop()
    {
        GameMaster.Instance.TerminalEventManager.CloseToDesktop();
    }
    
    
}

