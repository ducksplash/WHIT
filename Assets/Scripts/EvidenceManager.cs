using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class EvidenceManager : MonoBehaviour
{
    // Evidence Log
    // Key = EvidenceName, Value = full path to .quack file
    public Dictionary<EvidenceName, string> EvidenceFound = new Dictionary<EvidenceName, string>();

    public List<EvidenceName> NorasFlatEvidence = new List<EvidenceName>();
    public List<EvidenceName> TawleyMeatsEvidence = new List<EvidenceName>();
    public List<EvidenceName> RoarkEvidence = new List<EvidenceName>();
    
    private Coroutine DataLoadedCoroutine;
    
    

    // Evidence Quotient
    public int EQThisLevel;
    public int ExpectedEQThisLevel;

    public int ExpectedEQ_Level0 = 1;
    public int ExpectedEQ_Level1 = 18;
    public int ExpectedEQ_Level2 = 19;

    private void Awake()
    {
        EventManager.OnPlayerDataLoaded += LoadExistingEvidence;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        
        if (GameMaster.Instance.THISLEVEL == GAMELEVEL.TawleyMeats) ExpectedEQThisLevel = ExpectedEQ_Level1;
        else if (GameMaster.Instance.THISLEVEL == GAMELEVEL.RoarkInside) ExpectedEQThisLevel = ExpectedEQ_Level2;
        else ExpectedEQThisLevel = ExpectedEQ_Level0;
    }


    private void ApplyCollectedEvidence()
    {
        foreach (var evidence in EvidenceFound)
        {
            if (NorasFlatEvidence.Contains(evidence.Key))
            {
                
                Debug.Log("autocollect evidence.Key "+evidence.Key);
                GameMaster.Instance.EventManager.AutocollectEvidence(evidence.Key);
            }
        }

    }

    /// <summary>
    /// Loads existing evidence from the filesystem (NOT StoredPrefs)
    /// </summary>
    public void LoadExistingEvidence()
    {
        EvidenceFound.Clear();

        string evidenceDir = Path.Combine(
            Application.persistentDataPath,
            "Phone/0/Evidence"
        );

        if (!Directory.Exists(evidenceDir))
        {
            Debug.Log("[EvidenceManager] No evidence directory found.");
            return;
        }

        string[] files = Directory.GetFiles(evidenceDir, "*.quack");

        foreach (string filePath in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            // Filename MUST match enum name
            if (System.Enum.TryParse(fileName, true, out EvidenceName evidenceName))
            {
                if (!EvidenceFound.ContainsKey(evidenceName))
                {
                    EvidenceFound.Add(evidenceName, filePath);
                    Debug.Log($"[EvidenceManager] Loaded evidence from disk: {evidenceName}");
                }
            }
            else
            {
                Debug.LogWarning(
                    $"[EvidenceManager] Evidence file '{fileName}' does not map to EvidenceName enum."
                );
            }
        }

        if (DataLoadedCoroutine != null)
        {
            StopCoroutine(DataLoadedCoroutine);
            DataLoadedCoroutine = null;
        }

        DataLoadedCoroutine = StartCoroutine(DataLoadedCo());
    }

    private IEnumerator DataLoadedCo()
    {
        yield return new WaitForSeconds(1);

        Debug.Log($"[EvidenceManager] Init complete — Evidence count: {EvidenceFound.Count}");

        // only in nora's flat
        GameMaster.Instance.EventManager.EvidenceLoaded();
        
        // Ensure scene objects are updated AFTER load
        ApplyCollectedEvidence();
    }
    
    public static string EvidencePrefsKey(EvidenceName name)
    {
        return $"Evidence/{name}";
    }

}

public enum EvidenceName
{
    // Evidence Nora's House
    WineBottle = 1000,

    // Evidence Tawley Meats
    BrokenPhone = 2001,
    Skull = 2002,
    Blood = 2003,
    ManagersEmails = 2004,
    HighTechPanel = 2005

    // Evidence Roark (future)
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
