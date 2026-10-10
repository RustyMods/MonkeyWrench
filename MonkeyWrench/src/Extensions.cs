namespace MonkeyWrench;

public static class Extensions
{
    public static bool IsMonkeyWrenched(this Humanoid humanoid)
    {
        if (humanoid is not Player player) return false;
        return player.m_customData.TryGetValue("RustyMods.MonkeyWrenched", out var value) &&
               bool.TryParse(value, out var result) && 
               result;
    }

    public static void SetMonkeyWrenched(this Humanoid humanoid, bool value)
    {
        if (humanoid is not Player player) return;
        player.m_customData["RustyMods.MonkeyWrenched"] = value.ToString();
    }

    public static bool CanBeMonkeyWrenched(this ItemDrop.ItemData item)
    {
        if (item.m_shared.m_animationState is not 
            (ItemDrop.ItemData.AnimationState.Greatsword or 
            ItemDrop.ItemData.AnimationState.TwoHandedAxe or 
            ItemDrop.ItemData.AnimationState.TwoHandedClub))
        {
            return false;
        }

        var blacklist = MonkeyWrenchPlugin.Blacklist;
        if (blacklist.Count == 0) return true;
        if (item.m_dropPrefab == null) return false;
        
        return !blacklist.Contains(item.m_dropPrefab.name);
    }
}