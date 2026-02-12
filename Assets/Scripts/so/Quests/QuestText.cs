using UnityEngine;

[CreateAssetMenu(
    fileName = "QuestText",
    menuName = "{!!} Tawley Scriptable Object/QuestText",
    order = 10)]
public class QuestText : ScriptableObject
{
    [Header("Quest Text")]
    [TextArea(3, 10)]
    public string QuestTitleString; // This is in English

    
    [Header("Quest Objectives")]
    public string Objective1String; // This is in English
    public bool Objective1Complete;
    
    public string Objective2String; // This is in English
    public bool Objective2Complete;
    
    public string Objective3String; // This is in English
    public bool Objective3Complete;
    
    public string Objective4String; // This is in English
    public bool Objective4Complete;
    
    public string Objective5String; // This is in English
    public bool Objective5Complete;
    
    public QuestName QuestName = QuestName.GetReadyForWork;
    
    [Header("Localisation")] 
    public Languages QuestLanguage = Languages.EN; // English for default

    [Header("Color")] 
    public TextColour QtestTextColor = TextColour.white;

    [Header("Irish Text")]
    [TextArea(3, 10)]
    public string IrishQuestTextString;
    
    [Header("French Text")]
    [TextArea(3, 10)]
    public string FrenchQuestTextString;
    
    [Header("German Text")]
    [TextArea(3, 10)]
    public string GermanQuestTextString;
    
    [Header("Spanish Text")]
    [TextArea(3, 10)]
    public string SpanishQuestTextString;
    
    [Header("Korean Text")]
    [TextArea(3, 10)]
    public string KoreanQuestTextString;
    
    [Header("Arabic Text")]
    [TextArea(3, 10)]
    public string ArabicQuestTextString;
    
    [Header("Japanese Text")]
    [TextArea(3, 10)]
    public string JapaneseQuestTextString;
    
    [Header("Chinese Text")]
    [TextArea(3, 10)]
    public string ChineseQuestTextString;

    [Header("Russian Text")] 
    [TextArea(3, 10)]
    public string RussianQuestTextString;
}
