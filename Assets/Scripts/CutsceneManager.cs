using System.Collections;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    [Header("Time to rotate to face object")]
    public float panTime = 5.0f;

    [Header("Time to linger looking at object")]
    public float duration = 10.0f;

    public Camera mainCamera;
    public bool isZooming;
    public Zoom cameraZoom;

    public float originalFieldOfView = 70;
    public float targetFieldOfView = 40;
    public float elapsedCutsceneTime;
    
    public bool CutsceneInProgress;
    public GameObject ColliderCube;

    public void Start()
    {
        Debug.Log("UInstance Start");

        UInstance.Instance.cutsceneBarsCanvas.alpha = 0;
        originalFieldOfView = mainCamera.fieldOfView;
    }

    public IEnumerator ExecuteCutscene(float duration, float panTime, GameObject targetObject, DialogueName selectedMessage)
    {
        if (CutsceneInProgress) yield break;
        
        GameMaster.FROZEN = true;
        CutsceneInProgress = true;
        elapsedCutsceneTime = 0f;

        StartCoroutine(UInstance.Instance.FadeInCutsceneBars(panTime));

        yield return new WaitForSeconds(1f);

        GameMaster.Instance.CutsceneManager.CutsceneDialogue(selectedMessage, duration);

        float zoomTime   = duration * 0.33f;
        float unzoomTime = duration * 0.33f;
        float holdTime   = duration - zoomTime - unzoomTime;

        cameraZoom.enabled = false;

        StartCoroutine(CutsceneZoomSequence(zoomTime, holdTime, unzoomTime));


        while (elapsedCutsceneTime < duration)
        {
            float rotationSpeed = panTime;

            Vector3 targetDirection =
                targetObject.transform.position - mainCamera.transform.position;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            mainCamera.transform.rotation =
                Quaternion.Lerp(mainCamera.transform.rotation, targetRotation, rotationSpeed * Time.smoothDeltaTime);

            elapsedCutsceneTime += Time.smoothDeltaTime;

            yield return new WaitForEndOfFrame();
        }

        Vector3 dir = (targetObject.transform.position - mainCamera.transform.position).normalized;

        float yaw   = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        float pitch = Mathf.Asin(dir.y) * Mathf.Rad2Deg;

        Player.Instance.FirstPersonLook.SetPlayerRotation(new Vector2(yaw, pitch));
        CutsceneInProgress = false;

    }

    private IEnumerator CutsceneZoomSequence(float zoomTime, float holdTime, float unzoomTime)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(zoomTime, 0.0001f);
            mainCamera.fieldOfView = Mathf.Lerp(
                originalFieldOfView,
                targetFieldOfView,
                Mathf.SmoothStep(0, 1, t)
            );
            yield return null;
        }

        mainCamera.fieldOfView = targetFieldOfView;

        if (holdTime > 0f)
            yield return new WaitForSeconds(holdTime);

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(unzoomTime, 0.0001f);
            mainCamera.fieldOfView = Mathf.Lerp(
                targetFieldOfView,
                originalFieldOfView,
                Mathf.SmoothStep(0, 1, t)
            );
            yield return null;
        }

        mainCamera.fieldOfView = originalFieldOfView;

        StartCoroutine(UInstance.Instance.FadeOutCutsceneBars());

        cameraZoom.enabled = true;
        GameMaster.FROZEN = false;
    }

    public async Task CutsceneDialogue(DialogueName selectedMessage, float duration)
    {
        await GameMaster.Instance.DialogueManager.NewDialogue(selectedMessage, duration, true);
    }
}
