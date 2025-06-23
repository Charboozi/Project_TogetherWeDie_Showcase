using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class WeaponUpgradeStation : MonoBehaviour, IClientOnlyAction, ICheckIfInteractable
{
    [Header("Effects")]
    [SerializeField] private ParticleSystem upgradeEffect;

    public bool CanCurrentlyInteract()
    {
        var currentWeapon = CurrentWeaponHolder.Instance?.CurrentWeapon;
        return currentWeapon != null && currentWeapon.CanUpgrade && currentWeapon.upgradeWeapon != null;
    }

    public void DoClientAction()
    {
        var currentWeapon = CurrentWeaponHolder.Instance?.CurrentWeapon;

        if (currentWeapon == null || !currentWeapon.CanUpgrade || currentWeapon.upgradeWeapon == null)
        {
            Debug.LogWarning("❌ No valid weapon to upgrade.");
            return;
        }

        var upgradedWeapon = currentWeapon.upgradeWeapon;
        var weaponSwitcher = WeaponSwitcher.Instance;
        var weaponInventory = weaponSwitcher.GetComponent<WeaponInventory>();

        if (weaponInventory == null || weaponSwitcher == null)
        {
            Debug.LogError("❌ Weapon system references missing.");
            return;
        }

        // Perform the upgrade
        currentWeapon.gameObject.SetActive(false);
        weaponInventory.RemoveWeapon(weaponSwitcher.CurrentWeaponIndex);
        weaponInventory.AddWeapon(upgradedWeapon);

        if (upgradeEffect != null)
        {
            upgradeEffect.Play();
        }

        Debug.Log($"🔧 Weapon upgraded locally to: {upgradedWeapon.name}");
    }
}
