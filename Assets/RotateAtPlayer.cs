using UnityEngine;

public class RotateAtPlayer : MonoBehaviour
{
    public Camera mainCamera;

    void Start()
    {

    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        Vector3 direction = mainCamera.transform.position - transform.position;
        direction.y = 0; 
        if (direction.sqrMagnitude > 0.01f) 
        {
            transform.rotation = Quaternion.LookRotation(-direction); // Face camera
        }
    }
}