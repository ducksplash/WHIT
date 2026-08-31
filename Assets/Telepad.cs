using System;
using System.Collections;
using UnityEngine;

public class Telepad : MonoBehaviour
{
    public Vector3 destination;
    public bool thisPadActive = true;
    public MeshRenderer telepadRenderer;
    private Material tileMaterial; 

    [Header("Pad Settings")]
    public TelepadType telepadType;
    public float reactivateDelay = 10f;

    [Header("Inactive Color")]
    public Color inactiveColor = Color.gray * 0.2f;

    private Light spotLight;
    
    // Cache original colours
    private Color originalBaseColor;
    private Color originalEmissiveColor;
    private Color originalEmissiveLDRColor;

    private void Start()
    {
        telepadRenderer = GetComponent<MeshRenderer>();
        spotLight = GetComponentInChildren<Light>();
        if (telepadRenderer != null)
        {
            tileMaterial = new Material(telepadRenderer.material);
            telepadRenderer.material = tileMaterial;

            // Cache original colours from material
            originalBaseColor = tileMaterial.GetColor("_BaseColor");
            originalEmissiveColor = tileMaterial.GetColor("_EmissiveColor");
            originalEmissiveLDRColor = tileMaterial.GetColor("_EmissiveColorLDR");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("standing on teleporter");

        if (Player.Instance != null && !GameMaster.Instance.PLAYERBUSY) return;
        
        if (other.CompareTag("Player") && thisPadActive)
        {
            thisPadActive = false;
            GameMaster.Instance.PLAYERBUSY = true;
            StartCoroutine(TeleportWithCooldown(other));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        SetTileColorInactive();
    }

    private void OnTriggerExit(Collider other)
    {
        SetTileColorInactive();
    }

    private IEnumerator TeleportWithCooldown(Collider other)
    {
        CharacterController controller = other.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            Vector3 adjustedDestination = destination + Vector3.up * 0.5f;
            other.transform.position = adjustedDestination;
            controller.enabled = true;
        }
        else
        {
            other.transform.position = destination + Vector3.up * 0.5f;
        }

        yield return new WaitForSeconds(reactivateDelay);
        GameMaster.Instance.PLAYERBUSY = false;
        thisPadActive = true;
        SetTileColorActive();
    }

    private IEnumerator ColorCooldown()
    {
        yield return new WaitForSeconds(reactivateDelay);
        thisPadActive = true;
        SetTileColorActive();
    }

    public void SetTileColorInactive()
    {
        if (telepadRenderer == null || tileMaterial == null || !thisPadActive) return;

        spotLight.enabled = false;
        
        tileMaterial.SetColor("_BaseColor", inactiveColor);
        tileMaterial.SetColor("_EmissiveColor", inactiveColor);
        tileMaterial.SetColor("_EmissiveColorLDR", inactiveColor);

        StartCoroutine(ColorCooldown());
    }

    public void SetTileColorActive()
    {
        if (telepadRenderer == null || tileMaterial == null) return;

        spotLight.enabled = true;
        
        tileMaterial.SetColor("_BaseColor", originalBaseColor);
        tileMaterial.SetColor("_EmissiveColor", originalEmissiveColor);
        tileMaterial.SetColor("_EmissiveColorLDR", originalEmissiveLDRColor);
    }
}

public enum TelepadType
{
    RedTelepad,
    BlueTelepad
}
