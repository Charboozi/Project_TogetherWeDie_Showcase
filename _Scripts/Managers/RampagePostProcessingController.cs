using UnityEngine;
using Unity.Netcode;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class RampagePostProcessController : NetworkBehaviour
{
    public static RampagePostProcessController Instance { get; private set; }

    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private float fadeDuration = 2f;

    private SplitToning splitToning;
    private Coroutine fadeRoutine;

    // Colors
    private Color defaultShadowColor = new Color32(0x80, 0x80, 0x80, 0xFF); // #808080
    private Color rampageShadowColor = new Color32(0xFF, 0x94, 0x94, 0xFF); // #FF9494

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (postProcessVolume != null && postProcessVolume.profile.TryGet(out splitToning))
        {
            splitToning.active = true;
            splitToning.shadows.value = defaultShadowColor;
        }
        else
        {
            Debug.LogError("❌ SplitToning not found in PostProcess Volume profile!");
        }
    }

    public void StartRampageEffect()
    {
        if (IsServer)
            SetRampageEffectClientRpc(true);
    }

    public void StopRampageEffect()
    {
        if (IsServer)
            SetRampageEffectClientRpc(false);
    }

    [ClientRpc]
    private void SetRampageEffectClientRpc(bool enable)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeShadowColor(
            splitToning.shadows.value,
            enable ? rampageShadowColor : defaultShadowColor
        ));
    }

    private IEnumerator FadeShadowColor(Color start, Color end)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            splitToning.shadows.value = Color.Lerp(start, end, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        splitToning.shadows.value = end;
        fadeRoutine = null;
    }
}
