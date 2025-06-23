using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

[RequireComponent(typeof(ParticleSystem), typeof(AudioSource))]
public class ParticleSoundController : MonoBehaviour
{   
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    private ParticleSystem ps;
    private AudioSource audioSource;

    [Header("Settings")]
    [SerializeField] private bool isLooping = false;
    [SerializeField] private float fadeInSpeed = 2f;
    [SerializeField] private float fadeOutSpeed = 2f;

    private Coroutine fadeCoroutine;
    private bool isPlaying = false;

    private ParticleSystem.Particle[] _particles = new ParticleSystem.Particle[16]; // reusable buffer
    private int _lastParticleCount = 0;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        _lastParticleCount = ps.GetParticles(_particles);
    }

    void Update()
    {
        if (ps.isEmitting)
        {
            if (isLooping)
            {
                if (!isPlaying)
                {
                    isPlaying = true;
                    StartFadeIn();
                }
            }
            else
            {
                int count = ps.GetParticles(_particles);
                if (count > _lastParticleCount)
                {
                    PlayGunshotSound(); // ✅ Reliable even on short-lived bursts
                }
                _lastParticleCount = count;
            }
        }
        else
        {
            if (isLooping && isPlaying)
            {
                isPlaying = false;
                StartFadeOut();
            }

            _lastParticleCount = 0; // reset for burst detection when emission stops
        }
    }

    private void PlayGunshotSound()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            GameObject tempAudio = new GameObject("TempGunshotSound");
            tempAudio.transform.position = transform.position;

            AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
            tempSource.clip = audioSource.clip;
            tempSource.volume = audioSource.volume;
            tempSource.spatialBlend = audioSource.spatialBlend;
            tempSource.priority = audioSource.priority;
            tempSource.outputAudioMixerGroup = sfxMixerGroup;
            tempSource.Play();

            Destroy(tempAudio, tempSource.clip.length);
        }
    }

    private void StartFadeIn()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeAudio(1f, fadeInSpeed));
    }

    private void StartFadeOut()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeAudio(0f, fadeOutSpeed));
    }

    private IEnumerator FadeAudio(float targetVolume, float speed)
    {
        if (targetVolume > 0f && !audioSource.isPlaying)
        {
            audioSource.volume = 0f;
            audioSource.Play();
        }

        float startVolume = audioSource.volume;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        audioSource.volume = targetVolume;

        if (targetVolume == 0f)
        {
            audioSource.Stop();
        }
    }

    private void OnDisable()
    {
        if (isLooping && isPlaying)
        {
            isPlaying = false;
            audioSource.Stop();
        }
    }
}
