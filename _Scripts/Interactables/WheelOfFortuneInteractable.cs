using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NetworkObject))]
public class WheelOfFortuneInteractable : NetworkBehaviour, IInteractorAwareAction
{
    [Header("Wheel Spawn Settings")]
    [SerializeField] private WheelItemSpawner wheelItemSpawner;

    [Header("Enemy Spawn Settings")]
    [SerializeField] private List<Transform> enemySpawnPoints = new List<Transform>();
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private ParticleSystem spawnEffectPrefab;

    [Header("Reward Chances")]
    [Range(0f, 1f)] public float weaponChance = 0.3f;
    [Range(0f, 1f)] public float keycardChance = 0.3f;
    [Range(0f, 1f)] public float powerupChance = 0.3f;

    [Header("Blood Payment Settings")]
    [SerializeField] private bool payWithHpOnSpin = false;    // ✅ Should we damage player when they spin?
    [SerializeField] private int hpCostOnSpin = 80;           // ✅ How much HP to pay if active


    private Animator animator;
    private int rewardIndex;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void DoAction(ulong interactorClientId)
    {
        if (!IsServer) return;

        if (payWithHpOnSpin)
        {
            TryDamageInteractor(interactorClientId);
        }

        rewardIndex = RollRandomReward();
        PlaySpinAnimation(rewardIndex);
    }

    public void DoAction() { }

    private int RollRandomReward()
    {
        float roll = Random.value;

        if (roll <= weaponChance)
            return 1;
        else if (roll <= weaponChance + keycardChance)
            return 2;
        else if (roll <= weaponChance + keycardChance + powerupChance)
            return 3;
        else
            return 4;
    }

    private void PlaySpinAnimation(int animIndex)
    {
        animator.SetInteger("animIndex", animIndex);
    }

    public void OnSpinAnimationComplete()
    {
        if (!IsServer) return;

        switch (rewardIndex)
        {
            case 1:
                RewardRandomWeapon();
                break;
            case 2:
                RewardKeycard();
                break;
            case 3:
                ActivateRandomPowerup();
                break;
            case 4:
                SpawnEnemyAtRandomPoint();
                break;
            default:
                Debug.LogWarning("[WheelOfFortune] Invalid reward index!");
                break;
        }

        animator.SetInteger("animIndex", 0);
    }

    private void RewardRandomWeapon()
    {
        if (wheelItemSpawner != null)
            wheelItemSpawner.SpawnRandomWeapon();
    }

    private void RewardKeycard()
    {
        if (wheelItemSpawner != null)
            wheelItemSpawner.SpawnRandomKeycard();
    }

    private void ActivateRandomPowerup()
    {
        if (wheelItemSpawner != null)
            wheelItemSpawner.SpawnRandomPowerup();
    }

    private void SpawnEnemyAtRandomPoint()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("[WheelOfFortune] No enemyPrefab assigned!");
            return;
        }

        if (enemySpawnPoints.Count < 3)
        {
            Debug.LogWarning("[WheelOfFortune] Not enough spawn points to spawn 3 enemies!");
            return;
        }

        // 🔥 Copy and shuffle spawn points
        List<Transform> shuffledPoints = new List<Transform>(enemySpawnPoints);
        for (int i = 0; i < shuffledPoints.Count; i++)
        {
            var temp = shuffledPoints[i];
            int randomIndex = Random.Range(i, shuffledPoints.Count);
            shuffledPoints[i] = shuffledPoints[randomIndex];
            shuffledPoints[randomIndex] = temp;
        }

        // 🔥 Spawn 3 enemies at the first 3 shuffled points
        for (int i = 0; i < 3; i++)
        {
            Transform spawnPoint = shuffledPoints[i];

            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            enemy.GetComponent<NetworkObject>().Spawn(true);

            Debug.Log($"[WheelOfFortune] ☠️ Spawned enemy at {spawnPoint.position}");

            // 🔥 Tell ALL clients to spawn the particle effect locally
            PlaySpawnEffectClientRpc(spawnPoint.position);
        }
    }

    private void TryDamageInteractor(ulong clientId)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var networkClient))
        {
            Debug.LogWarning("[WheelOfFortune] ❌ Could not find player to damage.");
            return;
        }

        var playerObject = networkClient.PlayerObject;
        var healthProxy = playerObject.GetComponent<HealthProxy>();
        if (healthProxy == null)
        {
            Debug.LogWarning("[WheelOfFortune] ❌ No HealthProxy on player!");
            return;
        }

        var entityHealth = healthProxy.GetComponent<EntityHealth>();
        if (entityHealth == null)
        {
            Debug.LogWarning("[WheelOfFortune] ❌ No EntityHealth on player!");
            return;
        }

        // ✅ Always apply the HP cost, even if it kills the player
        entityHealth.ApplyDamage(hpCostOnSpin);
        Debug.Log($"[WheelOfFortune] ❤️ Player {playerObject.name} paid {hpCostOnSpin} HP to spin (even if fatal).");
    }

    [ClientRpc]
    private void PlaySpawnEffectClientRpc(Vector3 position)
    {
        if (spawnEffectPrefab != null)
        {
            ParticleSystem effect = Instantiate(spawnEffectPrefab, position, Quaternion.Euler(90, 0, 0));
            Destroy(effect.gameObject, effect.main.duration + effect.main.startLifetime.constantMax);
        }
    }

}
