using UnityEngine;

public class ExplosiveWeapon : WeaponBase
{
    [Header("Explosive Weapon Settings")]
    [Tooltip("The explosive projectile prefab. This should have an ExplosiveProjectile script attached.")]
    public GameObject explosiveProjectilePrefab;

    [Tooltip("The force to apply to the projectile when fired.")]
    public float projectileForce = 1000f;

    [Header("Multi-Shot Settings")]
    [Tooltip("Number of projectiles to fire at once.")]
    public int projectileCount = 1;

    [Tooltip("Spread angle in degrees.")]
    public float spreadAngle = 5f;

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

        if (explosiveProjectilePrefab != null && muzzleTransform != null)
        {
            for (int i = 0; i < projectileCount; i++)
            {
                GameObject projectileObj = Instantiate(explosiveProjectilePrefab, muzzleTransform.position, muzzleTransform.rotation);

                Rigidbody rb = projectileObj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Add slight random spread to direction
                    Vector3 spreadDirection = muzzleTransform.forward;
                    spreadDirection += muzzleTransform.right * Random.Range(-spreadAngle, spreadAngle) * 0.01f;
                    spreadDirection += muzzleTransform.up * Random.Range(-spreadAngle, spreadAngle) * 0.01f;
                    spreadDirection.Normalize();

                    // 🎯 Apply random force variation
                    float randomForceMultiplier = Random.Range(0.5f, 2f); // +/-10% variation
                    float finalForce = projectileForce * randomForceMultiplier;

                    rb.AddForce(spreadDirection * finalForce);
                }
            }
        }
        else
        {
            Debug.LogWarning("Explosive projectile prefab or muzzle transform not assigned!");
        }

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }
}
