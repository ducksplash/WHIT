using System;
using UnityEngine;

public class GotFood : MonoBehaviour
{
    public int foodWorth = 10; // health
    public int scoreWorth = 10; // health
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject.transform.parent.gameObject);
        }
    }
    
    
}
