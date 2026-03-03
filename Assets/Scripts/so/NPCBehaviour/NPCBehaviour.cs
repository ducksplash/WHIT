using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(
    fileName = "NPCBehaviour",
    menuName = "{!!} Tawley Scriptable Object/NPCBehaviour",
    order = 10)]
public class NPCBehaviour : ScriptableObject
{
    [Header("NPC Behaviour")]
    public Behaviour BehaviourType = Behaviour.idle;

    // if Behaviour is Dialogue, use this dialogue
    public DialogueName selectedDialogue; // an enum

    // if Behaviour is Act, use this Animator, note that this needs a reference to be added for specified NPC prior to playback
    public Animator npcAnimator;

    // animation state to blend into
    public string animationState = "idle";

    // If Behaviour is Go, use these waypoints
    public List<Vector3> waypointVectors = new List<Vector3>();

    // Optional
    public bool isTimed;

    // If timed checked, run the 'Behaviour' for this amount of time,
    public float timer;
}

#if UNITY_EDITOR
[CustomEditor(typeof(NPCBehaviour))]
public class NPCBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Waypoint Quick Add", EditorStyles.boldLabel);

        using (new EditorGUILayout.VerticalScope(GUI.skin.box))
        {
            EditorGUILayout.LabelField(
                "Drag one or more Transforms (or GameObjects) into the drop area.\n" +
                "Their world positions will be appended to waypointVectors immediately.",
                EditorStyles.wordWrappedMiniLabel
            );

            DrawDropAreaAndHandleAdd((NPCBehaviour)target);

            EditorGUILayout.Space(6);

            if (GUILayout.Button("Clear All Waypoints"))
            {
                var behaviour = (NPCBehaviour)target;
                Undo.RecordObject(behaviour, "Clear Waypoints");
                behaviour.waypointVectors?.Clear();
                EditorUtility.SetDirty(behaviour);
            }
        }
    }

    void DrawDropAreaAndHandleAdd(NPCBehaviour behaviour)
    {
        // Big drop zone
        Rect dropArea = GUILayoutUtility.GetRect(0f, 55f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "DROP TRANSFORMS HERE\n(you can drag multiple)", EditorStyles.centeredGreyMiniLabel);

        Event evt = Event.current;
        if (evt == null) return;

        // Only react when mouse is over the drop area
        if (!dropArea.Contains(evt.mousePosition))
            return;

        // Indicate we accept the drag
        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                // Collect transforms from dragged objects
                List<Transform> transforms = new List<Transform>();

                foreach (Object obj in DragAndDrop.objectReferences)
                {
                    if (obj == null) continue;

                    // If user drags a Transform
                    if (obj is Transform t)
                    {
                        transforms.Add(t);
                        continue;
                    }

                    // If user drags a GameObject
                    if (obj is GameObject go)
                    {
                        transforms.Add(go.transform);
                        continue;
                    }

                    // If user drags a Component
                    if (obj is Component c)
                    {
                        transforms.Add(c.transform);
                        continue;
                    }
                }

                if (transforms.Count > 0)
                {
                    Undo.RecordObject(behaviour, "Add Waypoints From Drag");

                    behaviour.waypointVectors ??= new List<Vector3>();

                    // Add in the same order as dragged
                    for (int i = 0; i < transforms.Count; i++)
                    {
                        if (transforms[i] == null) continue;
                        behaviour.waypointVectors.Add(transforms[i].position);
                    }

                    EditorUtility.SetDirty(behaviour);
                }

                // Clear the drag payload (keeps UX tidy; same "clear slot" idea)
                DragAndDrop.objectReferences = System.Array.Empty<Object>();

                evt.Use();
                Repaint();
            }
            else
            {
                evt.Use();
            }
        }
    }
}
#endif