using Duckov;
using Duckov.UI;
using HarmonyLib;
using ItemStatsSystem;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ItemLevelAndSearchSoundMod
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private const string Id = "Spuddy.ItemLevelAndSearchSoundMod";
        public const string Low = "UI/click";
        public const string Medium = "UI/sceneloader_click";
        public const string High = "UI/game_start";

        public static bool IsLooting = false;
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
            Debug.Log("ItemLevelAndSearchSoundMod OnEnable");

            InteractableLootbox.OnStartLoot += OnStartLoot;
            InteractableLootbox.OnStopLoot += OnStopLoot;

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

        private void OnDisable()
        {
            harmony.UnpatchAll(Id);

            InteractableLootbox.OnStartLoot -= OnStartLoot;
            InteractableLootbox.OnStopLoot -= OnStopLoot;
        }

        private void OnStartLoot(InteractableLootbox lootbox)
        {
            IsLooting = true;
        }

        private void OnStopLoot(InteractableLootbox lootbox)
        {
            IsLooting = false;
        }
    }
}

