using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameMaster : MonoBehaviour
{
  GameData saveData = new GameData();


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

        Scene ThisScene = SceneManager.GetActiveScene();


        THISLEVEL = ThisScene.name;

        WaitingForTrinity = true;


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