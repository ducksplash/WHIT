using UnityEngine;

public class FirstPersonCollision : MonoBehaviour
{
	
	
	private Collider thePlayerCollider;

	public float speed = 0.1f;
	public float walkspeed = 0.1f;
	public float sprintspeed = 0.2f;
	Vector2 velocity;
	private Collision thisCollision;
	private bool didCollide = false;
	private CharacterController thisRB;
	private Camera MainCam;
	public static bool FROZEN;
	public bool crouching;
	public float croucheight;
	public float standheight;


	void Start()
	{
		
		thePlayerCollider = gameObject.GetComponent<CapsuleCollider>();
		thisRB = gameObject.GetComponent<CharacterController>();
		MainCam = Camera.main;

		

	}
	
	
	
	void OnCollisionEnter(Collision thisCollision)
    {
		if (thisCollision.transform.name.Contains("abbotoir"))
		{
			didCollide = true;
		}
		
    }	
	
	void OnCollisionExit(Collision thisCollision)
    {
		didCollide = false;
    }



    private void Update()
    {

		if (Input.GetKeyUp(KeyCode.RightControl))
		{

			if (!crouching)
			{
				crouching = true;
				thisRB.height = croucheight;
			}
			else
			{
				var up = transform.TransformDirection(Vector3.up);
				RaycastHit hit;
				Debug.DrawRay(transform.position, up * 6, Color.green);

				if (!Physics.Raycast(transform.position, up, out hit, 5))
				{
					crouching = false;
					thisRB.height = standheight;
				}

			}
		}
	}



    void FixedUpdate()
	{

		if (!FROZEN)
		{

			if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
			{



				if (Input.GetKey(KeyCode.Keypad0) || Input.GetKey(KeyCode.LeftShift))
				{
					speed = sprintspeed;
				}
				else
				{
					speed = walkspeed;
				}


				//Debug.Log("f");
				var moveForce = transform.forward * speed;
				thisRB.Move(moveForce);
			}


			if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
			{
				//Debug.Log("l");
				var moveForce = transform.right * speed;
				thisRB.Move(-moveForce);
			}



			if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
			{
				//Debug.Log("r");
				var moveForce = transform.right * speed;
				thisRB.Move(moveForce);
			}



			if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
			{
				//Debug.Log("b");
				var moveForce = transform.forward * speed;
				thisRB.Move(-moveForce);
			}



		}


	}
}
