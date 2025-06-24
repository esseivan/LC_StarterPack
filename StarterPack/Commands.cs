using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StarterPack;

internal class Commands
{
    public static string DefaultCMD()
    {
        return "Not Yet Implemented";
    }

    public static string TestCMD()
    {
        Terminal t = UnityEngine.Object.FindObjectOfType<Terminal>();
        TerminalAccessibleObject[] objectsOfType =
            UnityEngine.Object.FindObjectsOfType<TerminalAccessibleObject>();

        StarterPack.Logger.LogInfo($"Found {objectsOfType.Length} objectsOfType");
        foreach (TerminalAccessibleObject obj in objectsOfType)
        {
            StarterPack.Logger.LogInfo($"{obj}");
            StarterPack.Logger.LogInfo($"\t{obj.terminalCodeEvent}");
            StarterPack.Logger.LogInfo($"\t{obj.transform.name}");
            StarterPack.Logger.LogInfo($"\t{obj.transform.parent.name}");
        }

        TerminalNode node_kit;

        return "Success\n";
    }

    public static string CheckTimeCMD()
    {
        float timeNormalized = TimeOfDay.Instance.normalizedTimeOfDay;
        int numberOfHours = TimeOfDay.Instance.numberOfHours;

        string amPM;

        string text;
        int num = (int)(timeNormalized * (60f * numberOfHours)) + 360;
        int num2 = (int)Mathf.Floor(num / 60);

        if (num2 >= 24)
        {
            text = "12:00 AM";
        }
        else
        {
            if (num2 < 12)
            {
                amPM = " AM";
            }
            else
            {
                amPM = " PM";
            }
            if (num2 > 12)
            {
                num2 %= 12;
            }
            int num3 = num % 60;
            text = $"{num2:00}:{num3:00}".TrimStart('0') + amPM;
        }

        return
            !StartOfRound.Instance.currentLevel.planetHasTime
            || !StartOfRound.Instance.shipDoorsEnabled
            ? "You're not on a moon. There is no time here.\n"
            : $"The time is currently {text}.\n";
    }
}
