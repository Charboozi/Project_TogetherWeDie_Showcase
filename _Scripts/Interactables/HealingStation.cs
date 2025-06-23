using UnityEngine;
using Unity.Netcode;

public class HealingStation : MonoBehaviour, IInteractableAction, IClientOnlyAction
{
    public void DoClientAction()
    {
        var player = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (player == null) return;

        var proxy = player.GetComponent<HealthProxy>();
        if (proxy != null)
        {
            proxy.FullHeal();
            FadeScreenEffect.Instance.ShowEffect(Color.green, 0.5f, 1.5f);
            Debug.Log("🩹 Healing station used.");
        }
        else
        {
            Debug.LogWarning("No HealthProxy found on player.");
        }
    }
    public void DoAction(){}
}
