using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class EvidenceBar : MonoBehaviour
{

    public TextMeshProUGUI EVThresholdFG;
    public TextMeshProUGUI EVThresholdBG;

    public Image FillColour;

    public Slider EQ_Slider;


    public static TextMeshProUGUI EVThresholdBGS;
    public static TextMeshProUGUI EVThresholdFGS;
    public static Image FillColourS;
    public static Slider EQ_SliderS;


    private void Start()
    {



        EVThresholdBGS = EVThresholdBG;
        EVThresholdFGS = EVThresholdFG;
        FillColourS = FillColour;
        EQ_SliderS = EQ_Slider;






        EVThresholdBG.text = "Nothing found yet";
        EVThresholdFG.text = "Nothing found yet";


    }


    private void Update()
    {

 
          //  EQReadout();
        


    }




    public static void EQReadout()
    {

        var ThisLevelsEQ = PlayerPrefs.GetInt("EQLevel" + GameMaster.THISLEVEL);


        int EQPercent = Convert.ToInt32(Math.Round(((decimal)ThisLevelsEQ / GameMaster.ExpectedEQThisLevel) * 100, 0));


        if (EQPercent > 0 && EQPercent < 25)
        {

            FillColourS.color = Color.white;

            EVThresholdBGS.text = "Not much to go on";
            EVThresholdFGS.text = "Not much to go on";
        }



        if (EQPercent > 25 && EQPercent < 50)
        {



            FillColourS.color = new Color(1, 0.5f, 0, 1);

            EVThresholdBGS.text = "Still need more";
            EVThresholdFGS.text = "Still need more";


        }


        if (EQPercent > 50 && EQPercent < 75)
        {

            FillColourS.color = new Color(1, 1, 0, 1);

            EVThresholdBGS.text = "Need a little more";
            EVThresholdFGS.text = "Need a little more";


        }


        if (EQPercent > 75 && EQPercent < 100)
        {


            EVThresholdBGS.text = "Enough for a story";
            EVThresholdFGS.text = "Enough for a story";

            FillColourS.color = Color.green;



        }


        if (EQPercent >= 100)
        {

            EVThresholdBGS.text = "100%";
            EVThresholdFGS.text = "100%";

            FillColourS.color = new Color(0, 0.5f, 0, 1);



        }





        EQ_SliderS.value = EQPercent;







    }



}
