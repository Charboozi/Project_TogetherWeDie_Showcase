using UnityEngine;

public class TrapTrigger : MonoBehaviour
{
    public FireTrap fireTrap;

    private void OnTriggerEnter(Collider other)
    {
        fireTrap?.NotifyTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        fireTrap?.NotifyTriggerExit(other);
    }
}
