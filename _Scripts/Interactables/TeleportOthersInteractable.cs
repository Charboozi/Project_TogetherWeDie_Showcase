using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkObject))]
public class TeleportOthersInteractable : NetworkBehaviour, IInteractorAwareAction
{
    [Header("Teleport Settings")]
    [Tooltip("Possible destinations. Each other player will get a unique one.")]
    [SerializeField] private Transform[] teleportPoints;

    /// <summary>
    /// Called on the server when someone interacts.
    /// </summary>
    /// <param name="interactorClientId">the client who triggered the interact</param>
    public void DoAction(ulong interactorClientId)
    {
        if (!IsServer) return;

        // collect everyone except the interactor
        var otherClients = new List<NetworkClient>();
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId != interactorClientId)
                otherClients.Add(client);
        }

        if (otherClients.Count == 0)
        {
            Debug.Log("[TeleportOthers] No other players to teleport.");
            return;
        }

        // copy & shuffle points
        var available = new List<Transform>(teleportPoints);
        Shuffle(available);

        Debug.Log($"[TeleportOthers] Teleporting {otherClients.Count} players to unique random points.");

        for (int i = 0; i < otherClients.Count; i++)
        {
            if (available.Count == 0)
            {
                Debug.LogWarning("[TeleportOthers] Ran out of unique points—reshuffling to reuse.");
                available = new List<Transform>(teleportPoints);
                Shuffle(available);
            }

            var point = available[0];
            available.RemoveAt(0);

            TeleportClientRpc(otherClients[i].ClientId, point.position);
            Debug.Log($"[TeleportOthers] -> Client {otherClients[i].ClientId} to {point.position}");
        }
    }

    // no-op so we still satisfy IInteractableAction
    public void DoAction() { }

    [ClientRpc]
    private void TeleportClientRpc(ulong targetClientId, Vector3 position)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        var playerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (playerObj == null)
        {
            Debug.LogWarning("[TeleportOthers] Local player object not found!");
            return;
        }

        // disable controller to avoid physics conflicts
        var cc = playerObj.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            cc.transform.position = position;
            cc.enabled = true;
        }
        else
        {
            playerObj.transform.position = position;
        }

        // optional flash
        if (FlashEffect.Instance != null)
            FlashEffect.Instance.TriggerFlash();
    }

    // Fisher–Yates shuffle
    private void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }
}
