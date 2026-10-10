using BepInEx;
using BepInEx.Configuration;

namespace MonkeyWrench;

public class AcceptableShortcuts : AcceptableValueBase
{
    public AcceptableShortcuts() : base(typeof(KeyboardShortcut))
    {
    }

    public override object Clamp(object value)
    {
        return value;
    }

    public override bool IsValid(object value)
    {
        return true;
    }

    public override string ToDescriptionString()
    {
        return "# Acceptable values: " + string.Join(", ", UnityInput.Current.SupportedKeyCodes);
    }
}