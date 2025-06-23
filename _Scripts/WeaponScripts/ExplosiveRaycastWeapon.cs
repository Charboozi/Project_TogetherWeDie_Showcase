using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class ExplosiveRaycastWeapon : WeaponBase
{
    [Header("Rocket Launcher Settings")]
    [SerializeField] private float splashRadius = 5f;
    [SerializeField] private int baseDamage = 100;
    [SerializeField] private float baseSpreadAngle = 0f;
    [SerializeField] private string impactEffect;

    protected override void Start()
    {
        base.Start();
    }

    public override void Shoot()
    {
        if (!CanShoot() || currentAmmo <= 0)
            return;

        currentAmmo--;
        UpdateEmissionIntensity();

        Vector3 direction = playerCamera.transform.forward;

        // Add optional spread
        if (baseSpreadAngle > 0f)
            direction = ApplySpread(direction);

        Ray ray = new Ray(playerCamera.transform.position, direction);
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            // Spawn VFX (tell server to spawn for all)
            if (NetworkImpactSpawner.Instance != null && !string.IsNullOrEmpty(impactEffect))
            {
                NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, impactEffect);
            }

            // Apply splash damage to all entities around the hit
            ApplySplashDamage(hit.point);
        }

        muzzleFlash?.Play();
        WeaponController.Instance?.TriggerShootEffect();
    }

    private Vector3 ApplySpread(Vector3 direction)
    {
        float spreadRadius = Mathf.Tan(baseSpreadAngle * Mathf.Deg2Rad);
        Vector2 randomPoint = Random.insideUnitCircle * spreadRadius;

        Vector3 spreadDirection = direction;
        spreadDirection += playerCamera.transform.right * randomPoint.x;
        spreadDirection += playerCamera.transform.up * randomPoint.y;

        return spreadDirection.normalized;
    }

    private void ApplySplashDamage(Vector3 center)
    {
        Collider[] hitColliders = Physics.OverlapSphere(center, splashRadius);
        HashSet<EntityHealth> damaged = new HashSet<EntityHealth>();

        foreach (Collider col in hitColliders)
        {
            if (col.TryGetComponent(out EntityHealth entity) && !damaged.Contains(entity))
            {
                if (entity.CompareTag("Player") && GameModeManager.Instance != null && GameModeManager.Instance.IsPvPMode)
                    continue;

                damaged.Add(entity);

                Vector3 closest = col.ClosestPoint(center);
                float dist = Vector3.Distance(center, closest);
                float multiplier = Mathf.Clamp01(1f - dist / splashRadius);
                int finalDamage = Mathf.RoundToInt(baseDamage * multiplier);

                if (finalDamage > 0)
                    entity.TakeDamageServerRpc(finalDamage); // ✅ works because EntityHealth is NetworkBehaviour
            }
        }
    }
}

