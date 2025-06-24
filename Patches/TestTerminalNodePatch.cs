using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;
using static ES3Spreadsheet;

namespace StarterPack.Patches
{
    [HarmonyPatch]
    internal class TestTerminalNodePatch
    {
        private static int myItemIndex = -1;

        public static TerminalKeyword CreateTerminalKeyword(
            string word,
            bool isVerb = false,
            CompatibleNoun[] compatibleNouns = null,
            TerminalNode specialKeywordResult = null,
            TerminalKeyword defaultVerb = null,
            bool accessTerminalObjects = false
        )
        {
            TerminalKeyword keyword = ScriptableObject.CreateInstance<TerminalKeyword>();
            keyword.name = word;
            keyword.word = word;
            keyword.isVerb = isVerb;
            keyword.compatibleNouns = compatibleNouns;
            keyword.specialKeywordResult = specialKeywordResult;
            keyword.defaultVerb = defaultVerb;
            keyword.accessTerminalObjects = accessTerminalObjects;
            return keyword;
        }

        [HarmonyPatch(
            typeof(PlayerControllerB),
            nameof(PlayerControllerB.ConnectClientToPlayerObject)
        )]
        [HarmonyPostfix]
        private static void ConnectClientToPlayerObjectPatch(StartOfRound __instance) { }

        [HarmonyPatch(typeof(Terminal), nameof(Terminal.Awake))]
        [HarmonyPrefix]
        public static bool TerminalAwakePatch(Terminal __instance)
        {
            Terminal terminal = UnityEngine.Object.FindAnyObjectByType<Terminal>();

            TerminalNode iTerminalNode = StarterPack.Assets.LoadAsset<TerminalNode>(
                "WI TerminalNode"
            );

            Item blank_item = new Item();
            blank_item.creditsWorth = 101;
            blank_item.itemName = "Starting Kit";

            var itemList = terminal.buyableItemsList.ToList();
            itemList.Add(blank_item);

            int j = 0;
            foreach (var item in itemList)
            {
                StarterPack.Logger.LogDebug(
                    $"index #{j++} - Item {item.itemName} (id={item.itemId}) cost {item.creditsWorth}"
                );
            }

            terminal.buyableItemsList = itemList.ToArray();

            var buyKeyword = terminal.terminalNodes.allKeywords.First(keyword =>
                keyword.word == "buy"
            );

            // Get confirm node
            var confirmNode = iTerminalNode.terminalOptions[0].result;
            var confirmKeyword = iTerminalNode.terminalOptions[0].noun;
            var denyNode = iTerminalNode.terminalOptions[1].result;
            var denyKeyword = iTerminalNode.terminalOptions[1].noun;

            myItemIndex = itemList.Count - 1;

            iTerminalNode.buyItemIndex = myItemIndex;
            confirmNode.buyItemIndex = myItemIndex;
            iTerminalNode.itemCost = 101;
            confirmNode.itemCost = 101;
            StarterPack.Logger.LogDebug(
                $"Item price: {iTerminalNode.itemCost}, Item index: {iTerminalNode.buyItemIndex}"
            );

            // Create new 'buy' keyword
            TerminalKeyword keyword = CreateTerminalKeyword("start", defaultVerb: buyKeyword);

            // Add the new 'buy' keyword to the list
            var nouns = buyKeyword.compatibleNouns.ToList();
            nouns.Add(new CompatibleNoun() { noun = keyword, result = iTerminalNode });
            buyKeyword.compatibleNouns = nouns.ToArray();

            // Add the keywords to the global list
            var allKeywords = terminal.terminalNodes.allKeywords.ToList();
            allKeywords.Add(keyword);
            allKeywords.Add(confirmKeyword);
            allKeywords.Add(denyKeyword);
            terminal.terminalNodes.allKeywords = allKeywords.ToArray();

            return true;
        }

        [HarmonyPatch(typeof(Terminal), nameof(Terminal.LoadNewNodeIfAffordable))]
        [HarmonyPrefix]
        public static bool TerminalLoadNewNodeIfAffordablePatch(
            TerminalNode node,
            Terminal __instance
        )
        {
            Terminal self = __instance;

            if (node.buyItemIndex == myItemIndex)
            {
                StarterPack.Logger.LogDebug($"Intercepted buy node");
                StarterPack.Logger.LogDebug($"Price : {node.itemCost}");
                StarterPack.Logger.LogDebug($"IsConfirmation : {node.isConfirmationNode}");

                if (node.isConfirmationNode)
                {
                    return true;
                }

                // Skip buy and manually add items

                int totalCost = 0;
                int index = 0; // Talkie x3 ; warning : id != index
                for (int i = 0; i < 3; ++i)
                {
                    totalCost += (int)(
                        self.buyableItemsList[index].creditsWorth
                        * (self.itemSalesPercentages[index] / 100.0)
                        * self.playerDefinedAmount
                    );
                    self.orderedItemsFromTerminal.Add(index); // Talkie
                }
                index = 1; // Flashlight x1
                for (int i = 0; i < 1; ++i)
                {
                    totalCost += (int)(
                        self.buyableItemsList[index].creditsWorth
                        * (self.itemSalesPercentages[index] / 100.0)
                        * self.playerDefinedAmount
                    );
                    self.orderedItemsFromTerminal.Add(index); // Talkie
                }
                index = 4; // Pro-Flashlight x2
                for (int i = 0; i < 2; ++i)
                {
                    totalCost += (int)(
                        self.buyableItemsList[index].creditsWorth
                        * (self.itemSalesPercentages[index] / 100.0)
                        * self.playerDefinedAmount
                    );
                    self.orderedItemsFromTerminal.Add(index); // Talkie
                }
                self.numberOfItemsInDropship += 6;

                self.totalCostOfItems = totalCost;

                if (self.groupCredits < self.totalCostOfItems)
                    self.LoadNewNode(self.terminalNodes.specialNodes[2]);
                else
                {
                    self.groupCredits = Mathf.Clamp(
                        self.groupCredits - self.totalCostOfItems,
                        0,
                        10000000
                    );

                    self.LoadNewNode(node);
                }
                return false;
            }
            return true;
        }
    }
}
