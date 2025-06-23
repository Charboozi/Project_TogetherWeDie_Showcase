using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance { get; private set; }

    private readonly List<Transform> playerTransforms = new List<Transform>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void RegisterPlayer(Transform playerTransform)
    {
        if (!playerTransforms.Contains(playerTransform))
        {
            playerTransforms.Add(playerTransform);
        }
    }

    public void UnregisterPlayer(Transform playerTransform)
    {
        if (playerTransforms.Contains(playerTransform))
        {
            playerTransforms.Remove(playerTransform);
        }
    }

    public float GetClosestPlayerDistance(Vector3 position)
    {
        float minDistance = float.MaxValue;

        foreach (var playerTransform in playerTransforms)
        {
            float distance = Vector3.Distance(position, playerTransform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }

        return minDistance;
    }
}
