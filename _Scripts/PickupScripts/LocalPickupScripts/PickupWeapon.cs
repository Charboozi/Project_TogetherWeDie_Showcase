using UnityEngine;
using System.Linq;

public class PickupWeapon : BaseLocalPickup
{
    [SerializeField] private WeaponInventory weaponInventory;

    private void Awake()
    {
        if (weaponInventory == null)
            Debug.LogError("WeaponInventory missing!");
    }

    protected override bool OnPickupFound(NetworkedPickupableItem pickup)
    {
        string weaponName = pickup.gameObject.name;

        if (weaponInventory.Weapons.Any(w => w.name == weaponName))
        {
            Debug.Log($"Weapon '{weaponName}' is already active. Not picking it up.");
            return false;
        }

        weaponInventory.PickUpWeapon(weaponName);
        DespawnPickup(pickup);
        return true; // ✅ Signal successful pickup
    }
}
