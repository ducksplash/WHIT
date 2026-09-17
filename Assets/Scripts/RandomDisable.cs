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
        if (other.CompareTag("MazeDoor"))
        {
            Debug.Log("Light disabled by OnTriggerEnter MazeDoor");
            gameObject.SetActive(false);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        Debug.Log("Light disabled by OnTriggerStay MazeDoor");
        if (other.CompareTag("MazeDoor"))
        {
            gameObject.SetActive(false);
        }
    }
}