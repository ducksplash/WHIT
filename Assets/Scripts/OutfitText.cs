using TMPro;
using UnityEngine;

public class OutfitText : MonoBehaviour
{
    public TextMeshProUGUI outfitText;



    void Start()
    {
        EventManager.OnOutfitWasChanged += SetOutfitText;
    }



    private void SetOutfitText(OutfitName outfitType)
    {
        outfitText.text = outfitType.ToString();
    }
}
