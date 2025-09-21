using System.Runtime.CompilerServices;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace MonkeyWrench;

public static class ItemDataExtensions
{
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
    private static class Humanoid_EquipItem_Patch
    {
        [UsedImplicitly]
        private static void Prefix(Humanoid __instance, ItemDrop.ItemData? item)
        {
            if (__instance is not Player || item == null) return;
            if (item.m_shared.m_itemType is not ItemDrop.ItemData.ItemType.TwoHandedWeapon) return;
            if (item.IsDualItem()) return;
            if (!ZInput.GetKey(KeyCode.LeftAlt)) return;
            CachedData data = item.GetCachedData();
            data.Setup(ref item);
        }
    }

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipItem))]
    private static class Humanoid_UnequipItem_Patch
    {
        [UsedImplicitly]
        private static void Prefix(Humanoid __instance, ItemDrop.ItemData? item)
        {
            if (__instance is not Player || item == null) return;
            CachedData data = item.GetCachedData();
            data.Reset(ref item);
        }
    }
    
    private static readonly ConditionalWeakTable<ItemDrop.ItemData, CachedData> extendedItems = new();
    
    [UsedImplicitly]
    public class CachedData
    {
        private ItemDrop.ItemData.ItemType itemType;
        private ItemDrop.ItemData.ItemType attachOverride;
        private ItemDrop.ItemData.AnimationState AnimState;
        private ItemAttack? attack;
        private ItemAttack? secondary;

        private bool modified;

        private class ItemAttack
        {
            public readonly Attack.AttackType type;
            public readonly string anim;
            public readonly int chain;

            public ItemAttack(Attack attack)
            {
                type = attack.m_attackType;
                anim = attack.m_attackAnimation;
                chain = attack.m_attackChainLevels;
            }
        }

        public void Setup(ref ItemDrop.ItemData item)
        {
            itemType = item.m_shared.m_itemType;
            attachOverride = item.m_shared.m_attachOverride;
            AnimState = item.m_shared.m_animationState;
            attack = new ItemAttack(item.m_shared.m_attack);
            secondary = new ItemAttack(item.m_shared.m_secondaryAttack);
            
            item.m_shared.m_itemType = ItemDrop.ItemData.ItemType.OneHandedWeapon;
            item.m_shared.m_attachOverride = ItemDrop.ItemData.ItemType.OneHandedWeapon;
            item.m_shared.m_animationState = ItemDrop.ItemData.AnimationState.OneHanded;
            switch (item.m_shared.m_skillType)
            {
                case Skills.SkillType.Swords or Skills.SkillType.Polearms:
                    item.m_shared.m_attack.m_attackType = Attack.AttackType.Horizontal;
                    item.m_shared.m_attack.m_attackAnimation = "swing_longsword";
                    item.m_shared.m_attack.m_attackChainLevels = 3;
                    item.m_shared.m_secondaryAttack.m_attackAnimation = "sword_secondary";
                    item.m_shared.m_secondaryAttack.m_attackType = Attack.AttackType.Horizontal;
                    break;
                case Skills.SkillType.Axes:
                    item.m_shared.m_attack.m_attackType = Attack.AttackType.Horizontal;
                    item.m_shared.m_attack.m_attackAnimation = "swing_axe";
                    item.m_shared.m_attack.m_attackChainLevels = 3;
                    item.m_shared.m_secondaryAttack.m_attackAnimation = "axe_secondary";
                    item.m_shared.m_secondaryAttack.m_attackType = Attack.AttackType.Vertical;
                    break;
            }

            modified = true;
        }

        public void Reset(ref ItemDrop.ItemData item)
        {
            if (!modified || attack == null || secondary == null) return;
            item.m_shared.m_itemType = itemType;
            item.m_shared.m_attachOverride = attachOverride;
            item.m_shared.m_animationState = AnimState;
            item.m_shared.m_attack.m_attackType = attack.type;
            item.m_shared.m_attack.m_attackAnimation = attack.anim;
            item.m_shared.m_attack.m_attackChainLevels = attack.chain;
            item.m_shared.m_secondaryAttack.m_attackType = secondary.type;
            item.m_shared.m_secondaryAttack.m_attackAnimation = secondary.anim;
            item.m_shared.m_secondaryAttack.m_attackChainLevels = secondary.chain;
            modified = false;
            attack = null;
            secondary = null;
        }
    }
    public static bool IsHarpoon(this ItemDrop.ItemData itemData) => itemData.m_shared.m_name == "$item_spear_chitin";
    public static bool IsDualItem(this ItemDrop.ItemData itemData) => IsDualItem(itemData.m_shared.m_name);
    public static bool IsDualItem(string name) => name.StartsWith("Dual") || IsBerzekr(name) || IsSkollAndHati(name);
    public static bool IsBerzekr(string name) => name.StartsWith("AxeBerzerkr") || name.StartsWith("$item_axe_berzerkr") ||  name is "AxeBerzerkr";
    public static bool IsSkollAndHati(string name) => name is "KnifeSkollAndHati" or "$item_knife_skollandhati";
    public static CachedData GetCachedData(this ItemDrop.ItemData item) => extendedItems.GetOrCreateValue(item);
}