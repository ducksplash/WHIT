using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class clock : MonoBehaviour
{
    public TextMeshProUGUI[] timereadout;



    void LateUpdate()
    {



        DateTime nowDateTime = DateTime.Now;
        string anHour = nowDateTime.Hour.ToString().PadLeft(2, '0');
        string aMinute = nowDateTime.Minute.ToString().PadLeft(2, '0');

        foreach (TextMeshProUGUI timetext in timereadout)
        {
            timetext.text =  anHour + ":" + aMinute;
        }




        
    }
}
