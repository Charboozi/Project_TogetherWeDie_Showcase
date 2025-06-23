using UnityEngine;
using Unity.Netcode;

public class CameraShakeBroadcaster : NetworkBehaviour
{
    public static CameraShakeBroadcaster Instance;

    private void Awake()
    {
        Instance = this;
    }

    [ClientRpc]
    public void ShakeAllClientsClientRpc(float amount, float duration)
    {
        // Find and shake on local player
        foreach (var shake in FindObjectsByType<CameraShakeController>(FindObjectsSortMode.None))
        {
            shake.Shake(amount, duration);
        }
    }
}
