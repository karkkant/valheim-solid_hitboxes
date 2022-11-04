using HarmonyLib;
using UnityEngine;

namespace SolidHitboxes.ConfigSync
{
    [HarmonyPatch(typeof(ZNet))]
    public class ConfigSync
    {
        [HarmonyPatch("OnNewConnection")]
        [HarmonyPostfix]
        private static void SyncConfigs(ZNet __instance, ZNetPeer peer)
        {
            if (!ZNet.instance.IsServer()) return;

            Debug.Log($"Sending server configuration for peer: {peer.m_rpc}");

            var pkg = new ZPackage();
            pkg.Write(FFDamageHandler.AIFriendlyFireEnabled);

            ZRoutedRpc.instance.InvokeRoutedRPC(peer.m_uid, nameof(CommandService.RPC_ReadSAIFFResponse), new object[] { pkg });
        }
    }
}
