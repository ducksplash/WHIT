using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class ConcentricCircleDriver : MonoBehaviour
{
    [Header("Volume")]
    public Volume volume;

    [Header("Ring Rotations (20 rings)")]
    [Range(-360f, 360f)] public float ring0 = 0f;
    [Range(-360f, 360f)] public float ring1 = 0f;
    [Range(-360f, 360f)] public float ring2 = 0f;
    [Range(-360f, 360f)] public float ring3 = 0f;
    [Range(-360f, 360f)] public float ring4 = 0f;
    [Range(-360f, 360f)] public float ring5 = 0f;
    [Range(-360f, 360f)] public float ring6 = 0f;
    [Range(-360f, 360f)] public float ring7 = 0f;
    [Range(-360f, 360f)] public float ring8 = 0f;
    [Range(-360f, 360f)] public float ring9 = 0f;
    [Range(-360f, 360f)] public float ring10 = 0f;
    [Range(-360f, 360f)] public float ring11 = 0f;
    [Range(-360f, 360f)] public float ring12 = 0f;
    [Range(-360f, 360f)] public float ring13 = 0f;
    [Range(-360f, 360f)] public float ring14 = 0f;
    [Range(-360f, 360f)] public float ring15 = 0f;
    [Range(-360f, 360f)] public float ring16 = 0f;
    [Range(-360f, 360f)] public float ring17 = 0f;
    [Range(-360f, 360f)] public float ring18 = 0f;
    [Range(-360f, 360f)] public float ring19 = 0f;

    public Color tintColor = Color.white;
    
    [Header("Swirl / Zigzag Settings")]
    public float swirlDuration = 2.5f;
    public float targetAngle = 180f;
    [Range(0.05f, 0.7f)] public float staggerFraction = 0.4f;
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Slide Settings")]
    public float slideDuration = 2.5f;
    [Range(0.01f, 1f)] public float slideDistance = 0.3f;
    [Range(0.05f, 0.7f)] public float slideStaggerFraction = 0.4f;

    [Header("Shared")]
    public bool loop = true;
    
    [Header("Tint Per Pattern")]
    public Color swirlStartColor = Color.white;
    public Color swirlEndColor = Color.white;

    public Color zigzagStartColor = Color.white;
    public Color zigzagEndColor = Color.white;

    public Color slideStartColor = Color.white;
    public Color slideEndColor = Color.white;

    public bool _isPlaying { get; private set; }
    private bool _stopRequested;
    private CancellationTokenSource _cts;

    
    
    readonly float[] _runtimeRingValues = new float[20];
    readonly float[] _runtimeSliceValues = new float[20];

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private Color EvaluateTint(Color a, Color b, float t)
    {
        return Color.LerpUnclamped(a, b, easeCurve.Evaluate(t));
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        ForceStop();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!_isPlaying)
            PushEditorValues();
        else
            PushRuntimeValues();
    }

    void OnValidate()
    {
        if (!_isPlaying && !Application.isPlaying)
            PushEditorValues();
    }

    void Update()
    {
        if (!_isPlaying)
            PushEditorValues();
    }

    void OnDisable() => ForceStop();

    // ─── Public API ────────────────────────────────────────────────────────
    public void PlaySwirl()     => StartPattern(RunSwirl);
    public void PlayZigzag()    => StartPattern(RunZigzag);
    public void PlaySlide()     => StartPattern(RunSlide);

    public void DoSwirl()       => StartPattern(RunDoSwirl);
    public void UndoSwirl()     => StartPattern(RunUndoSwirl);
    public void DoZigzag()      => StartPattern(RunDoZigzag);
    public void UndoZigzag()    => StartPattern(RunUndoZigzag);

    public void SlideIn()       => StartPattern(RunSlideIn);
    public void SlideOut()      => StartPattern(RunSlideOut);

    public void StopPattern() => _stopRequested = true;

    private void StartPattern(Func<CancellationToken, UniTaskVoid> routine)
    {
        ForceStop();
        _stopRequested = false;
        _cts = new CancellationTokenSource();
        _isPlaying = true;
        routine(_cts.Token).Forget();
    }

    private void ForceStop()
    {
        _stopRequested = false;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        _isPlaying = false;
    }

    private void Cleanup()
    {
        _isPlaying = false;
        _stopRequested = false;

        if (this == null || volume == null)
            return;

        ResetToDefault();
    }
    private void ResetToDefault()
    {
        for (int i = 0; i < 20; i++)
        {
            _runtimeRingValues[i] = 0f;
            _runtimeSliceValues[i] = 0f;
        }

        PushRuntimeValues();

#if UNITY_EDITOR
        ring0 = 0f; ring1 = 0f; ring2 = 0f; ring3 = 0f; ring4 = 0f;
        ring5 = 0f; ring6 = 0f; ring7 = 0f; ring8 = 0f; ring9 = 0f;
        ring10 = 0f; ring11 = 0f; ring12 = 0f; ring13 = 0f; ring14 = 0f;
        ring15 = 0f; ring16 = 0f; ring17 = 0f; ring18 = 0f; ring19 = 0f;
#endif

        // ✅ IMPORTANT: reset tint too
        ResetTint();
    }
    
    private void ResetTint()
    {
        if (volume == null) return;

        if (volume.profile.TryGet(out ConcentricCircleRotate fx))
        {
            fx.tintColor.overrideState = false;
            fx.tintColor.value = Color.white;
        }
    }
    
    // ─── Pattern Routines ──────────────────────────────────────────────────
    async UniTaskVoid RunSwirl(CancellationToken ct) { try { do { await RunRingPass(0f, targetAngle, ct); if (ct.IsCancellationRequested || _stopRequested) break; await RunRingPass(targetAngle, 0f, ct); if (ct.IsCancellationRequested || _stopRequested) break; } while (loop); } finally { Cleanup(); } }
    async UniTaskVoid RunZigzag(CancellationToken ct) { try { do { await RunZigzagPass(true, ct); if (ct.IsCancellationRequested || _stopRequested) break; await RunZigzagPass(false, ct); if (ct.IsCancellationRequested || _stopRequested) break; } while (loop); } finally { Cleanup(); } }
    async UniTaskVoid RunDoSwirl(CancellationToken ct) { try { do { await RunRingPass(0f, targetAngle, ct); if (ct.IsCancellationRequested || _stopRequested) break; } while (loop); } finally { Cleanup(); } }
    async UniTaskVoid RunUndoSwirl(CancellationToken ct) { try { do { await RunRingPass(targetAngle, 0f, ct); if (ct.IsCancellationRequested || _stopRequested) break; } while (loop); } finally { Cleanup(); } }
    async UniTaskVoid RunDoZigzag(CancellationToken ct) { try { do { await RunZigzagPass(true, ct); if (ct.IsCancellationRequested || _stopRequested) break; } while (loop); } finally { Cleanup(); } }
    async UniTaskVoid RunUndoZigzag(CancellationToken ct) { try { do { await RunZigzagPass(false, ct); if (ct.IsCancellationRequested || _stopRequested) break; } while (loop); } finally { Cleanup(); } }

    async UniTaskVoid RunSlide(CancellationToken ct) { try { do { await RunSlicePass(0f, slideDistance, ct); if (ct.IsCancellationRequested || _stopRequested) break; await RunSlicePass(slideDistance, 0f, ct); if (ct.IsCancellationRequested || _stopRequested) break; } while (loop); } finally { Cleanup(); } }
    async UniTaskVoid RunSlideIn(CancellationToken ct) { try { do { await RunSlicePass(0f, slideDistance, ct); if (ct.IsCancellationRequested || _stopRequested) break; } while (loop); } finally { Cleanup(); } }
    async UniTaskVoid RunSlideOut(CancellationToken ct) { try { do { await RunSlicePass(slideDistance, 0f, ct); if (ct.IsCancellationRequested || _stopRequested) break; } while (loop); } finally { Cleanup(); } }

    // ─── Core Pass Methods ─────────────────────────────────────────────────

    async UniTask RunRingPass(float from, float to, CancellationToken ct)
    {
        const int count = 20;
        float staggerSpread = swirlDuration * staggerFraction;
        float travelTime = swirlDuration - staggerSpread;
        float delayPerRing = staggerSpread / (count - 1);

        for (int r = 0; r < count; r++)
            _runtimeRingValues[r] = from;

        PushRuntimeValues();

        float passStart = Time.time;

        bool isUndo = to < from;

        while (true)
        {
            float now = Time.time - passStart;
            bool allDone = true;

            for (int r = 0; r < count; r++)
            {
                float t = Mathf.Clamp01(Mathf.InverseLerp(
                    r * delayPerRing,
                    r * delayPerRing + travelTime,
                    now));

                if (t < 1f) allDone = false;

                float eased = easeCurve.Evaluate(t);
                _runtimeRingValues[r] = Mathf.LerpUnclamped(from, to, eased);
            }

            float globalT = Mathf.Clamp01(now / swirlDuration);

            // 🔥 FIX: swap direction automatically for Undo
            if (!isUndo)
                tintColor = EvaluateTint(swirlStartColor, swirlEndColor, globalT);
            else
                tintColor = EvaluateTint(swirlEndColor, swirlStartColor, globalT);

            PushRuntimeValues();

            if (allDone) break;

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
            if (ct.IsCancellationRequested) return;
        }
    }

    async UniTask RunZigzagPass(bool forward, CancellationToken ct)
    {
        const int count = 20;
        float staggerSpread = swirlDuration * staggerFraction;
        float travelTime = swirlDuration - staggerSpread;
        float delayPerRing = staggerSpread / (count - 1);

        for (int r = 0; r < count; r++)
        {
            float target = (r % 2 == 0) ? targetAngle : -targetAngle;
            _runtimeRingValues[r] = forward ? 0f : target;
        }

        PushRuntimeValues();

        float passStart = Time.time;

        while (true)
        {
            float now = Time.time - passStart;
            bool allDone = true;

            for (int r = 0; r < count; r++)
            {
                float target = (r % 2 == 0) ? targetAngle : -targetAngle;
                float from = forward ? 0f : target;
                float to = forward ? target : 0f;

                float t = Mathf.Clamp01(Mathf.InverseLerp(
                    r * delayPerRing,
                    r * delayPerRing + travelTime,
                    now));

                if (t < 1f) allDone = false;

                float eased = easeCurve.Evaluate(t);
                _runtimeRingValues[r] = Mathf.LerpUnclamped(from, to, eased);
            }

            // 🔥 zigzag tint lerp
            float globalT = Mathf.Clamp01(now / swirlDuration);

            if (forward)
                tintColor = EvaluateTint(zigzagStartColor, zigzagEndColor, globalT);
            else
                tintColor = EvaluateTint(zigzagEndColor, zigzagStartColor, globalT);

            PushRuntimeValues();

            if (allDone) break;

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
            if (ct.IsCancellationRequested) return;
        }
    }

    async UniTask RunSlicePass(float from, float to, CancellationToken ct)
    {
        const int count = 20;
        float staggerSpread = slideDuration * slideStaggerFraction;
        float travelTime = slideDuration - staggerSpread;
        float delayPerSlice = staggerSpread / (count - 1);

        for (int s = 0; s < count; s++) _runtimeSliceValues[s] = from;

        PushRuntimeValues();

        float passStart = Time.time;

        while (true)
        {
            float now = Time.time - passStart;
            bool allDone = true;

            for (int s = 0; s < count; s++)
            {
                float t = Mathf.Clamp01(Mathf.InverseLerp(
                    s * delayPerSlice,
                    s * delayPerSlice + travelTime,
                    now));

                if (t < 1f) allDone = false;

                float eased = easeCurve.Evaluate(t);
                _runtimeSliceValues[s] = Mathf.LerpUnclamped(from, to, eased);
            }

            float globalT = Mathf.Clamp01(now / slideDuration);

            if (to > from)
                tintColor = EvaluateTint(slideStartColor, slideEndColor, globalT);
            else
                tintColor = EvaluateTint(slideEndColor, slideStartColor, globalT);

            PushRuntimeValues();

            if (allDone) break;

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
            if (ct.IsCancellationRequested) return;
        }
    }

    // ─── Push Methods (Fully Expanded) ─────────────────────────────────────

    private void PushEditorValues()
    {
        if (volume == null)
            return;

        if (volume.profile == null)
            return;

        if (!volume.profile.TryGet(out ConcentricCircleRotate fx))
            return;

        // Rings (20 fully expanded)
        fx.ring0.overrideState = true; fx.ring0.value = ring0;
        fx.ring1.overrideState = true; fx.ring1.value = ring1;
        fx.ring2.overrideState = true; fx.ring2.value = ring2;
        fx.ring3.overrideState = true; fx.ring3.value = ring3;
        fx.ring4.overrideState = true; fx.ring4.value = ring4;
        fx.ring5.overrideState = true; fx.ring5.value = ring5;
        fx.ring6.overrideState = true; fx.ring6.value = ring6;
        fx.ring7.overrideState = true; fx.ring7.value = ring7;
        fx.ring8.overrideState = true; fx.ring8.value = ring8;
        fx.ring9.overrideState = true; fx.ring9.value = ring9;
        fx.ring10.overrideState = true; fx.ring10.value = ring10;
        fx.ring11.overrideState = true; fx.ring11.value = ring11;
        fx.ring12.overrideState = true; fx.ring12.value = ring12;
        fx.ring13.overrideState = true; fx.ring13.value = ring13;
        fx.ring14.overrideState = true; fx.ring14.value = ring14;
        fx.ring15.overrideState = true; fx.ring15.value = ring15;
        fx.ring16.overrideState = true; fx.ring16.value = ring16;
        fx.ring17.overrideState = true; fx.ring17.value = ring17;
        fx.ring18.overrideState = true; fx.ring18.value = ring18;
        fx.ring19.overrideState = true; fx.ring19.value = ring19;

        // Slices (zeroed in editor)
        fx.slice0.overrideState = true;  fx.slice0.value = 0f;
        fx.slice1.overrideState = true;  fx.slice1.value = 0f;
        fx.slice2.overrideState = true;  fx.slice2.value = 0f;
        fx.slice3.overrideState = true;  fx.slice3.value = 0f;
        fx.slice4.overrideState = true;  fx.slice4.value = 0f;
        fx.slice5.overrideState = true;  fx.slice5.value = 0f;
        fx.slice6.overrideState = true;  fx.slice6.value = 0f;
        fx.slice7.overrideState = true;  fx.slice7.value = 0f;
        fx.slice8.overrideState = true;  fx.slice8.value = 0f;
        fx.slice9.overrideState = true;  fx.slice9.value = 0f;
        fx.slice10.overrideState = true; fx.slice10.value = 0f;
        fx.slice11.overrideState = true; fx.slice11.value = 0f;
        fx.slice12.overrideState = true; fx.slice12.value = 0f;
        fx.slice13.overrideState = true; fx.slice13.value = 0f;
        fx.slice14.overrideState = true; fx.slice14.value = 0f;
        fx.slice15.overrideState = true; fx.slice15.value = 0f;
        fx.slice16.overrideState = true; fx.slice16.value = 0f;
        fx.slice17.overrideState = true; fx.slice17.value = 0f;
        fx.slice18.overrideState = true; fx.slice18.value = 0f;
        fx.slice19.overrideState = true; fx.slice19.value = 0f;
        
        fx.tintColor.overrideState = true;
        fx.tintColor.value = Application.isPlaying ? tintColor : Color.white;
    }

 private void PushRuntimeValues()
{
    if (volume == null) return;

    if (volume.profile == null) return;

    if (!volume.profile.TryGet(out ConcentricCircleRotate fx)) return;

    // Rings (20 fully expanded)
    fx.ring0.overrideState = true; fx.ring0.value = _runtimeRingValues[0];
    fx.ring1.overrideState = true; fx.ring1.value = _runtimeRingValues[1];
    fx.ring2.overrideState = true; fx.ring2.value = _runtimeRingValues[2];
    fx.ring3.overrideState = true; fx.ring3.value = _runtimeRingValues[3];
    fx.ring4.overrideState = true; fx.ring4.value = _runtimeRingValues[4];
    fx.ring5.overrideState = true; fx.ring5.value = _runtimeRingValues[5];
    fx.ring6.overrideState = true; fx.ring6.value = _runtimeRingValues[6];
    fx.ring7.overrideState = true; fx.ring7.value = _runtimeRingValues[7];
    fx.ring8.overrideState = true; fx.ring8.value = _runtimeRingValues[8];
    fx.ring9.overrideState = true; fx.ring9.value = _runtimeRingValues[9];
    fx.ring10.overrideState = true; fx.ring10.value = _runtimeRingValues[10];
    fx.ring11.overrideState = true; fx.ring11.value = _runtimeRingValues[11];
    fx.ring12.overrideState = true; fx.ring12.value = _runtimeRingValues[12];
    fx.ring13.overrideState = true; fx.ring13.value = _runtimeRingValues[13];
    fx.ring14.overrideState = true; fx.ring14.value = _runtimeRingValues[14];
    fx.ring15.overrideState = true; fx.ring15.value = _runtimeRingValues[15];
    fx.ring16.overrideState = true; fx.ring16.value = _runtimeRingValues[16];
    fx.ring17.overrideState = true; fx.ring17.value = _runtimeRingValues[17];
    fx.ring18.overrideState = true; fx.ring18.value = _runtimeRingValues[18];
    fx.ring19.overrideState = true; fx.ring19.value = _runtimeRingValues[19];

    fx.slice0.overrideState = true; fx.slice0.value = _runtimeSliceValues[0];
    fx.slice1.overrideState = true; fx.slice1.value = _runtimeSliceValues[1];
    fx.slice2.overrideState = true; fx.slice2.value = _runtimeSliceValues[2];
    fx.slice3.overrideState = true; fx.slice3.value = _runtimeSliceValues[3];
    fx.slice4.overrideState = true; fx.slice4.value = _runtimeSliceValues[4];
    fx.slice5.overrideState = true; fx.slice5.value = _runtimeSliceValues[5];
    fx.slice6.overrideState = true; fx.slice6.value = _runtimeSliceValues[6];
    fx.slice7.overrideState = true; fx.slice7.value = _runtimeSliceValues[7];
    fx.slice8.overrideState = true; fx.slice8.value = _runtimeSliceValues[8];
    fx.slice9.overrideState = true; fx.slice9.value = _runtimeSliceValues[9];
    fx.slice10.overrideState = true; fx.slice10.value = _runtimeSliceValues[10];
    fx.slice11.overrideState = true; fx.slice11.value = _runtimeSliceValues[11];
    fx.slice12.overrideState = true; fx.slice12.value = _runtimeSliceValues[12];
    fx.slice13.overrideState = true; fx.slice13.value = _runtimeSliceValues[13];
    fx.slice14.overrideState = true; fx.slice14.value = _runtimeSliceValues[14];
    fx.slice15.overrideState = true; fx.slice15.value = _runtimeSliceValues[15];
    fx.slice16.overrideState = true; fx.slice16.value = _runtimeSliceValues[16];
    fx.slice17.overrideState = true; fx.slice17.value = _runtimeSliceValues[17];
    fx.slice18.overrideState = true; fx.slice18.value = _runtimeSliceValues[18];
    fx.slice19.overrideState = true; fx.slice19.value = _runtimeSliceValues[19];

    fx.tintColor.overrideState = true;
    fx.tintColor.value = Application.isPlaying ? tintColor : Color.white;
}
    
    private float frac(float v) => v - Mathf.Floor(v);

