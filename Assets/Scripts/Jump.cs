using UnityEngine;
using UnityEngine.InputSystem;

public class Jump : MonoBehaviour
{
    [SerializeField] public bool groundCheck;
    private CharacterController thisChar;

    public float jumpStrength = 2f;
    private Vector3 moveDirection = Vector3.zero;
    public float jumpSpeed = 8.0f;
    public float gravity = 1f;

    // New Input System reference
    public InputActionReference jumpAction;

    void Awake()
    {
        thisChar = GetComponent<CharacterController>();
        groundCheck = true;
    }

    void OnEnable()
    {
        if (jumpAction != null)
        {
            jumpAction.action.Enable();
            jumpAction.action.performed += OnJump;
        }
    }

    void OnDisable()
    {
        if (jumpAction != null)
        {
            jumpAction.action.performed -= OnJump;
        }
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (!Player.Instance.climbing && !GameMaster.Instance.FROZEN && groundCheck && !Player.Instance.crouching)
        {
            moveDirection.y = jumpSpeed;
            groundCheck = false;
        }
    }

    void FixedUpdate()
    {
        if (!Player.Instance.climbing)
        {
            // Apply gravity every frame
            moveDirection.y -= gravity * Time.deltaTime;
            thisChar.Move(moveDirection * Time.deltaTime);
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Set groundCheck if player hits a ground layer
        if (hit.gameObject.layer == 30)
        {
            groundCheck = true;
        }
        else
        {
            groundCheck = false;
        }
    }
}