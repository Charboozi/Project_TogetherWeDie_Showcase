using UnityEngine;

public class CameraRecoilController : MonoBehaviour
{
    [Header("Recoil Settings")]
    [SerializeField] private float recoilRecoverySpeed = 10f;

    [Tooltip("Multiplier for vertical recoil.")]
    [SerializeField] private float verticalMultiplier = 2f;

    [Tooltip("Multiplier for horizontal recoil.")]
    [SerializeField] private float horizontalMultiplier = 1f;

    [Tooltip("Maximum horizontal angle randomly applied per shot.")]
    [SerializeField] private float maxHorizontalAngle = 2f;

    [Tooltip("Maximum vertical angle per shot (before multipliers).")]
    [SerializeField] private float baseVerticalRecoil = 2f;

    [Tooltip("Use random horizontal direction?")]
    [SerializeField] private bool randomizeHorizontal = true;

    private Vector2 currentRecoil = Vector2.zero;
    private Vector2 targetRecoil = Vector2.zero;

    private void LateUpdate()
    {
        currentRecoil = Vector2.Lerp(currentRecoil, targetRecoil, Time.deltaTime * recoilRecoverySpeed);
        targetRecoil = Vector2.Lerp(targetRecoil, Vector2.zero, Time.deltaTime * recoilRecoverySpeed);
    }

    /// <summary>
    /// Call this every time the weapon fires to apply recoil.
    /// </summary>
    public void ApplyRecoil(float verticalStrength = 1f, float horizontalStrength = 1f)
    {
        float vertical = baseVerticalRecoil * verticalStrength * verticalMultiplier;
        float horizontal = maxHorizontalAngle * horizontalStrength * horizontalMultiplier;

        // Optional: stronger recoil = slower recovery
        recoilRecoverySpeed = Mathf.Lerp(10f, 4f, Mathf.Clamp01(verticalStrength));

        if (randomizeHorizontal)
        {
            horizontal = Random.Range(-horizontal, horizontal);
        }

        targetRecoil += new Vector2(horizontal, vertical);
    }


    public Vector2 GetCurrentRecoil()
    {
        return currentRecoil;
    }
}
