using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using System;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(TargetScanner))]
public class EnemyAttack : NetworkBehaviour
{
    [Header("Attack Settings")]
    public int attackDamage = 10;
    public float attackCooldown = 1.2f;
    public float rotationSpeed = 10f;
    [Tooltip("Radius of the attack hit area (for OverlapSphere)")]
    public float attackRadius = 2f;

    [Header("Dynamic Scaling")]
    [Tooltip("How much to increase damage per active day")]
    public float damageIncreasePerDay = 0.25f;

    [Header("Audio")]
    public AudioClip hitSound;

    private AudioSource audioSource;
    private RandomSoundPlayer randomSoundPlayer;
    private Collider enemyCollider;
    private NavMeshAgent agent;
    private IEnemyAnimationHandler enemyAnimation;

    private Transform target;
    private bool isAttacking = false;
    private float cooldownTimer = 0f;
    private bool isDead = false;

    private int daysSinceActivation = 0;

    public event Action<Transform> OnTargetHit;

    private TargetScanner scanner;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        scanner = GetComponent<TargetScanner>();
        enemyCollider = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
        randomSoundPlayer = GetComponent<RandomSoundPlayer>();
        enemyAnimation = GetComponent<IEnemyAnimationHandler>()
                          ?? NullEnemyAnimationHandler.Instance;

        if (TryGetComponent(out EnemyDeathHandler deathHandler))
        {
            deathHandler.OnEnemyDeath += HandleDeath;
        }
    }

    public override void OnNetworkSpawn()
    {

        if (IsServer && DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDayStarted += OnNewDayStarted;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDayStarted -= OnNewDayStarted;
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        // ✅ Keep rotating toward target even while attacking
        if (target != null && isAttacking)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0f; // Ignore vertical rotation
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void HandleDeath()
    {
        isDead = true;
        StopAttack();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void TargetInRange()
    {
        if (isDead || isAttacking || target == null || cooldownTimer > 0f) return;

        // ✅ Prevent attacking downed targets
        if (target.TryGetComponent<EntityHealth>(out var health))
        {
            if (health.isDowned.Value)
            {
                StopAttack();

                // ✅ Optionally tell movement to clear target and roam
                if (TryGetComponent<EnemyMovement>(out var movement))
                {
                    movement.ClearTarget();
                }

                return;
            }
        }

        StartAttack();
    }

    public void StopAttack()
    {
        isAttacking = false;
    }

    private void StartAttack()
    {
        isAttacking = true;
        cooldownTimer = attackCooldown;

        if (agent.enabled)
            agent.isStopped = true;

        if (enemyAnimation.HasAttackAnimation)
        {
            enemyAnimation.TriggerAttack();
            PlayAttackAnimationClientRpc();

            CancelInvoke(nameof(OnAttackAnimationComplete));
            Invoke(nameof(OnAttackAnimationComplete), 2.5f);
        }
        else
        {
            TryDoDamage();
            Invoke(nameof(OnAttackAnimationComplete), attackCooldown);
        }
    }

    public void TryDoDamage()
    {
        if (!IsServer || target == null)
            return;

        // Fallback origin if attackOrigin isn't assigned
        Vector3 origin = transform.position + Vector3.up * 1.2f; // around chest height

        float radius = attackRadius;

        // Only hit player layer by default (layer 6 is usually "Player" if using default setup)
        int playerLayer = LayerMask.NameToLayer("Player");
        int attackLayerMask = 1 << playerLayer;

        Collider[] hits = Physics.OverlapSphere(origin, attackRadius, attackLayerMask);

        foreach (var col in hits)
        {
            if (!col.transform.IsChildOf(target))
                continue;

            if (col.transform.TryGetComponent(out EntityHealth health))
            {
                OnTargetHit?.Invoke(col.transform);
                PlayHitSoundClientRpc();
                health.TakeDamageServerRpc(attackDamage);
                return;
            }
        }

        // Optional debug draw (shows red wire sphere for 1 second)
        Debug.DrawLine(origin, origin + Vector3.up * 0.2f, Color.red, 1f);
        Debug.DrawRay(origin, transform.forward * attackRadius, Color.red, 1f);
    }


    public void OnAttackAnimationComplete()
    {
        CancelInvoke(nameof(OnAttackAnimationComplete));

        isAttacking = false;

        if (agent.enabled)
            agent.isStopped = false;
    }

    [ClientRpc]
    private void PlayAttackAnimationClientRpc()
    {
        enemyAnimation.TriggerAttack();
    }

    [ClientRpc]
    private void PlayHitSoundClientRpc()
    {
        if (randomSoundPlayer != null)
            randomSoundPlayer.StopSounds();

        if (audioSource.isPlaying)
            audioSource.Stop();

        if (hitSound != null)
            audioSource.PlayOneShot(hitSound);
    }
    
    private void OnNewDayStarted(int day)
    {
        if (!IsServer || isDead) return;

        daysSinceActivation++;
        attackDamage = Mathf.RoundToInt(attackDamage + damageIncreasePerDay);

        Debug.Log($"[EnemyAttack] Day {day}: Damage increased to {attackDamage}");
    }
}
