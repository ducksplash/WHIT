using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OSDNavver : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference UpAction;
    public InputActionReference DownAction;
    public InputActionReference LeftAction;
    public InputActionReference RightAction;
    public InputActionReference SubmitAction;

    [Header("Grid (populate in inspector, index order top → bottom, left → right)")]
    public List<OSDButton> GridButtons = new List<OSDButton>();

    [Header("Navigation Layout")]
    public NavigationLayout Layout = NavigationLayout.Grid;

    [Header("Grid Size")]
    public int columns = 3;
    public int rows = 4;

    public Color originalColor = new Color(0,200,0,150);
    public Color hoverColor = new Color(0,200,0,150);
    
    private int currentIndex = 0;

    private void SubscribeEvents()
    {
        UpAction.action.performed += OnUp;
        DownAction.action.performed += OnDown;
        LeftAction.action.performed += OnLeft;
        RightAction.action.performed += OnRight;
        SubmitAction.action.performed += OnSubmit;
    }

    private void OnEnable()
    {
        SubscribeEvents();
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
        // sample orig color
        if (GridButtons[0] != null)
        {
            originalColor = GridButtons[0].GetComponent<Button>().image.color;
        }
        
        Highlight(currentIndex);
        SubscribeEvents();
    }

    private void OnUp(InputAction.CallbackContext ctx)
    {
        if (!GameMaster.Instance.PHONEOUT) return;

        switch (Layout)
        {
            case NavigationLayout.Grid:
                int nextGridUp = currentIndex - columns;
                if (nextGridUp >= 0) MoveTo(nextGridUp);
                break;

            case NavigationLayout.TopToBottom:
                int nextUp = currentIndex - 1;
                if (nextUp >= 0) MoveTo(nextUp);
                break;

            case NavigationLayout.LeftToRight:
                // ignore up
                break;
        }
    }

    private void OnDown(InputAction.CallbackContext ctx)
    {
        if (!GameMaster.Instance.PHONEOUT) return;

        switch (Layout)
        {
            case NavigationLayout.Grid:
                int nextGridDown = currentIndex + columns;
                if (nextGridDown < GridButtons.Count) MoveTo(nextGridDown);
                break;

            case NavigationLayout.TopToBottom:
                int nextDown = currentIndex + 1;
                if (nextDown < GridButtons.Count) MoveTo(nextDown);
                break;

            case NavigationLayout.LeftToRight:
                // ignore down
                break;
        }
    }

    private void OnLeft(InputAction.CallbackContext ctx)
    {
        if (!GameMaster.Instance.PHONEOUT) return;

        switch (Layout)
        {
            case NavigationLayout.Grid:
                bool isAtRowStart = (currentIndex % columns) == 0;
                if (!isAtRowStart) MoveTo(currentIndex - 1);
                break;

            case NavigationLayout.TopToBottom:
                // ignore
                break;

            case NavigationLayout.LeftToRight:
                int leftIndex = currentIndex - 1;
                if (leftIndex >= 0) MoveTo(leftIndex);
                break;
        }
    }

    private void OnRight(InputAction.CallbackContext ctx)
    {
        if (!GameMaster.Instance.PHONEOUT) return;

        switch (Layout)
        {
            case NavigationLayout.Grid:
                bool isAtRowEnd = ((currentIndex % columns) == columns - 1);
                if (!isAtRowEnd && currentIndex + 1 < GridButtons.Count)
                    MoveTo(currentIndex + 1);
                break;

            case NavigationLayout.TopToBottom:
                // ignore
                break;

            case NavigationLayout.LeftToRight:
                int rightIndex = currentIndex + 1;
                if (rightIndex < GridButtons.Count) MoveTo(rightIndex);
                break;
        }
    }

    private void OnSubmit(InputAction.CallbackContext ctx)
    {
        Debug.Log("submitty");
        if (!GameMaster.Instance.PHONEOUT) return;
        ActivateOSDButton(currentIndex);
    }
    
    private void MoveTo(int newIndex)
    {
        currentIndex = Mathf.Clamp(newIndex, 0, GridButtons.Count - 1);
        Highlight(currentIndex);
    }

    private void Highlight(int index)
    {
        for (int i = 0; i < GridButtons.Count; i++)
        {
            if (GridButtons[i] == null) continue;

            GridButtons[i].AppOutline.SetActive(i == index);

            var btn = GridButtons[i].GetComponent<Button>();

            btn.image.color = i == index ? hoverColor : originalColor;

            if (i == index) { GridButtons[i].GetComponent<OSDButton>().OnHover(); } else { GridButtons[i].GetComponent<OSDButton>().OffHover(); }
        }
    }


    public void ActivateOSDButton(int SelectedButton)
    {
        if (SelectedButton < 0 || SelectedButton >= GridButtons.Count) return;
        if (GridButtons[SelectedButton] != null) GridButtons[SelectedButton].ExecuteCommand();
    }
}

public enum NavigationLayout
{
    Grid,
    TopToBottom,
    LeftToRight
}