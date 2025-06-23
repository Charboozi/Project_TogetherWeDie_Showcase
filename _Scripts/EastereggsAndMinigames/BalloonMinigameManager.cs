using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class BalloonMinigameManager : NetworkBehaviour
{
    public static BalloonMinigameManager Instance;

    [Header("Settings")]
    public float gameDuration = 100f;
    public int numberOfBalloons = 3;
    public GameObject balloonPrefab;

    [Header("Spawn Points")]
    public List<BalloonSpawnPoint> spawnPoints;

    private List<NetworkObject> spawnedBalloons = new();
    private float timer;
    private bool isActive = false;

    private int balloonsPopped = 0;
    private int balloonsSpawned = 0;

    [SerializeField] private RewardSpawner rewardSpawner;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!IsServer || !isActive) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            EndMinigame(success: false);
        }
    }

    public void StartMinigame()
    {
        if (!IsServer || isActive) return;

        isActive = true;
        timer = gameDuration;
        balloonsPopped = 0;

        SpawnBalloons();
        Debug.Log("🎈 Balloon minigame started!");
    }

    private void SpawnBalloons()
    {
        spawnedBalloons.Clear();

        var shuffled = new List<BalloonSpawnPoint>(spawnPoints);
        Shuffle(shuffled);

        int spawnCount = Mathf.Min(numberOfBalloons, shuffled.Count);
        balloonsSpawned = spawnCount;

        for (int i = 0; i < spawnCount; i++)
        {
            var pos = shuffled[i].transform.position;
            var balloonInstance = Instantiate(balloonPrefab, pos, Quaternion.identity);
            var netObj = balloonInstance.GetComponent<NetworkObject>();

            if (netObj != null)
            {
                netObj.Spawn();
                spawnedBalloons.Add(netObj);
                Debug.Log("✅ Spawned balloon: " + netObj.name);
            }
            else
            {
                Debug.LogError("❌ Spawned balloon is missing NetworkObject!");
            }
        }
    }

    public void OnBalloonPopped()
    {
        if (!IsServer) return;

        balloonsPopped++;

        if (balloonsPopped >= balloonsSpawned && isActive)
        {
            EndMinigame(success: true);
        }
    }

    private void EndMinigame(bool success)
    {
        if (!isActive) return;

        isActive = false;

        foreach (var netObj in spawnedBalloons)
        {
            if (netObj != null && netObj.IsSpawned)
            {
                netObj.Despawn(true);
            }
        }

        spawnedBalloons.Clear();

        if (success)
        {
            Debug.Log("✅ All balloons popped in time! Reward granted.");
            StartCoroutine(DelayedRewardSpawn());
        }
        else
        {
            Debug.Log("❌ Minigame failed — time ran out.");
        }
    }

    private IEnumerator DelayedRewardSpawn()
    {
        yield return new WaitForSeconds(20f);
        rewardSpawner?.SpawnRandomReward();
    }

    private void Shuffle(List<BalloonSpawnPoint> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}
