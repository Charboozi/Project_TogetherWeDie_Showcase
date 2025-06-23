using UnityEngine;
using Unity.Netcode;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlackoutManager : NetworkBehaviour
{
    public static BlackoutManager Instance { get; private set; }

    [Header("Post-processing Volume")]
    [SerializeField] private Volume postProcessingVolume;

    private ShadowsMidtonesHighlights shSettings;

    [Header("Settings")]
    [SerializeField] private Vector4 blackoutShadows = new Vector4(0.1f, 0.1f, 0.1f, 0f);
    [SerializeField] private Vector4 blackoutMidtones = new Vector4(0.2f, 0.2f, 0.2f, 0f);
    [SerializeField] private Vector4 blackoutHighlights = new Vector4(0.3f, 0.3f, 0.3f, 0f);

    [SerializeField] private Vector4 normalShadows = new Vector4(1f, 1f, 1f, 0f);
    [SerializeField] private Vector4 normalMidtones = new Vector4(1f, 1f, 1f, 0f);
    [SerializeField] private Vector4 normalHighlights = new Vector4(1f, 1f, 1f, 0f);

    [Header("Audio")]
    [SerializeField] private AudioClip blackoutClip;
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (postProcessingVolume == null)
        {
            Debug.LogError("🚨 Post-processing volume is not assigned!");
        }

        audioSource = GetComponent<AudioSource>();
    }

    public void RequestBlackout()
    {
        if (IsServer)
            ApplyBlackoutServerSide();
        else
            RequestBlackoutServerRpc();
    }

    public void RequestLightsOn()
    {
        if (IsServer)
            ApplyLightsOnServerSide();
        else
            RequestLightsOnServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestBlackoutServerRpc() => ApplyBlackoutServerSide();

    [ServerRpc(RequireOwnership = false)]
    private void RequestLightsOnServerRpc() => ApplyLightsOnServerSide();

    private void ApplyBlackoutServerSide()
    {
        ApplyBlackoutClientRpc();
        InteractableChargeManager.Instance?.FullyDischargeAll();
        GameFeedManager.Instance?.PostFeedMessage("Power outage: turn on the power in the energy room! ('F' for flashlight)");
    }

    private void ApplyLightsOnServerSide()
    {
        ApplyLightsOnClientRpc();
    }

    [ClientRpc]
    private void ApplyBlackoutClientRpc() => ApplyBlackoutLocally();

    [ClientRpc]
    private void ApplyLightsOnClientRpc() => ApplyLightsOnLocally();

    private void ApplyBlackoutLocally()
    {
        if (postProcessingVolume.profile.TryGet(out shSettings))
        {
            shSettings.active = true;
            shSettings.shadows.value = blackoutShadows;
            shSettings.midtones.value = blackoutMidtones;
            shSettings.highlights.value = blackoutHighlights;
        }

        if (blackoutClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(blackoutClip);
        }
    }

    private void ApplyLightsOnLocally()
    {
        if (postProcessingVolume.profile.TryGet(out shSettings))
        {
            shSettings.shadows.value = normalShadows;
            shSettings.midtones.value = normalMidtones;
            shSettings.highlights.value = normalHighlights;
        }
    }
}
