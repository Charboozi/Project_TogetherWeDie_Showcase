using UnityEngine;

public class SteamActivationCondition : MonoBehaviour
{
    public enum SteamBehavior
    {
        EnableIfSteamInitialized,
        DisableIfSteamInitialized
    }

    [Tooltip("Enable this GameObject if Steam is initialized, or disable it if Steam is initialized.")]
    public SteamBehavior behavior = SteamBehavior.EnableIfSteamInitialized;

    private void OnEnable()
    {
        // Delay 1 frame to ensure SteamManager had time to initialize
        Invoke(nameof(ApplySteamCondition), 0.01f);
    }

    private void ApplySteamCondition()
    {
        bool isSteamReady = SteamManager.Instance != null && SteamManager.Instance.IsSteamInitialized;

        bool shouldBeActive = (behavior == SteamBehavior.EnableIfSteamInitialized && isSteamReady)
                           || (behavior == SteamBehavior.DisableIfSteamInitialized && !isSteamReady);

        gameObject.SetActive(shouldBeActive);
    }
}
