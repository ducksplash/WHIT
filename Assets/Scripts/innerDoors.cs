using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class innerDoors : MonoBehaviour
{
	
	public GameObject thisDoor;
	private string thisDoorName;
	public GameObject thisDoorHinge;
	public Light[] doorLights;
	public string doorLockTag;
	public string doorUnlockTag;
	private Animator doorAnimator;
	private bool isOpen = false;
	public bool isLocked = false;
	private Material[] theLightMats;
	private Collider theDoorCollider;

	
    void Start()
    {
		doorAnimator = thisDoorHinge.GetComponent<Animator>();
		thisDoorName = thisDoor.name;
    }
	
	
	IEnumerator changeLightMats(Material[] theseMats, bool doorOpen, Light thisLight)
	{
		if (theseMats != null)
		{
			for (int i = 0; i < theseMats.Length; i++)
			{
				if (theseMats[i].name.Contains("bulb"))
				{
					
					if (doorOpen)
					{
						var litLiteCol = thisLight.color;
						theseMats[i].SetColor("_EmissionColor", litLiteCol * 10f);
						theseMats[i].SetColor("_Color", litLiteCol);
					}
					else
					{
						var litLiteCol = new Color(0,0,0,0.8f);				
						theseMats[i].SetColor("_EmissionColor", litLiteCol * 10f);
						theseMats[i].SetColor("_Color", litLiteCol);
					}				
					
				}			
			}
		}
		yield return new WaitForSeconds(0.1f);
	}
	
	
	
	

	public void doLockedLights()
	{
		doorLights = thisDoorHinge.transform.parent.GetComponentsInChildren<Light>();
		
		foreach (Light lightie in doorLights)
		{
			if (lightie.transform.parent.parent.GetComponent<innerDoors>().isLocked)
			{
				
				if (lightie.name.Contains("locked"))
				{
					theLightMats = lightie.transform.parent.GetComponent<Renderer>().materials;
					
					lightie.enabled = true;	
					
					StartCoroutine(changeLightMats(theLightMats, true, lightie));
					
				}
				if (lightie.name.Contains("open"))
				{
					theLightMats = lightie.transform.parent.GetComponent<Renderer>().materials;
					
					lightie.enabled = false;	
					
					StartCoroutine(changeLightMats(theLightMats, false, lightie));
					
				}

			}
			else
			{
				if (lightie.name.Contains("locked"))
				{
					theLightMats = lightie.transform.parent.GetComponent<Renderer>().materials;
					
					lightie.enabled = false;	
					
					StartCoroutine(changeLightMats(theLightMats, false, lightie));
					
				}
				if (lightie.name.Contains("open"))
				{
					theLightMats = lightie.transform.parent.GetComponent<Renderer>().materials;
					
					lightie.enabled = true;	
					
					StartCoroutine(changeLightMats(theLightMats, true, lightie));
					
				}
			}
			
		}	
		
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
					
					if (!isLocked)
					{
						
						if (hit.transform.name.Equals(thisDoorName))
						{  
							theDoorCollider = hit.transform.GetComponent<BoxCollider>();
							
							if (!isOpen)
							{
							
								doorAnimator.SetTrigger("opened");
								StartCoroutine(weeWait(1,theDoorCollider));
								isOpen = true;
								
							}
							else
							{
								doorAnimator.SetTrigger("closed");
								isOpen = false;
								StartCoroutine(weeWait(1,theDoorCollider));
								doorAnimator.SetTrigger("idle");
							}
						}
					}					
				}  				
			}  
		} 
    }
}
