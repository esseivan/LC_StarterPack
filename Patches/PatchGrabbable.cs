using HarmonyLib;

namespace MyFirstMod.Patches;

[HarmonyPatch(typeof(GrabbableObject))]
public class PatchGrabbable
{
    [HarmonyPatch(nameof(GrabbableObject.EquipItem))]
    [HarmonyPostfix]
    private static void EquipItemPostfix(GrabbableObject __instance)
    {
        var itemName = __instance.itemProperties.itemName;
        MyFirstMod.Logger.LogDebug(itemName);
    }
}
