using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class ReviveSystem : NetworkBehaviour
{
    public float reviveTime = 3f;
    public float reviveRange = 2f;
    private bool isReviving = false;

    private void OnEnable() => PlayerInput.OnInteractPressed += TryFindAndReviveNearbyPlayer;
    private void OnDisable() => PlayerInput.OnInteractPressed -= TryFindAndReviveNearbyPlayer;

    private ReviveUI reviveUI;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            reviveUI = FindFirstObjectByType<ReviveUI>();
        }
    }

    private void TryFindAndReviveNearbyPlayer()
    {
        if (!IsOwner || isReviving) return;

        // 🔒 Prevent reviving in PvP mode
        if (GameModeManager.Instance != null && GameModeManager.Instance.IsPvPMode)
        {
            Debug.Log("⛔ Reviving is disabled in PvP mode.");
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, reviveRange);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<EntityHealth>(out var entityHealth))
            {
                if (entityHealth.isDowned.Value && hit.gameObject != gameObject)
                {
                    StartCoroutine(ReviveCoroutine(hit.gameObject, entityHealth));
                    break;
                }
            }
        }
    }

    private IEnumerator ReviveCoroutine(GameObject downedPlayer, EntityHealth entityHealth)
    {
        isReviving = true;
        reviveUI.Show();

        float timer = 0f;
        while (timer < reviveTime)
        {
            if (Vector3.Distance(transform.position, downedPlayer.transform.position) > reviveRange ||
                !entityHealth.isDowned.Value)
            {
                reviveUI.Hide();
                isReviving = false;
                yield break;
            }

            timer += Time.deltaTime;
            reviveUI.SetProgress(timer / reviveTime);
            yield return null;
        }

        reviveUI.Hide();

        RevivePlayerServerRpc(downedPlayer.GetComponent<NetworkObject>().NetworkObjectId);
        isReviving = false;
    }

    [ServerRpc]
    private void RevivePlayerServerRpc(ulong downedNetworkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(downedNetworkObjectId, out NetworkObject no))
        {
            var entityHealth = no.GetComponent<EntityHealth>();
            if (entityHealth != null && entityHealth.isDowned.Value)
            {
                entityHealth.Revive();
                Debug.Log($"Player {downedNetworkObjectId} revived.");
            }
        }
    }
}

