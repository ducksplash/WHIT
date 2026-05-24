// Player.cs

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;

public class Player : Singleton<Player>
{
    [Header("Player Status")] 
    public PlayerStatus PlayerStatus;
    
    [Header("Movement & State")]
    public float walkspeed = 0.1f;
    public float sprintspeed = 0.2f;
    public float jumpForce = 2f;

    private float speed;
    private Vector2 moveInput;

    public Me Me;
    
    private CharacterController thisCharController;

    public Camera FirstPersonCamera;
    public Camera ThirdPersonCamera;
    public Camera DebugCam;
    public Camera CurrentCamera;
    
    public float RayCastDistance = 4f;

    
    
    [Header("Animation")]
    public Animator Noranimator;

    private static readonly int AnimMoveX      = Animator.StringToHash("MoveX");
    private static readonly int AnimMoveY      = Animator.StringToHash("MoveY");
    private static readonly int AnimSpeed      = Animator.StringToHash("Speed");
    private static readonly int AnimCrouching  = Animator.StringToHash("Crouching");
    private static readonly int AnimJump       = Animator.StringToHash("JUMP");
    private static readonly int AnimGrounded   = Animator.StringToHash("Grounded");

    // Triggers
    private static readonly int AnimMelee      = Animator.StringToHash("MELEE");

    private static readonly int AnimPhoneOut      = Animator.StringToHash("PHONEOUT");
    private static readonly int AnimNotepadOut    = Animator.StringToHash("NOTEPADOUT");
    private static readonly int AnimPhoneOutCrouch = Animator.StringToHash("PHONEOUTCROUCH");

    // ── Sitting ────────────────────────────────────────────────────────────
    private static readonly int AnimSitDown = Animator.StringToHash("SitDown");
    private static readonly int AnimSitIdle = Animator.StringToHash("SitIdle");
    private static readonly int AnimStandUp = Animator.StringToHash("StandUp");
    // ───────────────────────────────────────────────────────────────────────

    [Header("Locomotion Torch Clip Overrides")]
    public AnimationClip idleWithTorch;
    public AnimationClip idleWithoutTorch;
    public AnimationClip walkingWithTorch;
    public AnimationClip walkingWithoutTorch;

    private AnimatorOverrideController _locomotionOverride;
    private RuntimeAnimatorController _baseController;
    private bool _hasTorch;

    [Header("Crawl Animation Detection (BlendTree Clip-Based)")]
    public string crawlClipName = "NoraCrawl";
    public int crawlLayerIndex = 0;
    [Range(0f, 1f)] public float crawlWeightThreshold = 0.15f;
    private bool _torchSuppressedByCrawl;

    [Tooltip("How quickly animation parameters catch up (bigger = snappier).")]
    public float animDampTime = 0.12f;

    [Tooltip("Turn this on to print debug values.")]
    public bool debugAnim = false;

    private Transform _animT;

    public GameObject PlayerTorch;
    private bool _lastIsCrawling;
    private Vector3 _lastWorldPos;
    private Vector3 _manualVelocityXZ;

    [Header("Jump Tuning")]
    public float groundedFalseAfterJumpSeconds = 0.12f;
    private float _forceUngroundedUntil;

    // ── Melee ───────────────────────────────────────────────────────────────
    [Header("Melee")] 
    public bool CombatEnabled;
    public InputActionReference meleeAction;
    public float meleeCooldown = 0.55f;
    public bool lockMovementDuringMelee = false;
    public float lockMoveSeconds = 0.25f;

    private float _nextMeleeTime;
    private float _moveLockedUntil;

    // ── UpperBody Layer gating ──────────────────────────────────────────────
    [Header("UpperBody Layer Gating")]
    public string upperBodyLayerName = "UpperBody";
    public float upperBodyBlendIn  = 0.03f;
    public float upperBodyBlendOut = 0.06f;

    private int      _upperBodyLayerIndex = -1;
    private Coroutine _upperBodyBlendCo;

    private bool _upperBodyHeldByMelee;
    private bool _upperBodyHeldByPhone;

    public UpperBodyPitch UpperBodyPitch;
    
    // ── Phone (UpperBody) ───────────────────────────────────────────────────
    [Header("Phone (UpperBody)")]
    public string upperBodyPhoneOutStateName  = "PHONE OUT";
    public string upperBodyPhoneAwayStateName = "PHONE AWAY";

    private int _phoneOutStateHash;
    private int _phoneAwayStateHash;

    // ── Crouch (Controller Collider) ────────────────────────────────────────
    [Header("Crouch (Controller Collider)")]
    public bool crouching;

    public float standheight       = 0f;
    public float croucheight       = 1.0f;
    public float crouchBlendSeconds = 0.12f;

    public LayerMask standCheckMask  = ~0;
    public float standCheckShrink    = 0.02f;

    public Image  stanceimg;
    public Sprite crouchsprite;
    public Sprite standsprite;

    private float   _standHeight;
    private Vector3 _standCenter;
    private float   _bottomLocalOffset;
    private bool    _controllerBaselineCaptured;
    private Camera _previousGameplayCamera;
    private bool _debugCameraEnabled;
    private Coroutine _crouchCo;

    public GameObject TravelNotepad;
    public bool climbing;
    public GameObject LadderAttachedTo;

