using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif



public class ETV_DIRECTOR : MonoBehaviour
{
    public Player thePlayer;

    public GameObject Ellsworth;
    public GameObject Presha; 
    
    
    public Coroutine RoutinePlayerCo;

    public GameObject ThePapers;
    
    public DirectedRoutines selectedRoutine = DirectedRoutines.PrefaceNoraFired;

    [Header("Input")]
    public InputActionReference advanceAction;

    private bool _advancePressed;
    
    private void Start()
    {
        DirectorEvents.OnStartDirector += EventStartDirector;
        EventManager.OnDialogueCanProceed += OnReadyToAdvance;
    }



    private void OnReadyToAdvance(bool canadvance)
    {
        if (canadvance)
        {
            advanceAction.action.performed += OnAdvance;
        }
        else
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
            DialogueName.EllsworthNora,
            5,
            DialogueType.normal,
            cutsceneDuration: 4f,
            cutscenePanTime: 1f,
            cutsceneTarget: Ellsworth,
            false);
        
        Debug.Log("start");

        yield return new WaitForSeconds(2f);
        
        DirectorEvents.UpperBodyAnimation(NPC.Ellsworth_Ohanlon, "DoPoint");
        
        yield return null;
    }


    


    private IEnumerator PlayMainRoutine()
    {
        // SET BUSY
        GameMaster.Instance.PLAYERBUSY = true;
        GameMaster.Instance.INAMEETING = true;

        // WAIT TIL SEATED
        while (!thePlayer.IsSeated)
        {
            yield return null;
        }
        
        // LOOK AT ELLSWORTH
        Player.Instance.FirstPersonLook.LookAtDevice(Ellsworth.transform);
        

        // WAIT FOR HIS DIALOGUE TO END
        yield return WaitForDialogueToEnd();
        
        
        
        // HAVE YOU MET PRESHA? SHE'S HERE TO PUT YOU ON THE DOLE
        GameMaster.Instance.DialogueManager.PlayDialogue(
            DialogueName.EllsworthPreshaNora,
            0,
            DialogueType.normal,
            cutsceneDuration: 4f,
            cutscenePanTime: 1f,
            cutsceneTarget: Ellsworth,
            false,
            true);

        
        // LOOK AT PRESHA
        Player.Instance.FirstPersonLook.LookAtDevice(Presha.transform);
        
        
        // PRESHA GREETS YOU
        DirectorEvents.UpperBodyAnimation(NPC.Presha, "DoWave");
        
        
        // WAITING FOR YOU TO ADVANCE
        yield return WaitForPlayerInput();
        
        
        // LOOK BACK AT ELLSWORTH
        Player.Instance.FirstPersonLook.LookAtDevice(Ellsworth.transform);

        
        // NORA: SOMEONE FROM HR FOR A CATCH UP...
        GameMaster.Instance.DialogueManager.PlayDialogue(
            DialogueName.NoraPresha,
            0,
            DialogueType.normal,
            cutsceneDuration: 4f,
            cutscenePanTime: 1f,
            cutsceneTarget: Ellsworth,
            false,
            true);
        
        // WAITING FOR YOU TO ADVANCE
        yield return WaitForPlayerInput();
        
        // Ellsworth Does Whatever
        DirectorEvents.UpperBodyAnimation(NPC.Ellsworth_Ohanlon, "DoWhatever");
        
        
        // Ellsworth: No Easy way
        GameMaster.Instance.DialogueManager.PlayDialogue(
            DialogueName.EllsworthNoraNoEasyWay,
            0,
            DialogueType.normal,
            cutsceneDuration: 4f,
            cutscenePanTime: 1f,
            cutsceneTarget: Ellsworth,
            false,
            true);
        
        
        
        // WAITING FOR YOU TO ADVANCE
        yield return WaitForPlayerInput();
        
        Player.Instance.FirstPersonLook.LookAtDevice(Presha.transform);
        
        // Presha shoves papers at you
        DirectorEvents.UpperBodyAnimation(NPC.Presha, "DoSlidePapers");
        
        // non negotiable
        GameMaster.Instance.DialogueManager.PlayDialogue(
            DialogueName.PreshaNoraTheOffer,
            0,
            DialogueType.normal,
            cutsceneDuration: 4f,
            cutscenePanTime: 1f,
            cutsceneTarget: Presha,
            false,
            true);
        
        
        Player.Instance.FirstPersonLook.LookAtDevice(ThePapers.transform);
        
        DirectorEvents.SlidePapers();

        // WAITING FOR YOU TO ADVANCE
        yield return WaitForPlayerInput();
        
        
        yield return new WaitForSeconds(1);

        Player.Instance.FirstPersonLook.LookAtDevice(ThePapers.transform);
        
        // 
        GameMaster.Instance.DialogueManager.PlayDialogue(
            DialogueName.NoraPreshaNeverLikedYou,
            0,
            DialogueType.normal,
            cutsceneDuration: 4f,
            cutscenePanTime: 1f,
            cutsceneTarget: Presha,
            false,
            true);
        
        
        
        // WAITING FOR YOU TO ADVANCE
        yield return WaitForPlayerInput();
        
        
        
        
        
        
        
        
        
        // END THE MEETING.
        
        
        
        
        
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


    private IEnumerator WaitForDialogueToEnd()
    {
        while (GameMaster.Instance.DialogueManager.DialogInProgress)
        {
            yield return null;
        }
    }


    
}


#if UNITY_EDITOR
[CustomEditor(typeof(ETV_DIRECTOR))]
public class ETV_DIRECTOREDITOR : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ETV_DIRECTOR DIRECTOR = (ETV_DIRECTOR)target;

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