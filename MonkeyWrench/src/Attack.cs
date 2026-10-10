using HarmonyLib;

namespace MonkeyWrench;

[HarmonyPatch(typeof(Attack), nameof(Attack.Start))]
public static class Modify_Attack_Animation_Patch
{
    private static void Prefix(
        Attack __instance, 
        Humanoid character, 
        ItemDrop.ItemData weapon)
    {
        if (!character.IsMonkeyWrenched()) return;
        if (!weapon.CanBeMonkeyWrenched()) return;
            
        if (!character.IsPlayer()) return;
        bool isSecondary = __instance.m_attackAnimation.EndsWith("_secondary");
        switch (weapon.m_shared.m_animationState)
        {
            case ItemDrop.ItemData.AnimationState.Greatsword:
                __instance.m_attackAnimation = isSecondary ? "sword_secondary" : "swing_longsword";
                __instance.m_attackChainLevels = isSecondary ? 0 : 3;
                break;
            case ItemDrop.ItemData.AnimationState.TwoHandedAxe:
                __instance.m_attackAnimation = isSecondary ? "axe_secondary" : "swing_axe";
                __instance.m_attackChainLevels = isSecondary ? 0 : 3;
                __instance.m_attackType = isSecondary ? Attack.AttackType.Vertical : Attack.AttackType.Horizontal;
                break;
            case ItemDrop.ItemData.AnimationState.TwoHandedClub:
                __instance.m_attackAnimation = isSecondary ? "mace_secondary" : "swing_longsword";
                __instance.m_attackChainLevels = isSecondary ? 0 : 3;
                __instance.m_attackType = isSecondary ? Attack.AttackType.Vertical : Attack.AttackType.Horizontal;
                break;
        }

        __instance.m_attackStamina *= MonkeyWrenchPlugin.StaminaModifier;
        __instance.m_damageMultiplier -= MonkeyWrenchPlugin.DamageModifier;
    }
}