using System;
using System.Collections;
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
    public PhoneNavigationLayout Layout = PhoneNavigationLayout.Grid;

    [Header("Grid Size")]
    public int columns = 3;
    public int rows = 4;

    public Color originalColor = new Color(0, 200, 0, 150);
    public Color hoverColor = new Color(0, 200, 0, 150);

    private Coroutine enablementCo;
    private int currentIndex = 0;

    // NEW: Remember last hovered index
    private int lastHoveredIndex = 0;

    private void SubscribeEvents()
    {
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
        Debug.Log("enabled");
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
        // sample orig color
        if (GridButtons.Count > 0)
        {
            originalColor = GridButtons[0].GetComponent<Button>().image.color;
        }

        Highlight(currentIndex);
        SubscribeEvents();
    }

    private void OnUp(InputAction.CallbackContext ctx)
    {
        Debug.Log("on UP");
        if (!GameMaster.Instance.PLAYERBUSY) return;

        Debug.Log("player busy "+GameMaster.Instance.PLAYERBUSY);
        
        int newIndex = currentIndex;
        switch (Layout)
        {
            case PhoneNavigationLayout.Grid:
                newIndex = currentIndex - columns;
                break;
            case PhoneNavigationLayout.TopToBottom:
                newIndex = currentIndex - 1;
                break;
            case PhoneNavigationLayout.BottomToTop:
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
            case PhoneNavigationLayout.Grid:
                newIndex = currentIndex + columns;
                break;
            case PhoneNavigationLayout.TopToBottom:
                newIndex = currentIndex + 1;
                break;
            case PhoneNavigationLayout.BottomToTop:
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
            case PhoneNavigationLayout.Grid:
                if ((currentIndex % columns) != 0)
                    newIndex = currentIndex - 1;
                break;
            case PhoneNavigationLayout.LeftToRight:
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
            case PhoneNavigationLayout.Grid:
                if ((currentIndex % columns) != columns - 1 && currentIndex + 1 < GridButtons.Count)
                    newIndex = currentIndex + 1;
                break;
            case PhoneNavigationLayout.LeftToRight:
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
        ActivateOSDButton(currentIndex);
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

            var btn = GridButtons[i].GetComponent<Button>();
            btn.image.color = i == index ? hoverColor : originalColor;

            if (i == index) GridButtons[i].OnHover();
            else GridButtons[i].OffHover();
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

    public void ResetList()
    {
        GridButtons = new List<OSDButton>();
    }
}

public enum PhoneNavigationLayout
{
    Grid,
    TopToBottom,
    BottomToTop,
    LeftToRight
}
