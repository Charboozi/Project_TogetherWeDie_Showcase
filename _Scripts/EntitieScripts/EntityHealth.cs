// ✅ Cleaned-up EntityHealth.cs
using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections;

public class EntityHealth : NetworkBehaviour
{
    [Header("Special Enemy")]
    [SerializeField] private bool isSpecialEnemy = false;

    public static event Action<EntityHealth> OnSpecialEnemyKilled;
    public event Action<EntityHealth> OnDowned;
    public static event Action<ulong> OnPlayerDowned;
    public static event Action<ulong, Vector3> OnPlayerRevived;

    [Header("Health Settings")]
    public int maxHealth = 100;
    public int armor = 0;
    private Coroutine regenCoroutine;

    [Header("Regeneration Settings")]
    public bool enableRegeneration = true;
    public int regenAmount = 2;
    public float regenInterval = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Limb Effects")]
    [SerializeField] private ParticleSystem headDetachEffect;

    public NetworkVariable<int> currentHealth = new(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isDowned = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public string lastHitLimbID = "None";
    public event Action OnTakeDamage;

    private CameraShakeController cameraShake;

    private void Awake()
    {
        if (IsOwner)
            cameraShake = FindFirstObjectByType<CameraShakeController>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
            isDowned.Value = false;

            if (CompareTag("Player"))
                GameOverManager.Instance?.RegisterPlayer(this);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && CompareTag("Player"))
            GameOverManager.Instance?.UnregisterPlayer(this);
    }

    public void OnLimbHit(string limbID, int damage)
    {
        lastHitLimbID = limbID;
        TakeDamageServerRpc(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        ApplyDamageInternal(damage);
    }

    public void ApplyDamage(int damage)
    {
        if (!IsServer) return;
        ApplyDamageInternal(damage);
    }

    private void ApplyDamageInternal(int damage)
    {
        if (currentHealth.Value <= 0) return;

        int effective = Mathf.Max(damage - armor, 1);
        currentHealth.Value -= effective;

        OnTakeDamage?.Invoke();
        TakeDamageClientRpc();

        if (currentHealth.Value <= 0)
        {
            if (CompareTag("Player"))
                HandleDowned();
            else
                HandleDeath();
        }

        RestartHealthRegen();

        if (CompareTag("Player"))
            ApplySlowEffectClientRpc(0.3f, 2f);
    }

    private void HandleDowned()
    {
        if (isDowned.Value) return;

        isDowned.Value = true;
        OnDowned?.Invoke(this);
        DownedClientRpc();
        OnPlayerDowned?.Invoke(OwnerClientId);
        ShowDownedFeedMessageClientRpc(GetSteamNameFromNameTag());
    }

    private void HandleDeath()
    {
        DieClientRpc();
        NotifySoulCollectors();

        if (isSpecialEnemy)
        {
            Debug.Log($"[SpecialKill] Special enemy '{gameObject.name}' killed.");
            OnSpecialEnemyKilled?.Invoke(this);
        }
    }

    private void RestartHealthRegen()
    {
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;
        }
        Invoke(nameof(StartHealthRegen), regenInterval * 2);
    }

    private void StartHealthRegen()
    {
        if (!IsServer || !enableRegeneration || isDowned.Value || CompareTag("Enemy")) return;

        if (regenCoroutine == null)
            regenCoroutine = StartCoroutine(HealthRegenCoroutine());
    }

    private IEnumerator HealthRegenCoroutine()
    {
        while (currentHealth.Value < maxHealth)
        {
            yield return new WaitForSeconds(regenInterval);
            currentHealth.Value = Mathf.Min(currentHealth.Value + regenAmount, maxHealth);
        }
        regenCoroutine = null;
    }

    [ClientRpc]
    private void TakeDamageClientRpc()
    {
        if (!IsOwner || CompareTag("Enemy")) return;

        FadeScreenEffect.Instance.ShowEffect(Color.red, 0.5f, 4f);
        audioSource?.PlayOneShot(damageSound);
        cameraShake?.Shake(0.1f, 0.15f);
    }

    [ClientRpc]
    private void DieClientRpc()
    {
        Debug.Log($"{gameObject.name} has died!");

        if (TryGetComponent<IKillable>(out var killable))
            killable.Die();

        if (lastHitLimbID.ToLower().Contains("head") || lastHitLimbID.ToLower().Contains("core"))
        {
            var limb = FindLimbByID(lastHitLimbID);
            if (limb != null)
                DetachLimb(limb);
        }
    }

