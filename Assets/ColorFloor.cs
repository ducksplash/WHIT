using UnityEngine;

public class ColorFloor : MonoBehaviour
{
    public bool ranOver;
    public Renderer meshRenderer;
    public Light attachedLight;

    private Material tileMaterial; 
    public Color tileColor = Color.lightGray; 

    private void Awake()
    {
        if (meshRenderer != null)
        {
            tileMaterial = new Material(meshRenderer.material); 
            meshRenderer.material = tileMaterial; 
        }
    }

    public void SetTileColor()
    {
        if (meshRenderer == null || tileMaterial == null) return;

        tileMaterial.SetColor("_BaseColor", tileColor);
    }

    private void OnDestroy()
    {
        if (tileMaterial != null)
        {
            Destroy(tileMaterial); // Clean up material instance
        }
    }
}