using UnityEngine;

public class TrapTriggerRandomActivator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float cooldownDuration = 10f;
    [SerializeField, Range(0f, 1f)] private float activationChance = 0.5f;

    private bool isOnCooldown = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy") || isOnCooldown) return;

        if (Random.value <= activationChance)
        {
            TrapBase trap = AffectableTrapsList.Instance?.GetRandomTrap();
            if (trap != null && !trap.isActive) // ✅ Avoid already active traps
            {
                trap.DoAction();
                Debug.Log($"🔥 Activated random trap: {trap.name}");
            }
        }

        StartCooldown();
    }

    private void StartCooldown()
    {
        isOnCooldown = true;
        Invoke(nameof(ResetCooldown), cooldownDuration);
    }

    private void ResetCooldown()
    {
        isOnCooldown = false;
    }
}
