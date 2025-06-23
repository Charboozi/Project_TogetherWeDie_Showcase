using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class HealthProxy : NetworkBehaviour
{
    private EntityHealth entityHealth;

    private void Awake()
    {
        entityHealth = GetComponent<EntityHealth>();
        if (entityHealth == null)
            Debug.LogError("EntityHealth component not found on player object.");
    }

    // These public methods are called locally by equipment items.
    public void AddArmor(int bonus)
    {
        if (NetworkManager.Singleton.IsClient)
            RequestAddArmorServerRpc(bonus);
    }

    public void RemoveArmor(int bonus)
    {
        if (NetworkManager.Singleton.IsClient)
            RequestRemoveArmorServerRpc(bonus);
    }

    public void FullHeal()
    {
        if (NetworkManager.Singleton.IsClient)
            RequestFullHealServerRpc();
    }

    public void MultiplyRegenInterval(float multiplier)
    {
        if (NetworkManager.Singleton.IsClient)
            RequestMultiplyRegenIntervalServerRpc(multiplier);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestAddArmorServerRpc(int bonus, ServerRpcParams rpcParams = default)
    {
        entityHealth.AddArmor(bonus);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestRemoveArmorServerRpc(int bonus, ServerRpcParams rpcParams = default)
    {
        entityHealth.RemoveArmor(bonus);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestFullHealServerRpc(ServerRpcParams rpcParams = default)
    {
        entityHealth.FullHeal();
    }

    public void AddTemporaryArmor(int armorAmount, float duration)
    {
        StartCoroutine(TemporaryArmorCoroutine(armorAmount, duration));
    }

    private IEnumerator TemporaryArmorCoroutine(int armorAmount, float duration)
    {
        AddArmor(armorAmount);

        PersistentScreenTint.Instance.SetPersistentTintForDuration(
            new Color(0.2f, 0.2f, 0.8f), duration
        );

        yield return new WaitForSeconds(duration);

        RemoveArmor(armorAmount);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestMultiplyRegenIntervalServerRpc(float multiplier, ServerRpcParams rpcParams = default)
    {
        entityHealth.regenInterval *= multiplier;
        Debug.Log($"{gameObject.name}: Regen interval modified by multiplier {multiplier}. New regen interval: {entityHealth.regenInterval}");
    }

    public void Heal(int amount)
    {
        if (NetworkManager.Singleton.IsClient)
            RequestHealServerRpc(amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestHealServerRpc(int amount, ServerRpcParams rpcParams = default)
    {
        if (entityHealth != null)
            entityHealth.ApplyHealingServerRpc(amount);
    }
    public void Revive()
    {
        if (NetworkManager.Singleton.IsClient)
            RequestReviveServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestReviveServerRpc(ServerRpcParams rpcParams = default)
    {
        if (entityHealth != null && entityHealth.isDowned.Value)
        {
            entityHealth.Revive();
        }
    }
}
