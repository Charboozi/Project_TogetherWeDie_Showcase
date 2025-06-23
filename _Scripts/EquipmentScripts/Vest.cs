using UnityEngine;
using Unity.Netcode;

public class Vest : BaseEquipment
{
    [Header("Vest Settings")]
    [SerializeField] private int armorBonus = 2; // Base armor bonus
    [SerializeField] private int upgradeBonus = 2; // How much more armor upgrade adds

    public int ArmorBonus => armorBonus;

    private bool effectApplied = false;

    private void OnEnable()
    {
        ApplyArmorBonus();
    }

    private void OnDisable()
    {
        RemoveArmorBonus();
    }

    private void ApplyArmorBonus()
    {
        if (effectApplied) return;

        var localPlayerObj = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (localPlayerObj != null)
        {
            var healthProxy = localPlayerObj.GetComponent<HealthProxy>();
            if (healthProxy != null)
            {
                healthProxy.AddArmor(armorBonus);
                effectApplied = true;
                Debug.Log($"{gameObject.name} applied armor bonus: +{armorBonus}");
            }
        }
    }

    private void RemoveArmorBonus()
    {
        if (!effectApplied || NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient)
            return;

        var localPlayerObj = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (localPlayerObj == null) return;

        var healthProxy = localPlayerObj.GetComponent<HealthProxy>();
        if (healthProxy == null) return;

        healthProxy.RemoveArmor(armorBonus);
        effectApplied = false;
        Debug.Log($"{gameObject.name} removed armor bonus: -{armorBonus}");
    }

    public override void Upgrade()
    {
        if (HasBeenUpgraded)
        {
            Debug.LogWarning($"{gameObject.name} is already upgraded. Ignoring.");
            return;
        }

        base.Upgrade(); // ✅ Set HasBeenUpgraded = true

        // Remove old armor bonus first
        RemoveArmorBonus();

        // Upgrade the armor bonus
        armorBonus += upgradeBonus;

        Debug.Log($"{gameObject.name} upgraded! New armor bonus: +{armorBonus}");

        // Apply new upgraded armor bonus
        ApplyArmorBonus();
    }
}
