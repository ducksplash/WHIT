using System;
using UnityEngine;

public class GotLoot : MonoBehaviour
{
    public int scoreWorth = 500; // points

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject.transform.parent.gameObject);
        }
        
    }
    
    
}