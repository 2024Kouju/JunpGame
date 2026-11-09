using UnityEngine;
using Unity.Netcode;

public class LobbyPlayer : NetworkBehaviour
{
    public NetworkVariable<bool> IsReady =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Host‚ÍÅ‰‚©‚ç€”õŠ®—¹
            IsReady.Value = IsOwner;
        }
    }

    [ServerRpc]
    public void SetReadyServerRpc(bool ready)
    {
        IsReady.Value = ready;
    }
}