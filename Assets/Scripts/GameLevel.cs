using System;
using UnityEngine;

public class GameLevel : MonoBehaviour
{
    public GAMELEVEL ThisGameLevel;


    private void Awake()
    {
        GameMaster.Instance.THISLEVEL = ThisGameLevel;
    }
}
