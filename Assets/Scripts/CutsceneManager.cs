using System.Collections;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class CutsceneManager : MonoBehaviour 
{
    // Variables to control the cutscene timing
    [Header("Time to rotate to face object")]
    public float panTime = 5.0f;     // How long it takes to pan to the object
    [Header("Time to linger looking at object")]
    public float duration = 10.0f;   // How long the cutscene lasts in total

    public Camera mainCamera;
    public bool isZooming;
    public Zoom cameraZoom;

    public float originalFieldOfView = 70;
    public float targetFieldOfView = 40;
    
    public float zoomDuration = 2f;
    public float unZoomDuration = 0.75f;

    public float elapsedCutsceneTime;
    

    public GameObject ColliderCube;
    
    public void Start()
    {    
        Debug.Log("UInstance Start");
        
        UInstance.Instance.cutsceneBarsCanvas.alpha = 0;  
        
        cameraZoom = mainCamera.GetComponent<Zoom>();

        originalFieldOfView = mainCamera.fieldOfView;
    }


    
    
    
    
    
    
    public IEnumerator ExecuteCutscene(float duration, float panTime, GameObject targetObject, Contacts ContactName, DialogueSelectorTemp selectedMessage)
    {

        GameMaster.FROZEN = true;
        
        StartCoroutine(UInstance.Instance.FadeInCutsceneBars(panTime));
        
        yield return new WaitForSeconds(1f);
        
        
        solo.Instance.CutsceneManager.CutsceneDialogue(duration, ContactName, selectedMessage);

        
        while (elapsedCutsceneTime < duration)
        {

            // Calculate the rotation speed based on panTime
            float rotationSpeed = panTime;

            // Calculate the angle between the camera and the target object
            Vector3 targetDirection = targetObject.transform.position - mainCamera.transform.position;
            float angle = Vector3.Angle(mainCamera.transform.forward, targetDirection);

            // Rotate the camera towards the target object
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, targetRotation, rotationSpeed * Time.smoothDeltaTime);

            
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
    
        StartCoroutine(UInstance.Instance.FadeOutCutsceneBars());
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
    

    // target object is the object camera will zoom in and look at
    // duration from start to finish
    // pan time is time allowed to pan (a sweeping rotation) toward the object if not already looking at it
    // contact name is the person who's name should appear as that which 'said' the dialogue.
    // DialogueSelector is a temporary enum with a selection of messages for ebug. will be replaced with scriptable objects from DialogueManager

    public async Task CutsceneDialogue(float duration, Contacts ContactName, DialogueSelectorTemp selectedMessage)
    {
        string message = TempSelectMessage(selectedMessage);
        
        Debug.Log(ContactName.ToString());
        await solo.Instance.DialogueManager.NewDialogue(ContactName.ToString(), message, duration, true);
    }

    
    
    
    // temp until scriptable objects take over
    // Dialogue Manager will supply messages in the future

    public string TempSelectMessage(DialogueSelectorTemp selectedMessage)
    {
        string returnMessage = "";
        switch (selectedMessage)
        {
            case DialogueSelectorTemp.NoraBathroom:
                returnMessage = "st";
                break;
            case DialogueSelectorTemp.NoraCorkboard:
                returnMessage = "I'll keep notes and the like here, sure that way if I forget what I'm to be at it's on the board and I can look at it.";
                break;
            case DialogueSelectorTemp.NoraIncinerator:
                returnMessage = "rd";
                break;
        }

        return returnMessage;
    }
    
    
}


public enum DialogueSelectorTemp
{
    NoraBathroom,
    NoraCorkboard,
    NoraIncinerator
}



// [CustomPropertyDrawer(typeof(cutscene))]
// public class ContactDrawerCutscene : PropertyDrawer
// {
//     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//     {
//         EditorGUI.BeginProperty(position, label, property);
//
//         // Draw the enum dropdown field
//         property.enumValueIndex = EditorGUI.Popup(position, label.text, property.enumValueIndex, property.enumDisplayNames);
//
//         EditorGUI.EndProperty();
//     }
// }