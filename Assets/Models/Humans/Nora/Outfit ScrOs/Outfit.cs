using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif


[CreateAssetMenu(fileName = "Outfit", menuName = "{!!} Tawley Scriptable Object/Outfit", order = 10)]
public class Outfit : ScriptableObject
{
    [Header("Details")]
    public OutfitName thisOutfit;
    public OutfitType outfitType;
    public OutfitStage outfitStage = OutfitStage.StageOne;
    public string outfitTitle;
    
    [Header("Settings")]
    public bool SpawnAs = true;
    public bool Jiggle;
    
    [Header("Style")]
    public List<GameObject> OutfitPrefabs;
    public Color lipsColor = new Color(0.95f, 0.6f, 0.7f);
    public Color nailsColor = new Color(0.9f, 0.7f, 0.8f);
    public HairName Hair = HairName.DefaultHair;
    public ShoesName Shoes = ShoesName.WorkFlats;
    
    [Header("Accessories")]
    public bool Wings;
    public bool Apron;
    public bool Hat;
    public bool Choker;
    public bool Glasses;
    public bool Cigarette;
}


#if UNITY_EDITOR

[CustomEditor(typeof(Outfit))]
public class OutfitEditor : Editor
{
    private const int StatTitleFontSize = 12;

    public override void OnInspectorGUI()
    {
        Outfit outfit = (Outfit)target;

        var outfitButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 10,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white },
            padding = new RectOffset(3, 3, 4, 4)
        };

        Color applyButtonColor = outfit.outfitType switch
        {
            OutfitType.Work => Color.cyan,
            OutfitType.Main => Color.green,
            OutfitType.Pyjamas => new Color(0.5f, 0.8f, 1f),
            OutfitType.NightOut => new Color(1f, 0.6f, 0.8f),
            OutfitType.Special => new Color(0.7f, 0.4f, 1f),
            OutfitType.Storyline => new Color(0.1f, 0.1f, 0.5f),
            OutfitType.Undergarments => Color.red,
            _ => Color.white
        };

        EditorGUILayout.Space();

        Texture2D backgroundTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/" + outfit.outfitType + "BG.png");

        var boxStyle = new GUIStyle { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };

        boxStyle.normal.textColor = Color.white;
        boxStyle.normal.background = backgroundTexture;

        var statsFieldStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 10, alignment = TextAnchor.MiddleLeft, richText = true, normal = { textColor = Color.white } };
        
        var sectionHeadingStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 14, alignment = TextAnchor.MiddleLeft, normal = { textColor = Color.cyan } };

        EditorGUILayout.BeginVertical(boxStyle);


        DrawStat("Outfit ID:", outfit.thisOutfit.ToString(), statsFieldStyle);
        DrawStat("Type:", outfit.outfitType.ToString(), statsFieldStyle);
        DrawStat("Stage:", outfit.outfitStage.ToString(), statsFieldStyle);
        DrawStat("Title:", outfit.outfitTitle, statsFieldStyle);



        DrawStat("Spawnable:", outfit.SpawnAs.ToString(), statsFieldStyle);
        DrawStat("Jiggle Enabled:", outfit.Jiggle.ToString(), statsFieldStyle);



        DrawColourStat("Lips:", outfit.lipsColor);
        DrawColourStat("Nails:", outfit.nailsColor);
        DrawStat("Selected Hair:", outfit.Hair.ToString(), statsFieldStyle);
        DrawStat("Shoes:", outfit.Shoes.ToString(), statsFieldStyle);



        DrawStat("Wings:", outfit.Wings.ToString(), statsFieldStyle);
        DrawStat("Apron:", outfit.Apron.ToString(), statsFieldStyle);
        DrawStat("Hat:", outfit.Hat.ToString(), statsFieldStyle);
        DrawStat("Choker:", outfit.Choker.ToString(), statsFieldStyle);
        DrawStat("Glasses:", outfit.Glasses.ToString(), statsFieldStyle);
        DrawStat("Cigarette:", outfit.Cigarette.ToString(), statsFieldStyle);

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = applyButtonColor;
        
        if (GUILayout.Button("Select Asset", outfitButtonStyle, GUILayout.Height(20))) { Selection.activeObject = outfit; EditorGUIUtility.PingObject(outfit); }

        if (GUILayout.Button("Copy Outfit ID", outfitButtonStyle, GUILayout.Height(20))) { EditorGUIUtility.systemCopyBuffer = outfit.thisOutfit.ToString(); }

        EditorGUILayout.EndHorizontal();
        
        string thisOutfit = outfit.thisOutfit.ToString().Length < 25 ? outfit.thisOutfit.ToString() : outfit.thisOutfit.ToString().Substring(0, 25) + "...";

        if (GUILayout.Button($"Apply\n{thisOutfit}", outfitButtonStyle, GUILayout.Height(30))) { ApplyOutfit(outfit); }

        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        DrawDefaultInspector();
    }

    private static void DrawStat(string title, string value, GUIStyle style)
    {
        EditorGUILayout.LabelField($"<size={StatTitleFontSize}>{title}</size>  <b>{value}</b>", style);
    }

    private static void DrawColourStat(string title, Color colour)
    {
        EditorGUILayout.BeginVertical();

        var titleStyle = new GUIStyle(GUI.skin.label) { fontSize = StatTitleFontSize, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = Color.white } };

        EditorGUILayout.LabelField(title, titleStyle);

        string hex = ColorUtility.ToHtmlStringRGBA(colour);
        string rgba = $"new Color({colour.r:0.00}f, {colour.g:0.00}f, {colour.b:0.00}f, {colour.a:0.00}f)";
        string rgbatext = $"{colour.r:0.00}, {colour.g:0.00}, {colour.b:0.00}, {colour.a:0.00}";

        var colourButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 10,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = GetReadableTextColour(colour) }
        };

        Color previousBackground = GUI.backgroundColor;

        GUI.backgroundColor = colour;

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button($"#{hex}", colourButtonStyle, GUILayout.Height(16), GUILayout.Width(80)))
        {
            EditorGUIUtility.systemCopyBuffer = $"#{hex}";
        }

        if (GUILayout.Button(rgbatext, colourButtonStyle, GUILayout.Height(16), GUILayout.Width(140)))
        {
            EditorGUIUtility.systemCopyBuffer = rgba;
        }

        EditorGUILayout.EndHorizontal();

        GUI.backgroundColor = previousBackground;

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
    }

    private static Color GetReadableTextColour(Color colour)
    {
        float luminance = (0.299f * colour.r) + (0.587f * colour.g) + (0.114f * colour.b);

        return luminance > 0.55f ? Color.black : Color.white;
    }

    private static void ApplyOutfit(Outfit outfit)
    {
        NorasWardrobe wardrobe = Application.isPlaying && Player.Instance != null ? GameMaster.Instance.NorasWardrobe : FindFirstObjectByType<NorasWardrobe>();

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