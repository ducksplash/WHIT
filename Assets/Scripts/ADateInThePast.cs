using TMPro;
using UnityEngine;

public class ADateInThePast : MonoBehaviour
{
    public int DaysInThePast = 3;
    public int MonthsInThePast = 2;
    public int YearsInThePast = 0;
    public TextMeshProUGUI datestring;

    private void Awake()
        {

        datestring = gameObject.GetComponent<TextMeshProUGUI>();

        int daynum = int.Parse(System.DateTime.Now.ToString("dd"));
        int monthnum = int.Parse(System.DateTime.Now.ToString("MM"));
        int yearnum = int.Parse(System.DateTime.Now.ToString("yyyy"));

        daynum -= DaysInThePast;
        if (daynum < 1) daynum = 1;

        monthnum -= MonthsInThePast;
        if (monthnum < 1) monthnum = 1;

        yearnum -= YearsInThePast;
        if (yearnum < 2010) yearnum = 2010;

        var DateInPast = daynum + "/" + monthnum + "/" + yearnum;

        Debug.Log(DateInPast);

        datestring.text = DateInPast;


    }

}