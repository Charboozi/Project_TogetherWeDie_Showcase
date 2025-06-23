using UnityEngine;
using Unity.Netcode;

public abstract class BaseLocalPickup : MonoBehaviour
{
    [SerializeField] protected float pickupRange = 3f;
    [SerializeField] protected LayerMask pickupLayer;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    private AudioSource audioSource;

    protected virtual void OnEnable()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        PlayerInput.OnInteractPressed += AttemptPickup;
    }

    protected virtual void OnDisable()
    {
        PlayerInput.OnInteractPressed -= AttemptPickup;
    }

    private void AttemptPickup()
    {
        if (!NetworkManager.Singleton.IsClient) return;

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, pickupRange, pickupLayer))
        {
            var pickup = hit.collider.GetComponent<NetworkedPickupableItem>();
            if (pickup != null)
            {
                if (OnPickupFound(pickup))
                {
                    PlayPickupSound(); // ✅ Only play sound if pickup succeeded
                }
            }
        }
    }

    /// <summary>
    /// Called when a pickup is found. Should return true if pickup was successful and sound should play.
    /// </summary>
    protected abstract bool OnPickupFound(NetworkedPickupableItem pickup);

    protected void DespawnPickup(NetworkedPickupableItem pickup)
    {
        pickup.Despawn();
    }

    private void PlayPickupSound()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("🎧 No AudioSource found!");
            return;
        }

        if (pickupSound == null)
        {
            Debug.LogWarning("🔇 No pickupSound assigned!");
            return;
        }

        Debug.Log($"🎵 Playing pickup sound: {pickupSound.name}");
        audioSource.PlayOneShot(pickupSound);
    }
}
