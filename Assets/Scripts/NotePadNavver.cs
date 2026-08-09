using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NotePadNavver : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference UpAction;
    public InputActionReference DownAction;
    public InputActionReference LeftAction;
    public InputActionReference RightAction;
    public InputActionReference SubmitAction;

    [Header("Grid (populate in inspector, index order top → bottom, left → right)")]
    public List<NotePadNavButton> GridButtons = new List<NotePadNavButton>();

    [Header("Navigation Layout")]
    public NotepadNavigationLayout Layout = NotepadNavigationLayout.Grid;

    [Header("Grid Size")]
    public int columns = 3;
    public int rows = 4;
    
    private Coroutine enablementCo;
    public int currentIndex = 0;

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
        yield return null;
        HoverLastOrDefault();
    }

    public void HoverLastOrDefault()
    {
        if (GridButtons.Count == 0) return;

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
        if (!GameMaster.Instance.TravelCompanion.CompanionOpen) return;
        if (!GameMaster.Instance.PLAYERBUSY) return;

        int newIndex = currentIndex;

        switch (Layout)
        {
            case NotepadNavigationLayout.Grid:
                newIndex = currentIndex - columns;
                break;

            case NotepadNavigationLayout.TopToBottom:
                newIndex = currentIndex - 1;
                break;

            case NotepadNavigationLayout.BottomToTop:
                newIndex = currentIndex + 1;
                break;
        }

        if (newIndex < 0)
            newIndex = GridButtons.Count - 1;
        else if (newIndex >= GridButtons.Count)
            newIndex = 0;

        MoveTo(newIndex);
    }

    private void OnDown(InputAction.CallbackContext ctx)
    {
        if (!GameMaster.Instance.TravelCompanion.CompanionOpen) return;
        if (!GameMaster.Instance.PLAYERBUSY) return;

        int newIndex = currentIndex;

        switch (Layout)
        {
            case NotepadNavigationLayout.Grid:
                newIndex = currentIndex + columns;
                break;

            case NotepadNavigationLayout.TopToBottom:
                newIndex = currentIndex + 1;
                break;

            case NotepadNavigationLayout.BottomToTop:
                newIndex = currentIndex - 1;
                break;
        }

        if (newIndex >= GridButtons.Count)
            newIndex = 0;
        else if (newIndex < 0)
            newIndex = GridButtons.Count - 1;

        MoveTo(newIndex);
    }

    private void OnLeft(InputAction.CallbackContext ctx)
    {
        if (!GameMaster.Instance.TravelCompanion.CompanionOpen) return;
        if (!GameMaster.Instance.PLAYERBUSY) return;

        int newIndex = currentIndex;
        switch (Layout)
        {
            case NotepadNavigationLayout.Grid:
                if ((currentIndex % columns) != 0)
                    newIndex = currentIndex - 1;
                break;
            case NotepadNavigationLayout.LeftToRight:
                if (currentIndex - 1 >= 0)
                    newIndex = currentIndex - 1;
                break;
        }

        if (newIndex != currentIndex)
            MoveTo(newIndex);
    }

    private void OnRight(InputAction.CallbackContext ctx)
    {
        if (!GameMaster.Instance.TravelCompanion.CompanionOpen) return;
        if (!GameMaster.Instance.PLAYERBUSY) return;

        int newIndex = currentIndex;
        switch (Layout)
        {
            case NotepadNavigationLayout.Grid:
                if ((currentIndex % columns) != columns - 1 && currentIndex + 1 < GridButtons.Count)
                    newIndex = currentIndex + 1;
                break;
            case NotepadNavigationLayout.LeftToRight:
                if (currentIndex + 1 < GridButtons.Count)
                    newIndex = currentIndex + 1;
                break;
        }

        if (newIndex != currentIndex)
            MoveTo(newIndex);
    }

    private void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (!GameMaster.Instance.TravelCompanion.CompanionOpen) return;
        if (!GameMaster.Instance.PLAYERBUSY) return;
        ActivateOSDButton(currentIndex);
    }

    private void MoveTo(int newIndex)
    {
        if (!GameMaster.Instance.TravelCompanion.CompanionOpen) return;
        currentIndex = Mathf.Clamp(newIndex, 0, GridButtons.Count - 1);

        lastHoveredIndex = currentIndex;

        Highlight(currentIndex);
    }

    private void Highlight(int index)
    {
        if (!GameMaster.Instance.TravelCompanion.CompanionOpen) return;
        for (int i = 0; i < GridButtons.Count; i++)
        {
            if (GridButtons[i] == null) continue;


            if (i == index)
            {
                GridButtons[i].textButtonMouseover.ManualMouseOn();
            }
            else
            {
                GridButtons[i].textButtonMouseover.ManualMouseOff();
            }
            

            var btn = GridButtons[i].GetComponent<Button>();

            if (i == index) GridButtons[i].OnHover();
            else GridButtons[i].OffHover();
        }
    }

    public void ActivateOSDButton(int SelectedButton)
    {
        if (!GameMaster.Instance.TravelCompanion.CompanionOpen) return;
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
        GridButtons = new List<NotePadNavButton>();
        currentIndex = 0;
        lastHoveredIndex = 0;
    }
}

public enum NotepadNavigationLayout
{
    Grid,
    TopToBottom,
    BottomToTop,
    LeftToRight
}

public enum NotepadGriddle
{
    Null = 1000,
}
