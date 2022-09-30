using System;
using MelonLoader;
using HarmonyLib;
using MyBhapticsTactsuit;

namespace Compound_bhaptics
{
    public class Compound_bhaptics : MelonMod
    {
        public static TactsuitVR tactsuitVr;
        public static bool playerRightHanded = true;

        public override void OnInitializeMelon()
        {
            //base.OnApplicationStart();
            tactsuitVr = new TactsuitVR();
            tactsuitVr.PlaybackHaptics("HeartBeat");
        }

        [HarmonyPatch(typeof(SLZ.Data.SaveData.PlayerSettings), "OnPropertyChanged", new Type[] { typeof(string) })]
        public class bhaptics_PropertyChanged
        {
            [HarmonyPostfix]
            public static void Postfix(SLZ.Data.SaveData.PlayerSettings __instance)
            {
                playerRightHanded = __instance.RightHanded;
            }
        }



        [HarmonyPatch(typeof(Player_Health), "Death", new Type[] { })]
        public class bhaptics_PlayerDeath
        {
            [HarmonyPostfix]
            public static void Postfix(Player_Health __instance)
            {
                tactsuitVr.StopThreads();
            }
        }

        [HarmonyPatch(typeof(Player_Health), "Update", new Type[] { })]
        public class bhaptics_PlayerHealthUpdate
        {
            [HarmonyPostfix]
            public static void Postfix(Player_Health __instance)
            {
                if (__instance.curr_Health <= 0.3f * __instance.max_Health) tactsuitVr.StartHeartBeat();
                else tactsuitVr.StopHeartBeat();
            }
        }
    }
}
