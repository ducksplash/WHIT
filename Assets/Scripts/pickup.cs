using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pickup : MonoBehaviour
{


	public static bool hasobject;
	public Transform defaultparent;
	public Transform myHeldItem;
	public Transform heldItemParent;
	public Transform handTransform;
	public bool hasobjectshown;
	public Vector3 StartRotation;
	public CanvasGroup RotationMenu;
	public LayerMask IgnoreLayer;

	public CanvasGroup phoneTick;
	public CanvasGroup notepadTick;
	public CanvasGroup torchTick;
	public Collider theHandCollider;



	private void Awake()
    {
		pickup.hasobject = false;
		StartRotation = handTransform.parent.eulerAngles;

	}



    void Update()
	{
		hasobjectshown = pickup.hasobject;
		if (Input.GetMouseButtonDown(1))
		{

			if (!pickup.hasobject)
			{


				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;

				if (Physics.Raycast(ray, out hit, 3.5f, ~IgnoreLayer))
				{

					PickupItem(hit);

				}
			}
			else
			{

				DropItem();

			}
		}


		if (Input.GetMouseButtonUp(0))
		{


			if (pickup.hasobject)
			{
				ThrowItem();
			}

			
		}




		// if focus broken for a reason other than dropping and throwing
		if (!pickup.hasobject)
		{
			myHeldItem = null;
			RotationMenu.alpha = 0f;
			hasobjectshown = false;
		}


		if (handTransform.childCount > 0 && !pickup.hasobject)
		{
			myHeldItem = null;
			RotationMenu.alpha = 0f;
			hasobjectshown = false;
			handTransform.GetChild(0).parent = defaultparent;
		}

	}




    void FixedUpdate()
	{





		if (pickup.hasobject == true && myHeldItem != null)
		{

			if (Input.GetKey(KeyCode.Keypad1))
			{

				myHeldItem.Rotate(new Vector3(-5, 5, 0) * (Time.smoothDeltaTime * 20));

			}


			if (Input.GetKey(KeyCode.Keypad2))
			{

				myHeldItem.Rotate(new Vector3(-5, 0, 0) * (Time.smoothDeltaTime * 20));

			}


			if (Input.GetKey(KeyCode.Keypad3))
			{

				myHeldItem.Rotate(new Vector3(-5, -5, 0) * (Time.smoothDeltaTime * 20));

			}





			if (Input.GetKey(KeyCode.Keypad4))
			{

				myHeldItem.Rotate(new Vector3(0, 5, 0) * (Time.smoothDeltaTime * 20));

			}


			if (Input.GetKeyUp(KeyCode.Keypad5))
			{

				Debug.Log("focus");

				//handTransform.LookAt(transform, -transform.forward);


				myHeldItem.transform.localEulerAngles = transform.forward * -1;
			}




			if (Input.GetKey(KeyCode.Keypad6))
			{

				myHeldItem.Rotate(new Vector3(0, -5, 0) * (Time.smoothDeltaTime * 20));

			}

			if (Input.GetKey(KeyCode.Keypad7))
			{

				myHeldItem.Rotate(new Vector3(5, -5, 0) * (Time.smoothDeltaTime * 20));

			}





			if (Input.GetKey(KeyCode.Keypad8))
			{

				myHeldItem.Rotate(new Vector3(5, 0, 0) * (Time.smoothDeltaTime * 20));

			}



			if (Input.GetKey(KeyCode.Keypad9))
			{

				myHeldItem.Rotate(new Vector3(5, 5, 0) * (Time.smoothDeltaTime * 20));

			}



		}



	}


	public void PickupItem(RaycastHit hit)
    {
		if (!GameMaster.PHONEOUT)
		{



			if (!hit.transform.gameObject.tag.Equals("COLLECTABLE"))
			{
				var TheItem = hit;

				RotationMenu.alpha = 0.7f;

				TheItem.transform.SetParent(handTransform, true);

				//handTransform.Rotate(new Vector3(0,0,0), Space.Self);
				TheItem.transform.localPosition = new Vector3(0, 0, 0);

				TheItem.transform.localEulerAngles = transform.forward * -1;


				TheItem.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

				myHeldItem = TheItem.transform;
				pickup.hasobject = true;
				GameMaster.HASITEM = true;

				if (TheItem.transform.GetComponent<Evidence>() != null)
                {
					

					Debug.Log("this is evidence");

					if (TheItem.transform.GetComponent<Evidence>().EvidenceQuality > 1)
                    {
						TheItem.transform.GetComponent<Evidence>().EvidenceQuality--;
                    }


					Debug.Log("evidence quality"+ TheItem.transform.GetComponent<Evidence>().EvidenceQuality);

				}

			}
			else
			{

				if (hit.transform.name.Contains("TORCH"))
				{

					GameMaster.TORCHCOLLECTED = true;
					Destroy(hit.transform.gameObject); 
					var dialogstring = "Found it! I can check that it works by pressing H.";
					gameObject.GetComponent<DialogueManager>().NewDialogue("NORA", dialogstring, 4, gameObject);
					torchTick.alpha = 1;
				}

				if (hit.transform.name.Contains("NOTEPAD"))
				{

					GameMaster.NOTEPADCOLLECTED = true;
					Destroy(hit.transform.gameObject);
					var dialogstring = "Got some places I wanna check out written down here. I should take a look when I'm leaving.";
					gameObject.GetComponent<DialogueManager>().NewDialogue("NORA", dialogstring, 4, gameObject);
					notepadTick.alpha = 1;

				}

				if (hit.transform.name.Contains("PHONE"))
				{

					GameMaster.PHONECOLLECTED = true;
					Destroy(hit.transform.gameObject);
					var dialogstring = "My phone. I should test that it works by pressing P.";
					gameObject.GetComponent<DialogueManager>().NewDialogue("NORA", dialogstring, 4, gameObject);
					phoneTick.alpha = 1;

				}

			}

		}
	}


	public void DropItem()
	{

		RotationMenu.alpha = 0f;
		myHeldItem.SetParent(defaultparent);
		myHeldItem.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
		myHeldItem = null;
		pickup.hasobject = false;
		GameMaster.HASITEM = false;
	}

	public void ThrowItem()
	{
		
		RotationMenu.alpha = 0f;
		myHeldItem.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
		myHeldItem.transform.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * 200f);
		myHeldItem.SetParent(defaultparent);

		if (myHeldItem.parent == defaultparent)
        {
			pickup.hasobject = false;
			GameMaster.HASITEM = false;
			myHeldItem = null;
        }

	}



}
