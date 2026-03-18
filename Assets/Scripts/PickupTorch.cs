using System;
using UnityEngine;

public class PickupTorch : MonoBehaviour
{
    private void Awake()
    {
        if (gameObject.activeSelf) EventManager.RegisterTorch(gameObject);
    }
}
