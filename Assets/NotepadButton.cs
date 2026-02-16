using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotepadButton : MonoBehaviour
{
    public TextMeshProUGUI buttonTextElement;
    public string buttonText;
    public GAMELEVEL targetScene;
    
    
    // Start is called before the first frame update
    void Start()
    {
        buttonTextElement.text = buttonText;
    }

    public void ClickToChangeScene()
    {
        if (!GameMaster.Instance.TravelCompanion.CompanionOpen) return;
        GameMaster.Instance.TravelCompanion.ChangeScene(targetScene);
    }
}
