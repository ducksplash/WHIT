using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
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

    [Header("State")]
    public bool lightOn = false;
    [SerializeField] private bool lightFlickers = false;

    [Header("Flicker Settings")]
    [SerializeField] private int smoothing = 5;

    [Tooltip("0.1 = ±10% flicker")]
    [SerializeField] private float flickerIntensity = 1f;

    [SerializeField] private float beamFlickerMultiplier = 1f;

    // ================= BASE VALUES =================

    private List<float> baseIntensities = new List<float>();
    private List<float> baseBeamIntensities = new List<float>();

    // Cached materials + emission colors
    private List<Material[]> materialCache = new List<Material[]>();
    private List<Color[]> baseEmissionColors = new List<Color[]>();

    // Flicker smoothing
    private Queue<float> smoothQueue = new Queue<float>();
    private float lastSum = 0f;

    private static readonly int HDRP_EmissiveColor =
        Shader.PropertyToID("_EmissiveColor");

    private CancellationTokenSource _flickerCts;

    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        CacheBaseValues();
        CacheMaterials();

        ApplyLightState();

        if (lightFlickers && lightOn)
            StartFlicker();
    }

    private void OnDestroy()
    {
        StopFlicker();
    }

    // =========================================================
    // CACHE
    // =========================================================

    private void CacheBaseValues()
    {
        baseIntensities.Clear();
        baseBeamIntensities.Clear();

        foreach (var light in bulbList)
        {
            baseIntensities.Add(light != null ? light.intensity : 1f);
        }

        foreach (var beam in lightBeams)
        {
            baseBeamIntensities.Add(beam != null ? beam.intensity : 1f);
        }
    }

    private void CacheMaterials()
    {
        materialCache.Clear();
        baseEmissionColors.Clear();

        foreach (var rend in rendererList)
        {
            if (rend == null)
            {
                materialCache.Add(null);
                baseEmissionColors.Add(null);
                continue;
            }

            var mats = rend.materials;

            materialCache.Add(mats);

            Color[] emissionCols = new Color[mats.Length];

            for (int i = 0; i < mats.Length; i++)
            {
                var mat = mats[i];

                if (mat == null)
                    continue;

                // HDRP emission setup
                mat.SetFloat("_UseEmissiveColor", 1f);
                mat.SetFloat("_EmissiveExposureWeight", 1f);

                mat.EnableKeyword("_EMISSIVE_COLOR");
                mat.EnableKeyword("_EMISSION");

                emissionCols[i] =
                    mat.GetColor(HDRP_EmissiveColor);
            }

            baseEmissionColors.Add(emissionCols);
        }
    }

    // =========================================================
    // TOGGLE
    // =========================================================

    public void ToggleLight()
    {
        lightOn = !lightOn;

        ApplyLightState();

        if (lightFlickers && lightOn)
            StartFlicker();
        else
            StopFlicker();
    }

    // =========================================================
    // APPLY STATE
    // =========================================================

    private void ApplyLightState()
    {
        // ================= LIGHTS =================

        for (int i = 0; i < bulbList.Count; i++)
        {
            var light = bulbList[i];

            if (light == null)
                continue;

            light.enabled = lightOn;

            light.intensity =
                lightOn
                ? baseIntensities[i]
                : 0f;
        }

        // ================= EMISSIVE RENDERERS =================

        for (int i = 0; i < materialCache.Count; i++)
        {
            var mats = materialCache[i];

            if (mats == null)
                continue;

            for (int j = 0; j < mats.Length; j++)
            {
                var mat = mats[j];

                if (mat == null)
                    continue;

                Color emission =
                    lightOn
                    ? baseEmissionColors[i][j]
                    : Color.black;

                mat.SetColor(HDRP_EmissiveColor, emission);
            }
        }

        // ================= VOLUMETRIC BEAMS =================

        for (int i = 0; i < lightBeams.Count; i++)
        {
            var beam = lightBeams[i];

            if (beam == null)
                continue;

            beam.enabled = lightOn;

            beam.intensity =
                lightOn
                ? baseBeamIntensities[i]
                : 0f;

            beam.UpdateAfterManualPropertyChange();
        }
    }

    // =========================================================
    // FLICKER
    // =========================================================

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
            float flickerMultiplier = GetSmoothedRandom();

            // ================= LIGHTS =================

            for (int i = 0; i < bulbList.Count; i++)
            {
                var light = bulbList[i];

                if (light == null || !light.enabled)
                    continue;

                light.intensity =
                    baseIntensities[i] *
                    flickerMultiplier;
            }

            // ================= EMISSIVE MATERIALS =================

            for (int i = 0; i < materialCache.Count; i++)
            {
                var mats = materialCache[i];

                if (mats == null)
                    continue;

                for (int j = 0; j < mats.Length; j++)
                {
                    var mat = mats[j];

                    if (mat == null)
                        continue;

                    Color baseEmission =
                        baseEmissionColors[i][j];

                    Color flickeredEmission =
                        baseEmission *
                        flickerMultiplier;

                    mat.SetColor(
                        HDRP_EmissiveColor,
                        flickeredEmission
                    );
                }
            }

            // ================= VOLUMETRIC BEAMS =================

            for (int i = 0; i < lightBeams.Count; i++)
            {
                var beam = lightBeams[i];

                if (beam == null || !beam.enabled)
                    continue;

                beam.intensity =
                    baseBeamIntensities[i] *
                    flickerMultiplier *
                    beamFlickerMultiplier;

                beam.UpdateAfterManualPropertyChange();
            }

            await UniTask.WaitForSeconds(
                0.05f,
                cancellationToken: token
            );
        }
    }

    // =========================================================
    // RANDOM
    // =========================================================

    private float GetSmoothedRandom()
    {
        while (smoothQueue.Count >= smoothing)
        {
            lastSum -= smoothQueue.Dequeue();
        }

        float min = 1f - flickerIntensity;
        float max = 1f + flickerIntensity;

        float v = Random.Range(min, max);

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

        EditorGUILayout.Space();

        if (GUILayout.Button("Toggle Light", GUILayout.Height(35)))
        {
            ((WorldLight)target).ToggleLight();
        }
    }
}
#endif