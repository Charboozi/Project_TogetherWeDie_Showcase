using UnityEngine;
using UnityEngine.SceneManagement;

public class GraphicsSettingsInitializer : MonoBehaviour
{
    private static bool hasInitializedOnce = false;

    private void Awake()
    {
        // Persist across scenes
        DontDestroyOnLoad(this.gameObject);

        if (!hasInitializedOnce)
        {
            ApplyGraphicsSettings(); // Initial application
            hasInitializedOnce = true;
        }

        // Listen for new scenes to re-apply scene-dependent settings
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyGraphicsSettings(); // Re-apply in each new scene (e.g., FOV for new main camera)
    }

    private void ApplyGraphicsSettings()
    {
        // Resolution & fullscreen
        Resolution[] resolutions = Screen.resolutions;
        int resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", resolutions.Length - 1);
        resolutionIndex = Mathf.Clamp(resolutionIndex, 0, resolutions.Length - 1);
        Resolution res = resolutions[resolutionIndex];
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        Screen.SetResolution(res.width, res.height, isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);

        // Quality
        int quality = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        QualitySettings.SetQualityLevel(quality);

        // VSync
        QualitySettings.vSyncCount = PlayerPrefs.GetInt("VSync", 1) == 1 ? 1 : 0;

        // AntiAliasing
        int[] aaLevels = { 0, 2, 4, 8 };
        int aaIndex = Mathf.Clamp(PlayerPrefs.GetInt("AntiAliasing", 0), 0, aaLevels.Length - 1);
        QualitySettings.antiAliasing = aaLevels[aaIndex];

        // FOV (requires scene-specific Camera)
        float fov = PlayerPrefs.GetFloat("FOV", 60f);
        Camera mainCam = Camera.main;
        if (mainCam != null)
            mainCam.fieldOfView = Mathf.Clamp(fov, 60f, 90f);

        // Sensitivity (gameplay setting)
        float sensitivity = PlayerPrefs.GetFloat("Sensitivity", 20f);
        InputSensitivity.Current = Mathf.Clamp(sensitivity, 1f, 300f);

    }
}

public static class InputSensitivity
{
    public static float Current { get; set; } = 1f;
}
