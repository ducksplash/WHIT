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

    private bool _lookLocked;
    private Coroutine _lockRoutine;

    [Header("Camera Height (Crouch/Crawl)")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float crouchLocalYOffset = -0.5f;
    [SerializeField] private float crawlLocalYOffset = -0.9f;
    [SerializeField] private float crouchBlendSeconds = 0.12f;
    [SerializeField] private float crawlBlendSeconds = 0.12f;

    [Header("Pitch Limits")]
    [SerializeField] private float pitchClampMin = -60f;
    [SerializeField] private float pitchClampMax = 60f;

    [Header("Initial Look")]
    [SerializeField] private bool useSceneStartingRotation = true;
    [SerializeField] private float startingYaw = 0f;
    [SerializeField] private float startingPitch = 0f;
    
    public bool bypassGM;

    private Vector3 _pivotStartLocalPos;
    private Coroutine _heightCo;

    private enum HeightMode { Stand, Crouch, Crawl }
    private HeightMode _heightMode = HeightMode.Stand;

    private bool _isCrouched;
    private bool _isCrawling;

    [Header("LookAt Device (Smooth)")]
    [SerializeField] private float lookAtBlendSeconds = 0.25f;
    [SerializeField] private AnimationCurve lookAtCurve = null;

    private Coroutine _lookAtCo;

    [Header("Camera Mode Device Follow")]
    [SerializeField] private float deviceFollowSharpness = 60f;

    private Transform _currentDevice;
    private bool _cameraModeActive;

    private Vector3 _deviceLocalPosInPivot;
    private Quaternion _deviceLocalRotInPivot;

    private Vector3 _deviceBaseLocalPos;
    private Quaternion _deviceBaseLocalRot;
    private bool _deviceBaseCaptured;

    public bool deviceCheckOverride;
    public DeviceHelperOutside deviceTypeSupplemental;

    private void Start()
    {
        if (cameraPivot == null) cameraPivot = transform;
        _pivotStartLocalPos = cameraPivot.localPosition;

        if (useSceneStartingRotation)
        {
            float yaw = character != null ? character.localEulerAngles.y : transform.localEulerAngles.y;

            float pitchSource = cameraPivot != null ? cameraPivot.localEulerAngles.x : transform.localEulerAngles.x;
            float pitch = NormalizeAngle180(pitchSource);

            currentMouseLook.x = yaw;
            currentMouseLook.y = Mathf.Clamp(-pitch, pitchClampMin, pitchClampMax);
        }
        else
        {
            currentMouseLook.x = startingYaw;
            currentMouseLook.y = Mathf.Clamp(startingPitch, pitchClampMin, pitchClampMax);
        }

        if (deviceCheckOverride)
            sensitivity = deviceTypeSupplemental.returnableSensitivity;
        else
            sensitivity = GameMaster.Instance.MouseSensitivity;

        EventManager.OnStartComputer += LookAtDevice;
        EventManager.OnStartPhone += LookAtDevice;
        EventManager.OnStartNotepad += LookAtDevice;

        EventManager.OnCameraOpen += PhoneCameraOpen;
        EventManager.OnCameraClosed += PhoneCameraClosed;
    }

    private void OnDestroy()
    {
        EventManager.OnStartComputer -= LookAtDevice;
        EventManager.OnStartPhone -= LookAtDevice;
        EventManager.OnStartNotepad -= LookAtDevice;

        EventManager.OnCameraOpen -= PhoneCameraOpen;
        EventManager.OnCameraClosed -= PhoneCameraClosed;
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

        if (_cameraModeActive && _currentDevice != null)
        {
            FollowDeviceToView();
        }
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

    public void LookAtDevice(Transform deviceTransform)
    {
        if (deviceTransform == null) return;

        _currentDevice = deviceTransform;
        CaptureDeviceBaselinePose();

        if (_lookAtCo != null) StopCoroutine(_lookAtCo);

        _lookAtCo = StartCoroutine(SmoothLookAtCoroutine(deviceTransform, lookAtBlendSeconds));
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

    private void CaptureDeviceBaselinePose()
    {
        if (_currentDevice == null) return;

        _deviceBaseLocalPos = _currentDevice.localPosition;
        _deviceBaseLocalRot = _currentDevice.localRotation;
        _deviceBaseCaptured = true;
    }

    public void PhoneCameraOpen()
    {
        if (_currentDevice == null) return;

        Transform pivot = (cameraPivot != null) ? cameraPivot : transform;

        _deviceLocalPosInPivot = pivot.InverseTransformPoint(_currentDevice.position);
        _deviceLocalRotInPivot = Quaternion.Inverse(pivot.rotation) * _currentDevice.rotation;

        _cameraModeActive = true;
        appliedMouseDelta = Vector2.zero;
    }

    public void PhoneCameraClosed()
    {
        // Ignore spurious CameraClosed events when we were never in camera mode.
        if (!_cameraModeActive)
            return;

        _cameraModeActive = false;
        appliedMouseDelta = Vector2.zero;

        if (_currentDevice != null && _deviceBaseCaptured)
        {
            _currentDevice.localPosition = _deviceBaseLocalPos;
            _currentDevice.localRotation = _deviceBaseLocalRot;
        }
    }

    private void FollowDeviceToView()
    {
        Transform pivot = (cameraPivot != null) ? cameraPivot : transform;

        Vector3 desiredPos = pivot.TransformPoint(_deviceLocalPosInPivot);
        Quaternion desiredRot = pivot.rotation * _deviceLocalRotInPivot;

        float k = 1f - Mathf.Exp(-deviceFollowSharpness * Time.deltaTime);

        _currentDevice.position = Vector3.Lerp(_currentDevice.position, desiredPos, k);
        _currentDevice.rotation = Quaternion.Slerp(_currentDevice.rotation, desiredRot, k);
    }

    // -----------------------------
    // Crouch / Crawl camera height
    // -----------------------------
    public void SetCrouch(bool crouched)
    {
        _isCrouched = crouched;
        if (!crouched) _isCrawling = false;
        ApplyResolvedHeightMode();
    }

    public void SetCrawl(bool crawling)
    {
        _isCrawling = crawling;
        if (crawling) _isCrouched = true;
        ApplyResolvedHeightMode();
    }

    private void ApplyResolvedHeightMode()
    {
        if (cameraPivot == null) cameraPivot = transform;

        HeightMode previous = _heightMode;

        if (_isCrawling) _heightMode = HeightMode.Crawl;
        else if (_isCrouched) _heightMode = HeightMode.Crouch;
        else _heightMode = HeightMode.Stand;

        float yOffset = 0f;
        if (_heightMode == HeightMode.Crouch) yOffset = crouchLocalYOffset;
        else if (_heightMode == HeightMode.Crawl) yOffset = crawlLocalYOffset;

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
    
    private static float NormalizeAngle180(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}