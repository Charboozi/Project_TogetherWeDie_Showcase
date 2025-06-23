using UnityEngine;
using System;

[RequireComponent(typeof(PickupWeapon))]
[RequireComponent(typeof(WeaponController))]
public class PlayerInput : MonoBehaviour
{
    public static PlayerInput Instance { get; private set; }

    public static event Action<Vector2> OnMouseLook;
    public static event Action<Vector2> OnMoveInput;
    public static event Action OnJumpPressed;
    public static event Action OnFirePressed;
    public static event Action OnFireReleased;
    public static event Action<int> OnSwitchWeapon;
    public static event Action<int> OnCycleWeapon;
    public static event Action OnInteractPressed;
    public static event Action OnPausePressed;
    public static event Action OnFlashlightToggle;
    public static event Action<bool> OnAim;

    public static bool IsAiming { get; private set; }
    public static event Action<bool> OnScopedChanged;

    public static bool CanInteract = true;

    private PlayerControls controls;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        controls = new PlayerControls();

        // Bind actions
        controls.Player.Look.performed += ctx =>
        {
            if (PauseManager.IsPaused) return;

            Vector2 look = ctx.ReadValue<Vector2>();
            look *= 0.1f;
            OnMouseLook?.Invoke(look);
        };

        controls.Player.Move.performed += ctx =>
        {
            if (PauseManager.IsPaused) return;
            OnMoveInput?.Invoke(ctx.ReadValue<Vector2>());
        };
        controls.Player.Move.canceled += ctx =>
        {
            if (PauseManager.IsPaused) return;
            OnMoveInput?.Invoke(Vector2.zero);
        };

        controls.Player.Jump.performed += ctx =>
        {
            if (PauseManager.IsPaused) return;
            OnJumpPressed?.Invoke();
        };

        controls.Player.Fire.started += ctx =>
        {
            if (PauseManager.IsPaused) return;
            OnFirePressed?.Invoke();
        };

        controls.Player.Fire.canceled += ctx =>
        {
            if (PauseManager.IsPaused) return;
            OnFireReleased?.Invoke();
        };

        controls.Player.Aim.started += ctx =>
        {
            if (PauseManager.IsPaused) return;
            IsAiming = true;
            OnAim?.Invoke(true);
            OnScopedChanged?.Invoke(true);
        };

        controls.Player.Aim.canceled += ctx =>
        {
            if (PauseManager.IsPaused) return;
            IsAiming = false;
            OnAim?.Invoke(false);
            OnScopedChanged?.Invoke(false);
        };

        controls.Player.Interact.performed += ctx =>
        {
            if (PauseManager.IsPaused || !CanInteract) return;
            OnInteractPressed?.Invoke();
        };

        controls.Player.Pause.performed += ctx =>
        {
            // Pause input always allowed
            OnPausePressed?.Invoke();
        };

        controls.Player.CycleWeapon.performed += ctx =>
        {
            if (PauseManager.IsPaused) return;
            float scroll = ctx.ReadValue<float>();
            Debug.Log("Scroll: " + scroll);
            if (scroll > 0f)
                OnCycleWeapon?.Invoke(1);
            else if (scroll < 0f)
                OnCycleWeapon?.Invoke(-1);
        };

        controls.Player.SwitchWeapon1.performed += ctx =>
        {
            if (PauseManager.IsPaused) return;
            OnSwitchWeapon?.Invoke(0);
        };
        controls.Player.SwitchWeapon2.performed += ctx =>
        {
            if (PauseManager.IsPaused) return;
            OnSwitchWeapon?.Invoke(1);
        };
        controls.Player.SwitchWeapon3.performed += ctx =>
        {
            if (PauseManager.IsPaused) return;
            OnSwitchWeapon?.Invoke(2);
        };
        controls.Player.SwitchWeapon4.performed += ctx =>
        {
            if (PauseManager.IsPaused) return;
            OnSwitchWeapon?.Invoke(3);
        };
        
        controls.Player.Flashlight.performed += ctx =>
        {
            if (PauseManager.IsPaused) return;
            OnFlashlightToggle?.Invoke();
        };
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    public Vector2 MoveInput { get; private set; }
    private bool jumpQueued = false;

    public void QueueJump() => jumpQueued = true;

    public bool ConsumeJumpQueued()
    {
        bool temp = jumpQueued;
        jumpQueued = false;
        return temp;
    }
}
