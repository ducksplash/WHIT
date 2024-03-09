using System.Collections;
using UnityEditor;
using UnityEngine;



public class DialogueBeef : MonoBehaviour
{



    [Header("Dev Placeholder Cube")]
    public GameObject ColliderCube;

    [Header("Message from:")]
    [SerializeField]
    public Contacts ContactName;
    [Header("Message contents:")]
    [TextArea(5, 10)]
    public string MessageBody;

    [Header("Display time in seconds")]
    public float DisplayTimer = 5.0f;


    [Header("Nora Reply?")]
    public bool Noraply;

    [Header("Nora's Reply:")]
    [TextArea(5, 10)]
    public string NoraplyBody;

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


            var TheCallingObject = gameObject;
            forhowlong = DisplayTimer;

            DialogueManager.Instance.NewDialogue(ContactName.ToString(), MessageBody, DisplayTimer);
           

            if (Noraply)
            {

                StartCoroutine(Norasponse(other.gameObject));

            }
        }
    }




    public IEnumerator Norasponse(GameObject ThePlayer)
    {
        yield return new WaitForSeconds(Noradelay);

        DialogueManager.Instance.NewDialogue(Contacts.Nora.ToString(), NoraplyBody, 4);


    }



}

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