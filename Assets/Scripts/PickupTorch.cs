using System;
using UnityEngine;

public class PickupTorch : MonoBehaviour
{
    private void Awake()
    {
        EventManager.RegisterTorch(gameObject);
    }
}
