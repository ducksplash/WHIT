using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif


[CreateAssetMenu(fileName = "NPCOutfit", menuName = "{!!} Tawley Scriptable Object/NPCOutfit", order = 10)]
public class NPCOutfit : ScriptableObject
{
    [Header("Details")]
    public NPCOutfitName thisOutfit;
    public NPCOutfitType NPCOutfitType;
    public string outfitTitle;
    
    public bool SpawnAs = true;
    public bool Jiggle;
    public bool MarkedForDeletion = false;
    
    public List<GameObject> OutfitPrefabs;
    public Color lipsColor = new Color(0.95f, 0.6f, 0.7f);
    public Color nailsColor = new Color(0.9f, 0.7f, 0.8f);
    public NPCHairName Hair = NPCHairName.DefaultHair;
    public NPCShoesName Shoes = NPCShoesName.Default;
    public NPCNecklaceName Necklace = NPCNecklaceName.None;
    public NPCGlassesName Glasses = NPCGlassesName.None;
    public NPCBodies Body = NPCBodies.Normal;
    public NPCEyes NPCEyes = NPCEyes.Default;

    public NPCWingsName Wings = NPCWingsName.None;
    public NPCApronName Apron = NPCApronName.None;
    public NPCHatName Hat = NPCHatName.None;
    public NPCChokerAccessoryName Choker = NPCChokerAccessoryName.None;
    public NPCViceName Vice = NPCViceName.None;
}


#if UNITY_EDITOR

[CustomEditor(typeof(NPCOutfit))]
public class NPCOutfitEditor : Editor
{
    private const int StatTitleFontSize = 12;

    public override void OnInspectorGUI()
    {
        NPCOutfit outfit = (NPCOutfit)target;

        var outfitButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 10,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white },
            padding = new RectOffset(3, 3, 4, 4)
        };

        Color applyButtonColor = outfit.NPCOutfitType switch
        {
            NPCOutfitType.Main => Color.cyan,
            NPCOutfitType.NightOut => Color.red,
            NPCOutfitType.Special => new Color(0.7f, 0.4f, 1f),
            NPCOutfitType.Storyline => new Color(0.1f, 0.1f, 0.5f),
            _ => Color.white
        };

        EditorGUILayout.Space();

        Texture2D backgroundTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/" + outfit.NPCOutfitType + "BGNPC.png");

        var boxStyle = new GUIStyle { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };

        boxStyle.normal.textColor = Color.white;
        boxStyle.normal.background = backgroundTexture;

        var statsFieldStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 14, alignment = TextAnchor.MiddleLeft, richText = true, normal = { textColor = Color.white } };
        

        EditorGUILayout.BeginVertical(boxStyle);


        DrawStat("Title:", outfit.outfitTitle, statsFieldStyle);
        DrawStat("Outfit ID:", "#"+(int)outfit.thisOutfit + " [" + outfit.thisOutfit.ToString() + "]", statsFieldStyle);
        DrawStat("Type:", outfit.NPCOutfitType.ToString(), statsFieldStyle);
        DrawStat("Spawnable:", outfit.SpawnAs.ToString(), statsFieldStyle);
        DrawStat("Jiggle:", outfit.Jiggle.ToString(), statsFieldStyle);
        DrawStat("Marked For Deletion:", outfit.MarkedForDeletion.ToString(), statsFieldStyle);
        
        DrawColourStat("Lips:", outfit.lipsColor);
        DrawColourStat("Nails:", outfit.nailsColor);
        DrawStat("Hair:", outfit.Hair.ToString(), statsFieldStyle);
        DrawStat("Shoes:", outfit.Shoes.ToString(), statsFieldStyle);
        DrawStat("Necklace:", outfit.Necklace.ToString(), statsFieldStyle);
        DrawStat("Glasses:", outfit.Glasses.ToString(), statsFieldStyle);
        DrawStat("Body:", outfit.Body.ToString(), statsFieldStyle);
        DrawStat("Eyes:", outfit.NPCEyes.ToString(), statsFieldStyle);

        DrawStat("Wings:", outfit.Wings.ToString(), statsFieldStyle);
        DrawStat("Apron:", outfit.Apron.ToString(), statsFieldStyle);
        DrawStat("Hat:", outfit.Hat.ToString(), statsFieldStyle);
        DrawStat("Choker:", outfit.Choker.ToString(), statsFieldStyle);
        DrawStat("Vice:", outfit.Vice.ToString(), statsFieldStyle);

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = applyButtonColor;
        
        if (GUILayout.Button("Select Asset", outfitButtonStyle, GUILayout.Height(20))) { Selection.activeObject = outfit; EditorGUIUtility.PingObject(outfit); }

        if (GUILayout.Button("Copy Outfit ID", outfitButtonStyle, GUILayout.Height(20))) { EditorGUIUtility.systemCopyBuffer = outfit.thisOutfit.ToString(); }

        EditorGUILayout.EndHorizontal();
        
        
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        DrawDefaultInspector();

        EditorGUILayout.Space();

        GUI.backgroundColor = outfit.MarkedForDeletion ? Color.green : Color.red;

        string deletionButtonLabel = outfit.MarkedForDeletion ? "Un-Mark For Deletion" : "Mark For Deletion";

        if (GUILayout.Button(deletionButtonLabel, outfitButtonStyle, GUILayout.Height(28)))
        {
            outfit.MarkedForDeletion = !outfit.MarkedForDeletion;
            EditorUtility.SetDirty(outfit);
        }

        GUI.backgroundColor = Color.white;
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

    }

    private static Color GetReadableTextColour(Color colour)
    {
        float luminance = (0.299f * colour.r) + (0.587f * colour.g) + (0.114f * colour.b);

        return luminance > 0.55f ? Color.black : Color.white;
    }
    
}

#endif