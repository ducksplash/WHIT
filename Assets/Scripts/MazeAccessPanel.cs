using System.Collections.Generic;
using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MazeAccessPanel : MonoBehaviour
{
    [Header("Display Text")]
    public string lockedText = "LOCKED";
    public string unlockedText = "UNLOCKED";

    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI topRow;
    [SerializeField] private TextMeshProUGUI bottomRow;
    [SerializeField] private TextMeshProUGUI lockStatus;

    [Header("Light Renderers (HDRP)")]
    public List<Renderer> renderers = new List<Renderer>();

    [Header("Colors")]
    public Color lockedColor = Color.red;
    public Color unlockedColor = Color.white;

    [Header("Emission Intensity")]
    public float lockedEmissionIntensity = 5f;
    public float unlockedEmissionIntensity = 5f;

    public string MacAddress;

    private static readonly int EmissiveColorProperty = Shader.PropertyToID("_EmissiveColor");

    private MetalGate linkedGate;
    private bool isLocked = true;
    private bool canHack;

    private Material[] cachedMaterials;

    private void Start()
    {
        linkedGate = GateBackbone.Instance.RegisterAccessPanel();

        if (linkedGate != null)
        {
            MacAddress = linkedGate.MacAddress;
        }

        CacheMaterials();
        UpdateVisuals();

        EventManager.OnKelliUnlockGate += OnGateToggled;
    }

    private void OnDestroy()
    {
        EventManager.OnKelliUnlockGate -= OnGateToggled;
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

    private void OnGateToggled(string suppliedMac)
    {
        if (string.IsNullOrEmpty(MacAddress)) return;
        if (!suppliedMac.Equals(MacAddress)) return;

        isLocked = !isLocked;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        Color displayColor = isLocked
            ? (linkedGate != null ? linkedGate.thisGateColor : lockedColor)
            : unlockedColor;

        float intensity = isLocked ? lockedEmissionIntensity : unlockedEmissionIntensity;
        string statusText = isLocked ? lockedText : unlockedText;

        if (lockStatus != null)
        {
            lockStatus.text = statusText;
            lockStatus.color = displayColor;
        }
        if (topRow != null) topRow.color = displayColor;
        if (bottomRow != null) bottomRow.color = displayColor;

        if (cachedMaterials != null)
        {
            foreach (var mat in cachedMaterials)
            {
                if (mat == null) continue;
                mat.color = displayColor;
                if (mat.HasProperty(EmissiveColorProperty))
                {
                    Color emissive = displayColor * intensity;
                    mat.SetColor(EmissiveColorProperty, emissive);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        EventManager.KelliFoundDeviceMaze(this);
        canHack = isLocked;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        EventManager.KelliLostDevice();
        canHack = false;
    }

    
#if UNITY_EDITOR
    public void EditorToggleLock()
    {
        EventManager.KelliUnlockGate(MacAddress);
    }
    
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(MazeAccessPanel))]
public class MazeAccessPanelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        MazeAccessPanel panel = (MazeAccessPanel)target;

        if (GUILayout.Button("Toggle Lock State", GUILayout.Height(30)))
        {
            panel.EditorToggleLock();
            EditorUtility.SetDirty(panel);
        }
    }
}
#endif