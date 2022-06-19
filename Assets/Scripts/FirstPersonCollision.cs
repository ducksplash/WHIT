using UnityEngine;
using UnityEngine.UI;

public class FirstPersonCollision : MonoBehaviour
{
	
	
	private Collider thePlayerCollider;

	public float speed = 0.1f;
	public float walkspeed = 0.1f;
	public float sprintspeed = 0.2f;
	Vector2 velocity;
	private Collision thisCollision;
	private bool didCollide = false;
	private CharacterController thisCharController;
	private Camera MainCam;
	public static bool FROZEN;
	public static bool crouching;
	public float croucheight;
	public float standheight;
	public Image stanceimg;
	public Sprite crouchsprite;
	public Sprite standsprite;


	void Start()
	{

		stanceimg = stanceimg.GetComponent<Image>();



		thePlayerCollider = gameObject.GetComponent<CapsuleCollider>();
		thisCharController = gameObject.GetComponent<CharacterController>();
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
			if (!FROZEN)
			{
				if (!crouching)
				{
					crouching = true;
					thisCharController.height = croucheight;
					speed = walkspeed;
					stanceimg.sprite = crouchsprite;
				}
				else
				{
					var up = transform.TransformDirection(Vector3.up);
					RaycastHit hit;

					if (!Physics.Raycast(transform.position, up, out hit, 5))
					{
						crouching = false;
						thisCharController.height = standheight;
						stanceimg.sprite = standsprite;
					}

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

				if(!crouching)
                {

					if (Input.GetKey(KeyCode.Keypad0) || Input.GetKey(KeyCode.LeftShift))
					{
						speed = sprintspeed;
					}
					else
					{
						speed = walkspeed;
					}
                }

				//Debug.Log("f");
				var moveForce = transform.forward * speed;
				thisCharController.Move(moveForce);
			}


			if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
			{
				//Debug.Log("l");
				var moveForce = transform.right * speed;
				thisCharController.Move(-moveForce);
			}



			if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
			{
				//Debug.Log("r");
				var moveForce = transform.right * speed;
				thisCharController.Move(moveForce);
			}



			if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
			{
				//Debug.Log("b");
				var moveForce = transform.forward * speed;
				thisCharController.Move(-moveForce);
			}



		}


	}
}
