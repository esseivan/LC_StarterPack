/*
 * Copyright (c) 2025 Esseiva Nicolas
 *
 * Author: Esseiva Nicolas
 * Date  : 06.2025
 *
 * Description:
 * Patch to auto-unlock teleporter items when starting a new game.
 * 
 * Source : 
 * This work uses code from this repository, under the MIT license.
 * https://github.com/Lomeli12/StartingTeleporter
 */

using EasyTextEffects.Editor.MyBoxCopy.Extensions;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Video;

namespace MyFirstMod.Patches;

/// <summary>
/// Patch to automatically unlock teleporter items on new game start
/// </summary>
[HarmonyPatch(typeof(PlayerControllerB))]
public class StartWithExtras
{
    private static readonly string TELEPORTER_NAME = "Teleporter";
    private static readonly string INVERSE_NAME = "Inverse Teleporter";
    private static int teleporterID = -1;
    private static int inverseTeleporterID = -1;
    private static bool idsInitialized;
    private static bool stLoaded;

    /// <summary>
    /// Find and cache teleporter item IDs from the unlockables list
    /// </summary>
    private static void getTeleporterIDs(StartOfRound instance)
    {
        MyFirstMod.Logger.LogInfo("Getting item IDs");
        for (var i = 0; i < instance.unlockablesList.unlockables.Count; i++)
        {
            var unlockableItem = instance.unlockablesList.unlockables[i];
            // Using localized names - not ideal but necessary
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

    /// <summary>
    /// Called when player connects - unlocks teleporter on new games
    /// </summary>
    [HarmonyPatch(nameof(PlayerControllerB.ConnectClientToPlayerObject))]
    [HarmonyPostfix]
    private static void ConnectClientToPlayerObjectPatch(PlayerControllerB __instance)
    {
        StartOfRound startOfRound = StartOfRound.Instance;
        MyFirstMod.Logger.LogDebug("ConnectClientToPlayerObjectPatch() called");
        
        MyFirstMod.Logger.LogDebug(
            $"daysPlayersSurvivedInARow={startOfRound.daysPlayersSurvivedInARow}"
        );
        
        // Only unlock on fresh games (0 days survived)
        if (startOfRound.daysPlayersSurvivedInARow == 0)
        {
            MyFirstMod.Logger.LogDebug("New game detected - Auto-unlocking items");
            
            // Only host/server can unlock items
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