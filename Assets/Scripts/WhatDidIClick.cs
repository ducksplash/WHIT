using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WhatDidIClick : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{





    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log(eventData.ToString());
    }


    public void OnPointerExit(PointerEventData eventData)
	{
		Debug.Log(eventData.ToString());
	}

	public void OnPointerClick(PointerEventData eventData)
	{


		Debug.Log(eventData.ToString());
		

	}
}



