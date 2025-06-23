using UnityEngine;
using Unity.Netcode;

public class AmmoStation : NetworkBehaviour, IInteractableAction, IClientOnlyAction
{
    public void DoAction(){}

    public void DoClientAction()
    {
        // This runs only on the client that triggered the interaction!
        GiveAmmoToLocalWeapon();
    }

    private void GiveAmmoToLocalWeapon()
    {
        WeaponBase weapon = CurrentWeaponHolder.Instance.CurrentWeapon;
        if (weapon != null)
        {
            int amountToGive = weapon.maxAmmo * AmmoMultiplierManager.Instance.AmmoMultiplier;;
            weapon.reserveAmmo += amountToGive;
            Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] AmmoStation applied: +{amountToGive} reserve ammo.");
        }
        else
        {
            Debug.LogWarning("AmmoStation: No current weapon found on client.");
        }
    }
}
