using UnityEngine;
using System.Collections;

public class WeaponRechargeAudioHandler : MonoBehaviour
{
    [SerializeField] private AudioSource chargingLoopAudioSource;
    [SerializeField] private AudioClip chargingLoopClip;

    private Coroutine fadeOutCoroutine;

    private void Start()
    {
        StartCoroutine(WarmUpAudio());
    }

    private IEnumerator WarmUpAudio()
    {
        if (chargingLoopAudioSource == null || chargingLoopClip == null)
            yield break;

        chargingLoopAudioSource.clip = chargingLoopClip;
        chargingLoopAudioSource.volume = 0f;
        chargingLoopAudioSource.loop = false;
        chargingLoopAudioSource.Play();

        yield return new WaitForSeconds(0.01f); // Wait a frame or two

        chargingLoopAudioSource.Stop();
        chargingLoopAudioSource.clip = null;
    }

    public void PlayChargingLoop()
    {
        if (chargingLoopAudioSource == null || chargingLoopClip == null) return;

        if (fadeOutCoroutine != null) StopCoroutine(fadeOutCoroutine);

        chargingLoopAudioSource.clip = chargingLoopClip;
        chargingLoopAudioSource.volume = 0f;
        chargingLoopAudioSource.loop = true;
        chargingLoopAudioSource.Play();
        StartCoroutine(FadeInAudio(0.2f));
    }

    public void StopChargingLoop()
    {
        if (chargingLoopAudioSource == null || !chargingLoopAudioSource.isPlaying) return;

        if (fadeOutCoroutine != null) StopCoroutine(fadeOutCoroutine);
        fadeOutCoroutine = StartCoroutine(FadeOutAudio(0.5f));
    }

    private IEnumerator FadeInAudio(float duration)
    {
        float t = 0f;
        float targetVolume = 0.2f;
        while (t < duration)
        {
            t += Time.deltaTime;
            chargingLoopAudioSource.volume = Mathf.Lerp(0f, targetVolume, t / duration);
            yield return null;
        }
        chargingLoopAudioSource.volume = targetVolume;
    }

    private IEnumerator FadeOutAudio(float duration)
    {
        float startVolume = chargingLoopAudioSource.volume;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            chargingLoopAudioSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        chargingLoopAudioSource.Stop();
        chargingLoopAudioSource.clip = null;
        chargingLoopAudioSource.volume = startVolume;
    }
}
