using System;
using UnityEngine;

public class CapturingSelfieWithFriends : MonoBehaviour
{

    public bool WithTheBoys;



    private void OnTriggerEnter(Collider other)
    {
        WithTheBoys = true;
    }
    private void OnTriggerExit(Collider other)
    {
        WithTheBoys = false;
    }
}
