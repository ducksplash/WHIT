using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;   // ← Added for UniTask

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
        Debug.Log("UInstance ProceedCanvas " + toggleval);
        ProceedCanvas.gameObject.SetActive(toggleval);
        advanceInputText.text = toggleval 
            ? GameMaster.Instance.InputManager.ReturnInputName(InputName.Submit) 
            : "";
    }

    void DebugDisableHUD(bool b)
    {
        Debug.Log("set hud " + b);
        HudCanvas.alpha = b ? 0 : 1;
    }

    /// <summary>
    /// Fade in cutscene bars and fade out HUD (UniTask version)
    /// </summary>
    public async UniTask FadeInCutsceneBars(float panTime)
    {
        float t = 0f;
        float duration = panTime / 2f;

        while (t < 1f)
        {
            t += Time.smoothDeltaTime / duration;
            t = Mathf.Clamp01(t); // prevent overshooting

            cutsceneBarsCanvas.alpha = Mathf.Lerp(0f, 1f, t);
            HudCanvas.alpha = Mathf.Lerp(1f, 0f, t);

            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        // Ensure final values
        cutsceneBarsCanvas.alpha = 1f;
        HudCanvas.alpha = 0f;
    }

    /// <summary>
    /// Fade out cutscene bars and fade in HUD (UniTask version)
    /// </summary>
    public async UniTask FadeOutCutsceneBars()
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.smoothDeltaTime;
            t = Mathf.Clamp01(t);

            cutsceneBarsCanvas.alpha = Mathf.Lerp(1f, 0f, t);
            HudCanvas.alpha = Mathf.Lerp(0f, 1f, t);

            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        // Ensure final values
        cutsceneBarsCanvas.alpha = 0f;
        HudCanvas.alpha = 1f;
    }

    public void DisplayTip(string tipText, ControlPadButton selectedButton)
    {
        tipsText.text = tipText;
        tipsTextBG.text = tipText;
        controlPad.HighlightButton(selectedButton);
    }
}