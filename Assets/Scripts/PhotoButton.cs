using UnityEngine;
using UnityEngine.UI;

public class PhotoButton : MonoBehaviour
{
    public PCPhoto ThisPhoto = PCPhoto.one;
    public GameObject AppOutline;
    public Button AppButton;
    private bool onHovver;
    public Color outlineColor;
    public DialogueName selectedDialogue;
    
    void Start()
    {
        outlineColor = AppOutline.GetComponent<Image>().color;
    }

    public void ExecuteCommand()
    {
        GameMaster.Instance.TerminalEventManager.OverrideClick();
        AppOutline.SetActive(true);
        GameMaster.Instance.TerminalEventManager.PhotoSelected(ThisPhoto);
    }
}