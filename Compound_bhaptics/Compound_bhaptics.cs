using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using MyBhapticsTactsuit;
using Object = UnityEngine.Object;

namespace Compound_bhaptics
{
    [BepInPlugin("org.bepinex.plugins.Compound_bhaptics", "Compound bhaptics integration", "1.4")]
    public class Plugin : BaseUnityPlugin
    {
#pragma warning disable CS0109 // Remove unnecessary warning
        internal static new ManualLogSource Log;
#pragma warning restore CS0109
        public static TactsuitVR tactsuitVr;

        public static bool twoHanded = false;

        private void Awake()
        {
            // Make my own logger so it can be accessed from the Tactsuit class
            Log = base.Logger;
            // Plugin startup logic
            Logger.LogMessage("Plugin Compound_bhaptics is loaded!");
            tactsuitVr = new TactsuitVR();
            // one startup heartbeat so you know the vest works correctly
            tactsuitVr.PlaybackHaptics("HeartBeat");
            tactsuitVr.PlaybackHaptics("RecoilArm_R");
            tactsuitVr.PlaybackHaptics("RecoilArm_L");
            tactsuitVr.PlaybackHaptics("eatingvisor");
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
            if (Plugin.tactsuitVr.suitDisabled)
            {
                return;
            }
            Plugin.tactsuitVr.PlaybackHaptics("eatingvisor");
            Plugin.tactsuitVr.PlaybackHaptics("Eating");
        }
    }

