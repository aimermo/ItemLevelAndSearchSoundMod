using Duckov.UI;
using Duckov.UI.Animations;
using Duckov.Utilities;
using FMOD;
using HarmonyLib;
using ItemStatsSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ItemLevelAndSearchSoundMod
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private const string Id = "Spuddy.ItemLevelAndSearchSoundMod";
        public const string Low = "UI/hover";
        public const string Medium = "UI/sceneloader_click";
        public const string High = "UI/game_start";

        /// <summary>
        /// 原本的搜索动画参数
        /// </summary>
        public static float DefaultSearchAnimationValue;
        /// <summary>
        /// 关闭自定义搜索时长
        /// </summary>
        public static bool DisableModSearchTime;

        /// <summary>
        /// 背景色正常显示，但是搜索时间和音效强制按白色稀有度计算的物品Id列表
        /// </summary>
        public static readonly int[] ForceWhiteLevelTypeID = new int[]{ 
            308, // 冷核碎片
            309, // 赤核碎片
            368, // 虚化的羽毛
            394, // 计算核心
            890  // 狗牌
        };

        public static Dictionary<ItemValueLevel, Sound> ItemValueLevelSound = new Dictionary<ItemValueLevel, Sound>();
        public static string ErrorMessage = "";
        public static ChannelGroup SfxGroup;

        public static Color White;
        public static Color Green;
        public static Color Blue;
        public static Color Purple;
        public static Color Orange;
        public static Color LightRed;
        public static Color Red;

        private Harmony harmony;

        private Channel searchingChannel;
        /// <summary>
        /// 搜索中播放的音效
        /// </summary>
        private static Sound searchingSound;

        private void OnEnable()
        {
            UnityEngine.Debug.Log("ItemLevelAndSearchSoundMod OnEnable");

            FMODUnity.RuntimeManager.GetBus("bus:/Master/SFX").getChannelGroup(out SfxGroup);

            DisableModSearchTime = File.Exists("ItemLevelAndSearchSoundMod/DisableModSearchTime.txt");

            var magnifier = GameplayDataSettings.UIPrefabs.ItemDisplay.transform.Find("InspectioningIndicator/Magnifier");
            if (magnifier != null)
            {
                var revolver = magnifier.GetComponent<Revolver>();
                if (revolver != null)
                {
                    DefaultSearchAnimationValue = revolver.rPM;
                }
            }

            try
            {
                string searchingSoundPath = "ItemLevelAndSearchSoundMod/Searching.mp3";
                if (File.Exists(searchingSoundPath))
                {
                    var soundResult = FMODUnity.RuntimeManager.CoreSystem.createSound(searchingSoundPath, MODE.LOOP_NORMAL, out searchingSound);
                    if (soundResult != RESULT.OK)
                    {
                        ErrorMessage += "FMOD failed to create searching sound: " + soundResult + "\n";
                    }
                    else
                    {
                        UnityEngine.Debug.Log("ItemLevelAndSearchSoundMod Load Searching Sound Success");
                    }
                }

                foreach (ItemValueLevel item in Enum.GetValues(typeof(ItemValueLevel)))
                {
                    string path = $"ItemLevelAndSearchSoundMod/{(int) item}.mp3";
                    if (File.Exists(path))
                    {
                        var soundResult = FMODUnity.RuntimeManager.CoreSystem.createSound(path, MODE.LOOP_OFF, out Sound sound);
                        if (soundResult != RESULT.OK)
                        {
                            ErrorMessage += "FMOD failed to create sound: " + soundResult + "\n";
                            continue;
                        }
                        ItemValueLevelSound.Add(item, sound);
                        UnityEngine.Debug.Log("ItemLevelAndSearchSoundMod Load Custom Sound Success: " + item);
                    }
                }
            }
            catch (Exception e)
            {
                ErrorMessage += e.ToString() + "\n";
            }

            ColorUtility.TryParseHtmlString("#FFFFFF00", out White);
            ColorUtility.TryParseHtmlString("#7cff7c40", out Green);
            ColorUtility.TryParseHtmlString("#7cd5ff40", out Blue);
            ColorUtility.TryParseHtmlString("#d0acff40", out Purple);
            ColorUtility.TryParseHtmlString("#ffdc2496", out Orange);
            ColorUtility.TryParseHtmlString("#ff585896", out LightRed);
            ColorUtility.TryParseHtmlString("#bb000096", out Red);

            ItemUtilities.OnItemSentToPlayerInventory += OnItemSentToPlayerInventory;
            InteractableLootbox.OnStartLoot += OnStartLoot;
            InteractableLootbox.OnStopLoot += OnStopLoot;

            harmony = new Harmony(Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void OnStartLoot(InteractableLootbox lootbox)
        {
            if (!lootbox.Inventory.NeedInspection || lootbox.Inventory.Content.All(item => item.Inspected))
            {
                return;
            }
            if (!searchingSound.hasHandle())
            {
                return;
            }
            RESULT result = FMODUnity.RuntimeManager.CoreSystem.playSound(searchingSound, SfxGroup, false, out searchingChannel);
            if (result != RESULT.OK)
            {
                ErrorMessage += "FMOD failed to play searching sound: " + result + "\n";
            }
        }

        private void OnStopLoot(InteractableLootbox lootbox)
        {
            if (searchingChannel.hasHandle())
            {
                searchingChannel.stop();
                searchingChannel = default;
            }
        }

        private void OnItemSentToPlayerInventory(Item item)
        {
            item.onInspectionStateChanged -= PatchItemDisplaySetup.OnInspectionStateChanged;
        }

        private void OnGUI()
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                var errorStyle = new GUIStyle(GUI.skin.label);
                errorStyle.normal.textColor = Color.red;
                GUI.Label(new Rect(10, 10, Screen.width - 10, Screen.height - 10), "ItemLevelAndSearchSoundMod Error: \n" + ErrorMessage, errorStyle);
            }
        }

        private void OnDisable()
        {
            ItemUtilities.OnItemSentToPlayerInventory -= OnItemSentToPlayerInventory;
            InteractableLootbox.OnStartLoot -= OnStartLoot;
            InteractableLootbox.OnStopLoot -= OnStopLoot;

            harmony.UnpatchAll(Id);

            if (searchingSound.hasHandle())
            {
                searchingSound.release();
            }

            foreach (var sound in ItemValueLevelSound)
            {
                sound.Value.release();
            }
            ItemValueLevelSound.Clear();
        }
    }
}

