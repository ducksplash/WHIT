using UnityEngine;

public class Jump : MonoBehaviour
{
    [SerializeField]
    public bool groundCheck;
    CharacterController thisChar;
    public float jumpStrength = 2;
    public event System.Action Jumped;
    private Vector3 moveDirection = Vector3.zero;
    public float jumpSpeed = 8.0F;
    public float gravity = 1F;



    void Awake()
    {
        thisChar = GetComponent<CharacterController>();
        groundCheck = true;
    }

    void FixedUpdate()
    {


        if (!FirstPersonCollision.FROZEN && groundCheck && (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.Space)))
        {
            moveDirection.y = jumpSpeed;
            groundCheck = false;

        }
        moveDirection.y -= gravity * Time.deltaTime;
        thisChar.Move(moveDirection * Time.deltaTime);



    }


    void OnControllerColliderHit(ControllerColliderHit hit)
    {
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
