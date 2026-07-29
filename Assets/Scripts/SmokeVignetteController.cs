using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif


public enum RotationDirection
{
    Clockwise = 1,
    CounterClockwise = -1
}


[System.Serializable]
public class SmokeImageLayer
{
    public Texture2D image;
    public float rotationSpeed = 0.1f;
    public RotationDirection rotationDirection = RotationDirection.Clockwise;
    public Color startColor = Color.white;
    public Color endColor = Color.white;
    public bool colorLerpEnabled = false;
    public float colorLerpSpeed = 1f;
    public bool isActive = true;
    [Range(0f, 1f)] public float vignetteRadius = 0.4f;
    [Range(0f, 1f)] public float vignetteSoftness = 0.4f;
    [Range(0f, 1f)] public float vignetteIntensity = 1f;
    [System.NonSerialized] public float currentAlpha = 0f;
}


[System.Serializable]
public class SmokeRoutineEntry
{
    public int layerIndex;
    public float startTime;
    public float duration;
}


[System.Serializable]
public class SmokeRoutineLayerData
{
    public int layerIndex;
    public string imageFileName;
    public float rotationSpeed = 0.1f;
    public RotationDirection rotationDirection = RotationDirection.Clockwise;
    public Color startColor = Color.white;
    public Color endColor = Color.white;
    public bool colorLerpEnabled = false;
    public float colorLerpSpeed = 1f;
    public bool isActive = true;
    public float vignetteRadius = 0.4f;
    public float vignetteSoftness = 0.4f;
    public float vignetteIntensity = 1f;

    // Preserves the live texture reference across in-memory plays so we never
    // need to round-trip through Resources.Load unless this came from disk (JSON).
    [System.NonSerialized] public Texture2D image;
}


[System.Serializable]
public class SmokeRoutine
{
    public string routineName;
    public List<SmokeRoutineEntry> entries = new List<SmokeRoutineEntry>();
    public List<SmokeRoutineLayerData> layerData = new List<SmokeRoutineLayerData>();
}


[System.Serializable]
public class SmokeVignetteCustomPass : CustomPass
{
    public SmokeVignetteController controller;

    MaterialPropertyBlock mpb;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        mpb = new MaterialPropertyBlock();
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if (controller == null || controller.smokeMaterial == null || !controller.isRunning)
            return;

        controller.ApplyToMaterialPropertyBlock(mpb);
        CoreUtils.SetRenderTarget(ctx.cmd, ctx.cameraColorBuffer);
        CoreUtils.DrawFullScreen(ctx.cmd, controller.smokeMaterial, mpb);
    }

    protected override void Cleanup() { }
}


[ExecuteAlways]
public class SmokeVignetteController : MonoBehaviour
{
    public const int MAX_LAYERS = 8;
    private const string RoutineFolderName = "SmokeRoutines";
    private const string ResourcesRoutinePath = "SmokeRoutines";
    private const float LayerFadeDuration = 0.5f;

    public Material smokeMaterial;
    public List<SmokeImageLayer> layers = new List<SmokeImageLayer>();

    public bool isRunning = true;

    public string currentRoutineName = "New Routine";
    public List<SmokeRoutineEntry> routineEntries = new List<SmokeRoutineEntry>();

    private CancellationTokenSource routineCts;

    public bool IsRoutinePlaying { get; private set; }

    [Header("UI Target")]
    public UnityEngine.UI.Graphic targetGraphic;
    public bool useUIMode = true;
    Material runtimeMaterial;


    void Awake()
    {
        if (useUIMode && targetGraphic != null && smokeMaterial != null)
        {
            // Create a unique material instance (important for arrays)
            runtimeMaterial = Instantiate(smokeMaterial);
            targetGraphic.material = runtimeMaterial;
        }
    }

    void OnDestroy()
    {
        if (runtimeMaterial != null)
        {
            
            
            #if UNITY_EDITOR
            DestroyImmediate(runtimeMaterial);
            #else
            Destroy(runtimeMaterial);
            #endif
            
        }
    }

    void LateUpdate()
    {
        if (!isRunning) return;

        if (useUIMode && runtimeMaterial != null)
        {
            ApplyToMaterial(runtimeMaterial);
        }
    }

