using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class ItemSpawner : NetworkBehaviour, IInteractableAction
{
    [Header("Item Spawning Settings")]
    [SerializeField] private List<GameObject> itemPrefabs;
    [SerializeField] private Vector3 spawnOffsetPosition = new Vector3(0, 1.5f, 0.25f);
    [SerializeField] private Vector3 spawnRotationOffsetEuler = Vector3.zero; // Rotation offset in degrees

    public void DoAction()
    {
        if (!IsServer) return;
        SpawnItem();
    }

    private void SpawnItem()
    {
        if (itemPrefabs == null || itemPrefabs.Count == 0)
        {
            Debug.LogWarning("No item prefabs assigned to ItemSpawner!");
            return;
        }

        int randomIndex = Random.Range(0, itemPrefabs.Count);
        Vector3 spawnPosition = transform.position + spawnOffsetPosition;
        Quaternion spawnRotation = Quaternion.Euler(spawnRotationOffsetEuler);

        GameObject itemInstance = Instantiate(itemPrefabs[randomIndex], spawnPosition, spawnRotation);

        if (itemInstance.TryGetComponent(out NetworkObject networkObject))
        {
            networkObject.Spawn();
            RenameItemClientRpc(networkObject.NetworkObjectId);
        }
        else
        {
            Debug.LogError("Spawned item has no NetworkObject component!");
        }
    }

    [ClientRpc]
    private void RenameItemClientRpc(ulong itemId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemId, out NetworkObject item))
        {
            item.gameObject.name = item.gameObject.name.Replace("(Clone)", "").Trim();
        }
    }
}
