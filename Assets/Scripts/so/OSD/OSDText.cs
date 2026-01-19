using UnityEngine;

[CreateAssetMenu(
    fileName = "OSDText",
    menuName = "{!!} Tawley Scriptable Object/OSDText",
    order = 10)]
public class OSDText : ScriptableObject
{
    [Header("Dialogue Parameters")]
    public OSDTextName OSDTextName = OSDTextName.TakePhoto;

    [Header("Dialogue contains replaceable strings i.e. for key binds.")]
    public bool EregiReplace = false;
    
    [Header("OSD Text")]
    [TextArea(3, 10)]
    public string OSDTextString; // This is in English
    
    [Header("Localisation")] 
    public Languages DialogueLanguage = Languages.EN; // English for default
    
    [Header("Irish Text")]
    [TextArea(3, 10)]
    public string IrishOSDTextString;
    
    [Header("French Text")]
    [TextArea(3, 10)]
    public string FrenchOSDTextString;
    
    [Header("German Text")]
    [TextArea(3, 10)]
    public string GermanOSDTextString;
    
    [Header("Spanish Text")]
    [TextArea(3, 10)]
    public string SpanishOSDTextString;
    
    [Header("Korean Text")]
    [TextArea(3, 10)]
    public string KoreanOSDTextString;
    
    [Header("Arabic Text")]
    [TextArea(3, 10)]
    public string ArabicOSDTextString;
    
    [Header("Japanese Text")]
    [TextArea(3, 10)]
    public string JapaneseOSDTextString;
    
    [Header("Chinese Text")]
    [TextArea(3, 10)]
    public string ChineseOSDTextString;

    [Header("Russian Text")] [TextArea(3, 10)]
    public string RussianOSDTextString;
}