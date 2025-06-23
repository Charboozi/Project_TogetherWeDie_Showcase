using UnityEngine;
using Steamworks;

public class SteamManager : MonoBehaviour
{
    public static SteamManager Instance { get; private set; }

    [Header("Steam Settings")]
    [Tooltip("480 = Spacewar (test AppID). Replace with your real Steam App ID for release.")]
    public uint steamAppId = 480;

    public bool IsSteamInitialized { get; private set; } = false;
    public string FallbackName => "DemoPlayer";
    public ulong FallbackSteamId => 999999999;

    private void Awake()
    {
        // Singleton protection
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ✅ Only call Init if SteamClient is NOT already valid
        if (SteamClient.IsValid)
        {
            Debug.Log("ℹ️ SteamClient already initialized. Skipping Init.");
            IsSteamInitialized = true;
            return;
        }

        try
        {
            SteamClient.Init(steamAppId, true);
            IsSteamInitialized = SteamClient.IsValid;

            if (IsSteamInitialized)
                Debug.Log("✅ SteamClient initialized (Facepunch)");
            else
                Debug.LogWarning("⚠️ SteamClient not valid after init attempt. Using fallback.");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("⚠️ Steam initialization failed. Running in offline mode. Reason: " + ex.Message);
            IsSteamInitialized = false;
        }
    }

    private void OnApplicationQuit()
    {
        if (SteamClient.IsValid)
        {
            SteamClient.Shutdown();
            Debug.Log("🛑 SteamClient shutdown");
        }
    }

    public string GetPlayerName() =>
        IsSteamInitialized ? SteamClient.Name : FallbackName;

    public ulong GetSteamId() =>
        IsSteamInitialized ? SteamClient.SteamId : FallbackSteamId;
}
