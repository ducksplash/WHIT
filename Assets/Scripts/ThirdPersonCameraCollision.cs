using UnityEngine;

public class ThirdPersonCameraCollision : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    public Transform cameraPivot;
    public Zoom zoomScript;

    [Header("Collision")]
    public LayerMask collisionMask;

    public float collisionRadius = 0.28f;

    [Tooltip("How close before forcing First Person")]
    public float firstPersonSnapDistance = 0.9f;

    [Header("Smoothing")]
    public float smoothSpeed = 15f;

    [Header("Debug")]
    public bool drawDebug = true;

    private Vector3 _idealLocalPosition;
    private Vector3 _currentLocalPosition;
    private Vector3 _velocity;

    private void Start()
    {
        if (playerTransform == null && transform.parent != null)
            playerTransform = transform.parent;

        _idealLocalPosition = transform.localPosition;
        _currentLocalPosition = _idealLocalPosition;

        // Ignore player layer
        collisionMask &= ~(1 << playerTransform.gameObject.layer);
    }

    private void LateUpdate()
    {
        if (playerTransform == null ||
            cameraPivot == null ||
            zoomScript == null)
        {
            return;
        }

        if (!zoomScript.IsThirdPersonActive)
        {
            ReturnToIdealPosition();
            return;
        }

        ResolveCollision();
    }

    private void ReturnToIdealPosition()
    {
        _currentLocalPosition =
            Vector3.SmoothDamp(
                _currentLocalPosition,
                _idealLocalPosition,
                ref _velocity,
                1f / smoothSpeed
            );
        
        
        _currentLocalPosition = Vector3.Lerp(
            _currentLocalPosition,
            _idealLocalPosition,
            Time.deltaTime * smoothSpeed
        );
        transform.localPosition = _currentLocalPosition;
    }

    private void ResolveCollision()
    {
        Vector3 pivot = cameraPivot.position;

        // Desired world position
        Vector3 desiredWorld = transform.parent.position + transform.parent.rotation * _idealLocalPosition;

        Vector3 castDirection =
            (desiredWorld - pivot).normalized;

        float idealDistance =
            Vector3.Distance(pivot, desiredWorld);

        float resolvedDistance = idealDistance;

        if (Physics.SphereCast(
            pivot,
            collisionRadius,
            castDirection,
            out RaycastHit hit,
            idealDistance,
            collisionMask,
            QueryTriggerInteraction.Ignore))
        {
            resolvedDistance = Mathf.Max(
                hit.distance - 0.05f,
                0f
            );
        }

        // SNAP TO FP BEFORE ENTERING PLAYER
        if (resolvedDistance <= firstPersonSnapDistance)
        {
            zoomScript.zoomAmount = 0f;
            return;
        }



        Vector3 localTarget = _idealLocalPosition;

        // Assuming camera sits behind player on local Z
        localTarget.z =
            Mathf.Lerp(
                0f,
                _idealLocalPosition.z,
                resolvedDistance / idealDistance
            );

        _currentLocalPosition =
            Vector3.SmoothDamp(
                _currentLocalPosition,
                localTarget,
                ref _velocity,
                1f / smoothSpeed
            );

        transform.localPosition = _currentLocalPosition;

        // Inform zoom system
        float closeness =
            resolvedDistance / idealDistance;

        zoomScript.ReportCameraCloseness(closeness);

        if (drawDebug)
        {
            Debug.DrawLine(pivot, desiredWorld, Color.green);
            Debug.DrawLine(pivot, transform.position, Color.yellow);
        }
    }
}