using Unity.Netcode;

public class EquipmentVisualProxy : NetworkBehaviour
{
    // The client calls this ServerRpc to request equipping an item.
    [ServerRpc(RequireOwnership = false)]
    public void RequestEquipEquipmentServerRpc(string equipmentName, ServerRpcParams rpcParams = default)
    {
        // Look up the sender's player object using their client ID.
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(rpcParams.Receive.SenderClientId, out var client))
        {
            var modelManager = client.PlayerObject.GetComponent<PlayerModelEquipmentManager>();
            if (modelManager != null)
            {
                modelManager.AddEquipment(equipmentName);
            }
        }
    }
}
