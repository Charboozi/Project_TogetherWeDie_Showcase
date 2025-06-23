using Unity.Netcode;
using UnityEngine;

public class GameModeManager : NetworkBehaviour
{
    public static GameModeManager Instance { get; private set; }

    public bool IsPvPMode { get; private set; }

    // ✅ Use a NetworkVariable to sync wager amount
    public NetworkVariable<int> PvPWagerAmount = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public int PvPRewardPot = 0; // Server-only

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetPvPMode(bool isPvP)
    {
        IsPvPMode = isPvP;
        Debug.Log($"🔁 PvP mode set to: {isPvP}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetWagerServerRpc(int amount)
    {
        amount = Mathf.Clamp(amount, 0, 9999);
        PvPWagerAmount.Value = amount;
        Debug.Log($"💰 Wager set to {amount} coins by host.");
    }
}
