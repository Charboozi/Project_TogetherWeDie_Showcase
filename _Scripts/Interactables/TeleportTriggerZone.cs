using UnityEngine;

public class TeleportTriggerZone : MonoBehaviour
{
    public TeleporterInteractable teleporter;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            teleporter.AddPlayerToZone(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            teleporter.RemovePlayerFromZone(other.gameObject);
        }
    }
}
