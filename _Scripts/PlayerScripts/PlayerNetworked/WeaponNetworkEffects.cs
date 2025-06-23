using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class WeaponNetworkEffects : NetworkBehaviour
{
    [Header("Effects")]
    public ParticleSystem muzzleFlash;

    [Header("Camera")]
    [SerializeField] private CameraRecoilController cameraRecoil;

    private bool isFiring = false;
    [SerializeField] private bool isLoopingWeapon = false;

    void OnEnable()
    {
        WeaponController.OnShoot -= TriggerMuzzleFlash;
        WeaponController.OnShoot += TriggerMuzzleFlash;

        WeaponController.OnStopShooting -= StopFiring;
        WeaponController.OnStopShooting += StopFiring;
    }

    void OnDisable()
    {
        WeaponController.OnShoot -= TriggerMuzzleFlash;
        WeaponController.OnStopShooting -= StopFiring;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner && muzzleFlash != null)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void TriggerMuzzleFlash()
    {
        if (!IsOwner) return;

        WeaponBase currentWeapon = CurrentWeaponHolder.Instance?.CurrentWeapon;
        if (currentWeapon == null) return;

        if (isLoopingWeapon)
        {
            if (!isFiring)
            {
                isFiring = true;
                RequestMuzzleFlashServerRpc(true);
            }
        }
        else
        {
            RequestMuzzleFlashServerRpc(true);
            Invoke(nameof(StopMuzzleFlash), currentWeapon.fireRate);
        }


        if (currentWeapon != null && cameraRecoil != null)
        {
            float strength = currentWeapon.recoilStrength;

            // This single value affects vertical kick, horizontal sway, and duration
            cameraRecoil.ApplyRecoil(strength, strength);
        }
    }

    private void StopMuzzleFlash()
    {
        if (isLoopingWeapon)
        {
            if (isFiring)
            {
                isFiring = false;
                RequestMuzzleFlashServerRpc(false);
            }
        }
        else
        {
            RequestMuzzleFlashServerRpc(false);
        }
    }

    public void StopFiring()
    {
        if (!IsOwner) return;

        if (isLoopingWeapon && isFiring)
        {
            isFiring = false;
            RequestMuzzleFlashServerRpc(false);
        }
    }

    [ServerRpc]
    private void RequestMuzzleFlashServerRpc(bool playEffect, ServerRpcParams rpcParams = default)
    {
        var targetIds = new List<ulong>();
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId != rpcParams.Receive.SenderClientId)
                targetIds.Add(clientId);
        }

        var rpcParamsOut = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = targetIds.ToArray() }
        };

        PlayMuzzleFlashClientRpc(playEffect, rpcParamsOut);
    }

    [ClientRpc]
    private void PlayMuzzleFlashClientRpc(bool playEffect, ClientRpcParams clientRpcParams = default)
    {
        if (muzzleFlash == null) return;

        if (playEffect)
        {
            muzzleFlash.Clear(true);
            muzzleFlash.Play(true);
        }
        else
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
