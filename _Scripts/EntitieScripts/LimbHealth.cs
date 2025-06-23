using UnityEngine;
using Unity.Netcode;

public class LimbHealth : NetworkBehaviour
{
    [Tooltip("Unique name for this limb, e.g., 'Head', 'Leg1', 'MandibleL'")]
    public string limbID;

    [SerializeField] private EntityHealth entityHealth;

    [ServerRpc(RequireOwnership = false)]
    public void TakeLimbDamageServerRpc(int amount)
    {
        entityHealth?.OnLimbHit(limbID, amount);
    }
}
