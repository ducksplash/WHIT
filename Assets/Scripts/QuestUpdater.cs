// using System;
// using System.Collections;
// using System.Collections.Generic;
// using TMPro;
// using UnityEngine;
// using System.Linq;
//
//
// #if UNITY_EDITOR
// using UnityEditor;
// #endif
//
// public class QuestUpdater : Singleton<QuestUpdater>
// {
//     [Header("Quest Text")]
//     public TextMeshProUGUI questText;
//     public TextMeshProUGUI questTextBG;
//     public Transform questTextBGimg;
//
//     [Header("Canvassy Stuff")]
//     public CanvasGroup QuestCanvas;
//
//     [Header("Quests")]
//     public List<QuestText> QuestList = new List<QuestText>();
//
//     public QuestText CurrentQuest;
//
//     [Header("Display Time")]
//     [Tooltip("Total visible time including all fade in, fade out and typing.")]
//     public float DisplayTime = 0.02f;
//
//     [Header("Typewriter")]
//     [Tooltip("Seconds per character. Lower = faster typing.")]
//     public float secondsPerCharacter = 0.02f;
//
//     [Tooltip("Optional small pause after spaces (seconds).")]
//     public float extraPauseOnSpace = 0.02f;
//
//     [Tooltip("Extra pause after full stops (seconds).")]
//     public float extraPauseOnPeriod = 0.15f;
//     
//     [Header("Typewriter Audio")]
//     public List<SFXResource> typewriterKeySfx = new List<SFXResource>
//     {
//         SFXResource.TypeWriter0,
//         SFXResource.TypeWriter1,
//         SFXResource.TypeWriter2,
//         SFXResource.TypeWriter3,
//         SFXResource.TypeWriter4,
//         SFXResource.TypeWriter5,
//     };
//
//     [Tooltip("Chance to skip a key sound (0 = always, 0.2 = 20% skipped).")]
//     [Range(0f, 1f)]
//     public float skipKeySoundChance = 0.0f;
//
//     private int _lastKeyIndex = -1;
//
//     
//     private Coroutine questTimerCo;
//     private Coroutine typewriterCo;
//
//     private void Start()
//     {
//         StoredPrefs.OnPlayerDataLoaded += FindLastQuest;
//     }
//
//     public void FindLastQuest()
//     {
//         Debug.Log("loaded now get");
//         string QuestItemText = StoredPrefs.Instance.GetString("CurrentQuest", "#");
//         
//         if (Enum.TryParse(QuestItemText, true, out QuestName questEnum))
//         {
//             CurrentQuest = QuestList.FirstOrDefault(q => q != null && q.QuestName == questEnum);
//         }
//         else
//         {
//             CurrentQuest = QuestList[0];
//         }
//
//         SetQuestText(CurrentQuest);
//     }
//     
//     
//     
//
//     public void SetQuestText(QuestText text)
//     {
//         if (text == null) return;
//
//         questText.text = text.QuestTitleString;
//         questTextBG.text = text.QuestTitleString;
//
//         questText.color = text.QtestTextColor.ToColor();
//
//         if (questTextBGimg is RectTransform rect) rect.sizeDelta = new Vector2(text.QuestTitleString.Length * 36, 70);
//
//         if (questTimerCo != null) { StopCoroutine(questTimerCo); questTimerCo = null; }
//         if (typewriterCo != null) { StopCoroutine(typewriterCo); typewriterCo = null; }
//
//         questText.maxVisibleCharacters = 0;
//         questTextBG.maxVisibleCharacters = 0;
//
//         typewriterCo = StartCoroutine(TypewriterReveal(text.QuestTitleString));
//         questTimerCo = StartCoroutine(QuestDisplay());
//
//         GameMaster.Instance.EventManager.QuestLoaded(text);
//
//     }
//
//
//
//     public void UpdateQuest(QuestText text)
//     {
//                 
//         StoredPrefs.Instance.SetString("CurrentQuest", text.ToString());
//         StoredPrefs.Instance.Save();
//
//         SetQuestText(text);
//     }
//     
//     
//     
//     private IEnumerator TypewriterReveal(string fullText)
//     {
//         if (questText == null || questTextBG == null) yield break;
//
//         questText.ForceMeshUpdate();
//         int total = questText.textInfo.characterCount;
//         if (total <= 0) total = fullText.Length;
//
//         // Reveal characters one by one
//         for (int i = 0; i < total; i++)
//         {
//             questText.maxVisibleCharacters = i + 1;
//             questTextBG.maxVisibleCharacters = i + 1;
//
//             float wait = secondsPerCharacter;
//
//             if (i < fullText.Length && fullText[i] == ' ') wait += extraPauseOnSpace;
//
//             if (i + 1 < fullText.Length && fullText[i + 1] == '.') wait += extraPauseOnPeriod;
//
//             if (i < fullText.Length && fullText[i] == '.') wait = 0f;
//
//             if (wait > 0f)
//             {
//                 yield return new WaitForSeconds(wait);
//             }
//             else
//             {
//                 yield return null;
//             }
//             
//             if (fullText[i] != ' ') PlayRandomTypewriterKey();
//         }
//     }
//     
//
//     private IEnumerator QuestDisplay()
//     {
//         StartCoroutine(FadeInQuestText(DisplayTime / 4));
//
//         yield return new WaitForSeconds(DisplayTime / 2);
//
//         StartCoroutine(FadeOutQuestText(DisplayTime / 3));
//     }
//
//     public IEnumerator FadeInQuestText(float fadeInTime)
//     {
//         float t = 0f;
//         while (t < 1f)
//         {
//             t += Time.smoothDeltaTime / (fadeInTime / 2);
//             QuestCanvas.alpha = Mathf.Lerp(0f, 1f, t);
//             yield return new WaitForEndOfFrame();
//         }
//     }
//
//     public IEnumerator FadeOutQuestText(float fadeOutTime)
//     {
//         float t = 0f;
//         while (t < 1f)
//         {
//             t += Time.smoothDeltaTime / (fadeOutTime / 2);
//             QuestCanvas.alpha = Mathf.Lerp(1f, 0f, t);
//             yield return new WaitForEndOfFrame();
//         }
//     }
//     
//     private void PlayRandomTypewriterKey()
//     {
//         if (GameMaster.Instance == null || GameMaster.Instance.AudioSlave == null) return;
//         if (typewriterKeySfx == null || typewriterKeySfx.Count == 0) return;
//
//         if (skipKeySoundChance > 0f && UnityEngine.Random.value < skipKeySoundChance) return;
//
//         // Pick random, avoid repeating the same clip twice in a row
//         int idx = UnityEngine.Random.Range(0, typewriterKeySfx.Count);
//         if (typewriterKeySfx.Count > 1 && idx == _lastKeyIndex)
//             idx = (idx + 1) % typewriterKeySfx.Count;
//
//         _lastKeyIndex = idx;
//
//         GameMaster.Instance.AudioSlave.PlaySFX(typewriterKeySfx[idx]);
//     }
//
// }
//
// public static class TextColours
// {
//     public static Color ToColor(this TextColour colour)
//     {
//         return colour switch
//         {
//             TextColour.white => Color.white,
//             TextColour.black => Color.black,
//             TextColour.red => Color.red,
//             TextColour.green => Color.green,
//             TextColour.blue => Color.blue,
//             TextColour.yellow => Color.yellow,
//             TextColour.cyan => Color.cyan,
//             TextColour.magenta => Color.magenta,
//             TextColour.grey => Color.grey,
//             TextColour.clear => Color.clear,
//             _ => Color.white
//         };
//     }
// }
//
// public enum TextColour
// {
//     red,
//     green,
//     blue,
//     white,
//     black,
//     yellow,
//     cyan,
//     magenta,
//     gray,
//     grey,
//     clear
// }
//
//
// public enum QuestName
// {
//     GetReadyForWork,
//     FindSomeEvidence
// }
//
// #if UNITY_EDITOR
// [CustomEditor(typeof(QuestUpdater))]
// public class QuestUpdaterEditor : Editor
// {
//     private int _selectedQuestIndex;
//     private bool _overrideColour;
//     private TextColour _overrideTextColour = TextColour.white;
//
//     public override void OnInspectorGUI()
//     {
//         DrawDefaultInspector();
//
//         var updater = (QuestUpdater)target;
//
//         EditorGUILayout.Space(12);
//         EditorGUILayout.LabelField("Quest Debug Controls", EditorStyles.boldLabel);
//
//         if (updater.QuestList == null || updater.QuestList.Count == 0)
//         {
//             EditorGUILayout.HelpBox("QuestList is empty. Add QuestText assets to QuestList first.", MessageType.Warning);
//             return;
//         }
//
//         string[] options = BuildQuestOptions(updater.QuestList);
//
//         _selectedQuestIndex = Mathf.Clamp(_selectedQuestIndex, 0, options.Length - 1);
//         _selectedQuestIndex = EditorGUILayout.Popup("QuestText", _selectedQuestIndex, options);
//
//         _overrideColour = EditorGUILayout.Toggle("Override Colour", _overrideColour);
//         using (new EditorGUI.DisabledScope(!_overrideColour))
//         {
//             _overrideTextColour = (TextColour)EditorGUILayout.EnumPopup("Text Colour", _overrideTextColour);
//         }
//
//         using (new EditorGUI.DisabledScope(!Application.isPlaying))
//         {
//             if (GUILayout.Button("SetQuestText"))
//             {
//                 QuestText selected = updater.QuestList[_selectedQuestIndex];
//                 updater.CurrentQuest = selected;
//                 updater.SetQuestText(selected);
//
//                 if (_overrideColour && updater.questText != null)
//                 {
//                     updater.questText.color = _overrideTextColour.ToColor();
//                 }
//
//                 EditorUtility.SetDirty(updater);
//             }
//         }
//
//         if (!Application.isPlaying) { EditorGUILayout.HelpBox("Enter Play Mode to use SetQuestText (it calls runtime methods).", MessageType.Info); }
//     }
//
//     private static string[] BuildQuestOptions(List<QuestText> questList)
//     {
//         var options = new string[questList.Count];
//         for (int i = 0; i < questList.Count; i++)
//         {
//             QuestText q = questList[i];
//             options[i] = q == null ? $"(Missing) [{i}]" : q.name;
//         }
//         return options;
//     }
// }
// #endif
