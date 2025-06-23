public interface IEnemyAnimationHandler
{
    void TriggerAttack();
    void PlayDeath();
    void SetCrawling(bool crawl);
    void SetMoveSpeed(float speed);
    bool HasAttackAnimation { get; } 
}