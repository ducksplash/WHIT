using UnityEngine;

public class Jump : MonoBehaviour
{
    [SerializeField]
    GroundCheck groundCheck;
    CharacterController thisChar;
    public float jumpStrength = 2;
    public event System.Action Jumped;
    private Vector3 moveDirection = Vector3.zero;
    public float jumpSpeed = 8.0F;
    public float gravity = 1F;

    void Reset()
    {
        groundCheck = GetComponentInChildren<GroundCheck>();
        if (!groundCheck)
            groundCheck = GroundCheck.Create(transform);
    }

    void Awake()
    {
        thisChar = GetComponent<CharacterController>();
    }

    void FixedUpdate()
    {


        if (groundCheck.isGrounded && (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.Space)))
        {
            moveDirection.y = jumpSpeed;
        }
        moveDirection.y -= gravity * Time.deltaTime;
        thisChar.Move(moveDirection * Time.deltaTime);



    }
}
