using System;
using UnityEngine;

public class PickupNotepad : MonoBehaviour
{
    private void Awake()
    {
        EventManager.RegisterNotepad(gameObject);
    }
}
