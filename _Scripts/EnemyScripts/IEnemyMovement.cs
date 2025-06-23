using UnityEngine;

public interface IEnemyMovement
{
    void TickMovement();
    void SetTarget(Transform target);
    void ClearTarget();
    void UpdateDestination();
}
