using Duckov.UI;
using HarmonyLib;
using ItemStatsSystem;

namespace ItemLevelAndSearchSoundMod
{
    [HarmonyPatch(typeof(ItemDisplay), "OnDisable")]
    public class PatchItemDisplayOnDisable
    {
        static void Postfix(ItemDisplay __instance)
        {
            if (__instance == null)
            {
                return;
            }
            Item item = __instance.Target;
            if (item == null)
            {
                return;
            }
            item.onInspectionStateChanged -= PatchItemDisplaySetup.OnInspectionStateChanged;
        }
    }
}