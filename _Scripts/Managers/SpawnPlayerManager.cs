using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SpawnPlayerManager : MonoBehaviour
{
    [Header("Prefabs & Points")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;

            // Solo host fallback
            if (NetworkManager.Singleton.ConnectedClientsList.Count == 1)
            {
                // Scene already loaded, spawn host manually after short delay
                Invoke(nameof(SpawnHostPlayer), 0.1f);
            }
        }
    }

    private void SpawnHostPlayer()
    {
        ulong hostId = NetworkManager.Singleton.LocalClientId;

        GameObject player = Instantiate(playerPrefab);
        var netObj = player.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(hostId);

        Transform spawn = spawnPoints[0];

        var handler = player.GetComponent<PlayerSpawnHandler>();
        if (handler != null)
        {
            handler.SetSpawnPointClientRpc(
                spawn.position,
                spawn.rotation,
                new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new[] { hostId }
                    }
                });
        }

        Debug.Log($"✅ Spawned solo host at {spawn.position}");
    }


    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(string sceneName, LoadSceneMode mode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (!NetworkManager.Singleton.IsServer || sceneName != SceneManager.GetActiveScene().name)
            return;

        for (int i = 0; i < clientsCompleted.Count; i++)
        {
            ulong clientId = clientsCompleted[i];
            GameObject player = Instantiate(playerPrefab);
            var netObj = player.GetComponent<NetworkObject>();
            netObj.SpawnAsPlayerObject(clientId);

            // Get assigned spawn point
            Transform spawn = spawnPoints[i % spawnPoints.Length];

            // Send spawn info to that player only
            var handler = player.GetComponent<PlayerSpawnHandler>();
            if (handler != null)
            {
                handler.SetSpawnPointClientRpc(
                    spawn.position,
                    spawn.rotation,
                    new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new[] { clientId }
                        }
                    });
            }
        }

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
    }
}
