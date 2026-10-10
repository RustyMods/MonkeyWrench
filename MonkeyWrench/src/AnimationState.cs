using HarmonyLib;

namespace MonkeyWrench;

[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.SetAnimationState))]
public static class Modify_Animation_State_Patch
{
    private static void Prefix(
        Humanoid __instance, 
        ref ItemDrop.ItemData.AnimationState state)
    {
        if (!__instance.IsMonkeyWrenched()) return;
        if (__instance.m_rightItem != null && !__instance.m_rightItem.CanBeMonkeyWrenched())
        {
            return;
        }
            
        if (!__instance.IsPlayer()) return;
        if (state is 
            ItemDrop.ItemData.AnimationState.Greatsword or 
            ItemDrop.ItemData.AnimationState.TwoHandedAxe or 
            ItemDrop.ItemData.AnimationState.TwoHandedClub)
        {
            state = ItemDrop.ItemData.AnimationState.OneHanded;
        }
    }
}