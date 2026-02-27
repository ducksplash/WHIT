using System;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class GameLevel : MonoBehaviour
{
    public GAMELEVEL ThisGameLevel;


    private void Start()
    {
        EventManager.OnPlayerDataLoaded += SetLevel;
    }



    private void SetLevel()
    {
        GameMaster.Instance.THISLEVEL = ThisGameLevel;
    }
}
