using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TextButtonMouseOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private TextMeshProUGUI theText;
    public Color OrigColor;
    public Color HoverColor = Color.red;

    public void Start()
    {
        theText = gameObject.GetComponent<TextMeshProUGUI>();

        OrigColor = theText.color;
    }



        public void OnPointerEnter(PointerEventData eventData)
    {
        theText.color = HoverColor;
        theText.fontStyle = FontStyles.Bold;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        theText.color = OrigColor;
        theText.fontStyle = FontStyles.Normal;
    }
}



