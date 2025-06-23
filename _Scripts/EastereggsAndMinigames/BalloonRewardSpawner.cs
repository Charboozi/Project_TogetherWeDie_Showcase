using UnityEngine;
using Unity.Netcode;

public class RewardSpawner : NetworkBehaviour
{
    [Tooltip("Possible items to randomly spawn as a reward.")]
    [SerializeField] private GameObject[] rewardPrefabs;

    [Tooltip("Offset from the spawner's position.")]
    [SerializeField] private Vector3 spawnOffset = Vector3.up * 0.5f;

    [Tooltip("Rotation offset to apply to the spawned item.")]
    [SerializeField] private Vector3 spawnRotationEuler = Vector3.zero;

    public void SpawnRandomReward()
    {
        if (!IsServer)
        {
            Debug.LogWarning("Tried to spawn reward from client! Aborted.");
            return;
        }

        if (rewardPrefabs == null || rewardPrefabs.Length == 0)
        {
            Debug.LogWarning("❌ No reward prefabs assigned.");
            return;
        }

        int index = Random.Range(0, rewardPrefabs.Length);
        GameObject prefab = rewardPrefabs[index];
        Vector3 spawnPosition = transform.position + spawnOffset;
        Quaternion spawnRotation = Quaternion.Euler(spawnRotationEuler);

        GameObject rewardInstance = Instantiate(prefab, spawnPosition, spawnRotation);

        if (rewardInstance.TryGetComponent(out NetworkObject networkObject))
        {
            networkObject.Spawn();
            RenameItemClientRpc(networkObject.NetworkObjectId);
            Debug.Log($"🎁 Spawned networked reward: {prefab.name}");
        }
        else
        {
            Debug.LogError("❌ Reward prefab is missing NetworkObject component!");
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
