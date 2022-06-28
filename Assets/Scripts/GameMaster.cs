using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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




    void Awake()
{
	//Application.targetFrameRate = 61;
}



void Start ()
    {
        POWER_SUPPLY_ENABLED = false;
        INCINERATOR_ENABLED = true;
    }


  // Update is called once per frame
/*   void Update()
  {
    if(Input.GetKeyDown(KeyCode.UpArrow))
    {
      saveData.AddScore(1);
      PrintScore();
    }
    if (Input.GetKeyDown(KeyCode.DownArrow))
    {
      saveData.AddScore(-1);
      PrintScore();
    }
    if(Input.GetKeyDown(KeyCode.S))
    {
      SaveSystem.instance.SaveGame(saveData);
      Debug.Log("Saved data.");
    }
    if(Input.GetKeyDown(KeyCode.L))
    {
      saveData = SaveSystem.instance.LoadGame();
      Debug.Log("Loaded data.");
      PrintScore();
    }
    if(Input.GetKeyDown(KeyCode.X))
    {
      saveData.ResetData();
      PrintScore();
    }
  } */

/*   void PrintScore()
  {
    Debug.Log("The current score is " + saveData.score);
  } */
}