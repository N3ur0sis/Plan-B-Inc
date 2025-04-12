public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    public GameObject offlinePlayerPrefab;
    public GameObject nameTagPrefab;
    private GameObject currentOfflinePlayer;

    private Dictionary<ulong, SteamId> clientToSteamIdMap = new Dictionary<ulong, SteamId>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SpawnOfflinePlayer()
    {
        // Implementation for spawning offline player  
    }

    public void DestroyOfflinePlayer()
    {
        // Implementation for destroying offline player  
    }

    public bool IsOfflinePlayer(GameObject obj)
    {
        // Implementation for checking offline player  
        return false;
    }

    public void AttachNameTag(NetworkObject netObj, string playerName)
    {
        // Implementation for attaching name tag  
    }

    // Adding the missing GetSteamId method  
    public SteamId GetSteamId(ulong clientId)
    {
        if (clientToSteamIdMap.TryGetValue(clientId, out SteamId steamId))
        {
            return steamId;
        }

        Debug.LogWarning($"[PlayerManager] SteamId not found for ClientId: {clientId}");
        return default;
    }

    // Optional: Add a method to map ClientId to SteamId  
    public void MapClientToSteamId(ulong clientId, SteamId steamId)
    {
        if (!clientToSteamIdMap.ContainsKey(clientId))
        {
            clientToSteamIdMap[clientId] = steamId;
        }
    }
}
