using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;

public class LobbyUI : MonoBehaviour
{
    public TextMeshProUGUI lobbyNameText;
    public TextMeshProUGUI visibilityText;
    public Transform playerListContainer;
    public GameObject playerEntryPrefab;

    public Button createLobbyButton;
    public Button leaveLobbyButton;
    public Toggle toggleVisibilityButton;
    public Button inviteButton;
    public Button refreshLobbyListButton;
    public Transform availableLobbyListContainer;
    public GameObject availableLobbyEntryPrefab;

    void Start()
    {
        createLobbyButton.onClick.RemoveAllListeners();
        leaveLobbyButton.onClick.RemoveAllListeners();
        toggleVisibilityButton.onValueChanged.RemoveAllListeners();
        inviteButton.onClick.RemoveAllListeners();
        refreshLobbyListButton.onClick.RemoveAllListeners();

        createLobbyButton.onClick.AddListener(() => {
            Debug.Log("[UI] Create Lobby clicked.");
            LobbyManager.Instance.CreateLobby();
        });

        leaveLobbyButton.onClick.AddListener(() => {
            Debug.Log("[UI] Leave Lobby clicked.");
            LobbyManager.Instance.LeaveLobby();
        });

        toggleVisibilityButton.onValueChanged.AddListener(_ => {
            Debug.Log("[UI] Toggle Visibility clicked.");
            LobbyManager.Instance.ToggleVisibility();
        });

        inviteButton.onClick.AddListener(() => {
            Debug.Log("[UI] Invite button clicked.");
            LobbyManager.Instance.OpenInviteOverlay();
        });

        refreshLobbyListButton.onClick.AddListener(() => {
            Debug.Log("[UI] Refresh Lobby List clicked.");
            LobbyManager.Instance.RefreshAvailableLobbies();
        });

        ShowLobbyButtons(false);
    }

    void OnEnable()
    {
        if (LobbyManager.Instance != null)
            LobbyManager.Instance.OnLobbyUpdated += RefreshUI;
    }

    void OnDisable()
    {
        if (LobbyManager.Instance != null)
            LobbyManager.Instance.OnLobbyUpdated -= RefreshUI;
    }

    void RefreshUI()
    {
        var lobby = LobbyManager.Instance.GetCurrentLobby();
        UpdateLobbyInfo(lobby);
        RefreshPlayerList(lobby);
    }

    public void UpdateLobbyInfo(Lobby? lobby)
    {
        if (!lobby.HasValue) return;
        lobbyNameText.text = lobby.Value.GetData("name");
        visibilityText.text = lobby.Value.GetData("visibility").ToUpperInvariant();
    }

    public void RefreshPlayerList(Lobby? lobby)
    {
        foreach (Transform child in playerListContainer)
            Destroy(child.gameObject);

        if (!lobby.HasValue) return;

        foreach (var member in lobby.Value.Members)
        {
            GameObject entry = Instantiate(playerEntryPrefab, playerListContainer);
            entry.GetComponentInChildren<TextMeshProUGUI>().text = member.Name;

            Button kickButton = entry.GetComponentInChildren<Button>();
            bool isHost = lobby.Value.Owner.Id == SteamClient.SteamId;
            kickButton.gameObject.SetActive(isHost && member.Id != SteamClient.SteamId);
            kickButton.onClick.AddListener(() => LobbyManager.Instance.KickPlayer(member.Id));
        }
    }

    public void ShowAvailableLobbies(List<Lobby> lobbies)
    {
        foreach (Transform child in availableLobbyListContainer)
            Destroy(child.gameObject);

        foreach (var lobby in lobbies)
        {
            GameObject entry = Instantiate(availableLobbyEntryPrefab, availableLobbyListContainer);
            entry.GetComponentInChildren<TextMeshProUGUI>().text = $"{lobby.GetData("name")} [{lobby.MemberCount}/{lobby.MaxMembers}]";
            entry.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                LobbyManager.Instance.JoinLobby(lobby.Id);
            });
        }
    }

    public void ClearLobbyUI()
    {
        lobbyNameText.text = "";
        visibilityText.text = "";
        foreach (Transform child in playerListContainer) Destroy(child.gameObject);
        foreach (Transform child in availableLobbyListContainer) Destroy(child.gameObject);
    }

    public void ShowLobbyButtons(bool isInLobby)
    {
        leaveLobbyButton.gameObject.SetActive(isInLobby);
        toggleVisibilityButton.gameObject.SetActive(isInLobby);
        inviteButton.gameObject.SetActive(isInLobby);
        createLobbyButton.gameObject.SetActive(!isInLobby);
    }
}
