using System.Collections;
using UnityEngine;



public class ThoughtTrigger : MonoBehaviour
{
    
    [Header("Dev Placeholder Cube")]
    public GameObject ColliderCube;

    private bool triggerUsed;
    public ThoughtName SelectedThoughtBubble = ThoughtName.StartingWork;

    void Start() 
    {
        ColliderCube.SetActive(false);
    }
    

    private void OnTriggerExit(Collider other)
    {
        if (triggerUsed) return;
        triggerUsed = true;
        
        if (other.gameObject.layer != 3) return;

        gameObject.GetComponent<Collider>().enabled = false;

        Debug.Log("play thought");

        GameMaster.Instance.DialogueManager.PlayThought(SelectedThoughtBubble);
        
        gameObject.SetActive(false);
    }

    

}

