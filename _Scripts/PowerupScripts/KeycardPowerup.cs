using UnityEngine;
using Unity.Netcode;

public class KeycardPowerup : PowerupBase
{
    [Tooltip("Consumable type name (e.g., Keycard, Battery)")]
    [SerializeField] private string consumableType = "Keycard";

    [Tooltip("Amount to add")]
    [SerializeField] private int amount = 1;

    protected override int GetEffectValue() => amount;

    [ClientRpc]
    protected override void ApplyPowerupClientRpc(int effectValue, ClientRpcParams clientRpcParams = default)
    {
        if (ConsumableManager.Instance != null)
        {
            ConsumableManager.Instance.Add(consumableType, effectValue);
        }
        else
        {
            Debug.LogWarning("❌ ConsumableManager.Instance is null!");
        }
    }

}
