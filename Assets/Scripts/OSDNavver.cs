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

    [Header("Grid Size")]
    public int columns = 3;   // 3 across
    public int rows = 4;      // 4 down

    public Color originalColor = new Color(0,200,0,150);
    public Color hoverColor = new Color(0,200,0,150);
    
    private int currentIndex = 0;

    private void OnEnable()
    {
        UpAction.action.performed += OnUp;
        DownAction.action.performed += OnDown;
        LeftAction.action.performed += OnLeft;
        RightAction.action.performed += OnRight;
        SubmitAction.action.performed += OnSubmit;

        UpAction.action.Enable();
        DownAction.action.Enable();
        LeftAction.action.Enable();
        RightAction.action.Enable();
        SubmitAction.action.Enable();
    }

    private void OnDisable()
    {
        UpAction.action.performed -= OnUp;
        DownAction.action.performed -= OnDown;
        LeftAction.action.performed -= OnLeft;
        RightAction.action.performed -= OnRight;
        SubmitAction.action.performed -= OnSubmit;

        UpAction.action.Disable();
        DownAction.action.Disable();
        LeftAction.action.Disable();
        RightAction.action.Disable();
        SubmitAction.action.Disable();
    }

    private void Start()
    {
        // sample orig color
        if (GridButtons[0] != null)
        {
            originalColor = GridButtons[0].GetComponent<Button>().image.color;
        }
        
        Highlight(currentIndex);
    }

    // --- NAVIGATION HANDLERS ---

    private void OnUp(InputAction.CallbackContext ctx)
    {
        int next = currentIndex - columns;
        if (next >= 0) MoveTo(next);
    }

    private void OnDown(InputAction.CallbackContext ctx)
    {
        int next = currentIndex + columns;
        if (next < GridButtons.Count)
            MoveTo(next);
    }

    private void OnLeft(InputAction.CallbackContext ctx)
    {
        // stay within row
        bool isAtRowStart = (currentIndex % columns) == 0;
        if (!isAtRowStart) MoveTo(currentIndex - 1);
    }

    private void OnRight(InputAction.CallbackContext ctx)
    {
        bool isAtRowEnd = ((currentIndex % columns) == columns - 1);
        
        if (!isAtRowEnd && currentIndex + 1 < GridButtons.Count) MoveTo(currentIndex + 1);
    }

    private void OnSubmit(InputAction.CallbackContext ctx)
    {
        ActivateOSDButton(currentIndex);
    }

    // --- LOGIC ---

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
            GridButtons[i].GetComponent<Button>().image.color = i == index ? hoverColor : originalColor;
        }
    }

    public void ActivateOSDButton(int SelectedButton)
    {
        if (SelectedButton < 0 || SelectedButton >= GridButtons.Count)
            return;

        if (GridButtons[SelectedButton] != null) GridButtons[SelectedButton].ExecuteCommand();
    }
}
