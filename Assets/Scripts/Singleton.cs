using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static bool isShuttingDown;

    public static T Instance
    {
        get
        {
            // Unity fake-null safe check + shutdown guard
            if (isShuttingDown) return null;
            if (!instance)
            {
                instance = FindFirstObjectByType<T>(); // Unity 2022+ (otherwise FindObjectOfType)

                if (!instance)
                {
                    // If you REALLY want auto-create, do it only when safe.
                    var go = new GameObject($"{typeof(T).Name} (Singleton)");
                    instance = go.AddComponent<T>();
                    DontDestroyOnLoad(go);
                }
            }

            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (!instance)
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        isShuttingDown = true;
    }

    protected virtual void OnDestroy()
    {
        // Important: if the singleton is being destroyed, clear the static ref
        if (instance == this) instance = null;
    }
}