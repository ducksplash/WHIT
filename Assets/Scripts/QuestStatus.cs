using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class QuestStatus : MonoBehaviour
{
    [Header("Texts")]
    public TextMeshProUGUI QuestTitleText;
    public TextMeshProUGUI Objective1Text;
    public TextMeshProUGUI Objective2Text;
    public TextMeshProUGUI Objective3Text;
    public TextMeshProUGUI Objective4Text;
    public TextMeshProUGUI Objective5Text;

    [Header("Objective Rows (CanvasGroups)")]
    public CanvasGroup ObjectiveComplete1;
    public CanvasGroup ObjectiveComplete2;
    public CanvasGroup ObjectiveComplete3;
    public CanvasGroup ObjectiveComplete4;
    public CanvasGroup ObjectiveComplete5;

    // Strips TMP/HTML-like tags: <...>
    private static readonly Regex _tagRegex = new Regex("<.*?>", RegexOptions.Compiled);

    private void Start()
    {
        EventManager.OnQuestLoaded += SetStatus;
    }

    private void OnDestroy()
    {
        EventManager.OnQuestLoaded -= SetStatus;
    }

    public void SetStatus(QuestText sentQuestText)
    {
        // ALWAYS start from a known hidden state
        ForceHide(Objective1Text, ObjectiveComplete1);
        ForceHide(Objective2Text, ObjectiveComplete2);
        ForceHide(Objective3Text, ObjectiveComplete3);
        ForceHide(Objective4Text, ObjectiveComplete4);
        ForceHide(Objective5Text, ObjectiveComplete5);

        if (sentQuestText == null)
        {
            if (QuestTitleText != null) QuestTitleText.text = string.Empty;
            return;
        }

        if (QuestTitleText != null)
            QuestTitleText.text = sentQuestText.QuestTitleString ?? string.Empty;

        ApplyObjective(sentQuestText.Objective1String, sentQuestText.Objective1Complete, Objective1Text, ObjectiveComplete1);
        ApplyObjective(sentQuestText.Objective2String, sentQuestText.Objective2Complete, Objective2Text, ObjectiveComplete2);
        ApplyObjective(sentQuestText.Objective3String, sentQuestText.Objective3Complete, Objective3Text, ObjectiveComplete3);
        ApplyObjective(sentQuestText.Objective4String, sentQuestText.Objective4Complete, Objective4Text, ObjectiveComplete4);
        ApplyObjective(sentQuestText.Objective5String, sentQuestText.Objective5Complete, Objective5Text, ObjectiveComplete5);
    }

    private void ApplyObjective(string objectiveText, bool isComplete, TextMeshProUGUI textField, CanvasGroup row)
    {
        string cleaned = NormalizeObjective(objectiveText);
        bool exists = !string.IsNullOrEmpty(cleaned);

        if (textField != null)
            textField.text = exists ? cleaned : string.Empty;

        if (row != null)
        {
            // ✅ show ONLY if it exists AND is complete
            row.alpha = (exists && isComplete) ? 1f : 0f;

            row.interactable = false;
            row.blocksRaycasts = false;
        }
    }

    private static string NormalizeObjective(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;

        string noTags = _tagRegex.Replace(s, string.Empty);
        return noTags.Trim();
    }

    private static void ForceHide(TextMeshProUGUI textField, CanvasGroup row)
    {
        if (textField != null) textField.text = string.Empty;

        if (row != null)
        {
            row.alpha = 0f;
            row.interactable = false;
            row.blocksRaycasts = false;
        }
    }
}
