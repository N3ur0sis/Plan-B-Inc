using UnityEngine;
using Unity.Netcode;
using Netcode.Transports.Facepunch;
using UnityEngine.UI;
using Steamworks; // Needed for SteamId

public class ConnectUIScript : MonoBehaviour
{
    public Button hostButton;
    public Button clientButton;

    // Replace with your host SteamID (in UInt64 format)
    [Header("Hardcoded Host SteamID")]
    [SerializeField] private string hostSteamIdString = "76561199845644414";

    private void Start()
    {
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void StartHost()
    {
        Debug.Log("[ConnectUI] Starting as Host...");

        bool started = NetworkManager.Singleton.StartHost();
        if (started)
        {
            Debug.Log("[ConnectUI] Host started successfully.");
        }
        else
        {
            Debug.LogError("[ConnectUI] Failed to start host.");
        }
    }

    private void StartClient()
    {
        Debug.Log("[ConnectUI] Starting as Client...");

        // Set target Steam ID for connection
        var transport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();
        if (transport != null)
        {
            if (ulong.TryParse(hostSteamIdString, out ulong steamIdRaw))
            {
                SteamId hostSteamId = new SteamId { Value = steamIdRaw }; // Fixed CS1729 and IDE0090
                transport.targetSteamId = hostSteamId;
                Debug.Log($"[ConnectUI] Connecting to Steam host: {hostSteamId}");
            }
            else
            {
                Debug.LogError("[ConnectUI] Invalid Steam ID format.");
                return;
            }
        }
        else
        {
            Debug.LogError("[ConnectUI] FacepunchTransport not found on NetworkManager.");
            return;
        }

        // Start client
        bool started = NetworkManager.Singleton.StartClient();
        if (started)
        {
            Debug.Log("[ConnectUI] Client started, awaiting connection...");
        }
        else
        {
            Debug.LogError("[ConnectUI] Failed to start client.");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[ConnectUI] Client connected: {clientId}");

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("[ConnectUI] This client is now connected!");

            var playerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            if (playerObj != null)
            {
                Debug.Log("[ConnectUI] Player object is spawned.");
            }
            else
            {
                Debug.LogWarning("[ConnectUI] Player object is NOT spawned yet!");
            }
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.LogWarning($"[ConnectUI] Client disconnected: {clientId}");

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.LogWarning("[ConnectUI] This client was disconnected — likely a timeout or connection error.");
        }
    }
}
