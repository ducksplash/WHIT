using TMPro;
using UnityEngine;

public class OutfitText : MonoBehaviour
{
    public TextMeshProUGUI outfitText;



    void Start()
    {
        EventManager.OnOutfitChanged += SetOutfitText;
    }



    private void SetOutfitText(OutfitType outfitType)
    {
        outfitText.text = outfitType.ToString();
    }
}
