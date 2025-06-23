using UnityEngine;
using System.Collections;

public class ContinuousFireWeapon : WeaponBase
{
    [Header("Continuous Fire Settings")]
    [SerializeField] private float initialChargeDelay = 1f;
    [SerializeField] private float damageInterval = 0.1f;
    [SerializeField] private float splashRadius = 2f;
    [SerializeField] private string impactEffect; 

    private PlayerControls input;
    private bool isFiring = false;

    private Coroutine firingRoutine;

    public override bool HandlesInput => true;

    private void OnEnable()
    {
        if (input == null)
        {
            input = new PlayerControls();
            input.Player.Fire.performed += ctx => StartFiring();
            input.Player.Fire.canceled += ctx => StopFiring();
        }
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }


    private IEnumerator FireRoutine()
    {
        if (muzzleFlash != null && !muzzleFlash.isPlaying)
            muzzleFlash.Play();

        yield return new WaitForSeconds(initialChargeDelay);

        while (CanShoot() && isFiring)
        {
            Shoot();
            yield return new WaitForSeconds(damageInterval);
        }

        StopFiring();
    }

    private void StartFiring()
    {
        if (!CanShoot() || firingRoutine != null)
            return;

        isFiring = true;
        firingRoutine = StartCoroutine(FireRoutine());
    }

    private void StopFiring()
    {
        isFiring = false;

        if (firingRoutine != null)
        {
            StopCoroutine(firingRoutine);
            firingRoutine = null;
        }

        if (muzzleFlash != null && muzzleFlash.isPlaying)
            muzzleFlash.Stop();
    }

    public override void Shoot()
    {
        if (!CanShoot() || currentAmmo <= 0) return;

        currentAmmo--;
        UpdateEmissionIntensity();

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, range);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        float damageMultiplier = 1f;

        foreach (RaycastHit hit in hits)
        {
            // Burn poster (if applicable)
            if (hit.collider.TryGetComponent(out FlammablePoster poster))
            {
                poster.BurnRequest(damageInterval);
            }

            // Splash damage based on scaled damage
            ApplySplashDamage(hit.point, Mathf.RoundToInt(damage * damageMultiplier));

            // Visual impact effect
            if (NetworkImpactSpawner.Instance != null && !string.IsNullOrEmpty(impactEffect))
            {
                NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, impactEffect);
            }

            // Apply damage falloff for next pierce
            damageMultiplier *= 1f - damageFalloffPerPierce;
            if (damageMultiplier <= 0.05f) break; // Optional: stop ray if damage becomes negligible
        }

        WeaponController.Instance?.TriggerShootEffect();
    }

    private void ApplySplashDamage(Vector3 center, int scaledDamage)
    {
        Collider[] hitObjects = Physics.OverlapSphere(center, splashRadius);
        foreach (Collider obj in hitObjects)
        {
            if (obj.TryGetComponent(out EntityHealth entity))
            {
                if (entity.CompareTag("Player") && GameModeManager.Instance.IsPvPMode)
                    continue;

                entity.TakeDamageServerRpc(scaledDamage);
            }
        }
    }
}
