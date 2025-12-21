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
    
    [Header("Dialogue Text")]
    [TextArea(3, 10)]
    public string DialogueText;

    
    
    [Header("Follow Up Dialogue")]
    public bool HasFollowUpDialogue = false;
    public DialogueName FollowupDialogueName;
    public DialogueType FollowupDialogueType;
}