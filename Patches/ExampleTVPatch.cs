using HarmonyLib;
using UnityEngine;
using UnityEngine.Video;

namespace MyFirstMod.Patches;

[HarmonyPatch(typeof(TVScript))]
public class ExampleTVPatch
{
    [HarmonyPatch(nameof(TVScript.SwitchTVLocalClient))]
    [HarmonyPostfix]
    private static void SwitchTVPrefix(TVScript __instance)
    {
        MyFirstMod.Logger.LogDebug("SwitchTVPrefix() called");
        StartOfRound.Instance.shipRoomLights.SetShipLightsBoolean(__instance.tvOn);
    }

    [HarmonyPatch("TurnTVOnOff")]
    [HarmonyPrefix]
    public static bool TurnTVOnOff(bool on, TVScript __instance)
    {
        MyFirstMod.Logger.LogDebug("TurnTVOnOff() called");

        return false;
    }
}
