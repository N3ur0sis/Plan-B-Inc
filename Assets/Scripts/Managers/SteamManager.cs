using UnityEngine;
using Steamworks;

public class SteamManager : MonoBehaviour
{
    public static SteamManager Instance { get; private set; }

    [Header("Steam Settings")]
    public uint appId = 480;

    public string PlayerName => SteamClient.Name;
    public SteamId PlayerId => SteamClient.SteamId;
    public bool SteamInitialized => isInitialized;

    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (SteamClient.IsValid) return; // Already initialized

        try
        {
#if UNITY_EDITOR
            SteamClient.Init(appId, true);
#else
            SteamClient.Init(appId);
#endif
            isInitialized = true;
            Debug.Log($"[STEAM] Initialized as {PlayerName} ({PlayerId})");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[STEAM] Failed to initialize: " + e.Message);
        }
    }

    private void Update()
    {
        if (isInitialized)
            SteamClient.RunCallbacks();
    }

    private void OnApplicationQuit()
    {
        if (isInitialized)
            SteamClient.Shutdown();
    }
}
