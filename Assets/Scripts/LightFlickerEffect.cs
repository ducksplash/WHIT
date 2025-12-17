using UnityEngine;
using System.Collections.Generic;

public class LightFlickerEffect : MonoBehaviour
{
    public Light thelight;
    public Renderer targetRenderer;

    public float minIntensity = 0f;
    public float maxIntensity = 1f;
    public int smoothing = 5;

    private Queue<float> smoothQueue = new Queue<float>();
    private float lastSum = 0f;

    private Material[] mats;

    // HDRP emissive properties
    static readonly int HDRP_EmissiveColor = Shader.PropertyToID("_EmissiveColor");
    static readonly int HDRP_UseEmissiveColor = Shader.PropertyToID("_UseEmissiveColor");
    static readonly int HDRP_EmissiveExposureWeight = Shader.PropertyToID("_EmissiveExposureWeight");

    void Start()
    {
        if (thelight == null)
            thelight = GetComponent<Light>();

        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        if (targetRenderer != null)
        {
            // Instantiate material instances for runtime changes
            mats = targetRenderer.materials;

            // Ensure HDRP emission is enabled
            foreach (var m in mats)
            {
                if (m == null) continue;

                m.SetFloat(HDRP_UseEmissiveColor, 1f);         // REQUIRED FOR HDRP
                m.SetFloat(HDRP_EmissiveExposureWeight, 1f);   // recommended default
                m.EnableKeyword("_EMISSIVE_COLOR");            // HDRP internal
                m.EnableKeyword("_EMISSION");
            }
        }

        smoothQueue = new Queue<float>(smoothing);
    }

    void FixedUpdate()
    {
        if (thelight != null && thelight.enabled)
        {
            FlickerLightAndEmissive();
        }
        else if (thelight == null)
        {
            FlickerEmissiveOnly();
        }
        // else: light exists but disabled → do nothing
    }

    float GetSmoothedRandom()
    {
        while (smoothQueue.Count >= smoothing)
            lastSum -= smoothQueue.Dequeue();

        float v = Random.Range(minIntensity, maxIntensity);
        smoothQueue.Enqueue(v);
        lastSum += v;

        return lastSum / smoothQueue.Count;
    }

    void FlickerLightAndEmissive()
    {
        float v = GetSmoothedRandom();

        // flicker real HDRP Light
        thelight.intensity = v;

        if (mats == null) return;

        Color finalColor = thelight.color * v;

        foreach (var m in mats)
        {
            if (m == null) continue;

            // Write HDRP emissive value
            m.SetColor(HDRP_EmissiveColor, finalColor);
        }
    }

    void FlickerEmissiveOnly()
    {
        float v = GetSmoothedRandom();

        if (mats == null) return;

        Color finalColor = Color.white * v;

        foreach (var m in mats)
        {
            if (m == null) continue;

            m.SetColor(HDRP_EmissiveColor, finalColor);
        }
    }
}
