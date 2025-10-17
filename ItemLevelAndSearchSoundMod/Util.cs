using System.Reflection;
using Duckov;
using ItemStatsSystem;
using UnityEngine;

namespace ItemLevelAndSearchSoundMod
{
    public static class Util
    {
        public static ItemValueLevel GetItemValueLevel(Item item)
        {
            float value = 0;
            if (item != null)
            {
                value = item.Value / 2f;
                if (item.name.StartsWith("Bullet_"))
                {
                    // 子弹一次会掉落很多，价值按30格计算
                    value *= 30;
                }
            }
            if (value >= 10000)
            {
                // 范围内53个道具
                return ItemValueLevel.Red;
            }
            else if (value >= 5000)
            {
                // 范围内84个道具
                return ItemValueLevel.LightRed;
            }   
            else if (value >= 2500)
            {
                // 范围内90个道具
                return ItemValueLevel.Orange;
            }
            else if (value >= 1200)
            {
                // 范围内146个道具
                return ItemValueLevel.Purple;
            }
            else if (value >= 600)
            {
                // 范围内177个道具
                return ItemValueLevel.Blue;
            }
            else if (value >= 200)
            {
                // 范围内253个道具
                return ItemValueLevel.Green;
            }
            else
            {
                // 范围内376个道具
                return ItemValueLevel.White;
            }
        }

        public static Color GetItemValueLevelColor(ItemValueLevel level)
        {
            switch (level)
            {
                case ItemValueLevel.Red:
                    return ModBehaviour.Red;
                case ItemValueLevel.LightRed:
                    return ModBehaviour.LightRed;
                case ItemValueLevel.Orange:
                    return ModBehaviour.Orange;
                case ItemValueLevel.Purple:
                    return ModBehaviour.Purple;
                case ItemValueLevel.Blue:
                    return ModBehaviour.Blue;
                case ItemValueLevel.Green:
                    return ModBehaviour.Green;
                case ItemValueLevel.White:
                    return ModBehaviour.White;
                default:
                    return ModBehaviour.White;
            }
        }

        public static float GetInspectingTime(ItemValueLevel level)
        {
            switch (level)
            {
                case ItemValueLevel.Red:
                    return 4.5f;
                case ItemValueLevel.LightRed:
                    return 3.25f;
                case ItemValueLevel.Orange:
                    return 2.25f;
                case ItemValueLevel.Purple:
                    return 1.75f;
                case ItemValueLevel.Blue:
                    return 1.25f;
                case ItemValueLevel.Green:
                    return 1f;
                case ItemValueLevel.White:
                    return 0.75f;
                default:
                    return 0.75f;
            }
        }

        public static string GetInspectedSound(ItemValueLevel level)
        {
            switch (level)
            {
                case ItemValueLevel.Red:
                    return ModBehaviour.High;
                case ItemValueLevel.LightRed:
                    return ModBehaviour.High;
                case ItemValueLevel.Orange:
                    return ModBehaviour.High;
                case ItemValueLevel.Purple:
                    return ModBehaviour.Medium;
                case ItemValueLevel.Blue:
                    return ModBehaviour.Medium;
                case ItemValueLevel.Green:
                    return ModBehaviour.Low;
                case ItemValueLevel.White:
                    return ModBehaviour.Low;
                default:
                    return ModBehaviour.Low;
            }
        }
    }
}