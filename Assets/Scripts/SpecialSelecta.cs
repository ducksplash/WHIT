using UnityEngine;

public class SpecialSelecta : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject PortalPrefabRocky;
    public GameObject PortalPrefabBlocky;

    [Header("Spawn Chance (%)")]
    [Range(0f, 100f)]
    public float percentChance = 30f;

    private void Start()
    {
        TrySpawnPrefab();
    }

    private void TrySpawnPrefab()
    {
        // Safety check
        if (PortalPrefabRocky == null && PortalPrefabBlocky == null)
        {
            //Debug.LogWarning($"{name}: No portal prefabs assigned!");
            return;
        }

        // Roll chance to spawn
        float roll = Random.Range(0f, 100f);

        if (roll <= percentChance)
        {
            // Randomly choose which prefab to spawn
            GameObject selectedPrefab;

            if (PortalPrefabRocky != null && PortalPrefabBlocky != null)
            {
                selectedPrefab = (Random.value < 0.5f) ? PortalPrefabRocky : PortalPrefabBlocky;
            }
            else
            {
                // If only one is assigned, fall back to that
                selectedPrefab = PortalPrefabRocky != null ? PortalPrefabRocky : PortalPrefabBlocky;
            }

            // Instantiate at this transform’s position and rotation, and parent it
            GameObject newPortal = Instantiate(selectedPrefab, transform.position, transform.rotation, transform);

            //Debug.Log($"{name}: Spawned {(newPortal.name)} (roll={roll:F1} <= {percentChance}%)");
        }
        else
        {
            //Debug.Log($"{name}: No portal spawned (roll={roll:F1} > {percentChance}%)");
        }
    }
}