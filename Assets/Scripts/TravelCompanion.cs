using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TravelCompanion : MonoBehaviour
{
    public CanvasGroup TravelCanvas;

    public bool CompanionOpen;
    private GameObject Notepad;

    public CanvasGroup crosshair;
    public CanvasGroup evidencecompanion;

    public Dictionary<GAMELEVEL, string> AvailableLocations = new Dictionary<GAMELEVEL, string>();

    public GameObject notepadButtonPrefab;
    public RectTransform scrollViewContent;

    public InputActionReference exitButton;
    public InputActionReference exitButtonPhoneKey;
    public InputActionReference StepBackInputRightClick;
    public InputActionReference StepBackInputESC;
    public MeshRenderer[] notepadMeshRenderers;

    
    private void Start()
    {
        // UI + input only
        Notepad = Player.Instance.TravelNotepad;

        exitButton?.action.Enable();
        exitButton.action.performed += CloseCompanionInput;
        exitButtonPhoneKey.action.performed += CloseCompanionInput;

        InitialiseLocations();

        notepadMeshRenderers = Notepad.GetComponentsInChildren<MeshRenderer>();

        ToggleNotepadVisibility(false);


    }



    private void ToggleNotepadVisibility(bool notepadVisible)
    {
        foreach (var rend in notepadMeshRenderers)
        {
            rend.enabled = notepadVisible;
        }
        
        
        TravelCanvas.alpha = notepadVisible ? 1 : 0;
        TravelCanvas.blocksRaycasts = notepadVisible;
        TravelCanvas.interactable = notepadVisible;
        
    }
    
    
    private void CloseCompanionInput(InputAction.CallbackContext callbackContext)
    {
        if (CompanionOpen) LaunchCompanion();
    }

    public void LaunchCompanion(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
    {
        if (GameMaster.Instance.PLAYERBUSY && !CompanionOpen) return;

        if (!GameMaster.Instance.OnboardingManager.TESTEVIDENCECOLLECTED)
        {
            GameMaster.Instance.OnboardingManager.EvidenceNotCollected();
            return;
        }

        if (!GameMaster.Instance.OnboardingManager.ONBOARDINGCOMPLETE)
        {
            GameMaster.Instance.OnboardingManager.NotReadyYet();
            return;
        }

        if (!CompanionOpen)
        {
            Notepad.SetActive(true);
            TravelCanvas.alpha = 1f;
            TravelCanvas.blocksRaycasts = true;

            GameMaster.Instance.PLAYERBUSY = true;
            CompanionOpen = true;

            ToggleNotepadVisibility(true);
            
            evidencecompanion.alpha = 0.0f;
            crosshair.alpha = 0.0f;

            StepBackInputRightClick.action.performed += LaunchCompanion;
            StepBackInputESC.action.performed += LaunchCompanion;
            
            Player.Instance.ToggleNotepad(false);
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            StartCoroutine(StopAnimating());
        }
        else
        {
            Notepad.SetActive(false);
            TravelCanvas.alpha = 0f;
            TravelCanvas.blocksRaycasts = false;

            GameMaster.Instance.PLAYERBUSY = false;
            CompanionOpen = false;

            evidencecompanion.alpha = 0.9f;
            crosshair.alpha = 0.9f;
            
            ToggleNotepadVisibility(false);
            
            Player.Instance.ToggleNotepad(true);
            
            StepBackInputRightClick.action.performed -= LaunchCompanion;
            StepBackInputESC.action.performed -= LaunchCompanion;
            
        
            GameMaster.Instance.Player.Noranimator.speed = 1;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    
    private IEnumerator StopAnimating()
    {

        for (var i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(0.1f);
            GameMaster.Instance.Player.Noranimator.speed -= 0.1f;
        }
        
            
        GameMaster.Instance.EventManager.StartNotepad(Notepad.transform);
        
        FacePlayerOnY();

    }

    
        
    public void FacePlayerOnY()
    {
        Vector3 toPlayer = Player.Instance.transform.position - Notepad.transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        float targetYaw = Quaternion.LookRotation(toPlayer, Vector3.up).eulerAngles.y;

        Vector3 e = Notepad.transform.rotation.eulerAngles;
        Notepad.transform.rotation = Quaternion.Euler(e.x, targetYaw, e.z);
    }

    
    
    // ===========================
    // UI LIST
    // ===========================

    public void InitialiseLocations()
    {
        if (scrollViewContent == null) return;

        foreach (Transform child in scrollViewContent.transform)
            Destroy(child.gameObject);

        AvailableLocations.Clear();

        // order matters
        AvailableLocations.Add(GAMELEVEL.TawleyMeats, "Tawley Meats");
        AvailableLocations.Add(GAMELEVEL.RoarkOutside, "Roark Microtech");
        AvailableLocations.Add(GAMELEVEL.NorasFlat, "\n...just go home");

        float verticalSpacing = 30f;

        float contentHeight = scrollViewContent.rect.height;
        float firstButtonHeight = notepadButtonPrefab.GetComponent<RectTransform>().rect.height;
        float initialYPosition = contentHeight / 2f - firstButtonHeight / 2f;
        float currentYPosition = initialYPosition;

        foreach (var availableLocation in AvailableLocations)
        {
            if (availableLocation.Key.ToString().Equals(GameMaster.Instance.THISLEVEL.ToString()))
                continue;

            GameObject buttonObj = Instantiate(notepadButtonPrefab, scrollViewContent);
            RectTransform buttonTransform = buttonObj.GetComponent<RectTransform>();
            buttonTransform.anchoredPosition = new Vector2(buttonTransform.anchoredPosition.x, currentYPosition);
            currentYPosition -= verticalSpacing;

            NotepadButton newButton = buttonObj.GetComponent<NotepadButton>();
            newButton.buttonText = availableLocation.Value;
            newButton.buttonTextElement.text = availableLocation.Value;
            newButton.targetScene = availableLocation.Key;

            // IMPORTANT: NotepadButton should call TravelCompanion.ChangeScene(...) already.
            // If it doesn't, you can add a UnityEvent hookup in the prefab,
            // or have NotepadButton call FindObjectOfType<TravelCompanion>().ChangeScene(targetScene).
        }
    }

    // ===========================
    // Scene transition request (delegated)
    // ===========================

    public void ChangeScene(GAMELEVEL sceneName)
    {
        
        
        GameMaster.Instance.PLAYERBUSY = false;

        LaunchCompanion();
        
        if (GameMaster.Instance.LoadingManager != null)
        {
            GameMaster.Instance.LoadingManager.LoadLevel(sceneName, onFinished: InitialiseLocations);
        }
        else
        {
            Debug.LogWarning("[TravelCompanion] LoadingManager missing on GameMaster.");
        }
    }

    // Kept for compatibility with your existing calls
    public void ChangeSceneOffTheBooks(GAMELEVEL sceneName) => ChangeScene(sceneName);
}
