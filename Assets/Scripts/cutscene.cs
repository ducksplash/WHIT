using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class cutscene : MonoBehaviour 
{
    // Variables to control the cutscene timing
    public float panTime = 5.0f;     // How long it takes to pan to the object
    public float duration = 10.0f;   // How long the cutscene lasts in total

    // Variables to store references to the player and camera
    public GameObject player;
    public GameObject targetObject;
    public Camera mainCamera;
    public GameObject cutsceneBars;
    public GameObject ColliderCube;

    [TextArea(3, 10)]
    public string message;
    
    public bool ZoomToTarget;
    private bool isZooming;    



    // Variables to store the original player and camera positions
    private Vector3 originalPlayerPos;
    private Vector3 originalCameraPos;

    // Variable to store the original camera aspect ratio and field of view
    private float originalAspectRatio;
    private float originalFieldOfView;

    // Variable to keep track of the elapsed cutscene time
    private float elapsedCutsceneTime = 0.0f;

    // Flag to indicate whether the cutscene is active
    private bool isCutsceneActive = false;

    // Start is called before the first frame update
    void Start() 
    {

        ColliderCube.SetActive(false);

        // Get references to the player and camera
        mainCamera = Camera.main;

        // Store the original player and camera positions
        originalPlayerPos = player.transform.position;
        originalCameraPos = mainCamera.transform.position;

        // Store the original camera aspect ratio and field of view
        originalAspectRatio = mainCamera.aspect;
        originalFieldOfView = mainCamera.fieldOfView;

        // Disable the black bars initially
        cutsceneBars.GetComponent<CanvasGroup>().alpha = 0;
    }


    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == 3) 
        {

            if (!GameMaster.CutSceneSeen.ContainsKey(message))
            {
                ActivateCutscene();
                SendALetter(other);
                GameMaster.CutSceneSeen.Add(message, "SYSTEM");
            }
        }
    }

    void Update()
    {
        if (isCutsceneActive)
        {
            

            GameMaster.FROZEN = true;

            // Calculate the elapsed cutscene time
            elapsedCutsceneTime += Time.deltaTime;

            // Check if the cutscene is over
            if (elapsedCutsceneTime >= duration)
            {

                // Reset the elapsed cutscene time
                elapsedCutsceneTime = 0.0f;

                // Deactivate the cutscene
                isCutsceneActive = false;

                StartCoroutine(FadeOutCutsceneBars());
            }
            else
            {
            // Calculate the interpolation factor based on the remaining time
            float t = Mathf.Clamp01(elapsedCutsceneTime / panTime);

            // Calculate the rotation speed based on panTime
            float rotationSpeed = 3.0f / panTime;

            // Calculate the angle between the camera and the target object
            Vector3 targetDirection = targetObject.transform.position - mainCamera.transform.position;
            float angle = Vector3.Angle(mainCamera.transform.forward, targetDirection);

            // Rotate the camera towards the target object
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            // Calculate the angle between the camera and the target object


                if (angle < 5f && !isZooming && ZoomToTarget)
                {
                    StartCoroutine(DoZoom());
                    isZooming = true;
                }
            }
        }
    }






    // Method to activate the cutscene
    public void ActivateCutscene()
    {
        // Activate the cutscene
        isCutsceneActive = true;

        // Reset the elapsed cutscene time
        elapsedCutsceneTime = 0.0f;

        StartCoroutine(FadeInCutsceneBars());
    }




    // Method to activate the cutscene
    public void SendALetter(Collider other)
    {


            var TheCallingObject = gameObject;

            other.gameObject.GetComponent<DialogueManager>().NewDialogue("SYSTEM", message, duration, TheCallingObject);

    }
    // Coroutine to fade out the cutscene bars
    private IEnumerator FadeInCutsceneBars()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.fixedDeltaTime /  (panTime / 2);
            cutsceneBars.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

    }

    // Coroutine to fade out the cutscene bars
    private IEnumerator FadeOutCutsceneBars()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.fixedDeltaTime;
            cutsceneBars.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        cleanup();

    }


    private IEnumerator DoZoom()
    {
        float duration = 2.0f; // The duration of the zoom in seconds
        float startTime = Time.time; // The start time of the zoom
        float startFOV = 70.0f; // The starting field of view
        float endFOV = 40.0f; // The ending field of view

        mainCamera.GetComponent<Zoom>().enabled = false;

        // Zoom in over time
        for (float t = 0.0f; t < duration; t += Time.deltaTime)
        {
            // Interpolate the field of view using Mathf.Lerp
            float fov = Mathf.Lerp(startFOV, endFOV, t / duration);

            // Set the camera's field of view
            mainCamera.fieldOfView = fov;

            // Wait for the end of the frame
            yield return new WaitForEndOfFrame();
        }

        // Set the final field of view to ensure accuracy
        mainCamera.fieldOfView = endFOV;
    }

    private void cleanup()
    {
        
        GameMaster.FROZEN = false;
        mainCamera.fieldOfView = 70.0f;
        mainCamera.GetComponent<Zoom>().enabled = true;
        cutsceneBars.GetComponent<CanvasGroup>().alpha = 0;
        Destroy(gameObject);
    }


}