    public void ApplyToMaterial(Material mat)
    {
        List<SmokeImageLayer> activeLayers = layers
            .Where(l => l.isActive || l.currentAlpha > 0.001f)
            .ToList();

        int count = Mathf.Min(activeLayers.Count, MAX_LAYERS);

        float totalAlpha = 0f;
        for (int i = 0; i < count; i++)
            totalAlpha += activeLayers[i].currentAlpha;
        float normalizer = Mathf.Max(1f, totalAlpha);

        float[] rotSpeed = new float[MAX_LAYERS];
        float[] rotDir = new float[MAX_LAYERS];
        Vector4[] startColors = new Vector4[MAX_LAYERS];
        Vector4[] endColors = new Vector4[MAX_LAYERS];
        float[] lerpEnabled = new float[MAX_LAYERS];
        float[] lerpSpeed = new float[MAX_LAYERS];
        float[] layerAlpha = new float[MAX_LAYERS];
        float[] vignetteRadius = new float[MAX_LAYERS];
        float[] vignetteSoftness = new float[MAX_LAYERS];
        float[] vignetteIntensity = new float[MAX_LAYERS];

        for (int i = 0; i < count; i++)
        {
            var l = activeLayers[i];
            rotSpeed[i] = l.rotationSpeed;
            rotDir[i] = (int)l.rotationDirection;
            startColors[i] = l.startColor;
            endColors[i] = l.endColor;
            lerpEnabled[i] = l.colorLerpEnabled ? 1f : 0f;
            lerpSpeed[i] = l.colorLerpSpeed;
            layerAlpha[i] = l.currentAlpha / normalizer;
            vignetteRadius[i] = l.vignetteRadius;
            vignetteSoftness[i] = l.vignetteSoftness;
            vignetteIntensity[i] = l.vignetteIntensity;

            mat.SetTexture("_Tex" + i, l.image != null ? l.image : Texture2D.blackTexture);
        }

        for (int i = count; i < MAX_LAYERS; i++)
            mat.SetTexture("_Tex" + i, Texture2D.blackTexture);

        mat.SetFloat("_LayerCount", count);
        mat.SetFloatArray("_LayerAlpha", layerAlpha);
        mat.SetFloatArray("_RotSpeed", rotSpeed);
        mat.SetFloatArray("_RotDir", rotDir);
        mat.SetVectorArray("_StartColor", startColors);
        mat.SetVectorArray("_EndColor", endColors);
        mat.SetFloatArray("_LerpEnabled", lerpEnabled);
        mat.SetFloatArray("_LerpSpeed", lerpSpeed);
        mat.SetFloatArray("_VignetteRadius", vignetteRadius);
        mat.SetFloatArray("_VignetteSoftness", vignetteSoftness);
        mat.SetFloatArray("_VignetteIntensity", vignetteIntensity);
    }

    private void Start()
    {
        EventManager.OnStartThoughtVignette += PlayThoughtRoutine;
    }

    private void PlayThoughtRoutine()
    {
        List<string> savedRoutines = GetSavedRoutineNames();
        if (savedRoutines == null || savedRoutines.Count == 0)
        {
            Debug.LogWarning("[SmokeVignette] No routines found in Resources/SmokeRoutines");
            return;
        }

        SmokeRoutine loaded = LoadRoutine(savedRoutines[0]);
        if (loaded != null)
            PlayRoutine(loaded);
    }

    public void StartEffect()
    {
        isRunning = true;
    }

    public void StopEffect()
    {
        isRunning = false;
    }

    public void ToggleEffect()
    {
        isRunning = !isRunning;
    }

    public void AddImage(Texture2D image, float rotationSpeed, RotationDirection direction, Color startColor, Color endColor, bool colorLerpEnabled, float colorLerpSpeed, float vignetteRadius = 0.4f, float vignetteSoftness = 0.4f, float vignetteIntensity = 1f)
    {
        if (layers.Count >= MAX_LAYERS)
            return;

        layers.Add(new SmokeImageLayer
        {
            image = image,
            rotationSpeed = rotationSpeed,
            rotationDirection = direction,
            startColor = startColor,
            endColor = endColor,
            colorLerpEnabled = colorLerpEnabled,
            colorLerpSpeed = colorLerpSpeed,
            isActive = true,
            vignetteRadius = vignetteRadius,
            vignetteSoftness = vignetteSoftness,
            vignetteIntensity = vignetteIntensity
        });
    }

    public void RemoveImageAt(int index)
    {
        if (index >= 0 && index < layers.Count)
            layers.RemoveAt(index);
    }

    public void RemoveImage(Texture2D image)
    {
        layers.RemoveAll(l => l.image == image);
    }

    public void ClearImages()
    {
        layers.Clear();
    }

    public void SetRotationSpeed(int index, float speed)
    {
        if (index >= 0 && index < layers.Count)
            layers[index].rotationSpeed = speed;
    }

