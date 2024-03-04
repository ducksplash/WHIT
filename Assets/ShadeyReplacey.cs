using UnityEngine;
using UnityEditor;

public class URPShaderConverter : EditorWindow
{
    [MenuItem("Tools/Convert Materials to URP")]
    static void ConvertMaterialsToURP()
    {
        // Get all materials in the project
        string[] materialGuids = AssetDatabase.FindAssets("t:Material");
        foreach (string materialGuid in materialGuids)
        {
            string materialPath = AssetDatabase.GUIDToAssetPath(materialGuid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

            // Check if the material's shader is HDRP

                // Find and assign the URP Lit shader
                Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
                if (urpShader != null)
                {
                    material.shader = urpShader;
                    EditorUtility.SetDirty(material);
                }
            
        }

        Debug.Log("Material conversion complete!");
    }
}