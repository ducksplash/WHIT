using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pickup : MonoBehaviour
{


	public bool hasobject;
	public Transform defaultparent;
	public Transform myHeldItem;
	public Transform heldItemParent;
	public Transform cameraTransform;


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

					var TheItem = hit;


					Debug.Log("before parenting" + TheItem.transform.name);



					TheItem.transform.SetParent(cameraTransform,true);

					TheItem.transform.GetComponent<Rigidbody>().isKinematic = true;

					Debug.Log("after parenting"+TheItem.transform.name);
					myHeldItem = TheItem.transform;
					hasobject = true;
				}
			}  
			else
            {
				myHeldItem.SetParent(defaultparent);
				myHeldItem.transform.GetComponent<Rigidbody>().isKinematic = false;
				myHeldItem = null;
				hasobject = false;

			}
		} 
    }






}
