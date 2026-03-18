using System;
using UnityEngine;

public class PickupNotepad : MonoBehaviour
{
    private void Awake()
    {
        if (gameObject.activeSelf) EventManager.RegisterNotepad(gameObject);
    }
}
