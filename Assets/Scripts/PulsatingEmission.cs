using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PulsatingEmission : MonoBehaviour
{
    public Color startColor = Color.black; // Initial dark color
    public Color endColor = Color.black; // Back to dark color
    public float lerpDuration = 3.0f; // Back to dark color
    public float emissionIntensity = 2.0f; // Total duration for one cycle
    private List<Material> EmissiveList = new List<Material>(); // List of materials to pulsate
    public List<MeshRenderer> MeshRenderers = new List<MeshRenderer>(); // List of MeshRenderers
    
    
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    private void Start()
    {
        // Clear the list before populating it to avoid duplicates
        EmissiveList.Clear();

        // Iterate through each MeshRenderer in the list
        foreach (MeshRenderer meshRenderer in MeshRenderers)
        {
            // Add all materials of the current MeshRenderer to the EmissiveList
            foreach (Material material in meshRenderer.materials)
            {
                EmissiveList.Add(material);
            }
        }

        StartCoroutine(LerpEmission());
    }

    private IEnumerator LerpEmission()
    {
        while (true)
        {
            float elapsedTime = 0.0f;

            while (elapsedTime < lerpDuration)
            {
                float t = elapsedTime / lerpDuration; // Normalize time between 0 and 1

                // Use custom normalized time value for pulsating effect
                float pulseT = Mathf.PingPong(t * 2, 1.0f);

                // Lerp between startColor and endColor using the pulseT value
                Color emissionColor = Color.Lerp(startColor, endColor, pulseT);
                SetEmissionColor(emissionColor);

                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }

    // Set emission color for all materials in EmissiveList
    private void SetEmissionColor(Color emissionColor)
    {
        foreach (Material emissiveMaterial in EmissiveList)
        {
            emissiveMaterial.SetColor(EmissionColor, emissionColor * emissionIntensity);
        }
    }
}
