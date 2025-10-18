using Duckov.Utilities;
using HarmonyLib;
using ItemStatsSystem;

namespace ItemLevelAndSearchSoundMod
{
    [HarmonyPatch(typeof(GameplayDataSettings.LootingData), "GetInspectingTime")]
    public class PatchLootingDataGetInspectingTime
    {
        static void Postfix(GameplayDataSettings.LootingData __instance, Item item, ref float __result)
        {
            ItemValueLevel valueLevel = Util.GetItemValueLevel(item);
            __result = Util.GetInspectingTime(valueLevel);
        }
    }
}