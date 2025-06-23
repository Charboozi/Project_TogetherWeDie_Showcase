using UnityEngine;
using Unity.Netcode;

public class ChargeStation : NetworkBehaviour, IInteractableAction, IBroadcastClientAction
{
    [SerializeField] private ParticleSystem chargeEffect;

    public void DoAction()
    {
        if (!IsServer) return;

        if (InteractableChargeManager.Instance != null)
        {
            InteractableChargeManager.Instance.FullyRechargeAll();
            Debug.Log("🔋 ChargeStation recharged all interactables (synced to everyone).");
        }
        else
        {
            Debug.LogWarning("⚠️ No InteractableChargeManager found in the scene.");
        }
        
        if (BlackoutManager.Instance != null)
        {
            BlackoutManager.Instance.RequestLightsOn();
            Debug.Log("💡 Lights restored by ChargeStation.");
        }
        else
        {
            Debug.LogWarning("⚠️ No LightmapSwitcher found in the scene.");
        }
    }

    public void DoAllClientsAction()
    {
        if (chargeEffect != null)
            chargeEffect.Play();

        if (FlashEffect.Instance != null)
            FlashEffect.Instance.TriggerFlash();
    }
}
