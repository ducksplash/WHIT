using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class WarningLight : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float lightspeed = 10f;
    public Vector3 rotationAxis = new Vector3(0, 0, 1);   // Z-axis by default
    public float degreesPerSecond = 30f;                   // Base rotation speed

    private CancellationTokenSource _cts;

    private void Start()
    {
        _cts = new CancellationTokenSource();
        RotateLightAsync(_cts.Token).Forget();
    }

    private async UniTask RotateLightAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            float rotationThisFrame = degreesPerSecond * Time.deltaTime * lightspeed;
            
            transform.Rotate(rotationAxis * rotationThisFrame);

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    // Optional: Public method to change speed at runtime
    public void SetSpeed(float newSpeed)
    {
        lightspeed = newSpeed;
    }
}