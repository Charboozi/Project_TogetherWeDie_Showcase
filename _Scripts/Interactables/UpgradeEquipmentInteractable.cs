using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class UpgradeEquipmentInteractable : MonoBehaviour, IClientOnlyAction, ICheckIfInteractable
{
    [SerializeField] private string equipmentNameToUpgrade;
    [SerializeField] private GameObject playerEquipmentHandler;

    public bool CanCurrentlyInteract()
    {
        if (playerEquipmentHandler == null)
            return false;

        var equipments = playerEquipmentHandler.GetComponentsInChildren<BaseEquipment>(true);

        foreach (var equipment in equipments)
        {
            if (equipment.gameObject.activeInHierarchy && equipment.name.Contains(equipmentNameToUpgrade))
            {
                // 🔥 New important check: already upgraded?
                return !equipment.HasBeenUpgraded;
            }
        }

        return false;
    }

    public void DoClientAction()
    {
        if (playerEquipmentHandler == null)
        {
            Debug.LogWarning("⚠️ PlayerEquipmentHandler not assigned.");
            return;
        }

        var equipments = playerEquipmentHandler.GetComponentsInChildren<BaseEquipment>(true);

        foreach (var equipment in equipments)
        {
            if (equipment.gameObject.activeInHierarchy && equipment.name.Contains(equipmentNameToUpgrade))
            {
                if (equipment.HasBeenUpgraded)
                {
                    Debug.LogWarning($"❌ Equipment '{equipment.name}' is already upgraded. Cannot upgrade again.");
                    return;
                }

                equipment.Upgrade();
                Debug.Log($"✅ Upgraded equipment: {equipment.name}");
                return;
            }
        }

        Debug.LogWarning($"❌ No matching active equipment '{equipmentNameToUpgrade}' found.");
    }
}
