using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LightSlider : MonoBehaviour
{
    [SerializeField] private List<Light> targetLights = new List<Light>();
    [SerializeField] private Slider intensitySlider;
    [SerializeField] private TMP_Text valueText;

    private void Start()
    {
        if (intensitySlider == null) return;

        if (targetLights != null && targetLights.Count > 0 && targetLights[0] != null)
            intensitySlider.value = targetLights[0].intensity;

        UpdateLights(intensitySlider.value);
        intensitySlider.onValueChanged.AddListener(UpdateLights);
    }

    private void OnDestroy()
    {
        if (intensitySlider != null)
            intensitySlider.onValueChanged.RemoveListener(UpdateLights);
    }

    private void UpdateLights(float value)
    {
        if (targetLights != null)
        {
            for (int i = 0; i < targetLights.Count; i++)
            {
                if (targetLights[i] != null)
                    targetLights[i].intensity = value;
            }
        }

        if (valueText != null)
            valueText.text = value.ToString("0.00");
    }
}