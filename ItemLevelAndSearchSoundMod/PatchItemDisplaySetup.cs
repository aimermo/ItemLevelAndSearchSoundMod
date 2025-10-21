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
        /// <summary>
        /// 保存正在播放搜索音效的Channel，用于停止播放
        /// </summary>
        private static Dictionary<Item, Channel> searchingChannelMap = new Dictionary<Item, Channel>();
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
            
            // 如果之前有正在播放的搜索音效，停止它
            if (searchingChannelMap.TryGetValue(target, out Channel channel))
            {
                channel.stop();
                searchingChannelMap.Remove(target);
            }

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

                // 开始搜索时播放搜索音效
                if (ModBehaviour.SearchingSound.hasHandle())
                {
                    FMODUnity.RuntimeManager.GetBus("bus:/Master/SFX").getChannelGroup(out ChannelGroup sfxGroup);
                    RESULT result = FMODUnity.RuntimeManager.CoreSystem.playSound(ModBehaviour.SearchingSound, sfxGroup, false, out Channel searchChannel);
                    if (result == RESULT.OK)
                    {
                        searchingChannelMap[target] = searchChannel;
                    }
                    else
                    {
                        ModBehaviour.ErrorMessage += "FMOD failed to play searching sound: " + result + "\n";
                    }
                }

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
                
                // 搜索完成，停止播放搜索音效
                if (searchingChannelMap.TryGetValue(item, out Channel channel))
                {
                    channel.stop();
                    searchingChannelMap.Remove(item);
                }
                
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
                    RESULT result = FMODUnity.RuntimeManager.CoreSystem.playSound(sound, sfxGroup, false, out Channel soundChannel);
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