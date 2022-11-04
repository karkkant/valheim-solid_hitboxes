using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace SolidHitboxes
{
    [BepInPlugin("org.bepinex.plugins.solid-hitboxes", "Solid Hitboxes", "1.0.0")]
    [BepInProcess("Valheim.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony _harmony = new Harmony("org.bepinex.plugins.solid-hitboxes");
        private ConfigEntry<bool> EnableFriendlyFire;

        private void Awake()
        {
            EnableFriendlyFire = Config.Bind("General", "EnableFriendlyFire", false, "Whether or not AI can damage their friends");
            FFDamageHandler.AIFriendlyFireEnabled = EnableFriendlyFire.Value;
            
            _harmony.PatchAll();
        }
    }
}