    public void SetRotationDirection(int index, RotationDirection direction)
    {
        if (index >= 0 && index < layers.Count)
            layers[index].rotationDirection = direction;
    }

    public void SetStartColor(int index, Color color)
    {
        if (index >= 0 && index < layers.Count)
            layers[index].startColor = color;
    }

    public void SetEndColor(int index, Color color)
    {
        if (index >= 0 && index < layers.Count)
            layers[index].endColor = color;
    }

    public void SetColorLerpEnabled(int index, bool enabled)
    {
        if (index >= 0 && index < layers.Count)
            layers[index].colorLerpEnabled = enabled;
    }

    public void SetColorLerpSpeed(int index, float speed)
    {
        if (index >= 0 && index < layers.Count)
            layers[index].colorLerpSpeed = speed;
    }

    public void SetLayerActive(int index, bool active)
    {
        if (index >= 0 && index < layers.Count)
            layers[index].isActive = active;
    }

    public void SetVignetteRadius(int index, float radius)
    {
        if (index >= 0 && index < layers.Count)
            layers[index].vignetteRadius = radius;
    }

    public void SetVignetteSoftness(int index, float softness)
    {
        if (index >= 0 && index < layers.Count)
            layers[index].vignetteSoftness = softness;
    }

    public void SetVignetteIntensity(int index, float intensity)
    {
        if (index >= 0 && index < layers.Count)
            layers[index].vignetteIntensity = intensity;
    }

    public SmokeRoutine BuildRoutineFromCurrentState()
    {
        SmokeRoutine routine = new SmokeRoutine
        {
            routineName = currentRoutineName,
            entries = new List<SmokeRoutineEntry>(),
            layerData = new List<SmokeRoutineLayerData>()
        };

        foreach (var entry in routineEntries)
        {
            routine.entries.Add(new SmokeRoutineEntry
            {
                layerIndex = entry.layerIndex,
                startTime = entry.startTime,
                duration = entry.duration
            });
        }

        for (int i = 0; i < layers.Count; i++)
        {
            SmokeImageLayer layer = layers[i];
            routine.layerData.Add(new SmokeRoutineLayerData
            {
                layerIndex = i,
                imageFileName = layer.image != null ? layer.image.name : "",
                image = layer.image,
                rotationSpeed = layer.rotationSpeed,
                rotationDirection = layer.rotationDirection,
                startColor = layer.startColor,
                endColor = layer.endColor,
                colorLerpEnabled = layer.colorLerpEnabled,
                colorLerpSpeed = layer.colorLerpSpeed,
                isActive = layer.isActive,
                vignetteRadius = layer.vignetteRadius,
                vignetteSoftness = layer.vignetteSoftness,
                vignetteIntensity = layer.vignetteIntensity
            });
        }

        return routine;
    }

    public void ApplyRoutineToState(SmokeRoutine routine)
    {
        if (routine == null)
            return;

        currentRoutineName = routine.routineName;

        routineEntries = new List<SmokeRoutineEntry>();
        foreach (var e in routine.entries)
        {
            routineEntries.Add(new SmokeRoutineEntry
            {
                layerIndex = e.layerIndex,
                startTime = e.startTime,
                duration = e.duration
            });
        }

        int maxIndex = -1;
        foreach (var ld in routine.layerData)
            maxIndex = Mathf.Max(maxIndex, Mathf.Min(ld.layerIndex, MAX_LAYERS - 1));

        SmokeImageLayer[] rebuilt = new SmokeImageLayer[maxIndex + 1];
        for (int i = 0; i <= maxIndex; i++)
            rebuilt[i] = new SmokeImageLayer { isActive = false, currentAlpha = 0f };

        foreach (var ld in routine.layerData)
        {
            if (ld.layerIndex < 0 || ld.layerIndex > maxIndex)
                continue;

            Texture2D resolvedImage = ld.image != null
                ? ld.image
                : (!string.IsNullOrEmpty(ld.imageFileName) ? Resources.Load<Texture2D>(ld.imageFileName) : null);

            rebuilt[ld.layerIndex] = new SmokeImageLayer
            {
                image = resolvedImage,
                rotationSpeed = ld.rotationSpeed,
                rotationDirection = ld.rotationDirection,
                startColor = ld.startColor,
                endColor = ld.endColor,
                colorLerpEnabled = ld.colorLerpEnabled,
                colorLerpSpeed = ld.colorLerpSpeed,
                isActive = ld.isActive,
                vignetteRadius = ld.vignetteRadius,
                vignetteSoftness = ld.vignetteSoftness,
                vignetteIntensity = ld.vignetteIntensity,
                currentAlpha = 0f
            };
        }

        layers = new List<SmokeImageLayer>(rebuilt);
    }

