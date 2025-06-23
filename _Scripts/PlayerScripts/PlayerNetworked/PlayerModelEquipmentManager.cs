using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System.Collections.Generic;

public class PlayerModelEquipmentManager : NetworkBehaviour
{
    [Tooltip("All possible equipment GameObjects on the player model (scattered on bones)")]
    [SerializeField] private List<GameObject> equipmentObjects = new List<GameObject>();

    // A networked list of equipped equipment names.
    private NetworkList<FixedString32Bytes> equippedItems = new NetworkList<FixedString32Bytes>();

    public override void OnNetworkSpawn()
    {
        if (IsClient)
            equippedItems.OnListChanged += OnEquippedItemsChanged;
        UpdateVisuals();
    }

    public override void OnDestroy()
    {
        if (IsClient)
            equippedItems.OnListChanged -= OnEquippedItemsChanged;
    }

    private void OnEquippedItemsChanged(NetworkListEvent<FixedString32Bytes> change)
    {
        UpdateVisuals();
    }

    // For each equipment object, enable it if its name appears in the network list.
    private void UpdateVisuals()
    {
        foreach (GameObject obj in equipmentObjects)
        {
            if (obj == null) continue;
            bool shouldBeActive = false;
            foreach (var name in equippedItems)
            {
                if (name.ToString() == obj.name)
                {
                    shouldBeActive = true;
                    break;
                }
            }
            obj.SetActive(shouldBeActive);
        }
    }

    // Called by a ServerRpc from the proxy.
    public void AddEquipment(string equipmentName)
    {
        FixedString32Bytes name = new FixedString32Bytes(equipmentName);
        if (!equippedItems.Contains(name))
        {
            equippedItems.Add(name);
        }
    }
}
