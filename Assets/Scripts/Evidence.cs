using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class Evidence : MonoBehaviour
{


    [Header("Evidence Setup")]
    public string EvidenceName;
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
        EvidenceTransform = transform;
        EvidenceRigidbody = GetComponent<Rigidbody>();
        EvidenceRenderer = GetComponent<Renderer>();
    }

    public void CollectEvidence()
    {
        if (EvidenceCollected)
            return;

        PhotographableEvidence = false;
        EvidenceCollected = true;

        GameMaster.EQThisLevel += EvidenceQuality;
        StoredPrefs.SetInt("EQLevel" + GameMaster.Instance.THISLEVEL, GameMaster.EQThisLevel);
        StoredPrefs.Save();

        EvidenceBar.EQReadout();
        GiveFeedback();
    }



    // todo: dialogue database 

    public void GiveFeedback()
    {
        if (selectedDialogue == DialogueName.None) return;
        GameMaster.Instance.DialogueManager.NewDialogue(selectedDialogue, 5);
    }


}

[System.Serializable]
public class EvidenceRecord
{
    public string Name;
    public string Photo;
    public string DateFound;
    public bool IsFake;
    public int Quality;
    public string Details;
    public string Level;
}

