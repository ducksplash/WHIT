using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FarsetCentralDirector : MonoBehaviour
{
    public Transform[] NoraWaypoints = new Transform[] { };
    public Camera AutonomyCamera;
    public float CameraXOffset = 5f;
    public Transform Train;

    [Tooltip("How long the player must stay still before we switch to the Train")]
    public float stopThresholdTime = 0.4f;

    [Tooltip("Speed below this is considered 'stopped'")]
    public float stopSpeedThreshold = 0.15f;

    private CancellationTokenSource _cts;
    private bool _hasStarted;

    public float exitXValue = 55f;

    public GAMELEVEL nextLevel = GAMELEVEL.EnteringTawley;
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (!_hasStarted)
        {
            _hasStarted = true;
            StartDirectorFlow().Forget();
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene != gameObject.scene) return;
        StartDirectorFlow().Forget();
    }

    private async UniTaskVoid StartDirectorFlow()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        await UniTask.WaitUntil(() => Player.Instance != null, cancellationToken: token);
        await UniTask.Yield(PlayerLoopTiming.Update, token);

        if (token.IsCancellationRequested) return;

        Debug.Log("[FarsetCentralDirector] Player ready – starting autonomous mode");

        if (AutonomyCamera != null)
        {
            Player.Instance.StartAutonomousMode(NoraWaypoints, AutonomyCamera);
            FollowXAsync(AutonomyCamera.transform, Player.Instance.transform, Train, token).Forget();
        }
        else
        {
            Player.Instance.StartAutonomousMode(NoraWaypoints);
        }
    }

    public async UniTask FollowXAsync(
        Transform autoCam,
        Transform playerTarget,
        Transform trainTarget,
        CancellationToken cancellationToken = default,
        float smoothTime = 0.15f,
        float maxSpeed = Mathf.Infinity)
    {
        if (autoCam == null || playerTarget == null) return;

        float velocityX = 0f;
        float stillTimer = 0f;
        bool followingTrain = false;
        bool hasExited = false;

        Vector3 lastPlayerPos = playerTarget.position;

        while (!cancellationToken.IsCancellationRequested && !hasExited)
        {
            // --- Detect if the player has stopped ---
            if (!followingTrain)
            {
                float playerSpeed = (playerTarget.position - lastPlayerPos).magnitude / Time.deltaTime;
                lastPlayerPos = playerTarget.position;

                if (playerSpeed < stopSpeedThreshold)
                {
                    stillTimer += Time.deltaTime;
                    if (stillTimer >= stopThresholdTime)
                    {
                        followingTrain = true;
                        Debug.Log("[FarsetCentralDirector] Player stopped – now following Train");
                    }
                }
                else
                {
                    stillTimer = 0f;
                }
            }

            // --- Choose which X to follow ---
            float targetX;
            if (followingTrain && trainTarget != null)
            {
                targetX = trainTarget.position.x + CameraXOffset;
            }
            else
            {
                targetX = playerTarget.position.x + CameraXOffset;
            }

            // --- Smooth follow ---
            float currentX = autoCam.position.x;
            float newX = Mathf.SmoothDamp(currentX, targetX, ref velocityX, smoothTime, maxSpeed, Time.deltaTime);

            Vector3 pos = autoCam.position;
            pos.x = newX;
            autoCam.position = pos;

            // --- Exit check (only while following the train) ---
            if (followingTrain && newX >= exitXValue)
            {
                hasExited = true;
                Debug.Log($"[FarsetCentralDirector] Camera X ({newX}) exceeded exitXValue ({exitXValue}) – loading next level");
                MoveToNextLevel();
                break;
            }

            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }
    }



    private void MoveToNextLevel()
    {
        GameMaster.Instance.LoadingManager.LoadLevel(nextLevel);
    }
    
    
    
    
    
    
    
}