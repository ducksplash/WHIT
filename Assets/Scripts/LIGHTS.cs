using System.Collections;  
using System.Collections.Generic;  
using UnityEngine;  

public class LIGHTS : MonoBehaviour
{
	public GameObject thisSwitch;
	public GameObject thatSwitch;
	private string thisSwitchName;
	private string thatSwitchName;
	private Material[] thatSwitchMats;
	private Material[] thisSwitchMats;
	public Light[] theseLights;
	public string lightTag;
	public float LightSwitchEmissionStrength = 5f;
	private Material[] theLightMats;
	private GameObject theLightBulb;

	[Header("Switch Type: PANEL or ROTATE")]
	public string SwitchType;



	void Start()
    {


		
		thisSwitchName = thisSwitch.name;
		thisSwitchMats = thisSwitch.GetComponent<Renderer>().materials;
		
		if (thatSwitch)
		{
		thatSwitchName = thatSwitch.name;
		
		thatSwitchMats = thatSwitch.GetComponent<Renderer>().materials;
		
		
		}
		else
		{
		thatSwitchName = null;	
		}
	}


	// change state of switch
	
	void changeSwitch(Material[] switchMats, bool onState)
	{
		var lightEmissionColour = new Color(0,0,0,0);
		
		if (onState)
		{
			lightEmissionColour = new Color(0,0.7f,0,1);
		}
		else
		{
			lightEmissionColour = new Color(0.5f,0,0,1);
		}

		for (int i = 0; i < switchMats.Length; i++)
		{
			if (switchMats[i].name.Contains("neon") || switchMats[i].name.Contains("led") || switchMats[i].name.Contains("bulb") || switchMats[i].name.Contains("diffuser"))
			{
				switchMats[i].SetColor("_Color", lightEmissionColour);
				switchMats[i].SetColor("_EmissiveColor", lightEmissionColour * 5f);
			}	

		}






	}
	

	void changeLights(Light myLight, bool powerState)
	{
		// Grab Light
		
		if (powerState)
		{
			myLight.enabled = true;	
		}
		else
		{
			myLight.enabled = false;	
		}
		
		// Read Colour
		var litLiteCol = myLight.color;
		

		theLightMats = myLight.transform.parent.GetComponent<Renderer>().materials;

		
		for (int i = 0; i < theLightMats.Length; i++)
		{
			if (theLightMats[i].name.Contains("bulb"))
			{
				
				if (powerState)
				{
					theLightMats[i].SetColor("_Color", litLiteCol);
					theLightMats[i].SetColor("_EmissiveColor", litLiteCol * 5f);
				}	
				else
				{
					var offColour = new Color(0,0,0,1f);
					theLightMats[i].SetColor("_Color", offColour);
					theLightMats[i].SetColor("_EmissiveColor", offColour);
				}
			}
		}
	}


    // Update is called once per frame
    void Update()
    {
		if (GameMaster.POWER_SUPPLY_ENABLED)
		{
			if (Input.GetMouseButtonDown(1))
			{  
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);  
				RaycastHit hit;  
				if (Physics.Raycast(ray, out hit)) {  
	  
					if (hit.distance <= 3f)
					{				
						if (hit.transform.name.Equals(thisSwitchName) || hit.transform.name.Equals(thatSwitchName))
						{
							ToggleLights();



							// is this a rotating switch?
							if (SwitchType == "ROTATE")
							{
								Debug.Log(SwitchType);
								RotateSwitch(thisSwitch);

								if (thatSwitch)
								{
									RotateSwitch(thatSwitch);
								}

							}

						}	
					}  				
				}  
			} 
		}		
    }



	void ToggleLights()
    {
		foreach (Light thisLight in theseLights)
		{
			if (thisLight.enabled == false)
			{
				// turn lights on
				changeLights(thisLight, true);

				// change appearance of switch
				changeSwitch(thisSwitchMats, true);

				// check if it's a rotatable switch

					// Change Second Switch
				if (thatSwitch != null)
				{
					// change appearance of switch
					changeSwitch(thatSwitchMats, true);
				}
			}

			else if (thisLight.enabled == true)
			{
				// turn lights on
				changeLights(thisLight, false);

				// change appearance of switch
				changeSwitch(thisSwitchMats, false);

				// Change Second Switch
				if (thatSwitch != null)
				{
					// change appearance of switch
					changeSwitch(thatSwitchMats, false);
				}
			}
		}
	}






	public void RotateSwitch(GameObject aswitch)
    {


		aswitch.transform.localScale = new Vector3(aswitch.transform.localScale.x, aswitch.transform.localScale.y, -aswitch.transform.localScale.z);




		//aswitch.transform.Rotate(new Vector3(180, 0, 0));

	}

}
