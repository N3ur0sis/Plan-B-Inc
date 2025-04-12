using UnityEngine;
using Unity.Netcode;

public class KickManager : NetworkBehaviour
{
    [ClientRpc]
    public void KickClientRpc()
    {
        Debug.Log("[KICK] You have been kicked.");

        // Leave the Netcode session and Steam lobby — but DO NOT touch the host or full shutdown
        LobbyManager.Instance.ForceLeaveLobby();
    }
}
