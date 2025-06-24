using HarmonyLib;

namespace StarterPack.Patches;

[HarmonyPatch(typeof(GrabbableObject))]
public class PatchGrabbable
{
    [HarmonyPatch(nameof(GrabbableObject.EquipItem))]
    [HarmonyPostfix]
    private static void EquipItemPostfix(GrabbableObject __instance)
    {
        var itemName = __instance.itemProperties.itemName;
        StarterPack.Logger.LogDebug(itemName);
    }
}
