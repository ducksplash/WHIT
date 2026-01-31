using TMPro;
using UnityEngine;

public class InputHelperMonkey : MonoBehaviour
{
    private TextMeshProUGUI inputField;
    void Start()
    {
        if (GetComponent<TextMeshProUGUI>() != null)
        {
            GetComponent<TextMeshProUGUI>().textWrappingMode = TextWrappingModes.NoWrap;

        }
    }
    
}
