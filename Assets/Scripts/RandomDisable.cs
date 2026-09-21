using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomDisable : MonoBehaviour
{
    public int percentChanceToDisable = 75;
    
    
    private void Start()
    {
        int randomNumber = Random.Range(1, 101);

        if (randomNumber < percentChanceToDisable)
        {
            gameObject.SetActive(false);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MazeDoor") || other.CompareTag("MetalBars"))
        {
            gameObject.SetActive(false);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("MazeDoor") || other.CompareTag("MetalBars"))
        {
            gameObject.SetActive(false);
        }
    }
}