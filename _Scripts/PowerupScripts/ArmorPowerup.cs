using UnityEngine;
using Unity.Netcode;

public class ArmorPowerup : PowerupBase
{
    [Tooltip("Amount of temporary armor to apply.")]
    [SerializeField] private int armorBonus = 10;

    [Tooltip("How long the armor lasts (in seconds).")]
    [SerializeField] private float duration = 20f;

    protected override int GetEffectValue()
    {
        return armorBonus;
    }

    [ClientRpc]
    protected override void ApplyPowerupClientRpc(int _, ClientRpcParams clientRpcParams = default)
    {
        var player = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (player == null)
        {
            Debug.LogWarning("ArmorPowerup: No local player found.");
            return;
        }

        var proxy = player.GetComponent<HealthProxy>();
        if (proxy != null)
        {
            proxy.AddTemporaryArmor(armorBonus, duration);
            PlayLoopedEffectSound(duration);
        }

        PowerupUIController.Instance?.ShowArmorBoost(duration);
    }

}
