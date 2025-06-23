using TMPro;
using UnityEngine;

public class KeycardShop : MonoBehaviour
{
    [Header("Pricing Settings")]
    [SerializeField] private int basePrice = 10;
    [SerializeField] private float priceMultiplier = 1.5f;

    [Header("UI")]
    [SerializeField] private TMP_Text keycardCountText;
    [SerializeField] private TMP_Text priceText;

    public void BuyKeycard()
    {
        int currentKeycards = PlayerInventoryManager.Instance.Keycards;
        int dynamicPrice = CalculatePrice(currentKeycards);

        if (CurrencyManager.Instance.Spend(dynamicPrice))
        {
            PlayerInventoryManager.Instance.AddKeycards(1);
            Debug.Log($"✅ Bought a keycard for {dynamicPrice} coins!");
            UpdateUI(); // Refresh price and count
        }
        else
        {
            Debug.Log("❌ Not enough coins to buy keycard.");
        }
    }

    public void UpdateUI()
    {
        int keycards = PlayerInventoryManager.Instance.Keycards;
        int dynamicPrice = CalculatePrice(keycards);

        keycardCountText.text = $"{keycards}";
        priceText.text = $"${dynamicPrice}";
    }

    private int CalculatePrice(int keycardsOwned)
    {
        return Mathf.RoundToInt(basePrice * Mathf.Pow(priceMultiplier, keycardsOwned));
    }
}
