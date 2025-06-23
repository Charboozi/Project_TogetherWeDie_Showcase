using UnityEngine;
using Unity.Netcode;

public class PickupAmmo : BaseLocalPickup
{
    protected override bool OnPickupFound(NetworkedPickupableItem pickup)
    {
        WeaponBase weapon = CurrentWeaponHolder.Instance.CurrentWeapon;
        if (weapon == null)
        {
            Debug.LogWarning("No current weapon found.");
            return false;
        }

        int amountToGive = weapon.maxAmmo * AmmoMultiplierManager.Instance.AmmoMultiplier;
        weapon.reserveAmmo += amountToGive;
        Debug.Log($"Ammo pickup: +{amountToGive} reserve ammo.");

        DespawnPickup(pickup);
        return true; // ✅ Signal success so the sound is played
    }
}

