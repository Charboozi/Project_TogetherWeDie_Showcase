using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [Header("Performance Settings")]

    [Tooltip("How many enemies will be processed per frame for TickMovement (general movement updates). Higher means more responsive movement, lower means better performance.")]
    public int updatesPerFrame = 10;

    [Tooltip("How many enemies will update their destination per frame. Higher makes pathing more responsive, lower reduces performance cost.")]
    public int destinationUpdatesPerFrame = 10;

    [Tooltip("Interval in seconds between destination updates. Lower values make enemies change paths more frequently, but increases CPU usage.")]
    public float destinationUpdateInterval = 0.2f;

    private List<IEnemyMovement> enemyMovements = new List<IEnemyMovement>();
    private int currentIndex = 0;
    private int destinationUpdateIndex = 0;
    private float destinationUpdateTimer = 0f;

    private List<IEnemyMovement> pendingRemovals = new List<IEnemyMovement>();

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterEnemy(IEnemyMovement enemy)
    {
        if (!enemyMovements.Contains(enemy))
            enemyMovements.Add(enemy);
    }

    public void UnregisterEnemy(IEnemyMovement enemy)
    {
        pendingRemovals.Add(enemy);
    }

    private void LateUpdate()
    {
        if (pendingRemovals.Count > 0)
        {
            foreach (var enemy in pendingRemovals)
            {
                enemyMovements.Remove(enemy);
            }
            pendingRemovals.Clear();

            // Reset indexes to be safe
            currentIndex = 0;
            destinationUpdateIndex = 0;
        }
    }

    private void Update()
    {
        // Regular movement updates
        int updatesThisFrame = Mathf.Min(updatesPerFrame, enemyMovements.Count);

        for (int i = 0; i < updatesThisFrame; i++)
        {
            if (enemyMovements.Count == 0) break;

            if (enemyMovements[currentIndex] != null)
                enemyMovements[currentIndex].TickMovement();

            currentIndex = (currentIndex + 1) % enemyMovements.Count;
        }

        // Destination updates (pathfinding)
        destinationUpdateTimer -= Time.deltaTime;
        if (destinationUpdateTimer <= 0f)
        {
            int destinationUpdatesThisFrame = Mathf.Min(destinationUpdatesPerFrame, enemyMovements.Count);

            for (int i = 0; i < destinationUpdatesThisFrame; i++)
            {
                if (enemyMovements.Count == 0) break;

                if (enemyMovements[destinationUpdateIndex] != null)
                    enemyMovements[destinationUpdateIndex].UpdateDestination();

                destinationUpdateIndex = (destinationUpdateIndex + 1) % enemyMovements.Count;
            }

            destinationUpdateTimer = destinationUpdateInterval;
        }
    }
}
