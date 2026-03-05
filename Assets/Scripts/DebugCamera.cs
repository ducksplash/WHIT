using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DebugCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform character;
    [SerializeField] private InputActionReference lookAction;

    [Header("Inputs")]
    [Tooltip("Hold this to lock cursor + rotate (eg: Right Mouse Button). Only works in Orbit mode.")]
    [SerializeField] private InputActionReference holdClick;

    [Tooltip("Click this (while NOT holding) to pick a new target (eg: Left Mouse Button).")]
    [SerializeField] private InputActionReference pickClick;

    [Tooltip("Press this to abandon Target and re-enter free cam")]
    [SerializeField] private InputActionReference escapeButton;

    [Header("Fly Cam Inputs")]
    [Tooltip("WASD / Left Stick (Vector2): X = strafe, Y = forward/back.")]
    [SerializeField] private InputActionReference flyMoveAction;

    [Header("Fly Cam Tuning")]
    [SerializeField] private float flyMoveSpeed = 6f;
    [SerializeField] private float flyLookSensitivity = 0.08f; // degrees per mouse unit (tune to taste)
    [SerializeField] private float flyLookSmoothing = 6f;
    [SerializeField] private bool flyKeepLevelMovement = true; // movement stays on ground plane

    [Header("Fly Cam Speed Modifier")]
    [SerializeField] private InputActionReference flyFastHold; // eg: Left Shift (Button)
    [SerializeField] private float flyFastMultiplier = 3f;

    private NavMeshAgent _agent;
    
    private bool _flyFastHeld;
    
    [Header("Target Picking")]
    [SerializeField] private string targetLayerName = "CameraTarget";
    [SerializeField] private float pickDistance = 500f;

    public enum PickFrom { MousePosition, ScreenCenter, CameraForward }
    [Tooltip("MousePosition = ray from cursor. ScreenCenter = ray where camera is looking. CameraForward = ray from transform.forward.")]
    [SerializeField] private PickFrom pickFrom = PickFrom.MousePosition;

    [Tooltip("If true, choose the ROOT of the clicked object (useful if you click child colliders).")]
    [SerializeField] private bool targetRootTransform = true;

    [Tooltip("If true and collider has Rigidbody, use rigidbody.transform as target.")]
    [SerializeField] private bool requireRigidbodyOwner = false;

    [Header("Orbit Center")]
    [Tooltip("Fallback center offset if we can't find a collider center.")]
    public Vector3 centerOffsetFallback = new Vector3(0f, 1.6f, 0f);

    [Tooltip("If true, when setting/retargeting we will orbit around collider bounds center (recommended).")]
    [SerializeField] private bool orbitAroundColliderCenter = true;

    [Header("Orbit")]
    public float distance = 3.0f;
    public float minDistance = 0.5f;
    public float maxDistance = 12f;

    [Header("Look Tuning")]
    public float sensitivity = 0.01f;
    public float smoothing = 5f;

    [Header("Pitch Limits")]
    [SerializeField] private float pitchClampMin = -60f;
    [SerializeField] private float pitchClampMax = 60f;

    [Header("Pan (Test / Cinematic)")]
    [Tooltip("Pan takes this many seconds from start to end.")]
    [SerializeField] private float panDurationSeconds = 3f;

    [Tooltip("Pan goes from -extent to +extent along the chosen axis.")]
    [SerializeField] private float panExtent = 10f;

    [Tooltip("While panning, camera stays this far in FRONT of target (positive means in front). If 0, uses current orbit distance.")]
    [SerializeField] private float panFrontDistance = 0f;

    [Tooltip("Scrub time for pan preview/testing (0..1).")]
    [Range(0f, 1f)]
    [SerializeField] private float panTime01 = 0f;

    [Header("FollowCam")]
    [SerializeField] private bool followEnabled = false;

    [Tooltip("Distance behind target (in movement direction).")]
    [SerializeField] private float followDistance = 4f;

    [Tooltip("Height above target center.")]
    [SerializeField] private float followHeight = 1.5f;

    [Tooltip("Optional sideways offset relative to movement direction.")]
    [SerializeField] private float followSideOffset = 0f;

    [Tooltip("Minimum planar speed to consider the target 'moving'.")]
    [SerializeField] private float followMinMoveSpeed = 0.05f;

    [Header("Follow Laziness")]
    [Tooltip("How long (seconds) to smooth the movement direction (bigger = less jitter, lazier turns).")]
    [SerializeField] private float followDirSmoothTime = 0.25f;

    [Header("Zoom (Orbit + Follow)")]
    [SerializeField] private InputActionReference zoomAction; // typically Mouse Scroll (Vector2)
    [SerializeField] private float zoomSpeed = 0.25f;         // how strong scroll is
    [SerializeField] private bool zoomAffectsFOV = false;     // if true, also drives ZoomNaura
    [SerializeField] private ZoomNaura zoomNaura;             // optional reference

    [Tooltip("How long (seconds) for position to catch up (bigger = lazier position).")]
    [SerializeField] private float followPosSmoothTime = 0.20f;

    [Tooltip("How long (seconds) for rotation to catch up (bigger = lazier rotation).")]
    [SerializeField] private float followRotSmoothTime = 0.35f;

    [Tooltip("Clamp how fast the camera position can change (units/sec). 0 = unlimited.")]
    [SerializeField] private float followMaxPosSpeed = 0f;

    [Header("Keep Level (No Tilt)")]
    [Tooltip("If true, follow camera rotation is Y-only (no pitch/roll).")]
    [SerializeField] private bool followKeepLevel = true;

    [Tooltip("If true, orbit camera also stays level (no pitch/roll).")]
    [SerializeField] private bool orbitKeepLevel = false;

    [Header("Debug")]
    [SerializeField] private bool debugRay = true;
    [SerializeField] private float debugRaySeconds = 1.0f;
    [SerializeField] private bool debugLogs = false;

    // -----------------------
    // Runtime
    // -----------------------

    private enum CamMode { Fly, Orbit, PanHorizontal, PanVertical, Follow }
    [SerializeField] private CamMode _mode = CamMode.Fly;

    private bool _isHoldingRotate;
    private Vector2 _orbitAngles;   // x=yaw, y=pitch
    private Vector2 _appliedDelta;

    private Camera _cam;
    private int _targetLayer;
    private int _targetLayerMask;

    // Orbit center expressed in LOCAL space of `character`
    private Vector3 _centerOffsetLocal;

    // Live center collider (so center updates if collider is on animated/moving child)
    private Collider _centerCollider;

    // Pan state
    private Coroutine _panCo;

    // Saved orbit view to restore after pan / follow
    private Vector2 _savedOrbitAngles;
    private float _savedOrbitDistance;
    private Vector3 _savedCenterOffsetLocal;

    // Follow toggle watcher
    private bool _prevFollowEnabled;

    // Movement-direction tracking
    private Vector3 _prevCenter;
    private Vector3 _lastMoveDir = Vector3.forward;
    private Rigidbody _rb;
    private CharacterController _cc;

    // Smoothed direction state (filters jitter)
    private Vector3 _smoothedMoveDir = Vector3.forward;
    private Vector3 _smoothedMoveDirVel; // SmoothDamp ref

    // Smooth position + yaw
    private Vector3 _followPosVel;       // SmoothDamp ref
    private float _followYawVel;         // SmoothDampAngle ref

    // Fly cam state
    private Vector2 _flyAngles;          // x=yaw, y=pitch
    private Vector2 _flyLookApplied;     // smoothing
    private bool _flyCaptured = true;

    // -----------------------
    // Unity
    // -----------------------

    void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam == null) _cam = Camera.main;

        _targetLayer = LayerMask.NameToLayer(targetLayerName);
        if (_targetLayer < 0)
        {
            Debug.LogWarning($"{nameof(DebugCamera)}: Layer '{targetLayerName}' not found. Create it in Tags & Layers.");
            _targetLayerMask = 0;
        }
        else
        {
            _targetLayerMask = 1 << _targetLayer;
        }

        RefreshCenterOffsetLocal(null);

        _prevFollowEnabled = followEnabled;

        // Start in Fly if no character
        if (character == null)
            EnterFlyMode();
        else
            EnterTargetModeAfterSet(character);
    }


    private void Start()
    {
        flyLookSensitivity = GameMaster.Instance.MouseSensitivity;
        sensitivity = GameMaster.Instance.MouseSensitivity;
    }

    void OnEnable()
    {
        if (lookAction != null) lookAction.action.Enable();

        if (holdClick != null)
        {
            holdClick.action.Enable();
            holdClick.action.performed += OnHoldStart;
            holdClick.action.canceled += OnHoldEnd;    
            flyFastHold.action.Enable();
            flyFastHold.action.performed += OnFlyFastStart;
            flyFastHold.action.canceled += OnFlyFastEnd;
        }

        if (pickClick != null)
        {
            pickClick.action.Enable();
            pickClick.action.performed += OnPick;
        }

        if (escapeButton != null)
        {
            escapeButton.action.Enable();
            escapeButton.action.performed += OnEscape;
        }

        if (zoomAction != null) zoomAction.action.Enable();

        if (flyMoveAction != null) flyMoveAction.action.Enable();
    }

    void OnDisable()
    {
        if (lookAction != null) lookAction.action.Disable();

        if (holdClick != null)
        {   
            flyFastHold.action.performed -= OnFlyFastStart;
            flyFastHold.action.canceled -= OnFlyFastEnd;
            flyFastHold.action.Disable();
            holdClick.action.performed -= OnHoldStart;
            holdClick.action.canceled -= OnHoldEnd;
            holdClick.action.Disable();
        }

        if (pickClick != null)
        {
            pickClick.action.performed -= OnPick;
            pickClick.action.Disable();
        }

        if (escapeButton != null)
        {
            escapeButton.action.performed -= OnEscape;
            escapeButton.action.Disable();
        }

        if (zoomAction != null) zoomAction.action.Disable();
        if (flyMoveAction != null) flyMoveAction.action.Disable();

        StopPanInternal();
        SetCaptured(false);
    }

    private void OnFlyFastStart(InputAction.CallbackContext _)
    {
        _flyFastHeld = true;
    }

    private void OnFlyFastEnd(InputAction.CallbackContext _)
    {
        _flyFastHeld = false;
    }
    
    void Update()
    {
        // Escape clears target and returns to Fly
        // (handled by callback too; this is just safety if you disable callbacks)
        if (character == null && _mode != CamMode.Fly)
            EnterFlyMode();

        if (followEnabled != _prevFollowEnabled)
        {
            SetFollowCam(followEnabled);
            _prevFollowEnabled = followEnabled;
        }

        HandleZoomInput();


        if (_mode == CamMode.Fly)
        {
            ApplyFlyLook();  // now only rotates while RMB held
            ApplyFlyMove();
            return;
        }

        // Orbit rotate only while holding (existing behaviour)
        if (_mode != CamMode.Orbit) return;
        if (!_isHoldingRotate) return;
        if (lookAction == null || character == null) return;

        Vector2 raw = lookAction.action.ReadValue<Vector2>();

        float smooth = Mathf.Max(1f, smoothing);
        Vector2 targetDelta = raw * (sensitivity * smooth);
        _appliedDelta = Vector2.Lerp(_appliedDelta, targetDelta, 1f / smooth);

        _orbitAngles += _appliedDelta;
        _orbitAngles.y = Mathf.Clamp(_orbitAngles.y, pitchClampMin, pitchClampMax);
    }

    void LateUpdate()
    {
        switch (_mode)
        {
            case CamMode.Fly:
                // Fly already applied in Update
                break;

            case CamMode.Orbit:
                if (character != null) ApplyOrbit();
                break;

            case CamMode.PanHorizontal:
                if (character != null) PanHorizontal(panTime01);
                break;

            case CamMode.PanVertical:
                if (character != null) PanVertical(panTime01);
                break;

            case CamMode.Follow:
                if (character != null) ApplyFollow();
                break;
        }
    }

    // -----------------------
    // Fly Cam
    // -----------------------

    void EnterFlyMode()
    {
        StopPanInternal();

        character = null;
        _centerCollider = null;

        _isHoldingRotate = false;
        _appliedDelta = Vector2.zero;

        _mode = CamMode.Fly;

        // ✅ Robust: seed from current forward (not euler x/y directly)
        Vector3 fwd = transform.forward;
        fwd.Normalize();

        float yaw = Mathf.Atan2(fwd.x, fwd.z) * Mathf.Rad2Deg;
        float pitch = -Mathf.Asin(Mathf.Clamp(fwd.y, -1f, 1f)) * Mathf.Rad2Deg;

        pitch = Mathf.Clamp(pitch, pitchClampMin, pitchClampMax);

        _flyAngles = new Vector2(yaw, pitch);
        _flyLookApplied = Vector2.zero;

        // ✅ In Fly mode: cursor should be visible by default (so you can click to pick)
        _flyCaptured = false;
        SetCaptured(false);

        // Apply rotation immediately so it doesn't "snap" next frame
        transform.rotation = Quaternion.Euler(_flyAngles.y, _flyAngles.x, 0f);
    }

    void ApplyFlyLook()
    {
        if (lookAction == null) return;

        // ✅ Only rotate in Fly while holding RMB (so mouse can be used to click when not holding)
        if (!_flyCaptured) return;

        Vector2 raw = lookAction.action.ReadValue<Vector2>();

        float smooth = Mathf.Max(1f, flyLookSmoothing);
        Vector2 targetDelta = raw * (flyLookSensitivity * smooth);
        _flyLookApplied = Vector2.Lerp(_flyLookApplied, targetDelta, 1f / smooth);

        _flyAngles.x += _flyLookApplied.x;   // yaw
        _flyAngles.y -= _flyLookApplied.y;   // pitch

        _flyAngles.y = Mathf.Clamp(_flyAngles.y, pitchClampMin, pitchClampMax);

        transform.rotation = Quaternion.Euler(_flyAngles.y, _flyAngles.x, 0f);
    }

    void ApplyFlyMove()
    {
        if (flyMoveAction == null) return;

        Vector2 move = flyMoveAction.action.ReadValue<Vector2>(); // X=strafe, Y=forward
        if (move.sqrMagnitude < 0.000001f) return;

        float dt = Time.deltaTime;
        float speed = flyMoveSpeed * (_flyFastHeld ? Mathf.Max(1f, flyFastMultiplier) : 1f);

        Vector3 fwd = transform.forward;
        Vector3 right = transform.right;

        // OPTIONAL: if you still want a "keep level" option, keep this block.
        // For your requested behavior, set flyKeepLevelMovement = false.
        if (flyKeepLevelMovement)
        {
            fwd.y = 0f;
            right.y = 0f;
            fwd.Normalize();
            right.Normalize();
        }

        // Normalize input so diagonals aren't faster
        Vector2 input = move.normalized;

        Vector3 wishDir = (right * input.x + fwd * input.y);
        transform.position += wishDir * (speed * dt);
    }

    // -----------------------
    // Zoom (Orbit + Follow)
    // -----------------------

    private void HandleZoomInput()
    {
        if (zoomAction == null) return;

        Vector2 scroll = zoomAction.action.ReadValue<Vector2>();
        float scrollY = scroll.y;
        if (Mathf.Abs(scrollY) < 0.0001f) return;

        float delta = scrollY * zoomSpeed * Time.unscaledDeltaTime;

        distance = Mathf.Clamp(distance - delta, minDistance, maxDistance);
        followDistance = Mathf.Clamp(followDistance - delta, minDistance, maxDistance);

        if (zoomAffectsFOV && zoomNaura != null)
        {
            float t = Mathf.InverseLerp(maxDistance, minDistance, distance); // near = 1
            zoomNaura.zoomAmount = Mathf.Clamp01(t);
            zoomNaura.AutoZoomToAmount(zoomNaura.zoomAmount);
        }
    }

    // -----------------------
    // Hold-to-rotate (Orbit mode only)
    // -----------------------

    private void OnHoldStart(InputAction.CallbackContext _)
    {
        if (_mode == CamMode.Orbit)
        {
            _isHoldingRotate = true;
            _appliedDelta = Vector2.zero;
            SetCaptured(true);
            return;
        }

        if (_mode == CamMode.Fly)
        {
            _flyCaptured = true;
            _flyLookApplied = Vector2.zero;
            SetCaptured(true);
            return;
        }
    }

    private void OnHoldEnd(InputAction.CallbackContext _)
    {
        if (_mode == CamMode.Orbit)
        {
            _isHoldingRotate = false;
            _appliedDelta = Vector2.zero;
            SetCaptured(false);
            return;
        }

        if (_mode == CamMode.Fly)
        {
            _flyCaptured = false;
            _flyLookApplied = Vector2.zero;
            SetCaptured(false);
            return;
        }
    }

    private void SetCaptured(bool captured)
    {
        Cursor.lockState = captured ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !captured;
    }

    // -----------------------
    // Escape: clear target
    // -----------------------

    private void OnEscape(InputAction.CallbackContext _)
    {
        // Abandon target and re-enter free cam
        EnterFlyMode();
    }

    // -----------------------
    // Picking (only when NOT holding)
    // -----------------------

    private void OnPick(InputAction.CallbackContext _)
    {
        // In fly mode, we still allow picking if you click the pick button.
        if (_isHoldingRotate) return;
        TryPickTargetOnce();
    }

    private void TryPickTargetOnce()
    {
        if (_cam == null) return;
        if (_targetLayerMask == 0) return;

        Ray ray = BuildPickRay();

        if (debugRay)
            Debug.DrawRay(ray.origin, ray.direction * pickDistance, Color.yellow, debugRaySeconds);

        RaycastHit[] hits = Physics.RaycastAll(ray, pickDistance, ~0, QueryTriggerInteraction.Collide);
        if (hits == null || hits.Length == 0)
        {
            if (debugLogs) Debug.Log($"{nameof(DebugCamera)}: Pick MISS (no hits).");
            return;
        }

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            Transform resolved = ResolvePickedTransform(h);
            if (resolved == null) continue;

            SetTarget(resolved, h.collider);
            return;
        }
    }

    private Ray BuildPickRay()
    {
        switch (pickFrom)
        {
            case PickFrom.ScreenCenter:
            {
                Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                return _cam.ScreenPointToRay(center);
            }
            case PickFrom.CameraForward:
                return new Ray(transform.position, transform.forward);

            case PickFrom.MousePosition:
            default:
            {
                if (Mouse.current == null) return new Ray(transform.position, transform.forward);
                Vector2 screenPos = Mouse.current.position.ReadValue();
                return _cam.ScreenPointToRay(screenPos);
            }
        }
    }

    private Transform ResolvePickedTransform(RaycastHit hit)
    {
        if (hit.collider == null) return null;

        if (requireRigidbodyOwner && hit.rigidbody != null)
            return FindCameraTargetInParents(hit.rigidbody.transform);

        Transform t = FindCameraTargetInParents(hit.collider.transform);
        if (t == null) return null;

        if (targetRootTransform) t = t.root;
        return t;
    }

    private Transform FindCameraTargetInParents(Transform start)
    {
        Transform t = start;
        while (t != null)
        {
            if (t.gameObject.layer == _targetLayer)
                return t;
            t = t.parent;
        }
        return null;
    }

    // -----------------------
    // Target / Orbit center
    // -----------------------

    public void SetTarget(Transform newTarget) => SetTarget(newTarget, null);

    public void SetTarget(Transform newTarget, Collider pickedCollider)
    {
        if (newTarget == null) return;

        character = newTarget;
        EnterTargetModeAfterSet(newTarget, pickedCollider);
    }

    void EnterTargetModeAfterSet(Transform newTarget, Collider pickedCollider = null)
    {
        RefreshCenterOffsetLocal(pickedCollider);

        SyncAnglesFromCurrentPose();
        CacheMovementSources();

        _prevCenter = GetOrbitCenterWorld();
        _lastMoveDir = FlattenOnGround(newTarget.forward);
        _smoothedMoveDir = _lastMoveDir;
        _smoothedMoveDirVel = Vector3.zero;

        // When we have a target, return to Orbit/Follow
        _mode = followEnabled ? CamMode.Follow : CamMode.Orbit;

        // In orbit, cursor is only captured while holding rotate.
        _isHoldingRotate = false;
        _appliedDelta = Vector2.zero;
        SetCaptured(false);

        if (_mode == CamMode.Follow)
            SeedFollowLazyState();
    }

    private void RefreshCenterOffsetLocal(Collider pickedCollider)
    {
        _centerCollider = null;

        if (character == null)
        {
            _centerOffsetLocal = centerOffsetFallback;
            return;
        }

        if (!orbitAroundColliderCenter)
        {
            _centerOffsetLocal = centerOffsetFallback;
            return;
        }

        if (pickedCollider != null)
        {
            _centerCollider = pickedCollider;
            Vector3 worldCenter = pickedCollider.bounds.center;
            _centerOffsetLocal = character.InverseTransformPoint(worldCenter);
            return;
        }

        Collider c = character.GetComponentInChildren<Collider>();
        if (c != null)
        {
            _centerCollider = c;
            Vector3 worldCenter = c.bounds.center;
            _centerOffsetLocal = character.InverseTransformPoint(worldCenter);
            return;
        }

        _centerOffsetLocal = centerOffsetFallback;
    }

    private Vector3 GetOrbitCenterWorld()
    {
        if (character == null) return transform.position;

        if (orbitAroundColliderCenter && _centerCollider != null)
            return _centerCollider.bounds.center;

        return character.TransformPoint(_centerOffsetLocal);
    }

    private void CacheMovementSources()
    {
        _rb = null;
        _cc = null;
        _agent = null;

        if (character == null) return;

        _rb = character.GetComponentInParent<Rigidbody>();
        if (_rb == null) _rb = character.GetComponent<Rigidbody>();

        // Prefer NavMeshAgent for NPCs
        _agent = character.GetComponentInParent<NavMeshAgent>();
        if (_agent == null) _agent = character.GetComponent<NavMeshAgent>();

        // Optional: keep CC support if you ever target a player character
        _cc = character.GetComponentInParent<CharacterController>();
        if (_cc == null) _cc = character.GetComponent<CharacterController>();
    }

    // -----------------------
    // Orbit
    // -----------------------

    private void ApplyOrbit()
    {
        if (character == null) return;

        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        Vector3 center = GetOrbitCenterWorld();
        Quaternion orbitRot = Quaternion.Euler(_orbitAngles.y, _orbitAngles.x, 0f);

        transform.position = center + (orbitRot * (Vector3.back * distance));

        if (!orbitKeepLevel)
        {
            transform.rotation = Quaternion.LookRotation((center - transform.position).normalized, Vector3.up);
        }
        else
        {
            Vector3 to = center - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude < 0.000001f) to = transform.forward;
            transform.rotation = Quaternion.LookRotation(to.normalized, Vector3.up);
        }
    }

    private static float NormalizeAngle360(float a)
    {
        a %= 360f;
        if (a < 0f) a += 360f;
        return a;
    }

    private static float ClosestYawToCurrent(float currentYaw, float candidateYaw)
    {
        float delta = Mathf.DeltaAngle(currentYaw, candidateYaw);
        return currentYaw + delta;
    }

    public void SyncAnglesFromCurrentPose()
    {
        if (character == null) return;

        Vector3 center = GetOrbitCenterWorld();
        Vector3 fromCenter = transform.position - center;

        if (fromCenter.sqrMagnitude < 0.000001f)
            return;

        distance = Mathf.Clamp(fromCenter.magnitude, minDistance, maxDistance);

        Vector3 dir = fromCenter.normalized;

        Vector3 flat = new Vector3(dir.x, 0f, dir.z);
        if (flat.sqrMagnitude < 0.000001f)
            flat = Vector3.forward;

        float yawCandidate = Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg + 180f;
        yawCandidate = NormalizeAngle360(yawCandidate);
        float yaw = ClosestYawToCurrent(_orbitAngles.x, yawCandidate);

        float pitch = -Mathf.Asin(Mathf.Clamp(dir.y, -1f, 1f)) * Mathf.Rad2Deg;
        pitch = Mathf.Clamp(pitch, pitchClampMin, pitchClampMax);

        _orbitAngles = new Vector2(yaw, pitch);
        _appliedDelta = Vector2.zero;
    }

    // -----------------------
    // Pan API (unchanged)
    // -----------------------

    public void PanHorizontal(float t01)
    {
        if (character == null) return;
        t01 = Mathf.Clamp01(t01);

        Vector3 center = GetOrbitCenterWorld();
        Transform t = character;

        float front = (panFrontDistance > 0.0001f) ? panFrontDistance : Mathf.Clamp(distance, minDistance, maxDistance);

        Vector3 axis = t.right;
        Vector3 basePos = center - (t.forward * front);

        Vector3 start = basePos + axis * (-panExtent);
        Vector3 end = basePos + axis * (panExtent);

        transform.position = Vector3.Lerp(start, end, t01);
        transform.rotation = Quaternion.LookRotation((center - transform.position).normalized, Vector3.up);
    }

    public void PanVertical(float t01)
    {
        if (character == null) return;
        t01 = Mathf.Clamp01(t01);

        Vector3 center = GetOrbitCenterWorld();
        Transform t = character;

        float front = (panFrontDistance > 0.0001f) ? panFrontDistance : Mathf.Clamp(distance, minDistance, maxDistance);

        Vector3 axis = Vector3.up;
        Vector3 basePos = center - (t.forward * front);

        Vector3 start = basePos + axis * (-panExtent);
        Vector3 end = basePos + axis * (panExtent);

        transform.position = Vector3.Lerp(start, end, t01);
        transform.rotation = Quaternion.LookRotation((center - transform.position).normalized, Vector3.up);
    }

    public void PlayPanHorizontal()
    {
        if (character == null) return;
        StartPanInternal(CamMode.PanHorizontal);
    }

    public void PlayPanVertical()
    {
        if (character == null) return;
        StartPanInternal(CamMode.PanVertical);
    }

    private void StartPanInternal(CamMode panMode)
    {
        StopPanInternal();
        SaveOrbitViewForRestore();

        _isHoldingRotate = false;
        SetCaptured(false);

        _mode = panMode;
        panTime01 = 0f;

        _panCo = StartCoroutine(PanRoutine());
    }

    private IEnumerator PanRoutine()
    {
        float dur = Mathf.Max(0.0001f, panDurationSeconds);
        float t = 0f;

        while (t < dur)
        {
            t += Time.deltaTime;
            panTime01 = Mathf.Clamp01(t / dur);
            yield return null;
        }

        panTime01 = 1f;
        RestoreOrbitView();
        _panCo = null;
    }

    public void StopPan()
    {
        StopPanInternal();
        RestoreOrbitView();
    }

    private void StopPanInternal()
    {
        if (_panCo != null)
        {
            StopCoroutine(_panCo);
            _panCo = null;
        }
    }

    private void SaveOrbitViewForRestore()
    {
        _savedOrbitAngles = _orbitAngles;
        _savedOrbitDistance = distance;
        _savedCenterOffsetLocal = _centerOffsetLocal;
    }

    private void RestoreOrbitView()
    {
        _centerOffsetLocal = _savedCenterOffsetLocal;
        _orbitAngles = _savedOrbitAngles;
        distance = _savedOrbitDistance;

        _mode = followEnabled ? CamMode.Follow : CamMode.Orbit;

        if (_mode == CamMode.Orbit)
            ApplyOrbit();
        else if (_mode == CamMode.Follow)
            SeedFollowLazyState();
    }

    // -----------------------
    // FollowCam (Y-only)
    // -----------------------

    public void SetFollowCam(bool enabled)
    {
        followEnabled = enabled;
        _prevFollowEnabled = followEnabled;

        StopPanInternal();

        if (followEnabled)
        {
            SaveOrbitViewForRestore();
            _mode = CamMode.Follow;

            _isHoldingRotate = false;
            SetCaptured(false);

            SeedFollowLazyState();
        }
        else
        {
            RestoreOrbitView();
        }
    }

    private void SeedFollowLazyState()
    {
        _prevCenter = GetOrbitCenterWorld();

        _lastMoveDir = FlattenOnGround(character != null ? character.forward : Vector3.forward);
        _smoothedMoveDir = _lastMoveDir;
        _smoothedMoveDirVel = Vector3.zero;

        _followYawVel = 0f;
        _followPosVel = Vector3.zero;
    }

    private static Vector3 FlattenOnGround(Vector3 v)
    {
        v.y = 0f;
        if (v.sqrMagnitude < 0.000001f) return Vector3.forward;
        return v.normalized;
    }

    private Vector3 GetPlanarVelocity(Vector3 center, float dt)
    {
        // ✅ NPCs: use NavMeshAgent velocity if available
        if (_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
        {
            Vector3 v = _agent.velocity;
            v.y = 0f;
            return v;
        }

        if (_rb != null)
        {
            Vector3 v = _rb.linearVelocity; // note: Unity Rigidbody uses .velocity; keeping your existing line as-is
            v.y = 0f;
            return v;
        }

        if (_cc != null)
        {
            Vector3 v = _cc.velocity;
            v.y = 0f;
            return v;
        }

        // fallback: center delta
        if (dt > 0.000001f)
        {
            Vector3 v = (center - _prevCenter) / dt;
            v.y = 0f;
            return v;
        }

        return Vector3.zero;
    }

    private void ApplyFollow()
    {
        if (character == null) return;

        Vector3 center = GetOrbitCenterWorld();
        float dt = Time.deltaTime;

        Vector3 v = GetPlanarVelocity(center, dt);
        float speed = v.magnitude;

        Vector3 targetDir = _lastMoveDir;
        if (speed > followMinMoveSpeed)
            targetDir = v / Mathf.Max(0.0001f, speed);

        float dirTime = Mathf.Max(0.01f, followDirSmoothTime);
        _smoothedMoveDir = Vector3.SmoothDamp(_smoothedMoveDir, targetDir, ref _smoothedMoveDirVel, dirTime, Mathf.Infinity, dt);
        _smoothedMoveDir = FlattenOnGround(_smoothedMoveDir);

        if (speed > followMinMoveSpeed)
            _lastMoveDir = _smoothedMoveDir;

        float desiredYaw = Quaternion.LookRotation(_smoothedMoveDir, Vector3.up).eulerAngles.y;

        float rotTime = Mathf.Max(0.01f, followRotSmoothTime);
        float currentYaw = transform.rotation.eulerAngles.y;
        float yaw = Mathf.SmoothDampAngle(currentYaw, desiredYaw, ref _followYawVel, rotTime, Mathf.Infinity, dt);

        Quaternion yawRot = Quaternion.Euler(0f, yaw, 0f);

        float back = Mathf.Max(0.1f, followDistance);
        Vector3 desiredPos =
            center
            - (yawRot * Vector3.forward) * back
            + Vector3.up * followHeight
            + (yawRot * Vector3.right) * followSideOffset;

        float posTime = Mathf.Max(0.01f, followPosSmoothTime);
        float maxSpeed = (followMaxPosSpeed > 0f) ? followMaxPosSpeed : Mathf.Infinity;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _followPosVel, posTime, maxSpeed, dt);

        transform.rotation = followKeepLevel ? yawRot : transform.rotation;
        _prevCenter = center;
    }

    // -----------------------
    // Convenience
    // -----------------------

    public Transform GetTarget() => character;
    public bool IsPanning => _panCo != null;
    public bool FollowEnabled => followEnabled;
}

#if UNITY_EDITOR
[CustomEditor(typeof(DebugCamera))]
public class DebugCameraEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DebugCamera cam = (DebugCamera)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Test Panel", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Play Pan Horizontal")) cam.PlayPanHorizontal();
            if (GUILayout.Button("Play Pan Vertical")) cam.PlayPanVertical();
            if (GUILayout.Button("Stop Pan")) cam.StopPan();
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif
