using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseNavver : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference UpAction;
    public InputActionReference DownAction;
    public InputActionReference LeftAction;
    public InputActionReference RightAction;

    [Header("Grid (populate in inspector, index order top → bottom, left → right)")]
    public List<PauseButton> GridButtons = new List<PauseButton>();

    
    
    [Header("Input Scriptable Objects")]
    public List<Inputs> InputSOs = new List<Inputs>();

    [Header("Input Display")] 
    public InputDisplay InputDisplayPanel;

    
    
    [Header("Navigation Layout")]
    public PauseNavigationLayout Layout = PauseNavigationLayout.Grid;
    
    [Header("Grid Size")]
    public int columns = 3;
    public int rows = 4;

    public Color hoverColor = new Color(0f, 0.78f, 0f, 1f);


    private Coroutine enablementCo;
    private int currentIndex = 0;

    // NEW: Remember last hovered index
    private int lastHoveredIndex = 0;


    private void OnEnable()
    {
        SubscribeEvents();
        StartCoroutine(HoverLastOrDefaultNextFrame());
    
        Debug.Log("Enabled");
    }
    private void SubscribeEvents()
    {
        TerminalEventManager.OnOverrideClick += DisableAllOutlines;
        
        UpAction.action.performed += OnUp;
        DownAction.action.performed += OnDown;
        LeftAction.action.performed += OnLeft;
        RightAction.action.performed += OnRight;
        
        if (enablementCo != null)
        {
            StopCoroutine(EnableButtons());
            enablementCo = null;
        }

        enablementCo = StartCoroutine(EnableButtons());
    }

    private IEnumerator HoverLastOrDefaultNextFrame()
    {
        yield return null; // wait one frame to ensure GridButtons populated
        HoverLastOrDefault();
    }

    public void HoverLastOrDefault()
    {
        if (GridButtons.Count == 0) return;

        // Use lastHoveredIndex if valid, otherwise 0
        currentIndex = Mathf.Clamp(lastHoveredIndex, 0, GridButtons.Count - 1);
        Highlight(currentIndex);
    }

    private void OnDisable()
    {
        UpAction.action.performed -= OnUp;
        DownAction.action.performed -= OnDown;
        LeftAction.action.performed -= OnLeft;
        RightAction.action.performed -= OnRight;
    }

    private void Start()
    {

        Highlight(currentIndex);
        SubscribeEvents();
    }

    private void OnUp(InputAction.CallbackContext ctx)
    {
        if (!GameMaster.Instance.PauseManager.IsPaused) return;

        int newIndex = currentIndex;
        switch (Layout)
        {
            case PauseNavigationLayout.Grid:
                newIndex = currentIndex - columns;
                break;
            case PauseNavigationLayout.TopToBottom:
                newIndex = currentIndex - 1;
                break;
            case PauseNavigationLayout.BottomToTop:
                newIndex = currentIndex + 1;
                break;
        }

        if (newIndex >= 0 && newIndex < GridButtons.Count)
            MoveTo(newIndex);
    }

    private void OnDown(InputAction.CallbackContext ctx)
    {
        if (!GameMaster.Instance.PauseManager.IsPaused) return;

        int newIndex = currentIndex;
        switch (Layout)
        {
            case PauseNavigationLayout.Grid:
                newIndex = currentIndex + columns;
                break;
            case PauseNavigationLayout.TopToBottom:
                newIndex = currentIndex + 1;
                break;
            case PauseNavigationLayout.BottomToTop:
                newIndex = currentIndex - 1;
                break;
        }

        if (newIndex >= 0 && newIndex < GridButtons.Count)
            MoveTo(newIndex);
    }

    private void OnLeft(InputAction.CallbackContext ctx)
    {
        if (!GameMaster.Instance.PauseManager.IsPaused) return;

        int newIndex = currentIndex;
        switch (Layout)
        {
            case PauseNavigationLayout.Grid:
                if ((currentIndex % columns) != 0)
                    newIndex = currentIndex - 1;
                break;
            case PauseNavigationLayout.LeftToRight:
                if (currentIndex - 1 >= 0)
                    newIndex = currentIndex - 1;
                break;
        }

        if (newIndex != currentIndex)
            MoveTo(newIndex);
    }

    private void OnRight(InputAction.CallbackContext ctx)
    {
        if (!GameMaster.Instance.PauseManager.IsPaused) return;

        int newIndex = currentIndex;
        switch (Layout)
        {
            case PauseNavigationLayout.Grid:
                if ((currentIndex % columns) != columns - 1 && currentIndex + 1 < GridButtons.Count)
                    newIndex = currentIndex + 1;
                break;
            case PauseNavigationLayout.LeftToRight:
                if (currentIndex + 1 < GridButtons.Count)
                    newIndex = currentIndex + 1;
                break;
        }

        if (newIndex != currentIndex)
            MoveTo(newIndex);
    }


    
    private void MoveTo(int newIndex)
    {
        currentIndex = Mathf.Clamp(newIndex, 0, GridButtons.Count - 1);

        // NEW: update last hovered index
        lastHoveredIndex = currentIndex;

        Highlight(currentIndex);
    }

    private void Highlight(int index)
    {
        // 1) outlines only
        for (int i = 0; i < GridButtons.Count; i++)
        {
            if (GridButtons[i] == null) continue;
            GridButtons[i].AppOutline.SetActive(i == index);
        }

        // 2) now update the input display ONLY for the selected button
        if (index < 0 || index >= GridButtons.Count) return;
        var selectedButton = GridButtons[index];
        if (selectedButton == null) return;

        Inputs selectedInput = InputSOs.FirstOrDefault(d => d != null && d.InputName == selectedButton.ThisInputName);

        if (selectedInput == null)
        {
            Debug.LogWarning($"Highlight: No Inputs SO found for '{selectedButton.ThisInputName}'.");
            InputDisplayPanel.SetInputDisplay(null);
            return;
        }

        Debug.Log("Highlight -> " + selectedInput.InputName);
        InputDisplayPanel.SetInputDisplay(selectedInput);
    }


    private void DisableAllOutlines()
    {
        for (int i = 0; i < GridButtons.Count; i++)
        {
            if (GridButtons[i] == null) continue;

            GridButtons[i].AppOutline.SetActive(false);
        }
    }



    public IEnumerator EnableButtons()
    {
        yield return new WaitForEndOfFrame();
        UpAction.action.performed += OnUp;
        DownAction.action.performed += OnDown;
        LeftAction.action.performed += OnLeft;
        RightAction.action.performed += OnRight;
    }
    
}

public enum PauseNavigationLayout
{
    Grid,
    TopToBottom,
    BottomToTop,
    LeftToRight
}