    // ── Sitting ─────────────────────────────────────────────────────────────
    // NOTE: EventManager.OnNoraSit must be declared as Action<Seat> to match
    // the updated NoraSit(Seat) signature below.

    [Header("Sitting")]
    [Tooltip("Walk speed used when approaching a seat.")]
    public float sitMoveSpeed = 3.5f;

    [Tooltip("Planar distance at which the approach phase is considered complete.")]
    public float sitArriveDistance = 0.3f;

    [Tooltip("How fast Nora rotates to face the seat direction.")]
    public float sitRotateSpeed = 7f;

    [Tooltip("Yaw tolerance (degrees) for the alignment phase.")]
    public float sitAlignToleranceDeg = 5f;

    [Tooltip("How far in front of the seat Nora stops before backstepping.")]
    public float sitPreSitForwardOffset = 0.45f;

    [Tooltip("How far back from the pre-sit point Nora steps before sitting.")]
    public float sitBackstepDistance = 0.25f;

    [Tooltip("Speed of the backstep movement.")]
    public float sitBackstepSpeed = 0.8f;

    [Tooltip("Extra offset applied to root position when snapped to a seated position.")]
    public Vector3 sitSeatedRootOffset = Vector3.zero;

    [Tooltip("Animator layer index that contains SitDown / SitIdle / StandUp states.")]
    public int sitAnimLayer = 0;

    [Tooltip("Duration of the SitDown clip – coroutine waits this long before entering SitIdle.")]
    public float sitDownDuration = 1.2f;

    [Tooltip("Duration of the StandUp clip – coroutine waits this long before returning to locomotion.")]
    public float standUpDuration = 1.0f;

    private Coroutine  _sitCoroutine;
    private Transform  _activeSeatTransform;
    public bool       IsSeated;
    // ───────────────────────────────────────────────────────────────────────

    [Header("UI References")]
    public TextMeshProUGUI PaperDeathText;
    public TextMeshProUGUI PaperDateText;
    public CanvasGroup DeathScreenMain;
    public CanvasGroup DeathScreenFader;
    public CanvasGroup PaperScreenFader;
    public CanvasGroup DiedTextFader;
    public CanvasGroup ButtonFaderLeave;
    public CanvasGroup ButtonFaderContinue;

    public CanvasGroup CrossHair;
    public CanvasGroup CrouchIndicator;
    public CanvasGroup TorchIndicator;
    public CanvasGroup EvidenceCompanion;

    [Header("Spawn & Hands")] 
    public Transform playerHand;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference crouchAction;
    public InputActionReference walkAction;
    public InputActionReference climbUpAction;
    public InputActionReference climbDownAction;
    public InputActionReference exitLadderAction;
    public InputActionReference CameraToggleAction;
    public InputActionReference StandUpAction;

    [Header("Scripts")]
    public FirstPersonLook FirstPersonLook;
    public Phone PlayerPhone;

    private Vector3 moveDirection = Vector3.zero;
    private bool jumpRequested    = false;
    private bool walking          = false;

    public bool MoveOverride;
    public bool ZoomOverride;

    // ────────────────────────────────────────────────────────────────────────
    // Start
    // ────────────────────────────────────────────────────────────────────────
    private CancellationTokenSource _sitCts;
    private CancellationTokenSource _crouchCts;
    private CancellationTokenSource _upperBodyCts;

    // ────────────────────────────────────────────────────────────────────────
    // Start
    // ────────────────────────────────────────────────────────────────────────
    void Start()
    {
        CurrentCamera = FirstPersonCamera;

        EventManager.OnTorchCollected += TorchCollected;
        EventManager.OnNoraSit        += NoraSit;

        if (StandUpAction != null) StandUpAction.action.performed += StandUp;

        thisCharController = GetComponentInParent<CharacterController>();
        if (thisCharController == null) thisCharController = GetComponent<CharacterController>();

        speed = sprintspeed;

        if (Noranimator == null)
            Noranimator = GetComponentInChildren<Animator>(true);

        if (Noranimator != null)
        {
            _animT = Noranimator.transform;
            Noranimator.applyRootMotion = false;
            Noranimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            Noranimator.SetBool(AnimCrouching, crouching);
            Noranimator.SetBool(AnimGrounded, true);
            Noranimator.SetFloat(AnimSpeed, 0f);
            Noranimator.SetFloat(AnimMoveX, 0f);
            Noranimator.SetFloat(AnimMoveY, 0f);

            _upperBodyLayerIndex = Noranimator.GetLayerIndex(upperBodyLayerName);
            if (_upperBodyLayerIndex < 0 && debugAnim)
                Debug.LogWarning($"[Player] Animator has no layer named '{upperBodyLayerName}'.");

            _phoneOutStateHash = Animator.StringToHash(upperBodyPhoneOutStateName);
            _phoneAwayStateHash = Animator.StringToHash(upperBodyPhoneAwayStateName);

            Noranimator.ResetTrigger(AnimMelee);
            Noranimator.ResetTrigger(AnimPhoneOutCrouch);
            Noranimator.ResetTrigger(AnimPhoneOut);
        }

        CaptureControllerBaseline();
        _lastWorldPos = GetWorldPositionForVelocity();

        _baseController = Noranimator.runtimeAnimatorController;
        _locomotionOverride = new AnimatorOverrideController(_baseController);
        Noranimator.runtimeAnimatorController = _locomotionOverride;

        SetTorchLocomotion(false);

        CameraToggleAction.action.performed += ToggleDebugCamera;
    }


