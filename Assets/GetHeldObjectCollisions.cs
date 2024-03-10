using System;
using UnityEngine;

public class GetHeldObjectCollisions : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        // Check if the other object is not on the "Player" layer
        if (other.gameObject.layer != LayerMask.NameToLayer("player"))
        {
            // Perform the action if the other object is not on the "Player" layer
            pickup.Instance.DropItem();
        }
    }
}