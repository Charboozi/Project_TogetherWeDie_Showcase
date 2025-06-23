using UnityEngine;

public class AmmoPouch : BaseEquipment
{
    [Header("Ammo Pouch Settings")]
    [SerializeField] private int multiplier = 2; // Default multiplier: 2x
    [SerializeField] private int upgradeBonus = 1; // Upgrade adds +1x

    private bool effectApplied = false;

    private void OnEnable()
    {
        ApplyAmmoBoost();
    }

    private void OnDisable()
    {
        RemoveAmmoBoost();
    }

    private void ApplyAmmoBoost()
    {
        if (effectApplied) return;

        if (AmmoMultiplierManager.Instance != null)
        {
            AmmoMultiplierManager.Instance.SetMultiplier(multiplier);
            effectApplied = true;
            Debug.Log($"{gameObject.name} applied ammo multiplier boost: x{multiplier}");
        }
    }

    private void RemoveAmmoBoost()
    {
        if (!effectApplied) return;

        if (AmmoMultiplierManager.Instance != null)
        {
            AmmoMultiplierManager.Instance.ResetMultiplier();
            Debug.Log($"{gameObject.name} removed ammo multiplier boost.");
        }

        effectApplied = false;
    }

    public override void Upgrade()
    {
        if (HasBeenUpgraded)
        {
            Debug.LogWarning($"{gameObject.name} is already upgraded. Ignoring.");
            return;
        }

        base.Upgrade(); // ✅ Mark as upgraded

        // Remove current multiplier
        RemoveAmmoBoost();

        // Increase multiplier
        multiplier += upgradeBonus;

        Debug.Log($"{gameObject.name} upgraded! New ammo multiplier: x{multiplier}");

        // Apply new multiplier
        ApplyAmmoBoost();
    }
}
