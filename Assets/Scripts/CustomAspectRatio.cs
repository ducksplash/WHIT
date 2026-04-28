using System.Collections;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CustomAspectRatio : MonoBehaviour
{
    [Header("Aspect Ratio Settings")]
    public float aspectWidth  = 16f;
    public float aspectHeight = 9f;

    [Header("Orthographic Settings")]
    [Tooltip("Half the height of the capture area in world units. Match the mirror's physical half-height.")]
    public float orthoSize = 1.5f;

    [Header("Letterbox Settings")]
    [Range(0f, 0.5f)]
    public float letterboxAmount = 0f;

    [Header("Tracking")]
    public Transform player;
    public float maxTrackingDistance = 10f;

    private Camera     cam;
    private Vector3    baseWorldPos;
    private Vector3    baseLocalPos;
    private Quaternion baseLocalRot;

    // -------------------------------------------------------------------------

    void Start() => StartCoroutine(GetPlayer());

    IEnumerator GetPlayer()
    {
        while (Player.Instance == null)
            yield return new WaitForSeconds(0.5f);
        player = Player.Instance.transform;
    }

    void OnEnable()
    {
        cam          = GetComponent<Camera>();
        baseWorldPos = transform.position;
        baseLocalPos = transform.localPosition;
        baseLocalRot = transform.localRotation;
    }

    void Update()     => ApplySettings();
    void LateUpdate() => HandleTracking();

    // -------------------------------------------------------------------------

    void HandleTracking()
    {
        if (!player) return;

        float distance = Vector3.Distance(player.position, baseWorldPos);

        // Grey zone — hold at base position when player is out of range
        if (distance > maxTrackingDistance)
        {
            transform.localPosition = baseLocalPos;
            transform.localRotation = baseLocalRot;
        }
        // Camera is fixed — orthographic projection places the player
        // at their natural position in the render texture automatically
    }

    // -------------------------------------------------------------------------

    void ApplySettings()
    {
        if (!cam) return;

        cam.aspect           = aspectWidth / aspectHeight;
        cam.orthographic     = true;
        cam.orthographicSize = orthoSize;

        if (letterboxAmount > 0f)
        {
            float h = 1f - letterboxAmount * 2f;
            cam.rect = new Rect(0f, letterboxAmount, 1f, h);
        }
        else
        {
            cam.rect = new Rect(0f, 0f, 1f, 1f);
        }
    }
}