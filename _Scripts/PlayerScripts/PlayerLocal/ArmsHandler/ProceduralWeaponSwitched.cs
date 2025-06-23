using UnityEngine;
using System.Collections;

public class ProceduralWeaponSwitched : MonoBehaviour, IArmsOffsetProvider
{
    [Header("Raise Settings")]
    public Vector3 loweredOffset = new Vector3(0f, -0.5f, 0f);
    public float raiseDuration = 0.3f;
    public AnimationCurve raiseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private float t = 1f; // Interpolation value (1 = fully raised)
    private Coroutine raiseRoutine;

    private void OnEnable()
    {
        if (WeaponSwitcher.Instance != null)
        {
            WeaponSwitcher.Instance.OnWeaponSwitched += PlayRaiseAnimation;
        }
    }

    private void OnDisable()
    {
        if (WeaponSwitcher.Instance != null)
        {
            WeaponSwitcher.Instance.OnWeaponSwitched -= PlayRaiseAnimation;
        }
    }

    public void PlayRaiseAnimation()
    {
        if (raiseRoutine != null)
            StopCoroutine(raiseRoutine);

        raiseRoutine = StartCoroutine(RaiseUpRoutine());
    }

    private IEnumerator RaiseUpRoutine()
    {
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / raiseDuration;
            yield return null;
        }
        t = 1f;
    }

    // 👇 This is what integrates into the procedural animation system
    public Vector3 GetOffset()
    {
        float curveValue = raiseCurve.Evaluate(t);
        return Vector3.Lerp(loweredOffset, Vector3.zero, curveValue);
    }
    public Quaternion GetRotation() => Quaternion.identity;
}
