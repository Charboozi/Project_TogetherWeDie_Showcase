using UnityEngine;
using Unity.Netcode;

public class Shoes : BaseEquipment
{
    [Header("Shoes Settings")]
    [SerializeField] private float speedBonus = 2f; // Base speed bonus
    [SerializeField] private float upgradeBonus = 1f; // Extra speed added on upgrade

    private bool effectApplied = false;

    private void OnEnable()
    {
        ApplySpeedBonus();
    }

    private void OnDisable()
    {
        RemoveSpeedBonus();
    }

    private void ApplySpeedBonus()
    {
        if (effectApplied) return;

        var localPlayerObj = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (localPlayerObj != null)
        {
            var movement = localPlayerObj.GetComponent<NetworkedCharacterMovement>();
            if (movement != null)
            {
                movement.AddBonusSpeed(speedBonus);
                effectApplied = true;
                Debug.Log($"{gameObject.name} applied speed bonus: +{speedBonus}");
            }
        }
    }

    private void RemoveSpeedBonus()
    {
        if (!effectApplied) return;

        if (NetworkManager.Singleton == null || NetworkManager.Singleton.LocalClient == null)
            return;

        var localPlayerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (localPlayerObj != null)
        {
            var movement = localPlayerObj.GetComponent<NetworkedCharacterMovement>();
            if (movement != null)
            {
                movement.RemoveBonusSpeed(speedBonus);
                effectApplied = false;
                Debug.Log($"{gameObject.name} removed speed bonus: -{speedBonus}");
            }
        }
    }

    public override void Upgrade()
    {
        if (HasBeenUpgraded)
        {
            Debug.LogWarning($"{gameObject.name} is already upgraded. Ignoring.");
            return;
        }

        base.Upgrade(); // ✅ Mark as upgraded

        // Remove current bonus first
        RemoveSpeedBonus();

        // Increase speed bonus
        speedBonus += upgradeBonus;

        Debug.Log($"{gameObject.name} upgraded! New speed bonus: +{speedBonus}");

        // Apply upgraded bonus
        ApplySpeedBonus();
    }
}
