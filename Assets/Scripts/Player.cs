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

    
    public float jumpForce = 9f;                    
    public float jumpForwardSpeed = 6.5f;         
    public float jumpCooldown = 0.55f;             
    private float _jumpCooldownUntil;

    private bool _isJumping;
    private Vector3 _jumpForwardDirection;
    private Coroutine _jumpRoutine;
    
    public float jumpControllerHeightBoost = 1.4f;
    public float jumpControllerBlendTime = 0.18f;
    private Coroutine _controllerJumpBlendRoutine;
    
    private Vector2 moveInput;
    
    private CharacterController thisCharController;

    public Camera FirstPersonCamera;
    public Camera ThirdPersonCamera;
    public Camera DebugCam;
    public Camera CurrentCamera;

    public ConcentricCircleDriver concentricCircleDriver;
    
    public float RayCastDistance = 4f;
    
    
    [Header("Animation")]
    public Animator Noranimator;

    private static readonly int AnimMoveX      = Animator.StringToHash("MoveX");
    private static readonly int AnimMoveY      = Animator.StringToHash("MoveY");
    private static readonly int AnimSpeed      = Animator.StringToHash("Speed");
    private static readonly int AnimCrouching  = Animator.StringToHash("Crouching");
    private static readonly int AnimJump       = Animator.StringToHash("JUMP");
    private static readonly int AnimGrounded   = Animator.StringToHash("Grounded");

    private static readonly int AnimMelee      = Animator.StringToHash("MELEE");

    private static readonly int AnimPhoneOut      = Animator.StringToHash("PHONEOUT");
    private static readonly int AnimNotepadOut    = Animator.StringToHash("NOTEPADOUT");
    private static readonly int AnimPhoneOutCrouch = Animator.StringToHash("PHONEOUTCROUCH");

    private static readonly int AnimSitDown = Animator.StringToHash("SitDown");
    private static readonly int AnimSitIdle = Animator.StringToHash("SitIdle");
    private static readonly int AnimStandUp = Animator.StringToHash("StandUp");

    private static readonly int AnimLieDown = Animator.StringToHash("LieDown");
    private static readonly int AnimLieIdle = Animator.StringToHash("LieIdle");
    private static readonly int AnimWakeUp  = Animator.StringToHash("WakeUp");
    private static readonly int AnimSpecialAttach  = Animator.StringToHash("LieAwake");

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

    private Vector3 _jumpHorizontalVelocity;

    [Header("Melee")] 
    public bool CombatEnabled;
    public InputActionReference meleeAction;
    public float meleeCooldown = 0.55f;
    public bool lockMovementDuringMelee = false;
    public float lockMoveSeconds = 0.25f;

    private float _nextMeleeTime;
    private float _moveLockedUntil;

    [Header("UpperBody Layer Gating")]
    public string upperBodyLayerName = "UpperBody";
    public float upperBodyBlendIn  = 0.03f;
    public float upperBodyBlendOut = 0.06f;

    private int      _upperBodyLayerIndex = -1;
    private Coroutine _upperBodyBlendCo;

    private bool _upperBodyHeldByMelee;
    private bool _upperBodyHeldByPhone;

    public UpperBodyPitch UpperBodyPitch;
    
    [Header("Phone (UpperBody)")]
    public string upperBodyPhoneOutStateName  = "PHONE OUT";
    public string upperBodyPhoneAwayStateName = "PHONE AWAY";

    private int _phoneOutStateHash;
    private int _phoneAwayStateHash;

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

    [Header("Sleeping")]
    [Tooltip("Walk speed used when approaching a bed.")]
    public float sleepMoveSpeed = 3.5f;

    [Tooltip("Planar distance at which the approach phase is considered complete.")]
    public float sleepArriveDistance = 0.3f;

    [Tooltip("How fast Nora rotates to face the bed direction.")]
    public float sleepRotateSpeed = 7f;

    [Tooltip("Yaw tolerance (degrees) for the alignment phase.")]
    public float sleepAlignToleranceDeg = 5f;

    [Tooltip("Extra offset applied to root position when snapped to a lying position.")]
    public Vector3 sleepLieRootOffset = Vector3.zero;

    [Tooltip("Animator layer index that contains LieDown / LieIdle / WakeUp states.")]
    public int sleepAnimLayer = 0;

    [Tooltip("Duration of the LieDown clip – coroutine waits this long before entering LieIdle.")]
    public float lieDownDuration = 1.2f;

    [Tooltip("Duration of the WakeUp clip – coroutine waits this long before returning to locomotion.")]
    public float wakeUpDuration = 1.0f;

    [Tooltip("Yaw offset applied to the bed's foot-facing direction to get the player's lying orientation.")]
    public float sleepFacingYawOffset = -90f;

    
    public InputActionReference specialAttach;
    
    private Transform  _activeBedTransform;
    public bool       IsLying;

    // Cached at NoraSleep time so WakeUpSequence can return the player to the
    // bed's foot/approach point instead of leaving them standing on the bed.
    private Vector3 _sleepStandPosition;
    private Vector3 _sleepStandFacing;
    private bool    _sleepStandCaptured;

    private bool _wakingUp;
    private bool _standingUp;

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

    private int cameraEffectSelected = 0;

    private CancellationTokenSource _sitCts;
    private CancellationTokenSource _sleepCts;
    private CancellationTokenSource _crouchCts;
    private CancellationTokenSource _upperBodyCts;

    void Start()
    {
        CurrentCamera = FirstPersonCamera;

        EventManager.OnTorchCollected += TorchCollected;
        EventManager.OnNoraSit        += NoraSit;
        EventManager.OnNoraSleep      += NoraSleep;
        
        EventManager.OnDoLoadingSwirl += DoCameraLoadingThings;
        EventManager.OnUnDoLoadingSwirl += UnDoCameraLoadingThings;

        thisCharController = GetComponentInParent<CharacterController>();
        if (thisCharController == null) thisCharController = GetComponent<CharacterController>();
        

        if (Noranimator == null) Noranimator = GetComponentInChildren<Animator>(true);

        if (Noranimator != null)
        {
            _animT = Noranimator.transform;
            Noranimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            Noranimator.SetBool(AnimCrouching, crouching);
            Noranimator.SetBool(AnimGrounded, true);
            Noranimator.SetFloat(AnimSpeed, 0f);
            Noranimator.SetFloat(AnimMoveX, 0f);
            Noranimator.SetFloat(AnimMoveY, 0f);

            _upperBodyLayerIndex = Noranimator.GetLayerIndex(upperBodyLayerName);
            if (_upperBodyLayerIndex < 0 && debugAnim) Debug.LogWarning($"[Player] Animator has no layer named '{upperBodyLayerName}'.");

            _phoneOutStateHash = Animator.StringToHash(upperBodyPhoneOutStateName);
            _phoneAwayStateHash = Animator.StringToHash(upperBodyPhoneAwayStateName);

            Noranimator.ResetTrigger(AnimMelee);
            Noranimator.ResetTrigger(AnimPhoneOutCrouch);
            Noranimator.ResetTrigger(AnimPhoneOut);
            Noranimator.ResetTrigger(AnimLieDown);
            Noranimator.ResetTrigger(AnimLieIdle);
            Noranimator.ResetTrigger(AnimWakeUp);
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
        Vector3 spawnPoint = Vector3.zero;
        Vector3 spawnRotation = Vector3.zero;

        switch (GameMaster.Instance.THISLEVEL)
        {
            case GAMELEVEL.ETVStudio:
                spawnPoint = GameMaster.Instance.SPAWNPOINTETV;
                spawnRotation = GameMaster.Instance.SPAWNROTETV;
                break;
            
            case GAMELEVEL.NorasOldFlat:
                spawnPoint = GameMaster.Instance.SPAWNPOINTNORASOLDFLAT;
                spawnRotation = GameMaster.Instance.SPAWNROTNORASOLDFLAT;
                break;
            
            case GAMELEVEL.TrainStation:
                spawnPoint = GameMaster.Instance.SPAWNPOINTTRAINSTATION;
                spawnRotation = GameMaster.Instance.SPAWNROTTRAINSTATION;
                break;

            case GAMELEVEL.NorasFlat:
                spawnPoint = GameMaster.Instance.SPAWNPOINTNORASFLAT;
                spawnRotation = GameMaster.Instance.SPAWNROTNORASFLAT;
                break;

            case GAMELEVEL.TawleyMeats:
                spawnPoint = GameMaster.Instance.SPAWNPOINTTAWLEYMEATS;
                spawnRotation = GameMaster.Instance.SPAWNROTTAWLEYMEATS;
                break;

            case GAMELEVEL.RoarkOutside:
                spawnPoint = GameMaster.Instance.SPAWNPOINTROARKOUTSIDE;
                spawnRotation = GameMaster.Instance.SPAWNROTROARKOUTSIDE;
                break;

            case GAMELEVEL.RoarkInside:
                spawnPoint = GameMaster.Instance.SPAWNPOINTROARKINSIDE;
                spawnRotation = GameMaster.Instance.SPAWNROTROARKINSIDE;
                break;

            default:
                spawnPoint = GameMaster.Instance.SPAWNPOINTNORASFLAT;
                spawnRotation = GameMaster.Instance.SPAWNROTNORASFLAT;
                break;
        }

        bool ccWasEnabled = thisCharController != null && thisCharController.enabled;
        if (thisCharController != null) thisCharController.enabled = false;

        Transform rootTransform = thisCharController != null ? thisCharController.transform : transform;

        rootTransform.position = spawnPoint;
        rootTransform.rotation = Quaternion.Euler(spawnRotation);

        Physics.SyncTransforms();

        if (thisCharController != null) thisCharController.enabled = ccWasEnabled;

        GameMaster.Instance.NoraManager.IsDead = false;
        GameMaster.Instance.PLAYERBUSY = false;
        
        Debug.Log($"Spawned at {spawnPoint} with rotation {spawnRotation}");
    }

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

    
    private void DoCameraLoadingThings()
    {
        cameraEffectSelected = UnityEngine.Random.Range(0, 3);

        switch (cameraEffectSelected)
        {
            case 0:
                concentricCircleDriver.DoSwirl();
                break;

            case 1:
                concentricCircleDriver.DoZigzag();
                break;

            case 2:
                concentricCircleDriver.ZigZagSlideIn();
                break;
        }
    }

    private void UnDoCameraLoadingThings()
    {
        switch (cameraEffectSelected)
        {
            case 0:
                concentricCircleDriver.UndoSwirl();
                break;

            case 1:
                concentricCircleDriver.UndoZigzag();
                break;

            case 2:
                concentricCircleDriver.ZigZagSlideOut();
                break;
        }
    }

    private void EnterDebugCamera()
    {
        Debug.Log("debug cam on");

        if (FirstPersonCamera != null && FirstPersonCamera.enabled)
        {
            _previousGameplayCamera = FirstPersonCamera;
        }
        else if (ThirdPersonCamera != null && ThirdPersonCamera.enabled)
        {
            _previousGameplayCamera = ThirdPersonCamera;
        }

        if (FirstPersonCamera != null)
        {
            FirstPersonCamera.enabled = false;
            FirstPersonCamera.gameObject.GetComponent<Zoom>().enabled = false;
        }

        if (ThirdPersonCamera != null)
        {
            ThirdPersonCamera.enabled = false;
        }

        if (DebugCam != null)
        {
            DebugCam.gameObject.SetActive(true);
            DebugCam.enabled = true;
        }
        
    }
    
    private void ExitDebugCamera()
    {
        Debug.Log("debug cam off");

        if (DebugCam != null)
        {
            DebugCam.enabled = false;
            DebugCam.gameObject.SetActive(false);
        }

        if (_previousGameplayCamera != null)
        {
            _previousGameplayCamera.enabled = true;
            try
            {
                _previousGameplayCamera.gameObject.GetComponent<Zoom>().enabled = true;
            }
            catch
            {
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

    private void CaptureControllerBaseline()
    {
        if (thisCharController == null || _controllerBaselineCaptured) return;

        _standHeight = thisCharController.height;
        _standCenter = thisCharController.center;
        if (standheight <= 0f) standheight = _standHeight;
        _bottomLocalOffset = _standCenter.y - (_standHeight * 0.5f);
        _controllerBaselineCaptured = true;

        int playerLayer = thisCharController.gameObject.layer;
        standCheckMask &= ~(1 << playerLayer);

        if (crouching) ApplyControllerHeightKeepBottom(croucheight);
    }

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
        specialAttach?.action.Enable();
        StandUpAction?.action.Enable();

        if (jumpAction   != null) jumpAction.action.performed   += OnJump;
        if (crouchAction != null) crouchAction.action.performed += OnCrouchToggle;

        if (walkAction != null)
        {
            walkAction.action.performed += OnWalkPressed;
            walkAction.action.canceled  += OnWalkReleased;
        }

        if (meleeAction != null)
            meleeAction.action.performed += OnMelee;
        
        if (specialAttach != null) specialAttach.action.performed += OnSpecialattach;

        if (StandUpAction != null)
            StandUpAction.action.performed += StandUp;
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

        if (specialAttach != null) specialAttach.action.performed -= OnSpecialattach;

        if (StandUpAction != null)
            StandUpAction.action.performed -= StandUp;
    }

    private void OnWalkPressed(InputAction.CallbackContext ctx) => walking = true;
    private void OnWalkReleased(InputAction.CallbackContext ctx) => walking = false;

    void Update()
    {
        if (CurrentCamera == DebugCam) return;

        if (GameMaster.Instance != null && GameMaster.Instance.PauseManager != null &&
            GameMaster.Instance.PauseManager.IsPaused) return;

        if (IsSeated || IsLying) return;

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
        ZoomOverride = true;

        if (FirstPersonLook != null) FirstPersonLook.LockLook(true);
        if (thisCharController != null) thisCharController.enabled = false;

        _sitCts?.Cancel();
        _sitCts = new CancellationTokenSource();

        SitSequence(seat.seatTransform, seat).Forget();
    }

    private async UniTask SitSequence(Transform seatTf, Seat seat)
    {
        var token = _sitCts.Token;

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
        if (IsSeated)
        {
            if (GameMaster.Instance.INAMEETING) return;
            if (_standingUp) return;
            _standingUp = true;

            _sitCts?.Cancel();

            StandUpSequence().Forget();
            return;
        }

        if (IsLying)
        {
            if (_wakingUp) return;
            _wakingUp = true;

            _sleepCts?.Cancel();

            WakeUpSequence().Forget();
            return;
        }
        
        
        ZoomOverride = false;
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
        ZoomOverride = false;
        _standingUp = false;
    }

    private void ReturnToLocomotion()
    {
        _activeSeatTransform = null;

        if (thisCharController != null)
        {
            thisCharController.enabled = true;
            Physics.SyncTransforms();
        }

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

    public void NoraSleep(Bed bed)
    {
        if (bed == null || bed.bedLyingTransform == null || bed.bedFootTransform == null) return;
        if (IsLying) return;

        GameMaster.Instance.PLAYERBUSY = true;
        ZoomOverride = true;

        if (FirstPersonLook != null) FirstPersonLook.LockLook(true);
        if (thisCharController != null) thisCharController.enabled = false;

        _sleepCts?.Cancel();
        _sleepCts = new CancellationTokenSource();

        SleepSequence(bed).Forget();
    }

    private async UniTask SleepSequence(Bed bed)
    {
        var token = _sleepCts.Token;

        _activeBedTransform = bed.bedLyingTransform;

        Vector3 bedFacing = bed.bedFootTransform.position - bed.bedLyingTransform.position;
        bedFacing.y = 0f;
        if (bedFacing.sqrMagnitude < 0.0001f)
            bedFacing = transform.forward;

        bedFacing.Normalize();

        Vector3 lieFacing = Quaternion.Euler(0f, sleepFacingYawOffset, 0f) * bedFacing;

        Vector3 approachPoint = bed.bedFootTransform.position;

        // Cache where/which way to stand the player back up once they wake —
        // the bed's foot/approach point, facing the same direction used to
        // approach the bed originally — so WakeUpSequence doesn't leave them
        // standing on top of the bed.
        _sleepStandPosition = approachPoint;
        _sleepStandFacing   = bedFacing;
        _sleepStandCaptured = true;

        try
        {
            await SleepPhaseApproach(approachPoint, token);
            if (bed.bedLyingTransform == null) throw new OperationCanceledException();

            await SleepPhaseAlign(lieFacing, token);
            if (bed.bedLyingTransform == null) throw new OperationCanceledException();

            transform.rotation = Quaternion.LookRotation(lieFacing, Vector3.up);

            if (Noranimator != null)
            {
                Noranimator.ResetTrigger(AnimLieDown);
                Noranimator.SetTrigger(AnimLieDown);
            }

            await UniTask.WaitForSeconds(lieDownDuration, cancellationToken: token);

            if (bed.bedLyingTransform != null)
            {
                transform.position = bed.bedLyingTransform.position + sleepLieRootOffset;
                transform.rotation = Quaternion.LookRotation(lieFacing, Vector3.up);
            }

            if (Noranimator != null)
            {
                Noranimator.ResetTrigger(AnimLieIdle);
                Noranimator.SetTrigger(AnimLieIdle);
            }

            float lieFacingYaw = Quaternion.LookRotation(lieFacing, Vector3.up).eulerAngles.y;
            FirstPersonLook?.SetLying(true, lieFacingYaw, true);

            await UniTask.WaitForSeconds(1f, cancellationToken: token);

            IsLying = true;
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _sleepCts?.Dispose();
            _sleepCts = null;
        }
    }

    private async UniTask SleepPhaseApproach(Vector3 target, CancellationToken token)
    {
        float floorY = transform.position.y;

        while (!token.IsCancellationRequested)
        {
            float dist = Vector2.Distance(
                new Vector2(target.x, target.z),
                new Vector2(transform.position.x, transform.position.z));

            if (dist <= sleepArriveDistance) return;

            Vector3 dir = target - transform.position;
            dir.y = 0f;
            dir.Normalize();

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir, Vector3.up),
                Time.deltaTime * sleepRotateSpeed);

            Vector3 next = transform.position + dir * sleepMoveSpeed * Time.deltaTime;
            next.y = floorY;
            transform.position = next;

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    private async UniTask SleepPhaseAlign(Vector3 targetFacing, CancellationToken token)
    {
        Quaternion targetRot = Quaternion.LookRotation(targetFacing, Vector3.up);

        while (!token.IsCancellationRequested)
        {
            if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetRot.eulerAngles.y)) <= sleepAlignToleranceDeg)
                break;

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * sleepRotateSpeed * 1.5f);
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        transform.rotation = targetRot;
    }

    private async UniTask WakeUpSequence()
    {
        if (Noranimator != null)
        {
            Noranimator.ResetTrigger(AnimWakeUp);
            Noranimator.SetTrigger(AnimWakeUp);
        }

        await UniTask.WaitForSeconds(wakeUpDuration);

        ReturnFromLying();
        GameMaster.Instance.PLAYERBUSY = false;
        ZoomOverride = false;
        _wakingUp = false;
    }

    private void OnSpecialattach(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("dospesh");
        if (IsLying)
        {
            Noranimator.SetTrigger(AnimSpecialAttach);
        }
    }
    
    
    private void ReturnFromLying()
    {
        _activeBedTransform = null;

        // Move the player back to the bed's foot point (captured at NoraSleep
        // time) before re-enabling the controller, so they don't end up
        // standing on top of the bed.
        if (thisCharController != null)
            thisCharController.enabled = false;

        if (_sleepStandCaptured)
        {
            transform.position = _sleepStandPosition;
            transform.rotation = Quaternion.LookRotation(_sleepStandFacing, Vector3.up);
            _sleepStandCaptured = false;
        }

        Physics.SyncTransforms();

        if (thisCharController != null)
        {
            thisCharController.enabled = true;
            Physics.SyncTransforms();
        }

        FirstPersonLook?.SetLying(false);

        moveDirection.y = 0f;

        if (Noranimator != null)
        {
            Noranimator.SetBool(AnimCrouching, crouching);
            Noranimator.SetBool(AnimGrounded, true);
            Noranimator.SetFloat(AnimSpeed, 0f);
            Noranimator.SetFloat(AnimMoveX, 0f);
            Noranimator.SetFloat(AnimMoveY, 0f);

            Noranimator.ResetTrigger(AnimLieDown);
            Noranimator.ResetTrigger(AnimLieIdle);
            Noranimator.ResetTrigger(AnimWakeUp);
        }

        IsLying = false;
    }
    
    private void HandleMovement()
    {
        if (CurrentCamera == DebugCam) return;
        if (GameMaster.Instance?.PauseManager?.IsPaused == true) return;
        if (IsSeated || IsLying) return;
        if (thisCharController == null || !thisCharController.enabled) return;
        if (MoveOverride && moveAction != null && !moveAction.action.enabled)
            moveAction.action.Enable();

        if (GameMaster.Instance?.PLAYERBUSY == true && !MoveOverride) return;

        if (climbing)
        {
            Vector3 climbMove = Vector3.zero;
            if (climbUpAction != null && climbUpAction.action.ReadValue<float>() > 0f)
                climbMove += Vector3.up * walkspeed;
            if (climbDownAction != null && climbDownAction.action.ReadValue<float>() > 0f)
                climbMove -= Vector3.up * walkspeed;

            thisCharController.Move(climbMove * Time.deltaTime);
            UpdateAnimator(false);
            return;
        }

        moveInput = moveAction?.action.ReadValue<Vector2>() ?? Vector2.zero;

        if (FirstPersonLook != null && FirstPersonLook.IsThirdPersonCameraFront) moveInput = -moveInput;
        
        Transform moveBasis = FirstPersonLook?.character ?? transform;
        Vector3 moveForward = moveBasis.forward;
        Vector3 moveRight = moveBasis.right;
        moveForward.y = 0f;
        moveRight.y = 0f;
        moveForward.Normalize();
        moveRight.Normalize();

        Vector3 desiredMove = (moveForward * moveInput.y) + (moveRight * moveInput.x);
        if (desiredMove.sqrMagnitude > 1f) desiredMove.Normalize();

        bool isGrounded = thisCharController.isGrounded;

        Vector3 horizontalMove = Vector3.zero;

        if (!_isJumping && Time.time >= _jumpCooldownUntil)
        {
            horizontalMove = desiredMove;
        }

        thisCharController.Move(horizontalMove * walkspeed * Time.deltaTime);

        moveDirection.y += Physics.gravity.y * Time.deltaTime;
        thisCharController.Move(moveDirection * Time.deltaTime);

        UpdateAnimator(isGrounded: isGrounded && !_isJumping);
        UpdatePeripheryFromAnimator();
    }

    private void UpdateAnimator(bool isGrounded)
    {
        if (Noranimator == null) return;

        float dt = Time.deltaTime;

        ComputeManualVelocityXZ();

        Vector3 vel = _manualVelocityXZ;
        float speed = vel.magnitude;

        float moveThreshold = 0.5f;
        bool isMoving = speed > moveThreshold;

        Vector3 localDir = Vector3.zero;
        if (isMoving && _animT != null) localDir = _animT.InverseTransformDirection(vel.normalized);

        float targetX = isMoving ? Mathf.Clamp(localDir.x, -1f, 1f) : 0f;
        float targetY = isMoving ? Mathf.Clamp(localDir.z, -1f, 1f) : 0f;

        Noranimator.SetBool(AnimCrouching, crouching);
        Noranimator.SetBool(AnimGrounded, isGrounded);

        Noranimator.SetFloat(AnimMoveX, targetX, animDampTime, dt);
        Noranimator.SetFloat(AnimMoveY, targetY, animDampTime, dt);

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
        bool shouldBeOn = _upperBodyHeldByMelee || _upperBodyHeldByPhone;
        float current = Noranimator?.GetLayerWeight(_upperBodyLayerIndex) ?? 0f;
        float target = shouldBeOn ? 1f : 0f;

        _upperBodyCts?.Cancel();
        _upperBodyCts = new CancellationTokenSource();

        BlendUpperBodyWeight(current, target, shouldBeOn ? upperBodyBlendIn : upperBodyBlendOut, _upperBodyCts.Token).Forget();
    }

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

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (GameMaster.Instance != null && GameMaster.Instance.PLAYERBUSY && !MoveOverride) return;
        if (climbing || crouching || thisCharController == null) return;
        if (!thisCharController.isGrounded) return;
        if (Time.time < _jumpCooldownUntil) return;

        StartJump();
    }
    private void StartJump()
    {
        if (_isJumping) return;
        _isJumping = true;
        Noranimator?.SetTrigger(AnimJump);
    }

    public void OnJumpTakeoff()
    {
        Transform moveBasis = FirstPersonLook?.character ?? transform;
        _jumpForwardDirection = moveBasis.forward;
        _jumpForwardDirection.y = 0f;
        _jumpForwardDirection.Normalize();

        moveDirection.y = jumpForce;

        if (_controllerJumpBlendRoutine != null) StopCoroutine(_controllerJumpBlendRoutine);
        _controllerJumpBlendRoutine = StartCoroutine(BlendControllerForJump(true));

        if (_jumpRoutine != null) StopCoroutine(_jumpRoutine);
        _jumpRoutine = StartCoroutine(JumpSequence());
    }

    private IEnumerator JumpSequence()
    {
        yield return null;

        while (!thisCharController.isGrounded)
        {
            Vector3 airBoost = _jumpForwardDirection * jumpForwardSpeed * Time.deltaTime;
            thisCharController.Move(airBoost);
            yield return null;
        }

        _isJumping = false;

        if (_controllerJumpBlendRoutine != null) 
            StopCoroutine(_controllerJumpBlendRoutine);
        _controllerJumpBlendRoutine = StartCoroutine(BlendControllerForJump(false));

        moveDirection.x = 0f;
        moveDirection.z = 0f;
        moveDirection.y = -8f;

        _jumpCooldownUntil = Time.time + jumpCooldown;

        _jumpRoutine = null;
    }
    
    
    
    private IEnumerator BlendControllerForJump(bool isJumpingUp)
    {
        if (thisCharController == null) yield break;

        float startHeight = thisCharController.height;
        Vector3 startCenter = thisCharController.center;

        float targetHeight = isJumpingUp 
            ? _standHeight + jumpControllerHeightBoost 
            : _standHeight;

        Vector3 targetCenter = isJumpingUp 
            ? new Vector3(startCenter.x, startCenter.y + (jumpControllerHeightBoost * 0.5f), startCenter.z) 
            : _standCenter;

        float t = 0f;
        float duration = jumpControllerBlendTime;

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / duration);

            thisCharController.height = Mathf.Lerp(startHeight, targetHeight, a);
            thisCharController.center = Vector3.Lerp(startCenter, targetCenter, a);

            yield return null;
        }

        thisCharController.height = targetHeight;
        thisCharController.center = targetCenter;
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

    public void DisableAllScreens()
    {
        CrossHair.alpha = 0f;
        CrouchIndicator.alpha = 0f;
        TorchIndicator.alpha = 0f;
        EvidenceCompanion.alpha = 0f;
        PaperScreenFader.alpha = 0f;
        DeathScreenMain.alpha = 0f;
        DeathScreenMain.blocksRaycasts = false;
        DeathScreenMain.interactable = false;
        DeathScreenFader.alpha = 0f;
        ButtonFaderLeave.alpha = 0f;
        ButtonFaderContinue.alpha = 0f;
        DiedTextFader.alpha = 0f;
        ButtonFaderContinue.blocksRaycasts = false;
        ButtonFaderLeave.blocksRaycasts = false;
    }

    public void CauseDeath(string cause)
    {
        if (GameMaster.Instance.PauseManager.IsPaused) GameMaster.Instance.PauseManager.UnpauseGame();
        GameMaster.Instance.PLAYERBUSY = true;
        GameMaster.Instance.NoraManager.IsDead = true;
        Debug.Log("death caused, player busy: "+GameMaster.Instance.PLAYERBUSY);
        Debug.Log($"[CauseDeath] Writing PLAYERBUSY=true on GameMaster instance {GameMaster.Instance.GetInstanceID()}");
        StartCoroutine(SlowDeath(cause));
    }

    private IEnumerator SlowDeath(string CauseString)
    {
        DisableAllScreens();

        GameMaster.Instance.LoadingManager.SceneFadeOut();
        
        string buildDate = System.DateTime.Now.ToString("dddd") + ", " + System.DateTime.Now.ToString("MMMM d") + MonthDay(System.DateTime.Now.ToString("dd")) + ", " + System.DateTime.Now.ToString("yyyy");

        PaperDeathText.text = CauseString + ".";
        PaperDateText.text  = buildDate;

        DeathScreenMain.alpha = 1f;
        DeathScreenMain.blocksRaycasts = true;
        DeathScreenMain.interactable = true;

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
        
        PlayerStatus.AddDeath();
        

        GameMaster.Instance.NorasWardrobe.BurnOutfit();
    }



    public void Respawn()
    {
        DeathScreenMain.blocksRaycasts = false;
        DeathScreenMain.interactable = false;

        GameMaster.Instance.NorasWardrobe.SelectAndApplyOutfitForDeaths(PlayerStatus.NumberOfDeaths);

        StartCoroutine(SlowRespawn());
    }
    
    private IEnumerator SlowRespawn()
    {
        DisableAllScreens();
        
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yield return new WaitForSeconds(0.5f);
        
        DeathScreenFader.alpha = DiedTextFader.alpha = PaperScreenFader.alpha = 0f;
        
        GameMaster.Instance.LoadingManager.SceneFadeIn();
        
        Uncrouch();
        Spawn();
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