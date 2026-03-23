using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class DialogueBeef : MonoBehaviour
{
    
    [Header("Dev Placeholder Cube")]
    public GameObject ColliderCube;

    [Header("Dialogue To Trigger:")] 
    public DialogueName selectedDialogue;

    [Header("Display time in seconds")]
    public float DisplayTimer = 5.0f;


    [Header("Does Nora Reply?")]
    public bool Noraply;

    [Header("Nora's Dialogue:")]
    public DialogueName followUpDialogue;

    [Header("Delay before Nora's reply?")]
    public float Noradelay = 5.0f;


    [Header("Debug (ignore)")]
    public float forhowlong;
    
    
    void Start() 
    {
        ColliderCube.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3)
        {

            gameObject.GetComponent<Collider>().enabled = false;


            forhowlong = DisplayTimer;

            GameMaster.Instance.DialogueManager.NewDialogue(selectedDialogue, DisplayTimer);
           
            if (Noraply)
            {
                StartCoroutine(Norasponse());
            }
        }
    }

    
    public IEnumerator Norasponse()
    {
        yield return new WaitForSeconds(Noradelay);

        GameMaster.Instance.DialogueManager.NewDialogue(followUpDialogue, 4);
        
    }

}

#if UNITY_EDITOR


[CustomPropertyDrawer(typeof(DialogueBeef))]
public class ContactDrawerDialogueBeef : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw the enum dropdown field
        property.enumValueIndex = EditorGUI.Popup(position, label.text, property.enumValueIndex, property.enumDisplayNames);

        EditorGUI.EndProperty();
    }
}
#endif