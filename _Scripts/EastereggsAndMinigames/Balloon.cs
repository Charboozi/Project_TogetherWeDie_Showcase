using Unity.Netcode;
using UnityEngine;

public class Balloon : NetworkBehaviour
{
    private bool isPopped = false;

    private void OnCollisionEnter(Collision collision)
    {
        TryPop();
    }

    private void OnTriggerEnter(Collider other)
    {
        TryPop();
    }

    public void TryPop()
    {
        if (isPopped) return;

        Debug.Log("🟢 TryPop() called on client for: " + name);
        RequestPopServerRpc(); // Let the server decide everything
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPopServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("📡 ServerRPC received for balloon: " + name);

        if (isPopped)
        {
            Debug.Log("❗ Already popped on server: " + name);
            return;
        }

        isPopped = true;

        BalloonMinigameManager.Instance?.OnBalloonPopped();

        if (NetworkObject.IsSpawned)
        {
            Debug.Log("🎈 Balloon popped and despawned: " + name);
            NetworkObject.Despawn(true); // Despawn network-wide
        }
        else
        {
            Debug.LogWarning($"⚠️ Tried to despawn unspawned balloon: {name}");
            Destroy(gameObject); // Fallback for scene-placed balloon
        }
    }
}
