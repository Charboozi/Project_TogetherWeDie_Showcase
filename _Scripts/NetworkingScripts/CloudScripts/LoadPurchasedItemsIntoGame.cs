using UnityEngine;

public class LoadPurchasedItemsIntoGame : MonoBehaviour
{
    [SerializeField] private WeaponInventory weaponInventory;

    private void Start()
    {
        var inventoryManager = PlayerInventoryManager.Instance;
        var consumables = ConsumableManager.Instance;

        if (inventoryManager == null)
        {
            Debug.LogWarning("❌ PlayerInventoryManager missing.");
            return;
        }

        // ✅ Only run this script if we're in PvP mode
        if (GameModeManager.Instance == null || !GameModeManager.Instance.IsPvPMode)
        {
            Debug.Log("⛔ Skipping item load — not in PvP mode.");
            return;
        }


        // ✅ Add consumables (e.g., keycards)
        if (consumables != null)
        {
            int keycards = inventoryManager.Keycards;
            consumables.Add("Keycard", keycards);
            Debug.Log($"🟩 Loaded {keycards} keycards into ConsumableManager.");
        }

        // ✅ Add only active weapons to WeaponInventory
        if (weaponInventory != null)
        {
            var activeWeapons = inventoryManager.ActiveWeapons;

            if (activeWeapons.Count == 0)
            {
                Debug.LogWarning("⚠️ No active weapons. Giving default 'Pistol'.");
            }
            else
            {
                foreach (var weaponName in activeWeapons)
                {
                    Debug.Log($"🎯 Loading active weapon: {weaponName}");
                    weaponInventory.PickUpWeapon(weaponName);
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠️ No WeaponInventory found on this GameObject.");
        }
    }
}
