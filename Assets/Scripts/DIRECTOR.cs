using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif



public class DIRECTOR : MonoBehaviour
{
    public Player thePlayer;
    public NPCManager npcManager;

    public List<GameObject> NPCTransforms = new List<GameObject>(); 
    public List<DialogueName> dialogueList = new List<DialogueName>();

    public Coroutine RoutinePlayerCo;
    
    
    public void PlaySelectedRoutine()
    {
        if (RoutinePlayerCo != null)
        {
            StopCoroutine(RoutinePlayerCo);
            RoutinePlayerCo = null;
        }
        
        
        RoutinePlayerCo = StartCoroutine(PlayRoutine());
    }

    public void StopSelectedRoutine()
    {
        if (RoutinePlayerCo != null)
        {
            StopCoroutine(RoutinePlayerCo);
            RoutinePlayerCo = null;
        }
    }

    

    private IEnumerator PlayRoutine()
    {
        GameMaster.Instance.DialogueManager.PlayDialogue(
            dialogueList[0],
            0,
            DialogueType.cutscene,
            cutsceneDuration: 7f,
            cutscenePanTime: 1f,
            cutsceneTarget: NPCTransforms[0],
            false);

        yield return null;
    }
    
}


#if UNITY_EDITOR
[CustomEditor(typeof(DIRECTOR))]
public class DIRECTOREDITOR : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DIRECTOR DIRECTOR = (DIRECTOR)target;

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Director Controls", EditorStyles.boldLabel);
        

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Play Routine")) DIRECTOR.PlaySelectedRoutine();
            if (GUILayout.Button("Stop Routine")) DIRECTOR.StopSelectedRoutine();
            EditorGUILayout.EndHorizontal();
        }
        
    }
}
#endif