using System.Collections.Generic;
using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DoorAccessPanel : MonoBehaviour
{
    [Header("Door Reference")]
    public Door door;

    [Header("Initial State")]
    public DoorLockState OnLoadState = DoorLockState.Locked;

    [Header("Broken Mode")]
    [Tooltip("Enable to put this panel in broken/flickering state")]
    public bool isBroken = false;

    [Header("Display Text")]
    public string lockedText = "LOCKED";
    public string unlockedText = "UNLOCKED";
    public string inactiveText = "DISABLED";
    public string brokenText = "MALFUNCTION";

    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI topRow;
    [SerializeField] private TextMeshProUGUI bottomRow;
    [SerializeField] private TextMeshProUGUI lockStatus;

    [Header("Lock Light")]
    public Light lockLight;

    [Header("Light Renderers (HDRP)")]
    public List<Renderer> renderers = new List<Renderer>();

    [Header("Colors")]
    public Color lockedColor = Color.red;
    public Color unlockedColor = Color.green;
    public Color inactiveColor = Color.grey;

    [Header("Emission Intensity")]
    public float lockedEmissionIntensity = 8f;
    public float unlockedEmissionIntensity = 8f;
    public float inactiveEmissionIntensity = 8f;

    [Header("Broken Flicker Settings")]
    public float brokenMinMultiplier = 0.3f;
    public float brokenMaxMultiplier = 1.8f;
    public int smoothing = 6;

    public string MacAddress;

    private static readonly int EmissiveColorProperty = Shader.PropertyToID("_EmissiveColor");

    public DoorLockState currentLockState;
    private bool canHack;

    // Cached materials
    private Material[] cachedMaterials;

    // Light base intensity
    private float originalLightIntensity = 1f;

    // Flicker smoothing
    private Queue<float> smoothQueue = new Queue<float>();
    private float lastSum = 0f;

    private void Start()
    {
        CacheMaterials();
        CacheOriginalLightIntensity();
        SetState(OnLoadState);
    }

    private void CacheMaterials()
    {
        if (renderers.Count == 0) return;

        cachedMaterials = new Material[renderers.Count];
        for (int i = 0; i < renderers.Count; i++)
        {
            if (renderers[i] != null)
                cachedMaterials[i] = renderers[i].material;
        }
    }

    private void CacheOriginalLightIntensity()
    {
        if (lockLight != null) originalLightIntensity = lockLight.intensity;
    }

    private void Update()
    {
        if (isBroken) HandleBrokenFlicker();
    }

    public void SetState(DoorLockState newState)
    {
        currentLockState = newState;

        if (isBroken)
            UpdateVisuals(DoorLockState.Inactive);
        else
        {
            switch (newState)
            {
                case DoorLockState.Locked:
                    if (door != null) door.isLocked = true;
                    UpdateVisuals(DoorLockState.Locked);
                    break;

                case DoorLockState.Unlocked:
                    if (door != null) door.isLocked = false;
                    UpdateVisuals(DoorLockState.Unlocked);
                    break;

                case DoorLockState.Inactive:
                    UpdateVisuals(DoorLockState.Inactive);
                    break;
            }
        }
    }

    public void UnlockDoor() => SetState(DoorLockState.Unlocked);
    public void LockDoor() => SetState(DoorLockState.Locked);

    private void HandleBrokenFlicker()
    {
        // Smooth random flicker
        while (smoothQueue.Count >= smoothing)
            lastSum -= smoothQueue.Dequeue();

        float randomValue = Random.Range(brokenMinMultiplier, brokenMaxMultiplier);
        smoothQueue.Enqueue(randomValue);
        lastSum += randomValue;

        float intensity = lastSum / smoothQueue.Count;

        Color flickerColor = inactiveColor * intensity;

        // Update Renderers
        foreach (var mat in cachedMaterials)
        {
            if (mat == null) continue;
            mat.color = inactiveColor;
            if (mat.HasProperty(EmissiveColorProperty))
                mat.SetColor(EmissiveColorProperty, flickerColor);
        }

        // Update Light - based on original intensity
        if (lockLight != null)
        {
            lockLight.color = inactiveColor;
            lockLight.intensity = originalLightIntensity * intensity;
        }

        // Flicker Text Color
        Color textColor = inactiveColor * Random.Range(0.65f, 1.35f);
        if (lockStatus != null) lockStatus.color = textColor;
        if (topRow != null) topRow.color = textColor;
        if (bottomRow != null) bottomRow.color = textColor;
    }

    private void UpdateVisuals(DoorLockState state)
    {
        Color targetColor = state switch
        {
            DoorLockState.Unlocked => unlockedColor,
            DoorLockState.Locked => lockedColor,
            DoorLockState.Inactive => inactiveColor,
            _ => lockedColor
        };

        string statusText = state switch
        {
            DoorLockState.Unlocked => unlockedText,
            DoorLockState.Locked => lockedText,
            DoorLockState.Inactive => isBroken ? brokenText : inactiveText,
            _ => lockedText
        };

        float emissiveIntensity = state switch
        {
            DoorLockState.Unlocked => unlockedEmissionIntensity,
            DoorLockState.Locked => lockedEmissionIntensity,
            DoorLockState.Inactive => inactiveEmissionIntensity,
            _ => lockedEmissionIntensity
        };

        // Update Text
        if (lockStatus != null)
        {
            lockStatus.text = statusText;
            lockStatus.color = targetColor;
        }
        if (topRow != null) topRow.color = targetColor;
        if (bottomRow != null) bottomRow.color = targetColor;

        // Update Renderers
        foreach (var mat in cachedMaterials)
        {
            if (mat == null) continue;
            mat.color = targetColor;
            if (mat.HasProperty(EmissiveColorProperty))
            {
                Color emissive = targetColor * emissiveIntensity;
                mat.SetColor(EmissiveColorProperty, emissive);
            }
        }

        // Update Light
        if (lockLight != null)
        {
            lockLight.color = targetColor;
            lockLight.intensity = isBroken ? 0f : originalLightIntensity * 0.8f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        EventManager.KelliFoundDevice(this);
        canHack = currentLockState != DoorLockState.Inactive && !isBroken;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        EventManager.KelliLostDevice();
        canHack = false;
    }
}

public enum DoorLockState
{
    Locked,
    Unlocked,
    Inactive
}

#if UNITY_EDITOR
[CustomEditor(typeof(DoorAccessPanel))]
public class DoorAccessPanelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        DoorAccessPanel panel = (DoorAccessPanel)target;

        if (GUILayout.Button("Unlock Door", GUILayout.Height(30)))
        {
            panel.UnlockDoor();
            EditorUtility.SetDirty(panel);
            if (panel.door != null) EditorUtility.SetDirty(panel.door);
        }

        if (GUILayout.Button("Lock Door", GUILayout.Height(30)))
        {
            panel.LockDoor();
            EditorUtility.SetDirty(panel);
            if (panel.door != null) EditorUtility.SetDirty(panel.door);
        }

        if (GUILayout.Button("Set Inactive", GUILayout.Height(30)))
        {
            panel.SetState(DoorLockState.Inactive);
            EditorUtility.SetDirty(panel);
        }

        if (GUILayout.Button("Toggle Broken Mode", GUILayout.Height(30)))
        {
            panel.isBroken = !panel.isBroken;
            panel.SetState(panel.currentLockState);
            EditorUtility.SetDirty(panel);
        }
    }
}
#endif