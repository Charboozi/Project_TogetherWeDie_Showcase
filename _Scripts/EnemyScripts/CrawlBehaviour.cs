using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(EnemyAnimationHandler))]
public class CrawlBehaviour : NetworkBehaviour
{
    private bool isCrawling;
    private EnemyAnimationHandler enemyAnimation;

    private void Awake()
    {
        enemyAnimation = GetComponent<EnemyAnimationHandler>();
    }

    public void SetCrawlingState(bool crawl)
    {
        if (crawl == isCrawling) return; 
        isCrawling = crawl;
        UpdateCrawlClientRpc(isCrawling);
    }

    [ClientRpc]
    private void UpdateCrawlClientRpc(bool crawl)
    {
        isCrawling = crawl;
        enemyAnimation.SetCrawling(crawl);
    }
}
