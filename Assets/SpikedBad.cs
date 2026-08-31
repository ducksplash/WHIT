using UnityEngine;

public class SpikedBad : MonoBehaviour
{
    private Transform spikesTransform;
    public float knockbackForce = 5f; // Adjustable in Inspector

    private void Start()
    {
        spikesTransform = transform;
    }


}