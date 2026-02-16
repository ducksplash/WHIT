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
    public float jumpForce = 2;
    private float speed;
    private Vector2 moveInput;
    private CharacterController thisCharController;
    public Camera MainCam;
    public float RayCastDistance = 4;

    public bool crouching;
    public float croucheight;
    public float standheight;
    public Image stanceimg;
    public Sprite crouchsprite;
    public Sprite standsprite;

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

        thisCharController = GetComponent<CharacterController>();
        SpawnPoint = transform.position;
        speed = sprintspeed;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
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

        jumpAction.action.performed += OnJump;
        crouchAction.action.performed += OnCrouchToggle;
        walkAction.action.performed += ctx => walking = true;
        walkAction.action.canceled += ctx => walking = false;
    }

    void OnDisable()
    {
        jumpAction.action.performed -= OnJump;
        crouchAction.action.performed -= OnCrouchToggle;
    }

    void Update()
    {
        if (GameMaster.Instance.PauseManager.IsPaused) return;

        if (MoveOverride && moveAction != null && !moveAction.action.enabled) moveAction.action.Enable();

        if (GameMaster.Instance.PLAYERBUSY && !MoveOverride) return;

        HandleMovement();
    }


    private void HandleMovement()
    {
        moveInput = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;

        Vector3 forward = MainCam.transform.forward;
        Vector3 right = MainCam.transform.right;
        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();

        Vector3 desiredMove = forward * moveInput.y + right * moveInput.x;

        speed = crouching ? walkspeed : (walking ? walkspeed : sprintspeed);

        // ----- CLIMBING -----
        if (climbing)
        {
            Vector3 climbMove = Vector3.zero;

            if (climbUpAction != null && climbUpAction.action.ReadValue<float>() > 0f)
                climbMove += Vector3.up * speed;

            if (climbDownAction != null && climbDownAction.action.ReadValue<float>() > 0f)
                climbMove -= Vector3.up * speed;

            thisCharController.Move(climbMove * Time.deltaTime);
            return;
        }

        if (thisCharController.isGrounded)
        {
            if (moveDirection.y < 0)
                moveDirection.y = -2f;

            if (jumpRequested)
            {
                moveDirection.y = jumpForce;
                jumpRequested = false;
            }
        }


        // Horizontal movement
        thisCharController.Move(desiredMove * speed * Time.deltaTime);

        // Gravity (use real-world-ish gravity)
        moveDirection.y += Physics.gravity.y * Time.deltaTime;

        // Apply vertical motion last
        thisCharController.Move(moveDirection * Time.deltaTime);
    }


    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (GameMaster.Instance.PLAYERBUSY && !MoveOverride)
        {
            jumpRequested = false;
            return;
        }
        
        if (climbing) return;
        if (crouching) return;

        jumpRequested = true;
    }


    private void OnCrouchToggle(InputAction.CallbackContext ctx)
    {
        if (climbing) return;
        if (GameMaster.Instance.PLAYERBUSY && !MoveOverride) return;
        
        if (!crouching) Crouch();
        else
        {
            if (!Physics.Raycast(MainCam.transform.position, Vector3.up, 2.5f))
                Uncrouch();
        }
    }

    public void Crouch()
    {
        crouching = true;
        thisCharController.height = croucheight;
        speed = walkspeed;
        stanceimg.sprite = crouchsprite;
    }

    public void Uncrouch()
    {
        crouching = false;
        thisCharController.height = standheight;
        stanceimg.sprite = standsprite;
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
