// Final Updated LobbyManager.cs with Kick, Loading and Recovery Support
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Netcode.Transports.Facepunch;
using System;
using System.Transactions;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    public LobbyUI lobbyUI;
    public int maxPlayers = 4;

    private Lobby currentLobby;
    private bool isHost;
    private bool isInLobby;

    public event Action OnLobbyUpdated;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeft;

        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        if (!isInLobby && !NetworkManager.Singleton.IsListening)
        {
            PlayerManager.Instance.SpawnOfflinePlayer();
        }
    }

    private void OnDestroy()
    {
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeft;

        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    public async void CreateLobby()
    {
        if (!SteamClient.IsValid)
        {
            Debug.LogError("[LOBBY] Steam is not initialized.");
            return;
        }

        TransitionManager.Instance.ShowLoading();

        var lobbyResult = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);
        if (!lobbyResult.HasValue)
        {
            Debug.LogError("[LOBBY] Failed to create lobby.");
            TransitionManager.Instance.HideLoading();
            return;
        }

        currentLobby = lobbyResult.Value;
        isHost = true;
        isInLobby = true;

        currentLobby.SetPublic();
        currentLobby.SetJoinable(true);
        currentLobby.SetData("name", SteamClient.Name + "'s Lobby");
        currentLobby.SetData("visibility", "public");
        currentLobby.SetData("game", "PlanBInc");
        currentLobby.SetData("host", SteamClient.SteamId.ToString());

        Debug.Log("[LOBBY] Created lobby as host: " + currentLobby.Id);

        PlayerManager.Instance.DestroyOfflinePlayer();

        NetworkManager.Singleton.StartHost();

        lobbyUI.ShowLobbyButtons(true);
        TriggerLobbyUpdate();

        TransitionManager.Instance.HideLoading();
    }

    public async void JoinLobby(SteamId lobbyId)
    {
        Debug.Log("[LOBBY] Attempting to join lobby: " + lobbyId);

        TransitionManager.Instance.ShowLoading();

        if (NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.Shutdown();

        if (currentLobby.Id != 0)
        {
            currentLobby.Leave();
            currentLobby = default;
        }

        await Task.Delay(300);

        var result = await SteamMatchmaking.JoinLobbyAsync(lobbyId);
        if (!result.HasValue)
        {
            Debug.LogError("[LOBBY] Failed to join lobby.");
            TransitionManager.Instance.HideLoading();
            return;
        }

        currentLobby = result.Value;
        isHost = false;
        isInLobby = true;

        Debug.Log("[LOBBY] Joined lobby: " + currentLobby.Id);

        PlayerManager.Instance.DestroyOfflinePlayer();

        var hostIdStr = currentLobby.GetData("host");
        if (ulong.TryParse(hostIdStr, out ulong hostIdValue))
        {
            SteamId hostId = new SteamId { Value = hostIdValue };
            var transport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();
            transport.targetSteamId = hostId;

            NetworkManager.Singleton.StartClient();
        }
        else
        {
            Debug.LogError("[LOBBY] Invalid host ID.");
            TransitionManager.Instance.HideLoading();
            return;
        }

        lobbyUI.ShowLobbyButtons(true);
        TriggerLobbyUpdate();

        TransitionManager.Instance.HideLoading();
    }

    public void LeaveLobby()
    {
        Debug.Log("[LOBBY] Leaving current lobby...");

        if (NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.Shutdown();

        if (currentLobby.Id != 0)
        {
            currentLobby.Leave();
            currentLobby = default;
        }

        isHost = false;
        isInLobby = false;

        lobbyUI.ClearLobbyUI();
        lobbyUI.ShowLobbyButtons(false);

        PlayerManager.Instance.SpawnOfflinePlayer();
        TransitionManager.Instance.HideLoading();
    }

    public void ForceLeaveLobby()
    {
        Debug.Log("[LOBBY] ForceLeaveLobby called.");

        if (NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.Shutdown();

        if (currentLobby.Id != 0)
        {
            currentLobby.Leave();
            currentLobby = default;
        }

        isHost = false;
        isInLobby = false;

        lobbyUI.ClearLobbyUI();
        lobbyUI.ShowLobbyButtons(false);

        if (PlayerManager.Instance != null)
            PlayerManager.Instance.SpawnOfflinePlayer();
        else
            Debug.LogWarning("[LOBBY] PlayerManager.Instance is null when trying to fallback.");

        TransitionManager.Instance.HideLoading();
    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend member)
    {
        if (lobby.Id != currentLobby.Id) return;

        Debug.Log("[LOBBY] Member joined: " + member.Name);
        TriggerLobbyUpdate();
    }

    private void OnLobbyMemberLeft(Lobby lobby, Friend member)
    {
        if (lobby.Id != currentLobby.Id) return;

        Debug.Log("[LOBBY] Member left: " + member.Name);
        TriggerLobbyUpdate();

        if (!isHost && currentLobby.Owner.Id != 0 && member.Id == currentLobby.Owner.Id)
        {
            Debug.LogWarning("[LOBBY] Host left. Returning to offline mode...");
            ForceLeaveLobby();
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"[LOBBY] Client disconnected: {clientId}");

        if (IsHost())
        {
            // Host sees others leave — do nothing
            return;
        }

        // Client disconnected from server (could be kicked or host left)
        if (clientId == NetworkManager.ServerClientId)
        {
            Debug.LogWarning("[LOBBY] Host disconnected. Returning to offline mode...");
            ForceLeaveLobby();
        }
        else if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.LogWarning("[LOBBY] Disconnected. Returning to offline mode...");
            ForceLeaveLobby();
        }
    }

    public void ToggleVisibility()
    {
        if (currentLobby.Id == 0) return;

        string vis = currentLobby.GetData("visibility");
        if (vis == "public")
        {
            currentLobby.SetPrivate();
            currentLobby.SetData("visibility", "private");
        }
        else
        {
            currentLobby.SetPublic();
            currentLobby.SetData("visibility", "public");
        }

        TriggerLobbyUpdate();
    }

    public async void RefreshAvailableLobbies()
    {
        var query = SteamMatchmaking.LobbyList
            .WithSlotsAvailable(1)
            .WithKeyValue("game", "PlanBInc");

        var result = await query.RequestAsync();
        var filtered = result.Where(l => l.Id != currentLobby.Id).ToList();

        Debug.Log("[LOBBY] Found " + filtered.Count + " available lobbies.");
        lobbyUI.ShowAvailableLobbies(filtered);
    }

    public void KickPlayer(SteamId steamId)
    {
        if (!IsHost()) return;

        ulong clientId = PlayerManager.Instance.GetClientId(steamId);
        if (clientId == ulong.MaxValue)
        {
            Debug.LogWarning("[LOBBY] Kick failed. Client not found.");
            return;
        }

        // Find the player's KickManager component on the correct networked player
        foreach (var netObj in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
        {
            if (netObj.OwnerClientId == clientId)
            {
                var kickManager = netObj.GetComponent<KickManager>();
                if (kickManager != null)
                {
                    kickManager.KickClientRpc();
                    Debug.Log($"[LOBBY] Kick RPC sent to {steamId}");
                }
                else
                {
                    Debug.LogWarning($"[LOBBY] KickManager not found on player {steamId}");
                }

                return;
            }
        }

        Debug.LogWarning($"[LOBBY] No NetworkObject found for client {clientId}");
    }


    // Helper (you must map Steam ID ↔ Client ID somewhere like in PlayerManager)
    private ulong GetClientIdFromSteamId(SteamId steamId)
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            if (client.Value.ClientId == NetworkManager.Singleton.LocalClientId) continue;
            if (PlayerManager.Instance.GetSteamId(client.Value.ClientId) == steamId)
                return client.Key;
        }

        return ulong.MaxValue; // not found
    }

    public void OpenInviteOverlay()
    {
        if (currentLobby.Id != 0)
            SteamFriends.OpenGameInviteOverlay(currentLobby.Id);
    }

    public Lobby GetCurrentLobby() => currentLobby;
    public bool IsHost() => isHost;
    public bool IsInLobby() => isInLobby;

    private void TriggerLobbyUpdate()
    {
        lobbyUI.UpdateLobbyInfo(currentLobby);
        lobbyUI.RefreshPlayerList(currentLobby);
        OnLobbyUpdated?.Invoke();
    }
}