    public void Spawn()
    {
        Vector3 spawnPoint;

        switch (GameMaster.Instance.THISLEVEL)
        {
            case GAMELEVEL.ETVStudio:    spawnPoint = GameMaster.Instance.SPAWNPOINTETV;          break;
            case GAMELEVEL.NorasFlat:    spawnPoint = GameMaster.Instance.SPAWNPOINTNORASFLAT;    break;
            case GAMELEVEL.TawleyMeats:  spawnPoint = GameMaster.Instance.SPAWNPOINTTAWLEYMEATS; break;
            case GAMELEVEL.RoarkOutside: spawnPoint = GameMaster.Instance.SPAWNPOINTROARKOUTSIDE; break;
            case GAMELEVEL.RoarkInside:  spawnPoint = GameMaster.Instance.SPAWNPOINTROARKINSIDE;          break;
            default:                     spawnPoint = GameMaster.Instance.SPAWNPOINTNORASFLAT;    break;
        }

        // Disable the CC first so it doesn't fight the teleport
        bool ccWasEnabled = thisCharController != null && thisCharController.enabled;
        if (thisCharController != null) thisCharController.enabled = false;

        // Move the CC's transform (the parent), not the Player child
        Transform rootTransform = thisCharController != null
            ? thisCharController.transform
            : transform;

        rootTransform.position = spawnPoint;
        Physics.SyncTransforms(); // tell the physics engine immediately

        if (thisCharController != null) thisCharController.enabled = ccWasEnabled;

        Debug.Log("Spawn point: " + spawnPoint + " | Root landed at: " + rootTransform.position);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Torch
    // ────────────────────────────────────────────────────────────────────────
    private void TorchCollected() => SetTorchLocomotion(true);

    public void ToggleDebugCamera(InputAction.CallbackContext callbackContext)
    {
        _debugCameraEnabled = !_debugCameraEnabled;

        if (_debugCameraEnabled)
        {
            EnterDebugCamera();
        }
        else
        {
            ExitDebugCamera();
        }
    }
    
    private void EnterDebugCamera()
    {
        Debug.Log("debug cam on");

        // Remember ACTIVE gameplay camera
        if (FirstPersonCamera != null && FirstPersonCamera.enabled)
        {
            _previousGameplayCamera = FirstPersonCamera;
        }
        else if (ThirdPersonCamera != null && ThirdPersonCamera.enabled)
        {
            _previousGameplayCamera = ThirdPersonCamera;
        }

        // Disable gameplay cameras
        if (FirstPersonCamera != null)
        {
            FirstPersonCamera.enabled = false;
            FirstPersonCamera.gameObject.GetComponent<Zoom>().enabled = false;
        }

        if (ThirdPersonCamera != null)
        {
            ThirdPersonCamera.enabled = false;
        }

        // Enable debug camera
        if (DebugCam != null)
        {
            DebugCam.gameObject.SetActive(true);
            DebugCam.enabled = true;
        }
        
    }
    
    private void ExitDebugCamera()
    {
        Debug.Log("debug cam off");

        // Disable debug camera
        if (DebugCam != null)
        {
            DebugCam.enabled = false;
            DebugCam.gameObject.SetActive(false);
        }

        

        // Restore previous gameplay camera
        if (_previousGameplayCamera != null)
        {
            _previousGameplayCamera.enabled = true;
            try
            {
                _previousGameplayCamera.gameObject.GetComponent<Zoom>().enabled = true;
            }
            catch
            {
                // 
            }
        }
        else if (FirstPersonCamera != null)
        {
            FirstPersonCamera.enabled = true;
            try
            {
                FirstPersonCamera.gameObject.GetComponent<Zoom>().enabled = true;
            }
            catch
            {
                // 
            }
        }
        
        
    }
    

    public void SetTorchLocomotion(bool hasTorch)
    {
        _hasTorch = hasTorch;

        if (_locomotionOverride == null)
        {
            Debug.LogWarning("[Player] Locomotion override controller not initialised yet.");
            return;
        }

        if (idleWithTorch == null || walkingWithTorch == null)
        {
            Debug.LogWarning("[Player] Missing WithTorch clips.");
            return;
        }

        if (!hasTorch && (idleWithoutTorch == null || walkingWithoutTorch == null))
        {
            Debug.LogWarning("[Player] Missing WithoutTorch clips.");
            return;
        }

        _locomotionOverride[idleWithTorch]    = hasTorch ? idleWithTorch    : idleWithoutTorch;
        _locomotionOverride[walkingWithTorch] = hasTorch ? walkingWithTorch : walkingWithoutTorch;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Controller baseline
    // ────────────────────────────────────────────────────────────────────────
    private void CaptureControllerBaseline()
    {
        if (thisCharController == null || _controllerBaselineCaptured) return;

        _standHeight  = thisCharController.height;
        _standCenter  = thisCharController.center;

        if (standheight <= 0f) standheight = _standHeight;

        _bottomLocalOffset = _standCenter.y - (_standHeight * 0.5f);
        _controllerBaselineCaptured = true;

        int playerLayer = thisCharController.gameObject.layer;
        standCheckMask &= ~(1 << playerLayer);

        if (crouching) ApplyControllerHeightKeepBottom(croucheight);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Enable / Disable
    // ────────────────────────────────────────────────────────────────────────
    void OnEnable()
    {
        moveAction?.action.Enable();
        jumpAction?.action.Enable();
        crouchAction?.action.Enable();
        walkAction?.action.Enable();
        climbUpAction?.action.Enable();
        climbDownAction?.action.Enable();
        exitLadderAction?.action.Enable();
        meleeAction?.action.Enable();

        if (jumpAction   != null) jumpAction.action.performed   += OnJump;
        if (crouchAction != null) crouchAction.action.performed += OnCrouchToggle;

        if (walkAction != null)
        {
            walkAction.action.performed += OnWalkPressed;
            walkAction.action.canceled  += OnWalkReleased;
        }

        if (meleeAction != null)
            meleeAction.action.performed += OnMelee;
    }

    void OnDisable()
    {
        if (jumpAction   != null) jumpAction.action.performed   -= OnJump;
        if (crouchAction != null) crouchAction.action.performed -= OnCrouchToggle;

        if (walkAction != null)
        {
            walkAction.action.performed -= OnWalkPressed;
            walkAction.action.canceled  -= OnWalkReleased;
        }

        if (meleeAction != null)
            meleeAction.action.performed -= OnMelee;
    }

    private void OnWalkPressed(InputAction.CallbackContext ctx) => walking = true;
    private void OnWalkReleased(InputAction.CallbackContext ctx) => walking = false;

    // ────────────────────────────────────────────────────────────────────────
    // Update
    // ────────────────────────────────────────────────────────────────────────
    void Update()
    {
        if (CurrentCamera == DebugCam) return;

        if (GameMaster.Instance != null && GameMaster.Instance.PauseManager != null &&
            GameMaster.Instance.PauseManager.IsPaused) return;

        if (MoveOverride && moveAction != null && !moveAction.action.enabled)
            moveAction.action.Enable();

        if (GameMaster.Instance != null && GameMaster.Instance.PLAYERBUSY && !MoveOverride) return;

        HandleMovement();
    }

public void NoraSit(Seat seat)
    {
        if (seat == null || seat.seatTransform == null) return;
        if (IsSeated) return;

        GameMaster.Instance.PLAYERBUSY = true;

        if (FirstPersonLook != null) FirstPersonLook.LockLook(true);
        if (thisCharController != null) thisCharController.enabled = false;

        _sitCts?.Cancel();
        _sitCts = new CancellationTokenSource();

        SitSequence(seat.seatTransform, seat).Forget();
    }

    private async UniTask SitSequence(Transform seatTf, Seat seat)
    {
        var token = _sitCts.Token;

        // Fixed: No .WithY()
        Vector3 seatForward = seatTf.forward;
        seatForward.y = 0f;
        if (seatForward.sqrMagnitude < 0.0001f)
            seatForward = transform.forward;

        seatForward.Normalize();

        Vector3 seatPos = seatTf.position;
        Vector3 preSitPoint = seatPos + seatForward * sitPreSitForwardOffset;
        Vector3 backstepPoint = seatPos + seatForward * sitBackstepDistance;

        bool allowLook = !seat.gameObject.CompareTag("EllsworthOfficeChair");
        GameMaster.Instance.INAMEETING = seat.gameObject.CompareTag("EllsworthOfficeChair");

        try
        {
            await SitPhaseApproach(preSitPoint, token);
            if (seatTf == null) throw new OperationCanceledException();

            await SitPhaseAlign(seatForward, token);
            if (seatTf == null) throw new OperationCanceledException();

            await SitPhaseBackstep(backstepPoint, seatForward, token);
            if (seatTf == null) throw new OperationCanceledException();

            transform.rotation = Quaternion.LookRotation(seatForward, Vector3.up);

            if (Noranimator != null)
            {
                Noranimator.ResetTrigger(AnimSitDown);
                Noranimator.SetTrigger(AnimSitDown);
            }

            await UniTask.WaitForSeconds(sitDownDuration, cancellationToken: token);

            if (seatTf != null)
            {
                transform.position = seatTf.position + sitSeatedRootOffset;
                transform.rotation = Quaternion.LookRotation(seatForward, Vector3.up);
            }

            if (Noranimator != null)
            {
                Noranimator.ResetTrigger(AnimSitIdle);
                Noranimator.SetTrigger(AnimSitIdle);
            }

            float seatFacingYaw = Quaternion.LookRotation(seatForward, Vector3.up).eulerAngles.y;
            FirstPersonLook?.SetSeated(true, seatFacingYaw, allowLook);

            if (!allowLook) DirectorEvents.StartDirector(DirectedRoutines.MainNoraFired);

            await UniTask.WaitForSeconds(1f, cancellationToken: token);

            IsSeated = true;
        }
        catch (OperationCanceledException)
        {
            // Sit was aborted
        }
        finally
        {
            _sitCts?.Dispose();
            _sitCts = null;
        }
    }

    private async UniTask SitPhaseApproach(Vector3 target, CancellationToken token)
    {
        float floorY = transform.position.y;

        while (!token.IsCancellationRequested)
        {
            float dist = Vector2.Distance(
                new Vector2(target.x, target.z), 
                new Vector2(transform.position.x, transform.position.z));

            if (dist <= sitArriveDistance) return;

            Vector3 dir = target - transform.position;
            dir.y = 0f;
            dir.Normalize();

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir, Vector3.up), 
                Time.deltaTime * sitRotateSpeed);

            Vector3 next = transform.position + dir * sitMoveSpeed * Time.deltaTime;
            next.y = floorY;
            transform.position = next;

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    private async UniTask SitPhaseAlign(Vector3 targetFacing, CancellationToken token)
    {
        Quaternion targetRot = Quaternion.LookRotation(targetFacing, Vector3.up);

        while (!token.IsCancellationRequested)
        {
            if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetRot.eulerAngles.y)) <= sitAlignToleranceDeg)
                break;

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * sitRotateSpeed * 1.5f);
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        transform.rotation = targetRot;
    }

