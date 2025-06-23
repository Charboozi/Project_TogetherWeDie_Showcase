using UnityEngine;
using System.Collections;

public class ArmsReloadAnimation : MonoBehaviour, IArmsOffsetProvider
{
    [Header("Timing Multipliers")]
    public float reloadDurationMultiplier = 1f;

    [Header("Phase Settings")]
    public float lowerAmount = 0.2f;
    public Vector3 tiltAngles = new Vector3(0f, 30f, 10f);
    public float lowerTime = 0.15f;
    public float tiltTime = 0.1f;

    [Header("Shake Settings")]
    public float shakeAmount = 0.05f;
    public float shakeSpeed = 20f;
    public float shakeDuration = 0.3f;

    [Header("Pause & End Shake")]
    public float pauseDuration = 0.25f;
    public float endShakeAmount = 0.02f;
    public float endShakeSpeed = 30f;
    public float endShakeDuration = 0.15f;

    [Header("Return")]
    public float returnTime = 0.2f;

    private Coroutine reloadCoroutine;

    private Vector3 baseOffset = Vector3.zero;
    private Quaternion baseRotation = Quaternion.identity;
    private Vector3 shakeOffset = Vector3.zero;
    private Quaternion shakeRotation = Quaternion.identity;

    void OnEnable() => WeaponController.OnReloadStart += StartReload;
    void OnDisable() => WeaponController.OnReloadStart -= StartReload;

    public Vector3 GetOffset() => baseOffset + shakeOffset;
    public Quaternion GetRotation() => baseRotation * shakeRotation;

    void LateUpdate()
    {
        transform.localRotation = GetRotation();
    }

    private void StartReload()
    {
        if (reloadCoroutine != null)
            StopCoroutine(reloadCoroutine);
        reloadCoroutine = StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        WeaponBase weapon = CurrentWeaponHolder.Instance?.CurrentWeapon;
        if (weapon == null) yield break;

        // PHASE 1: Lower
        yield return LerpPosition(Vector3.zero, new Vector3(0, -lowerAmount, 0), lowerTime);

        // PHASE 2: Tilt
        yield return LerpRotation(Quaternion.identity, Quaternion.Euler(tiltAngles), tiltTime);

        // Pause briefly to show tilt before shake
        yield return new WaitForSeconds(0.2f);

        // PHASE 3: Shake
        yield return Shake(shakeDuration, shakeAmount, shakeSpeed);

        // PHASE 4: Pause
        shakeOffset = Vector3.zero;
        shakeRotation = Quaternion.identity;
        yield return new WaitForSeconds(pauseDuration);

        // PHASE 5: End Shake
        yield return Shake(endShakeDuration, endShakeAmount, endShakeSpeed);

        // PHASE 6: Return to neutral
        yield return LerpPosition(baseOffset, Vector3.zero, returnTime);
        yield return LerpRotation(baseRotation, Quaternion.identity, returnTime);

        baseOffset = Vector3.zero;
        baseRotation = Quaternion.identity;
        shakeOffset = Vector3.zero;
        shakeRotation = Quaternion.identity;
    }

    private IEnumerator LerpPosition(Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            baseOffset = Vector3.Lerp(from, to, t / duration);
            yield return null;
        }
        baseOffset = to;
    }

    private IEnumerator LerpRotation(Quaternion from, Quaternion to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            baseRotation = Quaternion.Slerp(from, to, Mathf.Clamp01(t / duration));
            yield return null;
        }
        baseRotation = to;
    }

    private IEnumerator Shake(float duration, float amount, float speed)
    {
        float end = Time.time + duration;
        while (Time.time < end)
        {
            float x = (Mathf.PerlinNoise(Time.time * speed, 0f) - 0.5f) * 2f;
            float y = (Mathf.PerlinNoise(0f, Time.time * speed) - 0.5f) * 2f;
            float z = (Mathf.PerlinNoise(Time.time * speed, Time.time * speed) - 0.5f) * 2f;

            shakeOffset = new Vector3(x, y, z) * amount;
            shakeRotation = Quaternion.Euler(x * amount * 100f, y * amount * 100f, z * amount * 100f);

            yield return null;
        }

        shakeOffset = Vector3.zero;
        shakeRotation = Quaternion.identity;
    }
}

