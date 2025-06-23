using UnityEngine;

public class EnemyAnimationEventRelay : MonoBehaviour
{
    private EnemyAttack attack;

    void Awake()
    {
        attack = GetComponentInParent<EnemyAttack>();
    }

    public void TryDoDamage()
    {
        attack.TryDoDamage();
    }

    public void OnAttackAnimationComplete()
    {
        attack?.OnAttackAnimationComplete();
    }
}