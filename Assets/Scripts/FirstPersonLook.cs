// FirstPersonLook.cs
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField] Transform character;
    [SerializeField] InputActionReference lookAction;

    Vector2 currentMouseLook;
    Vector2 appliedMouseDelta;

    public float sensitivity = 0.01f;
    public float smoothing = 5f;

    // ---- Aim Assist Lock ----
    private bool _lookLocked;
    private Coroutine _lockRoutine;

    // ✅ Camera height (crouch) support
    [Header("Camera Height (Crouch)")]
    [Tooltip("Local pivot to move up/down. If null, uses this transform.")]
    [SerializeField] private Transform cameraPivot;

    [Tooltip("Negative lowers camera when crouched (local Y offset).")]
    [SerializeField] private float crouchLocalYOffset = -0.5f;

    [Tooltip("Seconds to blend between standing/crouch camera heights.")]
    [SerializeField] private float crouchBlendSeconds = 0.12f;

    private Vector3 _pivotStartLocalPos;
    private Coroutine _heightCo;

    // ✅ Smooth LookAt (minimal)
    [Header("LookAt Device (Smooth)")]
    [SerializeField] private float lookAtBlendSeconds = 0.25f;
    [SerializeField] private AnimationCurve lookAtCurve = null; // optional
    private Coroutine _lookAtCo;

    private void Awake()
    {
        lookAction = GameMaster.Instance.InputManager.LookAction;
    }

    private void Start()
    {
        if (cameraPivot == null) cameraPivot = transform;
        _pivotStartLocalPos = cameraPivot.localPosition;

        sensitivity = GameMaster.Instance.MouseSensitivity;

        EventManager.OnStartComputer += LookAtDevice;
        EventManager.OnStartPhone += LookAtDevice;

        lookAction.action.Enable();
    }

    void FixedUpdate()
    {
        if (GameMaster.Instance.PauseManager.IsPaused) return;
        if (GameMaster.Instance.PLAYERBUSY && !Player.Instance.MoveOverride) return;

        transform.localRotation = Quaternion.AngleAxis(-currentMouseLook.y, Vector3.right);
        character.localRotation = Quaternion.AngleAxis(currentMouseLook.x, Vector3.up);
    }

    private void LateUpdate()
    {
        if (GameMaster.Instance.PauseManager.IsPaused) return;
        if (GameMaster.Instance.PLAYERBUSY && !Player.Instance.MoveOverride) return;

        if (_lookLocked)
        {
            appliedMouseDelta = Vector2.zero;
            return;
        }

        Vector2 rawMouse = lookAction.action.ReadValue<Vector2>();

        Vector2 smoothMouseDelta = Vector2.Scale(rawMouse, Vector2.one * sensitivity * smoothing);
        appliedMouseDelta = Vector2.Lerp(appliedMouseDelta, smoothMouseDelta, 1f / smoothing);

        currentMouseLook += appliedMouseDelta;
        currentMouseLook.y = Mathf.Clamp(currentMouseLook.y, -60, 60);
    }

    public void SetPlayerRotation(Vector2 rotation)
    {
        currentMouseLook = rotation;
    }

    /// <summary>
    /// Temporarily ignores all look input for a short time (prevents jitter/fighting).
    /// Uses realtime so it still works if timescale changes.
    /// </summary>
    public void AimAssistLock(float seconds)
    {
        seconds = Mathf.Max(0f, seconds);

        if (_lockRoutine != null)
            StopCoroutine(_lockRoutine);

        _lockRoutine = StartCoroutine(AimAssistLockRoutine(seconds));
    }

    private IEnumerator AimAssistLockRoutine(float seconds)
    {
        _lookLocked = true;
        appliedMouseDelta = Vector2.zero;

        yield return new WaitForSecondsRealtime(seconds);

        _lookLocked = false;
        appliedMouseDelta = Vector2.zero;
        _lockRoutine = null;
    }

    /// <summary>
    /// Snap ONLY yaw to face a world point by modifying currentMouseLook
    /// (keeps camera/character rotations consistent).
    /// </summary>
    public void SnapYawTowardWorldPoint(Vector3 worldPoint)
    {
        Vector3 from = character != null ? character.position : transform.position;

        Vector3 to = worldPoint - from;
        to.y = 0f; // yaw-only
        if (to.sqrMagnitude < 0.000001f) return;

        float targetYaw = Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg;
        currentMouseLook.x = targetYaw;

        appliedMouseDelta = Vector2.zero;
    }

    // ------------------------------------------------------------
    // ✅ Minimal Smooth LookAt (just replaces instant LookAt)
    // ------------------------------------------------------------
    public void LookAtDevice(Transform deviceTransform)
    {
        if (deviceTransform == null) return;

        if (_lookAtCo != null)
            StopCoroutine(_lookAtCo);

        _lookAtCo = StartCoroutine(SmoothLookAtCoroutine(deviceTransform, lookAtBlendSeconds));
    }

    private IEnumerator SmoothLookAtCoroutine(Transform target, float seconds)
    {
        // Optional: lock input during the blend so it doesn't fight the camera motion.
        _lookLocked = true;
        appliedMouseDelta = Vector2.zero;

        Quaternion startRot = transform.rotation;

        // Capture target rotation ONCE (prevents chasing if target is in the hand / moving)
        Vector3 dir0 = target.position - transform.position;
        Quaternion endRot = (dir0.sqrMagnitude > 0.000001f)
            ? Quaternion.LookRotation(dir0.normalized, Vector3.up)
            : startRot;

        if (seconds <= 0f)
        {
            transform.rotation = endRot;
            _lookLocked = false;
            _lookAtCo = null;
            yield break;
        }

        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / seconds);
            if (lookAtCurve != null) a = lookAtCurve.Evaluate(a);

            transform.rotation = Quaternion.Slerp(startRot, endRot, a);
            yield return null;
        }

        transform.rotation = endRot;

        // Unlock if you only wanted to lock during the blend.
        // If your device mode freezes look anyway, this won't matter.
        _lookLocked = false;

        _lookAtCo = null;
    }

    // ------------------------------------------------------------
    // ✅ Crouch camera height API (called by Player)
    // ------------------------------------------------------------
    public void SetCrouch(bool crouched)
    {
        if (cameraPivot == null) cameraPivot = transform;

        if (_heightCo != null)
            StopCoroutine(_heightCo);

        Vector3 target = _pivotStartLocalPos + new Vector3(0f, crouched ? crouchLocalYOffset : 0f, 0f);
        _heightCo = StartCoroutine(BlendPivotHeight(target, crouchBlendSeconds));
    }

    private IEnumerator BlendPivotHeight(Vector3 targetLocalPos, float seconds)
    {
        if (cameraPivot == null) yield break;

        Vector3 start = cameraPivot.localPosition;

        if (seconds <= 0f)
        {
            cameraPivot.localPosition = targetLocalPos;
            _heightCo = null;
            yield break;
        }

        float t = 0f;
        while (t < seconds)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / seconds);
            cameraPivot.localPosition = Vector3.Lerp(start, targetLocalPos, a);
            yield return null;
        }

        cameraPivot.localPosition = targetLocalPos;
        _heightCo = null;
    }
}