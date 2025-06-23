using TMPro;
using UnityEngine;
using Steamworks;

public class CurrencyUI : MonoBehaviour
{
    [SerializeField] private TMP_Text coinsText;

    private void Start()
    {
        InvokeRepeating(nameof(UpdateUI), 0f, 1f); // Refresh every second (or tweak interval)
    }

    public void UpdateUI()
    {
        if (CurrencyManager.Instance != null && SteamClient.IsValid)
        {
            coinsText.text = $"$ {CurrencyManager.Instance.Coins}";
        }
    }
}
