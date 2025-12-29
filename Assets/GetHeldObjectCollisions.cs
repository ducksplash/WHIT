using System;
using UnityEngine;

public class GetHeldObjectCollisions : MonoBehaviour
{
    private float collisionGraceTime = 0.75f;
    private float collisionTimer = 0f;
    private bool colliding = false;

    
    private void Update()
    {
        if (!colliding)
        {
            collisionTimer = 0f;
            return;
        }

        collisionTimer += Time.deltaTime;

        if (collisionTimer >= collisionGraceTime)
        {
            GameMaster.Instance.Pickup.DropItem();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("player")) return;

        colliding = true;
    }

    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("player")) return;

        colliding = true;
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("player")) return;

        colliding = false;
        collisionTimer = 0f;
    }
}