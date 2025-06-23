using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class AlarmSequenceManager : NetworkBehaviour
{
    public static AlarmSequenceManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioSource alarmAudioSource;

    [Header("Alarm Settings")]
    [SerializeField] private float alarmDuration = 5f;

    private Coroutine alarmRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ActivateAlarm()
    {
        if (!IsServer)
        {
            Debug.LogWarning("Only the server can activate the alarm!");
            return;
        }

        if (alarmRoutine != null)
            StopCoroutine(alarmRoutine);

        ActivateAlarmClientRpc();
        PlayAlarm();

        alarmRoutine = StartCoroutine(AlarmTimerRoutine());
    }

    public void DeactivateAlarm()
    {
        if (!IsServer)
        {
            Debug.LogWarning("Only the server can deactivate the alarm!");
            return;
        }

        if (alarmRoutine != null)
        {
            StopCoroutine(alarmRoutine);
            alarmRoutine = null;
        }

        DeactivateAlarmClientRpc();
        StopAlarm();
    }

    [ClientRpc]
    private void ActivateAlarmClientRpc()
    {
        if (IsServer) return;
        PlayAlarm();
    }

    [ClientRpc]
    private void DeactivateAlarmClientRpc()
    {
        if (IsServer) return;
        StopAlarm();
    }

    private void PlayAlarm()
    {
        if (alarmAudioSource != null)
        {
            alarmAudioSource.loop = true;
            if (!alarmAudioSource.isPlaying)
                alarmAudioSource.Play();
        }
    }

    private void StopAlarm()
    {
        if (alarmAudioSource != null)
        {
            alarmAudioSource.loop = false;

            // If it's playing, let it finish the current loop
            if (alarmAudioSource.isPlaying)
            {
                float remainingTime = alarmAudioSource.clip.length - alarmAudioSource.time;
                StartCoroutine(WaitAndStopAudio(remainingTime));
            }
        }
    }

    private IEnumerator WaitAndStopAudio(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (alarmAudioSource.isPlaying)
            alarmAudioSource.Stop();
    }

    private IEnumerator AlarmTimerRoutine()
    {
        yield return new WaitForSeconds(alarmDuration);
        DeactivateAlarm(); // Will stop it on all clients via RPC
    }
}
