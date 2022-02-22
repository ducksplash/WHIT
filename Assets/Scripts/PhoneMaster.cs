using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class phoneMaster : MonoBehaviour
{
  phoneData saveData = new phoneData();

  // Update is called once per frame
  void Update()
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
      PhoneSaveSystem.instance.Savephone(saveData);
      Debug.Log("Saved data.");
    }
    if(Input.GetKeyDown(KeyCode.L))
    {
      saveData = PhoneSaveSystem.instance.Loadphone();
      Debug.Log("Loaded data.");
      PrintScore();
    }
    if(Input.GetKeyDown(KeyCode.X))
    {
      saveData.ResetData();
      PrintScore();
    }
  }

  void PrintScore()
  {
    Debug.Log("The current score is " + saveData.score);
  }
}