using UnityEngine;
using TMPro;
using System.Text;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private float updateInterval = 0.5f; // Update text every 0.5 seconds

    private float accum = 0f;
    private int frames = 0;
    private float timeLeft;
    private StringBuilder stringBuilder = new StringBuilder();

    private void Start()
    {
        if (fpsText == null)
        {
            Debug.LogError("FPSCounter: TextMeshProUGUI reference is not assigned!");
            enabled = false;
            return;
        }

        timeLeft = updateInterval;
    }

    private void Update()
    {
        timeLeft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        frames++;

        if (timeLeft <= 0f)
        {
            float fps = accum / frames;

            // Update text
            stringBuilder.Clear();
            stringBuilder.Append("FPS: ").Append(Mathf.RoundToInt(fps));
            
            // Optional: Add color coding
            if (fps >= 60f)
                stringBuilder.Append(" <color=green>●</color>");
            else if (fps >= 30f)
                stringBuilder.Append(" <color=yellow>●</color>");
            else
                stringBuilder.Append(" <color=red>●</color>");

            fpsText.SetText(stringBuilder);

            // Reset for next interval
            accum = 0f;
            frames = 0;
            timeLeft = updateInterval;
        }
    }
}