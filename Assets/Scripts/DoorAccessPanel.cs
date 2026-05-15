using System.Collections.Generic;
using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DoorAccessPanel : MonoBehaviour
{
    [Header("Door Reference")]
    [Tooltip("Reference to the Door this panel controls")]
    public Door door;

    [Header("Initial State")]
    [Tooltip("What state this panel should start in")]
    public DoorLockState OnLoadState = DoorLockState.Locked;

    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI topRow;
    [SerializeField] private TextMeshProUGUI bottomRow;
    [SerializeField] private TextMeshProUGUI lockStatus;

    [Header("Light Renderers (HDRP)")]
    [Tooltip("All renderers whose material should change color (panel lights, LEDs, etc.)")]
    public List<Renderer> renderers = new List<Renderer>();

    [Header("Colors")]
    public Color lockedColor = Color.red;
    public Color unlockedColor = Color.green;
    public Color inactiveColor = Color.grey;

    [Header("Emission Intensity")]
    [Tooltip("Emission strength for HDRP materials")]
    public float emissionIntensity = 8f;

    private static readonly int EmissiveColorProperty = Shader.PropertyToID("_EmissiveColor");

    private void Start()
    {
        SetState(OnLoadState);
    }

    public void SetState(DoorLockState newState)
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

    public void UnlockDoor() => SetState(DoorLockState.Unlocked);
    public void LockDoor()   => SetState(DoorLockState.Locked);

    private void UpdateVisuals(DoorLockState state)
    {
        Color targetColor = state switch
        {
            DoorLockState.Unlocked => unlockedColor,
            DoorLockState.Locked   => lockedColor,
            DoorLockState.Inactive => inactiveColor,
            _                      => lockedColor
        };

        string statusText = state switch
        {
            DoorLockState.Unlocked => "UNLOCKED",
            DoorLockState.Locked   => "LOCKED",
            DoorLockState.Inactive => "DISABLED",
            _                      => "LOCKED"
        };

        // Update Text
        if (lockStatus != null)
        {
            lockStatus.text = statusText;
            lockStatus.color = targetColor;
        }

        if (topRow != null) topRow.color = targetColor;
        if (bottomRow != null) bottomRow.color = targetColor;

        // Update Renderers + Emission (HDRP)
        foreach (Renderer rend in renderers)
        {
            if (rend == null) continue;

            Material mat = rend.material;

            mat.color = targetColor;

            if (mat.HasProperty(EmissiveColorProperty))
            {
                Color emissive = targetColor * emissionIntensity;
                mat.SetColor(EmissiveColorProperty, emissive);
            }
        }
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

        EditorGUILayout.Space();

        if (panel.door != null)
        {
            EditorGUILayout.LabelField("Current Door State:", 
                panel.door.isLocked ? "LOCKED" : "UNLOCKED", 
                panel.door.isLocked ? EditorStyles.helpBox : EditorStyles.whiteLabel);
        }
    }
}

#endif