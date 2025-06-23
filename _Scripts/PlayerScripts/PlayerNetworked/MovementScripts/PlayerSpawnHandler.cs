using UnityEngine;
using Unity.Netcode;
using System.Collections;

[RequireComponent(typeof(NetworkObject))]
public class PlayerSpawnHandler : NetworkBehaviour
{
    [SerializeField] private CharacterController characterController;

    [ClientRpc]
    public void SetSpawnPointClientRpc(Vector3 position, Quaternion rotation, ClientRpcParams rpcParams = default)
    {
        if (!IsOwner) return;

        StartCoroutine(MoveAfterDelay(position, rotation));
    }

    private IEnumerator MoveAfterDelay(Vector3 pos, Quaternion rot)
    {
        yield return new WaitForSeconds(0.1f);

        if (characterController != null)
        {
            characterController.enabled = false;
            transform.SetPositionAndRotation(pos, rot);
            characterController.enabled = true;
        }
        else
        {
            transform.SetPositionAndRotation(pos, rot);
        }

        Debug.Log($"✅ [Client {NetworkManager.Singleton.LocalClientId}] Moved to spawn at {pos}");
    }
}
