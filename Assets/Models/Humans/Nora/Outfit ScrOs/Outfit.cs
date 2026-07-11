using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif


[CreateAssetMenu(
    fileName = "Outfit",
    menuName = "{!!} Tawley Scriptable Object/Outfit",
    order = 10)]
public class Outfit : ScriptableObject
{
    public OutfitName thisOutfit;
    public OutfitType outfitType;
    public string outfitTitle;
    public bool SpawnAs = true;
    public List<GameObject> OutfitPrefabs;
    public Color lipsColor = new Color(0.95f, 0.6f, 0.7f);
    public Color nailsColor = new Color(0.9f, 0.7f, 0.8f);
    public bool Jiggle;
    public bool Wings;
    public bool Apron;
    public bool Hat;
    public bool Choker;
    public HairName Hair = HairName.DefaultHair;
}

#if UNITY_EDITOR

[CustomEditor(typeof(Outfit))]
public class OutfitEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Outfit outfit = (Outfit)target;

        var applyButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        GUI.backgroundColor = Color.green;

        if (GUILayout.Button("Apply Outfit", applyButtonStyle, GUILayout.Height(32)))
        {
            ApplyOutfit(outfit);
        }

        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("---------------------", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        DrawDefaultInspector();
    }

    private static void ApplyOutfit(Outfit outfit)
    {
        NorasWardrobe wardrobe = Application.isPlaying && Player.Instance != null
            ? GameMaster.Instance.NorasWardrobe
            : FindFirstObjectByType<NorasWardrobe>();

        if (wardrobe == null)
        {
            Debug.LogWarning("OutfitEditor: No NorasWardrobe found in the scene. Cannot apply outfit.");
            return;
        }

        wardrobe.SetMainOutfit(outfit.thisOutfit);

        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(wardrobe);
            EditorSceneManager.MarkSceneDirty(wardrobe.gameObject.scene);
        }
    }
}
#endif