using UnityEngine;
using TMPro;
using Unity.Netcode;
using Steamworks;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject offlinePlayerPrefab;
    public GameObject nameTagPrefab;

    private GameObject currentOfflinePlayer;

    // Mappings between SteamId and Netcode ClientId
    private Dictionary<ulong, SteamId> clientToSteamMap = new();
    private Dictionary<SteamId, ulong> steamToClientMap = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += RegisterClient;
        NetworkManager.Singleton.OnClientDisconnectCallback += UnregisterClient;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= RegisterClient;
            NetworkManager.Singleton.OnClientDisconnectCallback -= UnregisterClient;
        }
    }

    #region Spawn / Destroy Offline Player

    public void SpawnOfflinePlayer()
    {
        if (offlinePlayerPrefab == null) return;
        if (currentOfflinePlayer == null)
        {
            Debug.Log("[PLAYER MANAGER] Spawning offline player.");
            currentOfflinePlayer = Instantiate(offlinePlayerPrefab, new Vector3(0, 0.8f, 0), Quaternion.identity);

            if (nameTagPrefab != null)
            {
                GameObject tag = Instantiate(nameTagPrefab, currentOfflinePlayer.transform);
                tag.transform.localPosition = new Vector3(0, 2.2f, 0);
                tag.GetComponentInChildren<TextMeshPro>().text = SteamClient.Name + " (Offline)";
                Debug.Log("[PLAYER MANAGER] Attached name tag to offline player.");
            }
        }
    }

    public void DestroyOfflinePlayer()
    {
        if (currentOfflinePlayer != null)
        {
            Debug.Log("[PLAYER MANAGER] Destroying offline player.");
            Destroy(currentOfflinePlayer);
            currentOfflinePlayer = null;
        }
    }

    public bool IsOfflinePlayer(GameObject obj)
    {
        return obj == currentOfflinePlayer;
    }

    #endregion

    #region Mapping Registration

    private void RegisterClient(ulong clientId)
    {
        if (clientToSteamMap.ContainsKey(clientId)) return;

        SteamId steamId = SteamClient.SteamId; // fallback
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            steamId = SteamClient.SteamId;
        }
        else
        {
            Debug.LogWarning($"[PLAYER MANAGER] No SteamId sync logic for client {clientId}. Defaulting.");
        }

        clientToSteamMap[clientId] = steamId;
        steamToClientMap[steamId] = clientId;

        Debug.Log($"[PLAYER MANAGER] Registered client {clientId} <-> SteamId {steamId}");
    }

    private void UnregisterClient(ulong clientId)
    {
        if (clientToSteamMap.TryGetValue(clientId, out SteamId steamId))
        {
            clientToSteamMap.Remove(clientId);
            steamToClientMap.Remove(steamId);
            Debug.Log($"[PLAYER MANAGER] Unregistered client {clientId} <-> SteamId {steamId}");
        }
    }

    #endregion

    #region Accessors

    public SteamId GetSteamId(ulong clientId)
    {
        if (clientToSteamMap.TryGetValue(clientId, out var id))
            return id;

        Debug.LogWarning($"[PLAYER MANAGER] SteamId not found for client {clientId}");
        return default;
    }

    public ulong GetClientId(SteamId steamId)
    {
        if (steamToClientMap.TryGetValue(steamId, out var id))
            return id;

        Debug.LogWarning($"[PLAYER MANAGER] ClientId not found for SteamId {steamId}");
        return ulong.MaxValue;
    }

    #endregion

    #region Name Tag

    public void AttachNameTag(NetworkObject netObj, string playerName)
    {
        if (nameTagPrefab == null || netObj == null)
        {
            Debug.LogWarning("[PLAYER MANAGER] Can't attach name tag: Missing prefab or netObj");
            return;
        }

        GameObject tag = Instantiate(nameTagPrefab, netObj.transform);
        tag.transform.localPosition = new Vector3(0, 2.2f, 0); // Adjust height
        tag.GetComponentInChildren<TextMeshPro>().text = playerName;
        Debug.Log($"[PLAYER MANAGER] Set name tag: {playerName}");
    }

    #endregion
}