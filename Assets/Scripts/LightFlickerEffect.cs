using UnityEngine;
using System.Collections.Generic;

public class LightFlickerEffect : MonoBehaviour {
	
    private Light light;
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
		
		light = gameObject.GetComponent<Light>();
		
         smoothQueue = new Queue<float>(smoothing);
    }

    void Update()
	{


		if (GameMaster.POWER_SUPPLY_ENABLED && light.enabled)
		{

			while (smoothQueue.Count >= smoothing) {
				lastSum -= smoothQueue.Dequeue();
			}

			var thisLightParent = light.transform.parent.gameObject;
				
			theLightMats = thisLightParent.GetComponent<Renderer>().materials;


			var litLiteCol = light.color;

			float newVal = Random.Range(minIntensity, maxIntensity);
			smoothQueue.Enqueue(newVal);
			lastSum += newVal;

			light.intensity = lastSum / (float)smoothQueue.Count;
			
			for (int i = 0; i < theLightMats.Length; i++)
			{
			
				if (theLightMats[i].name.Contains("bulb"))
				{
					theLightMats[i].SetColor("_EmissionColor", litLiteCol * (lastSum / (float)smoothQueue.Count));
					theLightMats[i].SetColor("_Color", litLiteCol);
				}
			}
		
		}
		
		
    }

}