using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TextButtonMouseOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private TextMeshProUGUI theText;
    public Color OrigColor;
    public Color HoverColor = Color.white;

    public Material normalMaterial;
    public Material hoverMaterial;

    [SerializeField] private bool usingFormatting = true;

    public void Start()
    {
        theText = gameObject.GetComponent<TextMeshProUGUI>();

        OrigColor = theText.color;
        theText.fontSharedMaterial = normalMaterial;
    }



    public void OnPointerEnter(PointerEventData eventData)
    {
        theText.color = HoverColor;

        if (usingFormatting)
        {
            theText.fontStyle = FontStyles.Underline;
        }

        theText.fontSharedMaterial = hoverMaterial;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        theText.color = OrigColor;
        
        if (usingFormatting)
        {
            theText.fontStyle = FontStyles.Normal;
        }

        theText.fontSharedMaterial = normalMaterial;
    }
}



