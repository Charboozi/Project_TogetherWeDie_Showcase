using UnityEngine;
using TMPro;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance;

    [SerializeField] private GameObject tooltipRoot;
    [SerializeField] private TMP_Text tooltipText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        HideTooltip();
    }

    public void ShowTooltip(string message)
    {
        tooltipText.text = message;
        tooltipRoot.SetActive(true);
    }

    public void HideTooltip()
    {
        tooltipText.text = "";
        tooltipRoot.SetActive(false);
    }
}
