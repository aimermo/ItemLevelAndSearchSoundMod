using System;
using Duckov;
using HarmonyLib;

namespace ItemLevelAndSearchSoundMod
{
    [HarmonyPatch(typeof(AudioManager), "Post")]
    [HarmonyPatch(new Type[]{ typeof(string) })]
    public class PatchAudioManagerPlayOneShot
    {
        static void Prefix(AudioManager __instance, string eventName)
        {
            UnityEngine.Debug.Log($"AudioManager MPost called: eventName={eventName}");
        }
    }
}