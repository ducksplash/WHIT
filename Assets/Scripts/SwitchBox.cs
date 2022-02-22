using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchBox : MonoBehaviour
{
	
	public GameObject thisDoor;
	private string thisDoorName;
	private string thisSwitchName;
	public GameObject thisDoorHinge;
	public GameObject thisSwitch;
	private GameObject[] thisIndicatorSet;
	private Light[] allTheLights;
	public GameObject thisIndicator;
	public string indicatorTag;
	private Animator doorAnimator;
	private Animator switchAnimator;
	Animation anim;
	private bool isOpen = false;
	private bool switchIsOn = false;
	public string doorLockTag;
	public string doorUnlockTag;
	public innerDoors innerDoors;
	public GameObject[] doorLockedLights;
	public GameObject[] doorUnlockedLights;
	
	
	
	IEnumerator doLockedLights()
	{
		doorLockedLights = GameObject.FindGameObjectsWithTag(doorLockTag);
		doorUnlockedLights = GameObject.FindGameObjectsWithTag(doorUnlockTag);
		
		
		foreach (GameObject lightie in doorLockedLights)
		{
			if (lightie.transform.parent.parent.GetComponent<innerDoors>().isLocked)
			{
				lightie.transform.parent.parent.GetComponent<innerDoors>().doLockedLights();
			}
			else
			{
				lightie.transform.parent.parent.GetComponent<innerDoors>().doLockedLights();
			}
		}	
		
		foreach (GameObject lightie in doorUnlockedLights)
		{
			if (!lightie.transform.parent.parent.GetComponent<innerDoors>().isLocked)
			{
				lightie.transform.parent.parent.GetComponent<innerDoors>().doLockedLights();
			}
			else
			{
				lightie.transform.parent.parent.GetComponent<innerDoors>().doLockedLights();
			}
		}
		
		
		yield return new WaitForSeconds(0.1f);
		
		
	}
	
	
	void Awake()
	{
		

		
		allTheLights = FindObjectsOfType<Light>();
		
		
		foreach (Light aLight in allTheLights)
		{

			if (aLight.tag != "TORCH" && aLight.tag != "HOUSELIGHTS")
			{

			aLight.enabled = false;

			//var ledRenderer = thisIndicator.GetComponent<Renderer>();

			//ledRenderer.material.SetColor("_Color", Color.green);
			//ledRenderer.material.SetColor("_EmissionColor", Color.green);
			}

		}
		
		
	}
	
	
	
	
    void Start()
    {
		anim = GetComponent<Animation>();
		doorAnimator = thisDoorHinge.GetComponent<Animator>();
		switchAnimator = thisSwitch.GetComponent<Animator>();
		thisDoorName = thisDoor.name;
		thisSwitchName = thisSwitch.name;
		thisIndicatorSet = GameObject.FindGameObjectsWithTag(indicatorTag);
		
    }


	IEnumerator weeWait(float aWeeSecond, Collider theCollider)
	{
		
		theCollider.enabled = false;
		yield return new WaitForSeconds(aWeeSecond);
		theCollider.enabled = true;
		
	}



    void Update()
    {
        if (Input.GetMouseButtonDown(1))
		{  
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);  
			RaycastHit hit;  
			if (Physics.Raycast(ray, out hit)) 
			{  
				if (hit.distance <= 10.5f)
				{		


				
					if (hit.transform.name.Equals(thisDoorName))
					{  
					
						var theDoorCollider = hit.transform.GetComponent<BoxCollider>();
							
						if (!isOpen)
						{
							doorAnimator.SetTrigger("opened");
							StartCoroutine(weeWait(1,theDoorCollider));
							isOpen = true;
						}
						else
						{
							doorAnimator.SetTrigger("closed");
							StartCoroutine(weeWait(1,theDoorCollider));
							isOpen = false;
							doorAnimator.SetTrigger("idle");
						}
					}
				
				
					if (isOpen)
					{
						if (hit.transform.name.Equals(thisSwitchName))
						{  
								
							if (!switchIsOn)
							{
								switchAnimator.SetTrigger("switchon");
								switchIsOn = true;
								
								GameMaster.POWER_SUPPLY_ENABLED = true;
								
								StartCoroutine(doLockedLights());
								
															
								foreach (GameObject thisIndicator in thisIndicatorSet)
								{
									var ledRenderer = thisIndicator.GetComponent<Renderer>();
									
									var nuCol = new Color(0,1,0,1);
									
									ledRenderer.material.SetColor("_Color", nuCol);
									ledRenderer.material.SetColor("_EmissionColor", nuCol);
								
								}	
							}
							else
							{
								switchAnimator.SetTrigger("switchoff");
								switchIsOn = false;
								switchAnimator.SetTrigger("switchidle");								
								
								GameMaster.POWER_SUPPLY_ENABLED = false;
								
								foreach (GameObject thisIndicator in thisIndicatorSet)
								{
									var ledRenderer = thisIndicator.GetComponent<Renderer>();
									
									ledRenderer.material.SetColor("_Color", Color.black);
									ledRenderer.material.SetColor("_EmissionColor", Color.black);
								}
								
								
								foreach (Light aLight in allTheLights)
								{
									
									if (aLight.tag != "TORCH")
									{
									
									aLight.enabled = false;
									
									//var ledRenderer = thisIndicator.GetComponent<Renderer>();
									
									//ledRenderer.material.SetColor("_Color", Color.green);
									//ledRenderer.material.SetColor("_EmissionColor", Color.green);
									}
								}
							}
						}
					}
				}  				
			}  
		} 
    }
}
