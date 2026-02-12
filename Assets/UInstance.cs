using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UInstance : Singleton<UInstance>
{
    public CanvasGroup cutsceneBarsCanvas;
    public CanvasGroup HudCanvas;


    public void Start()
    {    
        //Debug.Log("UInstance Start");
        
        cutsceneBarsCanvas.alpha = 0;  
        
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
