using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using ServerSync;
using UnityEngine;

namespace MonkeyWrench
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class MonkeyWrenchPlugin : BaseUnityPlugin
    {
        internal const string ModName = "MonkeyWrench";
        internal const string ModVersion = "1.0.2";
        internal const string Author = "RustyMods";
        private const string ModGUID = Author + "." + ModName;
        private static readonly string ConfigFileName = ModGUID + ".cfg";
        private static readonly string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);
        public static readonly ManualLogSource MonkeyWrenchLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        private static readonly ConfigSync ConfigSync = new(ModGUID) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        public enum Toggle { On = 1, Off = 0 }
        private static ConfigEntry<Toggle> _serverConfigLocked = null!;

        private static ConfigEntry<KeyboardShortcut> _switchKey = null!;
        private static ConfigEntry<string> _blacklist = null!;
        private static ConfigEntry<float> _staminaModifier = null!;
        private static ConfigEntry<float> _damageModifier = null!;

        public static KeyboardShortcut SwitchKey => _switchKey.Value;
        public static List<string> Blacklist => new StringListConfig(_blacklist.Value).list;
        public static float StaminaModifier => 1 + _staminaModifier.Value;
        public static float DamageModifier => _damageModifier.Value;


        public void Awake()
        {
            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On, "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);


            _switchKey = config("2 - Settings", "Switch Key", new KeyboardShortcut(KeyCode.Y),
                new ConfigDescription("Set key to switch between one-handed and two-handed", null, new AcceptableShortcuts()));
            _blacklist = config("2 - Settings", "Blacklist", "",
                new ConfigDescription("List of items that cannot be monkey wrenched", null,
                    StringListConfig.attributes));

            _staminaModifier = config("2 - Settings", "Stamina Use Increase", 0f,
                new ConfigDescription("Set attack stamina use multiplier while monkey wrenched", null,
                    new AcceptableValueRange<float>(0f, 1f)));
            _damageModifier = config("2 - Settings", "Damage Reduction", 0f,
                new ConfigDescription("Set damage reduction while monkey wrenched", null, new AcceptableValueRange<float>(0f, 1f)));
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }

        private void OnDestroy()
        {
            Config.Save();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                MonkeyWrenchLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                MonkeyWrenchLogger.LogError($"There was an issue loading your {ConfigFileName}");
                MonkeyWrenchLogger.LogError("Please check your config entries for spelling and format!");
            }
        }
        
        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription = new(description.Description + (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"), description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        public class ConfigurationManagerAttributes
        {
            [UsedImplicitly] public int? Order = null!;
            [UsedImplicitly] public bool? Browsable = null!;
            [UsedImplicitly] public string? Category = null!;
            [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer = null!;
        }
        class AcceptableShortcuts : AcceptableValueBase
        {
            public AcceptableShortcuts() : base(typeof(KeyboardShortcut))
            {
            }

            public override object Clamp(object value) => value;
            public override bool IsValid(object value) => true;

            public override string ToDescriptionString() => "# Acceptable values: " + string.Join(", ", UnityInput.Current.SupportedKeyCodes);
        }
    }

    public static class KeyboardExtensions
    {
        public static bool IsKeyDown(this KeyboardShortcut shortcut)
        {
            return shortcut.MainKey != KeyCode.None && Input.GetKeyDown(shortcut.MainKey) && shortcut.Modifiers.All(Input.GetKey);
        }

        public static bool IsKeyHeld(this KeyboardShortcut shortcut)
        {
            return shortcut.MainKey != KeyCode.None && Input.GetKey(shortcut.MainKey) && shortcut.Modifiers.All(Input.GetKey);
        }
    }
}