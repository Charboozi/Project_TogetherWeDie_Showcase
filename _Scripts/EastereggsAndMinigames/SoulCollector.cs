using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class SoulCollector : NetworkBehaviour
{
    [Header("Settings")]
    public float detectionRadius = 8f;
    public float enemyKillRadius = 12f;
    public int soulsToFill = 10;

    [Header("Visual Fill")]
    public Transform fillCylinder;
    public float maxFillHeight = 1f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip soulCollectSound;
    public AudioClip filledSound;
    public AudioClip resetErrorSound;

    [Header("Visual Effects")]
    public ParticleSystem soulCollectEffect;
    public ParticleSystem collectionCompleteEffect;

    private NetworkVariable<int> currentSouls = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        currentSouls.OnValueChanged += OnSoulsChanged;
    }

    public override void OnDestroy()
    {
        currentSouls.OnValueChanged -= OnSoulsChanged;
    }

    private void Update()
    {
        if (!IsServer) return;

        var playersInRange = GetValidPlayersInDetectionRadius();

        if (playersInRange.Count == 0 && currentSouls.Value > 0)
        {
            currentSouls.Value = 0;

            if (resetErrorSound != null)
                PlaySoundClientRpc("error");
        }
    }

    private List<ulong> GetValidPlayersInDetectionRadius()
    {
        List<ulong> playersInRange = new();

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player") && hit.TryGetComponent(out NetworkObject netObj))
            {
                if (TryGetEntityHealth(netObj.OwnerClientId, out var health) && !health.isDowned.Value)
                {
                    playersInRange.Add(netObj.OwnerClientId);
                }
            }
        }

        return playersInRange;
    }

    private void OnSoulsChanged(int oldVal, int newVal)
    {
        float t = Mathf.Clamp01((float)newVal / soulsToFill);
        if (fillCylinder != null)
        {
            var scale = fillCylinder.localScale;
            scale.y = t;
            fillCylinder.localScale = scale;
        }

        if (newVal > oldVal && soulCollectSound != null)
            PlaySoundClientRpc("collect");

        if (newVal >= soulsToFill && oldVal < soulsToFill)
        {
            if (filledSound != null)
                PlaySoundClientRpc("filled");

            if (collectionCompleteEffect != null)
                SpawnEffectClientRpc("complete");

            // 👇 Cache list of eligible players BEFORE reset
            List<ulong> snapshot = GetValidPlayersInDetectionRadius();
            GiveKeycardsToPlayers(snapshot);

            currentSouls.Value = 0;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterSoulServerRpc(Vector3 killPosition)
    {
        var playersInRange = GetValidPlayersInDetectionRadius();

        foreach (var clientId in playersInRange)
        {
            if (TryGetPlayerPosition(clientId, out var pos))
            {
                if (Vector3.Distance(killPosition, pos) <= enemyKillRadius)
                {
                    currentSouls.Value++;
                    if (soulCollectEffect != null)
                    {
                        SpawnEffectClientRpc("soul");
                    }
                    break;
                }
            }
        }
    }

    private void GiveKeycardsToPlayers(List<ulong> playerIds)
    {
        foreach (var clientId in playerIds)
        {
            GiveKeycardClientRpc(clientId); // Send command to local player only
        }
    }

    private bool TryGetPlayerPosition(ulong clientId, out Vector3 position)
    {
        var player = GetPlayerByClientId(clientId);
        if (player != null)
        {
            position = player.transform.position;
            return true;
        }

        position = Vector3.zero;
        return false;
    }

    private bool TryGetEntityHealth(ulong clientId, out EntityHealth health)
    {
        var player = GetPlayerByClientId(clientId);
        if (player != null)
        {
            return player.TryGetComponent(out health);
        }

        health = null;
        return false;
    }

    private GameObject GetPlayerByClientId(ulong clientId)
    {
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (player.TryGetComponent<NetworkObject>(out var netObj) && netObj.OwnerClientId == clientId)
                return player;
        }
        return null;
    }

    [ClientRpc]
    private void SpawnEffectClientRpc(string effectType)
    {
        if (effectType == "soul" && soulCollectEffect != null)
        {
            soulCollectEffect.Play();
        }
        else if (effectType == "complete" && collectionCompleteEffect != null)
        {
            collectionCompleteEffect.Play();
        }
    }

    [ClientRpc]
    private void PlaySoundClientRpc(string soundType)
    {
        if (soundType == "collect" && soulCollectSound != null)
            audioSource.PlayOneShot(soulCollectSound);
        else if (soundType == "filled" && filledSound != null)
            audioSource.PlayOneShot(filledSound);
        else if (soundType == "error" && resetErrorSound != null)
            audioSource.PlayOneShot(resetErrorSound);
    }
    
    [ClientRpc]
    private void GiveKeycardClientRpc(ulong targetClientId)
    {
        // 🔐 Only let the *targeted* client execute this
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        if (ConsumableManager.Instance != null)
        {
            ConsumableManager.Instance.Add("Keycard", 1);
        }
    }
}
