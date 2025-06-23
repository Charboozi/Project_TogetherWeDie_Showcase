using UnityEngine;

public class TrapPrewarmManager : MonoBehaviour
{
    private void Awake()
    {
        // Find all traps in the scene and prewarm them
        var traps = FindObjectsByType<TrapBase>(FindObjectsSortMode.None);
        foreach (var trap in traps)
        {
            trap.Prewarm();
        }

        Debug.Log($"[TrapPrewarmManager] Prewarmed {traps.Length} traps.");
    }
}
