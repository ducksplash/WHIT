using UnityEngine;
using TMPro;
using System;

public class clock : MonoBehaviour
{
    public TextMeshProUGUI[] timereadout;

    public bool JustTheYear;
    public bool NewspaperFormat;


    private static TimeZoneInfo GetUkTimeZone()
    {
        // Windows uses Windows IDs.
        // Linux/macOS use IANA IDs.
        string id = Application.platform == RuntimePlatform.WindowsPlayer ||
                    Application.platform == RuntimePlatform.WindowsEditor
            ? "GMT Standard Time"
            : "Europe/London";

        return TimeZoneInfo.FindSystemTimeZoneById(id);
    }

    void LateUpdate()
    {
        DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GameMaster.Instance.GameTimeZone);

        string anHour = now.Hour.ToString("00");
        string aMinute = now.Minute.ToString("00");

        foreach (TextMeshProUGUI timetext in timereadout)
        {
            if (JustTheYear)
            {
                timetext.text = now.ToString("yyyy");
            }
            else if (NewspaperFormat)
            {
                timetext.text =
                    $"{now:dddd}, {now:MMMM d}{MonthDay(now.Day.ToString())}, {now:yyyy}";
            }
            else
            {
                timetext.text = $"{anHour}:{aMinute}";
            }
        }
    }

    public string MonthDay(string day)
    {
        string suffix = "th";
        int dayNum = int.Parse(day);

        if (dayNum < 11 || dayNum > 20)
        {
            switch (dayNum % 10)
            {
                case 1: suffix = "st"; break;
                case 2: suffix = "nd"; break;
                case 3: suffix = "rd"; break;
            }
        }

        return suffix;
    }
}