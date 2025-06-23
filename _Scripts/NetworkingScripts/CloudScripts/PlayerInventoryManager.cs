using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class PlayerInventoryManager : MonoBehaviour
{
    public static PlayerInventoryManager Instance;

    private string SavePath => Application.persistentDataPath + "/inventory_save.json";

    public int Keycards { get; private set; }
    public HashSet<string> UnlockedWeapons { get; private set; } = new();
    public HashSet<string> ActiveWeapons { get; private set; } = new();

    public HashSet<string> UnlockedMaps { get; private set; } = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadInventory();
    }

    // ------------------ Keycards ------------------

    public void AddKeycards(int amount)
    {
        Keycards += amount;
        Debug.Log($"➕ Gained {amount} keycards. New total: {Keycards}");
        SaveInventory();
    }

    // ------------------ Weapon Unlocks ------------------

    public int GetUnlockedWeaponTier(string baseWeaponName)
    {
        int maxTier = 0;

        if (UnlockedWeapons.Contains(baseWeaponName))
            maxTier = 1;

        for (int tier = 2; tier <= 3; tier++)
        {
            string tierName = $"{baseWeaponName} Tier {tier}";
            if (UnlockedWeapons.Contains(tierName))
                maxTier = tier;
        }

        return maxTier;
    }

    public void UnlockWeapon(string weaponName)
    {
        if (UnlockedWeapons.Add(weaponName))
        {
            Debug.Log($"✅ Unlocked weapon: {weaponName}");
            SaveInventory();
        }
        else
        {
            Debug.Log($"🟡 Weapon already unlocked: {weaponName}");
        }
    }

    public bool TryToggleActiveWeapon(string weaponName)
    {
        if (!UnlockedWeapons.Contains(weaponName)) return false;

        if (ActiveWeapons.Contains(weaponName))
        {
            ActiveWeapons.Remove(weaponName);
            SaveInventory();
            return true;
        }
        else
        {
            if (ActiveWeapons.Count >= 2)
            {
                Debug.Log("⚠️ Cannot activate more than 2 weapons.");
                return false;
            }

            ActiveWeapons.Add(weaponName);
            SaveInventory();
            return true;
        }
    }

    // ------------------ Save & Load ------------------

    public void SaveInventory()
    {
        var data = new InventoryData
        {
            keycards = Keycards,
            unlockedWeapons = UnlockedWeapons.ToList(),
            activeWeapons = ActiveWeapons.ToList(),
            unlockedMaps = UnlockedMaps.ToList()
        };

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(SavePath, json);
        Debug.Log($"💾 Saved inventory to: {SavePath}");
    }

    public void LoadInventory()
    {
        if (!File.Exists(SavePath))
        {
            Keycards = 0;
            UnlockedWeapons = new HashSet<string> { "Pistol" };
            ActiveWeapons = new HashSet<string>();
            Debug.Log("🆕 No save found. Created default inventory.");
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            var data = JsonUtility.FromJson<InventoryData>(json);

            Keycards = data.keycards;
            UnlockedWeapons = data.unlockedWeapons?.ToHashSet() ?? new();
            ActiveWeapons = data.activeWeapons?.ToHashSet() ?? new();
            UnlockedMaps = data.unlockedMaps?.ToHashSet() ?? new();
            if (!UnlockedMaps.Contains("Space Station"))
                UnlockedMaps.Add("Space Station"); // default map

            Debug.Log("✅ Loaded inventory from file.");
        }
        catch
        {
            Debug.LogWarning("⚠️ Failed to load or parse inventory. Resetting.");
            Keycards = 0;
            UnlockedWeapons = new HashSet<string> { "Pistol" };
            ActiveWeapons = new HashSet<string>();
        }
    }

    [System.Serializable]
    private class InventoryData
    {
        public int keycards;
        public List<string> unlockedWeapons;
        public List<string> activeWeapons;
        public List<string> unlockedMaps;
    }
}
