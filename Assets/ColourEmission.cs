using System.Collections;
using UnityEngine;

public class ColourEmission : MonoBehaviour
{
    [SerializeField] Renderer renderObject;
    [SerializeField] float pulseDuration = 2.0f;
    [SerializeField] float pulseDelay = 5.0f;
    [SerializeField] float minIntensity = 0.0f;
    [SerializeField] float maxIntensity = 1.0f;

    private Material material;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        material = renderObject.material;
        StartCoroutine(PulseEmissionStrength());
    }

    IEnumerator PulseEmissionStrength()
    {
        while (true)
        {
            // Increase intensity from 0 to 1 over pulseDuration
            for (float t = 0; t < pulseDuration; t += Time.deltaTime)
            {
                float intensity = Mathf.Lerp(minIntensity, maxIntensity, t / pulseDuration);
                SetEmissionIntensity(intensity);
                yield return null;
            }

            // Ensure intensity reaches maxIntensity at the end
            SetEmissionIntensity(maxIntensity);

            // Wait for pulseDelay seconds
            yield return new WaitForSeconds(pulseDelay);

            // Decrease intensity from 1 to 0 over pulseDuration
            for (float t = 0; t < pulseDuration; t += Time.deltaTime)
            {
                float intensity = Mathf.Lerp(maxIntensity, minIntensity, t / pulseDuration);
                SetEmissionIntensity(intensity);
                yield return null;
            }

            // Ensure intensity reaches minIntensity at the end
            SetEmissionIntensity(minIntensity);
        }
    }

    void SetEmissionIntensity(float intensity)
    {
        Color color = Color.white;
        color.r *= intensity; // Adjust the intensity of each color channel separately
        color.g *= intensity;
        color.b *= intensity;
        material.SetColor(EmissionColor, color);
        material.EnableKeyword("_EMISSION");
    }
}
