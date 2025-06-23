using UnityEngine;
using System.Collections;

public class ProjectileWeapon : WeaponBase
{
    [Header("Projectile Settings")]
    [SerializeField] private float baseSpreadAngle = 0f;

    [Header("Burst Settings")]
    [SerializeField] private bool isBurstWeapon = false;
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float burstDelay = 0.1f;

    private bool isBurstFiring = false;

    protected override void Start()
    {
        base.Start();
    }

    public override void Shoot()
    {
        if (!CanShoot())
            return;

        if (isBurstWeapon)
        {
            if (!isBurstFiring)
                StartCoroutine(BurstFire());
        }
        else
        {
            FireSingleShot();
        }
    }

    private void FireSingleShot()
    {
        if (currentAmmo <= 0)
            return;

        currentAmmo--;
        UpdateEmissionIntensity();

        Vector3 direction = playerCamera.transform.forward;
        if (baseSpreadAngle > 0f)
            direction = ApplySpread(direction);

        Ray ray = new Ray(playerCamera.transform.position, direction);
        RaycastHit[] hits = Physics.RaycastAll(ray, range);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        float damageMultiplier = 1f;

        foreach (var hit in hits)
        {
            float finalDamage = damage * damageMultiplier;

            if (hit.collider.TryGetComponent(out LimbHealth limb))
            {
                float limbDamage = finalDamage;
                if (limb.limbID.ToLower().Contains("head"))
                    limbDamage *= headshotMultiplier;

                limb.TakeLimbDamageServerRpc(Mathf.RoundToInt(limbDamage));

                if (NetworkImpactSpawner.Instance != null)
                {
                    string fx = limb.limbID.ToLower().Contains("head") ? "BloodImpactHeadshot" : "BloodImpact";
                    NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, fx);
                }

                damageMultiplier *= 1f - damageFalloffPerPierce;
                continue;
            }

            if (hit.collider.TryGetComponent(out EntityHealth bodyEntity))
            {
                bool isPlayer = bodyEntity.CompareTag("Player");
                if (isPlayer && GameModeManager.Instance != null && GameModeManager.Instance.IsPvPMode)
                {
                    Debug.Log("⚠️ PvP is ON — skipping player damage.");
                }
                else
                {
                    bodyEntity.TakeDamageServerRpc(Mathf.RoundToInt(finalDamage));
                    NetworkImpactSpawner.Instance?.SpawnImpactEffectServerRpc(hit.point, hit.normal, "BloodImpact");
                }

                damageMultiplier *= 1f - damageFalloffPerPierce;
                continue;
            }

            if (NetworkImpactSpawner.Instance != null)
            {
                var balloon = hit.collider.GetComponent<Balloon>();
                if (balloon != null)
                {
                    Debug.Log("🎯 Hit a Balloon: " + balloon.name);
                    balloon.TryPop();
                }

                NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, "BulletImpact");
            }

            break; // stop at first surface hit
        }

        muzzleFlash?.Play();
    }

    private IEnumerator BurstFire()
    {
        isBurstFiring = true;

        for (int i = 0; i < burstCount; i++)
        {
            if (!CanShoot()) break;

            FireSingleShot();
            yield return new WaitForSeconds(burstDelay);
        }

        isBurstFiring = false;
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

    public float GetBaseSpreadAngle() => baseSpreadAngle;
    public void SetBaseSpreadAngle(float angle) => baseSpreadAngle = angle;

}
