using UnityEngine;

public class CrawlZone : MonoBehaviour
{
    [Tooltip("Which layers are considered enemies.")]
    public LayerMask enemyLayer;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsInLayerMask(other.gameObject, enemyLayer)) return;

        if (other.TryGetComponent(out CrawlBehaviour crawlBehaviour))
        {
            crawlBehaviour.SetCrawlingState(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsInLayerMask(other.gameObject, enemyLayer)) return;

        if (other.TryGetComponent(out CrawlBehaviour crawlBehaviour))
        {
            crawlBehaviour.SetCrawlingState(false);
        }
    }

    private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return ((layerMask.value & (1 << obj.layer)) > 0);
    }
}
