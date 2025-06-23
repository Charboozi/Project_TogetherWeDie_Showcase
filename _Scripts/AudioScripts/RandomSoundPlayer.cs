using UnityEngine;
using Unity.Netcode;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class RandomSoundPlayer : NetworkBehaviour
{
    [Header("Sound Settings")]
    public AudioClip[] soundClips;
    public float minInterval = 3f;
    public float maxInterval = 8f;

    [Header("Pitch Variation")]
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    [Header("Voice Limiting")]
    public static int currentVoices = 0;
    public static int maxVoices = 10;

    [Header("Range Settings")]
    public float hearDistance = 15f; // 👈 Local player must be within this

    private AudioSource audioSource;
    private Coroutine playRoutine;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Server controls random timing
        if (IsServer)
            playRoutine = StartCoroutine(SoundRoutine());
    }

    private IEnumerator SoundRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            if (currentVoices < maxVoices)
            {
                int clipIndex = Random.Range(0, soundClips.Length);
                float pitch = Random.Range(minPitch, maxPitch);
                PlaySoundClientRpc(clipIndex, pitch);
            }
        }
    }

    [ClientRpc]
    private void PlaySoundClientRpc(int clipIndex, float pitch)
    {
        if (!IsClient) return;

        var localPlayer = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (localPlayer == null) return;

        float distance = Vector3.Distance(localPlayer.transform.position, transform.position);
        if (distance > hearDistance) return;

        if (clipIndex < 0 || clipIndex >= soundClips.Length) return;

        audioSource.pitch = pitch;
        audioSource.PlayOneShot(soundClips[clipIndex]);
        StartCoroutine(TrackVoiceDuration(soundClips[clipIndex].length));
    }

    private IEnumerator TrackVoiceDuration(float duration)
    {
        currentVoices++;
        yield return new WaitForSeconds(duration);
        currentVoices--;
    }

    public void StopSounds()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        if (audioSource.isPlaying)
            audioSource.Stop();
    }
}
