using UnityEngine;

public class HealingWeapon : WeaponBase
{
    [Header("Healing Settings")]
    [SerializeField] private float baseSpreadAngle = 0f;
    [SerializeField] private int healingAmount = 10;
    [SerializeField] private bool canHealDownedPlayers = false;
    [SerializeField] private float splashRadius = 0f; // 0 = single target, > 0 = area healing

    protected override void Start()
    {
        base.Start();
    }

    public override void Shoot()
    {
        if (!CanShoot())
            return;

        currentAmmo--;
        UpdateEmissionIntensity();

        Vector3 direction = playerCamera.transform.forward;

        if (baseSpreadAngle > 0f)
            direction = ApplySpread(direction);

        Ray ray = new Ray(playerCamera.transform.position, direction);
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            // 🔷 Show healing impact VFX
            if (NetworkImpactSpawner.Instance != null)
                NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, "HealImpact");

            // 🔷 Apply healing (single or splash)
            if (splashRadius <= 0f)
            {
                // 🔹 Heal single target
                if (hit.collider.TryGetComponent(out EntityHealth entity))
                {
                    TryHealEntity(entity);
                }
            }
            else
            {
                // 🔹 Splash healing
                Collider[] colliders = Physics.OverlapSphere(hit.point, splashRadius);
                foreach (var col in colliders)
                {
                    if (col.TryGetComponent(out EntityHealth entity))
                    {
                        TryHealEntity(entity);
                    }
                }
            }

            // 🔷 Weapon visuals
            muzzleFlash?.Play();
            WeaponController.Instance?.TriggerShootEffect();
        }
    }

    private void TryHealEntity(EntityHealth entity)
    {
        if (entity == null || !entity.CompareTag("Player"))
            return;

        bool isDowned = entity.isDowned.Value;
        bool canHealThis = !isDowned || (isDowned && canHealDownedPlayers);

        if (!canHealThis || entity.currentHealth.Value >= entity.maxHealth)
            return;

        entity.ApplyHealingServerRpc(healingAmount);

        // ✅ Revive after healing if allowed
        if (isDowned && canHealDownedPlayers)
        {
            entity.ReviveFromHealingServerRpc();
        }
    }

    private Vector3 ApplySpread(Vector3 direction)
    {
        if (baseSpreadAngle <= 0f)
            return direction;

        float spreadRadius = Mathf.Tan(baseSpreadAngle * Mathf.Deg2Rad);
        Vector2 randomPoint = Random.insideUnitCircle * spreadRadius;

        Vector3 spreadDirection = direction;
        spreadDirection += playerCamera.transform.right * randomPoint.x;
        spreadDirection += playerCamera.transform.up * randomPoint.y;

        return spreadDirection.normalized;
    }
}
