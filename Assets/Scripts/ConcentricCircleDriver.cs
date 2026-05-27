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

    [Header("Ring Rotations (degrees) — inactive while a pattern plays")]
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

    [Header("Swirl / Zigzag Settings")]
    public float swirlDuration = 2.5f;
    public float targetAngle   = 180f;
    [Range(0.05f, 0.7f)] public float staggerFraction = 0.4f;
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Slide Settings")]
    [Tooltip("Duration of each slide pass (seconds)")]
    public float slideDuration = 2.5f;
    [Tooltip("Maximum horizontal offset in UV space. 0.5 = half screen width.")]
    [Range(0.01f, 1f)] public float slideDistance = 0.3f;
    [Tooltip("Stagger fraction for slide, same meaning as swirl stagger.")]
    [Range(0.05f, 0.7f)] public float slideStaggerFraction = 0.4f;

    [Header("Shared")]
    public bool loop = true;

    // ─── State ─────────────────────────────────────────────────────────────

    public bool _isPlaying { get; private set; }
    public bool _stopRequested;
    CancellationTokenSource _cts;

    readonly float[] _runtimeRingValues  = new float[10];
    readonly float[] _runtimeSliceValues = new float[20];

    // ─── Unity lifecycle ───────────────────────────────────────────────────

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        ForceStop();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!_isPlaying) PushEditorValues();
        else             PushRuntimeValues();
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

    [ContextMenu("Play Swirl")]    public void PlaySwirl()   => StartPattern(RunSwirl);
    [ContextMenu("Play Zigzag")]   public void PlayZigzag()  => StartPattern(RunZigzag);
    [ContextMenu("Do Swirl")]      public void DoSwirl()     => StartPattern(RunDoSwirl);
    [ContextMenu("Undo Swirl")]    public void UndoSwirl()   => StartPattern(RunUndoSwirl);
    [ContextMenu("Do Zigzag")]     public void DoZigzag()    => StartPattern(RunDoZigzag);
    [ContextMenu("Undo Zigzag")]   public void UndoZigzag()  => StartPattern(RunUndoZigzag);
    [ContextMenu("Play Slide")]    public void PlaySlide()   => StartPattern(RunSlide);
    [ContextMenu("Slide In")]      public void SlideIn()     => StartPattern(RunSlideIn);
    [ContextMenu("Slide Out")]     public void SlideOut()    => StartPattern(RunSlideOut);

    [ContextMenu("Stop After Cycle")] public void StopPattern() => _stopRequested = true;

    // ─── Shared startup ────────────────────────────────────────────────────

    void StartPattern(Func<CancellationToken, UniTaskVoid> routine)
    {
        ForceStop();
        _stopRequested = false;
        _cts           = new CancellationTokenSource();
        _isPlaying     = true;
        routine(_cts.Token).Forget();
    }

    void ForceStop()
    {
        _stopRequested = false;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts       = null;
        _isPlaying = false;
    }

    void Cleanup()
    {
        _isPlaying     = false;
        _stopRequested = false;
    }

    // ─── Ring pattern routines ─────────────────────────────────────────────

    async UniTaskVoid RunSwirl(CancellationToken ct)
    {
        try
        {
            do
            {
                await RunRingPass(0f, targetAngle, ct);
                if (ct.IsCancellationRequested || _stopRequested) break;
                await RunRingPass(targetAngle, 0f, ct);
                if (ct.IsCancellationRequested || _stopRequested) break;
            }
            while (loop);
        }
        finally { Cleanup(); }
    }

    async UniTaskVoid RunZigzag(CancellationToken ct)
    {
        try
        {
            do
            {
                await RunZigzagPass(true, ct);
                if (ct.IsCancellationRequested || _stopRequested) break;
                await RunZigzagPass(false, ct);
                if (ct.IsCancellationRequested || _stopRequested) break;
            }
            while (loop);
        }
        finally { Cleanup(); }
    }

    async UniTaskVoid RunDoSwirl(CancellationToken ct)
    {
        try { do { await RunRingPass(0f, targetAngle, ct); if (ct.IsCancellationRequested || _stopRequested) break; } while (loop); }
        finally { Cleanup(); }
    }

    async UniTaskVoid RunUndoSwirl(CancellationToken ct)
    {
        try { do { await RunRingPass(targetAngle, 0f, ct); if (ct.IsCancellationRequested || _stopRequested) break; } while (loop); }
        finally { Cleanup(); }
    }

    async UniTaskVoid RunDoZigzag(CancellationToken ct)
    {
        try { do { await RunZigzagPass(true, ct); if (ct.IsCancellationRequested || _stopRequested) break; } while (loop); }
        finally { Cleanup(); }
    }

    async UniTaskVoid RunUndoZigzag(CancellationToken ct)
    {
        try { do { await RunZigzagPass(false, ct); if (ct.IsCancellationRequested || _stopRequested) break; } while (loop); }
        finally { Cleanup(); }
    }

    // ─── Slide pattern routines ────────────────────────────────────────────

    async UniTaskVoid RunSlide(CancellationToken ct)
    {
        try
        {
            do
            {
                await RunSlicePass(0f, slideDistance, ct);
                if (ct.IsCancellationRequested || _stopRequested) break;
                await RunSlicePass(slideDistance, 0f, ct);
                if (ct.IsCancellationRequested || _stopRequested) break;
            }
            while (loop);
        }
        finally { Cleanup(); }
    }

    async UniTaskVoid RunSlideIn(CancellationToken ct)
    {
        try { do { await RunSlicePass(0f, slideDistance, ct); if (ct.IsCancellationRequested || _stopRequested) break; } while (loop); }
        finally { Cleanup(); }
    }

    async UniTaskVoid RunSlideOut(CancellationToken ct)
    {
        try { do { await RunSlicePass(slideDistance, 0f, ct); if (ct.IsCancellationRequested || _stopRequested) break; } while (loop); }
        finally { Cleanup(); }
    }

    // ─── Core pass workers ─────────────────────────────────────────────────

    async UniTask RunRingPass(float from, float to, CancellationToken ct)
    {
        const int count     = 10;
        float staggerSpread = swirlDuration * staggerFraction;
        float travelTime    = swirlDuration - staggerSpread;
        float delayPerRing  = staggerSpread / (count - 1);

        for (int r = 0; r < count; r++) _runtimeRingValues[r] = from;
        PushRuntimeValues();

        float passStart = Time.time;
        while (true)
        {
            float now = Time.time - passStart;
            bool allDone = true;
            for (int r = 0; r < count; r++)
            {
                float t = Mathf.Clamp01(Mathf.InverseLerp(r * delayPerRing, r * delayPerRing + travelTime, now));
                if (t < 1f) allDone = false;
                _runtimeRingValues[r] = Mathf.LerpUnclamped(from, to, easeCurve.Evaluate(t));
            }
            PushRuntimeValues();
            if (allDone) break;
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
            if (ct.IsCancellationRequested) return;
        }
    }

    async UniTask RunZigzagPass(bool forward, CancellationToken ct)
    {
        const int count     = 10;
        float staggerSpread = swirlDuration * staggerFraction;
        float travelTime    = swirlDuration - staggerSpread;
        float delayPerRing  = staggerSpread / (count - 1);

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
                float from   = forward ? 0f     : target;
                float to     = forward ? target : 0f;
                float t      = Mathf.Clamp01(Mathf.InverseLerp(r * delayPerRing, r * delayPerRing + travelTime, now));
                if (t < 1f) allDone = false;
                _runtimeRingValues[r] = Mathf.LerpUnclamped(from, to, easeCurve.Evaluate(t));
            }
            PushRuntimeValues();
            if (allDone) break;
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
            if (ct.IsCancellationRequested) return;
        }
    }

    async UniTask RunSlicePass(float from, float to, CancellationToken ct)
    {
        const int count     = 20;
        float staggerSpread = slideDuration * slideStaggerFraction;
        float travelTime    = slideDuration - staggerSpread;
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
                float t = Mathf.Clamp01(Mathf.InverseLerp(s * delayPerSlice, s * delayPerSlice + travelTime, now));
                if (t < 1f) allDone = false;
                _runtimeSliceValues[s] = Mathf.LerpUnclamped(from, to, easeCurve.Evaluate(t));
            }
            PushRuntimeValues();
            if (allDone) break;
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
            if (ct.IsCancellationRequested) return;
        }
    }

    // ─── Push helpers ──────────────────────────────────────────────────────

    void PushEditorValues()
    {
        if (volume == null || !volume.profile.TryGet(out ConcentricCircleRotate fx)) return;

        fx.ring0.overrideState = true;  fx.ring0.value = ring0;
        fx.ring1.overrideState = true;  fx.ring1.value = ring1;
        fx.ring2.overrideState = true;  fx.ring2.value = ring2;
        fx.ring3.overrideState = true;  fx.ring3.value = ring3;
        fx.ring4.overrideState = true;  fx.ring4.value = ring4;
        fx.ring5.overrideState = true;  fx.ring5.value = ring5;
        fx.ring6.overrideState = true;  fx.ring6.value = ring6;
        fx.ring7.overrideState = true;  fx.ring7.value = ring7;
        fx.ring8.overrideState = true;  fx.ring8.value = ring8;
        fx.ring9.overrideState = true;  fx.ring9.value = ring9;

        // Keep slices zeroed when editor controls are active
        fx.slice0.overrideState  = true;  fx.slice0.value  = 0f;
        fx.slice1.overrideState  = true;  fx.slice1.value  = 0f;
        fx.slice2.overrideState  = true;  fx.slice2.value  = 0f;
        fx.slice3.overrideState  = true;  fx.slice3.value  = 0f;
        fx.slice4.overrideState  = true;  fx.slice4.value  = 0f;
        fx.slice5.overrideState  = true;  fx.slice5.value  = 0f;
        fx.slice6.overrideState  = true;  fx.slice6.value  = 0f;
        fx.slice7.overrideState  = true;  fx.slice7.value  = 0f;
        fx.slice8.overrideState  = true;  fx.slice8.value  = 0f;
        fx.slice9.overrideState  = true;  fx.slice9.value  = 0f;
        fx.slice10.overrideState = true;  fx.slice10.value = 0f;
        fx.slice11.overrideState = true;  fx.slice11.value = 0f;
        fx.slice12.overrideState = true;  fx.slice12.value = 0f;
        fx.slice13.overrideState = true;  fx.slice13.value = 0f;
        fx.slice14.overrideState = true;  fx.slice14.value = 0f;
        fx.slice15.overrideState = true;  fx.slice15.value = 0f;
        fx.slice16.overrideState = true;  fx.slice16.value = 0f;
        fx.slice17.overrideState = true;  fx.slice17.value = 0f;
        fx.slice18.overrideState = true;  fx.slice18.value = 0f;
        fx.slice19.overrideState = true;  fx.slice19.value = 0f;
    }

    void PushRuntimeValues()
    {
        if (volume == null || !volume.profile.TryGet(out ConcentricCircleRotate fx)) return;

        fx.ring0.overrideState = true;  fx.ring0.value = _runtimeRingValues[0];
        fx.ring1.overrideState = true;  fx.ring1.value = _runtimeRingValues[1];
        fx.ring2.overrideState = true;  fx.ring2.value = _runtimeRingValues[2];
        fx.ring3.overrideState = true;  fx.ring3.value = _runtimeRingValues[3];
        fx.ring4.overrideState = true;  fx.ring4.value = _runtimeRingValues[4];
        fx.ring5.overrideState = true;  fx.ring5.value = _runtimeRingValues[5];
        fx.ring6.overrideState = true;  fx.ring6.value = _runtimeRingValues[6];
        fx.ring7.overrideState = true;  fx.ring7.value = _runtimeRingValues[7];
        fx.ring8.overrideState = true;  fx.ring8.value = _runtimeRingValues[8];
        fx.ring9.overrideState = true;  fx.ring9.value = _runtimeRingValues[9];

        fx.slice0.overrideState  = true;  fx.slice0.value  = _runtimeSliceValues[0];
        fx.slice1.overrideState  = true;  fx.slice1.value  = _runtimeSliceValues[1];
        fx.slice2.overrideState  = true;  fx.slice2.value  = _runtimeSliceValues[2];
        fx.slice3.overrideState  = true;  fx.slice3.value  = _runtimeSliceValues[3];
        fx.slice4.overrideState  = true;  fx.slice4.value  = _runtimeSliceValues[4];
        fx.slice5.overrideState  = true;  fx.slice5.value  = _runtimeSliceValues[5];
        fx.slice6.overrideState  = true;  fx.slice6.value  = _runtimeSliceValues[6];
        fx.slice7.overrideState  = true;  fx.slice7.value  = _runtimeSliceValues[7];
        fx.slice8.overrideState  = true;  fx.slice8.value  = _runtimeSliceValues[8];
        fx.slice9.overrideState  = true;  fx.slice9.value  = _runtimeSliceValues[9];
        fx.slice10.overrideState = true;  fx.slice10.value = _runtimeSliceValues[10];
        fx.slice11.overrideState = true;  fx.slice11.value = _runtimeSliceValues[11];
        fx.slice12.overrideState = true;  fx.slice12.value = _runtimeSliceValues[12];
        fx.slice13.overrideState = true;  fx.slice13.value = _runtimeSliceValues[13];
        fx.slice14.overrideState = true;  fx.slice14.value = _runtimeSliceValues[14];
        fx.slice15.overrideState = true;  fx.slice15.value = _runtimeSliceValues[15];
        fx.slice16.overrideState = true;  fx.slice16.value = _runtimeSliceValues[16];
        fx.slice17.overrideState = true;  fx.slice17.value = _runtimeSliceValues[17];
        fx.slice18.overrideState = true;  fx.slice18.value = _runtimeSliceValues[18];
        fx.slice19.overrideState = true;  fx.slice19.value = _runtimeSliceValues[19];
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(ConcentricCircleDriver))]
public class ConcentricCircleDriverEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var driver = (ConcentricCircleDriver)target;

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Pattern Playback", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            EditorGUILayout.LabelField("Full Cycles", EditorStyles.miniLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("▶  Swirl",  GUILayout.Height(30))) driver.PlaySwirl();
            if (GUILayout.Button("▶  Zigzag", GUILayout.Height(30))) driver.PlayZigzag();
            if (GUILayout.Button("▶  Slide",  GUILayout.Height(30))) driver.PlaySlide();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Halves", EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("▶  Do Swirl",   GUILayout.Height(30))) driver.DoSwirl();
            if (GUILayout.Button("◀  Undo Swirl", GUILayout.Height(30))) driver.UndoSwirl();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("▶  Do Zigzag",   GUILayout.Height(30))) driver.DoZigzag();
            if (GUILayout.Button("◀  Undo Zigzag", GUILayout.Height(30))) driver.UndoZigzag();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("▶  Slide In",  GUILayout.Height(30))) driver.SlideIn();
            if (GUILayout.Button("◀  Slide Out", GUILayout.Height(30))) driver.SlideOut();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            using (new EditorGUI.DisabledScope(!driver._isPlaying))
            {
                if (GUILayout.Button("◼  Stop After Cycle", GUILayout.Height(32)))
                    driver.StopPattern();
            }
        }

        if (!Application.isPlaying)
            EditorGUILayout.HelpBox("Enter Play mode to preview patterns.", MessageType.Info);
        else if (driver._isPlaying && driver._stopRequested)
            EditorGUILayout.HelpBox("Finishing current cycle…", MessageType.None);
    }
}

#endif