using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;      // ← Required for UniTask
using VLB;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class WorldLight : MonoBehaviour
{
    [Header("Lights")]
    [SerializeField] private List<Light> bulbList = new List<Light>();
    [SerializeField] private List<Renderer> rendererList = new List<Renderer>();
    [SerializeField] private List<VolumetricLightBeamHD> lightBeams = new List<VolumetricLightBeamHD>();

    public bool lightOn = false;
    [SerializeField] private bool lightFlickers = false;

    [Header("Flicker Settings")]
    [SerializeField] private int smoothing = 5;
    [SerializeField] private float beamFlickerMultiplier = 1f;

    // Base values
    private List<float> baseIntensities = new List<float>();
    private List<float> baseBeamIntensities = new List<float>();

    private Queue<float> smoothQueue = new Queue<float>();
    private float lastSum = 0f;

    // Cached materials
    private List<Material[]> materialCache = new List<Material[]>();

    private static readonly int HDRP_EmissiveColor = Shader.PropertyToID("_EmissiveColor");

    private CancellationTokenSource _flickerCts;

    private void Start()
    {
        CacheBaseValues();
        CacheMaterials();
        ApplyLightState();

        if (lightFlickers && lightOn)
            StartFlicker();
    }

    private void CacheBaseValues()
    {
        baseIntensities.Clear();
        baseBeamIntensities.Clear();

        foreach (var light in bulbList) baseIntensities.Add(light != null ? light.intensity : 1f);

        foreach (var beam in lightBeams) baseBeamIntensities.Add(beam != null ? beam.intensity : 1f);
    }

    private void CacheMaterials()
    {
        materialCache.Clear();

        foreach (var rend in rendererList)
        {
            if (rend == null) 
            {
                materialCache.Add(null);
                continue;
            }

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

    public void ToggleLight()
    {
        lightOn = !lightOn;
        ApplyLightState();

        if (lightFlickers && lightOn)
            StartFlicker();
        else
            StopFlicker();
    }

    private void StartFlicker()
    {
        StopFlicker();
        _flickerCts = new CancellationTokenSource();
        LightFlickerAsync(_flickerCts.Token).Forget();
    }

    private void StopFlicker()
    {
        _flickerCts?.Cancel();
        _flickerCts?.Dispose();
        _flickerCts = null;
    }

    private async UniTask LightFlickerAsync(CancellationToken token)
    {
        while (lightOn && !token.IsCancellationRequested)
        {
            float flickerValue = GetSmoothedRandom();

            // Flicker Unity Lights + Emission
            for (int i = 0; i < bulbList.Count; i++)
            {
                var light = bulbList[i];
                if (light != null && light.enabled)
                {
                    float v = flickerValue * baseIntensities[i];
                    light.intensity = v;

                    Color finalColor = light.color * v / 5f;

                    if (i < materialCache.Count)
                    {
                        var mats = materialCache[i];
                        foreach (var m in mats)
                        {
                            if (m != null && m.name.ToLower().Contains("bulb"))
                            {
                                m.SetColor(HDRP_EmissiveColor, finalColor);
                            }
                        }
                    }
                }
            }

            // Flicker Volumetric Beams
            for (int i = 0; i < lightBeams.Count; i++)
            {
                var beam = lightBeams[i];
                if (beam != null && beam.enabled)
                {
                    beam.intensity = baseBeamIntensities[i] * flickerValue * beamFlickerMultiplier;
                    beam.UpdateAfterManualPropertyChange();
                }
            }

            await UniTask.WaitForSeconds(0.05f, cancellationToken: token);
        }
    }

    private void ApplyLightState()
    {
        // Unity Lights
        for (int i = 0; i < bulbList.Count; i++)
        {
            var light = bulbList[i];
            if (light != null)
            {
                light.enabled = lightOn;
                light.intensity = lightOn ? baseIntensities[i] : 0f;
                UpdateBulbEmission(light, i);
            }
        }

        // Volumetric Beams
        for (int i = 0; i < lightBeams.Count; i++)
        {
            var beam = lightBeams[i];
            if (beam != null)
            {
                beam.enabled = lightOn;
                beam.intensity = lightOn ? baseBeamIntensities[i] : 0f;
                beam.UpdateAfterManualPropertyChange();
            }
        }
    }

    private void UpdateBulbEmission(Light light, int index)
    {
        if (index >= materialCache.Count) return;

        float intensity = lightOn ? light.intensity : 0f;
        Color col = light.color * intensity / 5f;

        var mats = materialCache[index];
        foreach (var m in mats)
        {
            if (m != null && m.name.ToLower().Contains("bulb"))
            {
                m.SetColor(HDRP_EmissiveColor, col);
            }
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

    private void OnDestroy()
    {
        StopFlicker();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WorldLight))]
public class WorldLightEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        if (GUILayout.Button("Toggle Light", GUILayout.Height(35)))
        {
            ((WorldLight)target).ToggleLight();
        }
    }
}
#endif