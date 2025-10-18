using ItemStatsSystem;
using UnityEngine;

namespace ItemLevelAndSearchSoundMod
{
    public static class Util
    {
        public static ItemValueLevel GetItemValueLevel(Item item)
        {
            if (item == null)
            {
                return ItemValueLevel.White;
            }
            // 除2得到售价
            float value = item.Value / 2f;

            if (item.Tags.Contains("Bullet"))
            {
                // 子弹特殊处理
                if (item.DisplayQuality != DisplayQuality.None)
                {
                    // 有官方稀有度的子弹，使用官方的稀有度
                    return ParseDisplayQuality(item.DisplayQuality);
                }

                if (item.Quality == 1)
                {
                    // 生锈弹
                    return ItemValueLevel.White;
                }
                if (item.Quality == 2)
                {
                    // 普通弹
                    return ItemValueLevel.Green;
                }
                // 剩下的都是特殊子弹，根据30一组计算稀有度，最高到橙色
                ItemValueLevel bulletLevel = CalculateItemValueLevel((int)(value * 30));
                if (bulletLevel > ItemValueLevel.Orange)
                {
                    return ItemValueLevel.Orange;
                }
                return bulletLevel;
            }

            if (item.Tags.Contains("Equipment"))
            {
                // 装备特殊处理
                if (item.Tags.Contains("Special"))
                {
                    // 特殊装备
                    if (item.name.Contains("StormProtection"))
                    {
                        // 风暴系列的装备稀有度直接使用官方的
                        return (ItemValueLevel) (item.Quality - 1);
                    }
                    // 其他全部按价值计算
                    return CalculateItemValueLevel((int)value);
                }
                else
                {
                    // 非特殊装备
                    if (item.Quality <= 7)
                    {
                        // 7以内的装备按官方稀有度计算
                        return (ItemValueLevel) (item.Quality - 1);
                    }
                    return CalculateItemValueLevel((int)value);
                }
            }

            // 物品价值
            ItemValueLevel itemValueLevel = CalculateItemValueLevel((int)value);

            // 官方的物品稀有度和物品价值取最大值
            ItemValueLevel displayQuality = ParseDisplayQuality(item.DisplayQuality);

            if (displayQuality > itemValueLevel)
            {
                itemValueLevel = displayQuality;
            }
            return itemValueLevel;
        }

        public static ItemValueLevel CalculateItemValueLevel(int value)
        {
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

        public static ItemValueLevel ParseDisplayQuality(DisplayQuality displayQuality)
        {
            switch (displayQuality)
            {
                case DisplayQuality.None:
                case DisplayQuality.White:
                    return ItemValueLevel.White;
                case DisplayQuality.Green:
                    return ItemValueLevel.Green;
                case DisplayQuality.Blue:
                    return ItemValueLevel.Blue;
                case DisplayQuality.Purple:
                    return ItemValueLevel.Purple;
                case DisplayQuality.Orange:
                    return ItemValueLevel.Orange;
                case DisplayQuality.Red:
                case DisplayQuality.Q7:
                case DisplayQuality.Q8:
                    return ItemValueLevel.Red;
                default:
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