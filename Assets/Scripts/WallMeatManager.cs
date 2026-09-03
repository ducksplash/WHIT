using System.Collections.Generic;
using UnityEngine;

public enum WallMeaterial
{
    Graffiti1,
    Graffiti2,
    Blood1
}

public class WallMeatManager : MonoBehaviour
{
    public static WallMeatManager Instance { get; private set; }

    [SerializeField]
    private List<WallMeat> wallMeats;

    [SerializeField]
    private List<Material> usedMaterials = new List<Material>();

    public float ChanceOfWallMeat = 10;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public Material GetWallMeat(WallMeaterial requestedMaterial)
    {
        foreach (WallMeat wallMeat in wallMeats)
        {
            if (wallMeat != null && wallMeat.wallMeaterial == requestedMaterial)
            {
                return wallMeat.material;
            }
        }

        Debug.LogWarning($"WallMeatManager: No material found for {requestedMaterial}");
        return null;
    }

    public Material GetRandomWallMeat()
    {
        if (Random.Range(0f, 100f) > ChanceOfWallMeat)
        {
            return null;
        }

        List<WallMeat> available = new List<WallMeat>();

        foreach (WallMeat wallMeat in wallMeats)
        {
            if (wallMeat != null && wallMeat.material != null && !usedMaterials.Contains(wallMeat.material))
            {
                available.Add(wallMeat);
            }
        }

        if (available.Count == 0)
        {
            Debug.LogWarning("WallMeatManager: No unused materials remaining.");
            return null;
        }

        WallMeat chosen = available[Random.Range(0, available.Count)];
        usedMaterials.Add(chosen.material);

        return chosen.material;
    }
}