    public void PlayRoutine(SmokeRoutine routine)
    {
        StopRoutine();

        if (routine == null)
            return;

        ApplyRoutineToState(routine);

        routineCts = new CancellationTokenSource();
        PlayRoutineInternal(routineCts.Token).Forget();
    }

    public void StopRoutine()
    {
        if (routineCts != null)
        {
            routineCts.Cancel();
            routineCts.Dispose();
            routineCts = null;
        }

        IsRoutinePlaying = false;
    }

    private async UniTaskVoid PlayRoutineInternal(CancellationToken token)
    {
        IsRoutinePlaying = true;

        try
        {
            foreach (var layer in layers)
            {
                layer.isActive = false;
                layer.currentAlpha = 0f;
            }

            var tasks = new List<UniTask>();

            foreach (var entry in routineEntries)
                tasks.Add(PlayEntryAsync(entry, token));

            await UniTask.WhenAll(tasks);
        }
        catch (System.OperationCanceledException)
        {
        }
        finally
        {
            IsRoutinePlaying = false;
        }
    }

    private async UniTask PlayEntryAsync(SmokeRoutineEntry entry, CancellationToken token)
    {
        if (entry.startTime > 0f)
            await UniTask.Delay(System.TimeSpan.FromSeconds(entry.startTime), cancellationToken: token);

        if (entry.layerIndex < 0 || entry.layerIndex >= layers.Count)
            return;

        SmokeImageLayer layer = layers[entry.layerIndex];
        layer.isActive = true;

        float fadeIn = Mathf.Min(LayerFadeDuration, entry.duration * 0.5f);
        float fadeOut = Mathf.Min(LayerFadeDuration, entry.duration * 0.5f);
        float holdTime = Mathf.Max(0f, entry.duration - fadeIn - fadeOut);

        await FadeLayerAlpha(layer, 0f, 1f, fadeIn, token);

        if (holdTime > 0f)
            await UniTask.Delay(System.TimeSpan.FromSeconds(holdTime), cancellationToken: token);

        await FadeLayerAlpha(layer, 1f, 0f, fadeOut, token);

        if (entry.layerIndex >= 0 && entry.layerIndex < layers.Count)
            layers[entry.layerIndex].isActive = false;
    }

    private async UniTask FadeLayerAlpha(SmokeImageLayer layer, float from, float to, float duration, CancellationToken token)
    {
        if (duration <= 0f)
        {
            layer.currentAlpha = to;
            return;
        }

        float elapsed = 0f;
        layer.currentAlpha = from;

        while (elapsed < duration)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, token);
            elapsed += Time.deltaTime;
            layer.currentAlpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
        }

        layer.currentAlpha = to;
    }

    // ─────────────────────────────────────────────────────────────
    // Resources-based routine storage (ships with the build)
    // Files live at: Assets/Resources/SmokeRoutines/YourRoutine.json
    // Load path:     "SmokeRoutines/YourRoutine"
    // ─────────────────────────────────────────────────────────────

#if UNITY_EDITOR
    private static string GetEditorResourcesDirectory()
    {
        string path = Path.Combine(Application.dataPath, "Resources", RoutineFolderName);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        return path;
    }
#endif

    public void SaveRoutine(SmokeRoutine routine)
    {
        if (routine == null || string.IsNullOrEmpty(routine.routineName))
            return;

#if UNITY_EDITOR
        string path = Path.Combine(GetEditorResourcesDirectory(), routine.routineName + ".json");
        string json = JsonUtility.ToJson(routine, true);
        File.WriteAllText(path, json);
        AssetDatabase.Refresh();
        Debug.Log($"[SmokeVignette] Saved routine to Resources: {path}");
#else
        Debug.LogWarning("[SmokeVignette] SaveRoutine is Editor-only. Routines are shipped via Resources.");
#endif
    }

    public SmokeRoutine LoadRoutine(string routineName)
    {
        if (string.IsNullOrEmpty(routineName))
            return null;

        TextAsset asset = Resources.Load<TextAsset>($"{ResourcesRoutinePath}/{routineName}");
        if (asset == null)
        {
            Debug.LogWarning($"[SmokeVignette] Routine not found in Resources: {ResourcesRoutinePath}/{routineName}");
            return null;
        }

        return JsonUtility.FromJson<SmokeRoutine>(asset.text);
    }

    public void DeleteRoutine(string routineName)
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(routineName))
            return;

        string path = Path.Combine(GetEditorResourcesDirectory(), routineName + ".json");
        if (File.Exists(path))
        {
            File.Delete(path);
            string meta = path + ".meta";
            if (File.Exists(meta))
                File.Delete(meta);

            AssetDatabase.Refresh();
            Debug.Log($"[SmokeVignette] Deleted routine: {routineName}");
        }
