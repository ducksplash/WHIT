using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;
using TMPro;


public class GameMaster : MonoBehaviour
{
  GameData saveData = new GameData();


    // Debuggery
    [Header("Debug Mode Toggle")]
    public bool DEBUGGERY;

    // Game Globals

    // Electricity Enabled - We can probably use this for all levels, resetting to false on scene change.
    public static bool POWER_SUPPLY_ENABLED;
    public static bool INCINERATOR_ENABLED;
    public static bool FROZEN;
    public static bool ONLADDER;
    public static bool INMENU;
    public static bool HASITEM;
    public static bool PHONEOUT;
    public static bool  ISWRITING;
    public static string THISLEVEL;
    public Scene ThisScene;
    public bool WaitingForTrinity;
    public CanvasGroup DevModeIcon;


    // COLLECTED ITEMS
    public static bool TORCHCOLLECTED;
    public static bool NOTEPADCOLLECTED;
    public static bool PHONECOLLECTED;

    public static bool TRINITY;

    public GameObject phonePickup;
    public GameObject notepadPickup;
    public GameObject torchPickup;

    public CanvasGroup phoneTick;
    public CanvasGroup notepadTick;
    public CanvasGroup torchTick;
    public CanvasGroup evidenceTick;

    public RawImage MyFirstEvidence;
    public TextMeshProUGUI EvidenceDesc;
    public static bool GarbageRun;

    public bool checkForEvidence;


    // Evidence Quotient

    public static int TotalEQ;


    public static int EQThisLevel;
    public static int ExpectedEQThisLevel;


    // EQ Expected
    public static int ExpectedEQ_Level0 = 1;
    public static int ExpectedEQ_Level1 = 18;
    public static int ExpectedEQ_Level2 = 19;



    // Evidence Expected




    // Have I Ever - done this? - We'll set up per-level persistence shortly.



    // Dialog log

    // The main purpose is to prevent duplicates.
    // A secondary use is within the phone, as a message log.
    // The main dictionary is split into NoraSpeak - The player dialogue, and 'Messages' (from others)
    // 

    public static Dictionary<string, string> DialogueSeen = new Dictionary<string, string>();

    // now again for cutscenes

    public static Dictionary<string, string> CutSceneSeen = new Dictionary<string, string>();

    // Evidence Log
    // Again, mainly preventing duplication
    // With the secondary use being in the phone again, as an "Evidence Log" in the "Gallery App"

    public static Dictionary<string, string> EvidenceFound = new Dictionary<string, string>();

