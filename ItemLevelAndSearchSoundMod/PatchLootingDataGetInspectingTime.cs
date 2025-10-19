using System.Linq;
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
            if (ModBehaviour.DisableModSearchTime)
            {
                return;
            }
            ItemValueLevel valueLevel = ItemValueLevel.White;
            if (item == null || !ModBehaviour.ForceWhiteLevelTypeID.Contains(item.TypeID))
            {
                valueLevel = Util.GetItemValueLevel(item);
            }
            __result = Util.GetInspectingTime(valueLevel);
        }
    }
}