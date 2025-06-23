using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class SpeedPowerup : PowerupBase
{
    [Tooltip("Amount of speed to add.")]
    [SerializeField] private float speedBoost = 3f;

    [Tooltip("Duration of the speed boost in seconds.")]
    [SerializeField] private float duration = 20f;

    // Tracks active speed boosts per player
    private static Dictionary<ulong, Coroutine> activeBoosts = new();

    protected override int GetEffectValue()
    {
        return Mathf.RoundToInt(speedBoost);
    }

    [ClientRpc]
    protected override void ApplyPowerupClientRpc(int _, ClientRpcParams clientRpcParams = default)
    {
        var player = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (player == null)
        {
            Debug.LogWarning("SpeedPowerup: No local player found.");
            return;
        }

        var movement = player.GetComponent<NetworkedCharacterMovement>();
        if (movement == null)
        {
            Debug.LogWarning("SpeedPowerup: No movement component found.");
            return;
        }

        ulong playerId = player.OwnerClientId;

        // Stop any existing speed boost coroutine
        if (activeBoosts.TryGetValue(playerId, out var existingBoost))
        {
            Debug.Log("⚠️ Canceling existing speed boost");
            player.GetComponent<MonoBehaviour>().StopCoroutine(existingBoost);
            movement.RemoveBonusSpeed(speedBoost);
        }

        // Start new coroutine and track it
        Coroutine newBoost = player.GetComponent<MonoBehaviour>().StartCoroutine(ApplySpeedBoost(playerId, movement));
        activeBoosts[playerId] = newBoost;

        // ✅ Show UI boost icon
        PowerupUIController.Instance?.ShowSpeedBoost(duration);
    }


    private IEnumerator ApplySpeedBoost(ulong playerId, NetworkedCharacterMovement movement)
    {
        Debug.Log($"⚡ Speed boost applied: +{speedBoost}");

        PersistentScreenTint.Instance?.SetPersistentTintForDuration(
            new Color(1f, 0.85f, 0f), // Yellow/orange
            duration
        );

        GameObject loopAudio = PlayLoopedEffectSound(duration);

        movement.AddBonusSpeed(speedBoost);

        yield return new WaitForSeconds(duration);

        movement.RemoveBonusSpeed(speedBoost);
        Debug.Log($"✅ Speed boost ended: -{speedBoost}");

        if (loopAudio != null)
        {
            Destroy(loopAudio);
        }

        // Clean up from tracking
        if (activeBoosts.ContainsKey(playerId))
        {
            activeBoosts.Remove(playerId);
        }
    }
}