    void Awake()
    {



        // Use this loop for debugging the dialog log
        //foreach (var Message in DialogueSeen)
        //{
        //    Debug.Log("Message: " + Message.Key + "\n" + "Sender: " + Message.Value);
        //}

        // Use this loop to pre-mark collected evidence (the primary use of the evidence log)
        // The Gallery usage is all handled in phone.cs

        foreach (var Evidence in EvidenceFound)
        {

            if (GameObject.Find(Evidence.Key))
            {
                var ThisEvidence = GameObject.Find(Evidence.Key);


                if (ThisEvidence.GetComponent<Evidence>() != null)
                {
                    ThisEvidence.GetComponent<Evidence>().EvidenceCollected = true;
                }
            }

        }


        // cleanup
        // We will use this on boot every time
        // We then set GarbageRun to 'true' to say that it is done, and not to do it again.
        //
        // We'll handle saves by just copying this and then optionally restoring after GC.
        //
        // FFR; Dir Tree is:
        // <persistent data path>/Phone/0/DCIM
        // and
        // <persistent data path>/Phone/0/Evidence
        // So we can treat /0/ as a defacto root here.

        if (!GarbageRun)
        {

            var filepath = Application.persistentDataPath + "/Phone/0/";

            if (Directory.Exists(filepath))
            {
                // deleting the foler also deletes the files.
                // handy.
                Directory.Delete(filepath, true);
            }

            // then just make the root folder back up; the phone will create the rest when needed.
            Directory.CreateDirectory(filepath);

            // Clear out Evidence Quotient Player Prefs

            PlayerPrefs.SetInt("EQLevelNorasFlat", GameMaster.EQThisLevel);
            PlayerPrefs.SetInt("EQLevel1", GameMaster.EQThisLevel);
            PlayerPrefs.SetInt("EQLevel2", GameMaster.EQThisLevel);

            GarbageRun = true;
        }






        Scene ThisScene = SceneManager.GetActiveScene();


        THISLEVEL = ThisScene.name;




        WaitingForTrinity = true;

        if (DEBUGGERY)
        {

            TORCHCOLLECTED = true;
            NOTEPADCOLLECTED = true;
            PHONECOLLECTED = true;
            DevModeIcon.alpha = 1;

        }


        if (THISLEVEL == "NorasFlat")
        {
            torchTick.alpha = 0;
            phoneTick.alpha = 0;
            notepadTick.alpha = 0;
            evidenceTick.alpha = 0;

        }

    }



void Start ()
    {


        Application.targetFrameRate = 60;


        if (THISLEVEL == "NorasFlat")
        {

            if (PHONECOLLECTED)
            {
                Destroy(phonePickup);
                phoneTick.alpha = 1;
            }

            if (TORCHCOLLECTED)
            {
                Destroy(torchPickup);
                torchTick.alpha = 1;
            }

            if (NOTEPADCOLLECTED)
            {
                Destroy(notepadPickup);
                notepadTick.alpha = 1;
            }

            if (EvidenceFound.Count > 0)
            {
                evidenceTick.alpha = 1;
            }

            ExpectedEQThisLevel = ExpectedEQ_Level0;

        }



        if (THISLEVEL == "1")
        {

            ExpectedEQThisLevel = ExpectedEQ_Level1;
            ExpectedEQThisLevel = ExpectedEQ_Level1;



        }



        if (THISLEVEL == "2")
        {

            ExpectedEQThisLevel = ExpectedEQ_Level2;
            ExpectedEQThisLevel = ExpectedEQ_Level2;



        }

        // 1; Tawley Meats
        //  if (THISLEVEL == "1")
        //  {
        //      INCINERATOR_ENABLED = true;
        //  }

    }




    private void Update()
    {
        if (THISLEVEL == "NorasFlat")
        {
            if (!TRINITY)
            {
                if (WaitingForTrinity)
                {
                    if (TORCHCOLLECTED && NOTEPADCOLLECTED && PHONECOLLECTED)
                    {
                        StartCoroutine(NoraReady());
                        WaitingForTrinity = false;
                    }
                }
            }

            if (EvidenceFound.Count > 0 && !checkForEvidence)
            {



                // lets get the files
                var filepath = Application.persistentDataPath + "/Phone/0/Evidence/";


                DirectoryInfo dir = new DirectoryInfo(filepath);
                if (dir.Exists)
                {
                    FileInfo[] info = dir.GetFiles("*.quack");
                    var lines = System.IO.File.ReadAllLines(info[0].FullName);


                    MemoryStream dest = new MemoryStream();

                    var photopath = Application.persistentDataPath + "/Phone/0/DCIM/";

                    byte[] imageData = File.ReadAllBytes(photopath + lines[1]);

                    //Create new Texture2D
                    Texture2D tempTexture = new Texture2D(100, 100);

                    //Load the Image Byte to Texture2D
                    tempTexture.LoadImage(imageData);


                    var finaltexture = tempTexture;

                    //Load to Rawmage?


                    MyFirstEvidence.GetComponent<RawImage>().texture = finaltexture;


                    EvidenceDesc.text = lines[5];

                    evidenceTick.alpha = 1;
                    checkForEvidence = true;
                }
            }
        }

    }


    IEnumerator NoraReady()
    {
        yield return new WaitForSeconds(5);

        var fakegameobject = new GameObject("FakeObject", typeof(BoxCollider));
        fakegameobject.GetComponent<Collider>().enabled = false;

        var msg = "Ok, I think I'm ready to go now.";

        gameObject.GetComponent<DialogueManager>().NewDialogue("NORA", msg, 5, fakegameobject);


    }

}