using UnityEngine;
using System.IO;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    public int Coins { get; private set; }

    private string SavePath => Application.persistentDataPath + "/save_coins.json";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadCoins();
    }

    public void Add(int amount)
    {
        Coins += amount;
        Debug.Log($"➕ Added {amount} coins. New total: {Coins}");
        SaveCoins();
    }

    public bool Spend(int amount)
    {
        if (Coins >= amount)
        {
            Coins -= amount;
            Debug.Log($"➖ Spent {amount} coins. Remaining: {Coins}");
            SaveCoins();
            return true;
        }

        Debug.LogWarning("❌ Not enough coins to spend.");
        return false;
    }

    private void SaveCoins()
    {
        File.WriteAllText(SavePath, Coins.ToString());
        Debug.Log($"💾 Coins saved to {SavePath}");
    }

    private void LoadCoins()
    {
        if (!File.Exists(SavePath))
        {
            Coins = 0;
            Debug.Log("🆕 No save file found. Starting at 0 coins.");
            return;
        }

        string raw = File.ReadAllText(SavePath);
        if (int.TryParse(raw, out int loaded))
        {
            Coins = loaded;
            Debug.Log($"✅ Coins loaded from file: {Coins}");
        }
        else
        {
            Debug.LogError("❌ Failed to parse saved coins.");
            Coins = 0;
        }
    }
}
