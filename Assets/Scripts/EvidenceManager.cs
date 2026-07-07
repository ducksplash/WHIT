using System;
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

    public bool EvidenceLoaded;
    
    private void Awake()
    {
        EventManager.OnPlayerDataLoaded += LoadExistingEvidence;
    }

    private void Start()
    {
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
                
                //Debug.Log("autocollect evidence.Key "+evidence.Key);
                EventManager.AutocollectEvidence(evidence.Key);
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
            EvidenceLoaded = true;
            
            GameMaster.Instance.NotifyEvidenceManagerReady();
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
                    //Debug.Log($"[EvidenceManager] Loaded evidence from disk: {evidenceName}");
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
        

        EvidenceLoaded = true;
        
        EventManager.EvidenceLoaded();
        GameMaster.Instance.NotifyEvidenceManagerReady();
        ApplyCollectedEvidence();
    }
    

    public void RecordEvidence(Camera cam, Evidence ev)
    {
        if (cam == null || ev == null)
            return;

        RenderTexture active = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;

        cam.Render();

        Texture2D image = new Texture2D(
            cam.targetTexture.width,
            cam.targetTexture.height,
            TextureFormat.RGB24,
            false
        );
        image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
        image.Apply();

        RenderTexture.active = active;

        byte[] bytes = image.EncodeToPNG();
        Destroy(image);

        string dcimDir = Path.Combine(Application.persistentDataPath, "Phone/0/DCIM");
        string evidenceDir = Path.Combine(Application.persistentDataPath, "Phone/0/Evidence");

        if (!Directory.Exists(dcimDir))
            Directory.CreateDirectory(dcimDir);
        if (!Directory.Exists(evidenceDir))
            Directory.CreateDirectory(evidenceDir);

        string photoFileName = ev.EvidenceName + ".png";
        string photoPath = Path.Combine(dcimDir, photoFileName);

        File.WriteAllBytes(photoPath, bytes);

        string evidencedate = DateTime.Now.ToString("dd/MM/yyyy, HH:mm");

        string quackFileName = ev.EvidenceName + ".quack";
        string quackPath = Path.Combine(evidenceDir, quackFileName);

        string slug = "";
        slug += ev.EvidenceName + "\n";
        slug += photoFileName + "\n";
        slug += evidencedate + "\n";
        slug += ev.EvidenceFake + "\n";
        slug += ev.EvidenceQuality + "\n";
        slug += ev.EvidenceDetails + "\n";

        File.WriteAllText(quackPath, slug);

        if (!EvidenceFound.ContainsKey(ev.EvidenceName)) EvidenceFound.Add(ev.EvidenceName, quackPath);

        ev.CollectEvidence();
    }
}

public enum EvidenceName
{
    // Evidence Nora's House
    Wine = 1000,

    // Evidence Tawley Meats
    Phone = 2001,
    Skull = 2002,
    Blood = 2003,
    Emails = 2004,
    Terminal = 2005

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
