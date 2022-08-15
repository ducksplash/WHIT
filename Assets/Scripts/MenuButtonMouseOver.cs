using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButtonMouseOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private TextMeshProUGUI theText;
    public Color OrigColor;
    public Color HoverColor = Color.white;

    public Material normalMaterial;
    public Material hoverMaterial;


    public void Start()
    {
        theText = gameObject.GetComponent<TextMeshProUGUI>();

        OrigColor = theText.color;
        theText.fontSharedMaterial = normalMaterial;
    }



    public void OnPointerEnter(PointerEventData eventData)
    {
        theText.color = HoverColor;
        //theText.fontStyle = FontStyles.Underline;

        theText.fontSharedMaterial = hoverMaterial;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        theText.color = OrigColor;
        theText.fontStyle = FontStyles.Normal;
        theText.fontSharedMaterial = normalMaterial;
    }
}



