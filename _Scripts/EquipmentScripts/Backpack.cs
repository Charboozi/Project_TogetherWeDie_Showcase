using UnityEngine;

public class Backpack : BaseEquipment
{
    [Header("Backpack Settings")]
    [SerializeField] private int weaponBonus = 3;
    [SerializeField] private int upgradeBonus = 1;
    [SerializeField] private WeaponInventory weaponInventory;


    private void OnEnable()
    {
        if (weaponInventory != null)
        {
            weaponInventory.SetMaxWeapons(weaponBonus);
            Debug.Log($"{gameObject.name}: Backpack equipped. New max weapons: {weaponBonus}");
        }
    }

    private void OnDisable()
    {
        if (weaponInventory != null)
        {
            weaponInventory.SetMaxWeapons(2);
            Debug.Log($"{gameObject.name}: Backpack unequipped. New max weapons: {2}");
        }
    }

    public override void Upgrade()
    {
        if (HasBeenUpgraded)
        {
            Debug.LogWarning($"{gameObject.name} is already upgraded. Ignoring.");
            return;
        }

        base.Upgrade(); // 🧩 Mark as upgraded

        // Remove old bonus first
        OnDisable();

        // Upgrade the bonus
        weaponBonus += upgradeBonus;

        Debug.Log($"{gameObject.name} upgraded! New weapon bonus: +{weaponBonus}");

        // Reapply new bonus
        OnEnable();
    }
}
