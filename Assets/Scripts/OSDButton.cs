using UnityEngine;
using UnityEngine.UI;

public class OSDButton : MonoBehaviour
{
    public PhonerGriddle ThisItem;
    public GameObject AppOutline;
    public Button AppButton;
    public bool scaleUpButton = true;
    public Vector3 origScale;
    public float scaleUp = 1.1f;
    private bool onHovver;
    
    void Start()
    {
        origScale = transform.localScale;
    }

    public void OnHover()
    {
        if (onHovver) return;
        onHovver = true;
        if (scaleUpButton) transform.localScale = Vector3.one * scaleUp;
    }
    public void OffHover()
    {
        if (!onHovver) return;
        onHovver = false;
        if (scaleUpButton) transform.localScale = origScale;
    }
    
    public void ExecuteCommand()
    {
        Debug.Log("ExecuteCommand("+ThisItem+")");
        Player.Instance.PlayerPhone.SelectPhoneGridItem(ThisItem);
    }
}
