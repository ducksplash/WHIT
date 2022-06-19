using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Torch : MonoBehaviour
{
	
	public Light lightBeam;
	public GameObject theTorch;
	private bool torchToggle = false;
	public int lightIntensity = 4;
	private Animator torchAnimator;
	public GameObject thePhone;
	public GameObject thePC;
	private bool phoneBool;
	private bool pcBool;
	public Image torchimg;
	public Sprite litsprite;
	public Sprite unlitsprite;


	// Start is called before the first frame update
	void Start()
    {
		
		lightBeam = gameObject.GetComponentInChildren<Light>();
        torchAnimator = theTorch.GetComponentInChildren<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
		phoneBool = thePhone.GetComponent<phone>().PT;
		pcBool = thePC.GetComponent<ManagersPC>().usingComputer;
		
		
		
		if (phoneBool == false && pcBool == false)
		{
		
		
			if (Input.GetMouseButtonUp(2) || Input.GetButtonUp("Flashlight"))
			{
				if (!torchToggle)
				{
				lightBeam.enabled = true;
					torchimg.sprite = litsprite;
				torchToggle = true;
				}
				else
				{
				lightBeam.enabled = false;
				torchimg.sprite = unlitsprite;

				torchToggle = false;
				//Debug.Log("off");
				}
			}
			
			if (Input.GetMouseButtonUp(0))
			{  
				torchAnimator.SetTrigger("swing");		
			}
			else
			{
				torchAnimator.SetTrigger("idle");	
			}
		
		}
        
    }
}
