using System;
using UnityEngine;

public class PickupPhone : MonoBehaviour
{
    private void Awake()
    {
        if (gameObject.activeSelf) EventManager.RegisterPhone(gameObject);
    }
}
