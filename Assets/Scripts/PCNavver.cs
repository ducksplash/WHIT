using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PCNavver : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference UpAction;
    public InputActionReference DownAction;
    public InputActionReference LeftAction;
    public InputActionReference RightAction;
    public InputActionReference SubmitAction;

    [Header("Grid (populate in inspector, index order top → bottom, left → right)")]
    public List<PCButton> GridButtons = new List<PCButton>();

    [Header("Navigation Layout")]
    public PCNavigationLayout Layout = PCNavigationLayout.Grid;

    [Header("Grid Size")]
    public int columns = 3;
    public int rows = 4;

    public Color hoverColor    = new Color(0f, 0.78f, 0f, 1f);


    private Coroutine enablementCo;
    private int currentIndex = 0;

    // NEW: Remember last hovered index
    private int lastHoveredIndex = 0;

    private void SubscribeEvents()
    {
        TerminalEventManager.OnOverrideClick += DisableAllOutlines;
        
        if (enablementCo != null)
        {
            StopCoroutine(EnableButtons());
            enablementCo = null;
        }

        enablementCo = StartCoroutine(EnableButtons());
    }

    private void OnEnable()
    {
        SubscribeEvents();
        StartCoroutine(HoverLastOrDefaultNextFrame());
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

        SubmitAction.action.performed -= OnSubmit;
    }

    private void Start()
    {

        Highlight(currentIndex);
        SubscribeEvents();
    }

    private void OnUp(InputAction.CallbackContext ctx)
    {
        if (!GameMaster.Instance.PLAYERBUSY) return;

        int newIndex = currentIndex;
        switch (Layout)
        {
            case PCNavigationLayout.Grid:
                newIndex = currentIndex - columns;
                break;
            case PCNavigationLayout.TopToBottom:
                newIndex = currentIndex - 1;
                break;
            case PCNavigationLayout.BottomToTop:
                newIndex = currentIndex + 1;
                break;
        }

        if (newIndex >= 0 && newIndex < GridButtons.Count)
            MoveTo(newIndex);
    }

    private void OnDown(InputAction.CallbackContext ctx)
    {
        if (!GameMaster.Instance.PLAYERBUSY) return;

        int newIndex = currentIndex;
        switch (Layout)
        {
            case PCNavigationLayout.Grid:
                newIndex = currentIndex + columns;
                break;
            case PCNavigationLayout.TopToBottom:
                newIndex = currentIndex + 1;
                break;
            case PCNavigationLayout.BottomToTop:
                newIndex = currentIndex - 1;
                break;
        }

        if (newIndex >= 0 && newIndex < GridButtons.Count)
            MoveTo(newIndex);
    }

    private void OnLeft(InputAction.CallbackContext ctx)
    {
        if (!GameMaster.Instance.PLAYERBUSY) return;

        int newIndex = currentIndex;
        switch (Layout)
        {
            case PCNavigationLayout.Grid:
                if ((currentIndex % columns) != 0)
                    newIndex = currentIndex - 1;
                break;
            case PCNavigationLayout.LeftToRight:
                if (currentIndex - 1 >= 0)
                    newIndex = currentIndex - 1;
                break;
        }

        if (newIndex != currentIndex)
            MoveTo(newIndex);
    }

    private void OnRight(InputAction.CallbackContext ctx)
    {
        if (!GameMaster.Instance.PLAYERBUSY) return;

        int newIndex = currentIndex;
        switch (Layout)
        {
            case PCNavigationLayout.Grid:
                if ((currentIndex % columns) != columns - 1 && currentIndex + 1 < GridButtons.Count)
                    newIndex = currentIndex + 1;
                break;
            case PCNavigationLayout.LeftToRight:
                if (currentIndex + 1 < GridButtons.Count)
                    newIndex = currentIndex + 1;
                break;
        }

        if (newIndex != currentIndex)
            MoveTo(newIndex);
    }

    private void OnSubmit(InputAction.CallbackContext ctx)
    {
        
        if (!GameMaster.Instance.PLAYERBUSY) return;

        if (TryFocusInputField(currentIndex))
            return;

        ActivateOSDButton(currentIndex);
    }

    private bool TryFocusInputField(int index)
    {
        if (index < 0 || index >= GridButtons.Count) return false;
        var item = GridButtons[index];
        if (item == null) return false;

        // Legacy UI InputField
        var tmpInput = item.GetComponentInChildren<TMPro.TMP_InputField>(true);
        if (tmpInput != null && tmpInput.interactable)
        {
            EventSystem.current.SetSelectedGameObject(tmpInput.gameObject);
            tmpInput.Select();
            tmpInput.ActivateInputField();
            return true;
        }
        
        return false;
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
        for (int i = 0; i < GridButtons.Count; i++)
        {
            if (GridButtons[i] == null) continue;

            GridButtons[i].AppOutline.SetActive(i == index);
        }
    }

    private void DisableAllOutlines()
    {
        for (int i = 0; i < GridButtons.Count; i++)
        {
            if (GridButtons[i] == null) continue;

            GridButtons[i].AppOutline.SetActive(false);
        }
    }

    public void ActivateOSDButton(int SelectedButton)
    {
        if (SelectedButton < 0 || SelectedButton >= GridButtons.Count) return;
        if (GridButtons[SelectedButton] != null) GridButtons[SelectedButton].ExecuteCommand();
    }

    public IEnumerator EnableButtons()
    {
        yield return new WaitForEndOfFrame();
        UpAction.action.performed += OnUp;
        DownAction.action.performed += OnDown;
        LeftAction.action.performed += OnLeft;
        RightAction.action.performed += OnRight;
        SubmitAction.action.performed += OnSubmit;
    }

    // public void ResetList()
    // {
    //     GridButtons = new List<PCButton>();
    // }
}

public enum PCNavigationLayout
{
    Grid,
    TopToBottom,
    BottomToTop,
    LeftToRight
}
