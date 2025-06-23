using UnityEngine;
using System;

public class WeaponController : MonoBehaviour
{
    public static WeaponController Instance { get; private set; }

    [Header("Auto‑Reload Settings")]
    [Tooltip("How long after you stop shooting before recharge starts")]
    [SerializeField] private float reloadStartDelay = 1f;

    private WeaponReloadHandler reloadHandler;

    // Shortcut to your currently held weapon
    private WeaponBase currentWeapon => CurrentWeaponHolder.Instance.CurrentWeapon;

    // Firing state
    private float nextFireTime;
    private bool isFiringHeld;
    private float lastFireReleaseTime;

    // Global events
    public static event Action OnShoot;
    public static event Action OnStopShooting;
    public static Action OnReloadStart;
    public static Action OnReloadEnd;

    private void Awake()
    {
        // ✅ Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        reloadHandler = GetComponent<WeaponReloadHandler>();
    }

    private void OnEnable()
    {
        PlayerInput.OnFirePressed   += HandleFirePressed;
        PlayerInput.OnFireReleased  += HandleFireReleased;
        CurrentWeaponHolder.Instance.OnWeaponChanged += _ => OnWeaponSwitched();
    }

    private void OnDisable()
    {
        PlayerInput.OnFirePressed   -= HandleFirePressed;
        PlayerInput.OnFireReleased  -= HandleFireReleased;
        CurrentWeaponHolder.Instance.OnWeaponChanged -= _ => OnWeaponSwitched();
    }

    private void Start()
    {
        // Initialize your default weapon on spawn
        OnWeaponSwitched();
    }

    private void Update()
    {
        if (currentWeapon == null)
            return;

        // Automatic fire
        if (!currentWeapon.HandlesInput
            && isFiringHeld
            && currentWeapon.isAutomatic
            && Time.time >= nextFireTime
            && currentWeapon.CanShoot())
        {
            FireWeapon();
        }

        // Auto‑reload when truly idle
        if (!isFiringHeld
            && !reloadHandler.IsReloading
            && Time.time >= lastFireReleaseTime + reloadStartDelay)
        {
            reloadHandler.TryAutoReload();
        }
    }

    private void HandleFirePressed()
    {
        if (currentWeapon == null || !currentWeapon.CanShoot())
            return;

        isFiringHeld = true;
        reloadHandler.CancelReloads();

        if (currentWeapon.HandlesInput)
            return;

        if (!currentWeapon.isAutomatic && Time.time >= nextFireTime)
        {
            FireWeapon();
        }
    }

    private void HandleFireReleased()
    {
        isFiringHeld = false;
        lastFireReleaseTime = Time.time;

        if (currentWeapon is ContinuousFireWeapon)
            OnStopShooting?.Invoke();
    }

    private void FireWeapon()
    {
        nextFireTime = Time.time + currentWeapon.fireRate;
        currentWeapon.Shoot();
        OnShoot?.Invoke();
    }

    /// <summary>
    /// Call whenever you equip or switch to a new weapon—
    /// including on Start() to grab your default.
    /// </summary>
    public void OnWeaponSwitched()
    {
        if (reloadHandler != null)
        {
            reloadHandler.CancelReloads();
        }

        if (CurrentWeaponHolder.Instance != null && CurrentWeaponHolder.Instance.CurrentWeapon != null)
        {
            var weapon = CurrentWeaponHolder.Instance.CurrentWeapon;

            if (reloadHandler != null)
            {
                reloadHandler.Initialize(weapon);

                if (weapon.currentAmmo < weapon.maxAmmo)
                    reloadHandler.TryAutoReload();
            }
        }
        else
        {
            Debug.LogWarning("WeaponController: no weapon or weapon holder to switch to.");
        }
    }
    
    public void TriggerShootEffect()
    {
        OnShoot?.Invoke();
    }
}
