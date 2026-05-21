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

    [SerializeField] private List<Renderer> rendererList =
        new List<Renderer>();

    [SerializeField] private List<VolumetricLightBeamHD> lightBeams =
        new List<VolumetricLightBeamHD>();

    public bool lightOn;

    [SerializeField] private bool lightFlickers;

    [Header("Flicker Settings")]
    [SerializeField] private int smoothing = 5;

    [SerializeField] private float beamFlickerMultiplier = 1f;

    // Base intensity per light
    private List<float> baseIntensities =
        new List<float>();

    // Base beam intensity
    private List<float> baseBeamIntensities =
        new List<float>();

    private Queue<float> smoothQueue =
        new Queue<float>();

    private float lastSum = 0f;

    // Cached materials
    private List<Material[]> materialCache =
        new List<Material[]>();

    static readonly int HDRP_EmissiveColor =
        Shader.PropertyToID("_EmissiveColor");

    private void Start()
    {
        baseIntensities.Clear();
        baseBeamIntensities.Clear();

        // Cache original light intensities
        foreach (var light in bulbList)
        {
            if (light != null)
                baseIntensities.Add(light.intensity);
            else
                baseIntensities.Add(1f);
        }

        // Cache original beam intensities
        foreach (var beam in lightBeams)
        {
            if (beam != null)
                baseBeamIntensities.Add(beam.intensity);
            else
                baseBeamIntensities.Add(1f);
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

        ApplyLightState();

        // Start flicker if enabled
        if (lightFlickers && lightOn)
        {
            StopAllCoroutines();
            StartCoroutine(LightFlicker());
        }
    }

    public void ToggleLight()
    {
        lightOn = !lightOn;

        ApplyLightState();

        // Flicker handling
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

    private void ApplyLightState()
    {
        // Apply Unity light state
        for (int i = 0; i < bulbList.Count; i++)
        {
            var light = bulbList[i];

            if (light != null)
            {
                light.enabled = lightOn;

                if (lightOn)
                    light.intensity = baseIntensities[i];
                else
                    light.intensity = 0f;

                UpdateBulbEmission(light, i);
            }
        }

        // Apply beam state
        for (int i = 0; i < lightBeams.Count; i++)
        {
            var beam = lightBeams[i];

            if (beam != null)
            {
                beam.enabled = lightOn;

                if (lightOn)
                {
                    beam.intensity =
                        baseBeamIntensities[i];
                }
                else
                {
                    beam.intensity = 0f;
                }

                beam.UpdateAfterManualPropertyChange();
            }
        }
    }

    private void UpdateBulbEmission(
        Light light,
        int index
    )
    {
        float intensity =
            lightOn ? light.intensity : 0f;

        Color col =
            light.color * intensity * 5f;

        if (index < materialCache.Count)
        {
            var mats = materialCache[index];

            foreach (var m in mats)
            {
                if (m != null &&
                    m.name.ToLower().Contains("bulb"))
                {
                    m.SetColor(
                        HDRP_EmissiveColor,
                        col
                    );
                }
            }
        }
    }

    private IEnumerator LightFlicker()
    {
        while (lightOn)
        {
            float flickerValue =
                GetSmoothedRandom();

            // Flicker lights
            for (int i = 0; i < bulbList.Count; i++)
            {
                var light = bulbList[i];

                if (light != null &&
                    light.enabled)
                {
                    float v =
                        flickerValue *
                        baseIntensities[i];

                    light.intensity = v;

                    Color finalColor =
                        light.color *
                        v *
                        5f;

                    // Update emissive materials
                    if (i < materialCache.Count)
                    {
                        var mats =
                            materialCache[i];

                        foreach (var m in mats)
                        {
                            if (m != null &&
                                m.name
                                    .ToLower()
                                    .Contains("bulb"))
                            {
                                m.SetColor(
                                    HDRP_EmissiveColor,
                                    finalColor
                                );
                            }
                        }
                    }
                }
            }

            // Flicker volumetric beams
            for (int i = 0; i < lightBeams.Count; i++)
            {
                var beam = lightBeams[i];

                if (beam != null &&
                    beam.enabled)
                {
                    beam.intensity =
                        baseBeamIntensities[i] *
                        flickerValue *
                        beamFlickerMultiplier;

                    beam.UpdateAfterManualPropertyChange();
                }
            }

            yield return new WaitForSeconds(0.05f);
        }
    }

    private float GetSmoothedRandom()
    {
        while (smoothQueue.Count >= smoothing)
            lastSum -= smoothQueue.Dequeue();

        float v = Random.Range(0f, 1f);

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

        WorldLight wl =
            (WorldLight)target;

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Toggle"))
            wl.ToggleLight();

        GUILayout.EndHorizontal();
    }
}
#endif