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

    // ✅ Camera height (crouch/crawl) support
    [Header("Camera Height (Crouch/Crawl)")]
    [Tooltip("Local pivot to move up/down. If null, uses this transform.")]
    [SerializeField] private Transform cameraPivot;

    [Tooltip("Negative lowers camera when crouched (local Y offset).")]
    [SerializeField] private float crouchLocalYOffset = -0.5f;

    [Tooltip("Negative lowers camera when crawling (local Y offset). Should be LOWER than crouchLocalYOffset.")]
    [SerializeField] private float crawlLocalYOffset = -0.9f;

    [Tooltip("Seconds to blend between standing/crouch camera heights.")]
    [SerializeField] private float crouchBlendSeconds = 0.12f;

    [Tooltip("Seconds to blend between crouch/crawl camera heights.")]
    [SerializeField] private float crawlBlendSeconds = 0.12f;

    [Header("Pitch Limits")]
    [SerializeField] private float pitchClampMin = -60f;
    [SerializeField] private float pitchClampMax = 60f;

    public bool bypassGM;
    
    private Vector3 _pivotStartLocalPos;
    private Coroutine _heightCo;

    // NEW: stance tracking so SetCrawl(false) can return to crouch if needed
    private enum HeightMode { Stand, Crouch, Crawl }
    private HeightMode _heightMode = HeightMode.Stand;

    private bool _isCrouched;
    private bool _isCrawling;

    // ✅ Smooth LookAt (minimal)
    [Header("LookAt Device (Smooth)")]
    [SerializeField] private float lookAtBlendSeconds = 0.25f;
    [SerializeField] private AnimationCurve lookAtCurve = null; // optional
    private Coroutine _lookAtCo;

    // ------------------------------------------------------------
    // ✅ "Keep device centered" while camera app is open
    // ------------------------------------------------------------
    [Header("Phone Camera Mode: Keep device centered in view")]
    [Tooltip("How tightly device follows the camera pose while aiming (bigger = snappier).")]
    [SerializeField] private float deviceFollowSharpness = 60f;

    private Transform _currentDevice;

    // Saved device pose relative to pivot (aiming mode)
    private Vector3 _deviceLocalPosInPivot;
    private Quaternion _deviceLocalRotInPivot;

    private bool _keepDeviceCentered;

    // ------------------------------------------------------------
    // ✅ baseline pose captured when device is opened
    // ------------------------------------------------------------
    private Vector3 _deviceBaseLocalPos;
    private Quaternion _deviceBaseLocalRot;
    private bool _deviceBaseCaptured;

    private void Awake()
    {
        if (GameMaster.Instance != null)
        {

            lookAction = GameMaster.Instance.InputManager.LookAction;
        }
    }

    private void Start()
    {
        if (cameraPivot == null) cameraPivot = transform;
        _pivotStartLocalPos = cameraPivot.localPosition;

        sensitivity = GameMaster.Instance.MouseSensitivity;

        // “Device opened / started”
        EventManager.OnStartComputer += LookAtDevice;
        EventManager.OnStartPhone += LookAtDevice;
        EventManager.OnStartNotepad += LookAtDevice;

        // Phone camera app
        EventManager.OnCameraOpen += PhoneCameraOpen;
        EventManager.OnCameraClosed += PhoneCameraClosed;

        lookAction.action.Enable();
    }

    void FixedUpdate()
    {
        if (!bypassGM)
        {
            if (GameMaster.Instance.PauseManager.IsPaused) return;
            if (GameMaster.Instance.PLAYERBUSY && !Player.Instance.MoveOverride) return;
        }
        

        if (cameraPivot != null)
            cameraPivot.localRotation = Quaternion.AngleAxis(-currentMouseLook.y, Vector3.right);
        else
            transform.localRotation = Quaternion.AngleAxis(-currentMouseLook.y, Vector3.right);

        if (character != null)
            character.localRotation = Quaternion.AngleAxis(currentMouseLook.x, Vector3.up);
    }

    private void LateUpdate()
    {
        if (!bypassGM)
        {
            if (GameMaster.Instance.PauseManager.IsPaused) return;
            if (GameMaster.Instance.PLAYERBUSY && !Player.Instance.MoveOverride) return;
        }
        

        if (_lookLocked)
        {
            appliedMouseDelta = Vector2.zero;
            return;
        }

        Vector2 rawMouse = lookAction.action.ReadValue<Vector2>();

        Vector2 smoothMouseDelta = Vector2.Scale(rawMouse, Vector2.one * sensitivity * smoothing);
        appliedMouseDelta = Vector2.Lerp(appliedMouseDelta, smoothMouseDelta, 1f / smoothing);

        currentMouseLook += appliedMouseDelta;
        currentMouseLook.y = Mathf.Clamp(currentMouseLook.y, pitchClampMin, pitchClampMax);

        if (_keepDeviceCentered && _currentDevice != null)
            FollowDeviceToPivotPose();
    }

    public void SetPlayerRotation(Vector2 rotation)
    {
        currentMouseLook = rotation;
    }

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

    public void SnapYawTowardWorldPoint(Vector3 worldPoint)
    {
        Vector3 from = character != null ? character.position : transform.position;

        Vector3 to = worldPoint - from;
        to.y = 0f;
        if (to.sqrMagnitude < 0.000001f) return;

        float targetYaw = Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg;
        currentMouseLook.x = targetYaw;

        appliedMouseDelta = Vector2.zero;
    }

    // ------------------------------------------------------------
    // ✅ Device opened / focus on device
    // ------------------------------------------------------------
    public void LookAtDevice(Transform deviceTransform)
    {
        if (deviceTransform == null) return;

        _currentDevice = deviceTransform;
        CaptureDeviceBaselineLocalPose();

        if (_lookAtCo != null) StopCoroutine(_lookAtCo);

        _lookAtCo = StartCoroutine(SmoothLookAtCoroutine(deviceTransform, lookAtBlendSeconds));
    }

    private void CaptureDeviceBaselineLocalPose()
    {
        if (_currentDevice == null) return;

        _deviceBaseLocalPos = _currentDevice.localPosition;
        _deviceBaseLocalRot = _currentDevice.localRotation;
        _deviceBaseCaptured = true;
    }

    private void ApplyDeviceBaselineLocalPose()
    {
        if (_currentDevice == null || !_deviceBaseCaptured) return;

        _currentDevice.localPosition = _deviceBaseLocalPos;
        _currentDevice.localRotation = _deviceBaseLocalRot;
    }

    private IEnumerator SmoothLookAtCoroutine(Transform target, float seconds)
    {
        _lookLocked = true;
        appliedMouseDelta = Vector2.zero;

        Quaternion startRot = (cameraPivot != null) ? cameraPivot.rotation : transform.rotation;

        Vector3 origin = (cameraPivot != null) ? cameraPivot.position : transform.position;
        Vector3 dir0 = target.position - origin;

        Quaternion endRot = (dir0.sqrMagnitude > 0.000001f)
            ? Quaternion.LookRotation(dir0.normalized, Vector3.up)
            : startRot;

        if (seconds <= 0f)
        {
            if (cameraPivot != null) cameraPivot.rotation = endRot;
            else transform.rotation = endRot;

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

            Quaternion r = Quaternion.Slerp(startRot, endRot, a);

            if (cameraPivot != null) cameraPivot.rotation = r;
            else transform.rotation = r;

            yield return null;
        }

        if (cameraPivot != null) cameraPivot.rotation = endRot;
        else transform.rotation = endRot;

        _lookLocked = false;
        _lookAtCo = null;
    }

    // ------------------------------------------------------------
    // ✅ Height API (called by Player)
    // ------------------------------------------------------------

    public void SetCrouch(bool crouched)
    {
        _isCrouched = crouched;

        // Optional rule: if we stand up, we cannot remain crawling
        if (!crouched)
            _isCrawling = false;

        ApplyResolvedHeightMode();
    }

    public void SetCrawl(bool crawling)
    {
        _isCrawling = crawling;

        // Optional rule: crawling implies crouched (prevents weird stand+crawl combos)
        if (crawling)
            _isCrouched = true;

        ApplyResolvedHeightMode();
    }

    private void ApplyResolvedHeightMode()
    {
        if (cameraPivot == null) cameraPivot = transform;

        HeightMode previous = _heightMode;

        // Resolve priority: Crawl > Crouch > Stand
        if (_isCrawling) _heightMode = HeightMode.Crawl;
        else if (_isCrouched) _heightMode = HeightMode.Crouch;
        else _heightMode = HeightMode.Stand;

        // Pick correct offset
        float yOffset = 0f;
        if (_heightMode == HeightMode.Crouch) yOffset = crouchLocalYOffset;
        else if (_heightMode == HeightMode.Crawl) yOffset = crawlLocalYOffset;

        // Pick correct blend duration (stand<->crouch uses crouchBlend, crouch<->crawl uses crawlBlend)
        float duration =
            (previous == HeightMode.Crawl || _heightMode == HeightMode.Crawl)
                ? crawlBlendSeconds
                : crouchBlendSeconds;

        Vector3 target = _pivotStartLocalPos + new Vector3(0f, yOffset, 0f);

        if (_heightCo != null)
            StopCoroutine(_heightCo);

        _heightCo = StartCoroutine(BlendPivotHeight(target, duration));
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

    // ------------------------------------------------------------
    // ✅ Camera app events
    // ------------------------------------------------------------
    public void PhoneCameraOpen()
    {
        if (_currentDevice == null) return;

        if (!_deviceBaseCaptured)
            CaptureDeviceBaselineLocalPose();

        Transform pivot = (cameraPivot != null) ? cameraPivot : transform;

        _deviceLocalPosInPivot = pivot.InverseTransformPoint(_currentDevice.position);
        _deviceLocalRotInPivot = Quaternion.Inverse(pivot.rotation) * _currentDevice.rotation;

        _keepDeviceCentered = true;
        appliedMouseDelta = Vector2.zero;
    }

    public void PhoneCameraClosed()
    {
        _keepDeviceCentered = false;
        appliedMouseDelta = Vector2.zero;

        ApplyDeviceBaselineLocalPose();
    }

    private void FollowDeviceToPivotPose()
    {
        float k = 1f - Mathf.Exp(-deviceFollowSharpness * Time.deltaTime);

        Transform pivot = (cameraPivot != null) ? cameraPivot : transform;

        Vector3 desiredPos = pivot.TransformPoint(_deviceLocalPosInPivot);
        Quaternion desiredRot = pivot.rotation * _deviceLocalRotInPivot;

        _currentDevice.position = Vector3.Lerp(_currentDevice.position, desiredPos, k);
        _currentDevice.rotation = Quaternion.Slerp(_currentDevice.rotation, desiredRot, k);
    }
}