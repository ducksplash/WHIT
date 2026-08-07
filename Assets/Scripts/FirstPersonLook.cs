using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class FirstPersonLook : MonoBehaviour
{
    public Transform character;
    [SerializeField] InputActionReference lookAction;
    [SerializeField] InputActionReference flipThirdPersonCam;

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

    [Header("Seated Look Limits")]
    [Tooltip("How far down the player can look while seated.")]
    [SerializeField] private float seatedPitchClampMin = -25f;
    [Tooltip("How far up the player can look while seated.")]
    [SerializeField] private float seatedPitchClampMax = 25f;
    [Tooltip("How many degrees left or right the player can look from the seat facing direction.")]
    [SerializeField] private float seatedYawRange = 60f;

    [Header("Lying Look Limits")]
    [Tooltip("How far down the player can look while lying.")]
    [SerializeField] private float lyingPitchClampMin = -30f;
    [Tooltip("How far up the player can look while lying.")]
    [SerializeField] private float lyingPitchClampMax = 30f;

    private float _activePitchMin;
    private float _activePitchMax;

    private bool  _isSeated;
    private bool  _seatedLookAllowed;
    private float _seatedFacingYaw;

    private bool  _isLying;
    private bool  _lyingLookAllowed;
    private float _lyingFacingYaw;

    
    public Camera ThirdPersonCamera;

    [System.Serializable]
    private struct ThirdPersonCameraOffset
    {
        public Vector3 position;
        public Vector3 rotation;
    }


    [Header("Third Person Camera - Standing")]

    [SerializeField] private ThirdPersonCameraOffset standingDefaultOffset;
    [SerializeField] private ThirdPersonCameraOffset standingFrontOffset;
    [SerializeField] private ThirdPersonCameraOffset standingFrontLowOffset;


    [Header("Third Person Camera - Sitting")]

    [SerializeField] private ThirdPersonCameraOffset sittingDefaultOffset;
    [SerializeField] private ThirdPersonCameraOffset sittingFrontOffset;
    [SerializeField] private ThirdPersonCameraOffset sittingFrontLowOffset;


    [Header("Third Person Camera - Lying")]

    [SerializeField] private ThirdPersonCameraOffset lyingDefaultOffset;
    [SerializeField] private ThirdPersonCameraOffset lyingFrontOffset;
    [SerializeField] private ThirdPersonCameraOffset lyingFrontLowOffset;


    private enum ThirdPersonCamMode { Default, Front, FrontLow }

    private Vector3       _thirdPersonCamDefaultLocalPos;
    private Quaternion    _thirdPersonCamDefaultLocalRot;
    private Vector3       _thirdPersonCamDefaultLocalEuler;
    private bool          _thirdPersonCamCaptured;
    private ThirdPersonCamMode _thirdPersonCamMode = ThirdPersonCamMode.Default;

    public bool IsThirdPersonCameraFront => _thirdPersonCamMode == ThirdPersonCamMode.Front || _thirdPersonCamMode == ThirdPersonCamMode.FrontLow;
    
    [Header("Initial Look")]
    [SerializeField] private bool useSceneStartingRotation = true;
    [SerializeField] private float startingYaw   = 0f;
    [SerializeField] private float startingPitch = 0f;

    public bool bypassGM;

    private Vector3   _pivotStartLocalPos;
    private Coroutine _heightCo;

    private enum HeightMode { Stand, Crouch, Crawl }
    private HeightMode _heightMode = HeightMode.Stand;

    private bool _isCrouched;
    private bool _isCrawling;

    [Header("LookAt Device (Smooth)")]
    [SerializeField] private float lookAtBlendSeconds = 0.25f;
    [SerializeField] private AnimationCurve lookAtCurve = null;

    private Coroutine _lookAtCo;
    [Header("Phone Camera Look Tuning")]
    [Tooltip("Multiplier for pitch sensitivity when phone camera is open. Lower = slower/more controlled camera tilt.")]
    public float phoneCameraPitchMultiplier = 0.65f;

    private float _currentPitchMultiplier = 1f;
    [Header("Camera Mode Device Follow")]
    [SerializeField] private float deviceFollowSharpness = 60f;

    private Transform  _currentDevice;
    private bool       _cameraModeActive;

    private Vector3    _deviceLocalPosInPivot;
    private Quaternion _deviceLocalRotInPivot;

    private Vector3    _deviceBaseLocalPos;
    private Quaternion _deviceBaseLocalRot;
    private bool       _deviceBaseCaptured;
    public float CurrentPitch => currentMouseLook.y;
    
    public bool deviceCheckOverride;
    public DeviceHelperOutside deviceTypeSupplemental;

    public float CurrentYaw => currentMouseLook.x;

    private void Start()
    {
        if (cameraPivot == null) cameraPivot = transform;
        _pivotStartLocalPos = cameraPivot.localPosition;

        _activePitchMin = pitchClampMin;
        _activePitchMax = pitchClampMax;

        if (useSceneStartingRotation)
        {
            float yaw         = character != null ? character.localEulerAngles.y : transform.localEulerAngles.y;
            float pitchSource = cameraPivot != null ? cameraPivot.localEulerAngles.x : transform.localEulerAngles.x;
            float pitch       = NormalizeAngle180(pitchSource);

            currentMouseLook.x = yaw;
            currentMouseLook.y = Mathf.Clamp(-pitch, _activePitchMin, _activePitchMax);
        }
        else
        {
            currentMouseLook.x = startingYaw;
            currentMouseLook.y = Mathf.Clamp(startingPitch, _activePitchMin, _activePitchMax);
        }

        if (deviceCheckOverride)
            sensitivity = deviceTypeSupplemental.returnableSensitivity;
        else
            sensitivity = GameMaster.Instance.MouseSensitivity;

        if (ThirdPersonCamera != null)
        {
            _thirdPersonCamDefaultLocalPos   = ThirdPersonCamera.transform.localPosition;
            _thirdPersonCamDefaultLocalRot   = ThirdPersonCamera.transform.localRotation;
            _thirdPersonCamDefaultLocalEuler = _thirdPersonCamDefaultLocalRot.eulerAngles;
            _thirdPersonCamCaptured = true;
        }

        EventManager.OnResetThirdPersonState += ResetThirdPersonCameraMode;
        EventManager.OnStartComputer += LookAtThis;
        EventManager.OnStartPhone    += LookAtThis;
        EventManager.OnStartNotepad  += LookAtThis;
        EventManager.OnCameraOpen    += PhoneCameraOpen;
        EventManager.OnCameraClosed  += PhoneCameraClosed;

        
        flipThirdPersonCam.action.performed += OnFlipThirdPersonCam;
    }

    private void OnDestroy()
    {
        EventManager.OnResetThirdPersonState -= ResetThirdPersonCameraMode;
        EventManager.OnStartComputer -= LookAtThis;
        EventManager.OnStartPhone    -= LookAtThis;
        EventManager.OnStartNotepad  -= LookAtThis;
        EventManager.OnCameraOpen    -= PhoneCameraOpen;
        EventManager.OnCameraClosed  -= PhoneCameraClosed;
        flipThirdPersonCam.action.performed -= OnFlipThirdPersonCam;
    }

    
    
    private void OnFlipThirdPersonCam(InputAction.CallbackContext ctx)
    {
        CycleThirdPersonCameraMode();
    }

    private bool LookAllowedThisFrame()
    {
        if (bypassGM) return true;
        if (GameMaster.Instance.PauseManager.IsPaused) return false;

        // Lying with free look: bypass PLAYERBUSY entirely, same as seated.
        if (_isLying) return _lyingLookAllowed;

        // Seated with free look: bypass PLAYERBUSY entirely.
        if (_isSeated) return _seatedLookAllowed;

        // Normal: respect PLAYERBUSY unless MoveOverride is set.
        return !GameMaster.Instance.PLAYERBUSY || Player.Instance.MoveOverride;
    }

    void FixedUpdate()
    {
        if (!LookAllowedThisFrame()) return;
    }

    private void LateUpdate()
    {
        if (!LookAllowedThisFrame()) return;

        if (_lookLocked)
        {
            appliedMouseDelta = Vector2.zero;
            return;
        }

        Vector2 rawMouse         = lookAction.action.ReadValue<Vector2>();
        Vector2 smoothMouseDelta = Vector2.Scale(rawMouse, Vector2.one * sensitivity * smoothing);
        appliedMouseDelta        = Vector2.Lerp(appliedMouseDelta, smoothMouseDelta, 1f / smoothing);

        // === PHONE CAMERA PITCH MULTIPLIER ===
        float pitchMultiplier = IsPhoneCameraMode() ? phoneCameraPitchMultiplier : 1f;

        currentMouseLook.x += appliedMouseDelta.x;
        currentMouseLook.y += appliedMouseDelta.y * pitchMultiplier;   // Slow down only pitch input

        currentMouseLook.y = Mathf.Clamp(currentMouseLook.y, _activePitchMin, _activePitchMax);

        if (_isLying && _lyingLookAllowed)
        {
            // Lying: pitch only, yaw locked to the lying facing direction.
            currentMouseLook.x = _lyingFacingYaw;
        }
        else if (_isSeated && _seatedLookAllowed)
        {
            float yawDelta     = Mathf.DeltaAngle(_seatedFacingYaw, currentMouseLook.x);
            yawDelta           = Mathf.Clamp(yawDelta, -seatedYawRange, seatedYawRange);
            currentMouseLook.x = _seatedFacingYaw + yawDelta;
        }

        if (_cameraModeActive && _currentDevice != null) FollowDeviceToView();
    
        // APPLY ROTATION
        if (cameraPivot != null)
            cameraPivot.localRotation = Quaternion.AngleAxis(-currentMouseLook.y, Vector3.right);
        else
            transform.localRotation = Quaternion.AngleAxis(-currentMouseLook.y, Vector3.right);

        if (character != null)
            character.localRotation = Quaternion.AngleAxis(currentMouseLook.x, Vector3.up);

        ApplyThirdPersonCameraState();
    }

    public void ResetThirdPersonCameraMode()
    {
        _thirdPersonCamMode = ThirdPersonCamMode.Default;
    }

    private void ApplyThirdPersonCameraState()
    {
        if (ThirdPersonCamera == null || !_thirdPersonCamCaptured) return;

        Transform cam = ThirdPersonCamera.transform;

        Vector3 position;
        Quaternion rotation;

        // =========================================================
        // BASE CAMERA MODE
        // =========================================================

        switch (_thirdPersonCamMode)
        {
            case ThirdPersonCamMode.Default:
            {
                position = _thirdPersonCamDefaultLocalPos;
                rotation = _thirdPersonCamDefaultLocalRot;
                break;
            }

            case ThirdPersonCamMode.Front:
            {
                position = _thirdPersonCamDefaultLocalPos;
                position.z = Mathf.Abs(_thirdPersonCamDefaultLocalPos.z);

                rotation = Quaternion.Euler(
                    _thirdPersonCamDefaultLocalEuler.x,
                    _thirdPersonCamDefaultLocalEuler.y + 180f,
                    _thirdPersonCamDefaultLocalEuler.z);

                break;
            }

            case ThirdPersonCamMode.FrontLow:
            {
                position = _thirdPersonCamDefaultLocalPos;
                position.z = Mathf.Abs(_thirdPersonCamDefaultLocalPos.z);

                rotation =
                    _thirdPersonCamDefaultLocalRot *
                    Quaternion.Euler(0f, 180f, 0f);

                break;
            }

            default:
            {
                position = _thirdPersonCamDefaultLocalPos;
                rotation = _thirdPersonCamDefaultLocalRot;
                break;
            }
        }


        // =========================================================
        // DETERMINE PLAYER STATE
        // =========================================================

        ThirdPersonCameraOffset offset;

        if (_isLying)
        {
            // Lying
            switch (_thirdPersonCamMode)
            {
                case ThirdPersonCamMode.Default:
                    offset = lyingDefaultOffset;
                    break;

                case ThirdPersonCamMode.Front:
                    offset = lyingFrontOffset;
                    break;

                case ThirdPersonCamMode.FrontLow:
                    offset = lyingFrontLowOffset;
                    break;

                default:
                    offset = lyingDefaultOffset;
                    break;
            }
        }
        else if (_isSeated)
        {
            // Sitting
            switch (_thirdPersonCamMode)
            {
                case ThirdPersonCamMode.Default:
                    offset = sittingDefaultOffset;
                    break;

                case ThirdPersonCamMode.Front:
                    offset = sittingFrontOffset;
                    break;

                case ThirdPersonCamMode.FrontLow:
                    offset = sittingFrontLowOffset;
                    break;

                default:
                    offset = sittingDefaultOffset;
                    break;
            }
        }
        else
        {
            // Standing
            switch (_thirdPersonCamMode)
            {
                case ThirdPersonCamMode.Default:
                    offset = standingDefaultOffset;
                    break;

                case ThirdPersonCamMode.Front:
                    offset = standingFrontOffset;
                    break;

                case ThirdPersonCamMode.FrontLow:
                    offset = standingFrontLowOffset;
                    break;

                default:
                    offset = standingDefaultOffset;
                    break;
            }
        }


        // =========================================================
        // APPLY STATE/MODE OFFSET
        // =========================================================

        position += offset.position;

        rotation *= Quaternion.Euler(offset.rotation);

        cam.localPosition = position;
        cam.localRotation = rotation;
    }

    public void CycleThirdPersonCameraMode()
    {
        if (ThirdPersonCamera == null) return;

        switch (_thirdPersonCamMode)
        {
            case ThirdPersonCamMode.Default:
                _thirdPersonCamMode = ThirdPersonCamMode.Front;
                break;
            case ThirdPersonCamMode.Front:
                _thirdPersonCamMode = ThirdPersonCamMode.FrontLow;
                break;
            case ThirdPersonCamMode.FrontLow:
                _thirdPersonCamMode = ThirdPersonCamMode.Default;
                break;
        }
    }


    private bool IsPhoneCameraMode()
    {
        return Player.Instance?.PlayerPhone?.CameraOpen == true;
    }

    public void LockLook(bool locked)
    {
        _lookLocked = locked;
        if (locked) appliedMouseDelta = Vector2.zero;
    }
    
    public void SetSeated(bool seated, float facingYaw = 0f, bool allowLook = true)
    {
        _isSeated          = seated;
        _seatedLookAllowed = seated && allowLook;

        if (seated)
        {
            _seatedFacingYaw = facingYaw;
            _activePitchMin  = seatedPitchClampMin;
            _activePitchMax  = seatedPitchClampMax;

            if (cameraPivot != null)
            {
                float rawPitch     = NormalizeAngle180(cameraPivot.localEulerAngles.x);
                currentMouseLook.y = Mathf.Clamp(-rawPitch, _activePitchMin, _activePitchMax);
            }
            else
            {
                currentMouseLook.y = Mathf.Clamp(currentMouseLook.y, _activePitchMin, _activePitchMax);
            }

            currentMouseLook.x = facingYaw;

            // Zero delta so no leftover mouse movement fires on the first live frame
            appliedMouseDelta = Vector2.zero;

            if (allowLook) LockLook(false);
        }
        else
        {
            _activePitchMin   = pitchClampMin;
            _activePitchMax   = pitchClampMax;
            appliedMouseDelta = Vector2.zero;
            LockLook(false);
        }
    }


    public void SetLying(bool lying, float facingYaw = 0f, bool allowLook = true)
    {
        _isLying          = lying;
        _lyingLookAllowed = lying && allowLook;

        if (lying)
        {
            _lyingFacingYaw = facingYaw;
            _activePitchMin = lyingPitchClampMin;
            _activePitchMax = lyingPitchClampMax;

            if (cameraPivot != null)
            {
                float rawPitch     = NormalizeAngle180(cameraPivot.localEulerAngles.x);
                currentMouseLook.y = Mathf.Clamp(-rawPitch, _activePitchMin, _activePitchMax);
            }
            else
            {
                currentMouseLook.y = Mathf.Clamp(currentMouseLook.y, _activePitchMin, _activePitchMax);
            }

            currentMouseLook.x = facingYaw;

            appliedMouseDelta = Vector2.zero;

            if (allowLook) LockLook(false);
        }
        else
        {
            _activePitchMin   = pitchClampMin;
            _activePitchMax   = pitchClampMax;
            appliedMouseDelta = Vector2.zero;
            LockLook(false);
        }
    }

    // ─────────────────────────────────────────────────────────────────────

    public void SetPlayerRotation(Vector2 rotation)
    {
        currentMouseLook = rotation;
    }

    public void AimAssistLock(float seconds)
    {
        seconds = Mathf.Max(0f, seconds);
        if (_lockRoutine != null) StopCoroutine(_lockRoutine);
        _lockRoutine = StartCoroutine(AimAssistLockRoutine(seconds));
    }

    private IEnumerator AimAssistLockRoutine(float seconds)
    {
        _lookLocked       = true;
        appliedMouseDelta = Vector2.zero;

        yield return new WaitForSecondsRealtime(seconds);

        _lookLocked       = false;
        appliedMouseDelta = Vector2.zero;
        _lockRoutine      = null;
    }

    public void SnapYawTowardWorldPoint(Vector3 worldPoint)
    {
        Vector3 from = character != null ? character.position : transform.position;
        Vector3 to   = worldPoint - from;
        to.y = 0f;
        if (to.sqrMagnitude < 0.000001f) return;

        currentMouseLook.x = Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg;
        appliedMouseDelta  = Vector2.zero;
    }

    public void LookAtThis(Transform deviceTransform)
    {
        if (deviceTransform == null) return;

        _currentDevice = deviceTransform;
        CaptureDeviceBaselinePose();

        if (_lookAtCo != null) StopCoroutine(_lookAtCo);
        _lookAtCo = StartCoroutine(SmoothLookAtCoroutine(deviceTransform, lookAtBlendSeconds));
    }

    private IEnumerator SmoothLookAtCoroutine(Transform target, float seconds)
    {
        _lookLocked       = true;
        appliedMouseDelta = Vector2.zero;

        Quaternion startRot = cameraPivot != null ? cameraPivot.rotation : transform.rotation;
        Vector3    origin   = cameraPivot != null ? cameraPivot.position : transform.position;
        Vector3    dir0     = target.position - origin;

        Quaternion endRot = dir0.sqrMagnitude > 0.000001f
            ? Quaternion.LookRotation(dir0.normalized, Vector3.up)
            : startRot;

        if (seconds <= 0f)
        {
            if (cameraPivot != null) cameraPivot.rotation = endRot;
            else                     transform.rotation   = endRot;
            _lookLocked = false;
            _lookAtCo   = null;
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
            else                     transform.rotation   = r;
            yield return null;
        }

        if (cameraPivot != null) cameraPivot.rotation = endRot;
        else                     transform.rotation   = endRot;

        _lookLocked = false;
        _lookAtCo   = null;
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

        // ignore but leave intact
        // Transform pivot        = cameraPivot != null ? cameraPivot : transform;
        // _deviceLocalPosInPivot = pivot.InverseTransformPoint(_currentDevice.position);
        // _deviceLocalRotInPivot = Quaternion.Inverse(pivot.rotation) * _currentDevice.rotation;
        // _cameraModeActive      = true;
        // appliedMouseDelta      = Vector2.zero;
    }

    public void PhoneCameraClosed()
    {
        if (!_cameraModeActive) return;

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
        Transform  pivot      = cameraPivot != null ? cameraPivot : transform;
        Vector3    desiredPos = pivot.TransformPoint(_deviceLocalPosInPivot);
        Quaternion desiredRot = pivot.rotation * _deviceLocalRotInPivot;
        float      k          = 1f - Mathf.Exp(-deviceFollowSharpness * Time.deltaTime);

        _currentDevice.position = Vector3.Lerp(_currentDevice.position, desiredPos, k);
        _currentDevice.rotation = Quaternion.Slerp(_currentDevice.rotation, desiredRot, k);
    }

    // ── Crouch / Crawl camera height ──────────────────────────────────────
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

        if      (_isCrawling) _heightMode = HeightMode.Crawl;
        else if (_isCrouched) _heightMode = HeightMode.Crouch;
        else                  _heightMode = HeightMode.Stand;

        float yOffset = 0f;
        if      (_heightMode == HeightMode.Crouch) yOffset = crouchLocalYOffset;
        else if (_heightMode == HeightMode.Crawl)  yOffset = crawlLocalYOffset;

        float duration =
            (previous == HeightMode.Crawl || _heightMode == HeightMode.Crawl)
                ? crawlBlendSeconds
                : crouchBlendSeconds;

        Vector3 target = _pivotStartLocalPos + new Vector3(0f, yOffset, 0f);

        if (_heightCo != null) StopCoroutine(_heightCo);
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
            cameraPivot.localPosition = Vector3.Lerp(start, targetLocalPos, Mathf.Clamp01(t / seconds));
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