    private async UniTask SitPhaseBackstep(Vector3 backstepTarget, Vector3 seatFacing, CancellationToken token)
    {
        float floorY = transform.position.y;
        Quaternion faceRot = Quaternion.LookRotation(seatFacing, Vector3.up);

        while (!token.IsCancellationRequested)
        {
            float dist = Vector2.Distance(
                new Vector2(backstepTarget.x, backstepTarget.z), 
                new Vector2(transform.position.x, transform.position.z));

            if (dist <= 0.06f) return;

            transform.rotation = Quaternion.Slerp(transform.rotation, faceRot, 
                Time.deltaTime * sitRotateSpeed * 2f);

            // Fixed: Replaced .WithY(0) with manual Y = 0
            Vector3 dir = backstepTarget - transform.position;
            dir.y = 0f;
            dir.Normalize();

            Vector3 next = transform.position + dir * sitBackstepSpeed * Time.deltaTime;
            next.y = floorY;
            transform.position = next;

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }
    private void AbortSit()
    {
        Debug.LogWarning("[Player] SitSequence aborted (seat transform was destroyed).");
        ReturnToLocomotion();
        GameMaster.Instance.PLAYERBUSY = false;
    }

    public void StandUp(InputAction.CallbackContext ctx = default)
    {
        if (!IsSeated) return;
        if (GameMaster.Instance.INAMEETING) return;

        _sitCts?.Cancel();

        StandUpSequence().Forget();
    }

    private async UniTask StandUpSequence()
    {
        if (Noranimator != null)
        {
            Noranimator.ResetTrigger(AnimStandUp);
            Noranimator.SetTrigger(AnimStandUp);
        }

        await UniTask.WaitForSeconds(standUpDuration);

        ReturnToLocomotion();
        GameMaster.Instance.PLAYERBUSY = false;
    }

    private void ReturnToLocomotion()
    {
        _activeSeatTransform = null;

        if (thisCharController != null)
            thisCharController.enabled = true;

        FirstPersonLook?.SetSeated(false);

        moveDirection.y = 0f;

        if (Noranimator != null)
        {
            Noranimator.SetBool(AnimCrouching, crouching);
            Noranimator.SetBool(AnimGrounded, true);
            Noranimator.SetFloat(AnimSpeed, 0f);
            Noranimator.SetFloat(AnimMoveX, 0f);
            Noranimator.SetFloat(AnimMoveY, 0f);

            Noranimator.ResetTrigger(AnimSitDown);
            Noranimator.ResetTrigger(AnimSitIdle);
            Noranimator.ResetTrigger(AnimStandUp);
        }

        IsSeated = false;
    }
    

    
    // ────────────────────────────────────────────────────────────────────────
    // Movement
    // ────────────────────────────────────────────────────────────────────────
    private void HandleMovement()
    {
        if (lockMovementDuringMelee && Time.time < _moveLockedUntil)
        {
            UpdateAnimator(isGrounded: thisCharController != null && thisCharController.isGrounded);
            return;
        }

        moveInput = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;

        Transform moveBasis = FirstPersonLook != null && FirstPersonLook.character != null ? FirstPersonLook.character : transform;

        Vector3 moveForward = moveBasis.forward;
        Vector3 moveRight   = moveBasis.right;

        moveForward.y = 0f;
        moveRight.y   = 0f;

        moveForward.Normalize();
        moveRight.Normalize();

        Vector3 desiredMove = (moveForward * moveInput.y) + (moveRight * moveInput.x);
        desiredMove.Normalize();

        speed = crouching ? walkspeed : (walking ? walkspeed : sprintspeed);

        if (climbing)
        {
            Vector3 climbMove = Vector3.zero;

            if (climbUpAction   != null && climbUpAction.action.ReadValue<float>()   > 0f) climbMove += Vector3.up * speed;
            if (climbDownAction != null && climbDownAction.action.ReadValue<float>() > 0f) climbMove -= Vector3.up * speed;

            if (thisCharController != null)
                thisCharController.Move(climbMove * Time.deltaTime);

            UpdateAnimator(isGrounded: false);
            return;
        }

        if (thisCharController == null)
        {
            UpdateAnimator(isGrounded: true);
            return;
        }

        bool groundedBefore = thisCharController.isGrounded;

        if (groundedBefore)
        {
            if (moveDirection.y < 0f) moveDirection.y = -2f;

            if (jumpRequested)
            {
                moveDirection.y     = jumpForce;
                jumpRequested       = false;
                _forceUngroundedUntil = Time.time + groundedFalseAfterJumpSeconds;
            }
        }

        thisCharController.Move(desiredMove * speed * Time.deltaTime);

        moveDirection.y += Physics.gravity.y * Time.deltaTime;
        thisCharController.Move(moveDirection * Time.deltaTime);

        bool groundedAfter = thisCharController.isGrounded;
        bool animGrounded  = groundedAfter;
        if (Time.time < _forceUngroundedUntil) animGrounded = false;

        UpdateAnimator(isGrounded: animGrounded);
        UpdatePeripheryFromAnimator();
    }

    private void UpdateAnimator(bool isGrounded)
    {
        if (Noranimator == null) return;

        float dt = Time.deltaTime;

        // Calculate manual velocity (already smoothed externally if you applied that tip)
        ComputeManualVelocityXZ();

        Vector3 vel = _manualVelocityXZ;
        float speed = vel.magnitude;

        // Movement threshold to prevent idle jitter
        float moveThreshold = 1.5f;
        bool isMoving = speed > moveThreshold;

        // Convert velocity to local space for blend tree
        Vector3 localDir = Vector3.zero;
        if (isMoving && _animT != null) localDir = _animT.InverseTransformDirection(vel.normalized);

        float targetX = isMoving ? Mathf.Clamp(localDir.x, -1f, 1f) : 0f;
        float targetY = isMoving ? Mathf.Clamp(localDir.z, -1f, 1f) : 0f;

        // Set animator states
        Noranimator.SetBool(AnimCrouching, crouching);
        Noranimator.SetBool(AnimGrounded, isGrounded);

        Noranimator.SetFloat(AnimMoveX, targetX, animDampTime, dt);
        Noranimator.SetFloat(AnimMoveY, targetY, animDampTime, dt);

        // Speed blending (idle / walk / run)
        float targetSpeed01 = 0f;
        if (isMoving)
        {
            if (crouching) targetSpeed01 = 0.5f;
            else           targetSpeed01 = walking ? 0.5f : 1f;
        }

        Noranimator.SetFloat(AnimSpeed, targetSpeed01, animDampTime, dt);
    }

    private Vector3 GetWorldPositionForVelocity()
        => thisCharController != null ? thisCharController.transform.position : transform.position;

    private void ComputeManualVelocityXZ()
    {
        Vector3 now = GetWorldPositionForVelocity();
        float   dt  = Time.deltaTime;

        if (dt <= 0.000001f)
        {
            _manualVelocityXZ = Vector3.zero;
            _lastWorldPos     = now;
            return;
        }

        Vector3 delta = now - _lastWorldPos;
        delta.y = 0f;

        _manualVelocityXZ = delta / dt;
        _lastWorldPos     = now;
    }

    // ────────────────────────────────────────────────────────────────────────
    // UpperBody layer helpers
    // ────────────────────────────────────────────────────────────────────────
    private void StopUpperBodyBlend()
    {
        if (_upperBodyBlendCo != null)
        {
            StopCoroutine(_upperBodyBlendCo);
            _upperBodyBlendCo = null;
        }
    }

    private void SetUpperBodyWeight(float w)
    {
        if (Noranimator == null || _upperBodyLayerIndex < 0) return;
        Noranimator.SetLayerWeight(_upperBodyLayerIndex, Mathf.Clamp01(w));
    }

    private async UniTask BlendUpperBodyWeight(float from, float to, float seconds, CancellationToken token)
    {
        if (Noranimator == null || _upperBodyLayerIndex < 0) return;

        if (seconds <= 0f)
        {
            SetUpperBodyWeight(to);
            return;
        }

        float t = 0f;
        while (t < seconds && !token.IsCancellationRequested)
        {
            t += Time.deltaTime;
            SetUpperBodyWeight(Mathf.Lerp(from, to, t / seconds));
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        if (!token.IsCancellationRequested)
            SetUpperBodyWeight(to);
    }

    private void RefreshUpperBodyWeight()
    {
        // ... existing logic to decide target weight ...

        bool shouldBeOn = _upperBodyHeldByMelee || _upperBodyHeldByPhone;
        float current = Noranimator?.GetLayerWeight(_upperBodyLayerIndex) ?? 0f;
        float target = shouldBeOn ? 1f : 0f;

        _upperBodyCts?.Cancel();
        _upperBodyCts = new CancellationTokenSource();

        BlendUpperBodyWeight(current, target, shouldBeOn ? upperBodyBlendIn : upperBodyBlendOut, _upperBodyCts.Token).Forget();
    }


    // ────────────────────────────────────────────────────────────────────────
    // Torch suppression (crawl)
    // ────────────────────────────────────────────────────────────────────────
    private void UpdatePeripheryFromAnimator()
    {
        if (PlayerTorch == null || Noranimator == null) return;

        bool isCrawling = IsClipActiveOnLayer(Noranimator, crawlLayerIndex, crawlClipName, crawlWeightThreshold);

        if (isCrawling != _lastIsCrawling)
        {
            FirstPersonLook?.SetCrawl(isCrawling);
            _lastIsCrawling = isCrawling;
        }

        if (isCrawling)
        {
            if (!_torchSuppressedByCrawl)
            {
                PlayerTorch.SetActive(false);
                _torchSuppressedByCrawl = true;
            }

            if (PlayerPhone.phoneMeshRenderer.enabled)
            {
                if (!GameMaster.Instance.DialogueManager.DialogInProgress)
                    GameMaster.Instance.DialogueManager.NewDialogue(DialogueName.NoraCantCrawlAndPhone, 5);

                PlayerPhone.PutAwayPhone();
            }
        }
        else
        {
            if (_torchSuppressedByCrawl)
            {
                PlayerTorch.SetActive(true);
                _torchSuppressedByCrawl = false;
            }
        }
    }

    private static bool IsClipActiveOnLayer(Animator anim, int layer, string clipName, float weightThreshold)
    {
        var current = anim.GetCurrentAnimatorClipInfo(layer);
        for (int i = 0; i < current.Length; i++)
        {
            var c = current[i].clip;
            if (c != null && c.name == clipName && current[i].weight >= weightThreshold) return true;
        }

        if (anim.IsInTransition(layer))
        {
            var next = anim.GetNextAnimatorClipInfo(layer);
            for (int i = 0; i < next.Length; i++)
            {
                var c = next[i].clip;
                if (c != null && c.name == clipName && next[i].weight >= weightThreshold) return true;
            }
        }

        return false;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Melee
    // ────────────────────────────────────────────────────────────────────────
    public void TryMelee()
    {
        if (!CombatEnabled) return;
        if (Noranimator == null) return;
        if (GameMaster.Instance != null && GameMaster.Instance.PLAYERBUSY && !MoveOverride) return;
        if (GameMaster.Instance != null && GameMaster.Instance.PauseManager != null &&
            GameMaster.Instance.PauseManager.IsPaused) return;
        if (climbing) return;
        if (Time.time < _nextMeleeTime) return;

        _nextMeleeTime = Time.time + meleeCooldown;

        _upperBodyHeldByMelee = true;
        RefreshUpperBodyWeight();

        Noranimator.ResetTrigger(AnimMelee);
        Noranimator.SetTrigger(AnimMelee);

        if (lockMovementDuringMelee)
            _moveLockedUntil = Time.time + lockMoveSeconds;
    }

    private void OnMelee(InputAction.CallbackContext ctx) => TryMelee();

    // ────────────────────────────────────────────────────────────────────────
    // Phone
    // ────────────────────────────────────────────────────────────────────────
    public void TogglePhone(bool putaway)
    {
        if (Noranimator == null) return;
        if (GameMaster.Instance != null && GameMaster.Instance.PauseManager != null &&
            GameMaster.Instance.PauseManager.IsPaused) return;

        _upperBodyHeldByPhone = true;
        RefreshUpperBodyWeight();

        if (putaway)
        {
            Noranimator.SetBool(AnimPhoneOut, false);
            Noranimator.ResetTrigger(AnimPhoneOut);
            Noranimator.ResetTrigger(AnimPhoneOutCrouch);
            return;
        }

        if (crouching)
        {
            Debug.Log("crouch phone");
            Noranimator.SetTrigger(AnimPhoneOutCrouch);
        }
        else
        {
            Noranimator.SetTrigger(AnimPhoneOut);
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    // Notepad
    // ────────────────────────────────────────────────────────────────────────
    public void ToggleNotepad(bool putaway)
    {
        if (Noranimator == null) return;
        if (GameMaster.Instance != null && GameMaster.Instance.PauseManager != null &&
            GameMaster.Instance.PauseManager.IsPaused) return;

        _upperBodyHeldByPhone = true;
        RefreshUpperBodyWeight();

        if (putaway) Noranimator.SetBool(AnimNotepadOut, false);
        else         Noranimator.SetBool(AnimNotepadOut, true);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Jump / Crouch
    // ────────────────────────────────────────────────────────────────────────
    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (GameMaster.Instance != null && GameMaster.Instance.PLAYERBUSY && !MoveOverride)
        {
            jumpRequested = false;
            return;
        }

        if (climbing) return;
        if (crouching) return;
        if (thisCharController == null) return;
        if (!thisCharController.isGrounded) return;

        if (Me.currentOutfit is OutfitType.Wedding or OutfitType.Funeral)
        {
            if (!GameMaster.Instance.DialogueManager.DialogInProgress) GameMaster.Instance.DialogueManager.NewDialogue(DialogueName.NoraNotJumpingInThis, 5);
            return;
        }
        

        Noranimator?.SetTrigger(AnimJump);
        jumpRequested = true;
    }

    private void OnCrouchToggle(InputAction.CallbackContext ctx)
    {
        if (climbing) return;
        if (PlayerPhone.CameraOpen) return;
        if (GameMaster.Instance != null && GameMaster.Instance.PLAYERBUSY && !MoveOverride) return;
        if (thisCharController == null) return;

        if (!crouching) { Crouch(); return; }

        if (!CanStandUp())
        {
            if (debugAnim) Debug.Log("[Crouch] Can't stand up: blocked overhead.");
            return;
        }

        Uncrouch();
    }

    public void Crouch()
    {
        crouching = true;
        speed     = walkspeed;

        if (stanceimg != null) stanceimg.sprite = crouchsprite;

        if (thisCharController != null && _controllerBaselineCaptured) StartCrouchControllerBlend(toCrouch: true);

        FirstPersonLook?.SetCrouch(true);
        Noranimator?.SetBool(AnimCrouching, true);
        EventManager.Crouch();
    }

    public void Uncrouch()
    {
        crouching = false;

        if (stanceimg != null) stanceimg.sprite = standsprite;

        if (thisCharController != null && _controllerBaselineCaptured) StartCrouchControllerBlend(toCrouch: false);

        FirstPersonLook?.SetCrouch(false);
        Noranimator?.SetBool(AnimCrouching, false);
        EventManager.UnCrouch();
    }

    private void StartCrouchControllerBlend(bool toCrouch)
    {
        _crouchCts?.Cancel();
        _crouchCts = new CancellationTokenSource();

        BlendControllerHeight(toCrouch ? croucheight : standheight, crouchBlendSeconds, _crouchCts.Token).Forget();
    }

    private async UniTask BlendControllerHeight(float targetHeight, float duration, CancellationToken token)
    {
        if (thisCharController == null) return;

        float startHeight = thisCharController.height;
        Vector3 startCenter = thisCharController.center;
        float bottom = _bottomLocalOffset;

        float t = 0f;

        while (t < duration && !token.IsCancellationRequested)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / duration);
            float h = Mathf.Lerp(startHeight, targetHeight, a);
            float cy = bottom + (h * 0.5f);

            Vector3 c = startCenter;
            c.y = Mathf.Lerp(startCenter.y, cy, a);

            thisCharController.height = h;
            thisCharController.center = c;

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        if (!token.IsCancellationRequested)
        {
            thisCharController.height = targetHeight;
            Vector3 finalC = thisCharController.center;
            finalC.y = bottom + (targetHeight * 0.5f);
            thisCharController.center = finalC;
        }

        thisCharController.Move(Vector3.zero);
    }

    private void ApplyControllerHeightKeepBottom(float newHeight)
    {
        if (thisCharController == null) return;

        thisCharController.height = newHeight;

        Vector3 c = thisCharController.center;
        c.y = _bottomLocalOffset + (newHeight * 0.5f);
        thisCharController.center = c;

        thisCharController.Move(Vector3.zero);
    }

    private bool CanStandUp()
    {
        if (thisCharController == null || !_controllerBaselineCaptured) return true;

        int playerLayer = thisCharController.gameObject.layer;
        int mask        = standCheckMask & ~(1 << playerLayer);

        float h = Mathf.Max(0.2f, standheight);
        float r = Mathf.Max(0.01f, thisCharController.radius - standCheckShrink);

        float   centerY     = _bottomLocalOffset + (h * 0.5f);
        Vector3 localCenter = thisCharController.center;
        localCenter.y       = centerY;

        Vector3 centerWorld = thisCharController.transform.TransformPoint(localCenter);
        float   segment     = Mathf.Max(0f, h - (2f * r));
        Vector3 p1          = centerWorld + Vector3.up * (segment * 0.5f);
        Vector3 p2          = centerWorld - Vector3.up * (segment * 0.5f);

        return !Physics.CheckCapsule(p2, p1, r, mask, QueryTriggerInteraction.Ignore);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UI / Death
    // ────────────────────────────────────────────────────────────────────────
    public void DisableAllScreens()
    {
        CrossHair.alpha = 0f;
        CrouchIndicator.alpha = 0f;
        TorchIndicator.alpha = 0f;
        EvidenceCompanion.alpha = 0f;
        PaperScreenFader.alpha = 0f;
        DeathScreenMain.alpha = 0f;
        DeathScreenMain.blocksRaycasts = false;
        DeathScreenFader.alpha = 0f;
        ButtonFaderLeave.alpha = 0f;
        ButtonFaderContinue.alpha = 0f;
        DiedTextFader.alpha = 0f;
        ButtonFaderContinue.blocksRaycasts = false;
        ButtonFaderLeave.blocksRaycasts = false;
    }

    public void CauseDeath(string cause)
    {
        GameMaster.Instance.PLAYERBUSY = true;
        StartCoroutine(SlowDeath(cause));
    }

    private IEnumerator SlowDeath(string CauseString)
    {
        DisableAllScreens();

        string buildDate = System.DateTime.Now.ToString("dddd") + ", " +
                           System.DateTime.Now.ToString("MMMM d") +
                           MonthDay(System.DateTime.Now.ToString("dd")) + ", " +
                           System.DateTime.Now.ToString("yyyy");

        PaperDeathText.text = CauseString + ".";
        PaperDateText.text  = buildDate;

        DeathScreenMain.alpha = 1f;
        DeathScreenMain.blocksRaycasts = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        int duration = 100, diedDuration = 50, paperDuration = 50, buttonDuration = 50;

        while (duration     > 0) { DeathScreenFader.alpha += 0.01f; yield return new WaitForSeconds(0.01f); duration--; }
        while (diedDuration > 0) { DiedTextFader.alpha    += 0.02f; yield return new WaitForSeconds(0.02f); diedDuration--; }
        while (paperDuration > 0){ PaperScreenFader.alpha += 0.02f; yield return new WaitForSeconds(0.02f); paperDuration--; }

        while (buttonDuration > 0)
        {
            ButtonFaderContinue.blocksRaycasts = true;
            ButtonFaderLeave.blocksRaycasts    = true;
            ButtonFaderContinue.alpha += 0.02f;
            ButtonFaderLeave.alpha    += 0.02f;
            yield return new WaitForSeconds(0.02f);
            buttonDuration--;
        }

        Uncrouch();
    }

    private string MonthDay(string day)
    {
        string nuNum = "th";
        int d = int.Parse(day);
        if (d < 11 || d > 20)
        {
            day = day[day.Length - 1].ToString();
            switch (day)
            {
                case "1": nuNum = "st"; break;
                case "2": nuNum = "nd"; break;
                case "3": nuNum = "rd"; break;
            }
        }
        return nuNum;
    }
}
