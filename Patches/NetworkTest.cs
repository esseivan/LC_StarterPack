using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using Unity.Netcode;

namespace MyFirstMod.Patches;

[HarmonyPatch]
internal class NetworkTest
{
    [HarmonyPostfix, HarmonyPatch(typeof(RoundManager), nameof(RoundManager.GenerateNewFloor))]
    static void SubscribeToHandler()
    {
        ExampleNetworkHandler.LevelEvent += ReceivedEventFromServer;
    }

    [
        HarmonyPostfix,
        HarmonyPatch(typeof(RoundManager), nameof(RoundManager.DespawnPropsAtEndOfRound))
    ]
    static void UnsubscribeFromHandler()
    {
        ExampleNetworkHandler.LevelEvent -= ReceivedEventFromServer;
    }

    static void ReceivedEventFromServer(string eventName)
    {
        // Event Code Here
        MyFirstMod.Logger.LogError("Successfully received RPC");
    }

    public static void SendEventToClients(string eventName)
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
            return;

        MyFirstMod.Logger.LogError("Sending RPC...");
        ExampleNetworkHandler.Instance.EventClientRpc(eventName);
    }
}
