public class NullEnemyAnimationHandler : IEnemyAnimationHandler
{
    public static readonly NullEnemyAnimationHandler Instance = new NullEnemyAnimationHandler();

    public bool HasAttackAnimation => false; // ✅ No animation!

    public void TriggerAttack() { }
    public void PlayDeath() { }
    public void SetCrawling(bool crawl) { }
    public void SetMoveSpeed(float speed) { }
}
