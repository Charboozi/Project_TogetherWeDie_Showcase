using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsController : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] private AudioMixer mixer;

    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        if (masterSlider != null)
        {
            float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
            masterSlider.value = masterVol;
            SetMasterVolume(masterVol); // ✅ apply immediately
            masterSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (musicSlider != null)
        {
            float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicSlider.value = musicVol;
            SetMusicVolume(musicVol); // ✅ apply immediately
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxSlider.value = sfxVol;
            SetSFXVolume(sfxVol); // ✅ apply immediately
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }


    public void SetMasterVolume(float value)
    {
        mixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f);
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        mixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        mixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }
}
