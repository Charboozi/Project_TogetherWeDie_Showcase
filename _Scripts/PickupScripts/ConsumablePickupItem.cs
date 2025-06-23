using UnityEngine;
using Unity.Netcode;

public class ConsumablePickupItem : NetworkBehaviour
{
    public string ConsumableType = "Keycard"; // Set in inspector
    public int Amount = 1;
}
