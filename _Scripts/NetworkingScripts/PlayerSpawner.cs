using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class PlayerSpawner : MonoBehaviour
{
    // Optionally assign these manually in the Inspector or populate them in Awake.
    public List<Transform> spawnPoints = new List<Transform>();

    private int nextSpawnIndex = 0;

    private void Awake()
    {
        // If you haven't manually assigned the spawn points,
        // find all objects tagged as "SpawnPoint" and add them.
        if (spawnPoints.Count == 0)
        {
            GameObject[] points = GameObject.FindGameObjectsWithTag("StartPosition");
            foreach (GameObject point in points)
            {
                spawnPoints.Add(point.transform);
            }
        }

        // Subscribe to client connection callback.
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        // Only the server should handle spawning.
        if (NetworkManager.Singleton.IsServer)
        {
            SpawnPlayerForClient(clientId);
        }
    }

    private void SpawnPlayerForClient(ulong clientId)
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("No spawn points available!");
            return;
        }

        // Select the next spawn point in the list (sequentially).
        Transform spawnPoint = spawnPoints[nextSpawnIndex];
        nextSpawnIndex = (nextSpawnIndex + 1) % spawnPoints.Count;

        // Instantiate the player prefab at the selected spawn point.
        GameObject playerInstance = Instantiate(
            NetworkManager.Singleton.NetworkConfig.PlayerPrefab,
            spawnPoint.position,
            spawnPoint.rotation);

        // Spawn the player object and assign ownership to the client.
        NetworkObject netObj = playerInstance.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.SpawnAsPlayerObject(clientId);
        }
    }
}
