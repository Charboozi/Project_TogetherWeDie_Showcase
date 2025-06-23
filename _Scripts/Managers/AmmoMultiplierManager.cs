using UnityEngine;

public class AmmoMultiplierManager : MonoBehaviour
{
    public static AmmoMultiplierManager Instance { get; private set; }

    public int AmmoMultiplier { get; private set; } = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetMultiplier(int multiplier)
    {
        AmmoMultiplier = multiplier;
        Debug.Log($"Ammo multiplier set to: {AmmoMultiplier}");
    }

    public void ResetMultiplier()
    {
        AmmoMultiplier = 1;
        Debug.Log("Ammo multiplier reset to 1");
    }
}
