using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class SelfRevive : BaseEquipment
{
    [Header("Self Revive Settings")]
    [SerializeField] private float selfReviveDelay = 3f;

    private HealthProxy healthProxy;
    private EntityHealth entityHealth;
    private bool used = false;

    private void OnEnable()
    {
        used = false;

        var localPlayer = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (localPlayer == null || !localPlayer.IsOwner) return;

        healthProxy = localPlayer.GetComponent<HealthProxy>();
        entityHealth = FindNetworkedPlayerEntityHealth();

        if (entityHealth != null)
        {
            entityHealth.isDowned.OnValueChanged += OnDownedChanged;
        }
    }

    private void OnDisable()
    {
        if (entityHealth != null)
        {
            entityHealth.isDowned.OnValueChanged -= OnDownedChanged;
        }
    }

    private void OnDownedChanged(bool previous, bool current)
    {
        if (current && !used)
        {
            used = true;
            StartCoroutine(DelayedSelfRevive());
        }
    }

    private IEnumerator DelayedSelfRevive()
    {
        float timer = 0f;

        while (timer < selfReviveDelay)
        {
            if (entityHealth == null || !entityHealth.isDowned.Value)
            {
                used = false; // someone else revived us
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        healthProxy?.Revive(); // 🔁 Forward to server via HealthProxy

        // Remove the equipment after use
        var inv = GetComponentInParent<EquipmentInventory>();
        inv?.Unequip(gameObject.name);
    }

    // Try to find the networked EntityHealth that belongs to us
    private EntityHealth FindNetworkedPlayerEntityHealth()
    {
        foreach (var health in FindObjectsByType<EntityHealth>(FindObjectsSortMode.None))
        {
            if (health.IsOwner && health.CompareTag("Player"))
            {
                return health;
            }
        }
        return null;
    }

    public override void Upgrade()
    {
        Debug.Log($"{gameObject.name} cannot be upgraded (yet).");
    }
}
