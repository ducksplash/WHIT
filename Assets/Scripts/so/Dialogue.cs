using UnityEngine;

[CreateAssetMenu(
    fileName = "CharacterDialogue",
    menuName = "{!!} Tawley Scriptable Object/Dialogue",
    order = 10)]
public class Dialogue : ScriptableObject
{
    [Header("Dialogue Parameters")]
    public DialogueName DialogueName = DialogueName.None;
    public Contacts Contact = Contacts.System;
    public DialogueType DialogueType;

    [Header("Dialogue contains replaceable strings i.e. for key binds.")]
    public bool EregiReplace = false;
    
    [Header("Dialogue Text")]
    [TextArea(3, 10)]
    public string DialogueText; // This is in English
    
    [Header("Localisation")] 
    public Languages DialogueLanguage = Languages.EN; // English for default
    
    [Header("Irish Text")]
    [TextArea(3, 10)]
    public string IrishDialogueText;
    
    [Header("French Text")]
    [TextArea(3, 10)]
    public string FrenchDialogueText;
    
    [Header("German Text")]
    [TextArea(3, 10)]
    public string GermanDialogueText;
    
    [Header("Spanish Text")]
    [TextArea(3, 10)]
    public string SpanishDialogueText;
    
    [Header("Korean Text")]
    [TextArea(3, 10)]
    public string KoreanDialogueText;
    
    [Header("Arabic Text")]
    [TextArea(3, 10)]
    public string ArabicDialogueText;
    
    [Header("Japanese Text")]
    [TextArea(3, 10)]
    public string JapaneseDialogueText;
    
    [Header("Chinese Text")]
    [TextArea(3, 10)]
    public string ChineseDialogueText;
    
    [Header("Russian Text")]
    [TextArea(3, 10)]
    public string RussianDialogueText;
}