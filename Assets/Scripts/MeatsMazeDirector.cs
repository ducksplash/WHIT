using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MeatsMazeDirector : MonoBehaviour
{
    public List<GameObject> BloodPipesInScene = new List<GameObject>();
    public Transform BloodLayer;
    public Vector3 StartPosition = new Vector3();
    public float BloodRiseAmount = 4f;
    public float BloodBathDuration = 100f;
    public ThoughtName SelectedThoughtBubble = ThoughtName.NoraLookingAtBloodPipes;

    private CancellationTokenSource bloodBathCTS;

    private void Start()
    {
        EventManager.OnBloodPipeLoaded += CollectBloodPipe;
        EventManager.OnMazeDoorUsed += StartBloodBathSoon;
        EventManager.OnDeath += StopAllAndReset;

        StartPosition = BloodLayer.position;
    }

    private void OnDestroy()
    {
        EventManager.OnBloodPipeLoaded -= CollectBloodPipe;
        EventManager.OnMazeDoorUsed -= StartBloodBathSoon;
        EventManager.OnDeath -= StopAllAndReset;

        bloodBathCTS?.Cancel();
        bloodBathCTS?.Dispose();
    }

    private void CollectBloodPipe(GameObject pype)
    {
        BloodPipesInScene.Add(pype);
        pype.SetActive(false);
    }


    private void StopAllAndReset()
    {
        bloodBathCTS?.Cancel();
        bloodBathCTS?.Dispose();
        bloodBathCTS = null;

        BloodLayer.position = StartPosition;

        if (BloodLayer.gameObject.activeSelf) BloodLayer.gameObject.SetActive(false);

        StopPipes();
    }
    
    private async void StartBloodBathSoon()
    {
        
        await UniTask.WaitForSeconds(2);
        
        StartBloodBath();
        
        await UniTask.WaitForSeconds(1);
        
        GameMaster.Instance.DialogueManager.PlayThought(SelectedThoughtBubble);
    }
    
    
    
    private void StartBloodBath()
    {
        StartPipes();

        if (!BloodLayer.gameObject.activeSelf) BloodLayer.gameObject.SetActive(true);
        
        bloodBathCTS?.Cancel();
        bloodBathCTS?.Dispose();

        
        bloodBathCTS = new CancellationTokenSource();

        RaiseBloodLayer(bloodBathCTS.Token).Forget();
    }

    private void StopBloodBath()
    {
        bloodBathCTS?.Cancel();
        bloodBathCTS?.Dispose();
        bloodBathCTS = null;

        BloodLayer.position = StartPosition;
        
        if (BloodLayer.gameObject.activeSelf) BloodLayer.gameObject.SetActive(false);
        
        StopPipes();
    }

    private async UniTask RaiseBloodLayer(CancellationToken cancellationToken)
    {
        
        float elapsedTime = 0f;

        Vector3 startPosition = StartPosition;
        Vector3 targetPosition = new Vector3(
            StartPosition.x,
            StartPosition.y + BloodRiseAmount,
            StartPosition.z
        );

        BloodLayer.position = startPosition;

        try
        {
            while (elapsedTime < BloodBathDuration)
            {
                cancellationToken.ThrowIfCancellationRequested();

                elapsedTime += Time.deltaTime;

                float t = Mathf.Clamp01(elapsedTime / BloodBathDuration);

                Vector3 newPosition = Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

                BloodLayer.position = newPosition;

                await UniTask.Yield(
                    PlayerLoopTiming.Update,
                    cancellationToken
                );
            }

            BloodLayer.position = targetPosition;
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void StartPipes()
    {
        foreach (GameObject pype in BloodPipesInScene)
        {
            pype.SetActive(true);
        }
    }

    private void StopPipes()
    {
        foreach (GameObject pype in BloodPipesInScene)
        {
            pype.SetActive(false);
        }
    }
}