using System;
using UnityEngine;

public class LootChooser : MonoBehaviour
{
    public GameObject goldBarsPrefab; // Prefab for gold bars
    public GameObject chickenPrefab;  // Prefab for chicken
    public GameObject twoXPrefab;  // Prefab for 2x
    public Transform lootParentTransform; // Parent for spawned loot
    public Animator animator;
    
    [Serializable]
    private class LootItem
    {
        public GameObject prefab;
        public float weight; // Weight for random selection (percentage)
    }

    private LootItem[] lootTable = new LootItem[]
    {
        new LootItem { prefab = null, weight = 10f }, // Gold bars (10% chance)
        new LootItem { prefab = null, weight = 50f },  // Chicken (44% chance)
        new LootItem { prefab = null, weight = 40f }  // 2x ()
    };

    private void Start()
    {
        // Assign prefabs to loot table
        lootTable[0].prefab = goldBarsPrefab;
        lootTable[1].prefab = chickenPrefab;
        lootTable[2].prefab = twoXPrefab;

        // Set loot parent transform
        lootParentTransform = transform;

        // Select and spawn a loot
        SelectALoot();
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    private void SelectALoot()
    {
        // Calculate total weight
        float totalWeight = 0f;
        foreach (var item in lootTable)
        {
            totalWeight += item.weight;
        }

        // Generate random value (0 to 100)
        float randomValue = UnityEngine.Random.Range(0f, 100f);

        // Check if random value falls in no-spawn range (46%)
        if (randomValue > totalWeight)
        {
            return; // No loot spawned
        }

        // Select loot based on weight
        float currentWeight = 0f;
        foreach (var item in lootTable)
        {
            currentWeight += item.weight;
            if (randomValue <= currentWeight && item.prefab != null)
            {
                // Instantiate the selected loot
                Instantiate(item.prefab, lootParentTransform.position, item.prefab.transform.rotation, lootParentTransform);
                
                
                return;
            }
        }
    }
}