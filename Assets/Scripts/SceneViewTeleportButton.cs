#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[ExecuteAlways]
public class SceneViewTeleportButton : MonoBehaviour
{
    public string buttonLabel = "Teleport Here";
    public Texture2D buttonIcon;
    public bool onlyActiveInPlayMode = true;

    public int buttonSizePixels = 32;
    public int buttonSpacingPixels = 6;
    public int buttonMarginPixels = 10;

    [Range(0f, 1f)] public float anchorX = 0f;
    [Range(0f, 1f)] public float anchorY = 0f;

    private static readonly List<SceneViewTeleportButton> ActiveInstances = new List<SceneViewTeleportButton>();

    void OnEnable()
    {
        if (!ActiveInstances.Contains(this)) ActiveInstances.Add(this);
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDisable()
    {
        ActiveInstances.Remove(this);
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnDestroy()
    {
        ActiveInstances.Remove(this);
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (this == null) return;
        if (onlyActiveInPlayMode && !Application.isPlaying) return;

        int index = GetStackIndex();

        Handles.BeginGUI();

        int size = Mathf.Max(4, buttonSizePixels);

        float viewWidth = sceneView.position.width;
        float viewHeight = sceneView.position.height;

        float usableWidth = Mathf.Max(0f, viewWidth - size - (buttonMarginPixels * 2f));
        float usableHeight = Mathf.Max(0f, viewHeight - size - (buttonMarginPixels * 2f));

        float baseX = buttonMarginPixels + (usableWidth * anchorX);
        float baseY = buttonMarginPixels + (usableHeight * anchorY);

        int stackDirection = anchorY > 0.5f ? -1 : 1;
        float stackedY = baseY + (stackDirection * index * (size + buttonSpacingPixels));

        Rect rect = new Rect(baseX, stackedY, size, size);

        GUI.Label(rect, GUIContent.none, GUI.skin.button);

        if (buttonIcon != null)
        {
            Rect iconRect = new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f);
            GUI.DrawTexture(iconRect, buttonIcon, ScaleMode.ScaleToFit);
        }

        if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
        {
            TeleportToSceneCamera(sceneView);
        }

        if (GUI.tooltip != buttonLabel)
        {
            GUI.Label(rect, new GUIContent("", buttonLabel));
        }

        Handles.EndGUI();
    }

    private int GetStackIndex()
    {
        int count = 0;
        for (int i = 0; i < ActiveInstances.Count; i++)
        {
            if (ActiveInstances[i] == this) return count;

            if (Mathf.Approximately(ActiveInstances[i].anchorX, anchorX) &&
                Mathf.Approximately(ActiveInstances[i].anchorY, anchorY))
            {
                count++;
            }
        }

        return count;
    }

    private void TeleportToSceneCamera(SceneView sceneView)
    {
        if (sceneView == null || sceneView.camera == null) return;

        Vector3 targetPosition = sceneView.camera.transform.position;

        Transform targetTransform = transform;

        CharacterController controller = targetTransform.GetComponent<CharacterController>();
        if (controller == null) controller = targetTransform.GetComponentInParent<CharacterController>();

        bool controllerWasEnabled = false;
        if (controller != null)
        {
            controllerWasEnabled = controller.enabled;
            controller.enabled = false;
            targetTransform = controller.transform;
        }

        targetTransform.position = targetPosition;

        Physics.SyncTransforms();

        if (controller != null)
        {
            controller.enabled = controllerWasEnabled;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        SceneView.RepaintAll();
    }
}
#endif