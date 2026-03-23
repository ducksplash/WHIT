using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class DirectorTrigger : MonoBehaviour
{
    
    [Header("Dev Placeholder Cube")]
    public GameObject ColliderCube;

    private bool triggerUsed;
    public DirectedRoutines selectedDirectedRoutine = DirectedRoutines.PrefaceNoraFired;

    void Start() 
    {
        ColliderCube.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (triggerUsed) return;
        triggerUsed = true;
        
        if (other.gameObject.layer != 3) return;

        // Disable immediately – stops any further calls this physics step
        gameObject.GetComponent<Collider>().enabled = false;
        

        DirectorEvents.StartDirector(selectedDirectedRoutine);
        
        gameObject.SetActive(false);
    }

    

}

#if UNITY_EDITOR


[CustomPropertyDrawer(typeof(DirectorTrigger))]
public class ContactDrawerDirectorTrigger: PropertyDrawer
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