#else
        Debug.LogWarning("[SmokeVignette] DeleteRoutine is Editor-only.");
#endif
    }

    public List<string> GetSavedRoutineNames()
    {
        List<string> names = new List<string>();

        TextAsset[] assets = Resources.LoadAll<TextAsset>(ResourcesRoutinePath);
        foreach (var asset in assets)
        {
            if (asset != null)
                names.Add(asset.name);
        }

        names.Sort();
        return names;
    }

    public void ApplyToMaterialPropertyBlock(MaterialPropertyBlock block)
    {
        List<SmokeImageLayer> activeLayers = layers.Where(l => l.isActive || l.currentAlpha > 0.001f).ToList();
        int count = Mathf.Min(activeLayers.Count, MAX_LAYERS);

        float totalAlpha = 0f;
        for (int i = 0; i < count; i++)
            totalAlpha += activeLayers[i].currentAlpha;

        float normalizer = Mathf.Max(1f, totalAlpha);

        float[] rotSpeed = new float[MAX_LAYERS];
        float[] rotDir = new float[MAX_LAYERS];
        Vector4[] startColors = new Vector4[MAX_LAYERS];
        Vector4[] endColors = new Vector4[MAX_LAYERS];
        float[] lerpEnabled = new float[MAX_LAYERS];
        float[] lerpSpeed = new float[MAX_LAYERS];
        float[] layerAlpha = new float[MAX_LAYERS];
        float[] vignetteRadiusArr = new float[MAX_LAYERS];
        float[] vignetteSoftnessArr = new float[MAX_LAYERS];
        float[] vignetteIntensityArr = new float[MAX_LAYERS];

        for (int i = 0; i < count; i++)
        {
            SmokeImageLayer l = activeLayers[i];
            rotSpeed[i] = l.rotationSpeed;
            rotDir[i] = (int)l.rotationDirection;
            startColors[i] = l.startColor;
            endColors[i] = l.endColor;
            lerpEnabled[i] = l.colorLerpEnabled ? 1f : 0f;
            lerpSpeed[i] = l.colorLerpSpeed;
            layerAlpha[i] = l.currentAlpha / normalizer;
            vignetteRadiusArr[i] = l.vignetteRadius;
            vignetteSoftnessArr[i] = l.vignetteSoftness;
            vignetteIntensityArr[i] = l.vignetteIntensity;

            block.SetTexture("_Tex" + i, l.image != null ? l.image : Texture2D.blackTexture);
        }

        for (int i = count; i < MAX_LAYERS; i++)
        {
            block.SetTexture("_Tex" + i, Texture2D.blackTexture);
        }

        block.SetFloat("_LayerCount", count);
        block.SetFloatArray("_LayerAlpha", layerAlpha);
        block.SetFloatArray("_RotSpeed", rotSpeed);
        block.SetFloatArray("_RotDir", rotDir);
        block.SetVectorArray("_StartColor", startColors);
        block.SetVectorArray("_EndColor", endColors);
        block.SetFloatArray("_LerpEnabled", lerpEnabled);
        block.SetFloatArray("_LerpSpeed", lerpSpeed);
        block.SetFloatArray("_VignetteRadius", vignetteRadiusArr);
        block.SetFloatArray("_VignetteSoftness", vignetteSoftnessArr);
        block.SetFloatArray("_VignetteIntensity", vignetteIntensityArr);
    }
}


#if UNITY_EDITOR

[CustomEditor(typeof(SmokeVignetteController))]
public class SmokeVignetteControllerEditor : Editor
{
    private SmokeVignetteController controller;
    private ReorderableList layersList;
    private ReorderableList routineEntriesList;
    private int selectedRoutineIndex = 0;

    private SerializedProperty smokeMaterialProp;
    private SerializedProperty layersProp;
    private SerializedProperty isRunningProp;
    private SerializedProperty currentRoutineNameProp;
    private SerializedProperty routineEntriesProp;
    private Texture2D quickAddTexture;
    private SerializedProperty targetGraphicProp;
    private SerializedProperty useUIModeProp;