#if UNITY_EDITOR
    [CustomEditor(typeof(ConcentricCircleDriver))]
    public class ConcentricCircleDriverEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Pattern Controls", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("▶ Swirl", GUILayout.Height(30))) ((ConcentricCircleDriver)target).PlaySwirl();
            if (GUILayout.Button("▶ Zigzag", GUILayout.Height(30))) ((ConcentricCircleDriver)target).PlayZigzag();
            if (GUILayout.Button("▶ Slide", GUILayout.Height(30))) ((ConcentricCircleDriver)target).PlaySlide();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Do Swirl", GUILayout.Height(25))) ((ConcentricCircleDriver)target).DoSwirl();
            if (GUILayout.Button("Undo Swirl", GUILayout.Height(25))) ((ConcentricCircleDriver)target).UndoSwirl();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Do Zigzag", GUILayout.Height(25))) ((ConcentricCircleDriver)target).DoZigzag();
            if (GUILayout.Button("Undo Zigzag", GUILayout.Height(25))) ((ConcentricCircleDriver)target).UndoZigzag();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Slide In", GUILayout.Height(25))) ((ConcentricCircleDriver)target).SlideIn();
            if (GUILayout.Button("Slide Out", GUILayout.Height(25))) ((ConcentricCircleDriver)target).SlideOut();
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Stop After Cycle", GUILayout.Height(35))) ((ConcentricCircleDriver)target).StopPattern();
        }
    }
#endif
}