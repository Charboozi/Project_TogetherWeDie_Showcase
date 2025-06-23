using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : NetworkBehaviour, IEnemyMovement
{
    [Header("Roaming")]
    public float roamRadius = 10f;
    public float roamDelay = 3f;

    private NavMeshAgent agent;
    private Transform currentTarget;
    private float roamTimer;

    private NetworkVariable<float> syncedSpeed = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private IEnemyAnimationHandler enemyAnimation;

    private bool isWaitingToRoam = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyAnimation = GetComponent<IEnemyAnimationHandler>() ?? NullEnemyAnimationHandler.Instance;
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            syncedSpeed.OnValueChanged += OnSpeedChanged;
        }
    }

    private void OnSpeedChanged(float oldSpeed, float newSpeed)
    {
        enemyAnimation.SetMoveSpeed(newSpeed);
    }

    private void Start()
    {
        if (!IsServer) return;

        PickNewRoamDestination();
        roamTimer = roamDelay;

        EnemyManager.Instance.RegisterEnemy(this);
    }

    public override void OnDestroy()
    {
        if (IsClient && syncedSpeed != null)
        {
            syncedSpeed.OnValueChanged -= OnSpeedChanged;
        }

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterEnemy(this);
        }
    }

    public void TickMovement()
    {
        if (!IsServer || agent == null || !agent.isActiveAndEnabled) return;

        if (currentTarget != null)
        {
            agent.SetDestination(currentTarget.position); // ✅ Explicit chase
            UpdateMoveAnimation();
            return;
        }

        HandleRoaming();
        UpdateMoveAnimation();
    }

    private void HandleRoaming()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !isWaitingToRoam)
        {
            isWaitingToRoam = true;
            roamTimer = roamDelay;
        }

        if (isWaitingToRoam)
        {
            roamTimer -= Time.deltaTime;
            if (roamTimer <= 0f)
            {
                PickNewRoamDestination();
                isWaitingToRoam = false;
            }
        }
    }


    public void UpdateDestination()
    {
        if (!IsServer || agent == null || !agent.isActiveAndEnabled) return;

        if (currentTarget != null)
        {
            agent.SetDestination(currentTarget.position);
        }
        else if (agent.remainingDistance < 1f)
        {
            PickNewRoamDestination();
            roamTimer = roamDelay;
        }

        UpdateMoveAnimation();
    }

    public void SetTarget(Transform target)
    {
        currentTarget = target;
    }

    public void ClearTarget()
    {
        currentTarget = null;
    }

    private void PickNewRoamDestination()
    {
        Vector3 randomPoint = Random.insideUnitSphere * roamRadius + transform.position;

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, roamRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
    
    private void UpdateMoveAnimation()
    {
        float speed = agent.velocity.magnitude;
        syncedSpeed.Value = speed; 
    }

    public float GetSyncedSpeed()
    {
        return syncedSpeed.Value;
    }
}
