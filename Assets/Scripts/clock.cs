using UnityEngine;
using TMPro;
using System;

public class clock : MonoBehaviour
{
    public TextMeshProUGUI[] timereadout;

    public bool JustTheYear;
    public bool NewspaperFormat;

    // Change this to your real timezone
    private const string TimeZoneId = "GMT Standard Time";

    void LateUpdate()
    {
        // Always start from UTC
        DateTime utcNow = DateTime.UtcNow;

        // Convert to a known timezone (avoids Proton / Steam Deck offset bugs)
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        DateTime now = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);

        string anHour = now.Hour.ToString().PadLeft(2, '0');
        string aMinute = now.Minute.ToString().PadLeft(2, '0');

        foreach (TextMeshProUGUI timetext in timereadout)
        {
            if (JustTheYear)
            {
                timetext.text = now.ToString("yyyy");
            }
            else if (NewspaperFormat)
            {
                string buildDate = "";
                buildDate += now.ToString("dddd");
                buildDate += ", ";
                buildDate += now.ToString("MMMM d");
                buildDate += MonthDay(now.ToString("dd"));
                buildDate += ", ";
                buildDate += now.ToString("yyyy");

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
        int dayNum = int.Parse(day);

        if (dayNum < 11 || dayNum > 20)
        {
            switch (dayNum % 10)
            {
                case 1: nuNum = "st"; break;
                case 2: nuNum = "nd"; break;
                case 3: nuNum = "rd"; break;
            }
        }

        return nuNum;
    }
}