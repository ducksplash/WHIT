using System.Collections;  
using System.Collections.Generic;  
using UnityEngine;
using VLB;

public class LIGHTS : MonoBehaviour
{
	public GameObject thisSwitch;
	public GameObject thatSwitch;
	private string thisSwitchName;
	private string thatSwitchName;
	private Material[] thatSwitchMats;
	private Material[] thisSwitchMats;
	public Light[] theseLights;
	public GameObject theseEmissives;
	public string lightTag;
	public float LightSwitchEmissionStrength = 5f;
	//private Material[] theLightMats;
	public VolumetricLightBeamSD[] SpotlightBeams;
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
			lightEmissionColour = new Color(0.8f,0,0,1);
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
		
		Material[] theLightMats = myLight.transform.parent.GetComponent<Renderer>().materials;

		
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


	void changeEmissives(GameObject myEmissive, bool powerState)
	{
		// Read Colour
		Material[] theMats = myEmissive.transform.parent.GetComponent<Renderer>().materials;

		
		
		for (int i = 0; i < theMats.Length; i++)
		{
			
			var litLiteCol = theMats[0].GetColor("_Color");
			
			if (powerState)
			{
				theMats[i].SetColor("_Color", litLiteCol);
				theMats[i].SetColor("_EmissiveColor", litLiteCol * 5f);
			}	
			else
			{
				var offColour = new Color(0,0,0,1f);
				theMats[i].SetColor("_Color", offColour);
				theMats[i].SetColor("_EmissiveColor", offColour);
			}
		}
	}


    // Update is called once per frame
    void Update()
    {
		if (GameMaster.POWER_SUPPLY_ENABLED || GameMaster.THISLEVEL == "NorasFlat")
		{
			if (Input.GetMouseButtonDown(1))
			{  
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);  
				RaycastHit hit;  
				if (Physics.Raycast(ray, out hit)) 
				{  
	  
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
		    bool enableLight = !thisLight.enabled; // Toggle the state of the light

		    // Toggle the light
		    changeLights(thisLight, enableLight);

		    // Toggle the appearance of the switch
		    changeSwitch(thisSwitchMats, enableLight);

		    // Toggle the appearance of the second switch if available
		    if (thatSwitch != null)
		    {
			    changeSwitch(thatSwitchMats, enableLight);
		    }
	    }

	    foreach (VolumetricLightBeamSD VLBSD in SpotlightBeams)
	    {
		    VLBSD.enabled = !VLBSD.enabled;
	    }

	    
		
	}






	public void RotateSwitch(GameObject aswitch)
    {


		aswitch.transform.localScale = new Vector3(aswitch.transform.localScale.x, aswitch.transform.localScale.y, -aswitch.transform.localScale.z);




		//aswitch.transform.Rotate(new Vector3(180, 0, 0));

	}

}
