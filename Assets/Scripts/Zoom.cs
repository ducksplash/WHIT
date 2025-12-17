using System.Collections;
using UnityEngine;

public class Zoom : MonoBehaviour
{
    public float sensitivity = 1;
    public float defaultFOV;
    public float maxZoom = 10;
    public float zoomAmount;

    public bool zoomAllowed;

    void Start()
    {
        StartCoroutine(WaitForGameMaster());
        defaultFOV = gameObject.GetComponent<Camera>().fieldOfView;
    }

    void Update()
    {
        
        
        if (zoomAllowed)
        {
            if (!GameMaster.PHONEOUT && !GameMaster.Instance.TravelCompanion.CompanionIsOpen)
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


    private IEnumerator WaitForGameMaster()
    {
        while (GameMaster.Instance == null)
        {
            yield return null;
        }

        if (GameMaster.Instance != null) zoomAllowed = true;
    }
    
}
