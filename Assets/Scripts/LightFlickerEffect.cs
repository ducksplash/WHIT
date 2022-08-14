using UnityEngine;
using System.Collections.Generic;

public class LightFlickerEffect : MonoBehaviour {
	
    public Light thelight;
    private Material[] theLightMats;
	
    public float minIntensity = 0f;
	
    public float maxIntensity = 1f;
	
	
    public int smoothing = 5;

    Queue<float> smoothQueue;
    float lastSum = 0;

    public void Reset() {
        smoothQueue.Clear();
        lastSum = 0;
    }

    void Start() {

		thelight = gameObject.GetComponent<Light>();
		
         smoothQueue = new Queue<float>(smoothing);
    }

    void FixedUpdate()
	{


		if (thelight.enabled)
		{

			while (smoothQueue.Count >= smoothing) {
				lastSum -= smoothQueue.Dequeue();
			}

			var thisLightParent = thelight.transform.parent.gameObject;
				
			theLightMats = thisLightParent.GetComponent<Renderer>().materials;


			var litLiteCol = thelight.color;

			float newVal = Random.Range(minIntensity, maxIntensity);
			smoothQueue.Enqueue(newVal);
			lastSum += newVal;

			thelight.intensity = lastSum / (float)smoothQueue.Count;
			
			for (int i = 0; i < theLightMats.Length; i++)
			{
			
				if (theLightMats[i].name.Contains("bulb") || theLightMats[i].name.Contains("cable"))
				{
					theLightMats[i].SetColor("_Color", litLiteCol);
					theLightMats[i].SetColor("_EmissiveColor", litLiteCol * (lastSum / (float)smoothQueue.Count));
				}
			}
		
		}
		
		
    }

}