using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponShop : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private string baseWeaponName;
    [SerializeField] private int[] tierPrices = { 100, 200, 300 };

    [Header("UI")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Toggle activeToggle;

    private void OnEnable()
    {
        UpdateUI();
    }

    public void BuyWeapon()
    {
        int currentTier = PlayerInventoryManager.Instance.GetUnlockedWeaponTier(baseWeaponName);
        if (currentTier >= tierPrices.Length)
        {
            Debug.Log("🟡 Already at max tier.");
            return;
        }

        int price = tierPrices[currentTier];
        string weaponToUnlock = currentTier == 0 ? baseWeaponName : $"{baseWeaponName} Tier {currentTier + 1}";

        if (CurrencyManager.Instance.Spend(price))
        {
            PlayerInventoryManager.Instance.UnlockWeapon(weaponToUnlock);
            Debug.Log($"✅ Bought {weaponToUnlock} for {price} coins!");
            UpdateUI();
        }
        else
        {
            Debug.Log("❌ Not enough coins.");
        }
    }

    public void ToggleActiveWeapon(bool isOn)
    {
        string weaponName = GetBestTierName();

        bool isCurrentlyActive = PlayerInventoryManager.Instance.ActiveWeapons.Contains(weaponName);

        if (isOn && !isCurrentlyActive)
        {
            if (!PlayerInventoryManager.Instance.TryToggleActiveWeapon(weaponName))
                activeToggle.isOn = false; // force toggle off
        }
        else if (!isOn && isCurrentlyActive)
        {
            PlayerInventoryManager.Instance.TryToggleActiveWeapon(weaponName);
        }
    }

    private void UpdateUI()
    {
        int currentTier = PlayerInventoryManager.Instance.GetUnlockedWeaponTier(baseWeaponName);
        string bestTierName = GetBestTierName();

        // Update status and price
        switch (currentTier)
        {
            case 0:
                statusText.text = "Not Owned";
                priceText.text = $"${tierPrices[0]}";
                break;
            case 1:
                statusText.text = "Tier 1";
                priceText.text = tierPrices.Length > 1 ? $"${tierPrices[1]}" : "Maxed";
                break;
            case 2:
                statusText.text = "Tier 2";
                priceText.text = tierPrices.Length > 2 ? $"${tierPrices[2]}" : "Maxed";
                break;
            case 3:
                statusText.text = "Tier 3";
                priceText.text = "Maxed";
                break;
        }

        // Safely update toggle
        if (activeToggle != null)
        {
            bestTierName = GetBestTierName();

            activeToggle.onValueChanged.RemoveListener(ToggleActiveWeapon); // prevent accidental call

            activeToggle.isOn = PlayerInventoryManager.Instance.ActiveWeapons.Contains(bestTierName);
            
            // ✅ FIX: Check if any tier is unlocked
            bool isUnlocked = PlayerInventoryManager.Instance.GetUnlockedWeaponTier(baseWeaponName) > 0;
            activeToggle.interactable = isUnlocked;

            activeToggle.onValueChanged.AddListener(ToggleActiveWeapon); // re-attach
        }

    }


    private string GetBestTierName()
    {
        int currentTier = PlayerInventoryManager.Instance.GetUnlockedWeaponTier(baseWeaponName);
        return currentTier == 1 ? baseWeaponName : $"{baseWeaponName} Tier {currentTier}";
    }
}
