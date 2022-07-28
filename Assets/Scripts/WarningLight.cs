using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningLight : MonoBehaviour
{

	public float lightspeed = 10;

    void FixedUpdate()
	{
		gameObject.transform.Rotate(new Vector3(0, 0, 30) * (Time.fixedDeltaTime * lightspeed));
	}
}
