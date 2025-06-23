using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;
using Unity.Netcode;
using Netcode.Transports.Facepunch;
using Steamworks.Data;
using System;
using System.Threading.Tasks;

public class SteamOverlayJoinHandler : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject lobbyPanelPVP;
    [SerializeField] private GameObject lobbyPanelCOOP;

    [Header("Debug Text (optional)")]
    [SerializeField] private TMP_Text statusText;

    private void Start()
    {
        if (!SteamClient.IsValid)
        {
            Debug.LogError("❌ Steam not initialized.");
            return;
        }

        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested += OnOverlayJoinRequested;

        LogStatus("📡 Steam overlay join handler ready.");

        // ✅ Fallback: parse Steam lobby ID from command line if launched via overlay
        string[] args = Environment.GetCommandLineArgs();
        foreach (var arg in args)
        {
            if (ulong.TryParse(arg, out ulong lobbyId))
            {
                LogStatus($"🛰 Auto-joining lobby from launch args: {lobbyId}");
                _ = SteamMatchmaking.JoinLobbyAsync(lobbyId);
                break;
            }
        }
    }

    private async void OnOverlayJoinRequested(Lobby lobby, SteamId friendId)
    {
        LogStatus($"📨 Received join request via overlay from: {friendId}");
        await SteamMatchmaking.JoinLobbyAsync(lobby.Id);
    }

    private async void OnLobbyEntered(Lobby lobby)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            LogStatus("👑 Host entered their own lobby – skipping client join logic.");
            return;
        }

        LogStatus($"✅ Joined Steam lobby: {lobby.Id}");

        // Extract hostId from lobby metadata
        string hostIdStr = lobby.GetData("hostId");
        if (string.IsNullOrEmpty(hostIdStr) || !ulong.TryParse(hostIdStr, out ulong hostId))
        {
            LogStatus("❌ Invalid or missing hostId.");
            return;
        }

        // Extract game mode
        string gameMode = lobby.GetData("gameMode");
        if (string.IsNullOrEmpty(gameMode))
        {
            LogStatus("❌ Missing game mode in lobby metadata.");
            return;
        }

        // Update UI based on game mode
        lobbyPanelPVP?.SetActive(gameMode == "PvP");
        lobbyPanelCOOP?.SetActive(gameMode == "CoOp");

        // Setup Facepunch Transport
        FacepunchTransport transport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();
        transport.targetSteamId = hostId;
        Debug.Log($"🎯 Target Steam ID set: {hostId}");

        // Register disconnect callback early
        NetworkManager.Singleton.OnClientDisconnectCallback += (clientId) =>
        {
            Debug.Log($"❌ Client disconnected. ID: {clientId}");

            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                Debug.Log("🛑 YOU (the client) got disconnected. Possible bad hostId or Steam relay issue.");
            }
        };

        await Task.Delay(250); // Slight delay to ensure transport is ready

        NetworkManager.Singleton.StartClient();

        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            LogStatus($"✅ Connected to host! Client ID: {clientId}");
        };

        mainMenuPanel?.SetActive(false);
    }

    private void LogStatus(string message)
    {
        Debug.Log(message);
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}
