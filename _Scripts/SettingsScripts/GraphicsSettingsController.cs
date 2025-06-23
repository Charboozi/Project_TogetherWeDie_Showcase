using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GraphicsSettingsController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Slider fovSlider;
    [SerializeField] private TMP_Text fovValueLabel;
    [SerializeField] private Toggle vsyncToggle;
    [SerializeField] private TMP_Dropdown aaDropdown;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_Text sensitivityValueLabel;

    [Header("Target References")]
    [SerializeField] private Camera mainCamera;

    private Resolution[] resolutions;

    private void Start()
    {
        resolutions = Screen.resolutions;

        SetupResolutions();
        SetupQualityDropdown();
        SetupAntiAliasingDropdown();
        LoadSettings();

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(SetQualityLevel);
        if (fovSlider != null)
            fovSlider.onValueChanged.AddListener(SetFOV);
        if (vsyncToggle != null)
            vsyncToggle.onValueChanged.AddListener(SetVSync);
        if (aaDropdown != null)
            aaDropdown.onValueChanged.AddListener(SetAntiAliasing);
        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
    }

    private void SetupResolutions()
    {
        if (resolutionDropdown == null) return;

        resolutionDropdown.ClearOptions();

        List<string> options = new();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width} x {resolutions[i].height}";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);

        int savedIndex = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
        savedIndex = Mathf.Clamp(savedIndex, 0, resolutions.Length - 1);
        resolutionDropdown.value = savedIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void SetupQualityDropdown()
    {
        if (qualityDropdown == null) return;

        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));

        int savedQuality = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        qualityDropdown.value = Mathf.Clamp(savedQuality, 0, QualitySettings.names.Length - 1);
        qualityDropdown.RefreshShownValue();
    }

    private void SetupAntiAliasingDropdown()
    {
        if (aaDropdown == null) return;

        aaDropdown.ClearOptions();
        aaDropdown.AddOptions(new List<string> { "Off", "2x", "4x", "8x" });
    }

    private void LoadSettings()
    {
        if (resolutions == null || resolutions.Length == 0)
            resolutions = Screen.resolutions;

        // Resolution
        int resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
        resolutionIndex = Mathf.Clamp(resolutionIndex, 0, resolutions.Length - 1);
        if (resolutionDropdown != null)
        {
            resolutionDropdown.value = resolutionIndex;
            SetResolution(resolutionIndex);
        }

        // Fullscreen
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = isFullscreen;
            SetFullscreen(isFullscreen);
        }

        // Quality
        int quality = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        quality = Mathf.Clamp(quality, 0, QualitySettings.names.Length - 1);
        if (qualityDropdown != null)
        {
            qualityDropdown.value = quality;
            SetQualityLevel(quality);
        }

        // FOV
        float fov = PlayerPrefs.GetFloat("FOV", 60f);
        fov = Mathf.Clamp(fov, 60f, 90f);
        if (fovSlider != null)
        {
            fovSlider.value = fov;
            SetFOV(fov);
        }

        // VSync
        bool vsync = PlayerPrefs.GetInt("VSync", 1) == 1;
        if (vsyncToggle != null)
        {
            vsyncToggle.isOn = vsync;
            SetVSync(vsync);
        }

        // Anti-Aliasing
        int aaLevelIndex = PlayerPrefs.GetInt("AntiAliasing", 0);
        aaLevelIndex = Mathf.Clamp(aaLevelIndex, 0, 3);
        if (aaDropdown != null)
        {
            aaDropdown.value = aaLevelIndex;
            SetAntiAliasing(aaLevelIndex);
        }

        // Sensitivity
        float sensitivity = PlayerPrefs.GetFloat("Sensitivity", 20f);
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = Mathf.Clamp(sensitivity, 1f, 400f);
            SetSensitivity(sensitivity);
        }
    }

    public static float Sensitivity { get; private set; } = 20f; // Static if needed globally


    public void SetSensitivity(float value)
    {
        float clamped = Mathf.Clamp(value, 1f, 400f);
        InputSensitivity.Current = clamped;
        PlayerPrefs.SetFloat("Sensitivity", clamped);

        if (sensitivityValueLabel != null)
            sensitivityValueLabel.text = clamped.ToString("F1");
    }

    public void SetResolution(int index)
    {
        if (resolutions == null || index >= resolutions.Length) return;

        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        PlayerPrefs.SetInt("ResolutionIndex", index);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    public void SetQualityLevel(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("QualityLevel", index);
    }

    public void SetFOV(float fov)
    {
        float clampedFOV = Mathf.Clamp(fov, 60f, 90f);
        if (mainCamera != null)
            mainCamera.fieldOfView = clampedFOV;

        PlayerPrefs.SetFloat("FOV", clampedFOV);

        if (fovValueLabel != null)
            fovValueLabel.text = Mathf.RoundToInt(clampedFOV).ToString();
    }

    public void SetVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;
        PlayerPrefs.SetInt("VSync", enabled ? 1 : 0);
    }

    public void SetAntiAliasing(int index)
    {
        int[] levels = { 0, 2, 4, 8 };
        if (index >= 0 && index < levels.Length)
        {
            QualitySettings.antiAliasing = levels[index];
            PlayerPrefs.SetInt("AntiAliasing", index);
        }
    }

}
