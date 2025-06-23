using UnityEngine;
using System;

public class CurrentWeaponHolder : MonoBehaviour
{
    public static CurrentWeaponHolder Instance { get; private set; }

    private WeaponBase _currentWeapon;
    public WeaponBase CurrentWeapon
    {
        get => _currentWeapon;
        private set
        {
            if (_currentWeapon == value) return;
            _currentWeapon = value;
            OnWeaponChanged?.Invoke(_currentWeapon);
        }
    }

    /// <summary>Fired whenever CurrentWeapon is set to a new value.</summary>
    public event Action<WeaponBase> OnWeaponChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetWeapon(WeaponBase weapon)
    {
        CurrentWeapon = weapon;
    }
}
