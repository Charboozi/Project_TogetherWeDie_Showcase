using UnityEngine;
using Unity.Netcode;

public class PickupEquipment : BaseLocalPickup
{
    [SerializeField] private EquipmentInventory equipmentInventory;
    
    protected override bool OnPickupFound(NetworkedPickupableItem pickup)
    {
        string equipmentName = pickup.gameObject.name;

        if (equipmentInventory.HasEquipped(equipmentName))
        {
            Debug.Log($"Equipment '{equipmentName}' is already active. Not picking it up.");
            return false;
        }

        equipmentInventory.Equip(equipmentName);

        var localPlayerObj = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (localPlayerObj != null)
        {
            var proxy = localPlayerObj.GetComponent<EquipmentVisualProxy>();
            proxy?.RequestEquipEquipmentServerRpc(equipmentName);
        }

        DespawnPickup(pickup);
        return true; // ✅ Signal successful pickup
    }
}
