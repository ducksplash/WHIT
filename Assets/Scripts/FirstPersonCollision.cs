using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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
	public bool crouching;
	public float croucheight;
	public float standheight;
	public Image stanceimg;
	public Sprite crouchsprite;
	public Sprite standsprite;
	public bool climbing;
	public GameObject LadderAttachedTo;


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
	public CanvasGroup EvidenceCompanion;

	public Vector3 SpawnPoint;




	void Start()
	{

		stanceimg = stanceimg.GetComponent<Image>();


		SpawnPoint = transform.position;


		thePlayerCollider = gameObject.GetComponent<CapsuleCollider>();
		thisCharController = gameObject.GetComponent<CharacterController>();
		MainCam = Camera.main;

		

	}
	




    void Update()
	{
		if (InputManager.GetKeyUp("crouch") || Input.GetKeyUp(KeyCode.RightControl))
		{
			if (!GameMaster.FROZEN)
			{
				if (!crouching)
				{
					crouch();
				}
				else
				{
					var up = Vector3.up;
					RaycastHit hit;

					if (!Physics.Raycast(MainCam.transform.position, up, out hit, 3f))
					{

						uncrouch();

					}
					else
                    {
						Debug.Log("uncrouch ray hit?");

						Debug.Log(hit.transform.name);


					}

				}
			}
		}


		if (!GameMaster.FROZEN)
		{


			if (climbing)
			{

				if (InputManager.GetKey("jump"))
                {
					ExitLadder(LadderAttachedTo);
                }



				if (InputManager.GetKey("up") || Input.GetKey(KeyCode.UpArrow))
				{
					Debug.Log("u");


					var moveForce = transform.up * speed * Time.smoothDeltaTime;
					thisCharController.Move(moveForce);



				}

				if (InputManager.GetKey("down") || Input.GetKey(KeyCode.DownArrow))
				{
					Debug.Log("d");


					var moveForce = transform.up * speed * Time.smoothDeltaTime;
					thisCharController.Move(-moveForce);
				}
			}
			else
			{


				if (InputManager.GetKey("up") || Input.GetKey(KeyCode.UpArrow))
				{


					if (!crouching)
					{

						if (Input.GetKey(KeyCode.Keypad0) ||Input.GetKey(KeyCode.Alpha0) || InputManager.GetKey("sprint"))
						{
							speed = sprintspeed;
						}
						else
						{
							speed = walkspeed;
						}
					}

					//Debug.Log("f");
					var moveForce = transform.forward * speed * Time.smoothDeltaTime;
					thisCharController.Move(moveForce);
				}


				if (InputManager.GetKey("down") || Input.GetKey(KeyCode.DownArrow))
				{
					//Debug.Log("b");
					var moveForce = transform.forward * speed * Time.smoothDeltaTime;
					thisCharController.Move(-moveForce);
				}



				if (InputManager.GetKey("left") || Input.GetKey(KeyCode.LeftArrow))
				{
					//Debug.Log("l");
					var moveForce = transform.right * speed * Time.smoothDeltaTime;
					thisCharController.Move(-moveForce);
				}



				if (InputManager.GetKey("right") || Input.GetKey(KeyCode.RightArrow))
				{
					//Debug.Log("r");
					var moveForce = transform.right * speed * Time.smoothDeltaTime;
					thisCharController.Move(moveForce);
				}

			}



			if (climbing)
			{
				var xup = Vector3.up;
				RaycastHit xhit;
				if (Physics.Raycast(MainCam.transform.position, xup, out xhit, 3f))
				{

					crouch();

				}
			}






			//devmode


				if (InputManager.GetKey("respawn"))
                {
					transform.position = SpawnPoint;
                }


		}

	}



	public void crouch()
    {

		crouching = true;
		//thisCharController.height = croucheight;
		thisCharController.height = Mathf.Lerp(croucheight, standheight, 0f);
		speed = walkspeed;
		stanceimg.sprite = crouchsprite;

	}


	public void uncrouch()
    {
		Debug.Log("uncrouch ray didnt hit");
		crouching = false;
		//thisCharController.height = standheight;
		thisCharController.height = Mathf.Lerp(standheight, croucheight, 0f);
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






	public void QuitToMainmenu()
	{

		GameMaster.INMENU = false;
		GameMaster.FROZEN = false;


		DisableAllScreens();

		CrouchIndicator.alpha = 1;
		CrossHair.alpha = 1;
		if (GameMaster.TORCHCOLLECTED)
        {
			TorchIndicator.alpha = 1;
        }

	}






	public void ContinueGame()
	{

		GameMaster.INMENU = false;
		GameMaster.FROZEN = false;


		transform.position = SpawnPoint;

		Debug.Log("carry on");

		// do this before stuff lol
		DisableAllScreens();


		CrouchIndicator.alpha = 1;
		CrossHair.alpha = 1;
		EvidenceCompanion.alpha = 1;
		if (GameMaster.TORCHCOLLECTED)
		{
			TorchIndicator.alpha = 1;
		}


	}

	public void CauseDeath(string cause)
    {

		GameMaster.INMENU = true;
		GameMaster.FROZEN = true;

		StartCoroutine(SlowDeath(cause));
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

		uncrouch();


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





	// laddering


	private void OnTriggerEnter(Collider other)
	{


		if (other.tag.Equals("LADDERS") && !climbing)
		{
			LadderAttachedTo = other.gameObject;
			Debug.Log(other);
			Debug.Log("made contact");
			gameObject.transform.parent = transform;
			gameObject.GetComponent<Rigidbody>().useGravity = false;
			climbing = true;

		}


	}


    private void OnTriggerExit(Collider other)
    {
		if (other.tag.Equals("LADDERS") && climbing)
		{
			LadderAttachedTo = other.gameObject;

			speed = walkspeed;

			ExitLadder(LadderAttachedTo);

		}
	}
    private void ExitLadder(GameObject WeeLadder)
	{

		Debug.Log("broke contact");
		gameObject.transform.parent = null;
		gameObject.GetComponent<Rigidbody>().useGravity = true;
		climbing = false;


	}






}