    private Transform FindLimbByID(string id)
    {
        foreach (var limb in GetComponentsInChildren<LimbHealth>())
        {
            if (limb.limbID.Equals(id, StringComparison.OrdinalIgnoreCase))
                return limb.transform;
        }
        return null;
    }

    private void DetachLimb(Transform limb)
    {
        limb.localScale = Vector3.one * 0.001f;
        if (!limb.GetComponent<Collider>())
            limb.gameObject.AddComponent<BoxCollider>();

        if (lastHitLimbID.ToLower().Contains("head") && headDetachEffect != null)
            headDetachEffect.Play();
    }

    [ClientRpc]
    private void DownedClientRpc()
    {
        if (!IsOwner || !CompareTag("Player")) return;

        PlayerInput.CanInteract = false;
        FadeScreenEffect.Instance.ShowDownedEffect();

        if (GameModeManager.Instance?.IsPvPMode == true)
        {
            var freeCam = FindFirstObjectByType<TrailerFreeCam>();
            freeCam?.SendMessage("ActivateFreeCam", SendMessageOptions.DontRequireReceiver);
        }
    }

    [ClientRpc]
    private void ApplySlowEffectClientRpc(float slowFactor, float duration)
    {
        if (IsOwner && TryGetComponent(out NetworkedCharacterMovement movement))
            movement.ApplyTemporarySlow(slowFactor, duration);
    }

    public void FullHeal()
    {
        if (!CompareTag("Player")) return;

        currentHealth.Value = maxHealth;
        isDowned.Value = false;
        PlayReviveEffectClientRpc();
        EnableInteractionClientRpc();
        RestartHealthRegen();
    }

    public void Revive()
    {
        if (!CompareTag("Player")) return;

        currentHealth.Value = Mathf.Min(10, maxHealth);
        isDowned.Value = false;
        OnPlayerRevived?.Invoke(OwnerClientId, transform.position);
        PlayReviveEffectClientRpc();
        EnableInteractionClientRpc();
        RestartHealthRegen();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReviveFromHealingServerRpc()
    {
        if (isDowned.Value)
            Revive();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ApplyHealingServerRpc(int amount)
    {
        if (currentHealth.Value <= 0 || isDowned.Value) return;

        currentHealth.Value = Mathf.Min(currentHealth.Value + amount, maxHealth);
        PlayHealingEffectClientRpc();
    }

    [ClientRpc]
    private void PlayHealingEffectClientRpc()
    {
        if (IsOwner)
            FadeScreenEffect.Instance.ShowEffect(Color.green, 0.05f, 1f);
    }

    [ClientRpc]
    private void PlayReviveEffectClientRpc()
    {
        if (IsOwner && CompareTag("Player"))
            FadeScreenEffect.Instance.ShowReviveEffect();
    }

    [ClientRpc]
    private void EnableInteractionClientRpc()
    {
        if (IsOwner)
            PlayerInput.CanInteract = true;
    }

    private void NotifySoulCollectors()
    {
        if (!IsServer) return;

        foreach (var collector in FindObjectsByType<SoulCollector>(FindObjectsSortMode.None))
            collector.RegisterSoulServerRpc(transform.position);
    }

    private string GetSteamNameFromNameTag()
    {
        var current = transform;
        while (current != null)
        {
            if (current.TryGetComponent<PlayerNameTag>(out var tag))
                return string.IsNullOrWhiteSpace(tag.GetPlayerName()) ? null : tag.GetPlayerName();
            current = current.parent;
        }
        return SteamManager.Instance?.GetPlayerName() ?? $"DemoPlayer_{OwnerClientId}";
    }

    [ClientRpc]
    private void ShowDownedFeedMessageClientRpc(string playerName)
    {
        GameFeedManager.Instance?.PostFeedMessage($"{playerName} is downed!", Color.red);
    }

    public void AddArmor(int bonus)
    {
        armor += bonus;
        Debug.Log($"{gameObject.name}: Armor increased by {bonus}. Total: {armor}");
    }

    public void RemoveArmor(int bonus)
    {
        armor = Mathf.Max(0, armor - bonus);
        Debug.Log($"{gameObject.name}: Armor decreased by {bonus}. Total: {armor}");
    }
}
