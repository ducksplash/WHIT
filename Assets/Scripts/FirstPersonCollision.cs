using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FirstPersonCollision : MonoBehaviour
{
	
	
	private Collider thePlayerCollider;
	public bool INMENU;
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
	public CanvasGroup DeathScreenMain;
	public CanvasGroup DeathScreenFader;
	public CanvasGroup PaperScreenFader;
	public CanvasGroup DiedTextFader;
	public CanvasGroup ButtonFaderLeave;
	public CanvasGroup ButtonFaderContinue;

	// the stuff we must disable
	public CanvasGroup CrossHair;
	public CanvasGroup CrouchIndicator;
	public CanvasGroup TorchIndicator;

	public Vector3 SpawnPoint;


	void Start()
	{

		stanceimg = stanceimg.GetComponent<Image>();


		SpawnPoint = transform.position;


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












	public void DisableAllScreens()
	{
		CrossHair.alpha = 0f;
		CrouchIndicator.alpha = 0f;
		TorchIndicator.alpha = 0f;
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





	void OnTriggerEnter(Collider theOther)
	{


		if (theOther.gameObject.layer == 20 && !FROZEN)
		{

			var CauseString = theOther.name;

			StartCoroutine(SlowDeath(CauseString));

		}
	}




	public void QuitToMainmenu()
	{

		INMENU = false;
		FROZEN = false;


		DisableAllScreens();

		CrouchIndicator.alpha = 1;
		CrossHair.alpha = 1;
		TorchIndicator.alpha = 1;

	}














	public void ContinueGame()
	{

		INMENU = false;
		FROZEN = false;


		transform.position = SpawnPoint;

		Debug.Log("carry on");

		// do this before stuff lol
		DisableAllScreens();


		CrouchIndicator.alpha = 1;
		CrossHair.alpha = 1;
		TorchIndicator.alpha = 1;


	}




	IEnumerator SlowDeath(string CauseString)
    {
		DisableAllScreens();


		var buildDate = "";

		buildDate += System.DateTime.Now.ToString("dddd");
		buildDate += ", ";
		buildDate += System.DateTime.Now.ToString("MMMM d");
		buildDate += MonthDay(System.DateTime.Now.ToString("dd").ToString());
		buildDate += ", ";
		buildDate += System.DateTime.Now.ToString("yyyy");

		// put cause of death on paper
		// don't forget the full stop.
		PaperDeathText.text = CauseString + ".";
		PaperDateText.text = buildDate;


		Debug.Log(CauseString);

		// death screen first
		DeathScreenMain.alpha = 1f;
		DeathScreenMain.blocksRaycasts = true;


		INMENU = true;
		FROZEN = true;

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;


		var diedduration = 50;
		var duration = 100;
		var paperduration = 50;
		var buttonduration = 50;

		while (duration > 0)
		{
			DeathScreenFader.alpha += 0.01f;
			yield return new WaitForSeconds(0.01f);
			duration--;
			Debug.Log(duration);
		}

		if (duration == 0)
		{

			while (diedduration > 0)
			{
				DiedTextFader.alpha += 0.02f;
				yield return new WaitForSeconds(0.02f);
				diedduration--;
				Debug.Log(diedduration);
			}

		}


		if (duration == 0)
		{


			while (paperduration > 0)
			{
				PaperScreenFader.alpha += 0.02f;
				yield return new WaitForSeconds(0.02f);
				paperduration--;
				Debug.Log(paperduration);
			}

		}

		if (duration == 0)
		{


			while (buttonduration > 0)
			{

				ButtonFaderContinue.blocksRaycasts = true;
				ButtonFaderLeave.blocksRaycasts = true;
				ButtonFaderContinue.alpha += 0.02f;
				ButtonFaderLeave.alpha += 0.02f;
				yield return new WaitForSeconds(0.02f);
				buttonduration--;
				Debug.Log(buttonduration);
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
