using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class WheelItemSpawner : NetworkBehaviour
{
    [Header("Spawn Prefabs")]
    [SerializeField] private List<GameObject> weaponPrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> keycardPrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> powerupPrefabs = new List<GameObject>();

    [Header("Spawn Position Offsets")]
    [SerializeField] private Vector3 weaponSpawnOffset = Vector3.zero;
    [SerializeField] private Vector3 keycardSpawnOffset = Vector3.zero;
    [SerializeField] private Vector3 powerupSpawnOffset = Vector3.zero;

    [Header("Spawn Rotation Offsets")]
    [SerializeField] private Vector3 weaponRotationOffset = Vector3.zero;
    [SerializeField] private Vector3 keycardRotationOffset = Vector3.zero;
    [SerializeField] private Vector3 powerupRotationOffset = Vector3.zero;

    public void SpawnRandomWeapon()
    {
        if (!IsServer) return;
        SpawnItemFromList(weaponPrefabs, weaponSpawnOffset, weaponRotationOffset);
    }

    public void SpawnRandomKeycard()
    {
        if (!IsServer) return;
        SpawnItemFromList(keycardPrefabs, keycardSpawnOffset, keycardRotationOffset);
    }

    public void SpawnRandomPowerup()
    {
        if (!IsServer) return;
        SpawnItemFromList(powerupPrefabs, powerupSpawnOffset, powerupRotationOffset);
    }

    private void SpawnItemFromList(List<GameObject> prefabs, Vector3 spawnOffset, Vector3 rotationOffset)
    {
        if (prefabs == null || prefabs.Count == 0)
        {
            Debug.LogWarning("[WheelItemSpawner] No prefabs assigned to list!");
            return;
        }

        int randomIndex = Random.Range(0, prefabs.Count);
        var selectedPrefab = prefabs[randomIndex];

        if (selectedPrefab == null)
        {
            Debug.LogWarning("[WheelItemSpawner] Selected prefab is null!");
            return;
        }

        Vector3 spawnPosition = transform.position + spawnOffset;
        Quaternion spawnRotation = Quaternion.Euler(rotationOffset);

        GameObject item = Instantiate(selectedPrefab, spawnPosition, spawnRotation);

        if (item.TryGetComponent(out NetworkObject networkObject))
        {
            networkObject.Spawn();
            RenameItemClientRpc(networkObject.NetworkObjectId);
        }
        else
        {
            Debug.LogError("[WheelItemSpawner] Spawned item missing NetworkObject!");
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
