using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioSettingsInitializer : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;

    private static bool hasInitializedOnce = false;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        if (!hasInitializedOnce)
        {
            StartCoroutine(ApplyAudioSettingsDelayed());
            hasInitializedOnce = true;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ApplyAudioSettingsDelayed());
    }

    private IEnumerator ApplyAudioSettingsDelayed()
    {
        yield return null; // wait one frame to ensure AudioSource is fully initialized

        ApplyVolume("MasterVolume", PlayerPrefs.GetFloat("MasterVolume", 1f));
        ApplyVolume("MusicVolume", PlayerPrefs.GetFloat("MusicVolume", 1f));
        ApplyVolume("SFXVolume", PlayerPrefs.GetFloat("SFXVolume", 1f));
    }

    private void ApplyVolume(string parameter, float value)
    {
        float clamped = Mathf.Clamp(value, 0.0001f, 1f);
        float dB = Mathf.Log10(clamped) * 20f;
        mixer.SetFloat(parameter, dB);
    }
}
