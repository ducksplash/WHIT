using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

[RequireComponent(typeof(Image))]
public class UIPulsatingGlowUniTask : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color colorA = Color.black;
    [SerializeField] private Color colorB = Color.white;

    [Header("Pulse Settings")]
    [SerializeField] private float pulseSpeed = 1f;

    [Header("Glow")]
    [SerializeField] private float glowIntensity = 2f;

    private Image uiImage;

    private CancellationTokenSource cts;

    private void OnEnable()
    {
        uiImage = GetComponent<Image>();

        cts = new CancellationTokenSource();

        PulseRoutine(cts.Token).Forget();
    }


    private void OnDisable()
    {
        Dispose();
    }



    private async UniTaskVoid PulseRoutine(
        CancellationToken token
    )
    {
        while (!token.IsCancellationRequested)
        {
            float t =
                Mathf.PingPong(
                    Time.unscaledTime * pulseSpeed,
                    1f
                );

            Color finalColor =
                Color.Lerp(colorA, colorB, t);

            // Fake emission/glow
            finalColor *= glowIntensity;

            uiImage.color = finalColor;

            await UniTask.Yield(
                PlayerLoopTiming.Update,
                token
            );
        }
    }

    private void OnDestroy()
    {
        Dispose();
    }

    private void Dispose()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
}