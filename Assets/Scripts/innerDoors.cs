using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class innerDoors : MonoBehaviour
{
	
	public GameObject thisDoor;
	private string thisDoorName;
	public GameObject thisDoorHinge;
	public string doorLockTag;
	public string doorUnlockTag;
	private Animator doorAnimator;
	public bool isOpen = false;
	public bool isLocked = false;
	private Material[] theLightMats;
	private Transform[] doorLights;
	private Collider theDoorCollider;

	public bool PlayerClicked;
	
    void Start()
    {
		doorAnimator = thisDoorHinge.GetComponent<Animator>();
		thisDoorName = thisDoor.name;
		PlayerClicked = false;


    }
	
	
	
	
	

	public void doLockedLights()
	{
		doorLights = thisDoorHinge.transform.parent.GetComponentsInChildren<Transform>();
		
		foreach (Transform lightpart in doorLights)
		{
			Debug.Log(lightpart.name);

			if (lightpart.GetComponentInChildren<Renderer>().material.name.Contains("bulb"))
			{
				Debug.Log(lightpart.name);

				if (!isLocked)
				{
					var litLiteCol = new Color(0,0.5f,0,1);
					lightpart.GetComponent<Renderer>().material.SetColor("_Color", litLiteCol);
					lightpart.GetComponent<Renderer>().material.SetColor("_EmissiveColor", litLiteCol * 20f);
					Debug.Log(lightpart.name);


				}
				else
				{
					var litLiteCol = new Color(0.5f, 0, 0, 1);
					lightpart.GetComponent<Renderer>().material.SetColor("_Color", litLiteCol);
					lightpart.GetComponent<Renderer>().material.SetColor("_EmissiveColor", litLiteCol * 20f);
					Debug.Log(lightpart.name);
				}

			}
			
		}	
		
	}
	
	
	
	IEnumerator DisableColliderMomentarily(float aWeeSecond, Collider theCollider)
	{
		
		theCollider.enabled = false;
		yield return new WaitForSeconds(aWeeSecond);
		PlayerClicked = false;
		theCollider.enabled = true;
		
	}



	void Update()
	{

		if (!PlayerClicked)
		{
			if (Input.GetMouseButtonUp(1))
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, 4f))
				{
					if (!isLocked)
					{

						if (hit.transform.name.Equals(thisDoorName))
						{
							DoDoor(hit);
							PlayerClicked = true;
						}
					}
				}
			}
		}
	}
	



		public void DoDoor(RaycastHit hit)
		{

			var thisDoorCollider = hit.transform.GetComponent<Collider>();

			if (!isOpen)
			{

				doorAnimator.SetTrigger("opened");
				StartCoroutine(DisableColliderMomentarily(0.5f, thisDoorCollider));
				isOpen = true;

			}
			else
			{
				doorAnimator.SetTrigger("closed");
				isOpen = false;
				StartCoroutine(DisableColliderMomentarily(0.5f, thisDoorCollider));
				doorAnimator.SetTrigger("idle");
			}

		}





	}
