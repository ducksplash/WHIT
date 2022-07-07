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


    public void CollectEvidence()
     {


        if (!GameMaster.EvidenceFound.ContainsKey(gameObject.name))
        {



            var filepath = Application.persistentDataPath + "/Phone/0/Evidence/";

            var evidencedate = System.DateTime.Now.ToString("dd/MM/yyyy, HH:mm");



            if (gameObject.layer == 5)
            {
                Debug.Log("Digital Evidence");
            }



            DirectoryInfo di = Directory.CreateDirectory(filepath);


            var EvidenceFilename = transform.name + ".quack";
            var EvidencePhotoFilename = transform.name + ".png";
            var EvidenceSlug = EvidenceName + "\n";
            EvidenceSlug += EvidencePhotoFilename + "\n";
            EvidenceSlug += evidencedate + "\n";
            EvidenceSlug += EvidenceFake + "\n";
            EvidenceSlug += EvidenceQuality + "\n";
            EvidenceSlug += EvidenceDetails + "\n";



            System.IO.File.WriteAllText(filepath + EvidenceFilename, EvidenceSlug);

            GameMaster.EvidenceFound.Add(transform.name, filepath);

            gameObject.GetComponent<Evidence>().PhotographableEvidence = false;
            gameObject.GetComponent<Evidence>().EvidenceCollected = true;

        }
        else
        {
            gameObject.GetComponent<Evidence>().PhotographableEvidence = false;
            gameObject.GetComponent<Evidence>().EvidenceCollected = true;
        }




    }




}
