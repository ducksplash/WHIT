using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

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
    public static bool INMENU;
    public static bool HASITEM;
    public static bool PHONEOUT;
    public static string THISLEVEL;
    public Scene ThisScene;
    public bool WaitingForTrinity;

    // COLLECTED ITEMS
    public static bool TORCHCOLLECTED;
    public static bool NOTEPADCOLLECTED;
    public static bool PHONECOLLECTED;

    public static bool TRINITY;

    public GameObject phonePickup;
    public GameObject notepadPickup;
    public GameObject torchPickup;

    void Awake()
    {

        // cleanup
        // We will use this on boot every time
        // then copy a player's saved files back from the 'save' area.
        // we can just copy the whole data structure for ease. 

        // FFR; Dir Tree is:
        // <persistent data path>/Phone/0/DCIM
        // and
        // <persistent data path>/Phone/0/Evidence
        // So we can treat /0/ as a defacto root here.


        var filepath = Application.persistentDataPath + "/Phone/0/";

        if (Directory.Exists(filepath)) 
        { 
            // deleting the foler also deletes the files.
            // handy.
            Directory.Delete(filepath, true); 
        }

        // then just make the root folder back up; the phone will create the rest when needed.
        Directory.CreateDirectory(filepath);







        Scene ThisScene = SceneManager.GetActiveScene();


        THISLEVEL = ThisScene.name;

        WaitingForTrinity = true;

        if (DEBUGGERY)
        {

            TORCHCOLLECTED = true;
            NOTEPADCOLLECTED = true;
            PHONECOLLECTED = true;

        }

    }



void Start ()
    {






        if (THISLEVEL == "NorasFlat")
        {
            POWER_SUPPLY_ENABLED = true;

            if (PHONECOLLECTED)
            {
                Destroy(phonePickup);
            }

            if (TORCHCOLLECTED)
            {
                Destroy(torchPickup);
            }

            if (NOTEPADCOLLECTED)
            {
                Destroy(notepadPickup);
            }


        }
        else
        {
            POWER_SUPPLY_ENABLED = false;
        }

        // 1; Tawley Meats
        if (THISLEVEL == "1")
        {
            INCINERATOR_ENABLED = true;
        }

    }




    private void Update()
    {
        if (THISLEVEL == "NorasFlat")
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
    }


    IEnumerator NoraReady()
    {
        yield return new WaitForSeconds(5);

        var fakegameobject = new GameObject("FakeObject", typeof(BoxCollider));

        var msg = "Ok, I think I'm ready to go now.";

        gameObject.GetComponent<DialogueManager>().NewDialogue("NORA", msg, 5, fakegameobject);


    }

}