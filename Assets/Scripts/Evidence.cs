using UnityEngine;
using System.IO;

public class Evidence : MonoBehaviour
{
    public bool EvidenceCollected;
    public bool EvidenceFake;
    public bool PlayerSeen;
    public bool PhotographableEvidence;


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








    public void CollectEvidence()
     {


        var filepath = Application.persistentDataPath + "/Phone/0/Evidence/";

        var evidencedate = System.DateTime.Now.ToString("HH:mm on dd/MM/yyyy");


        DirectoryInfo di = Directory.CreateDirectory(filepath);


        var EvidenceFilename = transform.name + ".quack";
        var EvidenceSlug = "Name: " + EvidenceName + "\n";
        EvidenceSlug += "Details: " + EvidenceBody + "\n";
        EvidenceSlug += "Date Collected: " + evidencedate + "\n";



        System.IO.File.WriteAllText(filepath + EvidenceFilename, EvidenceSlug);



        System.IO.FileInfo file = new System.IO.FileInfo(filepath);


    }

    /* public static CharacterData LoadData()
     {
         string path = Application.persistentDataPath + "/Game.weeklyhow";

         if(File.Exists(path))
         {
             BinaryFormatter formatter = new BinaryFormatter();
             FileStream stream = new FileStream(path, FileMode.Open);

             CharacterData data = formatter.Deserialize(stream) as CharacterData;

             stream.Close();

             return data;
         } else
         {
             Debug.LogError("Error: Save file not found in " + path);
             return null;
         }
     }
    */


}
