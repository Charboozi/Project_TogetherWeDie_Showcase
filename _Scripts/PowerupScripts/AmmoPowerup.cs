using UnityEngine;
using Unity.Netcode;

public class AmmoPowerup : PowerupBase
{
    [Tooltip("Unused now - value is taken from weapon's max ammo")]
    public int fallbackAmount = 30;

    protected override int GetEffectValue()
    {
        // This is not used anymore since ammo amount is decided on client
        return fallbackAmount;
    }

    // On the client, find the local WeaponStats and add the ammo.
    [ClientRpc]
    protected override void ApplyPowerupClientRpc(int effectValue, ClientRpcParams clientRpcParams = default)
    {
        WeaponBase weapon = CurrentWeaponHolder.Instance.CurrentWeapon;
        if (weapon != null)
        {
            int amountToGive = weapon.maxAmmo * AmmoMultiplierManager.Instance.AmmoMultiplier;;
            weapon.reserveAmmo += amountToGive;
            Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] Ammo powerup applied: +{effectValue} reserve ammo.");
        }
        else
        {
            Debug.LogWarning("WeaponStats not found on this client.");
        }
    }
}
