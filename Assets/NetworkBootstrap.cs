using UnityEngine;
using Unity.Netcode;

public class NetworkBootstrap : MonoBehaviour
{
    void Start()
    {
        if (!NetworkManager.Singleton.IsListening)
        {
            Debug.Log("[BOOT] Starting as Host...");
            NetworkManager.Singleton.StartHost();
        }
    }
}
