using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles teleport interactions. Supports normal and timed reward override modes.
/// Players standing on the teleporter will be sent to either normal or reward points.
/// </summary>
[RequireComponent(typeof(Interactable))]
public class TeleporterInteractable : NetworkBehaviour, IInteractableAction
{
    [Header("Normal Teleport Points")]
    [SerializeField] private Transform[] teleportPoints;

    [Header("Reward Teleport Points")]
    [SerializeField] private Transform[] rewardTeleportPoints;

    [Header("Reward Room Spawners")]
    [SerializeField] private RewardSpawner[] rewardSpawners;

    [Header("Teleporter Visuals")]
    [SerializeField] private GameObject normalVisual;
    [SerializeField] private GameObject rewardVisual;

    // internal state
    private bool isOverrideActive = false;
    private bool rewardUsed = false;
    private float rewardDuration = 0f;

    // track which client IDs were sent to reward
    private readonly List<ulong> overrideTeleportedClients = new List<ulong>();
    // track players currently in the teleport zone
    private readonly List<GameObject> playersInZone = new List<GameObject>();

    /// <summary>
    /// Called by server to start a timed override.
    /// During override, the first use teleports zone players to reward points.
    /// </summary>
    public void OverrideDestinationTemporarily(float duration)
    {
        if (!IsServer) return;
        isOverrideActive = true;
        rewardUsed = false;
        rewardDuration = duration;
        overrideTeleportedClients.Clear();

        // update visuals on all clients
        TeleporterOverrideClientRpc(true);
        Debug.Log($"[Teleporter] Override enabled for {duration} seconds.");
    }

    /// <summary>
    /// Executes teleport logic when the interactable is used.
    /// </summary>
    public void DoAction()
    {
        if (!IsServer || playersInZone.Count == 0) return;

        // Reward override mode
        if (isOverrideActive && !rewardUsed)
        {
            // spawn rewards once
            int spawnCount = Mathf.Min(NetworkManager.Singleton.ConnectedClientsList.Count, rewardSpawners.Length);
            for (int i = 0; i < spawnCount; i++)
                rewardSpawners[i]?.SpawnRandomReward();

            rewardUsed = true;
            Debug.Log("[Teleporter] Teleporting zone players to reward area.");

            // teleport each player in zone to corresponding reward point
            for (int i = 0; i < playersInZone.Count; i++)
            {
                var p = playersInZone[i];
                if (p == null) continue;
                if (!p.TryGetComponent(out NetworkObject netObj)) continue;
                ulong clientId = netObj.OwnerClientId;

                Vector3 dest = (i < rewardTeleportPoints.Length)
                    ? rewardTeleportPoints[i].position
                    : rewardTeleportPoints[0].position;

                overrideTeleportedClients.Add(clientId);
                TeleportPlayerClientRpc(clientId, dest);
            }
            playersInZone.Clear();

            // schedule return teleport after duration
            StartCoroutine(EndOverrideAfterDelay());
        }
        // Normal teleport mode
        else if (!isOverrideActive)
        {
            Debug.Log("[Teleporter] Teleporting zone players to normal area.");
            foreach (var p in playersInZone)
            {
                if (p == null) continue;
                if (!p.TryGetComponent(out NetworkObject netObj)) continue;
                Vector3 dest = teleportPoints[Random.Range(0, teleportPoints.Length)].position;
                TeleportPlayerClientRpc(netObj.OwnerClientId, dest);
            }
            playersInZone.Clear();
        }
        // if override active but already used, do nothing
    }

    /// <summary>
    /// Moves override-mode visuals on clients.
    /// </summary>
    [ClientRpc]
    private void TeleporterOverrideClientRpc(bool overrideActive)
    {
        isOverrideActive = overrideActive;
        normalVisual?.SetActive(!overrideActive);
        rewardVisual?.SetActive(overrideActive);
    }

    /// <summary>
    /// Teleports a given client to position on their client.
    /// </summary>
    [ClientRpc]
    private void TeleportPlayerClientRpc(ulong clientId, Vector3 position)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;
        var obj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (obj == null) return;
        var ctrl = obj.GetComponent<CharacterController>();
        if (ctrl != null)
        {
            ctrl.enabled = false;
            ctrl.transform.position = position;
            ctrl.enabled = true;
        }
        FlashEffect.Instance?.TriggerFlash();
    }

    /// <summary>
    /// Returns override players back to normal points after delay.
    /// </summary>
    private IEnumerator EndOverrideAfterDelay()
    {
        yield return new WaitForSeconds(rewardDuration);
        Debug.Log("[Teleporter] Returning reward-mode players to normal area.");

        foreach (ulong clientId in overrideTeleportedClients)
        {
            Vector3 backPos = teleportPoints[Random.Range(0, teleportPoints.Length)].position;
            TeleportPlayerClientRpc(clientId, backPos);
        }
        overrideTeleportedClients.Clear();

        // end override
        isOverrideActive = false;
        TeleporterOverrideClientRpc(false);
    }

    /// <summary>
    /// Adds a player to the zone. Only server should register.
    /// </summary>
    public void AddPlayerToZone(GameObject player)
    {
        if (!IsServer || playersInZone.Contains(player)) return;
        playersInZone.Add(player);
    }

    /// <summary>
    /// Removes a player from the zone.
    /// </summary>
    public void RemovePlayerFromZone(GameObject player)
    {
        if (!IsServer) return;
        playersInZone.Remove(player);
    }
}
