using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SpikeHandler : MonoBehaviour
{
    public bool GenerateSpikes;
    public GameObject spikePrefab;
    [Range(0f, 100f)] 
    public float clusterChance = 30f; 
    public int maxTotalSpikes = 50; 
    public Vector3 spikeRotation = Vector3.zero; 

    private static int totalSpikesSpawned = 0; // Shared across instances
    private static List<GameObject> spawnedSpikes = new List<GameObject>(); // Track spikes for cleanup
    public bool spiking; 

    public void BeginSpiking()
    {
        if (!GenerateSpikes) return;
        
        if (spikePrefab == null)
        {
            return;
        }

        spiking = true;
        ClearSpikes(); 

        GameObject[] floorTiles = GameObject.FindGameObjectsWithTag("FloorTile"); 
        if (floorTiles.Length == 0)
        {
            spiking = false;
            return;
        }

        foreach (GameObject tile in floorTiles)
        {
            if (totalSpikesSpawned >= maxTotalSpikes) break;

            if (UnityEngine.Random.value <= clusterChance / 100f)
            {
                SpawnCluster(tile);
            }
        }

        spiking = false;
    }

    private void SpawnCluster(GameObject centerTile)
    {
        if (!GenerateSpikes) return;

        if (!InstantiateSpike(centerTile.transform, Quaternion.Euler(spikeRotation)))
        {
            return;
        }

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
                        }
                    }
                }
            }
        }

    }

    private bool InstantiateSpike(Transform parentTile, Quaternion rotation)
    {
        if (!GenerateSpikes) return false;
        if (totalSpikesSpawned >= maxTotalSpikes) return false;
        Vector3 position = parentTile.position + new Vector3(0, 0.05f, 0); 
        GameObject nuSpyke = Instantiate(spikePrefab, position, rotation, parentTile);
        nuSpyke.name = "spikes";
        DungeonGenerator.Instance.floorTilesCount--; 
        nuSpyke.transform.localScale = spikePrefab.transform.localScale; 
        spawnedSpikes.Add(nuSpyke); 
        totalSpikesSpawned++;

        if (parentTile.gameObject.GetComponent<ColorFloor>())
        {
            parentTile.gameObject.GetComponent<ColorFloor>().ranOver = true;
        }
        
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
            }
        }
    }
#endif
}