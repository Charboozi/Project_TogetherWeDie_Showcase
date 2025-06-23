using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameOverManager : NetworkBehaviour
{
    public static GameOverManager Instance;

    private readonly List<EntityHealth> playerEntities = new();

    private ulong lastSurvivorClientId;
    private ulong currentlyLastAliveClientId;

    private readonly List<ulong> eliminationOrder = new(); // From first dead to last

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterPlayer(EntityHealth player)
    {
        if (!IsServer || player == null || !player.CompareTag("Player")) return;

        if (!playerEntities.Contains(player))
        {
            playerEntities.Add(player);
            player.OnTakeDamage += () => StartCoroutine(DelayedGameOverCheck());
            player.OnDowned += OnPlayerDowned;
        }
    }

    public void UnregisterPlayer(EntityHealth player)
    {
        if (!IsServer || player == null || !player.CompareTag("Player")) return;

        if (playerEntities.Contains(player))
        {
            playerEntities.Remove(player);
            player.OnTakeDamage -= () => CheckGameOverCondition(player);
            player.OnDowned -= OnPlayerDowned;
        }
    }

    private void OnPlayerDowned(EntityHealth player)
    {
        if (!IsServer || player == null) return;

        ulong clientId = player.OwnerClientId;
        if (!eliminationOrder.Contains(clientId))
        {
            eliminationOrder.Add(clientId);
            Debug.Log($"☠️ Player {clientId} downed. Elimination order: {eliminationOrder.Count}");
        }
    }

    public void ClearEliminationOrder()
    {
        eliminationOrder.Clear();
    }

    private void CheckGameOverCondition(EntityHealth _)
    {
        if (!IsServer) return;

        ulong newLastAlive = 0;
        int aliveCount = 0;

        foreach (var player in playerEntities)
        {
            if (player == null) continue;

            Debug.Log($"🔍 Player {player.OwnerClientId} isDowned: {player.isDowned.Value}");

            if (!player.isDowned.Value)
            {
                aliveCount++;
                newLastAlive = player.OwnerClientId;
            }
        }

        // Always track the last one standing
        if (aliveCount == 1)
        {
            currentlyLastAliveClientId = newLastAlive;
        }

        if (GameModeManager.Instance.IsPvPMode)
        {
            // In PvP: Game over when only one left alive
            if (aliveCount == 1)
            {
                lastSurvivorClientId = currentlyLastAliveClientId;
                Debug.Log($"🏆 PvP: Only one player left! Client {lastSurvivorClientId}");
                TriggerGameOver();
            }
        }
        else
        {
            // In Co-Op: Game over when all are downed
            if (aliveCount == 0)
            {
                // Wait before triggering game over to allow self-revive to happen
                StartCoroutine(GracePeriodBeforeGameOver());
            }
        }
    }

    private IEnumerator GracePeriodBeforeGameOver()
    {
        Debug.Log("🕓 All players are down. Waiting to see if someone revives...");

        yield return new WaitForSeconds(3.1f); // Wait slightly longer than self-revive time

        // Re-check if anyone is still downed
        int stillDownCount = 0;
        ulong lastAlive = 0;

        foreach (var player in playerEntities)
        {
            if (player == null) continue;

            if (!player.isDowned.Value)
            {
                Debug.Log($"🟢 Player {player.OwnerClientId} revived during grace period.");
                yield break; // Someone got back up, cancel game over
            }
            else
            {
                stillDownCount++;
                lastAlive = player.OwnerClientId;
            }
        }

        if (stillDownCount == playerEntities.Count)
        {
            lastSurvivorClientId = lastAlive;
            Debug.Log($"🟥 No one revived. Proceeding with game over.");
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        if (!IsServer) return;

        if (NetworkManager.Singleton.SceneManager != null)
        {
            Debug.Log("✅ Triggering game over visuals...");
            RewardAllPlayersBasedOnDay();
            ShowGameOverEffectClientRpc(); // All players get death fade
            SetCanInteractClientRpc(true);

            // 👇 Winner-only victory effect
            if (GameModeManager.Instance.IsPvPMode)
            {
                ShowWinnerEffectClientRpc(new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new[] { lastSurvivorClientId }
                    }
                });
            }

            StartCoroutine(DelayedGameOverSceneLoad(4f)); // Fade duration

            if (MapManager.Instance != null && HighScoreManager.Instance != null)
            {
                string map = MapManager.Instance.CurrentMapName;
                int currentDay = DayManager.Instance.CurrentDayInt;

                HighScoreManager.Instance.SetHighScore(map, currentDay);
                SyncHighScoreClientRpc(map, currentDay);

                // ✅ Now push the updated score to all players
                for (int i = 0; i < LobbyPlayerList.Instance.Players.Count; i++)
                {
                    var updated = LobbyPlayerList.Instance.Players[i];
                    updated.HighScoreForCurrentMap = currentDay;
                    LobbyPlayerList.Instance.Players[i] = updated;
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠️ NetworkSceneManager not found.");
        }
    }


    private IEnumerator DelayedGameOverSceneLoad(float delay)
    {
        yield return new WaitForSeconds(delay);

        var playersToDespawn = new List<EntityHealth>(playerEntities);
        foreach (var player in playersToDespawn)
        {
            if (player != null && player.TryGetComponent(out NetworkObject netObj) && netObj.IsSpawned)
            {
                netObj.Despawn(true);
            }
        }

        NetworkManager.Singleton.SceneManager.LoadScene("GameOver", LoadSceneMode.Single);
        StartCoroutine(ShutdownAfterDelay(10f));
    }

    private IEnumerator ShutdownAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            NetworkManager.Singleton.DisconnectClient(client.Key);
        }

        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator DelayedGameOverCheck()
    {
        yield return new WaitForSeconds(0.1f); // Allow state updates
        CheckGameOverCondition(null);
    }

    private void RewardAllPlayersBasedOnDay()
    {
        if (GameModeManager.Instance.IsPvPMode)
        {
            int pot = GameModeManager.Instance.PvPRewardPot;

            // Fill remaining players if they weren't recorded
            foreach (var player in playerEntities)
            {
                if (!eliminationOrder.Contains(player.OwnerClientId))
                {
                    eliminationOrder.Add(player.OwnerClientId);
                }
            }

            var uniqueOrder = eliminationOrder
            .Distinct()
            .Reverse()
            .ToList();

            for (int i = 0; i < uniqueOrder.Count; i++)
            {
                ulong clientId = uniqueOrder[i];
                float percentage = 0f;

                if (i == 0) percentage = 0.80f;
                else if (i == 1) percentage = 0.15f;
                else if (i == 2) percentage = 0.05f;

                int reward = Mathf.RoundToInt(pot * percentage);

                // Update networked coin display
                for (int j = 0; j < LobbyPlayerList.Instance.Players.Count; j++)
                {
                    if (LobbyPlayerList.Instance.Players[j].ClientId == clientId)
                    {
                        var updated = LobbyPlayerList.Instance.Players[j];
                        updated.CoinsEarned = reward;
                        LobbyPlayerList.Instance.Players[j] = updated;
                        break;
                    }
                }

                // Send reward only to correct client
                if (reward > 0)
                {
                    RewardPvPWinnerClientRpc(reward, clientId, new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new[] { clientId }
                        }
                    });
                }

                Debug.Log($"🏅 Player {clientId} gets {reward} coins ({percentage * 100}%)");
            }
        }
        else
        {
            // Normal co-op reward
            int day = DayManager.Instance.CurrentDayInt;

            int reward = 0;
            if (day > 1)
            {
                float baseReward = 100f;
                float growthMultiplier = 1.1f;
                reward = Mathf.RoundToInt(baseReward * Mathf.Pow(growthMultiplier, day));
            }
            else
            {
                Debug.Log("🟥 No reward given for dying on Day 1.");
            }

            for (int i = 0; i < LobbyPlayerList.Instance.Players.Count; i++)
            {
                var updated = LobbyPlayerList.Instance.Players[i];
                updated.CoinsEarned = reward;
                LobbyPlayerList.Instance.Players[i] = updated;
            }

            RewardClientsClientRpc(reward);
        }
    }

    [ClientRpc]
    private void RewardClientsClientRpc(int reward)
    {
        CurrencyManager.Instance?.Add(reward);
        Debug.Log($"🟢 You received {reward} coins!");
    }

    [ClientRpc]
    private void SetCanInteractClientRpc(bool value)
    {
        PlayerInput.CanInteract = value;
    }

    [ClientRpc]
    private void RewardPvPWinnerClientRpc(int reward, ulong intendedRecipientClientId, ClientRpcParams rpcParams = default)
    {
        if (NetworkManager.Singleton.LocalClientId != intendedRecipientClientId)
        {
            // ❌ Not for this client
            return;
        }

        CurrencyManager.Instance?.Add(reward);
        Debug.Log($"💸 You (Client {intendedRecipientClientId}) received the PvP pot of {reward} coins!");
    }

    [ClientRpc]
    private void ShowGameOverEffectClientRpc()
    {
        if (FadeScreenEffect.Instance != null)
        {
            FadeScreenEffect.Instance.ShowDeathEffect(3.7f); // Fade to deep red in 4 seconds
        }
    }

    [ClientRpc]
    private void ShowWinnerEffectClientRpc(ClientRpcParams rpcParams = default)
    {
        if (FadeScreenEffect.Instance != null)
        {
            FadeScreenEffect.Instance.ShowVictoryEffect(3.7f); // You implement this in FadeScreenEffect
            Debug.Log("🎉 Victory effect triggered!");
        }
    }
    
    [ClientRpc]
    private void SyncHighScoreClientRpc(string mapName, int day)
    {
        HighScoreManager.Instance.SetHighScore(mapName, day);
    }
}
