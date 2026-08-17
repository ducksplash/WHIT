using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TreeLine
{
    public string LineName = "New Line";
    public bool Enabled = true;
    public Transform Source;
    public Transform End;
    public List<GameObject> Prefabs = new List<GameObject>();
    public Vector3 Direction = Vector3.forward;
    public float Speed = 1f;
    public float SpawnInterval = 1f;
    public int InitialPoolSizePerPrefab = 5;

    [HideInInspector] public float SpawnTimer;
    [HideInInspector] public Dictionary<GameObject, Queue<GameObject>> Pools = new Dictionary<GameObject, Queue<GameObject>>();
    [HideInInspector] public List<ActiveTreeInstance> ActiveTrees = new List<ActiveTreeInstance>();
    [HideInInspector] public Vector3 CachedDirection;
}

public class ActiveTreeInstance
{
    public GameObject Instance;
    public GameObject SourcePrefab;
}

public class TreeLineSpawner : MonoBehaviour
{
    public List<TreeLine> Lines = new List<TreeLine>();

    void Awake()
    {
        foreach (TreeLine line in Lines)
        {
            InitializeLine(line);
        }
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;

        foreach (TreeLine line in Lines)
        {
            if (!line.Enabled || line.Source == null || line.End == null)
            {
                continue;
            }

            HandleSpawning(line, deltaTime);
            HandleMovementAndRemoval(line, deltaTime);
        }
    }

    void InitializeLine(TreeLine line)
    {
        line.CachedDirection = line.Direction.normalized;
        line.SpawnTimer = line.SpawnInterval;

        foreach (GameObject prefab in line.Prefabs)
        {
            if (prefab == null || line.Pools.ContainsKey(prefab))
            {
                continue;
            }

            Queue<GameObject> pool = new Queue<GameObject>();

            for (int i = 0; i < line.InitialPoolSizePerPrefab; i++)
            {
                GameObject instance = Instantiate(prefab, transform);
                instance.SetActive(false);
                pool.Enqueue(instance);
            }

            line.Pools.Add(prefab, pool);
        }
    }

    void HandleSpawning(TreeLine line, float deltaTime)
    {
        if (line.Prefabs.Count == 0)
        {
            return;
        }

        line.SpawnTimer -= deltaTime;

        if (line.SpawnTimer <= 0f)
        {
            SpawnTree(line);
            line.SpawnTimer = line.SpawnInterval;
        }
    }

    void SpawnTree(TreeLine line)
    {
        GameObject prefab = line.Prefabs[Random.Range(0, line.Prefabs.Count)];

        if (prefab == null)
        {
            return;
        }

        if (!line.Pools.ContainsKey(prefab))
        {
            line.Pools.Add(prefab, new Queue<GameObject>());
        }

        Queue<GameObject> pool = line.Pools[prefab];
        GameObject instance;

        if (pool.Count > 0)
        {
            instance = pool.Dequeue();
        }
        else
        {
            instance = Instantiate(prefab, transform);
        }

        instance.transform.position = line.Source.position;
        instance.transform.rotation = line.Source.rotation;
        instance.SetActive(true);

        line.ActiveTrees.Add(new ActiveTreeInstance
        {
            Instance = instance,
            SourcePrefab = prefab
        });
    }

    void HandleMovementAndRemoval(TreeLine line, float deltaTime)
    {
        Vector3 direction = line.CachedDirection;
        Vector3 endPosition = line.End.position;

        for (int i = line.ActiveTrees.Count - 1; i >= 0; i--)
        {
            ActiveTreeInstance activeTree = line.ActiveTrees[i];

            if (activeTree.Instance == null)
            {
                line.ActiveTrees.RemoveAt(i);
                continue;
            }

            activeTree.Instance.transform.position += direction * line.Speed * deltaTime;

            Vector3 toInstance = activeTree.Instance.transform.position - endPosition;

            if (Vector3.Dot(toInstance, direction) >= 0f)
            {
                ReturnToPool(line, activeTree);
                line.ActiveTrees.RemoveAt(i);
            }
        }
    }

    void ReturnToPool(TreeLine line, ActiveTreeInstance activeTree)
    {
        activeTree.Instance.SetActive(false);

        if (!line.Pools.ContainsKey(activeTree.SourcePrefab))
        {
            line.Pools.Add(activeTree.SourcePrefab, new Queue<GameObject>());
        }

        line.Pools[activeTree.SourcePrefab].Enqueue(activeTree.Instance);
    }

    public void AddLine(TreeLine line)
    {
        Lines.Add(line);
        InitializeLine(line);
    }

    public void RemoveLine(TreeLine line)
    {
        if (!Lines.Contains(line))
        {
            return;
        }

        foreach (ActiveTreeInstance activeTree in line.ActiveTrees)
        {
            if (activeTree.Instance != null)
            {
                Destroy(activeTree.Instance);
            }
        }

        foreach (Queue<GameObject> pool in line.Pools.Values)
        {
            while (pool.Count > 0)
            {
                GameObject instance = pool.Dequeue();

                if (instance != null)
                {
                    Destroy(instance);
                }
            }
        }

        Lines.Remove(line);
    }

    public void AddPrefabToLine(TreeLine line, GameObject prefab)
    {
        if (prefab == null || line.Prefabs.Contains(prefab))
        {
            return;
        }

        line.Prefabs.Add(prefab);

        if (!line.Pools.ContainsKey(prefab))
        {
            Queue<GameObject> pool = new Queue<GameObject>();

            for (int i = 0; i < line.InitialPoolSizePerPrefab; i++)
            {
                GameObject instance = Instantiate(prefab, transform);
                instance.SetActive(false);
                pool.Enqueue(instance);
            }

            line.Pools.Add(prefab, pool);
        }
    }

    public void RemovePrefabFromLine(TreeLine line, GameObject prefab)
    {
        if (!line.Prefabs.Contains(prefab))
        {
            return;
        }

        line.Prefabs.Remove(prefab);

        for (int i = line.ActiveTrees.Count - 1; i >= 0; i--)
        {
            if (line.ActiveTrees[i].SourcePrefab == prefab)
            {
                if (line.ActiveTrees[i].Instance != null)
                {
                    Destroy(line.ActiveTrees[i].Instance);
                }

                line.ActiveTrees.RemoveAt(i);
            }
        }

        if (line.Pools.ContainsKey(prefab))
        {
            Queue<GameObject> pool = line.Pools[prefab];

            while (pool.Count > 0)
            {
                GameObject instance = pool.Dequeue();

                if (instance != null)
                {
                    Destroy(instance);
                }
            }

            line.Pools.Remove(prefab);
        }
    }
}