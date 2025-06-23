using UnityEngine;

public class ScopedArmsOffset : MonoBehaviour, IArmsOffsetProvider
{
    [Header("Scoped Offset Settings")]
    [SerializeField] private Vector3 scopedOffset = new Vector3(-0.05f, -0.03f, 0.15f);
    [SerializeField] private float blendDuration = 0.2f;
    [SerializeField] private AnimationCurve blendCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private float blendT = 0f;
    private bool isAiming = false;

    private void OnEnable()
    {
        PlayerInput.OnAim += HandleAim;
    }

    private void OnDisable()
    {
        PlayerInput.OnAim -= HandleAim;
    }

    private void HandleAim(bool aiming)
    {
        isAiming = aiming;
        StopAllCoroutines();
        StartCoroutine(BlendRoutine(aiming));
    }

    private System.Collections.IEnumerator BlendRoutine(bool aiming)
    {
        float start = blendT;
        float target = aiming ? 1f : 0f;
        float elapsed = 0f;

        while (elapsed < blendDuration)
        {
            elapsed += Time.deltaTime;
            blendT = Mathf.Lerp(start, target, blendCurve.Evaluate(elapsed / blendDuration));
            yield return null;
        }

        blendT = target;
    }

    public Vector3 GetOffset()
    {
        return Vector3.Lerp(Vector3.zero, scopedOffset, blendCurve.Evaluate(blendT));
    }

    public Quaternion GetRotation()
    {
        return Quaternion.identity;
    }
}
