using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ProjectileWeapon))]
public class SniperScope : MonoBehaviour
{
    [Header("Scope Settings")]
    [SerializeField] private float scopedFOV = 25f;
    [SerializeField] private float zoomSpeed = 8f;
    [SerializeField] private float scopedSpread = 0f;
    [SerializeField] private CanvasGroup scopeUI;

    private Camera playerCamera;
    private float trueDefaultFOV; // ✅ constant baseline FOV
    private float targetFOV;

    private ProjectileWeapon weapon;
    private float originalSpread;

    private void Awake()
    {
        weapon = GetComponent<ProjectileWeapon>();
        playerCamera = Camera.main;
        if (playerCamera != null)
        {
            trueDefaultFOV = playerCamera.fieldOfView;
            targetFOV = trueDefaultFOV;
        }

        originalSpread = weapon.GetBaseSpreadAngle();
    }

    private void OnEnable()
    {
        PlayerInput.OnAim += HandleAim;
    }

    private void OnDisable()
    {
        PlayerInput.OnAim -= HandleAim;
        ResetScopeCompletely();
    }

    private void HandleAim(bool isAiming)
    {
        if (!enabled || !gameObject.activeInHierarchy || playerCamera == null)
            return;

        if (isAiming)
            EnterScope();
        else
            ExitScope();
    }

    private void EnterScope()
    {
        targetFOV = scopedFOV;
        weapon.SetBaseSpreadAngle(scopedSpread);
        if (scopeUI != null) scopeUI.alpha = 1f;
    }

    private void ExitScope()
    {
        targetFOV = trueDefaultFOV;
        weapon.SetBaseSpreadAngle(originalSpread);
        if (scopeUI != null) scopeUI.alpha = 0f;
    }

    private void Update()
    {
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
        }
    }

    private void ResetScopeCompletely()
    {
        targetFOV = trueDefaultFOV;

        if (playerCamera != null)
            playerCamera.fieldOfView = trueDefaultFOV;

        if (weapon != null)
            weapon.SetBaseSpreadAngle(originalSpread);

        if (scopeUI != null)
            scopeUI.alpha = 0f;
    }
}
