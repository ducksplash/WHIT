using System;
using UnityEngine;

public class PickupPhone : MonoBehaviour
{
    private void Awake()
    {
        EventManager.RegisterPhone(gameObject);
    }
}
