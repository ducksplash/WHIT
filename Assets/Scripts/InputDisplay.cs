using System;
using TMPro;
using UnityEngine;

public class InputDisplay : MonoBehaviour
{
    public CanvasGroup inputDisplayCanvas;
    public TextMeshProUGUI primaryKeyText;
    public TextMeshProUGUI primaryInputValueText;


    public void Start()
    {
        inputDisplayCanvas.alpha = 0;
    }

    public void SetInputDisplay(Inputs theSelectedInput)
    {
        primaryKeyText.text = theSelectedInput.InputKeyController;
        primaryInputValueText.text = theSelectedInput.InputActionName;

        Debug.Log("Set Input Display"+theSelectedInput.InputKeyController);

        inputDisplayCanvas.alpha = 1;

    }
}
