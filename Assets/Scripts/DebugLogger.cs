using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugLogger : MonoBehaviour
{
    [Header("UI")]
    public ScrollRect scrollRect;
    public TextMeshProUGUI logText;

    [Header("Settings")]
    public int maxLines = 20;

    private readonly Queue<string> lines = new Queue<string>();

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
        RefreshView();
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
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