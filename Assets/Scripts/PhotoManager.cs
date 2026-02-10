using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhotoManager : MonoBehaviour
{
    public PhotoNavver PhotoNavver;
    private Coroutine photoViewerListenerCo;
    public InputActionReference goBack;

    
    private void Start()
    {
        DisableNavver();
        
        TerminalEventManager.OnPhotoManagerStarted += PhotoManagerStarted;
        TerminalEventManager.OnPhotoManagerClosed += PhotoManagerClosed;
        
        
        TerminalEventManager.OnPhotoViewerClosed += PhotoViewerClosed;
        TerminalEventManager.OnPhotoSelected += PhotoViewerOpened;
        
    }

    private void PhotoManagerStarted()
    {
        if (photoViewerListenerCo != null)
        {
            StopCoroutine(photoViewerListenerCo);
            photoViewerListenerCo = null;
        }

        photoViewerListenerCo = StartCoroutine(AssignListener());
    }
    
    
    private IEnumerator AssignListener()
    {
        yield return new WaitForEndOfFrame();
        
        EnableNavver();
    }


    private void PhotoManagerClosed()
    {
        EnableNavver();
    }


    void DisableNavver()
    {        
        PhotoNavver.enabled = false;
        goBack.action.performed -= ClosePhotoManager;
        GameMaster.Instance.TerminalEventManager.BackButtonOverride(false);
    }

    void EnableNavver()
    {
        goBack.action.performed += ClosePhotoManager;
        GameMaster.Instance.TerminalEventManager.BackButtonOverride(true);
        PhotoNavver.enabled = true;
    }

    
    
    

    public void PhotoViewerOpened(PCPhoto pcPhoto)
    {
        DisableNavver();
    }

    

    public void PhotoViewerClosed()
    {
        if (photoViewerListenerCo != null)
        {
            StopCoroutine(photoViewerListenerCo);
            photoViewerListenerCo = null;
        }

        photoViewerListenerCo = StartCoroutine(AssignListener());
    }

    public void ClosePhotoManager(InputAction.CallbackContext callbackContext)
    {
        DisableNavver();
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
