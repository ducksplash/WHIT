using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NoraThought",
    menuName = "{!!} Tawley Scriptable Object/NoraThought",
    order = 10)]
public class NoraThought : ScriptableObject
{

    [Header("Thought Designation")] 
    public ThoughtName ThoughtName;
    [Header("Thoughts Texts")]
    [TextArea(3, 10)]
    public List<string> NoraThoughtString = new List<string>(); 
    
    [Header("Dialogue contains replaceable strings i.e. for key binds.")]
    public bool EregiReplace = false;
    
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