using HarmonyLib;
using UnityEngine;
using UnityEngine.Video;

namespace MyFirstMod.Patches;

[HarmonyPatch(typeof(ManualCameraRenderer))]
public class ExampleScreenPatch
{
    [HarmonyPatch(nameof(ManualCameraRenderer.SwitchScreenButton))]
    [HarmonyPostfix]
    private static void SwitchScreenButton(ManualCameraRenderer __instance)
    {
        MyFirstMod.Logger.LogDebug("SwitchScreenButton() called");
        StartOfRound.Instance.shipRoomLights.SetShipLightsBoolean(__instance.isScreenOn);
    }
}
