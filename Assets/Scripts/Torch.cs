using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VLB;

public class Torch : MonoBehaviour
{

	public Light lightBeam;
	public GameObject theTorch;
	public static bool torchToggle;
	public int lightIntensity = 4;
	private Animator torchAnimator;
	//public GameObject thePhone;
	//public GameObject thePC;
	private bool phoneBool;
	private bool pcBool;
	public Image torchimg;
	public Sprite litsprite;
	public Sprite unlitsprite;
	public VolumetricLightBeamSD SpotlightBeam;

	
	
	public bool WaitingForTorch;

	// Start is called before the first frame update
	void Start()
	{

		torchimg.sprite = litsprite;
		WaitingForTorch = true;
		torchToggle = true;
		lightBeam = gameObject.GetComponentInChildren<Light>();
		torchAnimator = theTorch.GetComponentInChildren<Animator>();

		theTorch.SetActive(false);
		torchimg.transform.parent.GetComponent<CanvasGroup>().alpha = 0f;

	}

	void Update()
	{

		if (WaitingForTorch)
		{
			if (GameMaster.TORCHCOLLECTED)
			{
				theTorch.SetActive(true);
				WaitingForTorch = false;
				torchimg.transform.parent.GetComponent<CanvasGroup>().alpha = 1f;
			}
		}

		if (GameMaster.TORCHCOLLECTED)
		{
			if (!GameMaster.INMENU)
			{
				if (!GameMaster.FROZEN)
				{

					if (Input.GetMouseButtonUp(2) || InputManager.GetKeyUp("torch"))
					{
						if (!torchToggle)
						{
							lightBeam.enabled = true;
							torchimg.sprite = litsprite;
							torchToggle = true;
							SpotlightBeam.enabled = true;
						}
						else
						{
							lightBeam.enabled = false;
							torchimg.sprite = unlitsprite;

							SpotlightBeam.enabled = false;
							torchToggle = false;
							//Debug.Log("off");
						}
					}

					if (Input.GetMouseButtonUp(0))
					{
						if (!pickup.hasobject)
						{
							torchAnimator.SetTrigger("swing");
						}
					}
					else
					{
						torchAnimator.SetTrigger("idle");
					}

				}
			}


		}



	}


}
