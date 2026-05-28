using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Cutscene : MonoBehaviour
{
    // Variables to control the cutscene timing
    [Header("Time to rotate to face object")]
    public float panTime = 5.0f; // How long it takes to pan to the object

    [Header("Time to linger looking at object")]
    public float duration = 10.0f; // How long the cutscene lasts in total

    [Header("duration is deprecated, player now presses Submit (A, Enter)")] 
    [Header("Optional Dialogue")] 
    [SerializeField] public Contacts ContactName;

    public DialogueName selectedMessage;

    [Header("Object to zoom to")] 
    public GameObject targetObject;

    [Header("Collider that triggers the cutscene")]
    public GameObject ColliderCube;

    private bool Saved;

    void Start()
    {
        ColliderCube.SetActive(false);
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != 3) return;
        if (GameMaster.Instance.PLAYERBUSY) return;

        var dm = GameMaster.Instance?.DialogueManager;
        if (dm == null || !dm.SeenLoaded) return;

        if (dm.DialogueSeen.Contains(selectedMessage)) return;

        dm.cameraZoom.enabled = false;

        dm.PlayDialogue(selectedMessage, duration, DialogueType.cutscene, 7, panTime, targetObject);
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(Cutscene))]
public class ContactDrawerCutscene : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw the enum dropdown field
        property.enumValueIndex = EditorGUI.Popup(position, label.text, property.enumValueIndex, property.enumDisplayNames);

        EditorGUI.EndProperty();
    }
}

#endif