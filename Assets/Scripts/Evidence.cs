using UnityEngine;
using System.IO;

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


    public void CollectEvidence(string evfilename)
     {


        var filepath = Application.persistentDataPath + "/Phone/0/Evidence/";

        var evidencedate = System.DateTime.Now.ToString("dd/MM/yyyy, HH:mm");


        if (gameObject.layer == 13)
        {
            Debug.Log("Physical Evidence");
        }
        if (gameObject.layer == 5)
        {
            Debug.Log("Digital Evidence");
        }



        DirectoryInfo di = Directory.CreateDirectory(filepath);


        var EvidenceFilename = transform.name + ".quack";
        var EvidenceSlug = EvidenceName + "\n";
        EvidenceSlug += evfilename + "\n";
        EvidenceSlug += evidencedate + "\n";
        EvidenceSlug += EvidenceDetails + "\n";



        System.IO.File.WriteAllText(filepath + EvidenceFilename, EvidenceSlug);



        System.IO.FileInfo file = new System.IO.FileInfo(filepath);


    }




}
