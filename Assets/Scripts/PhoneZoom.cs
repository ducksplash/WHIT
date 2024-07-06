using UnityEngine;

public class PhoneZoom : MonoBehaviour
{
    public float sensitivity = 1;
    public float defaultFOV;
    public float maxZoom = 10;
    public float zoomAmount;

    void Start()
    {
        defaultFOV = gameObject.GetComponent<Camera>().fieldOfView;
    }

    void Update()
    {
        // if (GameMaster.PHONEOUT && gameObject.GetComponentInParent<phone>().CameraOpen)
        // {
        //     zoomAmount += Input.mouseScrollDelta.y * sensitivity * .05f;
        //     zoomAmount = Mathf.Clamp01(zoomAmount);
        //     gameObject.GetComponent<Camera>().fieldOfView = Mathf.Lerp(defaultFOV, maxZoom, zoomAmount);
        // }
    }
}
