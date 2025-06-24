using EasyTextEffects.Editor.MyBoxCopy.Extensions;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Video;

namespace StarterPack.Patches;

[HarmonyPatch(typeof(ShipLights))]
public class LightPatch
{
    [HarmonyPatch(nameof(ShipLights.ToggleShipLights))]
    [HarmonyPostfix]
    private static void ToggleShipLightsPostfix(ShipLights __instance)
    {
        bool isOn = __instance.areLightsOn;

        StarterPack.Logger.LogDebug("ToggleShipLightsPostfix() called");
        StarterPack.Logger.LogDebug($"Lights are {isOn}");
        ManualCameraRenderer obj = UnityEngine.Object.FindObjectOfType<ManualCameraRenderer>();

        Terminal terminal = UnityEngine.Object.FindAnyObjectByType<Terminal>();

        if ((NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
        {
            int credits = 500;
            terminal.useCreditsCooldown = true;
            terminal.groupCredits = credits;
            terminal.SyncGroupCreditsServerRpc(
                terminal.groupCredits,
                terminal.numberOfItemsInDropship
            );
        }

        NetworkTest.SendEventToClients("light");

        if (obj is not null && obj.HasMethod("SwitchScreenOn"))
        {
            StarterPack.Logger.LogDebug($"TVon is {obj.isScreenOn}");
            //obj.SwitchScreenOn(isOn);
            //obj.syncingSwitchScreen = true;
            //obj.SwitchScreenOnServerRpc(isOn);
        }
        else
        {
            StarterPack.Logger.LogDebug(
                $"Could not find 'ManualCameraRenderer' : {obj?.ToString()}"
            );
        }
    }
}
