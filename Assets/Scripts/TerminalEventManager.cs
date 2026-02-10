using System;
using UnityEngine;

public class TerminalEventManager : MonoBehaviour
{
    public static event Action<PCGriddle> OnPCGridClick = (PCGriddle) => { };
    public static event Action<FileGriddle> OnFileGridClick = (FileGriddle) => { };
    public static event Action OnOverrideClick = () => { };
    public static event Action OnFileManagerStarted = () => { };
    public static event Action OnFileManagerClosed = () => { };
    public static event Action OnVideoManagerStarted = () => { };
    public static event Action OnVideoManagerClosed = () => { };
    public static event Action OnVideoPlayerClosed = () => { };
    public static event Action<bool> OnBackButtonOverride = (isOverride) => { };
    public static event Action<VidControl> OnVideoControlCommand = (VidControl) => { };
    public static event Action<PCVideo> OnVideoSelected = (PCVideo) => { };
    public static event Action<PCPhoto> OnPhotoSelected = (PCPhoto) => { };
    public static event Action OnPhotoViewerClosed = () => { };
    public static event Action OnPhotoManagerStarted = () => { };
    public static event Action OnPhotoManagerClosed = () => { };
    
    
    public static event Action OnCloseToDesktop = () => { };
    
    public void PCGridClick(PCGriddle selectedGridItem)
    {
        OnPCGridClick.Invoke(selectedGridItem);
    }

    public void FileGridClick(FileGriddle selectedGridItem)
    {
        OnFileGridClick.Invoke(selectedGridItem);
    }

    public void OverrideClick()
    {
        OnOverrideClick.Invoke();
    }
    
    public void CloseToDesktop()
    {
        OnCloseToDesktop.Invoke();
    }
    public void FileManagerStarted()
    {
        OnFileManagerStarted.Invoke();
    }
    public void FileManagerClosed()
    {
        OnFileManagerClosed.Invoke();
    }
    public void VideoSelected(PCVideo selectedVideo)
    {
        OnVideoSelected.Invoke(selectedVideo);
    }
    public void VideoManagerStarted()
    {
        OnVideoManagerStarted.Invoke();
    }
    public void VideoManagerClosed()
    {
        OnVideoManagerClosed.Invoke();
    }
    public void VideoControlCommand(VidControl selectedControl)
    {
        OnVideoControlCommand.Invoke(selectedControl);
    }
    public void VideoPlayerClosed()
    {
        OnVideoPlayerClosed.Invoke();
    }
    public void BackButtonOverride(bool doesOverride)
    {
        OnBackButtonOverride.Invoke(doesOverride);
    }
    public void PhotoSelected(PCPhoto selectedPhoto)
    {
        OnPhotoSelected.Invoke(selectedPhoto);
    }
    public void PhotoManagerStarted()
    {
        OnPhotoManagerStarted.Invoke();
    }
    public void PhotoManagerClosed()
    {
        OnPhotoManagerClosed.Invoke();
    }
    public void PhotoViewerClosed()
    {
        OnPhotoViewerClosed.Invoke();
    }
}
