using UnityEngine;
using Unity.Netcode;

public class KickManager : NetworkBehaviour
{
    [ClientRpc]
    public void KickClientRpc()
    {
        Debug.Log("[KICK] You have been kicked.");
        LobbyManager.Instance.ForceLeaveLobby(); // This works because it's local
    }
}
