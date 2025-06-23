using UnityEngine;
using Unity.Netcode;
using System.Collections;

[RequireComponent(typeof(Interactable))]
public class ChallengeInteractable : NetworkBehaviour, IInteractableAction
{
    [Header("Challenge Settings")]
    [SerializeField] private float challengeTimeLimit = 200f;

    [Header("Reward Settings")]
    [SerializeField] private TeleporterInteractable rewardTeleporter;
    [SerializeField] private float rewardDuration = 20f;

    private bool challengeStarted;
    private bool challengeCompleted;

    public void DoAction()
    {
        if (!IsServer || challengeStarted || challengeCompleted) return;
        if (rewardTeleporter == null) return;
        StartCoroutine(ChallengeRoutine());
    }

    private IEnumerator ChallengeRoutine()
    {
        challengeStarted = true;
        bool enemyKilled = false;
        void OnKill(EntityHealth e) => enemyKilled = true;
        EntityHealth.OnSpecialEnemyKilled += OnKill;

        float t = 0f;
        while (t < challengeTimeLimit && !enemyKilled)
        {
            t += Time.deltaTime;
            yield return null;
        }

        EntityHealth.OnSpecialEnemyKilled -= OnKill;

        if (enemyKilled)
            rewardTeleporter.OverrideDestinationTemporarily(rewardDuration);
        else
            Debug.LogWarning("[Challenge] Failed—time's up.");

        challengeCompleted = enemyKilled;
        challengeStarted = false;
    }
}