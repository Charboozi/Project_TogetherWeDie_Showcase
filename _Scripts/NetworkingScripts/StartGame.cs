using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class StartGame : NetworkBehaviour
{
    [SerializeField] private GameObject lobbyPanelPVP;
    [SerializeField] private GameObject lobbyPanelCOOP;
    [SerializeField] private GameObject mainMenuPanel;


    public void OnStartClicked()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("Only host can start the game.");
            return;
        }

        // ✅ PvP mode requires at least 2 players
        if (GameModeManager.Instance.IsPvPMode && NetworkManager.Singleton.ConnectedClients.Count <= 1)
        {
            Debug.LogWarning("🚫 Cannot start PvP game with only one player in the lobby.");
            return;
        }

        GameOverManager.Instance?.ClearEliminationOrder();

        // ✅ Sync PvP flag to clients
        SyncPvPFlagClientRpc(GameModeManager.Instance.IsPvPMode);

        // ✅ Deduct wagers before switching scene
        DeductPvPWagers();

        // ✅ Load the game scene
        string selectedScene = MapManager.Instance.CurrentMapName;
        Debug.Log($"▶ Loading selected map: {selectedScene}");

        NetworkManager.Singleton.SceneManager.LoadScene(
            selectedScene,
            LoadSceneMode.Single
        );
    }

    [ClientRpc]
    private void SyncPvPFlagClientRpc(bool isPvP)
    {
        GameModeManager.Instance.SetPvPMode(isPvP);
        Debug.Log($"🛰 Synced PvP flag to client. PvP mode = {isPvP}");
    }

    public void DeductPvPWagers()
    {
        if (!IsServer || !GameModeManager.Instance.IsPvPMode)
            return;

        int wager = GameModeManager.Instance.PvPWagerAmount.Value;

        // ✅ Server (host) spends their own wager
        if (CurrencyManager.Instance != null)
        {
            bool success = CurrencyManager.Instance.Spend(wager);
            if (!success)
            {
                Debug.LogError("🚫 Host does not have enough coins to pay wager. Cancelling game start.");
                return; // Optionally kick to menu or prevent start
            }
        }

        // ✅ Ask all clients to deduct their wagers
        RequestClientWagerDeductionClientRpc(wager);

        GameModeManager.Instance.PvPRewardPot = wager * NetworkManager.Singleton.ConnectedClients.Count;
        Debug.Log($"💰 PvP Pot collected: {GameModeManager.Instance.PvPRewardPot}");
    }

    [ClientRpc]
    private void RequestClientWagerDeductionClientRpc(int wager)
    {
        if (IsHost) return; // Host already processes game logic

        if (CurrencyManager.Instance != null)
        {
            bool success = CurrencyManager.Instance.Spend(wager);

            if (!success)
            {
                Debug.LogError("🚫 Not enough coins to pay wager. Leaving lobby...");
                RequestKickFromServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestKickFromServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        Debug.LogWarning($"❌ Kicking client {clientId} due to insufficient funds.");

        NetworkManager.Singleton.DisconnectClient(clientId);
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("👋 Disconnected from host. Returning to main menu.");
            lobbyPanelPVP.SetActive(false);
            lobbyPanelCOOP.SetActive(false);
            mainMenuPanel.SetActive(true);
        }
    }
}
