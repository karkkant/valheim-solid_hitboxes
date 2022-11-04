using System;
using HarmonyLib;

namespace SolidHitboxes.ConfigSync;

[HarmonyPatch(typeof(Game), nameof(Game.Start))]
public static class GameStartPatch
{
    [HarmonyPrefix]
    private static void RegisterCustomRPC()
    {
        ZRoutedRpc.instance.Register(nameof(CommandService.RPC_RequestAIFFChange), new Action<long, ZPackage>(CommandService.RPC_RequestAIFFChange));
        ZRoutedRpc.instance.Register(nameof(CommandService.RPC_ReadSAIFFResponse), new Action<long, ZPackage>(CommandService.RPC_ReadSAIFFResponse));
    }
}