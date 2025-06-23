using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ExplosionManager : NetworkBehaviour
{
    public static ExplosionManager Instance { get; private set; }

    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private int baseDamage = 50;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Called by any client to request an explosion at world-space position.
    /// Server then does the real OverlapSphere and damage.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void ExplodeServerRpc(Vector3 explosionCenter)
    {
        // find every collider in the blast
        var hits    = Physics.OverlapSphere(explosionCenter, explosionRadius);
        var touched = new HashSet<EntityHealth>();

        foreach (var col in hits)
        {
            var entity = col.GetComponentInParent<EntityHealth>();
            if (entity == null || touched.Contains(entity))
                continue;

            touched.Add(entity);

            // measure exact surface distance
            var closest = col.ClosestPoint(explosionCenter);
            float dist  = Vector3.Distance(explosionCenter, closest);
            float mul   = Mathf.Clamp01(1f - dist / explosionRadius);
            int   dmg   = Mathf.RoundToInt(baseDamage * mul);

            if (dmg > 0)
                entity.ApplyDamage(dmg); // direct server‐only call
        }
    }
}
