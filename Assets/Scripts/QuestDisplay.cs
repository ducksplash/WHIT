// QuestDisplay.cs
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Temporary on-screen quest popup.
/// Listens to QuestManager.QuestLoaded and displays QuestTitleString with typewriter + fade.
/// Typewriter reveals ONLY when:
/// - QuestStarted is false (first time shown)
/// - QuestCompleted is true (completion moment)
/// </summary>
public class QuestDisplay : MonoBehaviour
{
    [Header("Quest Text")]
    public TextMeshProUGUI questText;
    public TextMeshProUGUI questTextBG;
    public TextMeshProUGUI questStatusText;
    public TextMeshProUGUI questStatusTextBG;
    public Transform questTextBGimg;

    [Header("Canvas")]
    public CanvasGroup QuestCanvas;

    [Header("Display Time")]
    [Tooltip("Total visible time including all fade in, fade out and typing.")]
    public float DisplayTime = 0.02f;

    [Header("Typewriter")]
    [Tooltip("Seconds per character. Lower = faster typing.")]
    public float secondsPerCharacter = 0.02f;

    [Tooltip("Optional small pause after spaces (seconds).")]
    public float extraPauseOnSpace = 0.02f;

    [Tooltip("Extra pause after full stops (seconds).")]
    public float extraPauseOnPeriod = 0.15f;

    [Header("Typewriter Audio")]
    public System.Collections.Generic.List<SFXResource> typewriterKeySfx = new System.Collections.Generic.List<SFXResource>
    {
        SFXResource.TypeWriter0,
        SFXResource.TypeWriter1,
        SFXResource.TypeWriter2,
        SFXResource.TypeWriter3,
        SFXResource.TypeWriter4,
        SFXResource.TypeWriter5,
    };

    [Tooltip("Chance to skip a key sound (0 = always, 0.2 = 20% skipped).")]
    [Range(0f, 1f)]
    public float skipKeySoundChance = 0.0f;

    private int _lastKeyIndex = -1;

    private Coroutine _questTimerCo;
    private Coroutine _typewriterCo;

    
    private void LoadByScene()
    {
        SetQuestText(GameMaster.Instance.QuestManager.CurrentQuest);
    }
    
    
    private void OnEnable()
    {
        EventManager.OnGameStarted += LoadByScene;
    }

    private void OnDisable()
    {
        //if (GameMaster.Instance.QuestManager != null) GameMaster.Instance.QuestManager.QuestLoaded -= OnQuestLoaded;

        StopAllRunning();
    }

    private void OnQuestLoaded(QuestText text)
    {
        SetQuestText(text);
    }

    private void SetQuestText(QuestText text)
    {
        if (text == null) return;
        if (questText == null || questTextBG == null || QuestCanvas == null) return;

        string title = text.QuestTitleString ?? string.Empty;

        questText.text = title;
        questTextBG.text = title;

        // Status label
        string status = string.Empty;
        if (text.QuestCompleted)
            status = "Completed";
        else if (text.QuestStarted)
            status = "Updated";

        if (questStatusText != null) questStatusText.text = status;
        if (questStatusTextBG != null) questStatusTextBG.text = status;

        // Keep your existing color logic.
        questText.color = text.QuestTextColor.ToColor();

        if (questTextBGimg is RectTransform rect)
            rect.sizeDelta = new Vector2(title.Length * 36, 70);

        StopAllRunning();

        // Decide whether to typewriter reveal:
        // - first time (QuestStarted == false)
        // - completion moment (QuestCompleted == true)
        bool shouldTypewriter = !text.QuestStarted || text.QuestCompleted;

        if (shouldTypewriter)
        {
            questText.maxVisibleCharacters = 0;
            questTextBG.maxVisibleCharacters = 0;
            _typewriterCo = StartCoroutine(TypewriterReveal(title));
        }
        else
        {
            // Show instantly (no typewriter)
            questText.maxVisibleCharacters = int.MaxValue;
            questTextBG.maxVisibleCharacters = int.MaxValue;
        }

        _questTimerCo = StartCoroutine(QuestDisplayRoutine());
    }

    private void StopAllRunning()
    {
        if (_questTimerCo != null) { StopCoroutine(_questTimerCo); _questTimerCo = null; }
        if (_typewriterCo != null) { StopCoroutine(_typewriterCo); _typewriterCo = null; }
    }

    private IEnumerator TypewriterReveal(string fullText)
    {
        if (questText == null || questTextBG == null) yield break;

        questText.ForceMeshUpdate();
        int total = questText.textInfo.characterCount;
        if (total <= 0) total = fullText.Length;

        for (int i = 0; i < total; i++)
        {
            questText.maxVisibleCharacters = i + 1;
            questTextBG.maxVisibleCharacters = i + 1;

            float wait = secondsPerCharacter;

            if (i < fullText.Length && fullText[i] == ' ')
                wait += extraPauseOnSpace;

            if (i + 1 < fullText.Length && fullText[i + 1] == '.')
                wait += extraPauseOnPeriod;

            if (i < fullText.Length && fullText[i] == '.')
                wait = 0f;

            if (wait > 0f) yield return new WaitForSeconds(wait);
            else yield return null;

            if (i < fullText.Length && fullText[i] != ' ')
                PlayRandomTypewriterKey();
        }
    }

    private IEnumerator QuestDisplayRoutine()
    {
        yield return StartCoroutine(FadeInQuestText(DisplayTime / 4f));
        yield return new WaitForSeconds(DisplayTime / 2f);
        yield return StartCoroutine(FadeOutQuestText(DisplayTime / 3f));
    }

    private IEnumerator FadeInQuestText(float fadeInTime)
    {
        if (QuestCanvas == null) yield break;

        float t = 0f;
        float denom = Mathf.Max(0.0001f, fadeInTime / 2f);

        while (t < 1f)
        {
            t += Time.smoothDeltaTime / denom;
            QuestCanvas.alpha = Mathf.Lerp(0f, 1f, t);
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator FadeOutQuestText(float fadeOutTime)
    {
        if (QuestCanvas == null) yield break;

        float t = 0f;
        float denom = Mathf.Max(0.0001f, fadeOutTime / 2f);

        while (t < 1f)
        {
            t += Time.smoothDeltaTime / denom;
            QuestCanvas.alpha = Mathf.Lerp(1f, 0f, t);
            yield return new WaitForEndOfFrame();
        }
    }

    private void PlayRandomTypewriterKey()
    {
        if (GameMaster.Instance == null || GameMaster.Instance.AudioSlave == null) return;
        if (typewriterKeySfx == null || typewriterKeySfx.Count == 0) return;

        if (skipKeySoundChance > 0f && Random.value < skipKeySoundChance) return;

        int idx = Random.Range(0, typewriterKeySfx.Count);
        if (typewriterKeySfx.Count > 1 && idx == _lastKeyIndex)
            idx = (idx + 1) % typewriterKeySfx.Count;

        _lastKeyIndex = idx;
        GameMaster.Instance.AudioSlave.PlaySFX(typewriterKeySfx[idx]);
    }
}
