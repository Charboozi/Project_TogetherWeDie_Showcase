using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

/// <summary>
/// Spawn the a impact effect on the network so everyone sees it.
/// </summary>
public class NetworkImpactSpawner : NetworkBehaviour
{
    public static NetworkImpactSpawner Instance { get; private set; }

    [Header("Impact Effects")]
    [SerializeField] private List<GameObject> impactEffectPrefabs;

    private Dictionary<string, GameObject> prefabLookup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Cache prefabs by name
        prefabLookup = new Dictionary<string, GameObject>();
        foreach (var prefab in impactEffectPrefabs)
        {
            if (prefab != null)
            {
                prefabLookup[prefab.name] = prefab;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnImpactEffectServerRpc(Vector3 position, Vector3 normal, string prefabName)
    {
        SpawnImpactEffectClientRpc(position, normal, prefabName);
    }

    [ClientRpc]
    private void SpawnImpactEffectClientRpc(Vector3 position, Vector3 normal, string prefabName)
    {
        if (prefabLookup.TryGetValue(prefabName, out var impactEffectPrefab))
        {
            var impactEffect = Instantiate(impactEffectPrefab, position, Quaternion.LookRotation(normal));
            Destroy(impactEffect, 2f); // TODO: Replace with pooling if needed
        }
        else
        {
            Debug.LogError($"Impact effect prefab '{prefabName}' not found in assigned list!");
        }
    }
}
