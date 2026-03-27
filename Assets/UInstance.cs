using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UInstance : Singleton<UInstance>
{
    public CanvasGroup cutsceneBarsCanvas;
    public CanvasGroup HudCanvas;
    public CanvasGroup ProceedCanvas;
    public TextMeshProUGUI advanceInputText;
    

    public void Start()
    {
        cutsceneBarsCanvas.alpha = 0;
        ProceedCanvas.alpha = 0;
        
        EventManager.OnDebugCameraToggle += DebugDisableHUD;
        EventManager.OnDialogueCanProceed += ToggleDialogueCanProceed;


    }



    private void ToggleDialogueCanProceed(bool toggleval)
    {       
        
        Debug.Log("UInstance ProceedCanvas "+toggleval);

        ProceedCanvas.alpha = toggleval ? 1 : 0;
        advanceInputText.text = toggleval ? GameMaster.Instance.InputManager.ReturnInputName(InputName.Submit) : "";
        Debug.Log("ProceedCanvas.alpha "+ProceedCanvas.alpha);
        Debug.Log("advanceInputText.text "+advanceInputText.text);
    }


    void DebugDisableHUD(bool b)
    {
        Debug.Log("set hud "+b);
        HudCanvas.alpha = b ? 0 : 1;
    }

    
    // Coroutine to fade out the cutscene bars
    public IEnumerator FadeInCutsceneBars(float panTime)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.smoothDeltaTime /  (panTime / 2);
            cutsceneBarsCanvas.alpha = Mathf.Lerp(0f, 1f, t);
            HudCanvas.alpha = Mathf.Lerp(1f, 0f, t);
            yield return new WaitForEndOfFrame();
            
        }

    }

    // Coroutine to fade out the cutscene bars
    public IEnumerator FadeOutCutsceneBars()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.smoothDeltaTime;
            cutsceneBarsCanvas.alpha = Mathf.Lerp(1f, 0f, t);
            HudCanvas.alpha = Mathf.Lerp(0f, 1f, t);
            yield return new WaitForEndOfFrame();
            
        }
    }

    
    
}
