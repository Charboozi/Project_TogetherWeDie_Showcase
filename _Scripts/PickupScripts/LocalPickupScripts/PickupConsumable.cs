using UnityEngine;
using Unity.Netcode;

public class PickupConsumable : BaseLocalPickup
{
    protected override bool OnPickupFound(NetworkedPickupableItem pickup)
    {
        if (ConsumableManager.Instance != null)
        {
            var consumableInfo = pickup.GetComponent<ConsumablePickupItem>();
            if (consumableInfo != null)
            {
                ConsumableManager.Instance.Add(consumableInfo.ConsumableType, consumableInfo.Amount);
                Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] Picked up {consumableInfo.Amount}x {consumableInfo.ConsumableType}");

                DespawnPickup(pickup);
                return true;
            }
            else
            {
                Debug.LogWarning("[PickupConsumable] No ConsumablePickupItem component found on pickup!");
            }
        }
        else
        {
            Debug.LogWarning("[PickupConsumable] ConsumableManager not found, cannot pick up item.");
        }

        return false; // ❌ Pickup failed
    }
}