using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class cutscene : MonoBehaviour 
{
    // Variables to control the cutscene timing
    public float panTime = 5.0f;     // How long it takes to pan to the object
    public float duration = 10.0f;   // How long the cutscene lasts in total
    
    // Variables to store references to the player and camera
    public GameObject targetObject;
    public Camera mainCamera;
    public Transform mainCameraTransform;
    public GameObject cutsceneBars;
    private CanvasGroup cutsceneBarsCanvas;
    public GameObject ColliderCube;

    [TextArea(3, 10)]
    public string message;

    private float zoomDuration = 2f;
    private float unZoomDuration = 0.75f;
    
    public bool ZoomToTarget;
    private bool isZooming;
    private Zoom cameraZoom;


    // Variable to store the original camera aspect ratio and field of view
    [SerializeField] private float originalFieldOfView = 70;
    [SerializeField] private float targetFieldOfView = 40;

    // Variable to keep track of the elapsed cutscene time
    private float elapsedCutsceneTime;


    private void Awake()
    {
        mainCamera = Camera.main;
        cutsceneBarsCanvas = cutsceneBars.GetComponent<CanvasGroup>();
    }


    // Start is called before the first frame update
    void Start() 
    {
        // stop the visible collider cube from rendering in-game
        ColliderCube.SetActive(false);

        // Get references to the player and camera
        mainCameraTransform = mainCamera.transform;
        cameraZoom = mainCamera.GetComponent<Zoom>();

        originalFieldOfView = mainCamera.fieldOfView;

        // Disable the black bars initially
        cutsceneBarsCanvas.alpha = 0;
    }


    private void OnTriggerEnter(Collider other) 
    {
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

    


    private IEnumerator ExecuteCutscene()
    {

        GameMaster.FROZEN = true;
        
        StartCoroutine(FadeInCutsceneBars());
        
        yield return new WaitForSeconds(1f);
        
        
        while (elapsedCutsceneTime < duration)
        {

            // Calculate the rotation speed based on panTime
            float rotationSpeed = panTime;

            // Calculate the angle between the camera and the target object
            Vector3 targetDirection = targetObject.transform.position - mainCameraTransform.position;
            float angle = Vector3.Angle(mainCameraTransform.forward, targetDirection);

            // Rotate the camera towards the target object
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            mainCameraTransform.rotation = Quaternion.Lerp(mainCameraTransform.rotation, targetRotation, rotationSpeed * Time.smoothDeltaTime);

            // Check if it's time to zoom
            if (angle < 5f && !isZooming && ZoomToTarget)
            {
                StartCoroutine(DoZoom());
                isZooming = true;
            }

            // Increment elapsed cutscene time
            elapsedCutsceneTime += Time.smoothDeltaTime;

            yield return new WaitForEndOfFrame(); 
        }


        // Fade out cutscene bars
        if (ZoomToTarget)
        {
            yield return StartCoroutine(UndoZoom());
        }
    }
    


    // Method to activate the cutscene
    public void ActivateCutscene()
    {
        // Reset the elapsed cutscene time
        elapsedCutsceneTime = 0.0f;

        StartCoroutine(ExecuteCutscene());
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
            t += Time.smoothDeltaTime /  (panTime / 2);
            cutsceneBarsCanvas.alpha = Mathf.Lerp(0f, 1f, t);
            yield return new WaitForEndOfFrame();
            
        }

    }

    // Coroutine to fade out the cutscene bars
    private IEnumerator FadeOutCutsceneBars()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.smoothDeltaTime;
            cutsceneBarsCanvas.alpha = Mathf.Lerp(1f, 0f, t);
            yield return new WaitForEndOfFrame();
            
        }

    }


    private IEnumerator DoZoom()
    {
        cameraZoom.enabled = false;

        // Zoom in over time
        for (float t = 0.0f; t < zoomDuration; t += Time.smoothDeltaTime)
        {
            // Interpolate the field of view using Mathf.Lerp
            float fov = Mathf.Lerp(originalFieldOfView, targetFieldOfView, t / zoomDuration);

            // Set the camera's field of view
            mainCamera.fieldOfView = fov;

            // Wait for the end of the frame
            yield return new WaitForEndOfFrame();
        }
        mainCamera.fieldOfView = targetFieldOfView;
    }
    
    private IEnumerator UndoZoom()
    {
        cameraZoom.enabled = false;

        // Zoom out over time (opposite of zooming in)
        for (float t = 0.0f; t < unZoomDuration; t += Time.smoothDeltaTime)
        {
            // Interpolate the field of view using Mathf.Lerp (reversed start and end values)
            float fov = Mathf.Lerp(targetFieldOfView, originalFieldOfView, t / unZoomDuration);

            // Set the camera's field of view
            mainCamera.fieldOfView = fov;

            // Wait for the end of the frame
            yield return new WaitForEndOfFrame();
        }

        // Set the final field of view to ensure accuracy
        mainCamera.fieldOfView = originalFieldOfView;
        
        yield return StartCoroutine(FadeOutCutsceneBars());

        cleanup();
    }



    private void cleanup()
    {
        mainCamera.fieldOfView = originalFieldOfView;
        cameraZoom.enabled = true;
        cutsceneBarsCanvas.alpha = 0;
        GameMaster.FROZEN = false;
        //Destroy(gameObject);
    }


}