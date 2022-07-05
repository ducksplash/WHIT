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


    // COLLECTED ITEMS
    public static bool TORCHCOLLECTED;
    public static bool NOTEPADCOLLECTED;
    public static bool PHONECOLLECTED;


    void Awake()
    {

        Scene ThisScene = SceneManager.GetActiveScene();


        THISLEVEL = ThisScene.name;




    }



void Start ()
    {

        Debug.Log(THISLEVEL);

        if (THISLEVEL != "NorasFlat")
        {
            POWER_SUPPLY_ENABLED = false;
        }
        else
        {
            POWER_SUPPLY_ENABLED = true;
        }

        // 1; Tawley Meats
        if (THISLEVEL == "1")
        {
            INCINERATOR_ENABLED = true;
        }



    }

}