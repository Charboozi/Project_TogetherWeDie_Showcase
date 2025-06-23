using UnityEngine;
using Unity.Netcode;
using System.Collections;
using UnityEngine.AI;
using System;

[RequireComponent(typeof(NavMeshAgent), typeof(CapsuleCollider), typeof(PowerupSpawner))]
public class EnemyDeathHandler : NetworkBehaviour, IKillable
{
    [Header("Death Settings")]
    public float destroyDelay = 10f;

    [SerializeField]private Animator animator;

    [Header("Components to Disable on Death")]
    [SerializeField] private Component[] componentsToDisable;

    [Header("GameObjects to Disable on Death")]
    [SerializeField] private GameObject[] objectsToDisable;

    private NavMeshAgent agent;
    private CapsuleCollider capsule;
    private PowerupSpawner powerupSpawner;
    private IEnemyAnimationHandler enemyAnimation;

    public AudioClip[] deathClips; // Assign in Inspector
    private AudioSource audioSource;
    private RandomSoundPlayer randomSoundPlayer;
    private int lastPlayedClipIndex = -1;

    private bool isDead = false;

    public event Action OnEnemyDeath;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        capsule = GetComponent<CapsuleCollider>();
        powerupSpawner = GetComponent<PowerupSpawner>();

        enemyAnimation = GetComponent<IEnemyAnimationHandler>() ?? NullEnemyAnimationHandler.Instance;

        audioSource = GetComponent<AudioSource>();
        randomSoundPlayer = GetComponent<RandomSoundPlayer>();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        agent.isStopped = true;
        agent.ResetPath();
        agent.enabled = false;
        capsule.enabled = false;

        DisableComponents();

        enemyAnimation.PlayDeath();
        randomSoundPlayer?.StopSounds();

        // Pick a random clip and store index
        if (deathClips.Length > 0)
        {
            lastPlayedClipIndex = UnityEngine.Random.Range(0, deathClips.Length);
            PlayDeathClipClientRpc(lastPlayedClipIndex);
        }

        powerupSpawner?.TrySpawn();
        StopAllCoroutines();
        StartCoroutine(DestroyAfterDelay());
        OnEnemyDeath?.Invoke();
    }

private void DisableComponents()
{
    foreach (var component in componentsToDisable)
    {
        if (component == null) continue;

        switch (component)
        {
            case MonoBehaviour monoBehaviour:
                monoBehaviour.enabled = false;
                break;

            case Collider collider:
                collider.enabled = false;
                break;

            case Rigidbody rigidbody:
                rigidbody.isKinematic = true;
                rigidbody.linearVelocity = Vector3.zero;
                break;

            case AudioSource audioSource:
                audioSource.enabled = false;
                break;

            case NavMeshAgent navMeshAgent:
                navMeshAgent.enabled = false;
                break;

            case ParticleSystem particleSystem:
                particleSystem.Stop();
                break;

            case Renderer renderer:
                renderer.enabled = false;
                break;

            // Add more types if needed!

            default:
                Debug.LogWarning($"Component type '{component.GetType().Name}' is not handled in DisableComponents(). Add support if needed.");
                break;
        }
    }

    foreach (var obj in objectsToDisable)
    {
        if (obj != null)
        {
            obj.SetActive(false);
        }
    }
}


    [ClientRpc]
    private void PlayDeathClipClientRpc(int clipIndex)
    {
        if (clipIndex < 0 || clipIndex >= deathClips.Length || audioSource == null)
            return;

        audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(deathClips[clipIndex]);
    }


    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);

        if (IsServer)
        {
            NetworkObject netObj = GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsSpawned)
                netObj.Despawn(true);

            Destroy(gameObject);
        }
    }
}
