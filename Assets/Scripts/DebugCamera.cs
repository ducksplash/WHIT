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
    [Tooltip("Hold this to rotate while a target is selected.")]
    [SerializeField] private InputActionReference holdClick;

    [Tooltip("Click this to pick a new target. Only works in Fly mode.")]
    [SerializeField] private InputActionReference pickClick;

    [Tooltip("Press this to abandon target and re-enter free cam.")]
    [SerializeField] private InputActionReference escapeButton;
    
    [Tooltip("Spawn NPC button")]
    [SerializeField] private InputActionReference interactClick;

    [Header("Fly Cam Inputs")]
    [Tooltip("WASD / Left Stick (Vector2): X = strafe, Y = forward/back.")]
    [SerializeField] private InputActionReference flyMoveAction;

    [Header("Fly Cam Tuning")]
    [SerializeField] private float flyMoveSpeed = 6f;
    [SerializeField] private float flyLookSensitivity = 0.08f;
    [SerializeField] private float flyLookSmoothing = 6f;
    [SerializeField] private bool flyKeepLevelMovement = true;

    [Header("Fly Cam Speed Modifier")]
    [SerializeField] private InputActionReference flyFastHold;
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

    [Tooltip("If true, when setting/retargeting we will orbit around collider bounds center.")]
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
    [SerializeField] private float panDurationSeconds = 3f;
    [SerializeField] private float panExtent = 10f;
    [SerializeField] private float panFrontDistance = 0f;

    [Range(0f, 1f)]
    [SerializeField] private float panTime01 = 0f;

    [Header("FollowCam")]
    [SerializeField] private bool followEnabled = false;
    [SerializeField] private float followDistance = 4f;
    [SerializeField] private float followHeight = 1.5f;
    [SerializeField] private float followSideOffset = 0f;
    [SerializeField] private float followMinMoveSpeed = 0.05f;

    [Header("Follow Laziness")]
    [SerializeField] private float followDirSmoothTime = 0.25f;

    [Header("Zoom (Orbit + Follow)")]
    [SerializeField] private InputActionReference zoomAction;
    [SerializeField] private float zoomSpeed = 0.25f;
    [SerializeField] private bool zoomAffectsFOV = false;
    [SerializeField] private ZoomNaura zoomNaura;

    [Tooltip("How long (seconds) for position to catch up (bigger = lazier position).")]
    [SerializeField] private float followPosSmoothTime = 0.20f;

    [Tooltip("How long (seconds) for rotation to catch up (bigger = lazier rotation).")]
    [SerializeField] private float followRotSmoothTime = 0.35f;

    [Tooltip("Clamp how fast the camera position can change (units/sec). 0 = unlimited.")]
    [SerializeField] private float followMaxPosSpeed = 0f;

    [Header("Keep Level (No Tilt)")]
    [SerializeField] private bool followKeepLevel = true;
    [SerializeField] private bool orbitKeepLevel = false;

    [Header("Debug")]
    [SerializeField] private bool debugRay = true;
    [SerializeField] private float debugRaySeconds = 1.0f;
    [SerializeField] private bool debugLogs = false;

    [Header("UI Distance Mapping")]
    [Tooltip("Higher values give more control near the close end of the orbit slider.")]
    [SerializeField] private float orbitDistanceSliderExponent = 2.2f;

    [Tooltip("Higher values give more control near the close end of the follow slider.")]
    [SerializeField] private float followDistanceSliderExponent = 2.2f;

    private enum CamMode { Fly, Orbit, PanHorizontal, PanVertical, Follow }
    [SerializeField] private CamMode _mode = CamMode.Fly;

    [Header("NPC Spawning")]
    [SerializeField] private NPCManager npcManager;
    [SerializeField] private NPC selectedSpawnNPC = NPC.Eimear_Scott;

    [Tooltip("Layers that count as valid click surfaces for spawning.")]
    [SerializeField] private LayerMask spawnSurfaceMask = ~0;

    [Tooltip("How far from the clicked point we search for NavMesh.")]
    [SerializeField] private float spawnNavMeshSampleRadius = 2.0f;

    [Tooltip("Spawned NPC will face this direction projected on the ground.")]
    [SerializeField] private bool spawnedNpcFacesCameraForward = true;
    
    
    
    private bool _isHoldingRotate;
    private Vector2 _orbitAngles;
    private Vector2 _appliedDelta;

    private Camera _cam;
    private int _targetLayer;
    private int _targetLayerMask;

    private Vector3 _centerOffsetLocal;
    private Collider _centerCollider;

    private Coroutine _panCo;

    private Vector2 _savedOrbitAngles;
    private float _savedOrbitDistance;
    private Vector3 _savedCenterOffsetLocal;

    private bool _prevFollowEnabled;

    private Vector3 _prevCenter;
    private Vector3 _lastMoveDir = Vector3.forward;
    private Rigidbody _rb;
    private CharacterController _cc;

    private Vector3 _smoothedMoveDir = Vector3.forward;
    private Vector3 _smoothedMoveDirVel;

    private Vector3 _followPosVel;
    private float _followYawVel;

    private Vector2 _flyAngles;
    private Vector2 _flyLookApplied;

    private float _defaultOrbitDistance;
    private float _defaultFollowDistance;

    // Cached pan snapshot so pan path does not change if target turns mid-pan
    private Vector3 _panCenterWorld;
    private Vector3 _panAxisWorld;
    private Vector3 _panForwardWorld;
    private float _panFrontDistanceWorld;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam == null) _cam = Camera.main;

        if (npcManager == null)
            npcManager = FindFirstObjectByType<NPCManager>();

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
        _defaultOrbitDistance = distance;
        _defaultFollowDistance = followDistance;

        if (character == null)
            EnterFlyMode();
        else
            EnterTargetModeAfterSet(character);
    }

    private void Start()
    {
        if (GameMaster.Instance != null)
        {
            flyLookSensitivity = GameMaster.Instance.MouseSensitivity;
            sensitivity = GameMaster.Instance.MouseSensitivity;
        }
        else
        {
            flyLookSensitivity = 0.02f;
            sensitivity = 0.02f;
        }
    }

    void OnEnable()
    {
        if (holdClick != null)
        {
            holdClick.action.performed += OnHoldStart;
            holdClick.action.canceled += OnHoldEnd;
        }

        if (flyFastHold != null)
        {
            flyFastHold.action.performed += OnFlyFastStart;
            flyFastHold.action.canceled += OnFlyFastEnd;
        }

        if (pickClick != null)
        {
            pickClick.action.performed += OnPick;
        }

        if (interactClick != null)
        {
            interactClick.action.performed += OnInteract;
        }

        if (escapeButton != null)
        {
            escapeButton.action.performed += OnEscape;
        }

    }

    void OnDisable()
    {
        if (holdClick != null)
        {
            holdClick.action.performed -= OnHoldStart;
            holdClick.action.canceled -= OnHoldEnd;
        }

        if (flyFastHold != null)
        {
            flyFastHold.action.performed -= OnFlyFastStart;
            flyFastHold.action.canceled -= OnFlyFastEnd;
        }

        if (pickClick != null)
        {
            pickClick.action.performed -= OnPick;
        }

        if (interactClick != null)
        {
            interactClick.action.performed -= OnInteract;
        }

        if (escapeButton != null)
        {
            escapeButton.action.performed -= OnEscape;
        }

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
            ApplyFlyLook();
            ApplyFlyMove();
            return;
        }

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
            case CamMode.Fly: break;
            case CamMode.Orbit: if (character != null) ApplyOrbit(); break;
            case CamMode.PanHorizontal: if (character != null) PanHorizontal(panTime01); break;
            case CamMode.PanVertical: if (character != null) PanVertical(panTime01); break;
            case CamMode.Follow: if (character != null) ApplyFollow(); break;
        }
    }

    private bool IsTargetCameraMode()
    {
        return _mode == CamMode.Orbit || _mode == CamMode.Follow;
    }

    private float SliderToDistance(float t, float exponent)
    {
        t = Mathf.Clamp01(t);
        exponent = Mathf.Max(0.01f, exponent);

        float curved = Mathf.Pow(t, exponent);
        return Mathf.Lerp(minDistance, maxDistance, curved);
    }

    private float DistanceToSlider(float d, float exponent)
    {
        exponent = Mathf.Max(0.01f, exponent);

        float linear = Mathf.InverseLerp(minDistance, maxDistance, d);
        return Mathf.Pow(Mathf.Clamp01(linear), 1f / exponent);
    }

    private void CachePanSnapshot(bool horizontal)
    {
        if (character == null) return;

        Vector3 center = GetOrbitCenterWorld();

        Vector3 forward = character.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.000001f)
            forward = transform.forward;
        forward.Normalize();

        Vector3 right = character.right;
        right.y = 0f;
        if (right.sqrMagnitude < 0.000001f)
            right = Vector3.Cross(Vector3.up, forward);
        right.Normalize();

        _panCenterWorld = center;
        _panForwardWorld = forward;
        _panAxisWorld = horizontal ? right : Vector3.up;
        _panFrontDistanceWorld = (panFrontDistance > 0.0001f)
            ? panFrontDistance
            : Mathf.Clamp(distance, minDistance, maxDistance);
    }

    private void ResetZoomToDefault()
    {
        distance = Mathf.Clamp(_defaultOrbitDistance, minDistance, maxDistance);
        followDistance = Mathf.Clamp(_defaultFollowDistance, minDistance, maxDistance);

        if (zoomNaura != null)
            zoomNaura.ResetZoomImmediate();
    }

    void EnterFlyMode()
    {
        StopPanInternal();
        ResetZoomToDefault();

        character = null;
        _centerCollider = null;

        _isHoldingRotate = false;
        _appliedDelta = Vector2.zero;

        _mode = CamMode.Fly;

        Vector3 fwd = transform.forward.normalized;

        float yaw = Mathf.Atan2(fwd.x, fwd.z) * Mathf.Rad2Deg;
        float pitch = -Mathf.Asin(Mathf.Clamp(fwd.y, -1f, 1f)) * Mathf.Rad2Deg;
        pitch = Mathf.Clamp(pitch, pitchClampMin, pitchClampMax);

        _flyAngles = new Vector2(yaw, pitch);
        _flyLookApplied = Vector2.zero;

        SetCaptured(true);

        transform.rotation = Quaternion.Euler(_flyAngles.y, _flyAngles.x, 0f);
    }

    void ApplyFlyLook()
    {
        if (lookAction == null) return;

        Vector2 raw = lookAction.action.ReadValue<Vector2>();

        float smooth = Mathf.Max(1f, flyLookSmoothing);
        Vector2 targetDelta = raw * (flyLookSensitivity * smooth);
        _flyLookApplied = Vector2.Lerp(_flyLookApplied, targetDelta, 1f / smooth);

        _flyAngles.x += _flyLookApplied.x;
        _flyAngles.y -= _flyLookApplied.y;
        _flyAngles.y = Mathf.Clamp(_flyAngles.y, pitchClampMin, pitchClampMax);

        transform.rotation = Quaternion.Euler(_flyAngles.y, _flyAngles.x, 0f);
    }

    void ApplyFlyMove()
    {
        if (flyMoveAction == null) return;

        Vector2 move = flyMoveAction.action.ReadValue<Vector2>();
        if (move.sqrMagnitude < 0.000001f) return;

        float dt = Time.deltaTime;
        float speed = flyMoveSpeed * (_flyFastHeld ? Mathf.Max(1f, flyFastMultiplier) : 1f);

        Vector3 fwd = transform.forward;
        Vector3 right = transform.right;

        if (flyKeepLevelMovement)
        {
            fwd.y = 0f;
            right.y = 0f;
            fwd.Normalize();
            right.Normalize();
        }

        Vector2 input = move.normalized;
        Vector3 wishDir = (right * input.x + fwd * input.y);
        transform.position += wishDir * (speed * dt);
    }

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
            float t = Mathf.InverseLerp(maxDistance, minDistance, distance);
            zoomNaura.zoomAmount = Mathf.Clamp01(t);
            zoomNaura.AutoZoomToAmount(zoomNaura.zoomAmount);
        }
    }

    private void OnHoldStart(InputAction.CallbackContext _)
    {
        if (_mode == CamMode.Orbit)
        {
            _isHoldingRotate = true;
            _appliedDelta = Vector2.zero;
        }
    }

    private void OnHoldEnd(InputAction.CallbackContext _)
    {
        if (_mode == CamMode.Orbit)
        {
            _isHoldingRotate = false;
            _appliedDelta = Vector2.zero;
        }
    }

    private void SetCaptured(bool captured)
    {
        Cursor.lockState = captured ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !captured;
    }

    private void OnEscape(InputAction.CallbackContext _)
    {
        EnterFlyMode();
    }

    private void OnPick(InputAction.CallbackContext _)
    {
        if (_mode != CamMode.Fly) return;
        TryPickTargetOnce();
    }
    
    private void OnInteract(InputAction.CallbackContext _)
    {
        if (_mode != CamMode.Fly) return;
        TrySpawnNpcAtClick();
    }
    
    private void TrySpawnNpcAtClick()
{
    if (_cam == null) return;
    if (npcManager == null)
    {
        Debug.LogWarning($"{nameof(DebugCamera)}: No NPCManager assigned/found.");
        return;
    }

    GameObject prefab = npcManager.GetPrefabForNPC(selectedSpawnNPC);
    if (prefab == null) return;

    Ray ray = BuildPickRay();

    if (debugRay)
        Debug.DrawRay(ray.origin, ray.direction * pickDistance, Color.green, debugRaySeconds);

    RaycastHit[] hits = Physics.RaycastAll(ray, pickDistance, spawnSurfaceMask, QueryTriggerInteraction.Ignore);
    if (hits == null || hits.Length == 0)
    {
        if (debugLogs) Debug.Log($"{nameof(DebugCamera)}: Spawn MISS (no surfaces hit).");
        return;
    }

    Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

    for (int i = 0; i < hits.Length; i++)
    {
        RaycastHit h = hits[i];
        if (h.collider == null) continue;

        // Ignore clicks on existing camera targets / NPCs while in spawn mode.
        Transform maybeNpc = ResolvePickedTransform(h);
        if (maybeNpc != null)
            continue;

        if (!NavMesh.SamplePosition(h.point, out NavMeshHit navHit, spawnNavMeshSampleRadius, NavMesh.AllAreas))
            continue;

        SpawnNpcAt(navHit.position);
        return;
    }

    if (debugLogs)
        Debug.Log($"{nameof(DebugCamera)}: Spawn failed - no valid NavMesh point under click.");
}

