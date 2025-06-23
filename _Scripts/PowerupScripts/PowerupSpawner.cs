using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PowerupSpawner : NetworkBehaviour
{
    [Header("Powerup Settings")]
    [Range(0f, 1f)] public float startChance = 0.35f;
    [Range(0f, 1f)] public float endChance = 0.05f;
    [Min(1)] public int fullDecreaseDay = 12;
    public float powerupLifetime = 10f;

    private Vector3 localSpawnOffset = new Vector3(0, -1f, 0);
    private GameObject spawnedPowerup;

    private float GetDynamicSpawnChance()
    {
        int currentDay = DayManager.Instance != null ? DayManager.Instance.CurrentDayInt : 0;
        float t = Mathf.Clamp01(currentDay / (float)fullDecreaseDay);
        return Mathf.Lerp(startChance, endChance, t);
    }

    public void TrySpawn()
    {
        if (!IsServer) return;
        if (Random.value > GetDynamicSpawnChance()) return;

        GameObject[] powerups = Resources.LoadAll<GameObject>("Powerups");
        if (powerups.Length == 0) return;

        GameObject chosen = powerups[Random.Range(0, powerups.Length)];

        spawnedPowerup = Instantiate(chosen, transform.position + localSpawnOffset, Quaternion.Euler(-90f, 0f, 0f));

        NetworkObject netObj = spawnedPowerup.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn(true);
            StartCoroutine(DestroyPowerupAfterDelay(spawnedPowerup, netObj, powerupLifetime));
        }
        else
        {
            Debug.LogError("Powerup prefab missing NetworkObject.");
        }
    }

    private IEnumerator DestroyPowerupAfterDelay(GameObject obj, NetworkObject netObj, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (IsServer && netObj != null && netObj.IsSpawned)
        {
            netObj.Despawn(true); // Despawn from the network
            Destroy(obj);         // Destroy the GameObject
        }
    }
}
