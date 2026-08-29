using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


[System.Serializable]
public class TreeLine
{
    public string LineName = "New Line";
    public bool Enabled = true;
    public Transform Source;
    public Transform End;
    public List<GameObject> Prefabs = new List<GameObject>();
    public bool Reverse = false;
    public float Speed = 1f;
    public float MinSpawnInterval = 1f;
    public float MaxSpawnInterval = 1f;
    public int InitialPoolSizePerPrefab = 5;

    [HideInInspector] public float SpawnTimer;
    [HideInInspector] public Dictionary<GameObject, Queue<GameObject>> Pools = new Dictionary<GameObject, Queue<GameObject>>();
    [HideInInspector] public Dictionary<GameObject, Quaternion> PrefabRotations = new Dictionary<GameObject, Quaternion>();
    [HideInInspector] public List<ActiveTreeInstance> ActiveTrees = new List<ActiveTreeInstance>();
    [HideInInspector] public List<GameObject> RecentPrefabs = new List<GameObject>();
}

public class ActiveTreeInstance
{
    public GameObject Instance;
    public GameObject SourcePrefab;
}


public class TreeLineSpawner : MonoBehaviour
{
    public List<TreeLine> Lines = new List<TreeLine>();

    void OnEnable()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        ClearAllChildren();

