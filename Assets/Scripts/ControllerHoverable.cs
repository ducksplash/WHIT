using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ControllerHoverable : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler
{
    public Image keyImage;
    public Color hoverColor = new Color(0,0.5f,0,1);
    public Color originalColor;

    private void Start()
    {

        originalColor = keyImage.color;

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        keyImage.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        keyImage.color = originalColor;
    }
}
