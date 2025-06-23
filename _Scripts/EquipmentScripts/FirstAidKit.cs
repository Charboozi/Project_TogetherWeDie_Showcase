using UnityEngine;
using Unity.Netcode;

public class RegenerationBooster : BaseEquipment
{
    [Header("Regeneration Booster Settings")]
    [SerializeField] private float regenSpeedMultiplier = 0.5f; // Example: 0.5x makes regen twice as fast
    [SerializeField] private float upgradeMultiplierReduction = 0.1f; // How much we improve when upgraded

    private bool effectApplied = false;

    private void OnEnable()
    {
        ApplyRegenBoost();
    }

    private void OnDisable()
    {
        RemoveRegenBoost();
    }

    private void ApplyRegenBoost()
    {
        if (effectApplied) return;

        var localPlayerObj = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (localPlayerObj != null)
        {
            var healthProxy = localPlayerObj.GetComponent<HealthProxy>();
            if (healthProxy != null)
            {
                healthProxy.MultiplyRegenInterval(regenSpeedMultiplier);
                effectApplied = true;
                Debug.Log($"{gameObject.name} applied regen speed boost: x{regenSpeedMultiplier}");
            }
        }
    }

    private void RemoveRegenBoost()
    {
        if (!effectApplied) return;

        if (NetworkManager.Singleton == null || NetworkManager.Singleton.ShutdownInProgress || NetworkManager.Singleton.LocalClient == null)
        {
            effectApplied = false;
            return;
        }

        var localPlayerObj = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (localPlayerObj != null)
        {
            var healthProxy = localPlayerObj.GetComponent<HealthProxy>();
            if (healthProxy != null)
            {
                healthProxy.MultiplyRegenInterval(1f / regenSpeedMultiplier); // Restore to original
                Debug.Log($"{gameObject.name} removed regen speed boost: x{regenSpeedMultiplier}");
            }
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

        base.Upgrade(); // 🧩 This sets HasBeenUpgraded = true

        // Remove old regen boost first
        RemoveRegenBoost();

        // Improve regen speed by reducing the regen interval multiplier
        regenSpeedMultiplier = Mathf.Max(0.1f, regenSpeedMultiplier - upgradeMultiplierReduction);

        Debug.Log($"{gameObject.name} upgraded! New regen speed multiplier: x{regenSpeedMultiplier}");

        // Re-apply the better regen boost
        ApplyRegenBoost();
    }
}
