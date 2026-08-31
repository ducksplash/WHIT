using UnityEngine;

public class LightMan : MonoBehaviour
{

    [Header("Light Flicker Settings")]
    public Light lightBulb;                 
    
    private int emissiveLightCount = 0; // Tracks active emissive lights
    private Material material;
    private Color baseEmissiveColor;
    private Color baseAlbedoColor;
    private bool isActiveRealtimeLight;      // Controls realtime Light component
    private bool isEmissiveActive;           // Controls emissive material

    void Start()
    {
        // Decide if realtime light is active (max 7 lights, 1 in 200 chance)
        isActiveRealtimeLight = lightBulb != null && DungeonGenerator.Instance.lightList.Count < DungeonGenerator.Instance.maximumRealtimeLights && Random.Range(1, 200) == 25;

        // Emissive is active if realtime light is active or with 1 in 15 chance (up to maxEmissiveLights)
        isEmissiveActive = isActiveRealtimeLight || (emissiveLightCount < DungeonGenerator.Instance.maximumEmissiveLights && Random.Range(1, 15) == 5);
        if (isEmissiveActive)
        {
            emissiveLightCount++;
        }


        // Initialize light
        if (lightBulb != null)
        {
            lightBulb.enabled = isActiveRealtimeLight;
            if (isActiveRealtimeLight)
            {
                DungeonGenerator.Instance.lightList.Add(lightBulb);
            }
            else
            {
                Destroy(lightBulb.gameObject);
                gameObject.SetActive(Random.Range(0,30) == 15); // disable disused panels sometimes
            }
        }


        if (!GetComponent<Renderer>()) return;
        
        material = GetComponent<Renderer>().material;

        // Cache base emissive color
        if (material.HasProperty("_EmissiveColor"))
        {
            baseEmissiveColor = material.GetColor("_EmissiveColor");
            if (baseEmissiveColor.maxColorComponent > 0f) baseEmissiveColor /= baseEmissiveColor.maxColorComponent;
        }
        else
        {
            Debug.LogError("Material does not have _EmissiveColor property. Ensure it uses HDRP/Lit shader.");
            enabled = false;
            return;
        }

        // Initialize emission state
        if (isEmissiveActive)
        {
            material.EnableKeyword("_EMISSIVECOLOR");
            material.SetFloat("_EmissiveIntensity", 2000);
            material.SetColor("_EmissiveColor", Color.white * 40);
        }
        else
        {
            material.DisableKeyword("_EMISSIVECOLOR");
            material.SetFloat("_EmissiveIntensity", 0f);
            material.SetColor("_EmissiveColor", Color.black);
        }
        
        
    }
}