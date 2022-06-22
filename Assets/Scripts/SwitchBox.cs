using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchBox : MonoBehaviour
{
	
	public GameObject thisDoor;
	private string thisDoorName;
	private string thisSwitchName;
	private string thatSwitchName;
	public GameObject thisDoorHinge;
	public GameObject thisSwitch;
	public GameObject thatSwitch;
	private GameObject[] thisIndicatorSet;
	private Light[] allTheLights;
	public GameObject thisIndicator;
	public GameObject thatIndicator;
	public string indicatorTag;
	private Animator doorAnimator;
	private Animator switchAnimator;
	private Animator thatSwitchAnimator;
	Animation anim;
	private bool isOpen = false;
	private bool switchIsOn = false;
	private bool incineratorIsOn = true;
	public string doorLockTag;
	public string doorUnlockTag;
	public innerDoors[] innerDoors;
	public GameObject[] doorLockedLights;
	public GameObject[] doorUnlockedLights;
	
	
		
	void Awake()
	{
		

		
		allTheLights = FindObjectsOfType<Light>();
		
		
		foreach (Light aLight in allTheLights)
		{

			if (aLight.tag != "TORCH" && aLight.tag != "STREETLAMP" && aLight.tag != "HOUSELIGHTS" && aLight.tag != "INCINERATOR")
			{

				aLight.enabled = false;

			}

		}
		
		
	}
	
	
	
	
    void Start()
    {
		anim = GetComponent<Animation>();
		doorAnimator = thisDoorHinge.GetComponent<Animator>();
		switchAnimator = thisSwitch.GetComponent<Animator>();
		thatSwitchAnimator = thatSwitch.GetComponent<Animator>();
		thisDoorName = thisDoor.name;
		thisSwitchName = thisSwitch.name;
		thatSwitchName = thatSwitch.name;
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
				if (hit.distance <= 5.5f)
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


								foreach (innerDoors thisdoor in innerDoors)
								{
									thisdoor.doLockedLights();
								}




								GameMaster.INCINERATOR_ENABLED = true;

								foreach (Light aLight in allTheLights)
								{
									if (aLight.tag == "INCINERATOR")
									{

										aLight.enabled = true;

									}
								}


								foreach (GameObject thisIndicator in thisIndicatorSet)
								{
									var ledRenderer = thisIndicator.GetComponent<Renderer>();

									var nuCol = new Color(0, 1, 0, 1);

									ledRenderer.material.SetColor("_Color", nuCol);
									ledRenderer.material.SetColor("_EmissiveColor", nuCol * 5);

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
									ledRenderer.material.SetColor("_EmissiveColor", Color.black);
								}


								GameMaster.INCINERATOR_ENABLED = false;


								foreach (Light aLight in allTheLights)
								{

									if (aLight.tag != "TORCH" && aLight.tag != "STREETLAMP" && aLight.tag != "HOUSELIGHTS")
									{

										aLight.enabled = false;

									}
								}
							}
						}






						if (hit.transform.name.Equals(thatSwitchName))
						{

							if (!incineratorIsOn)
							{

								if (GameMaster.POWER_SUPPLY_ENABLED)
								{

									thatSwitchAnimator.SetTrigger("switchoff");
									incineratorIsOn = true;
									thatSwitchAnimator.SetTrigger("switchidle");

									GameMaster.INCINERATOR_ENABLED = true;

									var nuCol = new Color(0, 1, 0, 1);


									thatIndicator.GetComponent<Renderer>().material.SetColor("_Color", nuCol);
									thatIndicator.GetComponent<Renderer>().material.SetColor("_EmissiveColor", nuCol * 5);

									foreach (Light aLight in allTheLights)
									{

										if (aLight.tag == "INCINERATOR")
										{

											aLight.enabled = true;

										}
									}
								}


							}
							else
							{



								thatSwitchAnimator.SetTrigger("switchon");
								incineratorIsOn = false;
								thatSwitchAnimator.SetTrigger("switchidle");

								GameMaster.INCINERATOR_ENABLED = false;

								thatIndicator.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
								thatIndicator.GetComponent<Renderer>().material.SetColor("_EmissiveColor", Color.black * 0); 
								
								
								foreach (Light aLight in allTheLights)
								{

									if (aLight.tag == "INCINERATOR")
									{

										aLight.enabled = false;

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
