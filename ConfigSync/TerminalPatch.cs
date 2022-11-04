using HarmonyLib;
using System;

namespace SolidHitboxes.ConfigSync;

[HarmonyPatch(typeof(Terminal))]
internal class TerminalPatch
{
    [HarmonyPatch(nameof(Terminal.TryRunCommand))]
    [HarmonyPrefix]
    private static bool TryRunCommandPatch(Terminal __instance, string text)
    {
        if (!text.ToLower().StartsWith("ai-ff"))
        {
            return true;
        }

        string[] args = text.Split(' ');

        if (args.Length < 2)
        {
            __instance.AddString("Toggles AI friendly fire.");
            __instance.AddString("Usage: /ai-ff on|off");
            return false;
        }

        if (args[1].Equals("on", StringComparison.InvariantCultureIgnoreCase))
        {
            HandleCommand(CreatePkg(true));
        }
        else if (args[1].Equals("off", StringComparison.InvariantCultureIgnoreCase))
        {
            HandleCommand(CreatePkg(false));
        } else
        {
            __instance.AddString("Invalid value");
        }

        return false;
    }

    private static void HandleCommand(ZPackage pkg)
    {
        if(ZNet.m_isServer)
        {
            // Send to all clients
            ZRoutedRpc.instance.InvokeRoutedRPC(0L, nameof(CommandService.RPC_ReadSAIFFResponse), new object[] { pkg});
        } else
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), nameof(CommandService.RPC_RequestAIFFChange), new object[] { pkg });
        }
    }

    private static ZPackage CreatePkg(bool newVal)
    {
        var pkg = new ZPackage();
        pkg.Write(newVal);
        return pkg;
    }
}