private void SpawnNpcAt(Vector3 worldPos)
{
    if (npcManager == null) return;

    GameObject prefab = npcManager.GetPrefabForNPC(selectedSpawnNPC);
    if (prefab == null) return;

    Vector3 spawnForward = Vector3.forward;

    if (spawnedNpcFacesCameraForward)
    {
        spawnForward = transform.forward;
        spawnForward.y = 0f;
        if (spawnForward.sqrMagnitude < 0.0001f)
            spawnForward = Vector3.forward;
        spawnForward.Normalize();
    }

    Quaternion rot = Quaternion.LookRotation(spawnForward, Vector3.up);

    GameObject go = Instantiate(prefab, worldPos, rot);

    NPCController npc = go.GetComponent<NPCController>();
    if (npc == null)
        npc = go.GetComponentInChildren<NPCController>();

    if (npc != null)
    {
        npc.sceneNPCManager = npcManager;
        npcManager.RegisterNPC(npc);

        if (npc.agent != null && npc.agent.enabled)
        {
            if (NavMesh.SamplePosition(worldPos, out NavMeshHit hit, spawnNavMeshSampleRadius, NavMesh.AllAreas))
                npc.agent.Warp(hit.position);
        }

        SetTarget(npc.transform);
    }
    else
    {
        Debug.LogWarning($"{nameof(DebugCamera)}: Spawned prefab '{go.name}' has no NPCController.");
    }

    if (debugLogs)
        Debug.Log($"{nameof(DebugCamera)}: Spawned '{selectedSpawnNPC}' at {worldPos}.");
}
    
    public void ClearTargetAndEnterFlyMode()
    {
        EnterFlyMode();
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

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

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
                if (Mouse.current == null)
                    return new Ray(transform.position, transform.forward);

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

        _mode = followEnabled ? CamMode.Follow : CamMode.Orbit;

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

        _agent = character.GetComponentInParent<NavMeshAgent>();
        if (_agent == null) _agent = character.GetComponent<NavMeshAgent>();

        _cc = character.GetComponentInParent<CharacterController>();
        if (_cc == null) _cc = character.GetComponent<CharacterController>();
    }

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

        // FIX: remove the minus sign
        float pitch = Mathf.Asin(Mathf.Clamp(dir.y, -1f, 1f)) * Mathf.Rad2Deg;
        pitch = Mathf.Clamp(pitch, pitchClampMin, pitchClampMax);

        _orbitAngles = new Vector2(yaw, pitch);
        _appliedDelta = Vector2.zero;
    }

    public void PanHorizontal(float t01)
    {
        if (character == null) return;
        t01 = Mathf.Clamp01(t01);

        Vector3 basePos = _panCenterWorld - (_panForwardWorld * _panFrontDistanceWorld);

        Vector3 start = basePos + _panAxisWorld * (-panExtent);
        Vector3 end = basePos + _panAxisWorld * (panExtent);

        transform.position = Vector3.Lerp(start, end, t01);
        transform.rotation = Quaternion.LookRotation((_panCenterWorld - transform.position).normalized, Vector3.up);
    }

    public void PanVertical(float t01)
    {
        if (character == null) return;
        t01 = Mathf.Clamp01(t01);

        Vector3 basePos = _panCenterWorld - (_panForwardWorld * _panFrontDistanceWorld);

        Vector3 start = basePos + _panAxisWorld * (-panExtent);
        Vector3 end = basePos + _panAxisWorld * (panExtent);

        transform.position = Vector3.Lerp(start, end, t01);
        transform.rotation = Quaternion.LookRotation((_panCenterWorld - transform.position).normalized, Vector3.up);
    }

    public void PlayPanHorizontal()
    {
        if (character == null) return;
        CachePanSnapshot(true);
        StartPanInternal(CamMode.PanHorizontal);
    }

    public void PlayPanVertical()
    {
        if (character == null) return;
        CachePanSnapshot(false);
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
        if (_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
        {
            Vector3 v = _agent.velocity;
            v.y = 0f;
            return v;
        }

        if (_rb != null)
        {
            Vector3 v = _rb.linearVelocity;
            v.y = 0f;
            return v;
        }

        if (_cc != null)
        {
            Vector3 v = _cc.velocity;
            v.y = 0f;
            return v;
        }

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
    // UI API
    // -----------------------

    public bool HasTarget() => character != null;

    public void UI_PlayPanHorizontal()
    {
        if (!HasTarget()) return;
        PlayPanHorizontal();
    }

    public void UI_PlayPanVertical()
    {
        if (!HasTarget()) return;
        PlayPanVertical();
    }

    public void UI_StopPan()
    {
        StopPan();
    }

    public void UI_SetFollowEnabled(bool enabled)
    {
        if (!HasTarget())
        {
            followEnabled = false;
            _prevFollowEnabled = false;
            return;
        }

        SetFollowCam(enabled);
    }

    public void UI_ToggleFollow()
    {
        UI_SetFollowEnabled(!followEnabled);
    }

    public void UI_ResetZoom()
    {
        ResetZoomToDefault();
    }

    public void UI_SetOrbitDistanceNormalized(float t)
    {
        distance = SliderToDistance(t, orbitDistanceSliderExponent);
    }

    public void UI_SetFollowDistanceNormalized(float t)
    {
        followDistance = SliderToDistance(t, followDistanceSliderExponent);
    }

    public float UI_GetOrbitDistanceNormalized()
    {
        return DistanceToSlider(distance, orbitDistanceSliderExponent);
    }

    public float UI_GetFollowDistanceNormalized()
    {
        return DistanceToSlider(followDistance, followDistanceSliderExponent);
    }
    
    public void UI_SetSpawnNpc(NPC npcType)
    {
        selectedSpawnNPC = npcType;
    }
    

    public void UI_DeleteSelectedNpc()
    {
        if (character == null) return;

        NPCController npc = character.GetComponentInParent<NPCController>();
        if (npc == null) return;

        if (npc.sceneNPCManager != null)
            npc.sceneNPCManager.UnregisterNPC(npc);

        ClearTargetAndEnterFlyMode();
        Destroy(npc.gameObject);
    }

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