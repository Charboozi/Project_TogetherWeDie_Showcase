using UnityEngine;
using Unity.Netcode;

public abstract class TrapBase : NetworkBehaviour, IInteractableAction
{
    [Header("Trap Settings")]
    public float activeDuration = 5f;

    [Header("Trap Audio")]
    [SerializeField] private AudioClip activeLoopClip;
    [SerializeField] AudioSource loopingAudioSource;

    [HideInInspector]public bool isActive = false;

    public void DoAction()
    {
        if (!IsServer || isActive)
            return;

        ActivateTrap();
    }

    protected void ActivateTrap()
    {
        isActive = true;
        OnTrapActivated();

        ActivateTrapClientRpc();

        Invoke(nameof(DeactivateTrap), activeDuration);
    }

    private void DeactivateTrap()
    {
        isActive = false;
        OnTrapDeactivated();

        DeactivateTrapClientRpc();
    }

    protected abstract void OnTrapActivated();
    protected abstract void OnTrapDeactivated();

    [ClientRpc]
    private void ActivateTrapClientRpc()
    {
        OnTrapActivated();
        PlayLoopingSound();
    }

    [ClientRpc]
    private void DeactivateTrapClientRpc()
    {
        OnTrapDeactivated();
        StopLoopingSound();
    }

    private void PlayLoopingSound()
    {
        if (loopingAudioSource != null && activeLoopClip != null)
        {
            loopingAudioSource.clip = activeLoopClip;
            loopingAudioSource.loop = true;
            loopingAudioSource.Play();
        }
    }

    private void StopLoopingSound()
    {
        if (loopingAudioSource != null && loopingAudioSource.isPlaying)
        {
            loopingAudioSource.Stop();
        }
    }

    public virtual void Prewarm()
    {
        // Prewarm Physics
        Physics.OverlapSphere(transform.position, 0.1f);

        // Prewarm looping audio
        if (loopingAudioSource != null && activeLoopClip != null)
        {
            loopingAudioSource.clip = activeLoopClip;
            loopingAudioSource.Play();
            loopingAudioSource.Stop();
        }
    }
}
