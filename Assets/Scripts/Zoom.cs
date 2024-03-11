using UnityEngine;

public class Zoom : MonoBehaviour
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
        if (!GameMaster.PHONEOUT && !TravelCompanion.Instance.CompanionIsOpen)
        {
            zoomAmount += Input.mouseScrollDelta.y * sensitivity * .05f;
            zoomAmount = Mathf.Clamp01(zoomAmount);
            gameObject.GetComponent<Camera>().fieldOfView = Mathf.Lerp(defaultFOV, maxZoom, zoomAmount);
        }
        else
        {
            gameObject.GetComponent<Camera>().fieldOfView = 70;
            zoomAmount = 0;
        }
    }
}
