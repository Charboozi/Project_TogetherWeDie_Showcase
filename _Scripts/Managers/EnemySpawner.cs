using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : NetworkBehaviour
{
    [Header("Spawn Settings")]
    public List<Transform> spawnPoints;
    public GameObject enemyPrefab;

    [Header("Spawn Interval Settings")]
    [Tooltip("Initial spawn interval in seconds")]
    public float spawnIntervalStart = 15f;

    [Tooltip("Final/minimum spawn interval in seconds")]
    public float spawnIntervalEnd = 6f;

    [Tooltip("Curve controlling spawn interval progression (X = active day, Y = 1 to 0)")]
    public AnimationCurve spawnRateCurve = AnimationCurve.Linear(0, 1, 30, 0);

    [Header("Max Enemies Settings")]
    [Tooltip("Curve controlling max enemies over time (X = active day, Y = max enemies)")]
    public AnimationCurve maxEnemiesCurve = AnimationCurve.Linear(0, 10, 30, 50);

    [Header("Activation Settings")]
    [Tooltip("Global day this spawner becomes active")]
    public int activateOnDay = 0;

    private Coroutine spawnCoroutine;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool isSpawnerActive = false;

    private int daysSinceActivation = 0;
    private float currentSpawnInterval;
    private int currentMaxEnemies;

    private void Start()
    {
        if (IsServer && DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDayStarted += OnNewDayStarted;

            if (DayManager.Instance.CurrentDayInt >= activateOnDay)
            {
                ActivateSpawner();
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer && DayManager.Instance.CurrentDayInt >= activateOnDay)
        {
            ActivateSpawner();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDayStarted -= OnNewDayStarted;
        }
    }

    private void OnNewDayStarted(int day)
    {
        if (!isSpawnerActive && day >= activateOnDay)
        {
            ActivateSpawner();
        }

        if (!isSpawnerActive) return;

        daysSinceActivation++;

        float spawnFactor = spawnRateCurve.Evaluate(daysSinceActivation);
        currentSpawnInterval = Mathf.Lerp(spawnIntervalEnd, spawnIntervalStart, spawnFactor);

        currentMaxEnemies = Mathf.FloorToInt(maxEnemiesCurve.Evaluate(daysSinceActivation));

        Debug.Log($"[Spawner Day {day}] ActiveDays={daysSinceActivation} | spawnInterval={currentSpawnInterval:F2}s | maxEnemies={currentMaxEnemies}");
    }

    private void ActivateSpawner()
    {
        if (isSpawnerActive || spawnPoints.Count == 0 || enemyPrefab == null) return;

        AdjustDifficultyForPlayerCount();

        isSpawnerActive = true;
        daysSinceActivation = 0;

        currentSpawnInterval = spawnIntervalStart;
        currentMaxEnemies = Mathf.FloorToInt(maxEnemiesCurve.Evaluate(0));

        // ✅ Delay first spawn by one frame to avoid spawn-before-ready bug
        StartCoroutine(SpawnFirstEnemyNextFrame());

        // ✅ Then start spawning with interval
        spawnCoroutine = StartCoroutine(SpawnEnemies());

        Debug.Log($"[Spawner] Activated on Day {DayManager.Instance.CurrentDayInt}");
    }

    private IEnumerator SpawnFirstEnemyNextFrame()
    {
        yield return new WaitForSeconds(1f);
        if (activeEnemies.Count < currentMaxEnemies)
            SpawnEnemy();
    }

    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentSpawnInterval);

            if (activeEnemies.Count < currentMaxEnemies)
            {
                SpawnEnemy();
            }
        }
    }

    private void SpawnEnemy()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        enemy.GetComponent<NetworkObject>().Spawn(true);
        activeEnemies.Add(enemy);

        var deathHandler = enemy.GetComponent<EnemyDeathHandler>();
        if (deathHandler != null)
        {
            deathHandler.OnEnemyDeath += () => activeEnemies.Remove(enemy);
        }
        else
        {
            Debug.LogWarning("Spawned enemy missing EnemyDeathHandler component!");
        }
    }

    private void AdjustDifficultyForPlayerCount()
    {
        int playerCount = NetworkManager.Singleton.ConnectedClientsList.Count;

        if (playerCount >= 4)
        {
            Debug.Log($"[Spawner] {playerCount} players: default difficulty.");
        }
        else if (playerCount >= 2)
        {
            spawnRateCurve = ScaleCurve(spawnRateCurve, 0.9f);
            maxEnemiesCurve = ScaleCurve(maxEnemiesCurve, 1f);
            Debug.Log($"[Spawner] Adjusted for {playerCount} players");
        }
        else
        {
            spawnRateCurve = ScaleCurve(spawnRateCurve, 0.85f);
            maxEnemiesCurve = ScaleCurve(maxEnemiesCurve, 1.2f);
            Debug.Log($"[Spawner] Adjusted for solo player");
        }
    }

    private AnimationCurve ScaleCurve(AnimationCurve curve, float multiplier)
    {
        Keyframe[] keys = curve.keys;
        for (int i = 0; i < keys.Length; i++)
        {
            keys[i].value *= multiplier;
        }
        return new AnimationCurve(keys);
    }
} 
