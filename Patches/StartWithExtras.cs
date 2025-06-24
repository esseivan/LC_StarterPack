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
[HarmonyPatch(typeof(StartOfRound))]
public class StartWithExtras
{
    private static readonly string TELEPORTER_NAME = "Teleporter";
    private static readonly string INVERSE_NAME = "Inverse Teleporter";
    private static int teleporterID = -1;
    private static int inverseTeleporterID = -1;
    private static bool idsInitialized;
    private static bool stLoaded;

    private static bool isFirstDayAboutToStart = false;
    private static bool gameAlreadyReset = false;

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

    private static void setCredits(int credits)
    {
        Terminal terminal = UnityEngine.Object.FindAnyObjectByType<Terminal>();

        if ((NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
        {
            terminal.useCreditsCooldown = true;
            terminal.groupCredits = credits;
            terminal.SyncGroupCreditsServerRpc(
                terminal.groupCredits,
                terminal.numberOfItemsInDropship
            );
        }
    }

    private static void ApplyGifts()
    {
        MyFirstMod.Logger.LogDebug("Applying first day on the job gifts");

        StartOfRound startOfRound = StartOfRound.Instance;

        // Only host/server can unlock items
        if (startOfRound.NetworkManager.IsHost || startOfRound.NetworkManager.IsServer)
        {
            if (MyFirstMod.configFreeTeleporter.Value)
            {
                if (!idsInitialized)
                {
                    getTeleporterIDs(startOfRound);
                }
                MyFirstMod.Logger.LogDebug("Unlocking teleporter...");
                if (!startOfRound.unlockablesList.unlockables[teleporterID].hasBeenUnlockedByPlayer)
                {
                    startOfRound.BuyShipUnlockableServerRpc(teleporterID, -1);
                }
                else
                {
                    MyFirstMod.Logger.LogDebug("Teleporter already available...");
                }
            }
            if (MyFirstMod.configFreeInverseTeleporter.Value)
            {
                if (!idsInitialized)
                {
                    getTeleporterIDs(startOfRound);
                }
                MyFirstMod.Logger.LogDebug("Unlocking inverse teleporter...");
                if (
                    !startOfRound
                        .unlockablesList
                        .unlockables[inverseTeleporterID]
                        .hasBeenUnlockedByPlayer
                )
                {
                    startOfRound.BuyShipUnlockableServerRpc(inverseTeleporterID, -1);
                }
                else
                {
                    MyFirstMod.Logger.LogDebug("Inverse teleporter already available...");
                }
            }
            if (MyFirstMod.configStartWithExtraCredits.Value)
            {
                MyFirstMod.Logger.LogDebug(
                    $"Setting start credits to {MyFirstMod.configStartWithExtraCreditsValue.Value}"
                );
                setCredits(MyFirstMod.configStartWithExtraCreditsValue.Value);
            }
        }
    }

    /// <summary>
    /// Called when launching a save that has day 0 - unlocks teleporter on new games
    /// Will launch multiple times if the save if then relaunched without starting a day.
    /// </summary>
    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.PlayFirstDayShipAnimation))]
    [HarmonyPostfix]
    private static void PlayFirstDayShipAnimationPatch(StartOfRound __instance)
    {
        MyFirstMod.Logger.LogDebug("PlayFirstDayShipAnimationPatch() called");
        if (!gameAlreadyReset)
        {
            gameAlreadyReset = false;
            isFirstDayAboutToStart = true;
        }
    }

    /// <summary>
    /// Called when the game resets for a new game from day 0
    /// </summary>
    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.ResetShip))]
    [HarmonyPostfix]
    private static void ResetShipPatch(StartOfRound __instance)
    {
        MyFirstMod.Logger.LogDebug("ResetShipPatch() called");

        gameAlreadyReset = true; // Indicate that gifts are already applied

        ApplyGifts();
    }

    /// <summary>
    /// Called when player connects - unlocks teleporter on new games
    /// </summary>
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject))]
    [HarmonyPostfix]
    private static void ConnectClientToPlayerObjectPatch(StartOfRound __instance)
    {
        MyFirstMod.Logger.LogDebug("ConnectClientToPlayerObjectPatch() called");

        if (!isFirstDayAboutToStart)
            return;
        isFirstDayAboutToStart = false;

        ApplyGifts();
    }
}
