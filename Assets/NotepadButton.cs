using TMPro;
using UnityEngine;

public class NotepadButton : MonoBehaviour
{
    public TextMeshProUGUI buttonTextElement;
    public string buttonText;
    public TravelCompanion.GameScene targetScene;
    
    
    // Start is called before the first frame update
    void Start()
    {
        buttonTextElement.text = buttonText;
    }

    public void ClickToChangeScene()
    {
    //    TravelCompanion.Instance.ChangeScene(targetScene.ToString());
    }
}
