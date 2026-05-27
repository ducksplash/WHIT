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

    [Header("Pattern Settings")]
    public float swirlDuration = 2.5f;
    public float targetAngle = 180f;
    [Range(0.05f, 0.7f)] public float staggerFraction = 0.4f;
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public bool loop = true;

    // State
    public bool _isPlaying { get; private set; }
    private bool _stopRequested;
    private CancellationTokenSource _cts;

    readonly float[] _runtimeValues = new float[10];

    private void Awake()
    {
        // Listen for scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        ForceStop();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[ConcentricCircleDriver] Scene loaded: {scene.name}");
        
        // Re-apply current values after scene load
        if (!_isPlaying)
            PushEditorValues();
        else
            PushRuntimeValues();
    }

    void OnValidate()
    {
        if (!_isPlaying && Application.isPlaying == false)
            PushEditorValues();
    }

    void Update()
    {
        if (!_isPlaying)
            PushEditorValues();
    }

    void OnDisable() => ForceStop();

    // ─── Public API ────────────────────────────────────────────────────────

    [ContextMenu("Play Swirl")]      public void PlaySwirl()     => StartPattern(RunSwirl);
    [ContextMenu("Play Zigzag")]     public void PlayZigzag()    => StartPattern(RunZigzag);
    [ContextMenu("Do Swirl")]        public void DoSwirl()       => StartPattern(RunDoSwirl);
    [ContextMenu("Undo Swirl")]      public void UndoSwirl()     => StartPattern(RunUndoSwirl);
    [ContextMenu("Do Zigzag")]       public void DoZigzag()      => StartPattern(RunDoZigzag);
    [ContextMenu("Undo Zigzag")]     public void UndoZigzag()    => StartPattern(RunUndoZigzag);

    [ContextMenu("Stop After Cycle")] public void StopPattern() => _stopRequested = true;

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
    }

    // ─── Pattern Routines (unchanged logic, better cancellation) ───────────

    async UniTaskVoid RunSwirl(CancellationToken ct)
    {
        try
        {
            do
            {
                await RunPass(0f, targetAngle, ct);
                if (ct.IsCancellationRequested || _stopRequested) break;

                await RunPass(targetAngle, 0f, ct);
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
                await RunPassZigzag(true, ct);
                if (ct.IsCancellationRequested || _stopRequested) break;

                await RunPassZigzag(false, ct);
                if (ct.IsCancellationRequested || _stopRequested) break;
            } 
            while (loop);
        }
        finally { Cleanup(); }
    }

    async UniTaskVoid RunDoSwirl(CancellationToken ct)   { try { do { await RunPass(0f, targetAngle, ct); if (ct.IsCancellationRequested || _stopRequested) break; } while (loop); } finally { Cleanup(); } }
    async UniTaskVoid RunUndoSwirl(CancellationToken ct) { try { do { await RunPass(targetAngle, 0f, ct); if (ct.IsCancellationRequested || _stopRequested) break; } while (loop); } finally { Cleanup(); } }
    async UniTaskVoid RunDoZigzag(CancellationToken ct)  { try { do { await RunPassZigzag(true, ct); if (ct.IsCancellationRequested || _stopRequested) break; } while (loop); } finally { Cleanup(); } }
    async UniTaskVoid RunUndoZigzag(CancellationToken ct){ try { do { await RunPassZigzag(false, ct); if (ct.IsCancellationRequested || _stopRequested) break; } while (loop); } finally { Cleanup(); } }

    // ─── Core Pass Logic (unchanged) ───────────────────────────────────────

    async UniTask RunPass(float from, float to, CancellationToken ct)
    {
        const int ringCount = 10;
        float staggerSpread = swirlDuration * staggerFraction;
        float travelTime = swirlDuration - staggerSpread;
        float delayPerRing = staggerSpread / (ringCount - 1);

        for (int r = 0; r < ringCount; r++)
            _runtimeValues[r] = from;
        PushRuntimeValues();

        float passStart = Time.time;

        while (true)
        {
            float now = Time.time - passStart;
            bool allDone = true;

            for (int r = 0; r < ringCount; r++)
            {
                float ringStart = r * delayPerRing;
                float t = Mathf.InverseLerp(ringStart, ringStart + travelTime, now);
                if (t < 1f) allDone = false;
                t = Mathf.Clamp01(t);

                _runtimeValues[r] = Mathf.LerpUnclamped(from, to, easeCurve.Evaluate(t));
            }

            PushRuntimeValues();
            if (allDone) break;

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
            if (ct.IsCancellationRequested) return;
        }
    }

    async UniTask RunPassZigzag(bool forward, CancellationToken ct)
    {
        const int ringCount = 10;
        float staggerSpread = swirlDuration * staggerFraction;
        float travelTime = swirlDuration - staggerSpread;
        float delayPerRing = staggerSpread / (ringCount - 1);

        for (int r = 0; r < ringCount; r++)
        {
            float ringTarget = (r % 2 == 0) ? targetAngle : -targetAngle;
            _runtimeValues[r] = forward ? 0f : ringTarget;
        }
        PushRuntimeValues();

        float passStart = Time.time;

        while (true)
        {
            float now = Time.time - passStart;
            bool allDone = true;

            for (int r = 0; r < ringCount; r++)
            {
                float ringTarget = (r % 2 == 0) ? targetAngle : -targetAngle;
                float from = forward ? 0f : ringTarget;
                float to = forward ? ringTarget : 0f;

                float ringStart = r * delayPerRing;
                float t = Mathf.InverseLerp(ringStart, ringStart + travelTime, now);
                if (t < 1f) allDone = false;
                t = Mathf.Clamp01(t);

                _runtimeValues[r] = Mathf.LerpUnclamped(from, to, easeCurve.Evaluate(t));
            }

            PushRuntimeValues();
            if (allDone) break;

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
            if (ct.IsCancellationRequested) return;
        }
    }

    private void PushEditorValues()
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
    }

    private void PushRuntimeValues()
    {
        if (volume == null || !volume.profile.TryGet(out ConcentricCircleRotate fx)) return;

        fx.ring0.overrideState = true;  fx.ring0.value = _runtimeValues[0];
        fx.ring1.overrideState = true;  fx.ring1.value = _runtimeValues[1];
        fx.ring2.overrideState = true;  fx.ring2.value = _runtimeValues[2];
        fx.ring3.overrideState = true;  fx.ring3.value = _runtimeValues[3];
        fx.ring4.overrideState = true;  fx.ring4.value = _runtimeValues[4];
        fx.ring5.overrideState = true;  fx.ring5.value = _runtimeValues[5];
        fx.ring6.overrideState = true;  fx.ring6.value = _runtimeValues[6];
        fx.ring7.overrideState = true;  fx.ring7.value = _runtimeValues[7];
        fx.ring8.overrideState = true;  fx.ring8.value = _runtimeValues[8];
        fx.ring9.overrideState = true;  fx.ring9.value = _runtimeValues[9];
    }
}