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

    public TextMeshProUGUI EVThresholdFG;
    public TextMeshProUGUI EVThresholdBG;

    public Image FillColour;

    public Slider EQ_Slider;


    private void Start()
    {


        EVThresholdBG.text = "Nothing found yet";
        EVThresholdFG.text = "Nothing found yet";


    }


    private void Update()
    {

 
            EQReadout();
        


    }


    public void EQReadout()
    {

        int EQPercent = Convert.ToInt32(Math.Round(((decimal)GameMaster.EQThisLevel / GameMaster.ExpectedEQThisLevel) * 100, 0));


        if (EQPercent > 0 && EQPercent < 25)
        {

            FillColour.color = Color.white;

            EVThresholdBG.text = "Not much to go on";
            EVThresholdFG.text = "Not much to go on";
        }



        if (EQPercent > 25 && EQPercent < 50)
        {



            FillColour.color = new Color(1, 0.5f, 0, 1);

            EVThresholdBG.text = "Still need more";
            EVThresholdFG.text = "Still need more";


        }


        if (EQPercent > 50 && EQPercent < 75)
        {

            FillColour.color = new Color(1, 1, 0, 1);

            EVThresholdBG.text = "Need a little more";
            EVThresholdFG.text = "Need a little more";


        }


        if (EQPercent > 75 && EQPercent < 100)
        {


            EVThresholdBG.text = "Enough for a story";
            EVThresholdFG.text = "Enough for a story";

            FillColour.color = Color.green;



        }


        if (EQPercent >= 100)
        {

            EVThresholdBG.text = "100%";
            EVThresholdFG.text = "100%";

            FillColour.color = new Color(0, 0.5f, 0, 1);



        }





        EQ_Slider.value = EQPercent;







    }



}
