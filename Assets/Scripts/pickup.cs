using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pickup : MonoBehaviour
{


	public static bool hasobject;
	public Transform defaultparent;
	public Transform myHeldItem;
	public Transform heldItemParent;
	public Transform cameraTransform;

    private void Awake()
    {
		hasobject = false;
    }



    void Update()
	{
		if (Input.GetMouseButtonDown(1))
		{

			if (!hasobject)
			{
				int pickuplayer = LayerMask.GetMask("pickupable");
				int evidencelayer = LayerMask.GetMask("evidence");

				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, 5.5f, pickuplayer) || Physics.Raycast(ray, out hit, 5.5f, evidencelayer))
				{

					hasobject = true;
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


			if (hasobject)
			{
				ThrowItem();
			}

			
		}
	}

	public void PickupItem(RaycastHit hit)
    {
		var TheItem = hit;

		TheItem.transform.SetParent(cameraTransform, true);

		TheItem.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

		myHeldItem = TheItem.transform;
		hasobject = true;
	}


	public void DropItem()
	{
		hasobject = false;
		myHeldItem.SetParent(defaultparent);
		myHeldItem.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
		myHeldItem = null;
		hasobject = false;
	}

	public void ThrowItem()
	{

		myHeldItem.SetParent(defaultparent);
		myHeldItem.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
		myHeldItem.transform.GetComponent<Rigidbody>().AddForce(cameraTransform.forward * 200f);
		myHeldItem = null;

		Invoke("SetAbsent", 0.5f);
	}


	public void SetAbsent()
    {
		hasobject = false;
    }

}
