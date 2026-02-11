using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLB;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WorldLight : MonoBehaviour
{
    [Header("Lights")]
    [SerializeField] private List<Light> bulbList = new List<Light>();
    [SerializeField] private List<Renderer> rendererList = new List<Renderer>();
    [SerializeField] private List<VolumetricLightBeamSD> lightBeams = new List<VolumetricLightBeamSD>();
    public bool lightOn;
    [SerializeField] private bool lightFlickers;

    [Header("Flicker Settings")]
    [SerializeField] private int smoothing = 5;

    // Base intensity per light
    private List<float> baseIntensities = new List<float>();
    private Queue<float> smoothQueue = new Queue<float>();
    private float lastSum = 0f;

    private List<Material[]> materialCache = new List<Material[]>();
    static readonly int HDRP_EmissiveColor = Shader.PropertyToID("_EmissiveColor");

    void Start()
    {
        baseIntensities.Clear();
        
        foreach (var light in bulbList)
        {
            if (light != null) baseIntensities.Add(light.intensity);
        }

        // Cache renderer materials
        foreach (var rend in rendererList)
        {
            if (rend != null)
            {
                var mats = rend.materials;
                materialCache.Add(mats);
                foreach (var m in mats)
                {
                    if (m != null)
                    {
                        m.SetFloat("_UseEmissiveColor", 1f);
                        m.SetFloat("_EmissiveExposureWeight", 1f);
                        m.EnableKeyword("_EMISSIVE_COLOR");
                        m.EnableKeyword("_EMISSION");
                    }
                }
            }
        }

        if (lightFlickers && lightOn)
        {
            StopAllCoroutines();
            StartCoroutine(LightFlicker());
        }

        for (int i = 0; i < bulbList.Count; i++)
        {
            bulbList[i].enabled = lightOn;
        }

    }

    public void ToggleLight()
    {
        lightOn = !lightOn;

        for (int i = 0; i < bulbList.Count; i++)
        {
            var light = bulbList[i];
            if (light != null)
            {
                light.enabled = lightOn;
                if (lightOn) light.intensity = baseIntensities[i];
                UpdateBulbEmission(light, i);
            }
        }

        foreach (var beam in lightBeams)
        {
            if (beam != null)
            {
                beam.enabled = lightOn;
                beam.colorFromLight = true;
                beam.intensityFromLight = true;
            }
        }

        if (lightFlickers && lightOn)
        {
            StopAllCoroutines();
            StartCoroutine(LightFlicker());
        }
        else
        {
            StopAllCoroutines();
        }
        
    }

    private void UpdateBulbEmission(Light light, int index)
    {
        float intensity = lightOn ? baseIntensities[index] : 0f;
        Color col = light.color * intensity * 5f;

        if (index < materialCache.Count)
        {
            var mats = materialCache[index];
            foreach (var m in mats)
            {
                if (m != null && m.name.Contains("bulb"))
                    m.SetColor(HDRP_EmissiveColor, col);
            }
        }
    }

    private IEnumerator LightFlicker()
    {
        while (lightOn)
        {
            float flickerValue = GetSmoothedRandom();

            // Apply same value to all lights & beams
            for (int i = 0; i < bulbList.Count; i++)
            {
                var light = bulbList[i];
                if (light != null && light.enabled)
                {
                    float v = flickerValue * baseIntensities[i]; // scale by each light's base intensity
                    light.intensity = v;
                    Color finalColor = light.color * v;

                    if (i < materialCache.Count)
                    {
                        var mats = materialCache[i];
                        foreach (var m in mats)
                        {
                            if (m != null && m.name.Contains("bulb"))
                                m.SetColor(HDRP_EmissiveColor, finalColor);
                        }
                    }

                    if (i < lightBeams.Count && lightBeams[i] != null && lightBeams[i].enabled)
                    {
                        float normalized = Mathf.Clamp01(v / baseIntensities[i]);
                        lightBeams[i].intensityGlobal = normalized;
                    }
                }
            }

            yield return new WaitForSeconds(0.05f);
        }
    }

    private float GetSmoothedRandom()
    {
        while (smoothQueue.Count >= smoothing)
            lastSum -= smoothQueue.Dequeue();

        float v = Random.Range(0f, 1f); // normalized 0..1
        smoothQueue.Enqueue(v);
        lastSum += v;

        return lastSum / smoothQueue.Count;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WorldLight))]
public class WorldLightEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WorldLight wl = (WorldLight)target;

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Toggle"))
            wl.ToggleLight();

        GUILayout.EndHorizontal();
    }
}
#endif
