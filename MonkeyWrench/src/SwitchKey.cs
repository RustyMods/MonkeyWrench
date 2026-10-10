using HarmonyLib;
using UnityEngine;

namespace MonkeyWrench;

[HarmonyPatch(typeof(Player), nameof(Player.SetControls))]
public static class Switch_MonkeyWrench_State_Update
{
    private static float lastSwitchTime;

    private static void Postfix(Player __instance)
    {
        if (Time.time - lastSwitchTime < 0.3f) return;

        if (MonkeyWrenchPlugin.SwitchKey.IsPressed())
        {
            bool wasMonkeyWrenched = __instance.IsMonkeyWrenched();
            __instance.SetMonkeyWrenched(!wasMonkeyWrenched);
                
            __instance.SetupAnimationState();
            if (wasMonkeyWrenched)
            {
                if (__instance.m_leftItem != null)
                {
                    __instance.UnequipItem(__instance.m_leftItem);
                }
            }
                
            lastSwitchTime = Time.time;
        }
    }
}