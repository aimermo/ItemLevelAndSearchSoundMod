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

            ItemValueLevel level = Util.GetItemValueLevel(target);
            Color color = Util.GetItemValueLevelColor(level);
            int id = target.TypeID;

            if (target != null && target.InInventory != null && target.InInventory.NeedInspection && !target.Inspected)
            {
                Color originalColor = color;
                color = ModBehaviour.White;
                target.onInspectionStateChanged += OnInspectionStateChanged;

                void OnInspectionStateChanged(Item item)
                {
                    if (item.TypeID != id || !item.InInventory.NeedInspection)
                    {
                        // 自动拾取Mod会在不触发onInspectionStateChanged的情况下拾取道具，Item会回到对象池，事件就会留到下次触发，这里校验是不是同一个对象，以及所属容器是否需要搜索
                        return;
                    }
                    if (item.Inspected)
                    {
                        item.onInspectionStateChanged -= OnInspectionStateChanged;
                        SetColor(__instance, originalColor);

                        ItemValueLevel playSoundLevel = level;
                        if (ModBehaviour.ForceWhiteLevelTypeID.Contains(target.TypeID))
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
                UnityEngine.Debug.LogError("ItemLevelAndSearchSoundMod Patch SetColor Error: " + ex.Message);
            }
        }
    }
}