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

    [HarmonyPatch(typeof(PlayerController), "OnDeath", new Type[] { })]
    public class bhaptics_OnDeath
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.PlaybackHaptics("Death");
        }
    }

    [HarmonyPatch(typeof(PlayerController), "OnHurt", new Type[] { })]
    public class bhaptics_OnHurt
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerController __instance)
        {
            var angleShift = TactsuitVR.getAngleAndShift(__instance.transform.parent, __instance.DamageableComponent.HitObject.transform.position);
            Plugin.tactsuitVr.PlayBackHit("BulletHit", angleShift.Key, angleShift.Value);
        }
    }

    [HarmonyPatch(typeof(GunController), "Fire", new Type[] { })]
    public class bhaptics_Fire
    {
        [HarmonyPostfix]
        public static void Postfix(GunController __instance)
        {
            if (Traverse.Create(__instance).Property("IsLeftHandedGun").GetValue<Boolean>())

            {
                Plugin.tactsuitVr.PlaybackHaptics("RecoilVest_L");
            } else
            {
                Plugin.tactsuitVr.PlaybackHaptics("RecoilVest_R");
            }
        }
    }

    [HarmonyPatch(typeof(PlayerController), "Update", new Type[] { })]
    public class bhaptics_LowHealth
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerController __instance)
        {
            //Plugin.Log.LogMessage(__instance.DamageableComponent.GetCurrentHealth());
            if (__instance.DamageableComponent.GetCurrentHealth() == 1)
            {
                Plugin.tactsuitVr.PlaybackHaptics("HeartBeat", false);
            }
        }
    }
    
    [HarmonyPatch(typeof(MutatorDisabler), "OnTriggerStay")]
    public class bhaptics_Shower
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.tactsuitVr.PlaybackHaptics("Shower", false, 0.4f);
        }
    }
}

