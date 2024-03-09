using System.Collections;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class cutscene : MonoBehaviour 
{
    // Variables to control the cutscene timing
    [Header("Time to rotate to face object")]
    public float panTime = 5.0f;     // How long it takes to pan to the object
    [Header("Time to linger looking at object")]
    public float duration = 10.0f;   // How long the cutscene lasts in total
    [Header("Time to zoom & unzoom; set zero to disable")]
    public float zoomDuration = 2f;
    public float unZoomDuration = 0.75f;

    [Header("Optional Dialogue")]
    [SerializeField]
    public Contacts ContactName;
    [TextArea(3, 10)]
    public string message;
    
    // Variables to store references to the player and camera
    public GameObject targetObject;
    public Camera mainCamera;
    public Transform mainCameraTransform;
    public GameObject cutsceneBars;
    private CanvasGroup cutsceneBarsCanvas;
    public GameObject ColliderCube;
    
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
        cutsceneBars = UInstance.Instance.BlackCutsceneBars;
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
                cameraZoom.enabled = false;
                elapsedCutsceneTime = 0.0f;
                CreateDialogue();
                GameMaster.CutSceneSeen.TryAdd(message, "SYSTEM");
                StartCoroutine(ExecuteCutscene());
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
            if (angle < 5f && !isZooming)
            {
                StartCoroutine(DoZoom());
                isZooming = true;
            }

            // Increment elapsed cutscene time
            elapsedCutsceneTime += Time.smoothDeltaTime;

            FirstPersonLook.Instance.SetPlayerRotation(new Vector3(targetDirection.x, targetDirection.y, targetDirection.z));
            yield return new WaitForEndOfFrame(); 
        }


        // Fade out cutscene bars

        yield return StartCoroutine(UndoZoom());
        
    }
    

    public async Task CreateDialogue()
    {
        Debug.Log(ContactName.ToString());
        await DialogueManager.Instance.NewDialogue(ContactName.ToString(), message, duration, true);
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
        // Zoom in over time
        float elapsedTime = 0.0f;

        if (zoomDuration > 0)
        {
            while (elapsedTime < zoomDuration)
            {
                // Calculate the interpolation factor using SmoothStep for smoother interpolation
                float t = Mathf.SmoothStep(0.0f, 1.0f, elapsedTime / zoomDuration);

                // Interpolate the field of view using Mathf.Lerp
                float fov = Mathf.Lerp(originalFieldOfView, targetFieldOfView, t);

                // Set the camera's field of view
                mainCamera.fieldOfView = fov;

                // Increment elapsed time using deltaTime for smoother animation
                elapsedTime += Time.deltaTime;

                yield return null; // Wait for the next frame
            }
            mainCamera.fieldOfView = targetFieldOfView;
        }
        else
        {
            yield return new WaitForSeconds(zoomDuration);
        }
        
        // Set the final field of view to ensure accuracy
    }

    private IEnumerator UndoZoom()
    {
    
        StartCoroutine(FadeOutCutsceneBars());
        if (zoomDuration > 0)
        {
            // Zoom out over time (opposite of zooming in)
            float elapsedTime = 0.0f;
            while (elapsedTime < unZoomDuration)
            {
                // Calculate the interpolation factor using SmoothStep for smoother interpolation
                float t = Mathf.SmoothStep(0.0f, 1.0f, elapsedTime / unZoomDuration);

                // Interpolate the field of view using Mathf.Lerp (reversed start and end values)
                float fov = Mathf.Lerp(targetFieldOfView, originalFieldOfView, t);

                // Set the camera's field of view
                mainCamera.fieldOfView = fov;

                // Increment elapsed time using deltaTime for smoother animation
                elapsedTime += Time.deltaTime;

                yield return null; // Wait for the next frame
            }
            mainCamera.fieldOfView = originalFieldOfView;
        }
        else
        {
            yield return new WaitForSeconds(unZoomDuration);
        }

        // Set the final field of view to ensure accuracy
        cameraZoom.enabled = true;
        GameMaster.FROZEN = false;
    }
    
}


[CustomPropertyDrawer(typeof(cutscene))]
public class ContactDrawerCutscene : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw the enum dropdown field
        property.enumValueIndex = EditorGUI.Popup(position, label.text, property.enumValueIndex, property.enumDisplayNames);

        EditorGUI.EndProperty();
    }
}