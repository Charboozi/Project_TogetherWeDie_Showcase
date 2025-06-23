using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class LeaveSessionButton : MonoBehaviour
{
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(FullyResetGame);
    }

    private void FullyResetGame()
    {
        Debug.Log("🔄 Full reset initiated...");

        // ✅ Reset any lobby state
        if (LobbyPlayerList.Instance != null)
        {
            LobbyPlayerList.Instance.ResetLobbyState();
        }

        // ✅ Reset high-level singletons manually (if needed)
        DestroyIfExists("SteamManager");
        DestroyIfExists("GameOverManager");
        DestroyIfExists("MapManager");
        DestroyIfExists("DayManager");
        DestroyIfExists("GameModeManager");
        DestroyIfExists("HighScoreManager");
        DestroyIfExists("CurrencyManager");
        DestroyIfExists("LobbyPlayerList");
        DestroyIfExists("PlayerListUI");

        // ✅ Disconnect from network
        if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost))
        {
            NetworkManager.Singleton.Shutdown();
        }

        // ✅ Unload NetworkManager (optional if persistent)
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }

        // ✅ Load main menu scene from scratch
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    private void DestroyIfExists(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null)
        {
            Destroy(obj);
            Debug.Log($"☠️ Destroyed: {name}");
        }
    }
}
