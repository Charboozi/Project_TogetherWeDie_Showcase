using UnityEngine;

public class CameraShakeController : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeDampingSpeed = 1.5f;
    [SerializeField] private float shakeFrequency = 20f; // how often the shake target changes

    private Vector3 originalPosition;
    private Vector3 targetOffset;
    private float shakeTimer = 0f;
    private float shakeDuration = 0f;
    private float shakeAmount = 0f;

    private void Awake()
    {
        originalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (shakeDuration > 0f)
        {
            shakeTimer += Time.deltaTime * shakeFrequency;

            // When timer passes 1, pick a new random target
            if (shakeTimer >= 1f)
            {
                shakeTimer = 0f;
                targetOffset = new Vector3(
                    Random.Range(-1f, 1f) * shakeAmount,
                    Random.Range(-1f, 1f) * shakeAmount,
                    0f
                );
            }

            // Smoothly move toward the current target
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition + targetOffset, Time.deltaTime * shakeFrequency);

            shakeDuration -= Time.deltaTime * shakeDampingSpeed;
        }
        else
        {
            shakeDuration = 0f;
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * shakeFrequency);
        }
    }

    public void Shake(float amount, float duration)
    {
        shakeAmount = amount;
        shakeDuration = duration;
        shakeTimer = 1f; // force picking a random offset immediately
    }
}
