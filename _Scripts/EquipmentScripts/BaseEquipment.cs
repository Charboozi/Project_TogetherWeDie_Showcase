using UnityEngine;

public class BaseEquipment : MonoBehaviour
{
    public Sprite equipmentIcon;    // Normal icon
    public Sprite upgradedIcon;     // Upgraded version

    public bool HasBeenUpgraded { get; private set; } = false;

    public virtual void Upgrade()
    {
        if (HasBeenUpgraded)
            return;

        HasBeenUpgraded = true;

        Debug.Log($"{gameObject.name} upgraded!");

        if (EquipmentUIManager.Instance != null)
        {
            // 🔥 Remove old icon
            EquipmentUIManager.Instance.HideIcon(equipmentIcon);
        }

        equipmentIcon = upgradedIcon; // 🔥 Swap to upgraded sprite

        if (EquipmentUIManager.Instance != null)
        {
            // 🔥 Display upgraded icon
            EquipmentUIManager.Instance.DisplayIcon(equipmentIcon);
        }
    }
}
