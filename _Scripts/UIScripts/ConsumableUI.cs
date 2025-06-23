using TMPro;
using UnityEngine;

/// <summary>
/// Update Consumable UI
/// </summary>
public class ConsumableUI : MonoBehaviour
{
    [Tooltip("Type must match consumable pickup object!")]
    [SerializeField] private string consumableType;   
    
    private TextMeshProUGUI text;

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();

        if (ConsumableManager.Instance != null)
        {
            ConsumableManager.Instance.OnConsumableChanged.AddListener(OnChanged);
            UpdateUI(ConsumableManager.Instance.Get(consumableType));
        }
        else
        {
            Debug.LogWarning("ConsumableManager instance is missing. ConsumableUI will not update.");
        }
    }

    private void OnChanged(string type, int count)
    {
        if (type == consumableType)
            UpdateUI(count);
    }

    private void UpdateUI(int count)
    {
        text.text = $"{count}";
    }
}
