using UnityEngine;
using UnityEngine.UI;

public class NotePadNavButton : MonoBehaviour
{
    public bool scaleUpButton = true;
    public Vector3 origScale;
    public float scaleUp = 1.1f;
    private bool onHovver;
    public NotepadButton thisNotepadButton;
    public TextButtonMouseOver textButtonMouseover;
    public DialogueName selectedDialogue;
    
    void Start()
    {
        origScale = transform.localScale;
    }

    public void OnHover()
    {
        if (onHovver) return;
        onHovver = true;
        if (scaleUpButton) transform.localScale = Vector3.one * scaleUp;
        textButtonMouseover.ManualMouseOn();
        
        
    }
    public void OffHover()
    {
        if (!onHovver) return;
        onHovver = false;
        if (scaleUpButton) transform.localScale = origScale;
        textButtonMouseover.ManualMouseOff();
        
        
    }
    
    public void ExecuteCommand()
    {
        thisNotepadButton.ClickToChangeScene();
        
        // Debug.Log("ExecuteCommand("+ThisItem+")");
        // Player.Instance.PlayerPhone.SelectPhoneGridItem(ThisItem, selectedDialogue);
    }
}
