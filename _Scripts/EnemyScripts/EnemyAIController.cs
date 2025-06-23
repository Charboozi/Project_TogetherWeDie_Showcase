using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(TargetScanner), typeof(EnemyAttack))]
public class EnemyAIController : NetworkBehaviour
{
    private IEnemyMovement movement;
    private TargetScanner targeting;
    private EnemyAttack attack;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        movement = GetComponent<IEnemyMovement>();
        targeting = GetComponent<TargetScanner>();
        attack = GetComponent<EnemyAttack>();

        // Register this enemy's movement with the centralized manager.
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.RegisterEnemy(movement);

        targeting.OnTargetAcquired += (target) => {
            movement.SetTarget(target);
            attack.SetTarget(target);
        };

        targeting.OnTargetInRange += () => {
            attack.TargetInRange();
        };

        targeting.OnTargetLost += () => {
            movement.ClearTarget();
            attack.SetTarget(null);
        };

        targeting.OnTargetOutOfRange += () => {
            attack.StopAttack();
        };
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        
        // Unregister from the manager when the enemy is despawned.
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.UnregisterEnemy(movement);
    }
}
