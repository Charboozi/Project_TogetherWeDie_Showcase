using UnityEngine;
using Unity.Netcode;

public abstract class PowerupBase : NetworkBehaviour
{
    [Tooltip("If true, all players get the effect; otherwise, only the player who triggers it.")]
    public bool applyToAll = false;

    [Header("Audio")]
    [SerializeField] private UnityEngine.Audio.AudioMixerGroup outputMixerGroup;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip loopedEffectSound;

    // Trigger detection common to all powerups.
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Retrieve the player's network identity.
        NetworkObject playerNetworkObject = other.GetComponent<NetworkObject>();
        if (playerNetworkObject == null)
            return;

        // Request the powerup effect on the server.
        CollectPowerupServerRpc(playerNetworkObject.OwnerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CollectPowerupServerRpc(ulong triggeringClientId)
    {
        if (applyToAll)
        {
            ApplyPowerupClientRpc(GetEffectValue());

            // ✅ If host is included in "all", we need to play it locally too
            if (IsHost)
            {
                PlayPickupSound();
            }
        }
        else
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { triggeringClientId } }
            };

            ApplyPowerupClientRpc(GetEffectValue(), clientRpcParams);

            // ✅ If host is the one who triggered it, play sound locally
            if (IsHost && triggeringClientId == NetworkManager.Singleton.LocalClientId)
            {
                PlayPickupSound();
            }
        }

        GetComponent<NetworkObject>().Despawn(true);
    }

    // Derived classes override this to specify the effect's magnitude (e.g., ammo amount, health value, etc.)
    protected abstract int GetEffectValue();

    // Derived classes override this ClientRpc to implement the specific effect.
    [ClientRpc]
    protected virtual void ApplyPowerupClientRpc(int effectValue, ClientRpcParams clientRpcParams = default)
    {
        PlayPickupSound();
    }

    protected void PlayPickupSound()
    {
        if (pickupSound != null)
        {
            GameObject soundObj = new GameObject("PickupSound");
            soundObj.transform.position = transform.position;

            AudioSource source = soundObj.AddComponent<AudioSource>();
            source.clip = pickupSound;
            source.volume = 0.45f;
            source.outputAudioMixerGroup = outputMixerGroup;
            source.Play();

            Destroy(soundObj, pickupSound.length);
        }
    }

    protected GameObject PlayLoopedEffectSound(float duration)
    {
        if (loopedEffectSound == null)
            return null;

        GameObject loopObj = new GameObject("LoopedPowerupSound");
        AudioSource source = loopObj.AddComponent<AudioSource>();
        source.clip = loopedEffectSound;
        source.outputAudioMixerGroup = outputMixerGroup;
        source.loop = true;
        source.volume = 0.45f;
        source.Play();

        Destroy(loopObj, duration);
        return loopObj;
    }
}

