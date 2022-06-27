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
				int pickuplayer = LayerMask.GetMask("pickupable");
				int evidencelayer = LayerMask.GetMask("evidence");

				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, 5.5f, pickuplayer) || Physics.Raycast(ray, out hit, 5.5f, evidencelayer))
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

	}




	void FixedUpdate()
	{



		if (pickup.hasobject == true && myHeldItem != null)
		{

			if (Input.GetKey(KeyCode.Keypad1))
			{

				handTransform.Rotate(new Vector3(-5, 0, 5) * (Time.smoothDeltaTime * 20));

			}


			if (Input.GetKey(KeyCode.Keypad2))
			{

				handTransform.Rotate(new Vector3(-5, 0, 0) * (Time.smoothDeltaTime * 20));

			}


			if (Input.GetKey(KeyCode.Keypad3))
			{

				handTransform.Rotate(new Vector3(-5, 0, -5) * (Time.smoothDeltaTime * 20));

			}





			if (Input.GetKey(KeyCode.Keypad4))
			{

				handTransform.Rotate(new Vector3(0, 0, 5) * (Time.smoothDeltaTime * 20));

			}


			if (Input.GetKeyUp(KeyCode.Keypad5))
			{

				Debug.Log("focus");

				handTransform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z);
				myHeldItem.eulerAngles = new Vector3(StartRotation.x, StartRotation.y, StartRotation.z);

			}




			if (Input.GetKey(KeyCode.Keypad6))
			{

				handTransform.Rotate(new Vector3(0, 0, -5) * (Time.smoothDeltaTime * 20));

			}

			if (Input.GetKey(KeyCode.Keypad7))
			{

				handTransform.Rotate(new Vector3(5, 0, 5) * (Time.smoothDeltaTime * 20));

			}





			if (Input.GetKey(KeyCode.Keypad8))
			{

				handTransform.Rotate(new Vector3(5, 0, 0) * (Time.smoothDeltaTime * 20));

			}



			if (Input.GetKey(KeyCode.Keypad9))
			{

				handTransform.Rotate(new Vector3(5, 0, -5) * (Time.smoothDeltaTime * 20));

			}



		}



	}


	public void PickupItem(RaycastHit hit)
    {
		var TheItem = hit;

		RotationMenu.alpha = 0.7f;

		TheItem.transform.SetParent(handTransform, true);


		TheItem.transform.localPosition = new Vector3(0, 0, 0);
		TheItem.transform.eulerAngles = new Vector3(TheItem.transform.eulerAngles.x, TheItem.transform.eulerAngles.y, TheItem.transform.eulerAngles.z);


		TheItem.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

		myHeldItem = TheItem.transform;
		pickup.hasobject = true;
	}


	public void DropItem()
	{

		RotationMenu.alpha = 0f;
		myHeldItem.SetParent(defaultparent);
		myHeldItem.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
		myHeldItem = null;
		pickup.hasobject = false;
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
			myHeldItem = null;
        }

	}



}
