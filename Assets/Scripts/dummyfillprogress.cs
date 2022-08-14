using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class dummyfillprogress : MonoBehaviour
{
    public TextMeshProUGUI fillFG;
    public TextMeshProUGUI fillBG;
    public float fillLevel = 0;
    public bool IsFilling = true;

    public Image FillMilestone0;
    public Image FillMilestone1;
    public Image FillMilestone2;
    public Image FillMilestone3;

    public Image CurrentlyFillingImage;

    public bool lerpOn;

    public Color startColor = Color.black;
    public Color endColor = Color.yellow;

    public float currentLerpTime;
    public float lerpTime = 2f;

    private void Start()
    {
        fillLevel = 0;

        CurrentlyFillingImage = FillMilestone0;


        StartCoroutine(FillUp());


        // FillMilestone0.color = Color.green;
        //  FillMilestone1.color = Color.red;
        //  FillMilestone2.color = Color.red;
        //  FillMilestone3.color = Color.red;


        lerpOn = true;

        fillFG.color = Color.red;
    }






    private void Update()
    {
        if (IsFilling)
        {
            if (lerpOn)
            {
                //increment timer once per frame
                currentLerpTime += Time.deltaTime;
                if (currentLerpTime > lerpTime)
                {
                    currentLerpTime = lerpTime;

                }

                //lerp!
                float perc = currentLerpTime / lerpTime;
                CurrentlyFillingImage.color = Color.Lerp(startColor, endColor, perc);

                if (CurrentlyFillingImage.color == endColor)
                {
                    lerpOn = false;
                    currentLerpTime = 0;
                }

            }
            else
            {
                //increment timer once per frame
                currentLerpTime += Time.deltaTime;
                if (currentLerpTime > lerpTime)
                {
                    currentLerpTime = lerpTime;
                }

                //lerp!
                float perc = currentLerpTime / lerpTime;
                CurrentlyFillingImage.color = Color.Lerp(endColor, startColor, perc);

                if (CurrentlyFillingImage.color == startColor)
                {
                    lerpOn = true;
                    currentLerpTime = 0;
                }
            }
        }

    }


    public IEnumerator FillUp()
    {

        while (fillLevel < 100)
        {
            yield return new WaitForSeconds(0.2f);

            fillLevel = fillLevel + 0.1f;

            fillBG.text = "<mspace=1.5em>" + fillLevel.ToString("#0") + "%</mspace>";
            fillFG.text = "<mspace=1.5em>" + fillLevel.ToString("#0") + "%</mspace>";





            if (fillLevel > 0 && fillLevel < 25)
            {

                fillFG.color = Color.red;
                CurrentlyFillingImage = FillMilestone0;

            }



            if (fillLevel > 25 && fillLevel < 50)
            {




                fillFG.color = new Color(1,0.5f,0,1);

                FillMilestone0.color = Color.green;

                CurrentlyFillingImage = FillMilestone1;

            }


            if (fillLevel > 50 && fillLevel < 75)
            {


                fillFG.color = new Color(0, 1, 1, 1);


                FillMilestone1.color = Color.green;
                CurrentlyFillingImage = FillMilestone2;


            }


            if (fillLevel > 75 && fillLevel < 100)
            {


                fillFG.color = new Color(0, 0.5f, 0, 1);

                FillMilestone2.color = Color.green;
                CurrentlyFillingImage = FillMilestone3;


            }


            if (fillLevel >= 100)
            {


                FillMilestone3.color = Color.green;

                fillLevel = 100;
                IsFilling = false;

            }






        }
    }





}