    [HarmonyPatch(typeof(PlayerController), "OnDeath", new Type[] { })]
    public class bhaptics_OnDeath
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (Plugin.tactsuitVr.suitDisabled)
            {
                return;
            }
            Plugin.tactsuitVr.PlaybackHaptics("Death");
            Plugin.tactsuitVr.PlaybackHaptics("hurtvisor");
        }
    }

    [HarmonyPatch(typeof(PlayerController), "OnHurt", new Type[] { })]
    public class bhaptics_OnHurt
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerController __instance)
        {
            if (Plugin.tactsuitVr.suitDisabled)
            {
                return;
            }
            var angleShift = TactsuitVR.getAngleAndShift(__instance.transform.parent, __instance.DamageableComponent.HitObject.transform.position);
            Plugin.tactsuitVr.PlayBackHit("BulletHit", angleShift.Key, angleShift.Value);
            Plugin.tactsuitVr.PlaybackHaptics("hurtvisor");
        }
    }

    [HarmonyPatch(typeof(Explosion), "CheckIfVisible")]
    public class bhaptics_OnExplosion
    {
        [HarmonyPostfix]
        public static void Postfix(bool __result)
        {
            if (Plugin.tactsuitVr.suitDisabled)
            {
                return;
            }

            if(__result)
            {
                Plugin.tactsuitVr.PlaybackHaptics("explosionarm_R");
                Plugin.tactsuitVr.PlaybackHaptics("explosionarm_L");
                Plugin.tactsuitVr.PlaybackHaptics("explosionvest");
                Plugin.tactsuitVr.PlaybackHaptics("firevisor");
            }
        }
    }

    [HarmonyPatch(typeof(Grabber), "TwoHandGrabModeUpdate")]
    public class bhaptics_twohanded
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.twoHanded = true;
        }
    }
    
    [HarmonyPatch(typeof(Grabber), "OneHandGrabModeUpdate")]
    public class bhaptics_onehanded
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Plugin.twoHanded = false;
        }
    }


    [HarmonyPatch(typeof(GunController), "Fire", new Type[] { })]
    public class bhaptics_Fire
    {
        [HarmonyPostfix]
        public static void Postfix(GunController __instance)
        {
            if (Plugin.tactsuitVr.suitDisabled)
            {
                return;
            }

            if (Traverse.Create(__instance).Property("IsLeftHandedGun").GetValue<Boolean>())

            {
                Plugin.tactsuitVr.PlaybackHaptics("RecoilArm_L");
                Plugin.tactsuitVr.PlaybackHaptics("RecoilVest_L");
                if(Plugin.twoHanded)
                {
                    Plugin.tactsuitVr.PlaybackHaptics("RecoilArm_R");
                }
            } else
            {
                Plugin.tactsuitVr.PlaybackHaptics("RecoilArm_R");
                Plugin.tactsuitVr.PlaybackHaptics("RecoilVest_R");
                if (Plugin.twoHanded)
                {
                    Plugin.tactsuitVr.PlaybackHaptics("RecoilArm_L");
                }
            }
            Plugin.tactsuitVr.PlaybackHaptics("firevisor");
        }
    }

    [HarmonyPatch(typeof(PlayerController), "Update", new Type[] { })]
    public class bhaptics_LowHealth
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerController __instance)
        {
            if (Plugin.tactsuitVr.suitDisabled)
            {
                return;
            }
            //Plugin.Log.LogMessage(__instance.DamageableComponent.GetCurrentHealth());
            if (__instance.DamageableComponent.GetCurrentHealth() == 1)
            {
                Plugin.tactsuitVr.PlaybackHaptics("HeartBeat", false);
            }
        }
    }
    
    [HarmonyPatch(typeof(EmergencyFirearm), "FixedUpdate")]
    public class bhaptics_OnEmergencyFirearm
    {
        [HarmonyPrefix]
        public static void Prefix(EmergencyFirearm __instance)
        {
            if (Plugin.tactsuitVr.suitDisabled)
            {
                return;
            }

            Vector3 vector3;
            Damageable caseGlass = Traverse.Create(__instance).Field("CaseGlass").GetValue<Damageable>();
            TrackedObjectPhysicsCalculator leftHand = Traverse.Create(__instance).Field("LeftHand").GetValue<TrackedObjectPhysicsCalculator>();
            TrackedObjectPhysicsCalculator rightHand = Traverse.Create(__instance).Field("RightHand").GetValue<TrackedObjectPhysicsCalculator>();

            if ((Object)caseGlass != (Object)null && (Object)leftHand != (Object)null)
            {
                vector3 = caseGlass.transform.position - leftHand.transform.position;
                if ((double)vector3.sqrMagnitude < 0.022500000894069672)
                {
                    vector3 = caseGlass.transform.forward * Vector3.Dot(leftHand.GetSmoothedLinearVelocity(), 
                        caseGlass.transform.forward);
                    if ((double)vector3.sqrMagnitude > 1.0)
                    {
                        Plugin.tactsuitVr.PlaybackHaptics("RecoilArm_L");
                        Plugin.tactsuitVr.PlaybackHaptics("RecoilVest_L");
                    }
                }
            }

            if ((Object)caseGlass != (Object)null && (Object)rightHand != (Object)null)
            {
                vector3 = caseGlass.transform.position - rightHand.transform.position;
                if ((double)vector3.sqrMagnitude < 0.022500000894069672)
                {
                    vector3 = caseGlass.transform.forward * Vector3.Dot(rightHand.GetSmoothedLinearVelocity(),
                        caseGlass.transform.forward);
                    if ((double)vector3.sqrMagnitude > 1.0)
                    {
                        Plugin.tactsuitVr.PlaybackHaptics("RecoilArm_R");
                        Plugin.tactsuitVr.PlaybackHaptics("RecoilVest_R");
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(MutatorDisabler), "OnTriggerStay")]
    public class bhaptics_Shower
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (Plugin.tactsuitVr.suitDisabled)
            {
                return;
            }
            Plugin.tactsuitVr.PlaybackHaptics("Shower", false, 0.4f);
            Plugin.tactsuitVr.PlaybackHaptics("shower_L", false, 0.4f);
            Plugin.tactsuitVr.PlaybackHaptics("shower_R", false, 0.4f);
            Plugin.tactsuitVr.PlaybackHaptics("showervisor");
        }
    }

    [HarmonyPatch(typeof(SyringeController), "Inject")]
    public class bhaptics_SyringeController
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (Plugin.tactsuitVr.suitDisabled)
            {
                return;
            }
            Plugin.tactsuitVr.PlaybackHaptics("SuperPower");
            Plugin.tactsuitVr.PlaybackHaptics("superpower_L");
            Plugin.tactsuitVr.PlaybackHaptics("superpower_R"); 
            Plugin.tactsuitVr.PlaybackHaptics("syringevisor");
        }
    }
}

