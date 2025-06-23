using UnityEngine;

public class ShotgunWeapon : WeaponBase
{
    [Header("Shotgun Settings")]
    public int pelletsPerShot = 6; // Number of pellets fired per shot
    public float spreadAngle = 10f; // Spread angle in degrees

    protected override void Start()
    {
        base.Start();
    }

    public override void Shoot()
    {
        if (!CanShoot()) return;

        currentAmmo--;
        UpdateEmissionIntensity();

        Debug.Log("Shotgun fired!");

        for (int i = 0; i < pelletsPerShot; i++)
        {
            FirePellet();
        }

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }


    private void FirePellet()
    {
        Ray ray = new Ray(playerCamera.transform.position, GetSpreadDirection());
        RaycastHit[] hits = Physics.RaycastAll(ray, range);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        float damageMultiplier = 1f;
        float basePelletDamage = damage / (float)pelletsPerShot;

        foreach (var hit in hits)
        {
            float finalDamage = basePelletDamage * damageMultiplier;

            // ✅ Try limb hit
            if (hit.collider.TryGetComponent(out LimbHealth limb))
            {
                if (limb.limbID.ToLower().Contains("head"))
                {
                    finalDamage *= headshotMultiplier;
                }

                limb.TakeLimbDamageServerRpc(Mathf.RoundToInt(finalDamage));

                if (NetworkImpactSpawner.Instance != null)
                {
                    string fx = limb.limbID.ToLower().Contains("head") ? "BloodImpactHeadshot" : "BloodImpact";
                    NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, fx);
                }

                damageMultiplier *= 1f - damageFalloffPerPierce;
                continue;
            }

            // ✅ Try body hit
            if (hit.collider.TryGetComponent(out EntityHealth entity))
            {
                if (entity.CompareTag("Player") && GameModeManager.Instance.IsPvPMode)
                    continue;

                entity.TakeDamageServerRpc(Mathf.RoundToInt(finalDamage));

                if (NetworkImpactSpawner.Instance != null)
                {
                    NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, "BloodImpact");
                }

                damageMultiplier *= 1f - damageFalloffPerPierce;
                continue;
            }

            // ✅ Environment hit (stop here)
            if (NetworkImpactSpawner.Instance != null)
            {
                NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, "BulletImpact");
            }

            break; // Solid surface stops the ray
        }
    }

    private Vector3 GetSpreadDirection()
    {
        float spreadX = Random.Range(-spreadAngle, spreadAngle);
        float spreadY = Random.Range(-spreadAngle, spreadAngle);
        Quaternion spreadRotation = Quaternion.Euler(spreadY, spreadX, 0);
        return spreadRotation * playerCamera.transform.forward;
    }
}
