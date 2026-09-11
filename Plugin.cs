using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;

namespace SolidHitboxes
{
    [BepInPlugin(pluginId, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string pluginId = "org.bepinex.plugins.solid-hitboxes";
        private const string pluginName = "Solid Hitboxes";
        private const string pluginVersion = "1.0.7";

        private readonly Harmony _harmony = new Harmony(pluginId);

        ConfigSync configSync = new ConfigSync(pluginId) { DisplayName = pluginName, CurrentVersion = pluginVersion, MinimumRequiredVersion = pluginVersion };
        private static ConfigEntry<bool> LockConfig;
        private static ConfigEntry<bool> EnableFriendlyFire;
        private static ConfigEntry<float> FFDamageModifierConfig;
        internal static float FFDamageModifier => FFDamageModifierConfig.Value;

        public static bool FFEnabled => EnableFriendlyFire.Value;

        private void Awake()
        {
            LockConfig = config("General", "LockConfig", true, "If on, the configuration is locked and can be changed by server admins only. [Synced with server]");
            EnableFriendlyFire = config("General", "EnableFriendlyFire", false, "Whether or not AI can damage their friends");
            FFDamageModifierConfig = config("General", "FriendlyFireDamageModifier", 1f,
                new ConfigDescription("Damage multiplier applied when AI deal friendly fire damage to each other (1 = full damage, 0 = no damage, 0.5 = half damage, etc).",
                new AcceptableValueRange<float>(0f, 5f)));

            _harmony.PatchAll();
        }

        #region ServerSync

        ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => config(group, name, value, new ConfigDescription(description), synchronizedSetting);

        #endregion
    }
}
