using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

/// <summary>
/// Temporarily changes the vignette color and restores it after a duration.
/// </summary>
public class PersistentScreenTint : MonoBehaviour
{
    public static PersistentScreenTint Instance;

    [SerializeField] private Volume globalVolume;
    private Vignette vignette;
    private Coroutine tintRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (globalVolume == null)
        {
            Debug.LogError("PersistentScreenTint: Global Volume not assigned.");
            return;
        }

        if (!globalVolume.profile.TryGet(out vignette))
        {
            Debug.LogError("PersistentScreenTint: Vignette not found in Global Volume profile.");
        }
    }

    /// <summary>
    /// Temporarily sets the vignette color and intensity, then restores the previous state.
    /// </summary>
    public void SetPersistentTintForDuration(Color color, float duration)
    {
        if (tintRoutine != null)
            StopCoroutine(tintRoutine);

        tintRoutine = StartCoroutine(TintRoutine(color, duration));
    }

    private IEnumerator TintRoutine(Color newColor, float duration)
    {
        if (vignette == null) yield break;

        vignette.active = true;

        // Apply new tint
        vignette.color.overrideState = true;
        vignette.intensity.overrideState = true;

        vignette.color.value = newColor;
        vignette.intensity.value = Mathf.Clamp01(0.4f);

        // Wait for duration
        yield return new WaitForSeconds(duration);

        // Revert to black
        vignette.color.value = Color.black;
        vignette.intensity.value = Mathf.Clamp01(0.4f); // or set to a different default if you want lower

        tintRoutine = null;
    }
}
