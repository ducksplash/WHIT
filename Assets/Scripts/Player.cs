using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class Player : Singleton<Player>
{
    [Header("Movement & State")]
    public float walkspeed = 0.1f;
    public float sprintspeed = 0.2f;
    public float jumpForce = 2f;

    private float speed;
    private Vector2 moveInput;

    private CharacterController thisCharController;

    public Camera MainCam;
    public float RayCastDistance = 4f;

    [Header("Animation")]
    public Animator Noranimator;

    private static readonly int AnimMoveX = Animator.StringToHash("MoveX");
    private static readonly int AnimMoveY = Animator.StringToHash("MoveY");
    private static readonly int AnimSpeed = Animator.StringToHash("Speed");
    private static readonly int AnimCrouching = Animator.StringToHash("Crouching");
    private static readonly int AnimJump = Animator.StringToHash("JUMP");
    private static readonly int AnimGrounded = Animator.StringToHash("Grounded");

    // ✅ Melee trigger
    private static readonly int AnimMelee = Animator.StringToHash("MELEE");

    [Tooltip("How quickly animation parameters catch up (bigger = snappier).")]
    public float animDampTime = 0.12f;

    [Tooltip("Turn this on to print debug values.")]
    public bool debugAnim = false;

    private Transform _animT;

    private Vector3 _lastWorldPos;
    private Vector3 _manualVelocityXZ;

    [Header("Jump Tuning")]
    public float groundedFalseAfterJumpSeconds = 0.12f;
    private float _forceUngroundedUntil;

    // ------------------------------------------------------------
    // ✅ Melee (moved from Torch -> Player)
    // ------------------------------------------------------------
    [Header("Melee")]
    public InputActionReference meleeAction;   // bind your attack input here
    public float meleeCooldown = 0.55f;        // gameplay cooldown (NOT animation length)
    public bool lockMovementDuringMelee = false;
    public float lockMoveSeconds = 0.25f;

    private float _nextMeleeTime;
    private float _moveLockedUntil;

    // ------------------------------------------------------------
    // ✅ Animator layer gating (robust w/ Animation Event)
    // ------------------------------------------------------------
    [Header("UpperBody Layer Gating")]
    [Tooltip("Animator layer name used for melee (must match Animator layer name exactly).")]
    public string upperBodyLayerName = "UpperBody";

    [Tooltip("Seconds to blend UpperBody layer from 0->1 when attacking.")]
    public float upperBodyBlendIn = 0.03f;

    [Tooltip("Seconds to blend UpperBody layer from 1->0 when attack ends.")]
    public float upperBodyBlendOut = 0.06f;

    private int _upperBodyLayerIndex = -1;
    private Coroutine _upperBodyBlendCo;

    // ------------------------------------------------------------
    // Crouch (Controller Collider)
    // ------------------------------------------------------------
    [Header("Crouch (Controller Collider)")]
    public bool crouching;

    [Tooltip("Standing CharacterController height (will be captured from controller on Start if 0).")]
    public float standheight = 0f;

    [Tooltip("Crouching CharacterController height.")]
    public float croucheight = 1.0f;

    [Tooltip("How long to blend the CharacterController height/center (0 = instant).")]
    public float crouchBlendSeconds = 0.12f;

    [Tooltip("Collision layers used to test if we can stand up. EXCLUDE Player layer.")]
    public LayerMask standCheckMask = ~0;

    [Tooltip("Extra tolerance to avoid false positives (skin width/precision).")]
    public float standCheckShrink = 0.02f;

    public Image stanceimg;
    public Sprite crouchsprite;
    public Sprite standsprite;

    private float _standHeight;
    private Vector3 _standCenter;
    private float _bottomLocalOffset;
    private bool _controllerBaselineCaptured;

    private Coroutine _crouchCo;

    public GameObject TravelNotepad;
    public bool climbing;
    public GameObject LadderAttachedTo;

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
    public Vector3 SpawnPoint;
    public Transform playerHand;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference crouchAction;
    public InputActionReference walkAction;
    public InputActionReference climbUpAction;
    public InputActionReference climbDownAction;
    public InputActionReference exitLadderAction;

    [Header("Scripts")]
    public FirstPersonLook FirstPersonLook;
    public Phone PlayerPhone;

    private Vector3 moveDirection = Vector3.zero;
    private bool jumpRequested = false;

    private bool walking = false;

    public bool MoveOverride;
    public bool ZoomOverride;

    void Start()
    {
        thisCharController = GetComponentInParent<CharacterController>();
        if (thisCharController == null)
            thisCharController = GetComponent<CharacterController>();

        if (thisCharController == null)
            Debug.LogError("[Player] No CharacterController found on this object or its parents.");

        SpawnPoint = transform.position;
        speed = sprintspeed;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

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

            // Cache layer index (do NOT force weight here; we gate it only during melee)
            _upperBodyLayerIndex = Noranimator.GetLayerIndex(upperBodyLayerName);
            if (_upperBodyLayerIndex < 0 && debugAnim)
                Debug.LogWarning($"[Player] Animator has no layer named '{upperBodyLayerName}'.");
            else
                Noranimator.SetLayerWeight(_upperBodyLayerIndex, 0f); // ensure default is off
        }

        CaptureControllerBaseline();
        _lastWorldPos = GetWorldPositionForVelocity();
    }

    private void CaptureControllerBaseline()
    {
        if (thisCharController == null || _controllerBaselineCaptured) return;

        _standHeight = thisCharController.height;
        _standCenter = thisCharController.center;

        if (standheight <= 0f) standheight = _standHeight;

        // keep bottom constant: center.y - height/2
        _bottomLocalOffset = _standCenter.y - (_standHeight * 0.5f);

        _controllerBaselineCaptured = true;

        // sanity: if your mask includes player layer, fix it
        int playerLayer = thisCharController.gameObject.layer;
        standCheckMask &= ~(1 << playerLayer);

        if (crouching)
            ApplyControllerHeightKeepBottom(croucheight);
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

        if (jumpAction != null) jumpAction.action.performed += OnJump;
        if (crouchAction != null) crouchAction.action.performed += OnCrouchToggle;

        if (walkAction != null)
        {
            walkAction.action.performed += OnWalkPressed;
            walkAction.action.canceled += OnWalkReleased;
        }

        if (meleeAction != null)
            meleeAction.action.performed += OnMelee;
    }

    void OnDisable()
    {
        if (jumpAction != null) jumpAction.action.performed -= OnJump;
        if (crouchAction != null) crouchAction.action.performed -= OnCrouchToggle;

        if (walkAction != null)
        {
            walkAction.action.performed -= OnWalkPressed;
            walkAction.action.canceled -= OnWalkReleased;
        }

        if (meleeAction != null)
            meleeAction.action.performed -= OnMelee;
    }

    private void OnWalkPressed(InputAction.CallbackContext ctx) => walking = true;
    private void OnWalkReleased(InputAction.CallbackContext ctx) => walking = false;

    void Update()
    {
        if (GameMaster.Instance != null && GameMaster.Instance.PauseManager != null && GameMaster.Instance.PauseManager.IsPaused)
            return;

        if (MoveOverride && moveAction != null && !moveAction.action.enabled)
            moveAction.action.Enable();

        if (GameMaster.Instance != null && GameMaster.Instance.PLAYERBUSY && !MoveOverride)
            return;

        HandleMovement();
    }

    private void HandleMovement()
    {
        // ✅ optional movement lock during melee
        if (lockMovementDuringMelee && Time.time < _moveLockedUntil)
        {
            UpdateAnimator(isGrounded: thisCharController != null && thisCharController.isGrounded);
            return;
        }

        moveInput = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;

        Vector3 camForward = MainCam != null ? MainCam.transform.forward : transform.forward;
        Vector3 camRight = MainCam != null ? MainCam.transform.right : transform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 desiredMove = camForward * moveInput.y + camRight * moveInput.x;

        speed = crouching ? walkspeed : (walking ? walkspeed : sprintspeed);

        if (climbing)
        {
            Vector3 climbMove = Vector3.zero;

            if (climbUpAction != null && climbUpAction.action.ReadValue<float>() > 0f)
                climbMove += Vector3.up * speed;

            if (climbDownAction != null && climbDownAction.action.ReadValue<float>() > 0f)
                climbMove -= Vector3.up * speed;

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
            if (moveDirection.y < 0f)
                moveDirection.y = -2f;

            if (jumpRequested)
            {
                moveDirection.y = jumpForce;
                jumpRequested = false;
                _forceUngroundedUntil = Time.time + groundedFalseAfterJumpSeconds;
            }
        }

        thisCharController.Move(desiredMove * speed * Time.deltaTime);

        moveDirection.y += Physics.gravity.y * Time.deltaTime;
        thisCharController.Move(moveDirection * Time.deltaTime);

        bool groundedAfter = thisCharController.isGrounded;

        bool animGrounded = groundedAfter;
        if (Time.time < _forceUngroundedUntil)
            animGrounded = false;

        UpdateAnimator(isGrounded: animGrounded);
    }

    private void UpdateAnimator(bool isGrounded)
    {
        if (Noranimator == null) return;

        Vector3 velXZ = Vector3.zero;

        if (thisCharController != null)
        {
            Vector3 v = thisCharController.velocity;
            v.y = 0f;
            velXZ = v;
        }

        ComputeManualVelocityXZ();
        if (velXZ.sqrMagnitude < 0.000001f && _manualVelocityXZ.sqrMagnitude > 0.000001f)
            velXZ = _manualVelocityXZ;

        bool isMoving = velXZ.sqrMagnitude > 0.0001f;

        Vector3 localDir = Vector3.zero;
        if (isMoving && _animT != null)
            localDir = _animT.InverseTransformDirection(velXZ.normalized);

        float targetX = isMoving ? Mathf.Clamp(localDir.x, -1f, 1f) : 0f;
        float targetY = isMoving ? Mathf.Clamp(localDir.z, -1f, 1f) : 0f;

        Noranimator.SetBool(AnimCrouching, crouching);
        Noranimator.SetBool(AnimGrounded, isGrounded);

        float dt = Time.deltaTime;
        Noranimator.SetFloat(AnimMoveX, targetX, animDampTime, dt);
        Noranimator.SetFloat(AnimMoveY, targetY, animDampTime, dt);

        float targetSpeed01 = 0f;

        if (isMoving)
        {
            if (crouching)
                targetSpeed01 = 0.5f;
            else
                targetSpeed01 = walking ? 0.5f : 1f;
        }

        Noranimator.SetFloat(AnimSpeed, targetSpeed01, animDampTime, dt);
    }

    private Vector3 GetWorldPositionForVelocity()
    {
        return thisCharController != null ? thisCharController.transform.position : transform.position;
    }

    private void ComputeManualVelocityXZ()
    {
        Vector3 now = GetWorldPositionForVelocity();
        float dt = Time.deltaTime;

        if (dt <= 0.000001f)
        {
            _manualVelocityXZ = Vector3.zero;
            _lastWorldPos = now;
            return;
        }

        Vector3 delta = (now - _lastWorldPos);
        delta.y = 0f;

        _manualVelocityXZ = delta / dt;
        _lastWorldPos = now;
    }

    // ------------------------------------------------------------
    // ✅ UpperBody layer helpers
    // ------------------------------------------------------------
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
        if (Noranimator == null) return;
        if (_upperBodyLayerIndex < 0) return;
        Noranimator.SetLayerWeight(_upperBodyLayerIndex, Mathf.Clamp01(w));
    }

    private IEnumerator BlendUpperBodyWeight(float from, float to, float seconds)
    {
        if (Noranimator == null || _upperBodyLayerIndex < 0) yield break;

        if (seconds <= 0f)
        {
            SetUpperBodyWeight(to);
            yield break;
        }

        float t = 0f;
        while (t < seconds)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / seconds);
            float w = Mathf.Lerp(from, to, a);
            Noranimator.SetLayerWeight(_upperBodyLayerIndex, w);
            yield return null;
        }

        Noranimator.SetLayerWeight(_upperBodyLayerIndex, to);
    }

    // ------------------------------------------------------------
    // ✅ Melee (robust gating using Animation Event to end)
    // ------------------------------------------------------------
    public void TryMelee()
    {
        if (debugAnim) Debug.Log("TryMelee fired");

        if (Noranimator == null) return;

        if (GameMaster.Instance != null && GameMaster.Instance.PLAYERBUSY && !MoveOverride) return;
        if (GameMaster.Instance != null && GameMaster.Instance.PauseManager != null && GameMaster.Instance.PauseManager.IsPaused) return;

        if (climbing) return;
        // if (crouching) return;

        if (Time.time < _nextMeleeTime) return;
        _nextMeleeTime = Time.time + meleeCooldown;

        // Ensure we have layer index (in case controller swaps at runtime)
        if (_upperBodyLayerIndex < 0)
            _upperBodyLayerIndex = Noranimator.GetLayerIndex(upperBodyLayerName);

        // Blend layer ON, then trigger melee
        StopUpperBodyBlend();
        float current = (_upperBodyLayerIndex >= 0) ? Noranimator.GetLayerWeight(_upperBodyLayerIndex) : 0f;
        _upperBodyBlendCo = StartCoroutine(BlendUpperBodyWeight(current, 1f, upperBodyBlendIn));

        // Fire trigger immediately (layer will be ramping in)
        Noranimator.ResetTrigger(AnimMelee);
        Noranimator.SetTrigger(AnimMelee);

        if (lockMovementDuringMelee)
            _moveLockedUntil = Time.time + lockMoveSeconds;
    }

    private void OnMelee(InputAction.CallbackContext ctx)
    {
        TryMelee();
    }

    /// <summary>
    /// Call this from an Animation Event placed at the end of the melee clip.
    /// Animation Window -> select melee clip -> add event on last frame -> function: OnMeleeAnimFinished
    /// </summary>
    public void OnMeleeAnimFinished()
    {
        if (Noranimator == null) return;

        // Ensure we have layer index (in case controller swaps at runtime)
        if (_upperBodyLayerIndex < 0)
            _upperBodyLayerIndex = Noranimator.GetLayerIndex(upperBodyLayerName);

        StopUpperBodyBlend();

        float current = (_upperBodyLayerIndex >= 0) ? Noranimator.GetLayerWeight(_upperBodyLayerIndex) : 1f;
        _upperBodyBlendCo = StartCoroutine(BlendUpperBodyWeight(current, 0f, upperBodyBlendOut));
    }

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

        Noranimator?.SetTrigger(AnimJump);
        jumpRequested = true;
    }

    private void OnCrouchToggle(InputAction.CallbackContext ctx)
    {
        if (climbing) return;
        if (GameMaster.Instance != null && GameMaster.Instance.PLAYERBUSY && !MoveOverride) return;
        if (thisCharController == null) return;

        if (!crouching)
        {
            Crouch();
            return;
        }

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
        speed = walkspeed;

        if (stanceimg != null) stanceimg.sprite = crouchsprite;

        if (thisCharController != null && _controllerBaselineCaptured)
            StartCrouchControllerBlend(toCrouch: true);

        Noranimator?.SetBool(AnimCrouching, true);
    }

    public void Uncrouch()
    {
        crouching = false;

        if (stanceimg != null) stanceimg.sprite = standsprite;

        if (thisCharController != null && _controllerBaselineCaptured)
            StartCrouchControllerBlend(toCrouch: false);

        Noranimator?.SetBool(AnimCrouching, false);
    }

    // ------------------------------------------------------------
    // CharacterController crouch implementation (no sinking)
    // ------------------------------------------------------------

    private void StartCrouchControllerBlend(bool toCrouch)
    {
        if (_crouchCo != null)
        {
            StopCoroutine(_crouchCo);
            _crouchCo = null;
        }

        float targetHeight = toCrouch ? croucheight : standheight;
        targetHeight = Mathf.Max(0.2f, targetHeight);

        if (crouchBlendSeconds <= 0f)
        {
            ApplyControllerHeightKeepBottom(targetHeight);
            return;
        }

        _crouchCo = StartCoroutine(BlendControllerHeight(targetHeight, crouchBlendSeconds));
    }

    private IEnumerator BlendControllerHeight(float targetHeight, float duration)
    {
        if (thisCharController == null) yield break;

        float startHeight = thisCharController.height;
        Vector3 startCenter = thisCharController.center;

        float bottom = _bottomLocalOffset;

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / duration);

            float h = Mathf.Lerp(startHeight, targetHeight, a);
            float cy = bottom + (h * 0.5f);

            Vector3 c = startCenter;
            c.y = Mathf.Lerp(startCenter.y, cy, a);

            thisCharController.height = h;
            thisCharController.center = c;

            yield return null;
        }

        thisCharController.height = targetHeight;

        Vector3 finalC = thisCharController.center;
        finalC.y = bottom + (targetHeight * 0.5f);
        thisCharController.center = finalC;

        thisCharController.Move(Vector3.zero);

        _crouchCo = null;
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
        int mask = standCheckMask & ~(1 << playerLayer);

        float h = Mathf.Max(0.2f, standheight);
        float r = Mathf.Max(0.01f, thisCharController.radius);

        r = Mathf.Max(0.01f, r - standCheckShrink);

        float centerY = _bottomLocalOffset + (h * 0.5f);

        Transform t = thisCharController.transform;

        Vector3 localCenter = thisCharController.center;
        localCenter.y = centerY;

        Vector3 centerWorld = t.TransformPoint(localCenter);

        Vector3 up = Vector3.up;

        float segment = Mathf.Max(0f, h - (2f * r));
        Vector3 p1 = centerWorld + up * (segment * 0.5f);
        Vector3 p2 = centerWorld - up * (segment * 0.5f);

        bool blocked = Physics.CheckCapsule(p2, p1, r, mask, QueryTriggerInteraction.Ignore);
        return !blocked;
    }

    // --- your existing UI/death code unchanged below ---

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
                           System.DateTime.Now.ToString("MMMM d") + MonthDay(System.DateTime.Now.ToString("dd")) + ", " +
                           System.DateTime.Now.ToString("yyyy");

        PaperDeathText.text = CauseString + ".";
        PaperDateText.text = buildDate;

        DeathScreenMain.alpha = 1f;
        DeathScreenMain.blocksRaycasts = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        int duration = 100, diedDuration = 50, paperDuration = 50, buttonDuration = 50;

        while (duration > 0)
        {
            DeathScreenFader.alpha += 0.01f;
            yield return new WaitForSeconds(0.01f);
            duration--;
        }

        while (diedDuration > 0)
        {
            DiedTextFader.alpha += 0.02f;
            yield return new WaitForSeconds(0.02f);
            diedDuration--;
        }

        while (paperDuration > 0)
        {
            PaperScreenFader.alpha += 0.02f;
            yield return new WaitForSeconds(0.02f);
            paperDuration--;
        }

        while (buttonDuration > 0)
        {
            ButtonFaderContinue.blocksRaycasts = true;
            ButtonFaderLeave.blocksRaycasts = true;
            ButtonFaderContinue.alpha += 0.02f;
            ButtonFaderLeave.alpha += 0.02f;
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