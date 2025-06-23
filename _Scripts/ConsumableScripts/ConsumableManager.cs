using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Singleton to to call when adding and using consumables 
/// </summary>
[System.Serializable]
public class ConsumableEvent : UnityEvent<string, int> {}

public class ConsumableManager : MonoBehaviour
{
    public static ConsumableManager Instance { get; private set; }

    private Dictionary<string, int> consumables = new();
    public ConsumableEvent OnConsumableChanged;

    private void Start()
    {
        //Add("Keycard", 100); // Give player keycards at the start FOR TESTING
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Add(string type, int amount = 1)
    {
        if (!consumables.ContainsKey(type))
            consumables[type] = 0;

        consumables[type] += amount;
        OnConsumableChanged?.Invoke(type, consumables[type]);

        // 🔔 Optional: Post feed message when certain items are added
        if (type == "Keycard" && amount > 0 && GameFeedManager.Instance != null)
        {
            GameFeedManager.Instance.PostLocalFeedMessage($"Received {amount} {type}{(amount > 1 ? "s" : "")}!", Color.yellow);
        }
    }

    public bool Use(string type, int amount = 1)
    {
        if (consumables.TryGetValue(type, out int count) && count >= amount)
        {
            consumables[type] -= amount;
            OnConsumableChanged?.Invoke(type, consumables[type]);
            return true;
        }

        Debug.Log($"❌ Not enough of '{type}' to use!");
        return false;
    }

    public int Get(string type) => consumables.TryGetValue(type, out int count) ? count : 0;
}
