using HarmonyLib;

namespace MonkeyWrench;

[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
public static class Allow_Shield_Patch
{
    private static bool Prefix(Humanoid __instance, ItemDrop.ItemData item, bool triggerEquipEffects, ref bool __result)
    {
        if (!__instance.IsMonkeyWrenched()) return true;
        
        if (__instance.m_rightItem == null) return true;

        if (__instance.m_rightItem.m_shared.m_animationState is
            not (ItemDrop.ItemData.AnimationState.Greatsword
            or ItemDrop.ItemData.AnimationState.TwoHandedAxe or
            ItemDrop.ItemData.AnimationState.TwoHandedClub))
        {
            return true;
        }

        if (item.m_shared.m_itemType is not ItemDrop.ItemData.ItemType.Shield) return true;
        
        
        if (__instance.IsItemEquiped(item) || !__instance.m_inventory.ContainsItem(item) || __instance.InAttack() ||
            __instance.InDodge() || __instance.IsDead() || __instance.IsSwimming() || !__instance.IsOnGround() ||
            (item.m_shared.m_useDurability && item.m_durability <= 0.0)) return true;
            
        if (item.m_shared.m_dlc.Length > 0 && !DLCMan.instance.IsDLCInstalled(item.m_shared.m_dlc)) return true;

        if (Game.m_worldLevel > 0 && item.m_worldLevel < Game.m_worldLevel &&
            (item.m_shared.m_itemType is ItemDrop.ItemData.ItemType.Trinket or ItemDrop.ItemData.ItemType.Utility))
        {
            return true;
        }

        if (__instance.m_leftItem != null)
        {
            __instance.UnequipItem(__instance.m_leftItem, triggerEquipEffects);
        }

        __instance.m_leftItem = item;
        __instance.m_leftItem.m_equipped = true;
        
        item.m_shared.m_equipEffect.Create(__instance.m_visEquipment.m_leftHand.position,
            __instance.m_visEquipment.m_leftHand.rotation);

        __instance.m_hiddenLeftItem = null;

        __instance.SetupEquipment();
        if (triggerEquipEffects) __instance.TriggerEquipEffect(item);
        
        __result = true;
        return false;
    }
}