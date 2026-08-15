using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MovingTrain : MonoBehaviour
{
    [Header("Train")]
    public Transform Train;                 // The only inspector reference you need

    [Header("Movement Settings")]
    public float startSpeed = 2f;
    public float endSpeed = 10f;
    public float accelerationDuration = 3f;
    public float delayBeforeStart = 1f;

    private bool _hasStarted;
    private CancellationTokenSource _cts;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("trigger The Train");
        
        // Only react once, and only to the Player
        if (_hasStarted) return;
        if (!other.CompareTag("Player")) return;
        if (other.GetComponent<CharacterController>() == null) return;

        _hasStarted = true;
        StartTrainSequence().Forget();
    }

    private async UniTaskVoid StartTrainSequence()
    {
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        try
        {
            // Wait 1 full second after the collision
            await UniTask.Delay(System.TimeSpan.FromSeconds(delayBeforeStart), cancellationToken: token);

            await MoveTrainAsync(token);
        }
        catch (System.OperationCanceledException)
        {
            // Sequence was cancelled
        }
    }

    private async UniTask MoveTrainAsync(CancellationToken token)
    {
        if (Train == null) return;

        float elapsed = 0f;
        float currentSpeed = startSpeed;

        while (!token.IsCancellationRequested)
        {
            // Accelerate from startSpeed → endSpeed over accelerationDuration
            if (elapsed < accelerationDuration)
            {
                float t = elapsed / accelerationDuration;
                currentSpeed = Mathf.Lerp(startSpeed, endSpeed, t);
            }
            else
            {
                currentSpeed = endSpeed;
            }

            // Move purely on the X axis
            Train.position += Vector3.right * currentSpeed * Time.deltaTime;

            elapsed += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    private void OnDisable()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}