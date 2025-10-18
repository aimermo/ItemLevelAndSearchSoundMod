using FMOD;
using HarmonyLib;
using ItemStatsSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ItemLevelAndSearchSoundMod
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private const string Id = "Spuddy.ItemLevelAndSearchSoundMod";
        public const string Low = "UI/click";
        public const string Medium = "UI/sceneloader_click";
        public const string High = "UI/game_start";

        public static Dictionary<ItemValueLevel, Sound> ItemValueLevelSound = new Dictionary<ItemValueLevel, Sound>();
        public static string ErrorMessage = "";

        public static Color White;
        public static Color Green;
        public static Color Blue;
        public static Color Purple;
        public static Color Orange;
        public static Color LightRed;
        public static Color Red;

        private Harmony harmony;

        private void OnEnable()
        {
            UnityEngine.Debug.Log("ItemLevelAndSearchSoundMod OnEnable");

            try
            {
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
            ColorUtility.TryParseHtmlString("#ffe60096", out Orange);
            ColorUtility.TryParseHtmlString("#ff585896", out LightRed);
            ColorUtility.TryParseHtmlString("#bb000096", out Red);

            harmony = new Harmony(Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
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
            harmony.UnpatchAll(Id);

            foreach (var sound in ItemValueLevelSound)
            {
                sound.Value.release();
            }
            ItemValueLevelSound.Clear();
        }
    }
}

