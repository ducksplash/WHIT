using System.Collections.Generic;
using UnityEngine;

public class LanguageManager : MonoBehaviour
{
    public bool placeHolderVariable = true;

    private Dictionary<TranslatableStrings, Languages> StringTranslationDict = new Dictionary<TranslatableStrings, Languages>();
    
    
    void Start()
    {
        Debug.Log("todo: language manager");
    }

    public void BuildTranslationDict()
    {
        StringTranslationDict.Add(TranslatableStrings.calling, Languages.EN);
    }

    public string GetTranslationOf(TranslatableStrings targetString, Languages targetLanguage)
    {
        string returnableString = "";


        return returnableString;
    }
    
    
}

// Strings to be used in game, translatables.
public enum TranslatableStrings {
    calling = 100,
}

public enum Languages
{
    EN, // ENGLISH (UK)
    IE, // IRISH (GAEILGE)
    FR, // FRENCH (FRANCE)
    DE, // GERMAN (BAVARIA)
    ES, // SPANISH (SPAIN)
    RU, // RUSSIAN (CYRILLIC)
    
    JP, // JAPANESE 
    CN, // CHINESE (SIMPLIFIED)
    KR, // KOREAN
    TG, // TAGALOG
    
    AR, // ARABIC (PERSIAN)
    HB, // HEBREW (MODERN)
    
    // ToDo: Need a lot of fonts man
}
