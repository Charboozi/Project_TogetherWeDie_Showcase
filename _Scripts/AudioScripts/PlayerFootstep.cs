using UnityEngine;
using Unity.Netcode;
using System.Linq;

[RequireComponent(typeof(AudioSource))]
public class PlayerFootstep : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float baseStepInterval = 0.4f;
    [SerializeField] private float stepRandomness = 0.15f;
    [SerializeField] private Vector2 volumeRange = new Vector2(0.8f, 1.2f);
    [SerializeField] private AudioClip[] footstepClips;

    private AudioSource audioSource;
    private NetworkedCharacterMovement movement;
    private float stepTimer;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        movement = GetComponent<NetworkedCharacterMovement>();
    }

    private void Update()
    {
        if (!CanPlayFootstep()) return;

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            PlayFootstepLocal(); // Local footstep
            RequestFootstepServerRpc(); // Tell server to play for others

            // Randomize next step interval slightly
            float randomInterval = baseStepInterval * Random.Range(1f - stepRandomness, 1f + stepRandomness);
            stepTimer = randomInterval;
        }
    }

    private bool CanPlayFootstep()
    {
        if (movement == null) return false;

        bool isGrounded = movement.IsGrounded;
        bool isMoving = movement.MovementVelocity.sqrMagnitude > 0.1f;

        var health = GetComponent<EntityHealth>();
        bool isDowned = health != null && health.isDowned.Value;

        return !isDowned && isGrounded && isMoving;
    }

    private void PlayFootstepLocal()
    {
        if (footstepClips.Length == 0) return;

        int randomIndex = Random.Range(0, footstepClips.Length);
        float randomPitch = Random.Range(0.95f, 1.05f);
        float randomVolume = Random.Range(volumeRange.x, volumeRange.y);

        PlayFootstepAudio(randomIndex, randomPitch, randomVolume);
    }
    
    private void PlayFootstepAudio(int clipIndex, float pitch, float volume)
    {
        if (footstepClips.Length == 0) return;
        if (clipIndex < 0 || clipIndex >= footstepClips.Length) return;

        AudioClip clip = footstepClips[clipIndex];
        audioSource.pitch = pitch;
        audioSource.volume = volume;
        audioSource.PlayOneShot(clip);
    }

    [ServerRpc]
    private void RequestFootstepServerRpc(ServerRpcParams rpcParams = default)
    {
        // Send to everyone EXCEPT sender
        PlayFootstepClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds
                    .Where(id => id != rpcParams.Receive.SenderClientId)
                    .ToArray()
            }
        });
    }

    [ClientRpc]
    private void PlayFootstepClientRpc(ClientRpcParams clientRpcParams = default)
    {
        PlayFootstepLocal();
    }
}
