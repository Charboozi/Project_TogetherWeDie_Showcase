using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeScreenEffect : MonoBehaviour
{
    public static FadeScreenEffect Instance;

    private Image screenImage;
    private Coroutine currentFadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        screenImage = GetComponent<Image>();

        if (screenImage != null)
            screenImage.color = new Color(0, 0, 0, 0); // Fully transparent at start
    }

    private bool IsImageValid()
    {
        return screenImage != null && screenImage.enabled && screenImage.gameObject.activeInHierarchy;
    }

    public void ShowEffect(Color effectColor, float initialAlpha = 0.5f, float fadeSpeed = 2f)
    {
        if (!IsImageValid()) return;

        if (currentFadeCoroutine != null)
            StopCoroutine(currentFadeCoroutine);

        currentFadeCoroutine = StartCoroutine(FadeOutEffect(effectColor, initialAlpha, fadeSpeed));
    }

    private IEnumerator FadeOutEffect(Color color, float startAlpha, float fadeSpeed)
    {
        float alpha = startAlpha;
        color.a = alpha;
        screenImage.color = color;

        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            color.a = Mathf.Clamp01(alpha);
            if (IsImageValid())
                screenImage.color = color;
            yield return null;
        }

        if (IsImageValid())
            screenImage.color = new Color(0, 0, 0, 0);
        currentFadeCoroutine = null;
    }

    public void ShowDownedEffect()
    {
        if (!IsImageValid()) return;

        if (currentFadeCoroutine != null)
            StopCoroutine(currentFadeCoroutine);

        currentFadeCoroutine = StartCoroutine(FadeToSemiBlackEffect());
    }

    private IEnumerator FadeToSemiBlackEffect()
    {
        float alpha = screenImage.color.a;
        Color targetColor = new Color(0, 0, 0, 0.7f); // 70% black

        while (alpha < targetColor.a)
        {
            alpha += Time.deltaTime * 1f;
            alpha = Mathf.Clamp(alpha, 0, targetColor.a);
            if (IsImageValid())
                screenImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        if (IsImageValid())
            screenImage.color = targetColor;
        currentFadeCoroutine = null;
    }

    public void ShowReviveEffect()
    {
        if (!IsImageValid()) return;

        if (currentFadeCoroutine != null)
            StopCoroutine(currentFadeCoroutine);

        currentFadeCoroutine = StartCoroutine(FadeFromBlackEffect());
    }

    private IEnumerator FadeFromBlackEffect()
    {
        float alpha = screenImage.color.a;
        Color color = screenImage.color;

        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * 1.5f;
            alpha = Mathf.Clamp01(alpha);
            if (IsImageValid())
                screenImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        if (IsImageValid())
            screenImage.color = new Color(0, 0, 0, 0);
        currentFadeCoroutine = null;
    }

    public void ShowDeathEffect(float duration = 4f)
    {
        if (!IsImageValid()) return;

        if (currentFadeCoroutine != null)
            StopCoroutine(currentFadeCoroutine);

        currentFadeCoroutine = StartCoroutine(FadeToDeathRed(duration));
    }
    private IEnumerator FadeToDeathRed(float duration)
    {
        float elapsed = 0f;
        Color startColor = screenImage.color;
        Color targetColor = new Color(0.3f, 0f, 0f, 1f); // Deep red

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            if (IsImageValid())
                screenImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        if (IsImageValid())
            screenImage.color = targetColor;
        currentFadeCoroutine = null;
    }

    public void ShowVictoryEffect(float duration = 4f)
    {
        if (!IsImageValid()) return;

        if (currentFadeCoroutine != null)
            StopCoroutine(currentFadeCoroutine);

        currentFadeCoroutine = StartCoroutine(FadeToVictoryWhite(duration));
    }
    private IEnumerator FadeToVictoryWhite(float duration)
    {
        float elapsed = 0f;
        Color startColor = screenImage.color;
        Color targetColor = new Color(1f, 1f, 1f, 1f); // Pure white

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            if (IsImageValid())
                screenImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        if (IsImageValid())
            screenImage.color = targetColor;

        currentFadeCoroutine = null;
    }

}
