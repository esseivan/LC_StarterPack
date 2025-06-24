/*
 * Copyright (c) 2025 Esseiva Nicolas
 *
 * Author: Esseiva Nicolas
 * Date  : 06.2025
 *
 * Description:
 * Patch to sync ship interior lights with monitor screen state.
 */

using HarmonyLib;
using UnityEngine;
using UnityEngine.Video;

namespace MyFirstMod.Patches;

/// <summary>
/// Patch to sync ship lights with monitor screen state
/// </summary>
[HarmonyPatch(typeof(ManualCameraRenderer))]
public class ExampleScreenPatch
{
    /// <summary>
    /// Called after SwitchScreenButton to sync lights with screen state
    /// </summary>
    [HarmonyPatch(nameof(ManualCameraRenderer.SwitchScreenButton))]
    [HarmonyPostfix]
    private static void SwitchScreenButton(ManualCameraRenderer __instance)
    {
        MyFirstMod.Logger.LogDebug("SwitchScreenButton() called");
        // Sync ship lights with screen on/off state
        StartOfRound.Instance.shipRoomLights.SetShipLightsBoolean(__instance.isScreenOn);
    }
}
