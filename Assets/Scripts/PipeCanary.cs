using UnityEngine;

public class PipeCanary : MonoBehaviour
{
    // pipe canary just registers the pipe in the director
    // that's all. 
    void Start()
    {
        EventManager.BloodPipeLoaded(gameObject);
    }

}
