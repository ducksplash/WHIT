using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(
    fileName = "InputText",
    menuName = "{!!} Tawley Scriptable Object/InputText",
    order = 10)]
public class InputSO : ScriptableObject
{
    [Header("Input Name")]
    public string InputTitleString;
    
    public InputName InputName = InputName.Jump;

    [Header("Input Keys")]
    public string InputKeyDesktop;
    public string InputKeySteam;
    
    
    [Header("Replaceable Dialogue String")]
    public string EregiReplaceString;
    

    
    
    [Header("Localisation")] 
    public Languages InputLanguage = Languages.EN; // English for default


    [Header("Irish Text")]
    public string IrishInputTitleString;
    
    [Header("French Text")]
    public string FrenchInputTitleString;
    
    [Header("German Text")]
    public string GermanInputTitleString;
    
    [Header("Spanish Text")]
    public string SpanishInputTitleString;
    
    [Header("Korean Text")]
    public string KoreanInputTitleString;
    
    [Header("Arabic Text")]
    public string ArabicInputTitleString;
    
    [Header("Japanese Text")]
    public string JapaneseInputTitleString;
    
    [Header("Chinese Text")]
    public string ChineseInputTitleString;

    [Header("Russian Text")] 
    public string RussianInputTitleString;
}
