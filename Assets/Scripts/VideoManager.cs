using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class VideoManager : MonoBehaviour
{
    
    public PCVideo CurrentVideo = PCVideo.testone;
    public VidNavver VidNavver;
    public VideoSystem theVideoSystem;
    public CanvasGroup videoPlayerPanel;


    void Start()
    {
        TerminalEventManager.OnVideoManagerStarted += VideoManagerStarted;
        TerminalEventManager.OnVideoManagerClosed += VideoManagerClosed;
        VidNavver.enabled = false;
    }
    

    private void VideoManagerStarted()
    {
        TerminalEventManager.OnVideoSelected += OpenVideoPlayer;
        VidNavver.enabled = true;
        CloseVideoPlayer(); // initialising video system to 'off' just in case i left the canvasgroup wrong in the editor :)
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
    }


    public void CloseVideoPlayer()
    {
        theVideoSystem.StopVideo();
        videoPlayerPanel.alpha = 0;
        videoPlayerPanel.blocksRaycasts = false;
        videoPlayerPanel.interactable = false;
    }

    

    public void CloseToDesktop()
    {
        GameMaster.Instance.TerminalEventManager.CloseToDesktop();
    }
    
    
}

