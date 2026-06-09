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

    [Header("Thought Settings")]
    [Tooltip("How long each thought fades in then out (seconds)")]
    public float thoughtFadeDuration = 3f;
    [Tooltip("How long the final thought fades out (seconds)")]
    public float thoughtFinalFadeDuration = 4f;
    [Tooltip("Delay between each thought starting — should be less than thoughtFadeDuration to create overlap")]
    public float thoughtStaggerDelay = 1.5f;
    [Tooltip("How long the colour-to-white fade-in takes at the start of each thought")]
    public float thoughtFadeInDuration = 0.3f;
    
    [Header("Dialogue contains replaceable strings i.e. for key binds.")]
    public bool EregiReplace = false;
    
    [Header("Localisation")] 
    public Languages DialogueLanguage = Languages.EN; // English for default
    
    [Header("Irish Text")]
    [TextArea(3, 10)]
    public List<string> IrishThoughtString = new List<string>(); 
    
    [Header("French Text")]
    [TextArea(3, 10)]
    public List<string> FrenchThoughtString = new List<string>(); 
    
    [Header("German Text")]
    [TextArea(3, 10)]
    public List<string> GermanThoughtString = new List<string>(); 
    
    [Header("Spanish Text")]
    [TextArea(3, 10)]
    public List<string> SpanishThoughtString = new List<string>(); 
    
    [Header("Korean Text")]
    [TextArea(3, 10)]
    public List<string> KoreanThoughtString = new List<string>(); 
    
    [Header("Arabic Text")]
    [TextArea(3, 10)]
    public List<string> ArabicThoughtString = new List<string>(); 
    
    [Header("Japanese Text")]
    [TextArea(3, 10)]
    public List<string> JapaneseThoughtString = new List<string>(); 
    
    [Header("Chinese Text")]
    [TextArea(3, 10)]
    public List<string> ChineseThoughtString = new List<string>(); 

    [Header("Russian Text")] 
    [TextArea(3, 10)]
    public List<string> RussianThoughtString = new List<string>(); 
}