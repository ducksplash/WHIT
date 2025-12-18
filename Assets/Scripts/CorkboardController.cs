using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CorkboardController : MonoBehaviour
{
    public List<Image> corkBoardImages = new List<Image>();
    public Color baseColor;

    private void Start()
    {
        if (corkBoardImages[0] != null)
        {
            baseColor = corkBoardImages[0].color;
        }
        
        LightenImages(); // Normalise the basecolours so that I don't have to

        EventManager.OnLightToggleChanged += ToggleBrightness;
    }


    public void ToggleBrightness(bool isOn)
    {
        if (isOn)
        {
            LightenImages();
        }
        else
        {
            DarkenImages();
        }
    }
    
    
    public void DarkenImages()
    {
        foreach (Image img in corkBoardImages)
        {
            img.color = new Color(0.3f,0.3f,0.3f,1);
        }
    }

    public void LightenImages()
    {
        foreach (Image img in corkBoardImages)
        {
            img.color = baseColor;
        }
    }
    
    
}


#if UNITY_EDITOR
[CustomEditor(typeof(CorkboardController))]
public class CorkboardControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CorkboardController controller = (CorkboardController)target;

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Darken")) controller.DarkenImages();
        if (GUILayout.Button("Lighten")) controller.LightenImages();

        GUILayout.EndHorizontal();
    }
}
#endif
