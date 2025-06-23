using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class ExplosiveProjectile : NetworkBehaviour
{
    [Header("Explosion Settings")]
    public float explosionRadius = 5f;          // Radius within which damage is applied.
    public float explosionDamage = 50f;         // Maximum damage at the center.
    public float selfDestructTime = 5f;           // Lifetime of the projectile if it doesn't hit.
    public GameObject explosionEffectPrefab;    // Name of the impact effect prefab (in Resources/ImpactEffects)

    private bool hasExploded = false;

    void Start()
    {
        Destroy(gameObject, selfDestructTime); // Ensure it doesn't live forever.
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasExploded)
            return;
        hasExploded = true;

        // Use the NetworkImpactSpawner to spawn the explosion effect across all clients.
        if (NetworkImpactSpawner.Instance != null && explosionEffectPrefab != null)
        {
            // You can pass the projectile's position and the collision's contact normal.
            NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(transform.position, collision.contacts[0].normal, explosionEffectPrefab.name);
        }

        // 💥 Apply splash damage
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            if (hit.TryGetComponent(out EntityHealth entity))
            {
                // 🛡️ Block damage if it's a player and PvP is ON
                if (entity.CompareTag("Player") && GameModeManager.Instance.IsPvPMode)
                    continue;

                float distance = Vector3.Distance(transform.position, hit.transform.position);
                float multiplier = Mathf.Clamp01(1f - (distance / explosionRadius));
                int damageToApply = Mathf.RoundToInt(explosionDamage * multiplier);

                entity.TakeDamageServerRpc(damageToApply);
            }
        }

        Destroy(gameObject);
    }
}
