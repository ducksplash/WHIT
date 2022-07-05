using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

	public bool WaitingForTorch;

	// Start is called before the first frame update
	void Start()
	{

		WaitingForTorch = true;
		lightBeam = gameObject.GetComponentInChildren<Light>();
		torchAnimator = theTorch.GetComponentInChildren<Animator>();

		theTorch.SetActive(false);

	}

	void Update()
	{

		if (WaitingForTorch)
		{
			if (GameMaster.TORCHCOLLECTED)
			{
				theTorch.SetActive(true);
				WaitingForTorch = false;
			}
		}

		if (GameMaster.TORCHCOLLECTED)
		{
			if (!GameMaster.INMENU)
			{
				if (!GameMaster.FROZEN)
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
