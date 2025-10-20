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
                if (valueLevel > ItemValueLevel.Orange && item != null && item.Tags.Contains("Bullet"))
                {
                    // 子弹搜索时间最多为金色物品时间
                    valueLevel = ItemValueLevel.Orange;
                }
            }
            __result = Util.GetInspectingTime(valueLevel);
        }
    }
}