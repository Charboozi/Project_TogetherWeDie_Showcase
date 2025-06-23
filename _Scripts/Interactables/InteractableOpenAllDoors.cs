using UnityEngine;

public class InteractableOpenAllDoors : MonoBehaviour, IInteractableAction
{
    public void DoAction()
    {
        if (AffectableDoorsList.Instance == null)
        {
            Debug.LogWarning("🚫 AffectableDoors instance not found!");
            return;
        }

        foreach (var door in AffectableDoorsList.Instance.GetAllDoors())
        {
            if (door != null && !door.IsLocked)
            {
                door.Open();
                Debug.Log($"🔓 Opened door: {door.name}");
            }
        }
    }
}
