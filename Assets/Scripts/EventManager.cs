using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{    
    public static event Action OnTorchCollected = () => { };
    public static event Action OnPhoneCollected = () => { };
    public static event Action OnNotepadCollected = () => { };
    public static event Action OnPlayerDataLoaded = () => { };
    public static event Action OnEvidenceLoaded = () => { };
    public static event Action OnEvidenceCollected = () => { };
    public static event Action OnPhoneOpened = () => { };
    public static event Action<EvidenceName> OnAutoCollectEvidence = (EvidenceName) => { };
    public static event Action<bool> OnPaused = (GamePaused) => { };
    
    //public static event Action OnLightSwitchClick = () => { };
    //public static event Action<bool> OnBoolToggled = (bool) => { };
    
    
    
    public void TorchCollectedEvent()
    {
        Debug.Log("TorchCollectedEvent");
        OnTorchCollected.Invoke();
    }
    
    public void NotepadCollectedEvent()
    {
        Debug.Log("NotepadCollectedEvent");
        OnPhoneCollected.Invoke();
    }
    
    public void PhoneCollectedEvent()
    {
        Debug.Log("PhoneCollectedEvent");
        OnNotepadCollected.Invoke();
    }
    
    public void GamePaused(bool GamePaused)
    {
        Debug.Log("OnPaused");
        OnPaused.Invoke(GamePaused);
    }
    public void PlayerDataLoaded()
    {
        Debug.Log("PlayerDataLoaded");
        OnPlayerDataLoaded.Invoke();
    }
    public void PhoneOpened()
    {
        Debug.Log("PhoneOpened");
        OnPhoneOpened.Invoke();
    }
    public void EvidenceLoaded()
    {
        Debug.Log("EvidenceLoaded");
        OnEvidenceLoaded.Invoke();
    }
    public void EvidenceCollected()
    {
        Debug.Log("EvidenceCollected");
        OnEvidenceCollected.Invoke();
    }
    public void AutocollectEvidence(EvidenceName evidenceName)
    {
        Debug.Log("AutocollectEvidence");
        OnAutoCollectEvidence.Invoke(evidenceName);
    }
    
}