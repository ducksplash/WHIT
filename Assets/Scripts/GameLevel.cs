using System;
using UnityEngine;

public class GameLevel : MonoBehaviour
{
    public GameMaster.GAMELEVEL ThisGameLevel;


    private void Awake()
    {
        GameMaster.Instance.THISLEVEL = ThisGameLevel;
    }
}
