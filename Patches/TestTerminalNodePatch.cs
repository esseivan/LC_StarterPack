using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace StarterPack.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    internal class TestTerminalNodePatch
    {
        [HarmonyPatch(nameof(Terminal.LoadNewNodeIfAffordable))]
        [HarmonyPrefix]
        public static bool LoadNewNodeIfAffordablePatch(TerminalNode node, Terminal __instance)
        {
            StarterPack.Logger.LogDebug($"Terminal node found : {node.name}");
            StarterPack.Logger.LogDebug(node.displayText);
            StarterPack.Logger.LogDebug(node.ToString());
            StarterPack.Logger.LogDebug(node);
            StarterPack.Logger.LogDebug(node.itemCost);
            StarterPack.Logger.LogDebug(node.buyItemIndex);

            return true;
        }
    }
}
