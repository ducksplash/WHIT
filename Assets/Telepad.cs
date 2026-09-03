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

            originalBaseColor = tileMaterial.GetColor("_BaseColor");
            originalEmissiveColor = tileMaterial.GetColor("_EmissiveColor");
            originalEmissiveLDRColor = tileMaterial.GetColor("_EmissiveColorLDR");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (Player.Instance == null) return;

        if (telepadType == TelepadType.BlueTelepad)
        {
            if (!DungeonGenerator.Instance.allowBackwardTravel) return;
        }

        if (other.transform.root != Player.Instance.transform.root) return;

        if (!Player.Instance.CanTeleport) return;

        if (!thisPadActive) return;

        Debug.Log($"Telepad '{name}' triggered. " + $"Destination: {destination}");
        
        Player.Instance.CanTeleport = false;

        // Disable this individual telepad.
        thisPadActive = false;

        StartCoroutine(TeleportWithCooldown());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Player.Instance == null)
            return;

        // Only react to the player's collider.
        if (other.transform.root != Player.Instance.transform.root)
            return;

        SetTileColorInactive();
    }

    private void OnTriggerExit(Collider other)
    {
        if (Player.Instance == null)
            return;

        // Only react to the player's collider.
        if (other.transform.root != Player.Instance.transform.root)
            return;

        // If this pad is still inactive because it is on cooldown,
        // don't turn it back on visually.
        if (!thisPadActive)
            return;

        SetTileColorActive();
    }

    private IEnumerator TeleportWithCooldown()
    {
        Vector3 target = destination;

        Debug.Log($"Telepad '{name}' teleporting player " + $"from {Player.Instance.transform.position} to {target}");


        Player.Instance.SpawnOverride(target);

        Player.Instance.CanTeleport = false;

        Debug.Log($"Teleport complete. " + $"Player position: {Player.Instance.transform.position}. " + $"CanTeleport = {Player.Instance.CanTeleport}");
        
        yield return new WaitForSeconds(reactivateDelay);

        Player.Instance.CanTeleport = true;

        thisPadActive = true;

        SetTileColorActive();

        Debug.Log($"Telepad '{name}' reactivated. " + $"CanTeleport = {Player.Instance.CanTeleport}");
    }

    public void SetTileColorInactive()
    {
        if (telepadRenderer == null || tileMaterial == null) return;

        if (spotLight != null) spotLight.enabled = false;

        tileMaterial.SetColor("_BaseColor", inactiveColor);
        tileMaterial.SetColor("_EmissiveColor", inactiveColor);
        tileMaterial.SetColor("_EmissiveColorLDR", inactiveColor);
    }

    public void SetTileColorActive()
    {
        if (telepadRenderer == null || tileMaterial == null)
            return;

        if (spotLight != null) spotLight.enabled = true;

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
