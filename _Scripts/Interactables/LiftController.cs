using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class LiftController : NetworkBehaviour, IInteractableAction
{
    [Header("Lift Settings (Animated)")]
    [SerializeField] private Animator liftAnimator;
    [SerializeField] private string liftAnimationTrigger = "Move";

    [Header("Audio")]
    [SerializeField] private AudioClip liftLoopSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Lift Door (Procedural)")]
    [SerializeField] private Transform door;
    [SerializeField] private Vector3 doorOpenOffset = new Vector3(0, 2f, 0); // Door movement direction
    [SerializeField] private float doorMoveSpeed = 3f;
    [SerializeField] private float doorCloseDelay = 5f;

    private bool isMoving = false;

    private Vector3 doorClosedPos;
    private Vector3 doorOpenPos;

    private void Awake()
    {
        if (door != null)
        {
            doorClosedPos = door.localPosition;
            doorOpenPos = doorClosedPos + doorOpenOffset;
        }
    }

    public void DoAction()
    {
        if (!IsServer || isMoving)
            return;

        StartLift();
    }

    private void StartLift()
    {
        isMoving = true;

        BalloonMinigameManager.Instance.StartMinigame();
        PlayLiftClientRpc();

        float animationLength = GetAnimationClipLength(liftAnimationTrigger);
        Invoke(nameof(ResetLift), animationLength);
    }

    [ClientRpc]
    private void PlayLiftClientRpc()
    {
        // Trigger lift animation
        if (liftAnimator != null)
        {
            liftAnimator.SetTrigger(liftAnimationTrigger);
        }

        // Play looping lift sound
        if (audioSource != null && liftLoopSound != null)
        {
            audioSource.clip = liftLoopSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        // Open door
        if (door != null)
        {
            StopAllCoroutines();
            StartCoroutine(MoveDoor(doorClosedPos, doorOpenPos, doorMoveSpeed, () =>
            {
                Invoke(nameof(CloseDoor), doorCloseDelay);
            }));
        }
    }

    private void CloseDoor()
    {
        if (door != null)
        {
            StartCoroutine(MoveDoor(doorOpenPos, doorClosedPos, doorMoveSpeed));
        }
    }

    private IEnumerator MoveDoor(Vector3 from, Vector3 to, float speed, System.Action onComplete = null)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            door.localPosition = Vector3.Lerp(from, to, t);
            yield return null;
        }
        door.localPosition = to;
        onComplete?.Invoke();
    }

    private void ResetLift()
    {
        isMoving = false;

        // Stop lift looping sound cleanly
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
        }
    }

    private float GetAnimationClipLength(string triggerName)
    {
        if (liftAnimator == null) return 5f;

        RuntimeAnimatorController ac = liftAnimator.runtimeAnimatorController;
        foreach (var clip in ac.animationClips)
        {
            if (clip.name == triggerName)
            {
                return clip.length;
            }
        }
        return 60f; // Fallback duration
    }
}
