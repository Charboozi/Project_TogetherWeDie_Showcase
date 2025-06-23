using UnityEngine;

public class DoorTriggerRandomCloser : MonoBehaviour
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
            SlidingDoor door = AffectableDoorsList.Instance?.GetRandomDoor();
            if (door != null)
            {
                door.Close();
                Debug.Log($"🔒 Closed random door: {door.name}");
            }
        }

        StartCooldown(); // Cooldown always starts even if no door is closed
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
