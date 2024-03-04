using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlickerEffect : MonoBehaviour
{
    [SerializeField] Light theLight;
    [SerializeField] int smoothing = 10;
    [SerializeField] float minIntensity = 0.5f;
    [SerializeField] float maxIntensity = 1.5f;

    private Queue<float> smoothQueue = new Queue<float>();
    private float lastSum = 0.0f;
    private Material[] theLightMats;
    private static readonly int EmissiveColor = Shader.PropertyToID("_EmissiveColor");
    private static readonly int Color1 = Shader.PropertyToID("_Color");

    void Start()
    {
        var thisLightParent = theLight.transform.parent.gameObject;
        theLightMats = thisLightParent.GetComponent<Renderer>().materials;
        StartCoroutine(FlickeringEffect());
    }

    IEnumerator FlickeringEffect()
    {
        while (true)
        {
            if (theLight.enabled)
            {
                while (smoothQueue.Count >= smoothing)
                {
                    lastSum -= (float)smoothQueue.Dequeue();
                }
                var litLiteCol = theLight.color;

                float newVal = Random.Range(minIntensity, maxIntensity);
                smoothQueue.Enqueue(newVal);
                lastSum += newVal;

                theLight.intensity = lastSum / (float)smoothQueue.Count;

                for (int i = 0; i < theLightMats.Length; i++)
                {
                    if (theLightMats[i].name.Contains("bulb") || theLightMats[i].name.Contains("cable"))
                    {
                        theLightMats[i].SetColor(Color1, litLiteCol);
                        theLightMats[i].SetColor(EmissiveColor, litLiteCol * (lastSum / (float)smoothQueue.Count));
                    }
                }
            }

            yield return null; // Wait for the next frame
        }
    }
}