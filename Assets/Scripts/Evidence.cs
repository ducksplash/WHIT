using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class Evidence : MonoBehaviour
{


    [Header("Evidence Setup")]
    public EvidenceName EvidenceName;
    public string EvidenceDetails;

    [Header("Evidence Veracity")]
    public int EvidenceQuality;
    public bool EvidenceFake;


    [Header("Does player photograph it?")]
    public bool PhotographableEvidence;
    

    [Header("Dialogue to use")] 
    public DialogueName selectedDialogue;
    

    [Header("LevelFrom")]
    public string LevelFrom;


    [Header("Debug Stuff (ignore)")]
    public bool EvidenceCollected;
    public bool PlayerSeen;
    public Transform EvidenceTransform;
    public Rigidbody EvidenceRigidbody;
    public Renderer EvidenceRenderer;

    
    private void Start()
    {
        EventManager.OnAutoCollectEvidence += AutoCollectEvidence;
        EvidenceTransform = transform;
        EvidenceRigidbody = GetComponent<Rigidbody>();
        EvidenceRenderer = GetComponent<Renderer>();
    }

    public void CollectEvidence()
    {
        if (EvidenceCollected) return;

        PhotographableEvidence = false;
        EvidenceCollected = true;

        GameMaster.Instance.EvidenceManager.EQThisLevel += EvidenceQuality;
        StoredPrefs.Instance.SetInt("EQLevel" + GameMaster.Instance.THISLEVEL, GameMaster.Instance.EvidenceManager.EQThisLevel);
        StoredPrefs.Instance.Save();

        
        
        EvidenceBar.EQReadout();
        GiveFeedback();

        if (EvidenceName == GameMaster.Instance.OnboardingManager.FirstOnboardingEvidence)
        {
            GameMaster.Instance.OnboardingManager.CollectTestEvidence();
            
        }
        
    }

    public void AutoCollectEvidence(EvidenceName evidenceName)
    {
        Debug.Log("autocollect");
        if (EvidenceName != evidenceName) return;
        if (EvidenceCollected) return;

        Debug.Log("autocollect");
        
        PhotographableEvidence = false;
        EvidenceCollected = true;
        EvidenceBar.EQReadout();
        
        GameMaster.Instance.EventManager.EvidenceCollected();
        //GameMaster.Instance.EvidenceManager.EQThisLevel += EvidenceQuality;
        //StoredPrefs.Instance.SetInt("EQLevel" + GameMaster.Instance.THISLEVEL, GameMaster.Instance.EvidenceManager.EQThisLevel);
        //StoredPrefs.Instance.Save();
        //GiveFeedback();
    }



    // todo: dialogue database 

    public void GiveFeedback()
    {
        if (selectedDialogue == DialogueName.None) return;
        GameMaster.Instance.DialogueManager.NewDialogue(selectedDialogue, 5);
    }


}
