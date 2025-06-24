using EasyTextEffects.Editor.MyBoxCopy.Extensions;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Video;

namespace MyFirstMod.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
public class FreeMoney
{
    private static readonly string TELEPORTER_NAME = "Teleporter";
    private static readonly string INVERSE_NAME = "Inverse Teleporter";
    private static int teleporterID = -1;
    private static int inverseTeleporterID = -1;

    private static bool idsInitialized;
    private static bool stLoaded;

    private static void getTeleporterIDs(StartOfRound instance)
    {
        MyFirstMod.Logger.LogInfo("Getting item IDs");
        for (var i = 0; i < instance.unlockablesList.unlockables.Count; i++)
        {
            var unlockableItem = instance.unlockablesList.unlockables[i];
            // I really hate this, cause it's a localized name. Hopefully this gets changed to proper item ids
            var itemName = unlockableItem.unlockableName.ToLower();

            if (itemName.Equals(TELEPORTER_NAME.ToLower()))
            {
                MyFirstMod.Logger.LogInfo($"{TELEPORTER_NAME} ID: {i}");
                teleporterID = i;
            }
            else if (itemName.Equals(INVERSE_NAME.ToLower()))
            {
                MyFirstMod.Logger.LogInfo($"{INVERSE_NAME} ID: {i}");
                inverseTeleporterID = i;
            }
        }

        idsInitialized = true;
    }

    [HarmonyPatch(nameof(PlayerControllerB.ConnectClientToPlayerObject))]
    [HarmonyPostfix]
    private static void ConnectClientToPlayerObjectPatch(PlayerControllerB __instance)
    {
        StartOfRound startOfRound = StartOfRound.Instance;

        MyFirstMod.Logger.LogDebug("ConnectClientToPlayerObjectPatch() called");
        //__instance.UnlockShipObject();

        MyFirstMod.Logger.LogDebug(
            $"daysPlayersSurvivedInARow={startOfRound.daysPlayersSurvivedInARow}"
        );

        if (startOfRound.daysPlayersSurvivedInARow == 0)
        {
            MyFirstMod.Logger.LogDebug("New game detected - Auto-unlocking items");

            if (startOfRound.NetworkManager.IsHost || startOfRound.NetworkManager.IsServer)
            {
                if (!idsInitialized)
                {
                    getTeleporterIDs(startOfRound);
                }

                MyFirstMod.Logger.LogDebug("Unlocking teleporter...");
                startOfRound.BuyShipUnlockableServerRpc(teleporterID, -1);
            }
        }
    }
}
