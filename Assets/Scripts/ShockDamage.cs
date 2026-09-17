using UnityEngine;

public class ShockDamage : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Adjust the tag/layer check to match your player
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null)
        {

        }
    }
}