using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using MyBhapticsTactsuit;

namespace Compound_bhaptics
{
    [BepInPlugin("org.bepinex.plugins.Compound_bhaptics", "Compound bhaptics integration", "1.4")]
    public class Plugin : BaseUnityPlugin
    {
#pragma warning disable CS0109 // Remove unnecessary warning
        internal static new ManualLogSource Log;
#pragma warning restore CS0109
        public static TactsuitVR tactsuitVr;
        public static Vector3 playerPosition;
        // I couldn't find a way to read out the max health. So this is a global variable hack that
        // will just store the maximum health ever read.
        public static float maxHealth = 0f;


        private void Awake()
        {
            // Make my own logger so it can be accessed from the Tactsuit class
            Log = base.Logger;
            // Plugin startup logic
            Logger.LogMessage("Plugin Compound_bhaptics is loaded!");
            tactsuitVr = new TactsuitVR();
            // one startup heartbeat so you know the vest works correctly
            tactsuitVr.PlaybackHaptics("HeartBeat");
            // patch all functions
            var harmony = new Harmony("bhaptics.patch.compound");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(Food), "OnEat", new Type[] { })]
    public class bhaptics_OnEat
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.PlaybackHaptics("Eating");
        }
    }
}

