using System;
using System.Collections.Generic;
using System.Linq;
using Duckov;
using Duckov.UI;
using Duckov.UI.Animations;
using FMOD;
using HarmonyLib;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.UI;

namespace ItemLevelAndSearchSoundMod
{
    [HarmonyPatch(typeof(ItemDisplay), "Setup")]
    public class PatchItemDisplaySetup
    {
        private static HashSet<ItemDisplay> updatedAnimationItemDisplays = new HashSet<ItemDisplay>();
        private static Dictionary<Item, ItemDisplay> itemDisplayMap = new Dictionary<Item, ItemDisplay>();
        static void Postfix(ItemDisplay __instance, Item target)
        {
            if (__instance == null)
            {
                return;
            }

            if (target == null)
            {
                SetColor(__instance, Util.GetItemValueLevelColor(ItemValueLevel.White));
                return;
            }

            // 情况1. 在搜索中关闭了容器，之后再打开容器，OnInspectionStateChanged事件未消耗
            // 情况2. 自动拾取Mod会在不触发onInspectionStateChanged的情况下拾取道具，Item会回到对象池，事件就会留到下次触发
            target.onInspectionStateChanged -= OnInspectionStateChanged;
            itemDisplayMap.Remove(target);

            if (!updatedAnimationItemDisplays.Contains(__instance))
            {
                updatedAnimationItemDisplays.Add(__instance);
                var magnifier = __instance.transform.Find("InspectioningIndicator/Magnifier");
                if (magnifier != null)
                {
                    var revolver = magnifier.GetComponent<Revolver>();
                    if (revolver != null)
                    {
                        revolver.rPM = ModBehaviour.DefaultSearchAnimationValue * 0.75f;
                    }
                }
            }

            if (target.InInventory != null && target.InInventory.NeedInspection && !target.Inspected)
            {
                // 物品还未搜索的情况
                target.onInspectionStateChanged += OnInspectionStateChanged;
                itemDisplayMap[target] = __instance;

                SetColor(__instance, Util.GetItemValueLevelColor(ItemValueLevel.White));
                return;
            }

            ItemValueLevel level = Util.GetItemValueLevel(target);
            Color color = Util.GetItemValueLevelColor(level);
            SetColor(__instance, color);
        }

        static void OnInspectionStateChanged(Item item)
        {
            if (!itemDisplayMap.TryGetValue(item, out ItemDisplay itemDisplay))
            {
                return;
            }
            if (item.Inspected)
            {
                item.onInspectionStateChanged -= OnInspectionStateChanged;
                ItemValueLevel level = Util.GetItemValueLevel(item);
                Color color = Util.GetItemValueLevelColor(level);
                SetColor(itemDisplay, color);

                ItemValueLevel playSoundLevel = level;
                if (ModBehaviour.ForceWhiteLevelTypeID.Contains(item.TypeID))
                {
                    playSoundLevel = ItemValueLevel.White;
                }
                if (ModBehaviour.ItemValueLevelSound.TryGetValue(playSoundLevel, out Sound sound))
                {
                    FMODUnity.RuntimeManager.GetBus("bus:/Master/SFX").getChannelGroup(out ChannelGroup sfxGroup);
                    RESULT result = FMODUnity.RuntimeManager.CoreSystem.playSound(sound, sfxGroup, false, out Channel channel);
                    if (result != RESULT.OK)
                    {
                        ModBehaviour.ErrorMessage += "FMOD failed to play sound: " + result + "\n";
                    }
                }
                else
                {
                    (string eventName, float volume) = Util.GetInspectedSound(playSoundLevel);
                    if (AudioManager.TryCreateEventInstance(eventName, out var eventInstance))
                    {
                        eventInstance.setVolume(volume);
                        eventInstance.start();
                        eventInstance.release();
                    }
                }
            }
        }

        static void SetColor(ItemDisplay __instance, Color color)
        {
            try
            {
                __instance.transform.Find("BG").GetComponent<Image>().color = color;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("ItemLevelAndSearchSoundMod Patch SetColor Error: " + ex.Message);
            }
        }
    }
}