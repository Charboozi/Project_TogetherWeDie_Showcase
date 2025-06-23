using System.Collections.Generic;
using UnityEngine;

public class AffectableTrapsList : MonoBehaviour
{
    public static AffectableTrapsList Instance { get; private set; }

    [Header("Manually Assigned Traps")]
    [SerializeField] private List<TrapBase> traps = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public TrapBase GetRandomTrap()
    {
        if (traps.Count == 0) return null;
        return traps[Random.Range(0, traps.Count)];
    }

    public List<TrapBase> GetAllTraps() => traps;

    public void AddTrap(TrapBase trap)
    {
        if (!traps.Contains(trap))
            traps.Add(trap);
    }

    public void RemoveTrap(TrapBase trap)
    {
        if (traps.Contains(trap))
            traps.Remove(trap);
    }
}
