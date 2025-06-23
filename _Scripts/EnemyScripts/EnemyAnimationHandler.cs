using UnityEngine;

public class EnemyAnimationHandler : MonoBehaviour, IEnemyAnimationHandler
{
    [SerializeField] private Animator animator;
    
    public bool HasAttackAnimation => true;
    public int numberOfRandomAnimations = 2;

    private EntityHealth entityHealth;

    private void Awake()
    {
        SetRandomWalkIndex();

        entityHealth = GetComponent<EntityHealth>();
        if (entityHealth != null)
        {
            entityHealth.OnTakeDamage += HandleTakeDamage;
        }
    }

    private void OnDestroy()
    {
        if (entityHealth != null)
        {
            entityHealth.OnTakeDamage -= HandleTakeDamage;
        }
    }

    public void TriggerAttack()
    {
        animator.ResetTrigger("Attack");
        animator.SetTrigger("Attack");
    }

    public void PlayDeath()
    {
        animator.SetBool("isDead", true);
    }

    public void SetCrawling(bool crawl)
    {
        animator.SetBool("isCrawling", crawl);
    }

    public void SetMoveSpeed(float speed)
    {
        if (animator.enabled)
        {
            animator.SetFloat("moveSpeed", speed);
        }
    }

    private void SetRandomWalkIndex()
    {
        int index = Random.Range(0, numberOfRandomAnimations);
        animator.SetFloat("randomIndex", index);
    }

    private void HandleTakeDamage()
    {
        animator.SetTrigger("TriggerHit");
    }
}
