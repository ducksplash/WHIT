using System.Collections;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class cutscene : MonoBehaviour 
{
    // Variables to control the cutscene timing
    [Header("Time to rotate to face object")]
    public float panTime = 5.0f;     // How long it takes to pan to the object
    [Header("Time to linger looking at object")]
    public float duration = 10.0f;   // How long the cutscene lasts in total
    [Header("Time to zoom & unzoom; set zero to disable")]

    [Header("Optional Dialogue")]
    [SerializeField]
    public Contacts ContactName;

    public DialogueSelectorTemp selectedMessage;
    
    [Header("Object to zoom to")]
    public GameObject targetObject;
    
    [Header("Collider that triggers the cutscene")]
    public GameObject ColliderCube;
    
    

    void Start() 
    {
        ColliderCube.SetActive(false);
    }

    

    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.layer == 3) 
        {
            if (!GameMaster.CutSceneSeen.ContainsKey(selectedMessage.ToString()))
            {
                solo.Instance.CutsceneManager.cameraZoom.enabled = false;
                solo.Instance.CutsceneManager.elapsedCutsceneTime = 0.0f;
                
                // we don't want to await, that's why we're not awaiting. ignore this 'hint' 
                solo.Instance.CutsceneManager.CutsceneDialogue(duration, ContactName, selectedMessage);
                
                
                GameMaster.CutSceneSeen.TryAdd(selectedMessage.ToString(), "SYSTEM");
                
                
                StartCoroutine(solo.Instance.CutsceneManager.ExecuteCutscene(duration, panTime, targetObject));
                
            }
        }
    }
}



[CustomPropertyDrawer(typeof(cutscene))]
public class ContactDrawerCutscene : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw the enum dropdown field
        property.enumValueIndex = EditorGUI.Popup(position, label.text, property.enumValueIndex, property.enumDisplayNames);

        EditorGUI.EndProperty();
    }
}