using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

public class LightningCompanionLight : MonoBehaviour
{
    public Light thelight;

    [Header("Intensity")]
    public float minIntensity = 0.5f;
    public float maxIntensity = 2.5f;

    [Header("Timing Between Flashes")]
    public float minTimeBetweenFlashes = 2f;
    public float maxTimeBetweenFlashes = 6f;

    [Header("Flash Duration")]
    public float minFlashDuration = 0.05f;
    public float maxFlashDuration = 0.2f;

    private CancellationTokenSource _cts;

    private float baseIntensity;

    private void OnEnable()
    {
        if (thelight == null) return;

        baseIntensity = thelight.intensity;

        _cts = new CancellationTokenSource();
        LightningLoop(_cts.Token).Forget();
    }

    private void OnDisable()
    {
        _cts?.Cancel();
    }

    private async UniTask LightningLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            // ⏱ wait random time between flashes
            float waitTime = Random.Range(minTimeBetweenFlashes, maxTimeBetweenFlashes);
            await UniTask.Delay((int)(waitTime * 1000), cancellationToken: token);

            // ⚡ random flash parameters
            float peakIntensity = Random.Range(minIntensity, maxIntensity);
            float duration = Random.Range(minFlashDuration, maxFlashDuration);

            await Flash(peakIntensity, duration, token);
        }
    }

    private async UniTask Flash(float peak, float duration, CancellationToken token)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // smooth falloff (matches shader "hard start, quick decay feel")
            float curve = 1f - t * t;

            thelight.intensity = Mathf.Lerp(baseIntensity, peak, curve);

            elapsed += Time.deltaTime;
            await UniTask.Yield(token);
        }

        thelight.intensity = baseIntensity;
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}