using UnityEngine;
using System;
using Unity.Netcode;
using UnityEngine.AI;

public class TargetScanner : NetworkBehaviour
{
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private LayerMask targetLayer;

    public float inRangeThreshold = 2f;

    private Transform currentTarget;
    private float checkInterval = 1f;
    private float checkTimer = 0f;

    public event Action<Transform> OnTargetAcquired;
    public event Action OnTargetLost;
    public event Action OnTargetInRange;
    public event Action OnTargetOutOfRange;

    
    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!IsServer || agent == null) return;

        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0f)
        {
            checkTimer = checkInterval;
            ScanForTargets();
        }

        if (currentTarget != null)
        {
            float dist = Vector3.Distance(transform.position, currentTarget.position);
            if (dist > detectionRange)
            {
                OnTargetLost?.Invoke();
                currentTarget = null;
            }
            else if (dist <= inRangeThreshold)
            {
                OnTargetInRange?.Invoke();
            }
            else
            {
                OnTargetOutOfRange?.Invoke();
            }
        }
    }

    private void ScanForTargets()
    {   
        if (!agent.isOnNavMesh) return;

        Collider[] targets = Physics.OverlapSphere(transform.position, detectionRange, targetLayer);
        Transform closestTarget = null;
        float closestDist = Mathf.Infinity;

        foreach (var candidate in targets)
        {        
            // Check if the candidate has EntityHealth and is not downed
            if (candidate.TryGetComponent<EntityHealth>(out var entityHealth))
            {
                if (entityHealth.isDowned.Value)
                    continue; // Skip downed targets
            }
            else
            {
                continue; // Skip if no EntityHealth (optional safety)
            }
            
            float dist = Vector3.Distance(transform.position, candidate.transform.position);

            NavMeshHit hit;
            bool foundNavMesh = NavMesh.SamplePosition(candidate.transform.position, out hit, 1.5f, NavMesh.AllAreas);

            if (!foundNavMesh)
                continue; // Target not on navmesh

            NavMeshPath path = new NavMeshPath();
            if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete)
                continue;

            if (dist < closestDist)
            {
                closestDist = dist;
                closestTarget = candidate.transform;
            }
        }

        if (closestTarget != null && closestTarget != currentTarget)
        {
            currentTarget = closestTarget;
            OnTargetAcquired?.Invoke(closestTarget);
        }
    }
}