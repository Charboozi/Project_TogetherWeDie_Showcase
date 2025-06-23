using UnityEngine;
using Unity.Netcode;

namespace ProceduralSpider
{
public class SpiderRaycastDebugger : NetworkBehaviour
{
    private Spider spider;

    private void Awake()
    {
        spider = GetComponent<Spider>();
    }

    private void FixedUpdate()
    {
        if (spider == null) return;

        Debug.DrawRay(transform.position, -transform.up * spider.GetColliderHeight() * 3, Color.green);
        Debug.DrawRay(transform.position, transform.forward * spider.GetColliderLength() * 3, Color.red);

        RaycastHit hit;
        LayerMask layer = spider.walkableLayer;

        if (Physics.SphereCast(transform.position, spider.GetDownRayRadius(), -transform.up, out hit, spider.GetColliderHeight() * 3, layer, QueryTriggerInteraction.Ignore))
        {
            Debug.Log($"[DownRay Hit] {hit.collider.gameObject.name} (Layer {LayerMask.LayerToName(hit.collider.gameObject.layer)})");
        }
        else
        {
            Debug.Log("[DownRay Miss]");
        }

        if (Physics.SphereCast(transform.position, spider.GetForwardRayRadius(), transform.forward, out hit, spider.GetColliderLength() * 3, layer, QueryTriggerInteraction.Ignore))
        {
            Debug.Log($"[ForwardRay Hit] {hit.collider.gameObject.name} (Layer {LayerMask.LayerToName(hit.collider.gameObject.layer)})");
        }
        else
        {
            Debug.Log("[ForwardRay Miss]");
        }
    }
}
}