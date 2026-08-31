using System;
using UnityEngine;

public class GotPowerUp : MonoBehaviour
{
    public PowerUp selectedPowerUp;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (selectedPowerUp == PowerUp.twoX)
            {

                
            }
            
            Destroy(gameObject.transform.parent.gameObject);
        }
    }
    
    
}

public enum PowerUp
{
    twoX,
    
}