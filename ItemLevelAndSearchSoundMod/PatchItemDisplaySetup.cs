using System;
using Duckov;
using Duckov.UI;
using HarmonyLib;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.UI;

namespace ItemLevelAndSearchSoundMod
{
    [HarmonyPatch(typeof(ItemDisplay), "Setup")]
    public class PatchItemDisplaySetup
    {
        static void Postfix(ItemDisplay __instance, Item target)
        {
            if (__instance == null)
            {
                return;
            }
            ItemValueLevel level = Util.GetItemValueLevel(target);
            Color color = Util.GetItemValueLevelColor(level);

            if (target != null && ModBehaviour.IsLooting && !target.Inspected)
            {
                Color originalColor = color;
                color = ModBehaviour.White;
                target.onInspectionStateChanged += OnInspectionStateChanged;

                void OnInspectionStateChanged(Item item)
                {
                    if (item.Inspected)
                    {
                        item.onInspectionStateChanged -= OnInspectionStateChanged;
                        SetColor(__instance, originalColor);
                        AudioManager.Post(Util.GetInspectedSound(level));
                    }
                }
            }

            SetColor(__instance, color);
        }

        static void SetColor(ItemDisplay __instance, Color color)
        {
            try
            {
                __instance.transform.Find("BG").GetComponent<Image>().color = color;
            }
            catch (Exception ex)
            {
                Debug.LogError("ItemLevelAndSearchSoundMod Patch SetColor Error: " + ex.Message);
            }
        }
    }
}