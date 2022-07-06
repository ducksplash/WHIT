using UnityEngine;
using System.IO;

public class Evidence : MonoBehaviour
{
    public bool EvidenceCollected;
    public bool EvidenceFake;
    public bool PlayerSeen;
    public bool PhotographableEvidence;
    public int EvidenceQuality;


    public string EvidenceName;
    public string EvidenceBody;
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

        var evidencedate = System.DateTime.Now.ToString("HH:mm on dd/MM/yyyy");


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
        EvidenceSlug += EvidenceBody + "\n";
        EvidenceSlug += evidencedate + "\n";
        EvidenceSlug += evfilename + "\n";



        System.IO.File.WriteAllText(filepath + EvidenceFilename, EvidenceSlug);



        System.IO.FileInfo file = new System.IO.FileInfo(filepath);


    }




}
