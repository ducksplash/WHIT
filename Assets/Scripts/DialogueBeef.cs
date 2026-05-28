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


    [Header("Slide in the News Ticker?")]
    public bool slideInTicker;
    public bool slideOutTicker;

    [Header("Does Nora have any thoughts?")]
    public bool NoraThought;

    [Header("Nora's Dialogue:")]
    public ThoughtName followUpThought;

    [Header("Delay before Nora's thought?")]
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

            if (slideInTicker)
            {
                EventManager.SlideTickerIn();
            }


            GameMaster.Instance.DialogueManager.NewDialogue(selectedDialogue, DisplayTimer);
           
            if (NoraThought)
            {
                StartCoroutine(Norasponse());
            }
        }
    }

    
    public IEnumerator Norasponse()
    {
        yield return new WaitForSeconds(Noradelay);

        GameMaster.Instance.DialogueManager.PlayThought(followUpThought);
        
        
        
        if (slideOutTicker)
        {
            EventManager.SlideTickerOut();
        }
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