    private void OnEnable()
    {
        controller = (SmokeVignetteController)target;

        smokeMaterialProp = serializedObject.FindProperty("smokeMaterial");
        layersProp = serializedObject.FindProperty("layers");
        isRunningProp = serializedObject.FindProperty("isRunning");
        currentRoutineNameProp = serializedObject.FindProperty("currentRoutineName");
        routineEntriesProp = serializedObject.FindProperty("routineEntries");
        targetGraphicProp = serializedObject.FindProperty("targetGraphic");
        useUIModeProp = serializedObject.FindProperty("useUIMode");

        layersList = new ReorderableList(serializedObject, layersProp, true, true, true, true);

        layersList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Smoke Image Layers");
        };

        layersList.elementHeightCallback = (int index) =>
        {
            return EditorGUI.GetPropertyHeight(layersProp.GetArrayElementAtIndex(index), true) + 4;
        };

        layersList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = layersProp.GetArrayElementAtIndex(index);
            rect.y += 2;
            rect.height = EditorGUI.GetPropertyHeight(element, true);
            EditorGUI.PropertyField(rect, element, new GUIContent($"Layer {index}"), true);
        };

        layersList.onAddCallback = (ReorderableList list) =>
        {
            if (controller.layers.Count >= SmokeVignetteController.MAX_LAYERS)
            {
                EditorUtility.DisplayDialog("Max Layers",
                    $"Cannot add more than {SmokeVignetteController.MAX_LAYERS} layers.", "OK");
                return;
            }

            list.serializedProperty.arraySize++;
            var newElement = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);

            newElement.FindPropertyRelative("rotationSpeed").floatValue = 0.1f;
            newElement.FindPropertyRelative("rotationDirection").enumValueIndex = 0;
            newElement.FindPropertyRelative("startColor").colorValue = Color.white;
            newElement.FindPropertyRelative("endColor").colorValue = Color.white;
            newElement.FindPropertyRelative("colorLerpEnabled").boolValue = false;
            newElement.FindPropertyRelative("colorLerpSpeed").floatValue = 1f;
            newElement.FindPropertyRelative("isActive").boolValue = true;
            newElement.FindPropertyRelative("vignetteRadius").floatValue = 0.4f;
            newElement.FindPropertyRelative("vignetteSoftness").floatValue = 0.4f;
            newElement.FindPropertyRelative("vignetteIntensity").floatValue = 1f;
        };

        routineEntriesList = new ReorderableList(serializedObject, routineEntriesProp, true, true, true, true);

        routineEntriesList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Routine Entries");
        };

        routineEntriesList.elementHeightCallback = (int index) =>
        {
            return (EditorGUIUtility.singleLineHeight + 2f) * 3 + 4f;
        };

        routineEntriesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = routineEntriesProp.GetArrayElementAtIndex(index);
            var layerIndexProp = element.FindPropertyRelative("layerIndex");
            var startTimeProp = element.FindPropertyRelative("startTime");
            var durationProp = element.FindPropertyRelative("duration");

            float y = rect.y + 2;
            float rowHeight = EditorGUIUtility.singleLineHeight;
            float spacing = 2f;

            int maxLayerIndex = Mathf.Max(0, controller.layers.Count - 1);

            Rect layerRect = new Rect(rect.x, y, rect.width, rowHeight);
            layerIndexProp.intValue = EditorGUI.IntSlider(layerRect, "Layer Index", layerIndexProp.intValue, 0, maxLayerIndex);

            Rect startRect = new Rect(rect.x, y + rowHeight + spacing, rect.width, rowHeight);
            startTimeProp.floatValue = EditorGUI.FloatField(startRect, "Start Time (s)", startTimeProp.floatValue);

            Rect durationRect = new Rect(rect.x, y + (rowHeight + spacing) * 2, rect.width, rowHeight);
            durationProp.floatValue = EditorGUI.FloatField(durationRect, "Duration (s)", durationProp.floatValue);
        };

        routineEntriesList.onAddCallback = (ReorderableList list) =>
        {
            list.serializedProperty.arraySize++;
            var newElement = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            newElement.FindPropertyRelative("layerIndex").intValue = 0;
            newElement.FindPropertyRelative("startTime").floatValue = 0f;
            newElement.FindPropertyRelative("duration").floatValue = 3f;
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Smoke Vignette Controller", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.Toggle("Is Running", controller.isRunning);
            EditorGUILayout.Toggle("Is Routine Playing", controller.IsRoutinePlaying);
        }

        EditorGUILayout.Space(6);

        EditorGUILayout.LabelField("Global Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(smokeMaterialProp);
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("UI Target", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(useUIModeProp);
        EditorGUILayout.PropertyField(targetGraphicProp);
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Start Effect", GUILayout.Height(28)))
            {
                controller.StartEffect();
                EditorUtility.SetDirty(controller);
            }

            if (GUILayout.Button("Stop Effect", GUILayout.Height(28)))
            {
                controller.StopEffect();
                EditorUtility.SetDirty(controller);
            }

            if (GUILayout.Button("Toggle Effect", GUILayout.Height(28)))
            {
                controller.ToggleEffect();
                EditorUtility.SetDirty(controller);
            }
        }

        EditorGUILayout.Space(6);

        EditorGUILayout.LabelField($"Layers ({controller.layers.Count}/{SmokeVignetteController.MAX_LAYERS})", EditorStyles.boldLabel);

        layersList.DoLayoutList();

        EditorGUILayout.Space(4);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Clear All Layers"))
            {
                if (EditorUtility.DisplayDialog("Clear Layers",
                    "Remove all image layers?", "Yes", "Cancel"))
                {
                    controller.ClearImages();
                    EditorUtility.SetDirty(controller);
                }
            }
        }

        if (layersList.index >= 0 && layersList.index < controller.layers.Count)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"Selected Layer Tools (Index {layersList.index})", EditorStyles.boldLabel);

            var layer = controller.layers[layersList.index];

            EditorGUILayout.Space(2);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.backgroundColor = new Color(0.4f, 0.85f, 0.45f);
                if (GUILayout.Button("▶ Play This Layer", GUILayout.Height(28)))
                {
                    controller.StartEffect();

                    for (int i = 0; i < controller.layers.Count; i++)
                    {
                        if (i == layersList.index)
                        {
                            controller.layers[i].isActive = true;
                            controller.layers[i].currentAlpha = 1f;
                        }
                        else
                        {
                            controller.layers[i].isActive = false;
                            controller.layers[i].currentAlpha = 0f;
                        }
                    }

                    EditorUtility.SetDirty(controller);
                    SceneView.RepaintAll();
                }

                GUI.backgroundColor = Color.white;

                if (GUILayout.Button("Stop Layer Preview", GUILayout.Height(28)))
                {
                    controller.SetLayerActive(layersList.index, false);
                    controller.layers[layersList.index].currentAlpha = 0f;
                    EditorUtility.SetDirty(controller);
                    SceneView.RepaintAll();
                }
            }

            EditorGUILayout.Space(6);

            EditorGUI.BeginChangeCheck();
            bool newIsActive = EditorGUILayout.Toggle("Active", layer.isActive);
            if (EditorGUI.EndChangeCheck())
            {
                controller.SetLayerActive(layersList.index, newIsActive);
                EditorUtility.SetDirty(controller);
            }

            EditorGUI.BeginChangeCheck();
            float newSpeed = EditorGUILayout.Slider("Rotation Speed", layer.rotationSpeed, 0f, 5f);
            if (EditorGUI.EndChangeCheck())
            {
                controller.SetRotationSpeed(layersList.index, newSpeed);
                EditorUtility.SetDirty(controller);
            }

            EditorGUI.BeginChangeCheck();
            var newDir = (RotationDirection)EditorGUILayout.EnumPopup("Rotation Direction", layer.rotationDirection);
            if (EditorGUI.EndChangeCheck())
            {
                controller.SetRotationDirection(layersList.index, newDir);
                EditorUtility.SetDirty(controller);
            }

            EditorGUI.BeginChangeCheck();
            Color newStart = EditorGUILayout.ColorField("Start Color", layer.startColor);
            if (EditorGUI.EndChangeCheck())
            {
                controller.SetStartColor(layersList.index, newStart);
                EditorUtility.SetDirty(controller);
            }

            EditorGUI.BeginChangeCheck();
            Color newEnd = EditorGUILayout.ColorField("End Color", layer.endColor);
            if (EditorGUI.EndChangeCheck())
            {
                controller.SetEndColor(layersList.index, newEnd);
                EditorUtility.SetDirty(controller);
            }

            EditorGUI.BeginChangeCheck();
            bool newLerp = EditorGUILayout.Toggle("Color Lerp Enabled", layer.colorLerpEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                controller.SetColorLerpEnabled(layersList.index, newLerp);
                EditorUtility.SetDirty(controller);
            }

            EditorGUI.BeginChangeCheck();
            float newLerpSpeed = EditorGUILayout.Slider("Color Lerp Speed", layer.colorLerpSpeed, 0f, 10f);
            if (EditorGUI.EndChangeCheck())
            {
                controller.SetColorLerpSpeed(layersList.index, newLerpSpeed);
                EditorUtility.SetDirty(controller);
            }

            EditorGUI.BeginChangeCheck();
            float newVigRadius = EditorGUILayout.Slider("Vignette Radius", layer.vignetteRadius, 0f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                controller.SetVignetteRadius(layersList.index, newVigRadius);
                EditorUtility.SetDirty(controller);
            }

            EditorGUI.BeginChangeCheck();
            float newVigSoftness = EditorGUILayout.Slider("Vignette Softness", layer.vignetteSoftness, 0f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                controller.SetVignetteSoftness(layersList.index, newVigSoftness);
                EditorUtility.SetDirty(controller);
            }

            EditorGUI.BeginChangeCheck();
            float newVigIntensity = EditorGUILayout.Slider("Vignette Intensity", layer.vignetteIntensity, 0f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                controller.SetVignetteIntensity(layersList.index, newVigIntensity);
                EditorUtility.SetDirty(controller);
            }

            EditorGUILayout.Space(4);

            if (GUILayout.Button("Remove This Layer", GUILayout.Height(24)))
            {
                controller.RemoveImageAt(layersList.index);
                layersList.index = -1;
                EditorUtility.SetDirty(controller);
            }
        }

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Quick Add Layer", EditorStyles.boldLabel);

        using (new EditorGUILayout.VerticalScope("box"))
        {
            EditorGUILayout.HelpBox("Assign a texture below and click Add. Defaults will be used for other values.", MessageType.Info);

            quickAddTexture = (Texture2D)EditorGUILayout.ObjectField("Texture", quickAddTexture, typeof(Texture2D), false);

            if (quickAddTexture != null && GUILayout.Button("Add Layer with this Texture"))
            {
                if (controller.layers.Count >= SmokeVignetteController.MAX_LAYERS)
                {
                    EditorUtility.DisplayDialog("Max Layers",
                        $"Cannot add more than {SmokeVignetteController.MAX_LAYERS} layers.", "OK");
                }
                else
                {
                    controller.AddImage(
                        quickAddTexture,
                        0.1f,
                        RotationDirection.Clockwise,
                        Color.white,
                        Color.white,
                        false,
                        1f
                    );
                    EditorUtility.SetDirty(controller);
                    quickAddTexture = null;
                }
            }
        }

        EditorGUILayout.Space(14);
        EditorGUILayout.LabelField("Routine Editor", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(currentRoutineNameProp);

        routineEntriesList.DoLayoutList();

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Save Routine", GUILayout.Height(24)))
            {
                serializedObject.ApplyModifiedProperties();
                controller.SaveRoutine(controller.BuildRoutineFromCurrentState());
            }

            if (GUILayout.Button("Play Current Routine", GUILayout.Height(24)))
            {
                serializedObject.ApplyModifiedProperties();
                controller.PlayRoutine(controller.BuildRoutineFromCurrentState());
            }

            if (GUILayout.Button("Stop Routine", GUILayout.Height(24)))
            {
                controller.StopRoutine();
            }
        }

        EditorGUILayout.Space(8);

        List<string> savedRoutines = controller.GetSavedRoutineNames();

        if (savedRoutines.Count > 0)
        {
            selectedRoutineIndex = Mathf.Clamp(selectedRoutineIndex, 0, savedRoutines.Count - 1);
            selectedRoutineIndex = EditorGUILayout.Popup("Saved Routines", selectedRoutineIndex, savedRoutines.ToArray());

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Load Selected"))
                {
                    SmokeRoutine loaded = controller.LoadRoutine(savedRoutines[selectedRoutineIndex]);
                    if (loaded != null)
                    {
                        controller.ApplyRoutineToState(loaded);
                        EditorUtility.SetDirty(controller);
                    }
                }

                if (GUILayout.Button("Play Selected"))
                {
                    SmokeRoutine loaded = controller.LoadRoutine(savedRoutines[selectedRoutineIndex]);
                    if (loaded != null)
                    {
                        controller.PlayRoutine(loaded);
                        EditorUtility.SetDirty(controller);
                    }
                }

                if (GUILayout.Button("Delete Selected"))
                {
                    controller.DeleteRoutine(savedRoutines[selectedRoutineIndex]);
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No saved routines found in Resources/SmokeRoutines.", MessageType.None);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif