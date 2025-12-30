using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugLogger : MonoBehaviour
{
    [Header("UI")]
    public ScrollRect scrollRect;
    public TextMeshProUGUI logText;
    public CanvasGroup canvasGroup;
    
    [Header("Settings")]
    public int maxLines = 20;

    private readonly Queue<string> lines = new Queue<string>();

    
    
    private void OnEnable()
    {
        EventManager.OnPaused += PanelToggle;
        Application.logMessageReceived += HandleLog;
    }

    private void PanelToggle(bool isPaused)
    {
        if (isPaused)
        {
            PanelEnable();
        }
        else
        {
            PanelDisable();
        }
    }
    
    
    
    void PanelEnable()
    {
        canvasGroup.alpha = 1;
        RefreshView();
    }

    void PanelDisable()
    {
        canvasGroup.alpha = 0;
    }

    private void HandleLog(string message, string stackTrace, LogType type)
    {
        // Prefix type (optional)
        string formatted = $"[{type}] {message}";

        lines.Enqueue(formatted);

        // Trim to max
        while (lines.Count > maxLines)
            lines.Dequeue();

        RefreshView();
    }

    private void RefreshView()
    {
        if (logText == null) return;

        logText.text = string.Join("\n", lines);

        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

}