using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    public DirectedRoutines selectedRoutine = DirectedRoutines.PrefaceNoraFired;

    [Header("Input")]
    public InputActionReference advanceAction;

    private bool _advancePressed;
    
    private void Start()
    {
        DirectorEvents.OnStartDirector += EventStartDirector;
    }

    private void OnEnable()
    {
        if (advanceAction != null)
        {
            advanceAction.action.performed += OnAdvance;
        }
    }

    private void OnDisable()
    {
        if (advanceAction != null)
        {
            advanceAction.action.performed -= OnAdvance;
        }
    }

    private void OnAdvance(InputAction.CallbackContext ctx)
    {
        _advancePressed = true;
    }
    

    public void EventStartDirector(DirectedRoutines directedRoutine)
    {
        selectedRoutine = directedRoutine;
        PlaySelectedRoutine();
    }

    public void PlaySelectedRoutine()
    {
        if (RoutinePlayerCo != null)
        {
            StopCoroutine(RoutinePlayerCo);
            RoutinePlayerCo = null;
        }
        
        switch (selectedRoutine)
        {
            case DirectedRoutines.PrefaceNoraFired:
                RoutinePlayerCo = StartCoroutine(RoutinePreface());
                break;

            case DirectedRoutines.MainNoraFired:
                RoutinePlayerCo = StartCoroutine(PlayMainRoutine());
                break;
        }
        
    }

    public void StopSelectedRoutine()
    {
        if (RoutinePlayerCo != null)
        {
            StopCoroutine(RoutinePlayerCo);
            RoutinePlayerCo = null;
        }
    }


    private IEnumerator RoutinePreface()
    {
        // Ellsworth "Just wanted to have a quick catch-up, take a wee seat there"

        GameMaster.Instance.DialogueManager.PlayDialogue(
            dialogueList[0],
            5,
            DialogueType.normal,
            cutsceneDuration: 4f,
            cutscenePanTime: 1f,
            cutsceneTarget: NPCTransforms[0],
            false);
        Debug.Log("start");

        yield return new WaitForSeconds(2f);
        
        DirectorEvents.UpperBodyAnimation(NPC.Ellsworth_Ohanlon, "DoPoint");
        
        yield return null;
    }


    


    private IEnumerator PlayMainRoutine()
    {
        GameMaster.Instance.PLAYERBUSY = true;
        GameMaster.Instance.INAMEETING = true;

        while (!thePlayer.IsSeated)
        {
            yield return null;
        }
        
        
        Player.Instance.FirstPersonLook.LookAtDevice(NPCTransforms[0].transform);
        

        while (GameMaster.Instance.DialogueManager.DialogInProgress)
        {
            yield return null;
        }
        
        GameMaster.Instance.DialogueManager.PlayDialogue(
            dialogueList[1],
            5,
            DialogueType.normal,
            cutsceneDuration: 4f,
            cutscenePanTime: 1f,
            cutsceneTarget: NPCTransforms[0],
            false);

        
        Player.Instance.FirstPersonLook.LookAtDevice(NPCTransforms[1].transform);
        
        yield return WaitForPlayerInput();
        
        
        Player.Instance.FirstPersonLook.LookAtDevice(NPCTransforms[0].transform);

        
        yield return new WaitForSeconds(1);
        
        GameMaster.Instance.INAMEETING = false;
        
        yield return null;
    }
    
    private IEnumerator WaitForPlayerInput()
    {
        _advancePressed = false;

        while (!_advancePressed)
        {
            yield return null;
        }
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


public enum DirectedRoutines
{
    PrefaceNoraFired = 0,
    MainNoraFired = 1,
}