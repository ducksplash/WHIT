using UnityEngine;
using UnityEngine.UI;

public class PCButton : MonoBehaviour
{
    public PCGriddle ThisItem;
    public GameObject AppOutline;
    public Button AppButton;
    public bool scaleUpButton = true;
    public Vector3 origScale;
    public float scaleUp = 1.1f;
    private bool onHovver;
    public Color outlineColor;
    public DialogueName selectedDialogue;
    
    void Start()
    {
        origScale = transform.localScale;
        outlineColor = AppOutline.GetComponent<Image>().color;
    }

    public void ExecuteCommand()
    {
        GameMaster.Instance.TerminalEventManager.OverrideClick();
        AppOutline.SetActive(true);
        GameMaster.Instance.TerminalEventManager.PCGridClick(ThisItem);
    }
}
