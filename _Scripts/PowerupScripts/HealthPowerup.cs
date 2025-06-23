using UnityEngine;
using Unity.Netcode;

public class HealthPowerup : PowerupBase
{
    [Tooltip("Unused")]
    private int fallbackAmount = 30;

    protected override int GetEffectValue()
    {
        // This is not since we do full health from health proxy
        return fallbackAmount;
    }

    [ClientRpc]
    protected override void ApplyPowerupClientRpc(int _, ClientRpcParams clientRpcParams = default)
    {
        var player = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (player == null)
        {
            Debug.LogWarning("HealthPowerup: No local player found.");
            return;
        }

        var proxy = player.GetComponent<HealthProxy>();
        if (proxy != null)
        {
            proxy.FullHeal();
            Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] 🔋 Full heal applied.");

            FadeScreenEffect.Instance?.ShowEffect(Color.green, 0.5f, 1.5f);
        }
        else
        {
            Debug.LogWarning("HealthPowerup: No HealthProxy found on player.");
        }
    }
}
