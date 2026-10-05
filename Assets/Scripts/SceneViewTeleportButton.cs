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

    public Color backgroundColor = new Color(0f, 0f, 0f, 0f);
    public Color hoverBackgroundColor = new Color(1f, 1f, 1f, 0.15f);
    public Color pressedBackgroundColor = new Color(1f, 1f, 1f, 0.3f);
    public Color fontColor = Color.white;

    [Header("Spawn Point Teleport")]
    public GAMELEVEL selectedTeleportLevel = GAMELEVEL.NorasFlat;
    public int spawnRowWidthPixels = 170;
    public int spawnRowHeightPixels = 18;
    public int spawnRowSpacingPixels = 0;

    private static readonly List<SceneViewTeleportButton> ActiveInstances = new List<SceneViewTeleportButton>();

    private GUIStyle cachedStyle;
    private Texture2D normalTex;
    private Texture2D hoverTex;
    private Texture2D activeTex;
    private Color cachedNormalColor;
    private Color cachedHoverColor;
    private Color cachedActiveColor;
    private Color cachedFontColor;

    private static readonly string[] LevelNames = System.Enum.GetNames(typeof(GAMELEVEL));

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
        DestroyCachedTextures();
    }

    void OnDestroy()
    {
        ActiveInstances.Remove(this);
        SceneView.duringSceneGui -= OnSceneGUI;
        DestroyCachedTextures();
    }

    private void DestroyCachedTextures()
    {
        if (normalTex != null) DestroyImmediate(normalTex);
        if (hoverTex != null) DestroyImmediate(hoverTex);
        if (activeTex != null) DestroyImmediate(activeTex);
        normalTex = hoverTex = activeTex = null;
        cachedStyle = null;
    }

    private static Texture2D MakeSolidTexture(Color color)
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        tex.hideFlags = HideFlags.HideAndDontSave;
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }

    private GUIStyle GetButtonStyle()
    {
        bool needsRebuild = cachedStyle == null
                            || normalTex == null || hoverTex == null || activeTex == null
                            || cachedNormalColor != backgroundColor
                            || cachedHoverColor != hoverBackgroundColor
                            || cachedActiveColor != pressedBackgroundColor
                            || cachedFontColor != fontColor;

        if (needsRebuild)
        {
            DestroyCachedTextures();

            normalTex = MakeSolidTexture(backgroundColor);
            hoverTex = MakeSolidTexture(hoverBackgroundColor);
            activeTex = MakeSolidTexture(pressedBackgroundColor);

            cachedNormalColor = backgroundColor;
            cachedHoverColor = hoverBackgroundColor;
            cachedActiveColor = pressedBackgroundColor;
            cachedFontColor = fontColor;

            cachedStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                border = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(2, 2, 2, 2),
                fontSize = 10,
                clipping = TextClipping.Clip
            };

            ApplyState(cachedStyle.normal, normalTex);
            ApplyState(cachedStyle.hover, hoverTex);
            ApplyState(cachedStyle.active, activeTex);
            ApplyState(cachedStyle.focused, normalTex);
            ApplyState(cachedStyle.onNormal, normalTex);
            ApplyState(cachedStyle.onHover, hoverTex);
            ApplyState(cachedStyle.onActive, activeTex);
            ApplyState(cachedStyle.onFocused, normalTex);
        }

        return cachedStyle;
    }

    private void ApplyState(GUIStyleState state, Texture2D tex)
    {
        state.background = tex;
        state.scaledBackgrounds = null; // prevents HiDPI skin sprites from overriding our flat texture
        state.textColor = fontColor;
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

        GUIStyle style = GetButtonStyle();

        bool clicked = GUI.Button(rect, GUIContent.none, style);

        if (buttonIcon != null)
        {
            Rect iconRect = new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f);
            GUI.DrawTexture(iconRect, buttonIcon, ScaleMode.ScaleToFit);
        }

        if (!string.IsNullOrEmpty(buttonLabel))
        {
            GUI.Label(rect, new GUIContent("", buttonLabel));
        }

        if (clicked)
        {
            TeleportToSceneCamera(sceneView);
        }

        // ── Spawn point row, drawn immediately below (or above, if bottom-anchored) this instance's button ──
        DrawSpawnPointRow(rect, size, stackDirection, style);

        Handles.EndGUI();
    }

    private void DrawSpawnPointRow(Rect buttonRect, int size, int stackDirection, GUIStyle style)
    {
        float rowHeight = Mathf.Max(14f, spawnRowHeightPixels);
        float rowSpacing = Mathf.Max(0f, spawnRowSpacingPixels);
        float totalRowHeight = (rowHeight * 2f) + rowSpacing;

        float rowY = stackDirection > 0
            ? buttonRect.y + size + buttonSpacingPixels
            : buttonRect.y - buttonSpacingPixels - totalRowHeight;

        float fullWidth = Mathf.Max(60, spawnRowWidthPixels);

        Rect dropdownRect = new Rect(buttonRect.x, rowY, fullWidth, rowHeight);
        Rect goRect = new Rect(buttonRect.x, rowY + rowHeight + rowSpacing, fullWidth, rowHeight);

        int currentIndex = (int)selectedTeleportLevel;
        int newIndex = EditorGUI.Popup(dropdownRect, currentIndex, LevelNames, style);
        if (newIndex != currentIndex)
        {
            selectedTeleportLevel = (GAMELEVEL)newIndex;
        }

        bool canTeleport = Application.isPlaying && GameMaster.Instance != null
                                                 && TryGetSpawnPoint(selectedTeleportLevel, out _, out _);

        using (new EditorGUI.DisabledScope(!canTeleport))
        {
            if (GUI.Button(goRect, "Go", style))
            {
                TeleportToSpawnPoint(selectedTeleportLevel);
            }
        }
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

    private bool TryGetSpawnPoint(GAMELEVEL level, out Vector3 position, out Vector3 rotation)
    {
        position = Vector3.zero;
        rotation = Vector3.zero;

        GameMaster gm = GameMaster.Instance;
        if (gm == null) return false;

        switch (level)
        {
            case GAMELEVEL.ETV:
                position = gm.SPAWNPOINTETV;
                rotation = gm.SPAWNROTETV;
                return true;
            case GAMELEVEL.NorasFlat:
                position = gm.SPAWNPOINTNORASFLAT;
                rotation = gm.SPAWNROTNORASFLAT;
                return true;
            case GAMELEVEL.NorasOldFlat:
                position = gm.SPAWNPOINTNORASOLDFLAT;
                rotation = gm.SPAWNROTNORASOLDFLAT;
                return true;
            case GAMELEVEL.TawleyMeats:
                position = gm.SPAWNPOINTTAWLEYMEATS;
                rotation = gm.SPAWNROTTAWLEYMEATS;
                return true;
            case GAMELEVEL.TawleyMeatsMaze:
                position = gm.SPAWNPOINTTAWLEYMEATSMAZE;
                rotation = gm.SPAWNROTTAWLEYMEATSMAZE;
                return true;
            case GAMELEVEL.RoarkOutside:
                position = gm.SPAWNPOINTROARKOUTSIDE;
                rotation = gm.SPAWNROTROARKOUTSIDE;
                return true;
            case GAMELEVEL.RoarkInside:
                position = gm.SPAWNPOINTROARKINSIDE;
                rotation = gm.SPAWNROTROARKINSIDE;
                return true;
            case GAMELEVEL.FarsetCentralStation:
                position = gm.SPAWNPOINTTRAINSTATION;
                rotation = gm.SPAWNROTTRAINSTATION;
                return true;
            case GAMELEVEL.EnteringTawley:
                position = gm.SPAWNPOINTENTERINGTAWLEY;
                rotation = gm.SPAWNROTENTERINGTAWLEY;
                return true;
            case GAMELEVEL.MainMenu:
            default:
                return false;
        }
    }

    private void TeleportToSpawnPoint(GAMELEVEL level)
    {
        if (!TryGetSpawnPoint(level, out Vector3 targetPosition, out Vector3 targetRotation))
        {
            Debug.LogWarning($"SceneViewTeleportButton: No spawn point available for level '{level}'.");
            return;
        }

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
        targetTransform.eulerAngles = targetRotation;

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