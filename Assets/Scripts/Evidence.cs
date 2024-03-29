﻿using UnityEngine;
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


    public void CollectEvidence(GameObject Player)
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



            // if evidence definitely collected, increment Evidence Quotient

            GameMaster.EQThisLevel += EvidenceQuality;
            PlayerPrefs.SetInt("EQLevel" + GameMaster.THISLEVEL, GameMaster.EQThisLevel);
            EvidenceBar.EQReadout();

            // output notification to player
            GiveFeedback(EvidenceName, Player);

        }
        else
        {
            // if the evidence has already been found before, silence it.
            gameObject.GetComponent<Evidence>().PhotographableEvidence = false;
            gameObject.GetComponent<Evidence>().EvidenceCollected = true;
        }
    }


    // todo: dialogue database 

    public void GiveFeedback(string EvidenceName, GameObject Player)
    {

        if (EvidenceName.Contains("Blood"))
        {
            var msg = "Who's blood is this...?";
            DialogueManager.Instance.NewDialogue(Contacts.Nora.ToString(), msg, 5);
        }

        if (EvidenceName.Contains("Email"))
        {
            var msg = "Is he talking about Eimear??";
            DialogueManager.Instance.NewDialogue(Contacts.Nora.ToString(), msg, 5);
        }


        if (EvidenceName.Contains("Skull"))
        {
            var msg = "Oh my... This one looks real...";
            DialogueManager.Instance.NewDialogue(Contacts.Nora.ToString(), msg, 5);
        }
    }


}