        foreach (TreeLine line in Lines)
        {
            InitializeLine(line);
        }
    }

    void OnDisable()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        ClearAllChildren();
    }

    void ClearAllChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            SafeDestroy(transform.GetChild(i).gameObject);
        }

        foreach (TreeLine line in Lines)
        {
            line.Pools.Clear();
            line.PrefabRotations.Clear();
            line.ActiveTrees.Clear();
            line.RecentPrefabs.Clear();
        }
    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        float deltaTime = Time.deltaTime;

        foreach (TreeLine line in Lines)
        {
            if (!line.Enabled)
            {
                continue;
            }

            if (line.Source == null || line.End == null || line.Prefabs.Count == 0)
            {
                Debug.LogWarning("TreeLineSpawner: line \"" + line.LineName + "\" is Enabled but missing Source, End, or Prefabs, so it will not spawn.", this);
                continue;
            }

            Transform spawnPoint = line.Reverse ? line.End : line.Source;
            Transform removePoint = line.Reverse ? line.Source : line.End;

            Vector3 flow = removePoint.position - spawnPoint.position;
            flow.y = 0f;

            if (flow.sqrMagnitude < 0.0001f)
            {
                Debug.LogWarning("TreeLineSpawner: line \"" + line.LineName + "\" has Source and End at the same XZ position, so no direction of travel can be computed.", this);
                continue;
            }

            Vector3 direction = flow.normalized;

            HandleSpawning(line, spawnPoint, deltaTime);
            HandleMovementAndRemoval(line, removePoint, direction, deltaTime);
        }
    }

    float GetNextSpawnInterval(TreeLine line)
    {
        if (line.MaxSpawnInterval <= line.MinSpawnInterval)
        {
            return line.MinSpawnInterval;
        }

        return Random.Range(line.MinSpawnInterval, line.MaxSpawnInterval);
    }

    void InitializeLine(TreeLine line)
    {
        line.SpawnTimer = GetNextSpawnInterval(line);

        foreach (GameObject prefab in line.Prefabs)
        {
            if (prefab == null || line.Pools.ContainsKey(prefab))
            {
                continue;
            }

            if (!line.PrefabRotations.ContainsKey(prefab))
            {
                line.PrefabRotations.Add(prefab, prefab.transform.rotation);
            }

            Queue<GameObject> pool = new Queue<GameObject>();

            for (int i = 0; i < line.InitialPoolSizePerPrefab; i++)
            {
                GameObject instance = Instantiate(prefab, transform);
                instance.transform.rotation = line.PrefabRotations[prefab];
                instance.SetActive(false);
                pool.Enqueue(instance);
            }

            line.Pools.Add(prefab, pool);
        }
    }

    void HandleSpawning(TreeLine line, Transform spawnPoint, float deltaTime)
    {
        if (line.Prefabs.Count == 0)
        {
            return;
        }

        line.SpawnTimer -= deltaTime;

        if (line.SpawnTimer <= 0f)
        {
            SpawnTree(line, spawnPoint);
            line.SpawnTimer = GetNextSpawnInterval(line);
        }
    }

    void SpawnTree(TreeLine line, Transform spawnPoint)
    {
        GameObject prefab = PickPrefab(line);

        if (prefab == null)
        {
            return;
        }

        if (!line.PrefabRotations.ContainsKey(prefab))
        {
            line.PrefabRotations.Add(prefab, prefab.transform.rotation);
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

        instance.transform.position = spawnPoint.position;
        instance.transform.rotation = line.PrefabRotations[prefab];
        instance.SetActive(true);

        line.ActiveTrees.Add(new ActiveTreeInstance
        {
            Instance = instance,
            SourcePrefab = prefab
        });

        line.RecentPrefabs.Add(prefab);

        while (line.RecentPrefabs.Count > 3)
        {
            line.RecentPrefabs.RemoveAt(0);
        }
    }

    GameObject PickPrefab(TreeLine line)
    {
        List<GameObject> candidates = new List<GameObject>();

        foreach (GameObject prefab in line.Prefabs)
        {
            if (prefab != null && !line.RecentPrefabs.Contains(prefab))
            {
                candidates.Add(prefab);
            }
        }

        if (candidates.Count == 0)
        {
            foreach (GameObject prefab in line.Prefabs)
            {
                if (prefab != null)
                {
                    candidates.Add(prefab);
                }
            }
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        return candidates[Random.Range(0, candidates.Count)];
    }

    void HandleMovementAndRemoval(TreeLine line, Transform removePoint, Vector3 direction, float deltaTime)
    {
        Vector3 removePosition = removePoint.position;

        for (int i = line.ActiveTrees.Count - 1; i >= 0; i--)
        {
            ActiveTreeInstance activeTree = line.ActiveTrees[i];

            if (activeTree.Instance == null)
            {
                line.ActiveTrees.RemoveAt(i);
                continue;
            }

            activeTree.Instance.transform.position += direction * line.Speed * deltaTime;

            Vector3 toInstance = activeTree.Instance.transform.position - removePosition;
            toInstance.y = 0f;

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

    void SafeDestroy(GameObject instance)
    {
        if (instance == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(instance);
        }
        else
        {
            DestroyImmediate(instance);
        }
    }

    public void StartLine(TreeLine line)
    {
        line.Enabled = true;
        line.SpawnTimer = GetNextSpawnInterval(line);
    }

    public void StopLine(TreeLine line)
    {
        line.Enabled = false;
    }

    public void ClearLine(TreeLine line)
    {
        for (int i = line.ActiveTrees.Count - 1; i >= 0; i--)
        {
            ReturnToPool(line, line.ActiveTrees[i]);
            line.ActiveTrees.RemoveAt(i);
        }
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
            SafeDestroy(activeTree.Instance);
        }

        foreach (Queue<GameObject> pool in line.Pools.Values)
        {
            while (pool.Count > 0)
            {
                SafeDestroy(pool.Dequeue());
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

        if (!line.PrefabRotations.ContainsKey(prefab))
        {
            line.PrefabRotations.Add(prefab, prefab.transform.rotation);
        }

        if (!line.Pools.ContainsKey(prefab))
        {
            Queue<GameObject> pool = new Queue<GameObject>();

            for (int i = 0; i < line.InitialPoolSizePerPrefab; i++)
            {
                GameObject instance = Instantiate(prefab, transform);
                instance.transform.rotation = line.PrefabRotations[prefab];
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
        line.RecentPrefabs.RemoveAll(p => p == prefab);

        for (int i = line.ActiveTrees.Count - 1; i >= 0; i--)
        {
            if (line.ActiveTrees[i].SourcePrefab == prefab)
            {
                SafeDestroy(line.ActiveTrees[i].Instance);
                line.ActiveTrees.RemoveAt(i);
            }
        }

        if (line.Pools.ContainsKey(prefab))
        {
            Queue<GameObject> pool = line.Pools[prefab];

            while (pool.Count > 0)
            {
                SafeDestroy(pool.Dequeue());
            }

            line.Pools.Remove(prefab);
        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(TreeLineSpawner))]
public class TreeLineSpawnerEditor : Editor
{
    SerializedProperty linesProperty;
    List<bool> foldouts = new List<bool>();

    void OnEnable()
    {
        linesProperty = serializedObject.FindProperty("Lines");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        TreeLineSpawner spawner = (TreeLineSpawner)target;

        while (foldouts.Count < linesProperty.arraySize)
        {
            foldouts.Add(true);
        }

        while (foldouts.Count > linesProperty.arraySize)
        {
            foldouts.RemoveAt(foldouts.Count - 1);
        }

        bool lineWasRemoved = false;

        for (int i = 0; i < linesProperty.arraySize; i++)
        {
            SerializedProperty lineProperty = linesProperty.GetArrayElementAtIndex(i);

            if (DrawLine(spawner, lineProperty, i))
            {
                lineWasRemoved = true;
                break;
            }
        }

        if (lineWasRemoved)
        {
            serializedObject.Update();
            EditorUtility.SetDirty(spawner);
        }

        EditorGUILayout.Space();

        if (Application.isPlaying)
        {
            if (GUILayout.Button("Add Line"))
            {
                spawner.AddLine(new TreeLine());
                serializedObject.Update();
                EditorUtility.SetDirty(spawner);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    bool DrawLine(TreeLineSpawner spawner, SerializedProperty lineProperty, int index)
    {
        SerializedProperty lineNameProperty = lineProperty.FindPropertyRelative("LineName");
        SerializedProperty enabledProperty = lineProperty.FindPropertyRelative("Enabled");
        SerializedProperty sourceProperty = lineProperty.FindPropertyRelative("Source");
        SerializedProperty endProperty = lineProperty.FindPropertyRelative("End");
        SerializedProperty prefabsProperty = lineProperty.FindPropertyRelative("Prefabs");
        SerializedProperty reverseProperty = lineProperty.FindPropertyRelative("Reverse");
        SerializedProperty speedProperty = lineProperty.FindPropertyRelative("Speed");
        SerializedProperty minSpawnIntervalProperty = lineProperty.FindPropertyRelative("MinSpawnInterval");
        SerializedProperty maxSpawnIntervalProperty = lineProperty.FindPropertyRelative("MaxSpawnInterval");
        SerializedProperty poolSizeProperty = lineProperty.FindPropertyRelative("InitialPoolSizePerPrefab");

        TreeLine line = spawner.Lines[index];

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();

        foldouts[index] = EditorGUILayout.Foldout(foldouts[index], string.IsNullOrEmpty(lineNameProperty.stringValue) ? "Line " + index : lineNameProperty.stringValue, true);

        GUILayout.FlexibleSpace();

        GUIStyle statusStyle = new GUIStyle(EditorStyles.miniBoldLabel);
        statusStyle.normal.textColor = enabledProperty.boolValue ? new Color(0.2f, 0.7f, 0.2f) : new Color(0.7f, 0.2f, 0.2f);
        GUILayout.Label(enabledProperty.boolValue ? "Enabled" : "Disabled", statusStyle, GUILayout.Width(60));

        if (Application.isPlaying)
        {
            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                spawner.RemoveLine(line);
                return true;
            }
        }

        EditorGUILayout.EndHorizontal();

        if (foldouts[index])
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(lineNameProperty);
            EditorGUILayout.PropertyField(sourceProperty);
            EditorGUILayout.PropertyField(endProperty);
            EditorGUILayout.PropertyField(reverseProperty);
            EditorGUILayout.PropertyField(speedProperty);
            EditorGUILayout.PropertyField(minSpawnIntervalProperty);
            EditorGUILayout.PropertyField(maxSpawnIntervalProperty);
            EditorGUILayout.PropertyField(poolSizeProperty);
            EditorGUILayout.PropertyField(prefabsProperty, true);

            if (Application.isPlaying)
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();

                GUI.enabled = !enabledProperty.boolValue;
                if (GUILayout.Button("Start"))
                {
                    enabledProperty.boolValue = true;
                    serializedObject.ApplyModifiedProperties();
                    spawner.StartLine(line);
                    EditorUtility.SetDirty(spawner);
                }

                GUI.enabled = enabledProperty.boolValue;
                if (GUILayout.Button("Stop"))
                {
                    enabledProperty.boolValue = false;
                    serializedObject.ApplyModifiedProperties();
                    spawner.StopLine(line);
                    EditorUtility.SetDirty(spawner);
                }

                GUI.enabled = true;
                if (GUILayout.Button("Clear Active"))
                {
                    spawner.ClearLine(line);
                    EditorUtility.SetDirty(spawner);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();

        return false;
    }
}
#endif