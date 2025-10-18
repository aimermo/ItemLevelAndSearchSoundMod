using System;
using System.Linq;
using Duckov;
using Duckov.UI;
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
        static void Postfix(ItemDisplay __instance, Item target)
        {
            if (__instance == null)
            {
                return;
            }
            ItemValueLevel level = Util.GetItemValueLevel(target);
            Color color = Util.GetItemValueLevelColor(level);

            if (target != null && target.InInventory != null && target.InInventory.NeedInspection && !target.Inspected)
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
                            else
                            {
                                channel.setVolume(AudioManager.GetBus("Master/SFX").Volume);
                            }
                        }
                        else
                        {
                            AudioManager.Post(Util.GetInspectedSound(playSoundLevel));
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