#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;

[InitializeOnLoad]
public static class ScriptableObjectIcons
{
    static readonly string RoutineIconPath = "Assets/Editor/NPCControllerIcons/routines.png";
    static readonly string BehaviourIconPath = "Assets/Editor/NPCControllerIcons/behaviours.png";

    static ScriptableObjectIcons()
    {
        EditorApplication.delayCall += ApplyIcons;
    }

    static void ApplyIcons()
    {
        TrySetIconForScript<NPCRoutine>(RoutineIconPath);
        TrySetIconForScript<NPCBehaviour>(BehaviourIconPath);
    }

    static void TrySetIconForScript<T>(string iconPath) where T : ScriptableObject
    {
        var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
        if (!icon)
            return;

        // Find the script asset that defines T
        string typeName = typeof(T).Name;
        string[] guids = AssetDatabase.FindAssets($"{typeName} t:MonoScript");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (script == null) continue;

            if (script.GetClass() == typeof(T))
            {
                EditorGUIUtility.SetIconForObject(script, icon);
                // no AssetDatabase.SaveAssets needed; this is editor-side metadata
                break;
            }
        }
    }
}
#endif