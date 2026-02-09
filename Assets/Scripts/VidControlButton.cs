using UnityEngine;
using UnityEngine.UI;

public class VidControlButton : MonoBehaviour
{
    public VidControl VidControl = VidControl.Play;
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

        Debug.Log("Control Vid press: "+VidControl);
        
        GameMaster.Instance.TerminalEventManager.VideoControlCommand(VidControl);
    }
}
