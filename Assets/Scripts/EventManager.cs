using System;
using UnityEngine;

public static class EventManager
{    
    
    public static event Action OnGameStarted = () => { };
    public static event Action OnLevelLoaded = () => { };
    public static event Action OnTorchCollected = () => { };
    public static event Action OnPhoneCollected = () => { };
    public static event Action OnNotepadCollected = () => { };
    public static event Action OnPlayerDataLoaded = () => { };
    public static event Action OnEvidenceLoaded = () => { };
    public static event Action OnEvidenceCollected = () => { };
    public static event Action OnPhoneOpened = () => { };
    public static event Action<Transform> OnStartComputer = (ComputerTransform) => { };
    public static event Action<Transform> OnStartPhone = (PhoneTransform) => { };
    public static event Action<Transform> OnStartNotepad = (NorepadTransform) => { };
    public static event Action OnStopComputer = () => { };
    public static event Action OnStopPhone = () => { };
    public static event Action OnCameraOpen = () => { };
    public static event Action OnCameraClosed = () => { };
    public static event Action<EvidenceName> OnAutoCollectEvidence = (EvidenceName) => { };
    public static event Action<bool> OnPaused = (GamePaused) => { };
    public static event Action<QuestText> OnQuestLoaded = (QuestText) => { };
    public static event Action<bool> OnDebugCameraToggle = (DebugCamEnabled) => { };
    
    
    public static event Action<NPCController> OnRegisterNPC = (NPCCont) => { };
    
    
    public static event Action<GameObject> OnRegisterNotepad = (NotepadObj) => { };
    public static event Action<GameObject> OnRegisterTorch = (TorchObj) => { };
    public static event Action<GameObject> OnRegisterPhone = (PhoneObj) => { };
    public static event Action OnUpdateCorkboard = () => { };
    public static event Action<Seat> OnNoraSit = (thisSeat) => { };
    
    
    
    //public static event Action OnLightSwitchClick = () => { };
    //public static event Action<bool> OnBoolToggled = (bool) => { };
    
    
    
    public static void GameStartedEvent()
    {
        OnGameStarted.Invoke();
    }
    
    public static void LevelLoaded()
    {
        OnLevelLoaded.Invoke();
    }
    
    public static void TorchCollectedEvent()
    {
        //Debug.Log("TorchCollectedEvent");
        OnTorchCollected.Invoke();
    }
    
    public static void NotepadCollectedEvent()
    {
        //Debug.Log("NotepadCollected");
        OnNotepadCollected.Invoke();
    }
    public static void PhoneCollectedEvent()
    {
        //Debug.Log("PhoneCollectedEvent");
        OnPhoneCollected.Invoke();
    }
    
    public static void GamePaused(bool GamePaused)
    {
        //Debug.Log("OnPaused");
        OnPaused.Invoke(GamePaused);
    }
    public static void PlayerDataLoaded()
    {
        ////Debug.Log("PlayerDataLoaded");
        OnPlayerDataLoaded.Invoke();
    }
    public static void PhoneOpened()
    {
        //Debug.Log("PhoneOpened");
        OnPhoneOpened.Invoke();
    }
    public static void EvidenceLoaded()
    {
        //Debug.Log("EvidenceLoaded");
        OnEvidenceLoaded.Invoke();
    }
    public static void EvidenceCollected()
    {
        OnEvidenceCollected.Invoke();
    }
    public static void StartComputer(Transform pcTransform)
    {
        OnStartComputer.Invoke(pcTransform);
    }
    public static void StartPhone(Transform phoneTransform)
    {
        OnStartPhone.Invoke(phoneTransform);
    }
    public static void StartNotepad(Transform notepadTransform)
    {
        OnStartNotepad.Invoke(notepadTransform);
    }
    public static void AutocollectEvidence(EvidenceName evidenceName)
    {
        //Debug.Log("AutocollectEvidence");
        OnAutoCollectEvidence.Invoke(evidenceName);
    }
    public static void StopComputer()
    {
        OnStopComputer.Invoke();
    }
    public static void StopPhone()
    {
        OnStopPhone.Invoke();
    }
    public static void CameraOpen()
    {
        OnCameraOpen.Invoke();
    }
    public static void CameraClosed()
    {
        OnCameraClosed.Invoke();
    }
    public static void QuestLoaded(QuestText QuestText)
    {
        OnQuestLoaded.Invoke(QuestText);
    }

    public static void DebugCamEnabled(bool debugCamOn)
    {
        OnDebugCameraToggle.Invoke(debugCamOn);
    }

    public static void RegisterNPC(NPCController thisNPC)
    {
        Debug.Log("Register "+thisNPC.thisNPC);
        OnRegisterNPC.Invoke(thisNPC);
    }


    public static void RegisterNotepad(GameObject thisNotepad)
    {
        Debug.Log("Register "+thisNotepad.transform.name);
        OnRegisterNotepad.Invoke(thisNotepad);
    }


    public static void RegisterTorch(GameObject thisTorch)
    {
        Debug.Log("Register "+thisTorch.transform.name);
        OnRegisterTorch.Invoke(thisTorch);
    }


    public static void RegisterPhone(GameObject thisPhone)
    {
        Debug.Log("Register "+thisPhone.transform.name);
        OnRegisterPhone.Invoke(thisPhone);
    }

    public static void UpdateCorkboard()
    {
        OnUpdateCorkboard.Invoke();
    }

    public static void NoraSit(Seat selectedSeat)
    {
        OnNoraSit.Invoke(selectedSeat);
    }

    
}

