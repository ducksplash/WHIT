using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FirstPersonCollision : MonoBehaviour
{
	
	
	private Collider thePlayerCollider;

	public float speed = 0.1f;
	public float walkspeed = 0.1f;
	public float sprintspeed = 0.2f;
	Vector2 velocity;
	private Collision thisCollision;
	//private bool didCollide = false;
	private CharacterController thisCharController;
	private Camera MainCam;
	public static bool FROZEN;
	public static bool crouching;
	public float croucheight;
	public float standheight;
	public Image stanceimg;
	public Sprite crouchsprite;
	public Sprite standsprite;

	public TextMeshProUGUI PaperDeathText;
	public TextMeshProUGUI PaperDateText;
	public CanvasGroup DeathScreen;
	public CanvasGroup NewspaperBundle;

	// the stuff we must disable
	public CanvasGroup CrossHair;
	public CanvasGroup CrouchIndicator;
	public CanvasGroup TorchIndicator;




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
			//didCollide = true;
		}

	}

	void OnCollisionExit(Collision thisCollision)
	{
		//didCollide = false;
	}



	void OnTriggerEnter(Collider theOther)
	{


		if (theOther.gameObject.layer == 20)
		{
			DEAD("Electrocution ( this isn't dynamic yet :p )");
		}
	}



	public void DisableAllScreens()
	{
		CrossHair.alpha = 0f;
		CrouchIndicator.alpha = 0f;
		TorchIndicator.alpha = 0f;
		NewspaperBundle.alpha = 0f;
		DeathScreen.alpha = 0f;
	}



	public void DEAD(string CauseOfDeath) 
	{


		//we need to lerp this later, but for now just post it
		// disable all screens and then re-enable the one I want. lazy, but the overheads are minimal (though not zero) :p
		DisableAllScreens();

		// death screen first
		DeathScreen.alpha = 1f;

		// then newspaper
		NewspaperBundle.alpha = 1f;

		var buildDate = "";

		buildDate += System.DateTime.Now.ToString("dddd");
		buildDate += ", ";
		buildDate += System.DateTime.Now.ToString("MMMM d");
		buildDate += MonthDay(System.DateTime.Now.ToString("dd").ToString());
		buildDate += ", ";
		buildDate += System.DateTime.Now.ToString("yyyy");

		PaperDateText.text = buildDate;


		Debug.Log("ya dead!");



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




















	public string MonthDay(string day)
	{
		string nuNum = "th";
		if (int.Parse(day) < 11 || int.Parse(day) > 20)
		{
			day = day.ToCharArray()[^1].ToString();
			switch (day)
			{
				case "1":
					nuNum = "st";
					break;
				case "2":
					nuNum = "nd";
					break;
				case "3":
					nuNum = "rd";
					break;
			}
		}
		return nuNum;
	}
}
