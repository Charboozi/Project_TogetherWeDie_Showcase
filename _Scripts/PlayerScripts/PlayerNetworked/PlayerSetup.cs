using Unity.Netcode;
using UnityEngine;

public class PlayerSetup : NetworkBehaviour
{
    private void Start()
    {
        if (IsOwner)
        {
            SetupPlayerUI();
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            SetupPlayerUI();
        }
    }

    private void SetupPlayerUI()
    {
        PlayerHealthUI playerHealthUI = FindFirstObjectByType<PlayerHealthUI>();
        if (playerHealthUI != null)
        {
            EntityHealth entityHealth = GetComponent<EntityHealth>();
            playerHealthUI.SetPlayer(entityHealth);
        }
    }
}
