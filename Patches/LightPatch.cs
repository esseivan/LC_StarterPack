using EasyTextEffects.Editor.MyBoxCopy.Extensions;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Video;

namespace MyFirstMod.Patches;

[HarmonyPatch(typeof(ShipLights))]
public class LightPatch
{
    [HarmonyPatch(nameof(ShipLights.ToggleShipLights))]
    [HarmonyPostfix]
    private static void ToggleShipLightsPostfix(ShipLights __instance)
    {
        bool isOn = __instance.areLightsOn;

        MyFirstMod.Logger.LogDebug("ToggleShipLightsPostfix() called");
        MyFirstMod.Logger.LogDebug($"Lights are {isOn}");
        ManualCameraRenderer obj = UnityEngine.Object.FindObjectOfType<ManualCameraRenderer>();

        NetworkTest.SendEventToClients("light");

        if (obj is not null && obj.HasMethod("SwitchScreenOn"))
        {
            MyFirstMod.Logger.LogDebug($"TVon is {obj.isScreenOn}");
            //obj.SwitchScreenOn(isOn);
            //obj.syncingSwitchScreen = true;
            //obj.SwitchScreenOnServerRpc(isOn);
        }
        else
        {
            MyFirstMod.Logger.LogDebug(
                $"Could not find 'ManualCameraRenderer' : {obj?.ToString()}"
            );
        }
    }
}
