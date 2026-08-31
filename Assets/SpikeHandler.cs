using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SpikeHandler : MonoBehaviour
{
    public GameObject spikePrefab;
    [Range(0f, 100f)]
    public float clusterChance = 30f; // Percentage chance to spawn a spike
    public int maxTotalSpikes = 50; // Hard limit on total spikes
    public Vector3 spikeRotation = Vector3.zero; // Public rotation tweak

    private static int totalSpikesSpawned = 0; // Shared across instances
    private static List<GameObject> spawnedSpikes = new List<GameObject>(); // Track spikes for cleanup
    public bool spiking; // Flag for ongoing spawning

    public void BeginSpiking()
    {
        if (spikePrefab == null)
        {
            //Debug.LogError("SpikePrefab is null!");
            return;
        }

        spiking = true;
        ClearSpikes(); // Clear existing spikes before spawning

        // Process each floor tile
        GameObject[] floorTiles = GameObject.FindGameObjectsWithTag("FloorTile"); // Assume floor tiles tagged "FloorTile"
        if (floorTiles.Length == 0)
        {
            //Debug.LogError("No floor tiles tagged 'FloorTile'!");
            spiking = false;
            return;
        }
        //Debug.Log($"Found {floorTiles.Length} floor tiles");

        foreach (GameObject tile in floorTiles)
        {
            if (totalSpikesSpawned >= maxTotalSpikes) break;

            // RNG to decide if this tile spawns a spike
            if (UnityEngine.Random.value <= clusterChance / 100f)
            {
                SpawnCluster(tile);
            }
        }
        //Debug.Log($"Total spikes spawned: {totalSpikesSpawned}");
        spiking = false;
    }

    private void SpawnCluster(GameObject centerTile)
    {
        // Spawn on center tile
        if (!InstantiateSpike(centerTile.transform, Quaternion.Euler(spikeRotation)))
        {
            //Debug.LogWarning($"Failed to spawn spike on center tile: {centerTile.name}");
            return;
        }
        //Debug.Log($"Spawned center spike at {centerTile.name}");

        // Try adjacent tiles (2 and 3 steps up/down based on name number)
        int[] offsets = { 2, 3, -2, -3 };
        int adjacentCount = 0;

        foreach (int offset in offsets)
        {
            if (totalSpikesSpawned >= maxTotalSpikes) break;
            string centerNumberStr = centerTile.name.Replace("Floor_", "");
            if (int.TryParse(centerNumberStr, out int centerNumber))
            {
                int adjacentNumber = centerNumber + offset;
                GameObject adjacentTile = GameObject.Find("Floor_" + adjacentNumber);
                if (adjacentTile != null)
                {
                    // RNG for adjacent tile
                    if (UnityEngine.Random.value <= clusterChance / 100f)
                    {
                        if (InstantiateSpike(adjacentTile.transform, Quaternion.Euler(spikeRotation)))
                        {
                            adjacentCount++;
                            //Debug.Log($"Spawned adjacent spike at {adjacentTile.name}");
                        }
                    }
                }
                else
                {
                    //Debug.Log($"No adjacent tile: Floor_{adjacentNumber}");
                }
            }
        }

        //Debug.Log($"Spawned cluster at {centerTile.name} with {adjacentCount + 1} spikes");
    }

    private bool InstantiateSpike(Transform parentTile, Quaternion rotation)
    {
        if (totalSpikesSpawned >= maxTotalSpikes) return false;
        Vector3 position = parentTile.position + new Vector3(0, 0.05f, 0); // Align with tile
        GameObject nuSpyke = Instantiate(spikePrefab, position, rotation, parentTile);
        nuSpyke.name = "spikes";
        DungeonGenerator.Instance.floorTilesCount--; // detract from floor tiles total 
        nuSpyke.transform.localScale = spikePrefab.transform.localScale; // Use prefab scale
        spawnedSpikes.Add(nuSpyke); // Track for cleanup
        totalSpikesSpawned++;

        if (parentTile.gameObject.GetComponent<ColorFloor>())
        {
            parentTile.gameObject.GetComponent<ColorFloor>().ranOver = true;
        }
        
        //Debug.Log($"Spawned spike at {position}, parent: {parentTile.name}");
        return true;
    }

    private void ClearSpikes()
    {
        foreach (GameObject spike in spawnedSpikes)
        {
            if (spike != null) DestroyImmediate(spike);
        }
        spawnedSpikes.Clear();
        totalSpikesSpawned = 0;
        //Debug.Log("Cleared all spikes");
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SpikeHandler))]
    public class SpikeHandlerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            SpikeHandler spikeHandler = (SpikeHandler)target;
            if (GUILayout.Button("Regenerate Spikes"))
            {
                if (!spikeHandler.spiking)
                {
                    spikeHandler.BeginSpiking();
                }
                else
                {
                    //Debug.LogWarning("Spiking in progress, please wait!");
                }
            }
        }
    }
#endif
}