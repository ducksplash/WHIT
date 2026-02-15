using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PhotoViewer : MonoBehaviour
{
    [Header("UI")]
    public Image selectedPhotoImage;
    public CanvasGroup photoViewerCanvas;
    public InputActionReference goBack;

    [Header("State")]
    public PCPhoto selectedPhoto;

    private const string ResourcesPhotoFolder = "PHOTOS"; 

    private void Start()
    {
        TerminalEventManager.OnPhotoSelected += SetPhoto;

        // Optional: ensure closed on start
        if (photoViewerCanvas != null)
        {
            photoViewerCanvas.alpha = 0f;
            photoViewerCanvas.interactable = false;
            photoViewerCanvas.blocksRaycasts = false;
        }
    }
    
    public void SetPhoto(PCPhoto pcPhoto)
    {
        selectedPhoto = pcPhoto;
        
        string resourcePath = $"{ResourcesPhotoFolder}/{pcPhoto}";

        
        goBack.action.performed += ClosePhotoAction;
        GameMaster.Instance.TerminalEventManager.BackButtonOverride(true);
        
        Sprite sprite = Resources.Load<Sprite>(resourcePath);
        if (sprite == null)
        {
            Debug.LogError($"PhotoViewer: Could not load photo sprite at Resources path '{resourcePath}'. " + $"Expected: Assets/Resources/{resourcePath}.png");
            return;
        }

        if (selectedPhotoImage != null) selectedPhotoImage.sprite = sprite;

        if (photoViewerCanvas != null)
        {
            photoViewerCanvas.alpha = 1f;
            photoViewerCanvas.interactable = true;
            photoViewerCanvas.blocksRaycasts = true;
        }
    }


    private void ClosePhotoAction(InputAction.CallbackContext callbackContext)
    {
        ClosePhoto();
    }
    
    public void ClosePhoto()
    {
        if (photoViewerCanvas != null)
        {
            photoViewerCanvas.alpha = 0f;
            photoViewerCanvas.interactable = false;
            photoViewerCanvas.blocksRaycasts = false;
        }
        
        goBack.action.performed -= ClosePhotoAction;
        GameMaster.Instance.TerminalEventManager.BackButtonOverride(false);
        GameMaster.Instance.TerminalEventManager.PhotoViewerClosed();
    }
}