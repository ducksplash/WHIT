using TMPro;
using UnityEngine;

public class OutfitText : MonoBehaviour
{
    public TextMeshProUGUI outfitText;



    void Start()
    {
        EventManager.OnOutfitWasChanged += SetOutfitText;
    }



    private void SetOutfitText(string outfitName)
    {

        if (outfitName.Length > 0)
        {
            outfitText.text = outfitName;
        }
        else
        {
            outfitText.text = "not specified!";
        }
    }
}
