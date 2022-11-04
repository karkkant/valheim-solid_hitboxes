using UnityEngine;

namespace SolidHitboxes.ConfigSync;
internal static class CommandService
{
    #region Client side
    public static void RPC_ReadSAIFFResponse(long sender, ZPackage pkg)
    {
        if (sender == ZRoutedRpc.instance.GetServerPeerID() && pkg != null && pkg.Size() > 0)
        {
            var newVal = pkg.ReadBool();
            FFDamageHandler.AIFriendlyFireEnabled = newVal;

            Chat.instance.AddString("Server", $"Set AI friendly fire to {(newVal ? "ON" : "OFF")}", Talker.Type.Normal);
        }
    }

    #endregion

    #region Server side

    public static void RPC_RequestAIFFChange(long sender, ZPackage pkg)
    {
        if (!ZNet.m_isServer) return;

        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        string peerSteamID = ((ZSteamSocket)peer.m_socket).GetPeerID().m_SteamID.ToString();

        if (ZNet.m_isServer ||
            ZNet.instance.m_adminList != null &&
            ZNet.instance.m_adminList.Contains(peerSteamID))
        {
            var newVal = pkg.ReadBool();
            pkg.SetPos(0);
            FFDamageHandler.AIFriendlyFireEnabled = newVal;

            // Send to all clients
            ZRoutedRpc.instance.InvokeRoutedRPC(0L, nameof(RPC_ReadSAIFFResponse), new object[] { pkg });
        }
        else
        {
            Debug.Log($"Not authorized to request FF change. SteamId: (${peerSteamID})");
        }
    }

    #endregion
}
