using UnityEngine;

public class ColorFloor : MonoBehaviour
{
    public bool ranOver;
    public Renderer meshRenderer;
    public Light attachedLight;

    private Material tileMaterial; // Per-tile material instance
    private static readonly Color emissionColor = Color.red* 0.2f; // Cached emission color

    private void Awake()
    {
        if (meshRenderer != null)
        {
            tileMaterial = new Material(meshRenderer.material); // Create unique material instance
            meshRenderer.material = tileMaterial; // Assign to renderer
        }
    }

    public void SetTileColor(Color baseColor)
    {
        if (meshRenderer == null || tileMaterial == null) return;

        tileMaterial.SetColor("_BaseColor", baseColor);
        tileMaterial.SetColor("_EmissiveColor", emissionColor);
        tileMaterial.SetColor("_EmissiveColorLDR", emissionColor);
    }

    private void OnDestroy()
    {
        if (tileMaterial != null)
        {
            Destroy(tileMaterial); // Clean up material instance
        }
    }
}