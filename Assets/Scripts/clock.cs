using UnityEngine;
using TMPro;
using System;

public class clock : MonoBehaviour
{
    public TextMeshProUGUI[] timereadout;

    public bool JustTheYear;
    public bool NewspaperFormat;

    void LateUpdate()
    {



        DateTime nowDateTime = DateTime.Now;
        string anHour = nowDateTime.Hour.ToString().PadLeft(2, '0');
        string aMinute = nowDateTime.Minute.ToString().PadLeft(2, '0');

        foreach (TextMeshProUGUI timetext in timereadout)
        {
            if (JustTheYear)
            {
                timetext.text = System.DateTime.Now.ToString("yyyy");
            }
            else if (NewspaperFormat)
            {



                var buildDate = "";

                buildDate += System.DateTime.Now.ToString("dddd");
                buildDate += ", ";
                buildDate += System.DateTime.Now.ToString("MMMM d");
                buildDate += MonthDay(System.DateTime.Now.ToString("dd").ToString());
                buildDate += ", ";
                buildDate += System.DateTime.Now.ToString("yyyy");


                timetext.text = buildDate;


            }
            else
            {

                timetext.text = anHour + ":" + aMinute;
            }
        }

    }


        public string MonthDay(string day)
        {
            string nuNum = "th";
            if (int.Parse(day) < 11 || int.Parse(day) > 20)
            {
                day = day.ToCharArray()[^1].ToString();
                switch (day)
                {
                    case "1":
                        nuNum = "st";
                        break;
                    case "2":
                        nuNum = "nd";
                        break;
                    case "3":
                        nuNum = "rd";
                        break;
                }
            }
            return nuNum;
        }


    
}
