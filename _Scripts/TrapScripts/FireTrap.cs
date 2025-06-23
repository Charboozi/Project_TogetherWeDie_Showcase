using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Interactable))]
public class FireTrap : TrapBase
{
    [Header("Fire Trap Settings")]
    [SerializeField] private int damagePerTick = 10;
    [SerializeField] private float damageInterval = 1f;

    [Header("Fire Trap Effects")]
    [SerializeField] private List<ParticleSystem> fireEffects = new List<ParticleSystem>();

    [Header("Detection")]
    [SerializeField] private Collider detectionTrigger;

    private HashSet<EntityHealth> victimsInRange = new HashSet<EntityHealth>();
    private Coroutine damageCoroutine;

    protected override void OnTrapActivated()
    {
        // ✅ Scan trigger manually for victims already inside
        ScanTriggerForVictims();

        damageCoroutine = StartCoroutine(DamageOverTimeCoroutine());

        foreach (var fireEffect in fireEffects)
        {
            fireEffect?.Play();
        }
    }

    protected override void OnTrapDeactivated()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }

        victimsInRange.Clear();

        foreach (var fireEffect in fireEffects)
        {
            fireEffect?.Stop();
        }
    }

    public void NotifyTriggerEnter(Collider other)
    {
        if (!IsServer || !isActive) return;

        if (other.TryGetComponent(out EntityHealth victim))
        {
            victimsInRange.Add(victim);
        }
    }

    public void NotifyTriggerExit(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent(out EntityHealth victim))
        {
            victimsInRange.Remove(victim);
        }
    }

    private IEnumerator DamageOverTimeCoroutine()
    {
        while (isActive)
        {
            foreach (var victim in victimsInRange)
            {
                if (victim == null) continue;

                int finalDamage = damagePerTick;

                if (victim.CompareTag("Player")) // ✅ Only for players
                {
                    finalDamage = Mathf.Max(1, damagePerTick / 2); // Never less than 1
                }

                victim.TakeDamageServerRpc(finalDamage);
            }

            yield return new WaitForSeconds(damageInterval);
        }
    }

    private void ScanTriggerForVictims()
    {
        Collider[] hits = Physics.OverlapBox(
            detectionTrigger.bounds.center,
            detectionTrigger.bounds.extents,
            detectionTrigger.transform.rotation
        );

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out EntityHealth victim))
            {
                victimsInRange.Add(victim);
            }
        }
    }

    private void OnValidate()
    {
        if (detectionTrigger != null)
        {
            var trigger = detectionTrigger.GetComponent<TrapTrigger>();
            if (trigger == null)
            {
                trigger = detectionTrigger.gameObject.AddComponent<TrapTrigger>();
            }
            trigger.fireTrap = this;
        }
    }

    public override void Prewarm()
    {
        base.Prewarm(); // Prewarm physics/audio first

        // Prewarm all fire effects
        foreach (var fireEffect in fireEffects)
        {
            if (fireEffect != null)
            {
                fireEffect.Play();
                fireEffect.Stop();
            }
        }

        // Prewarm trigger collider query
        if (detectionTrigger != null)
        {
            Physics.OverlapBox(detectionTrigger.bounds.center, detectionTrigger.bounds.extents, detectionTrigger.transform.rotation);
        }
    }

}
