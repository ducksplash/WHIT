using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System;

public class EvidenceBar : MonoBehaviour
{

    public TextMeshProUGUI EQ_ReadoutFG;
    public TextMeshProUGUI EQ_ReadoutBG;

    public Slider EQ_Slider;


    private void Start()
    {


        EQ_ReadoutFG.text = GameMaster.EQThisLevel.ToString();
        EQ_ReadoutBG.text = GameMaster.EQThisLevel.ToString();


    }


    private void Update()
    {
        EQReadout();


    }


    public void EQReadout()
    {



        int EQPercent = Convert.ToInt32(Math.Round(((decimal)GameMaster.EQThisLevel / GameMaster.ExpectedEQThisLevel) * 100, 0));


        EQ_Slider.value = EQPercent;


        EQ_ReadoutFG.text = EQPercent.ToString();
        EQ_ReadoutBG.text = EQPercent.ToString();





    }



}
