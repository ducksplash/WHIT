using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UInstance : Singleton<UInstance>
{
    public CanvasGroup cutsceneBarsCanvas;
    public CanvasGroup HudCanvas;
    public CanvasGroup ProceedCanvas;
    public TextMeshProUGUI advanceInputText;
    public CanvasGroup TipsCanvas;
    public TextMeshProUGUI tipsText;
    public TextMeshProUGUI tipsTextBG;


    public ControlPad controlPad;
    

    public void Start()
    {
        cutsceneBarsCanvas.alpha = 0;
        ProceedCanvas.gameObject.SetActive(false);
        
        EventManager.OnDebugCameraToggle += DebugDisableHUD;
        EventManager.OnDialogueCanProceed += ToggleDialogueCanProceed;
    }

    
    private void ToggleDialogueCanProceed(bool toggleval)
    {       
        
        Debug.Log("UInstance ProceedCanvas "+toggleval);

        ProceedCanvas.gameObject.SetActive(toggleval);
        advanceInputText.text = toggleval ? GameMaster.Instance.InputManager.ReturnInputName(InputName.Submit) : "";
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

    public void DisplayTip(string tipText, ControlPadButton selectedButton)
    {
        tipsText.text = tipText;
        tipsTextBG.text = tipText;
        controlPad.HighlightButton(selectedButton);
        
    }

    
    
}
