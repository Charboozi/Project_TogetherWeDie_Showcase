using UnityEngine;

public class PickupTooltipDetector : MonoBehaviour
{
    [SerializeField] private float detectionRange = 3f;
    [SerializeField] private LayerMask detectionLayer;

    private void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, detectionRange, detectionLayer))
        {
            var tooltip = hit.collider.GetComponent<PickupTooltip>();
            if (tooltip != null && !string.IsNullOrEmpty(tooltip.tooltipText))
            {
                TooltipUI.Instance.ShowTooltip(tooltip.tooltipText);
                return;
            }
        }

        TooltipUI.Instance.HideTooltip();
    }
}
